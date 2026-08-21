using NUnit.Framework;
using UnityEngine;

namespace BibleMatch3.Tests
{
    public class MatchDetectorTests
    {
        private GameObject detectorObject;
        private MatchDetector detector;

        [SetUp]
        public void SetUp()
        {
            detectorObject = new GameObject("MatchDetector");
            detector = detectorObject.AddComponent<MatchDetector>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(detectorObject);
        }

        [Test]
        public void FindMatches_TresNaHorizontal_DestroiAsTresPecas()
        {
            var tipos = new TileType[5, 1];
            tipos[0, 0] = TileType.Pao;
            tipos[1, 0] = TileType.Pao;
            tipos[2, 0] = TileType.Pao;
            tipos[3, 0] = TileType.Peixe;
            tipos[4, 0] = TileType.Uva;

            Tile[,] grid = TestUtils.CriarGrid(tipos);
            MatchResult result = detector.FindMatches(grid, 5, 1);

            Assert.AreEqual(3, result.TilesToDestroy.Count);
            Assert.IsTrue(result.TilesToDestroy.Contains(grid[0, 0]));
            Assert.IsTrue(result.TilesToDestroy.Contains(grid[1, 0]));
            Assert.IsTrue(result.TilesToDestroy.Contains(grid[2, 0]));

            TestUtils.DestruirGrid(grid);
        }

        [Test]
        public void FindMatches_QuatroNaHorizontal_GeraEspadaLinha()
        {
            var tipos = new TileType[4, 1];
            for (int x = 0; x < 4; x++) tipos[x, 0] = TileType.Peixe;

            Tile[,] grid = TestUtils.CriarGrid(tipos);
            MatchResult result = detector.FindMatches(grid, 4, 1);

            Assert.AreEqual(4, result.TilesToDestroy.Count);
            Assert.AreEqual(1, result.SpecialsToSpawn.Count);
            Assert.AreEqual(SpecialType.Espada_Linha, result.SpecialsToSpawn[0].Type);

            TestUtils.DestruirGrid(grid);
        }

        [Test]
        public void FindMatches_CincoEmLinha_GeraArcaDaAlianca()
        {
            var tipos = new TileType[5, 1];
            for (int x = 0; x < 5; x++) tipos[x, 0] = TileType.Uva;

            Tile[,] grid = TestUtils.CriarGrid(tipos);
            MatchResult result = detector.FindMatches(grid, 5, 1);

            Assert.AreEqual(1, result.SpecialsToSpawn.Count);
            Assert.AreEqual(SpecialType.Arca_Alianca, result.SpecialsToSpawn[0].Type);

            TestUtils.DestruirGrid(grid);
        }

        [Test]
        public void FindMatches_FormatoL_GeraTochaAcesa()
        {
            // L: 3 peças na horizontal (y=0) + 3 na vertical (x=0), compartilhando (0,0).
            var tipos = new TileType[3, 3];
            for (int x = 0; x < 3; x++) tipos[x, 0] = TileType.Espiga;
            for (int y = 0; y < 3; y++) tipos[0, y] = TileType.Espiga;
            tipos[1, 1] = TileType.Azeite;
            tipos[2, 1] = TileType.Pomba;
            tipos[1, 2] = TileType.Peixe;
            tipos[2, 2] = TileType.Pao;

            Tile[,] grid = TestUtils.CriarGrid(tipos);
            MatchResult result = detector.FindMatches(grid, 3, 3);

            bool geraTocha = result.SpecialsToSpawn.Exists(s =>
                s.Type == SpecialType.Tocha_Acesa && s.X == 0 && s.Y == 0);
            Assert.IsTrue(geraTocha, "Esperava uma Tocha Acesa nascendo na interseção (0,0).");

            TestUtils.DestruirGrid(grid);
        }

        [Test]
        public void FindMatches_Bloco2x2_GeraEstrelaGuia()
        {
            var tipos = new TileType[2, 2];
            tipos[0, 0] = TileType.Azeite;
            tipos[1, 0] = TileType.Azeite;
            tipos[0, 1] = TileType.Azeite;
            tipos[1, 1] = TileType.Azeite;

            Tile[,] grid = TestUtils.CriarGrid(tipos);
            MatchResult result = detector.FindMatches(grid, 2, 2);

            Assert.AreEqual(4, result.TilesToDestroy.Count);
            Assert.AreEqual(1, result.SpecialsToSpawn.Count);
            Assert.AreEqual(SpecialType.Estrela_Guia, result.SpecialsToSpawn[0].Type);

            TestUtils.DestruirGrid(grid);
        }

        [Test]
        public void FindMatches_SemCombinacoes_NaoDestroiNada()
        {
            var tipos = new TileType[3, 3]
            {
                { TileType.Pao, TileType.Peixe, TileType.Uva },
                { TileType.Peixe, TileType.Uva, TileType.Pao },
                { TileType.Uva, TileType.Pao, TileType.Peixe }
            };

            Tile[,] grid = TestUtils.CriarGrid(tipos);
            MatchResult result = detector.FindMatches(grid, 3, 3);

            Assert.AreEqual(0, result.TilesToDestroy.Count);

            TestUtils.DestruirGrid(grid);
        }

        [Test]
        public void ResolveSpecialCombo_DuasEspadas_LimpaLinhaEColuna()
        {
            var tipos = new TileType[3, 3];
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    tipos[x, y] = TileType.Pao;

            Tile[,] grid = TestUtils.CriarGrid(tipos);
            Tile espadaA = grid[1, 1];
            espadaA.Special = SpecialType.Espada_Linha;
            Tile espadaB = grid[1, 2];
            espadaB.Special = SpecialType.Espada_Coluna;

            MatchResult result = detector.ResolveSpecialCombo(grid, 3, 3, espadaA, espadaB);

            // Linha y=1 (3 peças) + coluna x=1 (3 peças), com (1,1) contado uma única vez = 5.
            Assert.AreEqual(5, result.TilesToDestroy.Count);

            TestUtils.DestruirGrid(grid);
        }
    }
}
