using System.Collections.Generic;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Um obstáculo posicionado em uma célula específica de uma fase da Campanha.
    /// </summary>
    [System.Serializable]
    public class ObstaculoPosicionado
    {
        public ObstacleType Tipo;
        public int X;
        public int Y;
    }

    /// <summary>
    /// Dados de uma fase da Campanha — um asset por fase. O GameManager lê este
    /// asset e traduz para chamadas em ScoreAndObjectiveManager/ObstacleManager;
    /// o LevelData em si não conhece nem manipula esses managers diretamente.
    /// </summary>
    [CreateAssetMenu(fileName = "NovaFase", menuName = "BibleMatch3/Fase de Campanha")]
    public class LevelData : ScriptableObject
    {
        [Header("Identificação")]
        public int Numero;
        public string Nome;

        [Header("Regras")]
        public int Movimentos = 20;
        public List<ObjectiveEntry> Objetivos = new List<ObjectiveEntry>();
        public int EstrelaScore1 = 1000;
        public int EstrelaScore2 = 2000;
        public int EstrelaScore3 = 3000;

        [Header("Obstáculos")]
        public List<ObstaculoPosicionado> Obstaculos = new List<ObstaculoPosicionado>();
    }
}
