using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Responsável pela "física" do tabuleiro: destruir peças combinadas,
    /// aplicar gravidade (queda), reabastecer o topo com novas peças e repetir
    /// a checagem de combinações enquanto houver combos em cascata.
    /// </summary>
    public class BoardPhysics : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private GameManager gameManager;

        [Header("Obstáculos (opcional — deixe vazio se a fase não usa bloqueadores)")]
        [SerializeField] private ObstacleManager obstacleManager;

        // Pool simples de peças — evita Instantiate/Destroy repetidos, que têm
        // custo alto de GC/alocação em dispositivos Android.
        private readonly Queue<Tile> pool = new Queue<Tile>();

        // ---------------------------------------------------------------
        // Eventos de feedback (áudio/haptics). São apenas notificações: se
        // ninguém se inscrever, nada muda no comportamento do tabuleiro —
        // por isso os testes existentes continuam válidos sem alteração.
        // ---------------------------------------------------------------

        /// <summary>Peças destruídas numa passada. Parâmetro = quantidade.</summary>
        public event Action<int> OnMatchDestruido;

        /// <summary>Uma peça especial nasceu no tabuleiro.</summary>
        public event Action<SpecialType> OnEspecialCriado;

        /// <summary>
        /// A resolução encadeou outra rodada de matches (combo em cascata).
        /// Parâmetro = número da passada (2 = primeiro encadeamento).
        /// </summary>
        public event Action<int> OnCascataAvancou;

        /// <summary>
        /// Processa um resultado de match (obstáculos + destruição + especiais +
        /// queda) e continua repetindo enquanto novas combinações surgirem por
        /// causa da queda das peças (efeito cascata).
        /// </summary>
        public IEnumerator ResolveBoard(MatchDetector detector, ScoreAndObjectiveManager score, MatchResult result)
        {
            int passada = 0;

            while (result != null && result.TilesToDestroy.Count > 0)
            {
                passada++;
                if (passada >= 2) OnCascataAvancou?.Invoke(passada);

                // Gelo pode poupar peças (remove do result) e Corrente/Caixa/Pedra
                // podem quebrar por causa deste match — precisa rodar antes de destruir.
                obstacleManager?.ResolveObstacles(result, boardManager.Width, boardManager.Height);

                yield return StartCoroutine(DestroyMatchedTiles(result, score));
                yield return StartCoroutine(SpawnSpecials(result));
                yield return StartCoroutine(CollapseAndRefill());

                result = detector.FindMatches(boardManager.Grid, boardManager.Width, boardManager.Height);
            }
        }

        private IEnumerator DestroyMatchedTiles(MatchResult result, ScoreAndObjectiveManager score)
        {
            int total = result.TilesToDestroy.Count;
            if (total == 0) yield break;

            OnMatchDestruido?.Invoke(total);

            int completed = 0;
            bool competitivo = gameManager != null && gameManager.ModoAtual == GameMode.ContraRelogio;
            float multiplicador = competitivo
                ? CompetitiveScoreRules.Multiplicador(gameManager.ComboAtual, gameManager.SegundosDesdeJogadaValida)
                : 1f;

            foreach (Tile tile in result.TilesToDestroy)
            {
                boardManager.Grid[tile.X, tile.Y] = null;
                int pontos = competitivo
                    ? Mathf.RoundToInt(GetPointsFor(tile) * multiplicador)
                    : GetPointsFor(tile);
                score.AddScore(pontos, tile.Type);

                tile.PlayDestroyEffect(() =>
                {
                    ReturnToPool(tile);
                    completed++;
                });
            }

            while (completed < total)
                yield return null;

            if (competitivo && gameManager.UltimoBonusCompetitivo > 0)
                score.AddScore(gameManager.UltimoBonusCompetitivo, TileType.Pao);
        }

        private IEnumerator SpawnSpecials(MatchResult result)
        {
            foreach (SpecialSpawnInfo info in result.SpecialsToSpawn)
            {
                // A célula pode já ter sido reocupada por outra peça (raro, mas possível
                // em cascatas complexas) — a peça especial sempre tem prioridade.
                Tile existing = boardManager.Grid[info.X, info.Y];
                if (existing != null)
                {
                    existing.gameObject.SetActive(false);
                    ReturnToPool(existing);
                }

                Tile special = GetPooledOrNewTile();
                special.Setup(info.SourceType, info.X, info.Y, boardManager.GetSpriteFor(info.SourceType));
                special.PromoteToSpecial(info.Type, boardManager.GetSpecialSprite(info.Type));
                special.FitToCell(boardManager.CellSize);
                special.transform.position = boardManager.WorldPosition(info.X, info.Y);
                boardManager.Grid[info.X, info.Y] = special;

                OnEspecialCriado?.Invoke(info.Type);
            }

            yield break;
        }

        /// <summary>
        /// Compacta cada coluna (peças caem para preencher espaços vazios) e
        /// preenche o topo com peças novas vindas de fora da grade visível.
        /// Quando há Pedra do Deserto na coluna, ela é tratada como uma parede:
        /// a coluna é processada em segmentos independentes acima/abaixo dela,
        /// já que nenhuma peça pode cair através de uma célula bloqueada.
        /// </summary>
        private IEnumerator CollapseAndRefill()
        {
            Tile[,] grid = boardManager.Grid;
            int width = boardManager.Width;
            int height = boardManager.Height;

            // Usamos int[1] em vez de "ref int" porque um array é um tipo
            // referência e pode ser capturado livremente dentro das lambdas de
            // callback abaixo (parâmetros ref/out não podem ser usados dentro
            // de lambdas/expressões anônimas em C#).
            int[] pending = { 0 };

            for (int x = 0; x < width; x++)
            {
                int segmentStart = 0;

                for (int y = 0; y <= height; y++)
                {
                    bool isBoundary = y == height || (obstacleManager != null && obstacleManager.IsBlocked(x, y));
                    if (!isBoundary) continue;

                    CollapseSegment(grid, x, segmentStart, y, pending);
                    segmentStart = y + 1; // pula a própria célula bloqueada (não recebe peça)
                }
            }

            while (pending[0] > 0)
                yield return null;
        }

        /// <summary>
        /// Compacta e reabastece um trecho vertical [start, end) de uma coluna —
        /// o "segmento" corresponde ao intervalo entre duas Pedras do Deserto
        /// (ou entre uma Pedra e a borda do tabuleiro).
        /// </summary>
        private void CollapseSegment(Tile[,] grid, int x, int start, int end, int[] pending)
        {
            if (end <= start) return; // segmento vazio (duas pedras coladas, por exemplo)

            int writeY = start;

            // 1) Compacta as peças existentes para o fundo do segmento.
            for (int y = start; y < end; y++)
            {
                Tile tile = grid[x, y];
                if (tile == null) continue;

                if (writeY != y)
                {
                    grid[x, writeY] = tile;
                    grid[x, y] = null;

                    pending[0]++;
                    int targetY = writeY;
                    tile.MoveToGridPosition(x, targetY, boardManager.WorldPosition(x, targetY), () => pending[0]--);
                }
                writeY++;
            }

            // 2) Preenche o restante do segmento com peças novas, caindo de cima.
            int segmentHeight = end - start;
            for (int y = writeY; y < end; y++)
            {
                Tile newTile = GetPooledOrNewTile();
                var tiposAtivos = boardManager.ActiveTypes;
                TileType type = tiposAtivos[UnityEngine.Random.Range(0, tiposAtivos.Count)];
                newTile.Setup(type, x, y, boardManager.GetSpriteFor(type));
                newTile.FitToCell(boardManager.CellSize);
                newTile.transform.position = boardManager.WorldPosition(x, start + segmentHeight + (y - writeY));

                grid[x, y] = newTile;

                pending[0]++;
                int targetY = y;
                newTile.MoveToGridPosition(x, targetY, boardManager.WorldPosition(x, targetY), () => pending[0]--);
            }
        }

        private int GetPointsFor(Tile tile) => tile.Special == SpecialType.Nenhum ? 10 : 30;

        // ---------------------------------------------------------------
        // Pool de peças
        // ---------------------------------------------------------------

        private Tile GetPooledOrNewTile()
        {
            while (pool.Count > 0)
            {
                Tile pooled = pool.Dequeue();
                if (pooled != null)
                {
                    pooled.gameObject.SetActive(true);
                    return pooled;
                }
            }

            GameObject obj = Instantiate(boardManager.TilePrefab, boardManager.TilesParent);
            return obj.GetComponent<Tile>();
        }

        private void ReturnToPool(Tile tile)
        {
            tile.gameObject.SetActive(false);
            pool.Enqueue(tile);
        }
    }
}
