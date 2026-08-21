using NUnit.Framework;
using UnityEngine;

namespace BibleMatch3.Tests
{
    public class ObstacleManagerTests
    {
        private GameObject managerObject;
        private ObstacleManager manager;
        private GameObject obstaclePrefab;

        [SetUp]
        public void SetUp()
        {
            managerObject = new GameObject("ObstacleManager");
            manager = managerObject.AddComponent<ObstacleManager>();

            obstaclePrefab = new GameObject("ObstaclePrefab");
            obstaclePrefab.AddComponent<Obstacle>();

            TestUtils.SetPrivateField(manager, "obstaclePrefab", obstaclePrefab);
            manager.Initialize(3, 3);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(obstaclePrefab);
        }

        [Test]
        public void PlaceObstacle_PedraDoDeserto_BloqueiaACelula()
        {
            manager.PlaceObstacle(ObstacleType.PedraDeserto, 1, 1, Vector3.zero);

            Assert.IsTrue(manager.IsBlocked(1, 1));
            Assert.IsFalse(manager.IsBlocked(0, 0));
        }

        [Test]
        public void PlaceObstacle_Corrente_TravaACelula()
        {
            manager.PlaceObstacle(ObstacleType.Corrente, 0, 0, Vector3.zero);

            Assert.IsTrue(manager.IsLocked(0, 0));
        }

        [Test]
        public void ResolveObstacles_Gelo_AbsorveHitEPoupaAPecaNestaPassada()
        {
            manager.PlaceObstacle(ObstacleType.Gelo, 1, 1, Vector3.zero);

            Tile peca = TestUtils.CriarTile(TileType.Pao, 1, 1);
            var result = new MatchResult();
            result.TilesToDestroy.Add(peca);

            manager.ResolveObstacles(result, 3, 3);

            Assert.AreEqual(0, result.TilesToDestroy.Count, "O Gelo deveria poupar a peça na primeira passada.");
            Assert.IsNull(manager.GetObstacle(1, 1), "O Gelo tem 1 hit e deveria quebrar após absorver o golpe.");

            Object.DestroyImmediate(peca.gameObject);
        }

        [Test]
        public void ResolveObstacles_PedraComDoisHits_SoQuebraNoSegundoMatchAdjacente()
        {
            manager.PlaceObstacle(ObstacleType.PedraDeserto, 1, 1, Vector3.zero);

            Tile vizinho1 = TestUtils.CriarTile(TileType.Peixe, 0, 1);
            var result1 = new MatchResult();
            result1.TilesToDestroy.Add(vizinho1);
            manager.ResolveObstacles(result1, 3, 3);

            Assert.IsTrue(manager.IsBlocked(1, 1), "Um hit não deveria ser suficiente para quebrar a Pedra.");

            Tile vizinho2 = TestUtils.CriarTile(TileType.Peixe, 1, 0);
            var result2 = new MatchResult();
            result2.TilesToDestroy.Add(vizinho2);
            manager.ResolveObstacles(result2, 3, 3);

            Assert.IsFalse(manager.IsBlocked(1, 1), "Dois hits adjacentes deveriam quebrar a Pedra.");

            Object.DestroyImmediate(vizinho1.gameObject);
            Object.DestroyImmediate(vizinho2.gameObject);
        }
    }
}
