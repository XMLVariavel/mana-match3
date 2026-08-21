using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BibleMatch3.Tests
{
    public class ObstacleGravityFunctionalTests
    {
        private PlayModeTestUtils.TabuleiroDeTeste tabuleiro;

        [TearDown]
        public void TearDown()
        {
            tabuleiro.Destruir();
        }

        [UnityTest]
        public IEnumerator PecaTravadaPorCorrente_TrySwapNaoAlteraOTabuleiro()
        {
            tabuleiro = PlayModeTestUtils.ConstruirTabuleiro(3, 3, comObstaculos: true);
            tabuleiro.BoardGO.SetActive(true);
            yield return null;

            var grid = tabuleiro.Board.Grid;
            grid[0, 0].Setup(TileType.Pao, 0, 0, null);
            grid[1, 0].Setup(TileType.Peixe, 1, 0, null);

            tabuleiro.Obstacles.PlaceObstacle(ObstacleType.Corrente, 0, 0, Vector3.zero);

            PlayModeTestUtils.InvokePrivateMethod(tabuleiro.Board, "TrySwap", grid[0, 0], grid[1, 0]);
            yield return null;
            yield return null;

            Assert.AreEqual(TileType.Pao, tabuleiro.Board.Grid[0, 0].Type,
                "A peça travada por Corrente não deveria ter sido trocada.");
            Assert.AreEqual(TileType.Peixe, tabuleiro.Board.Grid[1, 0].Type);
        }

        [UnityTest]
        public IEnumerator PedraDoDeserto_IsolaAGravidadeEmSegmentos()
        {
            tabuleiro = PlayModeTestUtils.ConstruirTabuleiro(3, 5, comObstaculos: true);
            tabuleiro.BoardGO.SetActive(true);
            yield return null;

            var grid = tabuleiro.Board.Grid;

            // Colunas 0 e 2: padrão diagonal, garante ausência de qualquer match acidental.
            for (int y = 0; y < 5; y++)
            {
                grid[0, y].Setup((TileType)((0 + y) % 6), 0, y, null);
                grid[2, y].Setup((TileType)((2 + y) % 6), 2, y, null);
            }

            // Coluna 1: y=0,1 (segmento inferior) | y=2 = Pedra | y=3,4 (segmento superior)
            grid[1, 0].Setup(TileType.Pao, 1, 0, null);
            grid[1, 1].Setup(TileType.Peixe, 1, 1, null);
            grid[1, 3].Setup(TileType.Uva, 1, 3, null);
            grid[1, 4].Setup(TileType.Espiga, 1, 4, null);

            tabuleiro.Board.RemoverPecaEBloquear(1, 2);
            tabuleiro.Obstacles.PlaceObstacle(ObstacleType.PedraDeserto, 1, 2, Vector3.zero);

            // Destrói a peça no topo do segmento superior e resolve a queda.
            var result = new MatchResult();
            result.TilesToDestroy.Add(grid[1, 4]);

            yield return tabuleiro.Physics.ResolveBoard(tabuleiro.Detector, tabuleiro.Score, result);

            Assert.AreEqual(TileType.Pao, tabuleiro.Board.Grid[1, 0].Type, "O segmento inferior não deveria ser afetado.");
            Assert.AreEqual(TileType.Peixe, tabuleiro.Board.Grid[1, 1].Type, "O segmento inferior não deveria ser afetado.");
            Assert.IsNull(tabuleiro.Board.Grid[1, 2], "A célula da Pedra continua bloqueada, sem peça.");
            Assert.IsTrue(tabuleiro.Obstacles.IsBlocked(1, 2), "A Pedra ainda não recebeu hits suficientes para quebrar.");
            Assert.AreEqual(TileType.Uva, tabuleiro.Board.Grid[1, 3].Type,
                "A peça do segmento superior não deveria cair para dentro da célula bloqueada.");
            Assert.IsNotNull(tabuleiro.Board.Grid[1, 4], "O topo do segmento superior deveria ter sido reabastecido.");
        }
    }
}
