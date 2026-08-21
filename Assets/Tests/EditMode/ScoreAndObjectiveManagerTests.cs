using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace BibleMatch3.Tests
{
    public class ScoreAndObjectiveManagerTests
    {
        private GameObject managerObject;
        private ScoreAndObjectiveManager manager;

        [SetUp]
        public void SetUp()
        {
            managerObject = new GameObject("ScoreManager");
            manager = managerObject.AddComponent<ScoreAndObjectiveManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(managerObject);
        }

        private void ConfigurarObjetivo(TileType tipo, int quantidade)
        {
            var objetivo = new ObjectiveEntry { Type = tipo, RequiredAmount = quantidade };
            TestUtils.SetPrivateField(manager, "objectives", new List<ObjectiveEntry> { objetivo });
        }

        [Test]
        public void AddScore_IncrementaPontuacaoCorretamente()
        {
            manager.AddScore(50, TileType.Pao);
            manager.AddScore(30, TileType.Peixe);

            Assert.AreEqual(80, manager.CurrentScore);
        }

        [Test]
        public void UseMove_DecrementaMovimentosENuncaFicaNegativo()
        {
            TestUtils.SetPrivateField(manager, "movesRemaining", 1);

            manager.UseMove();
            Assert.AreEqual(0, manager.MovesRemaining);

            manager.UseMove(); // já em zero — não deve ir a negativo nem lançar exceção
            Assert.AreEqual(0, manager.MovesRemaining);
        }

        [Test]
        public void UseMove_SemMovimentosEObjetivoIncompleto_DisparaDerrota()
        {
            ConfigurarObjetivo(TileType.Uva, 10);
            TestUtils.SetPrivateField(manager, "movesRemaining", 1);

            bool perdeu = false;
            manager.OnLose += () => perdeu = true;

            manager.UseMove();

            Assert.IsTrue(perdeu);
            Assert.IsTrue(manager.LevelEnded);
        }

        [Test]
        public void AddScore_ObjetivoCompleto_DisparaVitoriaComEstrelasCorretas()
        {
            ConfigurarObjetivo(TileType.Uva, 2);
            TestUtils.SetPrivateField(manager, "scoreForStar1", 100);
            TestUtils.SetPrivateField(manager, "scoreForStar2", 200);
            TestUtils.SetPrivateField(manager, "scoreForStar3", 300);

            int estrelasRecebidas = -1;
            manager.OnWin += estrelas => estrelasRecebidas = estrelas;

            manager.AddScore(150, TileType.Uva);
            manager.AddScore(150, TileType.Uva); // 2ª ocorrência completa o objetivo; score total = 300

            Assert.IsTrue(manager.LevelEnded);
            Assert.AreEqual(3, estrelasRecebidas);
        }

        [Test]
        public void AddMoves_ReabreFaseSoQuandoTerminouPorDerrota()
        {
            ConfigurarObjetivo(TileType.Uva, 10);
            TestUtils.SetPrivateField(manager, "movesRemaining", 1);
            manager.UseMove(); // dispara derrota

            Assert.IsTrue(manager.LevelEnded);

            manager.AddMoves(5);

            Assert.IsFalse(manager.LevelEnded);
            Assert.AreEqual(5, manager.MovesRemaining);
        }

        [Test]
        public void Configurar_ReiniciaPontuacaoEMovimentos()
        {
            manager.AddScore(500, TileType.Pao);
            Assert.AreEqual(500, manager.CurrentScore);

            manager.Configurar(30, new List<ObjectiveEntry> { new ObjectiveEntry { Type = TileType.Uva, RequiredAmount = 3 } }, 100, 200, 300);

            Assert.AreEqual(0, manager.CurrentScore, "Configurar deveria reiniciar a pontuação.");
            Assert.AreEqual(30, manager.MovesRemaining);
            Assert.IsFalse(manager.LevelEnded);
        }

        [Test]
        public void Configurar_NaoCompartilhaReferenciaComOTemplateDeObjetivos()
        {
            var template = new List<ObjectiveEntry> { new ObjectiveEntry { Type = TileType.Uva, RequiredAmount = 1 } };

            manager.Configurar(10, template, 10, 20, 30);
            manager.AddScore(1, TileType.Uva); // completa o objetivo interno do manager

            Assert.AreEqual(0, template[0].CurrentAmount,
                "O LevelData original não deveria ser alterado pelo progresso da partida (evita 'vazar' entre replays).");
        }

        [Test]
        public void AddScore_SemObjetivosConfigurados_NuncaDisparaVitoriaAutomatica()
        {
            // Cenário do Estudo Infinito: Configurar(..., objectivesTemplate: null, ...)
            manager.Configurar(int.MaxValue, null, 0, 0, 0);

            bool venceu = false;
            manager.OnWin += _ => venceu = true;

            manager.AddScore(9999, TileType.Pao);

            Assert.IsFalse(venceu, "Sem objetivos configurados, não deveria haver vitória automática.");
            Assert.IsFalse(manager.LevelEnded);
        }

        [Test]
        public void GetPriorityObjectiveType_RetornaOTipoComMaiorQuantidadePendente()
        {
            var objetivos = new List<ObjectiveEntry>
            {
                new ObjectiveEntry { Type = TileType.Pao, RequiredAmount = 5, CurrentAmount = 4 }, // falta 1
                new ObjectiveEntry { Type = TileType.Uva, RequiredAmount = 10, CurrentAmount = 2 } // falta 8
            };
            TestUtils.SetPrivateField(manager, "objectives", objetivos);

            TileType? prioridade = manager.GetPriorityObjectiveType();

            Assert.AreEqual(TileType.Uva, prioridade);
        }
    }
}
