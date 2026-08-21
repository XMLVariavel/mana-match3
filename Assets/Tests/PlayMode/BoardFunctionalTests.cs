using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BibleMatch3.Tests
{
    public class BoardFunctionalTests
    {
        private PlayModeTestUtils.TabuleiroDeTeste tabuleiro;

        [TearDown]
        public void TearDown()
        {
            tabuleiro.Destruir();
        }

        private void DefinirTipo(Tile[,] grid, int x, int y, TileType type) => grid[x, y].Setup(type, x, y, null);

        [UnityTest]
        public IEnumerator GenerateBoard_NaoComecaComNenhumMatchPronto()
        {
            tabuleiro = PlayModeTestUtils.ConstruirTabuleiro(8, 8, comObstaculos: false);
            tabuleiro.BoardGO.SetActive(true);
            yield return null; // deixa o Start() rodar o GenerateBoard

            MatchResult result = tabuleiro.Detector.FindMatches(tabuleiro.Board.Grid, tabuleiro.Board.Width, tabuleiro.Board.Height);

            Assert.AreEqual(0, result.TilesToDestroy.Count,
                "O tabuleiro inicial não deveria conter nenhuma combinação pronta.");
        }

        [UnityTest]
        public IEnumerator TrocaValida_FormaMatchEAumentaAPontuacao()
        {
            tabuleiro = PlayModeTestUtils.ConstruirTabuleiro(3, 3, comObstaculos: false);
            tabuleiro.BoardGO.SetActive(true);
            yield return null;

            var grid = tabuleiro.Board.Grid;
            // Trocar (1,0) com (1,1) forma Pao,Pao,Pao na linha y=0.
            DefinirTipo(grid, 0, 0, TileType.Pao);
            DefinirTipo(grid, 1, 0, TileType.Peixe); // recebe Pao ao trocar
            DefinirTipo(grid, 2, 0, TileType.Pao);
            DefinirTipo(grid, 0, 1, TileType.Uva);
            DefinirTipo(grid, 1, 1, TileType.Pao);   // move para (1,0)
            DefinirTipo(grid, 2, 1, TileType.Espiga);
            DefinirTipo(grid, 0, 2, TileType.Azeite);
            DefinirTipo(grid, 1, 2, TileType.Pomba);
            DefinirTipo(grid, 2, 2, TileType.Uva);

            int scoreAntes = tabuleiro.Score.CurrentScore;

            yield return PlayModeTestUtils.InvokePrivateCoroutine(tabuleiro.Board, "SwapRoutine", grid[1, 0], grid[1, 1]);

            Assert.Greater(tabuleiro.Score.CurrentScore, scoreAntes,
                "A troca válida deveria ter pontuado ao formar um match.");
        }

        [UnityTest]
        public IEnumerator TrocaInvalida_DesfazAPosicaoOriginal()
        {
            tabuleiro = PlayModeTestUtils.ConstruirTabuleiro(3, 3, comObstaculos: false);
            tabuleiro.BoardGO.SetActive(true);
            yield return null;

            var grid = tabuleiro.Board.Grid;
            // Padrão diagonal (tipo = (x+y) % 6): garante ausência de match em qualquer troca de vizinhos.
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    DefinirTipo(grid, x, y, (TileType)((x + y) % 6));

            Tile a = grid[0, 0]; // Pao
            Tile b = grid[1, 0]; // Peixe

            yield return PlayModeTestUtils.InvokePrivateCoroutine(tabuleiro.Board, "SwapRoutine", a, b);

            Assert.AreEqual(TileType.Pao, tabuleiro.Board.Grid[0, 0].Type,
                "A troca inválida deveria ter sido revertida.");
            Assert.AreEqual(TileType.Peixe, tabuleiro.Board.Grid[1, 0].Type);
        }
    }
}
