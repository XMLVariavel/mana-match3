using System.Collections.Generic;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Gerencia a grade paralela de obstáculos. É consultado pelo BoardManager
    /// (para travar seleção de peças sob Corrente e saber quais células estão
    /// bloqueadas por Pedra) e pelo BoardPhysics (para resolver hits antes de
    /// destruir peças e para a gravidade pular células bloqueadas).
    /// </summary>
    public class ObstacleManager : MonoBehaviour
    {
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private Transform obstaclesParent;
        [SerializeField] private Sprite pedraSprite;
        [SerializeField] private Sprite correnteSprite;
        [SerializeField] private Sprite geloSprite;
        [SerializeField] private Sprite caixaSprite;

        private Obstacle[,] obstacles;
        private static readonly (int dx, int dy)[] Direcoes = { (1, 0), (-1, 0), (0, 1), (0, -1) };

        public void Initialize(int width, int height)
        {
            obstacles = new Obstacle[width, height];
        }

        /// <summary>Remove obstáculos antigos antes de iniciar outra fase ou modo.</summary>
        public void ClearAll()
        {
            if (obstacles != null)
            {
                for (int x = 0; x < obstacles.GetLength(0); x++)
                {
                    for (int y = 0; y < obstacles.GetLength(1); y++)
                        obstacles[x, y] = null;
                }
            }

            if (obstaclesParent == null) return;
            for (int i = obstaclesParent.childCount - 1; i >= 0; i--)
                Destroy(obstaclesParent.GetChild(i).gameObject);
        }

        public int QuantidadeAtiva
        {
            get
            {
                if (obstacles == null) return 0;
                int total = 0;
                for (int x = 0; x < obstacles.GetLength(0); x++)
                    for (int y = 0; y < obstacles.GetLength(1); y++)
                        if (obstacles[x, y] != null) total++;
                return total;
            }
        }

        public Obstacle GetObstacle(int x, int y) => obstacles[x, y];

        public bool IsBlocked(int x, int y) =>
            obstacles[x, y] != null && obstacles[x, y].Type == ObstacleType.PedraDeserto;

        public bool IsLocked(int x, int y) =>
            obstacles[x, y] != null && obstacles[x, y].Type == ObstacleType.Corrente;

        /// <summary>
        /// Posiciona um obstáculo numa célula (usado por um carregador de nível/fase).
        /// </summary>
        public void PlaceObstacle(ObstacleType type, int x, int y, Vector3 worldPosition)
        {
            int hits = type switch
            {
                ObstacleType.PedraDeserto => 2,
                ObstacleType.Corrente => 1,
                ObstacleType.Gelo => 1,
                ObstacleType.CaixaSelada => 1,
                _ => 0
            };
            if (hits == 0) return;

            GameObject obj = Instantiate(obstaclePrefab, worldPosition, Quaternion.identity, obstaclesParent);
            Obstacle obstacle = obj.GetComponent<Obstacle>();
            obstacle.Setup(type, x, y, hits, SpriteFor(type));
            obstacles[x, y] = obstacle;
        }

        private Sprite SpriteFor(ObstacleType type) => type switch
        {
            ObstacleType.PedraDeserto => pedraSprite,
            ObstacleType.Corrente => correnteSprite,
            ObstacleType.Gelo => geloSprite,
            ObstacleType.CaixaSelada => caixaSprite,
            _ => null
        };

        /// <summary>
        /// Resolve os obstáculos a partir de um MatchResult recém-calculado, ANTES
        /// da destruição de fato acontecer:
        /// - Gelo: absorve o hit da própria célula e poupa a peça nesta passada.
        /// - Corrente / Caixa Selada / Pedra: recebem hit de qualquer match adjacente.
        /// Obstáculos que chegam a 0 hits são quebrados/liberados.
        /// </summary>
        public void ResolveObstacles(MatchResult result, int width, int height)
        {
            if (obstacles == null) return;

            // 1) Gelo: hit na própria célula, peça sobrevive a esta passada.
            var poupadas = new List<Tile>();
            foreach (Tile tile in result.TilesToDestroy)
            {
                Obstacle obs = obstacles[tile.X, tile.Y];
                if (obs == null || obs.Type != ObstacleType.Gelo) continue;

                obs.RegisterHit();
                if (obs.IsBroken)
                {
                    obs.PlayBreakEffect();
                    obstacles[tile.X, tile.Y] = null;
                }
                poupadas.Add(tile);
            }
            foreach (Tile t in poupadas) result.TilesToDestroy.Remove(t);

            // 2) Corrente / Caixa Selada / Pedra: hit vindo de match adjacente.
            var celulasJaChecadas = new HashSet<(int, int)>();
            foreach (Tile tile in result.TilesToDestroy)
            {
                foreach (var (dx, dy) in Direcoes)
                {
                    int nx = tile.X + dx;
                    int ny = tile.Y + dy;
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    if (!celulasJaChecadas.Add((nx, ny))) continue;

                    Obstacle obs = obstacles[nx, ny];
                    if (obs == null || obs.Type == ObstacleType.Gelo) continue;

                    obs.RegisterHit();
                    if (obs.IsBroken)
                    {
                        obs.PlayBreakEffect();
                        obstacles[nx, ny] = null;
                    }
                }
            }
        }
    }
}
