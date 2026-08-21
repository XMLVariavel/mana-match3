using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace BibleMatch3.Tests
{
    public class GameManagerTests
    {
        private GameObject boardGO, scoreGO, obstacleGO, obstaclePrefab, gmGO;
        private BoardManager board;
        private ScoreAndObjectiveManager score;
        private ObstacleManager obstacles;
        private GameManager gm;

        [SetUp]
        public void SetUp()
        {
            // Board com dimensões padrão (8x8) — Awake roda na hora, então
            // Grid/ActiveTypes já existem sem precisar de GenerateBoard.
            boardGO = new GameObject("Board");
            board = boardGO.AddComponent<BoardManager>();

            scoreGO = new GameObject("Score");
            score = scoreGO.AddComponent<ScoreAndObjectiveManager>();

            obstaclePrefab = new GameObject("ObstaclePrefab");
            obstaclePrefab.AddComponent<Obstacle>();

            obstacleGO = new GameObject("Obstacles");
            obstacles = obstacleGO.AddComponent<ObstacleManager>();
            TestUtils.SetPrivateField(obstacles, "obstaclePrefab", obstaclePrefab);
            obstacles.Initialize(board.Width, board.Height);

            // Inativo até conectar as referências por reflexão, senão o OnEnable
            // (que se inscreve em scoreManager.OnScoreChanged) rodaria cedo demais,
            // com scoreManager ainda nulo.
            gmGO = new GameObject("GameManager");
            gmGO.SetActive(false);
            gm = gmGO.AddComponent<GameManager>();
            TestUtils.SetPrivateField(gm, "boardManager", board);
            TestUtils.SetPrivateField(gm, "scoreManager", score);
            TestUtils.SetPrivateField(gm, "obstacleManager", obstacles);
            gmGO.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(boardGO);
            Object.DestroyImmediate(scoreGO);
            Object.DestroyImmediate(obstacleGO);
            Object.DestroyImmediate(obstaclePrefab);
            Object.DestroyImmediate(gmGO);
        }

        [Test]
        public void IniciarCampanha_ConfiguraMovimentosEEstrelasDaFase()
        {
            var fase = ScriptableObject.CreateInstance<LevelData>();
            fase.Movimentos = 15;
            fase.EstrelaScore1 = 500;
            fase.EstrelaScore2 = 1000;
            fase.EstrelaScore3 = 1500;
            fase.Objetivos = new List<ObjectiveEntry> { new ObjectiveEntry { Type = TileType.Uva, RequiredAmount = 5 } };

            gm.IniciarCampanha(fase);

            Assert.AreEqual(GameMode.Campanha, gm.ModoAtual);
            Assert.AreEqual(15, score.MovesRemaining);

            Object.DestroyImmediate(fase);
        }

        [Test]
        public void IniciarContraRelogio_UsaTempoConfiguradoEIgnoraMovimentos()
        {
            TestUtils.SetPrivateField(gm, "duracaoContraRelogio", 45f);

            gm.IniciarContraRelogio();

            Assert.AreEqual(GameMode.ContraRelogio, gm.ModoAtual);
            Assert.IsTrue(gm.ModoTemporizado);
            Assert.IsFalse(gm.ModoUsaLimiteDeMovimentos);
            Assert.AreEqual(45f, gm.TempoRestante);
            Assert.AreEqual(int.MaxValue, score.MovesRemaining);
        }

        [Test]
        public void ModosComObjetivosMantemLimiteDeMovimentos()
        {
            gm.IniciarDesafioDiario();
            Assert.IsTrue(gm.ModoUsaLimiteDeMovimentos);
            Assert.AreEqual(30, score.MovesRemaining);

            gm.IniciarGuardiaoDaPalavra();
            Assert.IsTrue(gm.ModoUsaLimiteDeMovimentos);
            Assert.AreEqual(35, score.MovesRemaining);
            Assert.IsTrue(score.HasObjectives);
        }

        [Test]
        public void IniciarCampanha_PosicionaPedraDoDeserto_BloqueiaACelula()
        {
            var fase = ScriptableObject.CreateInstance<LevelData>();
            fase.Obstaculos = new List<ObstaculoPosicionado>
            {
                new ObstaculoPosicionado { Tipo = ObstacleType.PedraDeserto, X = 2, Y = 2 }
            };

            gm.IniciarCampanha(fase);

            Assert.IsTrue(obstacles.IsBlocked(2, 2));

            Object.DestroyImmediate(fase);
        }

        [Test]
        public void IniciarCampanha_PosicionaCorrente_TravaACelula()
        {
            var fase = ScriptableObject.CreateInstance<LevelData>();
            fase.Obstaculos = new List<ObstaculoPosicionado>
            {
                new ObstaculoPosicionado { Tipo = ObstacleType.Corrente, X = 1, Y = 1 }
            };

            gm.IniciarCampanha(fase);

            Assert.IsTrue(obstacles.IsLocked(1, 1));

            Object.DestroyImmediate(fase);
        }

        [Test]
        public void IniciarEstudoInfinito_ComecaComPoucosTiposDePeca()
        {
            TestUtils.SetPrivateField(gm, "tiposIniciaisInfinito", 3);

            gm.IniciarEstudoInfinito();

            Assert.AreEqual(GameMode.EstudoInfinito, gm.ModoAtual);
            Assert.AreEqual(3, board.ActiveTypes.Count);
            Assert.AreEqual(TileType.Pao, board.ActiveTypes[0]);
            Assert.AreEqual(TileType.Uva, board.ActiveTypes[2]);
        }

        [Test]
        public void EstudoInfinito_AoCruzarMarcoDePontos_AumentaVariedadeDePecas()
        {
            TestUtils.SetPrivateField(gm, "tiposIniciaisInfinito", 3);
            TestUtils.SetPrivateField(gm, "pontosPorEscalonamento", 100);
            TestUtils.SetPrivateField(gm, "chanceBaseDeObstaculo", -1f); // nunca dispara, não interfere no teste
            TestUtils.SetPrivateField(gm, "pontosPorVersiculo", int.MaxValue); // idem, pra versículo

            gm.IniciarEstudoInfinito();
            Assert.AreEqual(3, board.ActiveTypes.Count);

            int nivelRecebido = -1;
            gm.OnDificuldadeAumentou += n => nivelRecebido = n;

            score.AddScore(150, TileType.Pao); // cruza o marco de 100

            Assert.AreEqual(1, nivelRecebido);
            Assert.AreEqual(4, board.ActiveTypes.Count);
        }

        [Test]
        public void EstudoInfinito_AoCruzarMarcoDePontos_ExibeVersiculoEGanhaXp()
        {
            var versiculo = ScriptableObject.CreateInstance<VerseData>();
            versiculo.Texto = "Texto de teste";
            versiculo.Referencia = "Teste 1:1";

            TestUtils.SetPrivateField(gm, "pontosPorVersiculo", 100);
            TestUtils.SetPrivateField(gm, "xpBonusPorVersiculo", 25);
            TestUtils.SetPrivateField(gm, "versiculosDisponiveis", new List<VerseData> { versiculo });
            TestUtils.SetPrivateField(gm, "pontosPorEscalonamento", int.MaxValue);
            TestUtils.SetPrivateField(gm, "chanceBaseDeObstaculo", -1f);

            gm.IniciarEstudoInfinito();

            VerseData versiculoRecebido = null;
            gm.OnVersiculoExibido += v => versiculoRecebido = v;

            score.AddScore(120, TileType.Pao);

            Assert.AreEqual(versiculo, versiculoRecebido);
            Assert.AreEqual(25, gm.XpAcumulado);

            Object.DestroyImmediate(versiculo);
        }

        [Test]
        public void EstudoInfinito_ComChance100PorCento_SempreInsereObstaculoEmCelulaComPeca()
        {
            for (int x = 0; x < board.Width; x++)
                for (int y = 0; y < board.Height; y++)
                    board.Grid[x, y] = TestUtils.CriarTile(TileType.Pao, x, y);

            TestUtils.SetPrivateField(gm, "chanceBaseDeObstaculo", 2f); // sempre dispara
            TestUtils.SetPrivateField(gm, "incrementoChancePorNivel", 0f);
            TestUtils.SetPrivateField(gm, "pontosPorEscalonamento", int.MaxValue);
            TestUtils.SetPrivateField(gm, "pontosPorVersiculo", int.MaxValue);

            gm.IniciarEstudoInfinito();
            score.AddScore(10, TileType.Pao);

            bool algumObstaculoNasceu = false;
            for (int x = 0; x < board.Width && !algumObstaculoNasceu; x++)
                for (int y = 0; y < board.Height && !algumObstaculoNasceu; y++)
                    if (obstacles.GetObstacle(x, y) != null) algumObstaculoNasceu = true;

            Assert.IsTrue(algumObstaculoNasceu, "Com 100% de chance, um obstáculo deveria ter nascido em alguma célula.");

            for (int x = 0; x < board.Width; x++)
                for (int y = 0; y < board.Height; y++)
                    Object.DestroyImmediate(board.Grid[x, y].gameObject);
        }
    }
}
