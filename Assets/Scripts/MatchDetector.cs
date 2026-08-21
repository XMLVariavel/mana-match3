using System.Collections.Generic;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Informa onde e qual peça especial deve nascer após uma combinação.
    /// </summary>
    public struct SpecialSpawnInfo
    {
        public int X;
        public int Y;
        public SpecialType Type;
        public TileType SourceType;
    }

    /// <summary>
    /// Resultado de uma varredura de combinações: quais peças devem ser destruídas
    /// e quais peças especiais devem nascer em seus lugares.
    /// </summary>
    public class MatchResult
    {
        public readonly HashSet<Tile> TilesToDestroy = new HashSet<Tile>();
        public readonly List<SpecialSpawnInfo> SpecialsToSpawn = new List<SpecialSpawnInfo>();

        // Peças especiais atingidas "de raspão" por outro efeito (ex: Tocha Nv.3)
        // que também devem ter seu próprio efeito ativado — resolvido pelo MatchDetector.
        public readonly HashSet<Tile> SpecialsAtivadasEmCascata = new HashSet<Tile>();
    }

    /// <summary>
    /// Detecta combinações horizontais/verticais de 3+ peças, decide quais viram
    /// peças especiais (linha de 4 → Espada, L/T → Tocha, linha de 5 → Arca,
    /// bloco 2x2 → Estrela Guia) e resolve a ativação/cruzamento de especiais,
    /// delegando a execução de cada efeito ao PowerUpConfig/EfeitoEspecialSO
    /// correspondente (padrão Strategy).
    /// </summary>
    public class MatchDetector : MonoBehaviour
    {
        [Header("Objetivo (usado pela Estrela Guia)")]
        [SerializeField] private ScoreAndObjectiveManager objectiveManager;

        [Header("Poderes de Tabuleiro (Strategy + ScriptableObject)")]
        [Tooltip("Um PowerUpConfig por SpecialType (Espada_Linha, Espada_Coluna, Tocha_Acesa, Arca_Alianca, Estrela_Guia).")]
        [SerializeField] private List<PowerUpConfig> especiaisDeTabuleiro;

        private Dictionary<SpecialType, PowerUpConfig> configPorTipo;

        private void Awake()
        {
            configPorTipo = new Dictionary<SpecialType, PowerUpConfig>();
            if (especiaisDeTabuleiro == null) return;

            foreach (PowerUpConfig config in especiaisDeTabuleiro)
            {
                if (config != null && config.Tipo == TipoPoder.EspecialDeTabuleiro)
                    configPorTipo[config.TipoEspecialAssociado] = config;
            }
        }

        /// <summary>
        /// Varre o tabuleiro inteiro em busca de combinações de 3+ peças iguais.
        /// </summary>
        public MatchResult FindMatches(Tile[,] grid, int width, int height)
        {
            var result = new MatchResult();
            bool[,] matched = new bool[width, height];

            List<List<Vector2Int>> horizontalRuns = ScanRuns(grid, width, height, horizontal: true);
            List<List<Vector2Int>> verticalRuns = ScanRuns(grid, width, height, horizontal: false);

            foreach (var run in horizontalRuns) MarkRun(run, grid, matched, result);
            foreach (var run in verticalRuns) MarkRun(run, grid, matched, result);

            DetectLShapesAndTShapes(horizontalRuns, verticalRuns, grid, result);
            DetectLineSpecials(horizontalRuns, isHorizontal: true, grid, result);
            DetectLineSpecials(verticalRuns, isHorizontal: false, grid, result);
            Detect2x2Blocks(grid, width, height, result);

            return result;
        }

        // ---------------------------------------------------------------
        // Varredura genérica de sequências (reaproveitada para linhas e colunas)
        // ---------------------------------------------------------------

        private List<List<Vector2Int>> ScanRuns(Tile[,] grid, int width, int height, bool horizontal)
        {
            var runs = new List<List<Vector2Int>>();
            int outerCount = horizontal ? height : width;
            int innerCount = horizontal ? width : height;

            for (int outer = 0; outer < outerCount; outer++)
            {
                int runStart = 0;
                for (int inner = 1; inner <= innerCount; inner++)
                {
                    Tile current = inner < innerCount ? GetTile(grid, horizontal, outer, inner) : null;
                    Tile start = GetTile(grid, horizontal, outer, runStart);
                    bool sameAsStart = current != null && start != null && current.Type == start.Type;

                    if (!sameAsStart)
                    {
                        int runLength = inner - runStart;
                        if (runLength >= 3)
                        {
                            var run = new List<Vector2Int>();
                            for (int i = runStart; i < inner; i++)
                                run.Add(horizontal ? new Vector2Int(i, outer) : new Vector2Int(outer, i));
                            runs.Add(run);
                        }
                        runStart = inner;
                    }
                }
            }

            return runs;
        }

        private Tile GetTile(Tile[,] grid, bool horizontal, int outer, int inner) =>
            horizontal ? grid[inner, outer] : grid[outer, inner];

        private void MarkRun(List<Vector2Int> run, Tile[,] grid, bool[,] matched, MatchResult result)
        {
            foreach (var cell in run)
            {
                matched[cell.x, cell.y] = true;
                Tile tile = grid[cell.x, cell.y];
                if (tile != null) result.TilesToDestroy.Add(tile);
            }
        }

        // ---------------------------------------------------------------
        // Peças especiais por formato
        // ---------------------------------------------------------------

        private void DetectLShapesAndTShapes(List<List<Vector2Int>> hRuns, List<List<Vector2Int>> vRuns, Tile[,] grid, MatchResult result)
        {
            foreach (var h in hRuns)
            {
                foreach (var v in vRuns)
                {
                    Vector2Int? intersection = FindIntersection(h, v);
                    if (!intersection.HasValue) continue;

                    Vector2Int p = intersection.Value;
                    Tile origin = grid[p.x, p.y];
                    if (origin == null) continue;

                    result.SpecialsToSpawn.Add(new SpecialSpawnInfo
                    {
                        X = p.x,
                        Y = p.y,
                        Type = SpecialType.Tocha_Acesa,
                        SourceType = origin.Type
                    });
                }
            }
        }

        private Vector2Int? FindIntersection(List<Vector2Int> a, List<Vector2Int> b)
        {
            foreach (var cellA in a)
                foreach (var cellB in b)
                    if (cellA == cellB) return cellA;
            return null;
        }

        private void DetectLineSpecials(List<List<Vector2Int>> runs, bool isHorizontal, Tile[,] grid, MatchResult result)
        {
            foreach (var run in runs)
            {
                if (run.Count < 4) continue; // combinações de exatamente 3 não geram especial

                Vector2Int center = run[run.Count / 2];
                Tile origin = grid[center.x, center.y];
                if (origin == null) continue;

                SpecialType special = run.Count >= 5
                    ? SpecialType.Arca_Alianca
                    : (isHorizontal ? SpecialType.Espada_Linha : SpecialType.Espada_Coluna);

                result.SpecialsToSpawn.Add(new SpecialSpawnInfo
                {
                    X = center.x,
                    Y = center.y,
                    Type = special,
                    SourceType = origin.Type
                });
            }
        }

        private void Detect2x2Blocks(Tile[,] grid, int width, int height, MatchResult result)
        {
            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    Tile a = grid[x, y];
                    Tile b = grid[x + 1, y];
                    Tile c = grid[x, y + 1];
                    Tile d = grid[x + 1, y + 1];

                    if (a == null || b == null || c == null || d == null) continue;
                    if (a.Type != b.Type || a.Type != c.Type || a.Type != d.Type) continue;

                    result.TilesToDestroy.Add(a);
                    result.TilesToDestroy.Add(b);
                    result.TilesToDestroy.Add(c);
                    result.TilesToDestroy.Add(d);

                    result.SpecialsToSpawn.Add(new SpecialSpawnInfo
                    {
                        X = x,
                        Y = y,
                        Type = SpecialType.Estrela_Guia,
                        SourceType = a.Type
                    });
                }
            }
        }

        // ---------------------------------------------------------------
        // Ativação de uma peça especial isolada (trocada com uma peça comum)
        // ---------------------------------------------------------------

        public MatchResult ActivateSpecial(Tile[,] grid, int width, int height, Tile special)
        {
            var result = new MatchResult();
            AplicarEfeitoConfigurado(grid, width, height, special, special.Type, result);
            result.TilesToDestroy.Add(special);
            ProcessarCascata(grid, width, height, result);
            return result;
        }

        /// <summary>
        /// Ponte entre uma peça especial no tabuleiro e o efeito configurado no
        /// seu PowerUpConfig (padrão Strategy) — substitui o antigo switch fixo.
        /// </summary>
        private void AplicarEfeitoConfigurado(Tile[,] grid, int width, int height, Tile special, TileType colorFilter, MatchResult result)
        {
            if (special.Special == SpecialType.Nenhum) return;

            if (!configPorTipo.TryGetValue(special.Special, out PowerUpConfig config) || config.EfeitoDeTabuleiro == null)
            {
                Debug.LogWarning($"Nenhum PowerUpConfig configurado para {special.Special}. Efeito ignorado.");
                return;
            }

            var contexto = new EfeitoContexto
            {
                Grid = grid,
                Width = width,
                Height = height,
                OriginX = special.X,
                OriginY = special.Y,
                CorAlvo = colorFilter,
                Nivel = config.NivelAtual,
                ObjectiveManager = objectiveManager
            };

            config.EfeitoDeTabuleiro.Aplicar(contexto, result);
        }

        /// <summary>
        /// Expande peças especiais marcadas por outro efeito como "ativadas em
        /// cascata" (ex: Tocha Nv.3), aplicando o efeito de cada uma até não
        /// sobrar nenhuma pendente. Protegido contra ciclos (peça já processada
        /// não é processada de novo).
        /// </summary>
        private void ProcessarCascata(Tile[,] grid, int width, int height, MatchResult result)
        {
            var processadas = new HashSet<Tile>();

            while (result.SpecialsAtivadasEmCascata.Count > 0)
            {
                Tile tile = null;
                foreach (Tile t in result.SpecialsAtivadasEmCascata) { tile = t; break; }
                result.SpecialsAtivadasEmCascata.Remove(tile);

                if (!processadas.Add(tile)) continue;

                result.TilesToDestroy.Add(tile);
                AplicarEfeitoConfigurado(grid, width, height, tile, tile.Type, result);
            }
        }

        // ---------------------------------------------------------------
        // Cruzamento de duas peças especiais trocadas entre si
        // ---------------------------------------------------------------

        public MatchResult ResolveSpecialCombo(Tile[,] grid, int width, int height, Tile a, Tile b)
        {
            var result = new MatchResult();

            bool bothArca = a.Special == SpecialType.Arca_Alianca && b.Special == SpecialType.Arca_Alianca;
            bool bothEspada = IsEspada(a.Special) && IsEspada(b.Special);
            bool bothTocha = a.Special == SpecialType.Tocha_Acesa && b.Special == SpecialType.Tocha_Acesa;
            bool arcaComEspada = (a.Special == SpecialType.Arca_Alianca && IsEspada(b.Special)) ||
                                  (b.Special == SpecialType.Arca_Alianca && IsEspada(a.Special));
            bool arcaComTocha = (a.Special == SpecialType.Arca_Alianca && b.Special == SpecialType.Tocha_Acesa) ||
                                 (b.Special == SpecialType.Arca_Alianca && a.Special == SpecialType.Tocha_Acesa);

            if (bothArca)
            {
                // Arca + Arca: limpa o tabuleiro inteiro.
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        if (grid[x, y] != null) result.TilesToDestroy.Add(grid[x, y]);
            }
            else if (arcaComEspada)
            {
                // Arca + Espada: toda peça da cor-alvo "vira" uma espada e é ativada
                // (limpa a linha e a coluna de cada ocorrência daquela cor).
                Tile arca = a.Special == SpecialType.Arca_Alianca ? a : b;
                ActivateColorAsEspadas(grid, width, height, arca == a ? b.Type : a.Type, result);
            }
            else if (arcaComTocha)
            {
                // Arca + Tocha: toda peça da cor-alvo explode em área 3x3.
                Tile arca = a.Special == SpecialType.Arca_Alianca ? a : b;
                ActivateColorAsTochas(grid, width, height, arca == a ? b.Type : a.Type, result);
            }
            else if (bothEspada)
            {
                // Duas espadas: limpa a linha E a coluna do ponto de troca.
                ClearRow(grid, width, a.Y, result);
                ClearColumn(grid, height, a.X, result);
            }
            else if (bothTocha)
            {
                // Duas tochas: explosão ampliada em área 5x5.
                for (int x = a.X - 2; x <= a.X + 2; x++)
                    for (int y = a.Y - 2; y <= a.Y + 2; y++)
                    {
                        if (x < 0 || x >= width || y < 0 || y >= height) continue;
                        Tile t = grid[x, y];
                        if (t != null) result.TilesToDestroy.Add(t);
                    }
            }
            else
            {
                // Combinação sem regra dedicada (ex: Estrela Guia + outra especial):
                // ativa cada peça isoladamente através do PowerUpConfig configurado.
                AplicarEfeitoConfigurado(grid, width, height, a, a.Type, result);
                AplicarEfeitoConfigurado(grid, width, height, b, b.Type, result);
            }

            result.TilesToDestroy.Add(a);
            result.TilesToDestroy.Add(b);

            ProcessarCascata(grid, width, height, result);
            return result;
        }

        private void ActivateColorAsEspadas(Tile[,] grid, int width, int height, TileType color, MatchResult result)
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    Tile t = grid[x, y];
                    if (t == null || t.Type != color) continue;

                    ClearRow(grid, width, y, result);
                    ClearColumn(grid, height, x, result);
                }
        }

        private void ActivateColorAsTochas(Tile[,] grid, int width, int height, TileType color, MatchResult result)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile t = grid[x, y];
                    if (t == null || t.Type != color) continue;

                    for (int ex = x - 1; ex <= x + 1; ex++)
                        for (int ey = y - 1; ey <= y + 1; ey++)
                        {
                            if (ex < 0 || ex >= width || ey < 0 || ey >= height) continue;
                            Tile e = grid[ex, ey];
                            if (e != null) result.TilesToDestroy.Add(e);
                        }
                }
            }
        }

        private void ClearRow(Tile[,] grid, int width, int y, MatchResult result)
        {
            for (int x = 0; x < width; x++)
            {
                Tile t = grid[x, y];
                if (t != null) result.TilesToDestroy.Add(t);
            }
        }

        private void ClearColumn(Tile[,] grid, int height, int x, MatchResult result)
        {
            for (int y = 0; y < height; y++)
            {
                Tile t = grid[x, y];
                if (t != null) result.TilesToDestroy.Add(t);
            }
        }

        private bool IsEspada(SpecialType type) =>
            type == SpecialType.Espada_Linha || type == SpecialType.Espada_Coluna;
    }
}
