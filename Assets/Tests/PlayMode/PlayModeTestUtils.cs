using System.Collections;
using System.Reflection;
using UnityEngine;

namespace BibleMatch3.Tests
{
    /// <summary>
    /// Utilitários para os testes de PlayMode: montagem de um tabuleiro
    /// completo (BoardManager + MatchDetector + BoardPhysics + ScoreManager
    /// e, opcionalmente, ObstacleManager) sem depender de assets arrastados
    /// no Inspector, além de helpers de reflexão para acionar membros
    /// privados durante os testes de integração.
    /// </summary>
    public static class PlayModeTestUtils
    {
        public struct TabuleiroDeTeste
        {
            public GameObject BoardGO, DetectorGO, PhysicsGO, ScoreGO, ObstacleGO, TilePrefab, ObstaclePrefab;
            public BoardManager Board;
            public MatchDetector Detector;
            public BoardPhysics Physics;
            public ScoreAndObjectiveManager Score;
            public ObstacleManager Obstacles;

            public void Destruir()
            {
                if (BoardGO != null) Object.Destroy(BoardGO);
                if (DetectorGO != null) Object.Destroy(DetectorGO);
                if (PhysicsGO != null) Object.Destroy(PhysicsGO);
                if (ScoreGO != null) Object.Destroy(ScoreGO);
                if (ObstacleGO != null) Object.Destroy(ObstacleGO);
                if (TilePrefab != null) Object.Destroy(TilePrefab);
                if (ObstaclePrefab != null) Object.Destroy(ObstaclePrefab);
            }
        }

        public static GameObject CriarTilePrefab()
        {
            var prefab = new GameObject("TestTilePrefab");
            prefab.AddComponent<SpriteRenderer>();
            prefab.AddComponent<BoxCollider2D>();
            prefab.AddComponent<Tile>();
            return prefab;
        }

        /// <summary>
        /// Monta os managers e conecta tudo por reflexão. Deixa o GameObject do
        /// BoardManager inativo — o chamador deve ativá-lo (o que dispara Awake)
        /// e aguardar um frame (para o Start/GenerateBoard rodar) antes de usar.
        /// </summary>
        public static TabuleiroDeTeste ConstruirTabuleiro(int width, int height, bool comObstaculos)
        {
            var t = new TabuleiroDeTeste();

            t.TilePrefab = CriarTilePrefab();

            t.DetectorGO = new GameObject("Detector");
            t.Detector = t.DetectorGO.AddComponent<MatchDetector>();

            t.PhysicsGO = new GameObject("Physics");
            t.Physics = t.PhysicsGO.AddComponent<BoardPhysics>();

            t.ScoreGO = new GameObject("Score");
            t.Score = t.ScoreGO.AddComponent<ScoreAndObjectiveManager>();

            if (comObstaculos)
            {
                t.ObstaclePrefab = new GameObject("ObstaclePrefab");
                t.ObstaclePrefab.AddComponent<Obstacle>();

                t.ObstacleGO = new GameObject("Obstacles");
                t.Obstacles = t.ObstacleGO.AddComponent<ObstacleManager>();
                SetPrivateField(t.Obstacles, "obstaclePrefab", t.ObstaclePrefab);
                t.Obstacles.Initialize(width, height);

                SetPrivateField(t.Physics, "obstacleManager", t.Obstacles);
            }

            t.BoardGO = new GameObject("Board");
            t.BoardGO.SetActive(false);
            t.Board = t.BoardGO.AddComponent<BoardManager>();

            SetPrivateField(t.Board, "width", width);
            SetPrivateField(t.Board, "height", height);
            SetPrivateField(t.Board, "cellSize", 1f);
            SetPrivateField(t.Board, "tilePrefab", t.TilePrefab);
            SetPrivateField(t.Board, "tileSprites", new Sprite[6]);
            SetPrivateField(t.Board, "specialSprites", new Sprite[6]);
            SetPrivateField(t.Board, "swipeThreshold", 0.4f);
            SetPrivateField(t.Board, "matchDetector", t.Detector);
            SetPrivateField(t.Board, "boardPhysics", t.Physics);
            SetPrivateField(t.Board, "scoreManager", t.Score);
            if (comObstaculos) SetPrivateField(t.Board, "obstacleManager", t.Obstacles);

            SetPrivateField(t.Physics, "boardManager", t.Board);

            return t;
        }

        public static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        public static object InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return method.Invoke(target, args);
        }

        public static IEnumerator InvokePrivateCoroutine(object target, string methodName, params object[] args)
        {
            return (IEnumerator)InvokePrivateMethod(target, methodName, args);
        }
    }
}
