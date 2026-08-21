using NUnit.Framework;
using UnityEngine;

namespace BibleMatch3.Tests
{
    /// <summary>
    /// Não-funcional / performance: comprova que o pool de peças do
    /// BoardPhysics realmente reaproveita instâncias em vez de instanciar/
    /// destruir objetos a cada ciclo — o principal cuidado de performance
    /// pedido no briefing para dispositivos Android de entrada (evita
    /// picos de GC durante cascatas).
    /// </summary>
    public class PerformanceNonFunctionalTests
    {
        private GameObject boardGO;
        private GameObject physicsGO;
        private BoardPhysics physics;
        private GameObject tilePrefab;

        [SetUp]
        public void SetUp()
        {
            tilePrefab = PlayModeTestUtils.CriarTilePrefab();

            boardGO = new GameObject("BoardStub");
            var board = boardGO.AddComponent<BoardManager>();
            PlayModeTestUtils.SetPrivateField(board, "tilePrefab", tilePrefab);

            physicsGO = new GameObject("Physics");
            physics = physicsGO.AddComponent<BoardPhysics>();
            PlayModeTestUtils.SetPrivateField(physics, "boardManager", board);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(boardGO);
            Object.Destroy(physicsGO);
            Object.Destroy(tilePrefab);
        }

        [Test]
        public void Pool_ReaproveitaAMesmaInstanciaEmVezDeInstanciarDeNovo()
        {
            var primeira = (Tile)PlayModeTestUtils.InvokePrivateMethod(physics, "GetPooledOrNewTile");
            PlayModeTestUtils.InvokePrivateMethod(physics, "ReturnToPool", primeira);

            var segunda = (Tile)PlayModeTestUtils.InvokePrivateMethod(physics, "GetPooledOrNewTile");

            Assert.AreSame(primeira, segunda,
                "Depois de devolvida ao pool, a mesma instância deveria ser reaproveitada em vez de criar uma nova (custo de GC em Android).");
        }

        [Test]
        public void Pool_InstanciaNovaApenasQuandoPoolEstaVazio()
        {
            var a = (Tile)PlayModeTestUtils.InvokePrivateMethod(physics, "GetPooledOrNewTile");
            var b = (Tile)PlayModeTestUtils.InvokePrivateMethod(physics, "GetPooledOrNewTile");

            Assert.AreNotSame(a, b, "Sem nada devolvido ao pool, cada chamada deveria criar uma instância nova.");
        }
    }
}
