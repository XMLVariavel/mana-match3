using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BibleMatch3.Tests
{
    public class GameManagerFunctionalTests
    {
        private PlayModeTestUtils.TabuleiroDeTeste tabuleiro;
        private GameObject gmGO;
        private GameManager gm;

        [TearDown]
        public void TearDown()
        {
            tabuleiro.Destruir();
            if (gmGO != null) Object.Destroy(gmGO);
        }

        [UnityTest]
        public IEnumerator EstudoInfinito_TrocaValidaNoTabuleiroReal_EscalaDificuldade()
        {
            tabuleiro = PlayModeTestUtils.ConstruirTabuleiro(3, 3, comObstaculos: true);
            tabuleiro.BoardGO.SetActive(true);
            yield return null;

            gmGO = new GameObject("GameManager");
            gmGO.SetActive(false);
            gm = gmGO.AddComponent<GameManager>();
            PlayModeTestUtils.SetPrivateField(gm, "boardManager", tabuleiro.Board);
            PlayModeTestUtils.SetPrivateField(gm, "scoreManager", tabuleiro.Score);
            PlayModeTestUtils.SetPrivateField(gm, "obstacleManager", tabuleiro.Obstacles);
            PlayModeTestUtils.SetPrivateField(gm, "tiposIniciaisInfinito", 3);
            PlayModeTestUtils.SetPrivateField(gm, "pontosPorEscalonamento", 10);
            PlayModeTestUtils.SetPrivateField(gm, "chanceBaseDeObstaculo", -1f);
            PlayModeTestUtils.SetPrivateField(gm, "pontosPorVersiculo", int.MaxValue);
            gmGO.SetActive(true);

            gm.IniciarEstudoInfinito();
            Assert.AreEqual(3, tabuleiro.Board.ActiveTypes.Count);

            int nivelRecebido = -1;
            gm.OnDificuldadeAumentou += n => nivelRecebido = n;

            var grid = tabuleiro.Board.Grid;
            // Trocar (1,0) com (1,1) forma Pao,Pao,Pao na linha y=0 — 3 peças = 30 pontos.
            grid[0, 0].Setup(TileType.Pao, 0, 0, null);
            grid[1, 0].Setup(TileType.Peixe, 1, 0, null);
            grid[2, 0].Setup(TileType.Pao, 2, 0, null);
            grid[0, 1].Setup(TileType.Uva, 0, 1, null);
            grid[1, 1].Setup(TileType.Pao, 1, 1, null);
            grid[2, 1].Setup(TileType.Espiga, 2, 1, null);
            grid[0, 2].Setup(TileType.Azeite, 0, 2, null);
            grid[1, 2].Setup(TileType.Pomba, 1, 2, null);
            grid[2, 2].Setup(TileType.Uva, 2, 2, null);

            yield return PlayModeTestUtils.InvokePrivateCoroutine(tabuleiro.Board, "SwapRoutine", grid[1, 0], grid[1, 1]);

            Assert.Greater(nivelRecebido, -1, "O match real via troca deveria ter cruzado o marco de dificuldade.");
            Assert.Greater(tabuleiro.Board.ActiveTypes.Count, 3, "A variedade de peças deveria ter aumentado.");
        }
    }
}
