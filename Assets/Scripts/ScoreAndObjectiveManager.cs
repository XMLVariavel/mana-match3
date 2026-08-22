using System;
using System.Collections.Generic;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Um objetivo da fase: coletar uma certa quantidade de um tipo de peça.
    /// </summary>
    [Serializable]
    public class ObjectiveEntry
    {
        public TileType Type;
        public int RequiredAmount;
        [HideInInspector] public int CurrentAmount;

        public bool IsComplete => CurrentAmount >= RequiredAmount;
    }

    /// <summary>
    /// Controla pontuação, movimentos restantes, objetivos da fase e as
    /// condições de vitória (com estrelas) e derrota. Não conhece UI diretamente —
    /// expõe eventos para que o Canvas/HUD se inscreva e se atualize.
    /// </summary>
    public class ScoreAndObjectiveManager : MonoBehaviour
    {
        [Header("Movimentos")]
        [SerializeField] private int movesRemaining = 20;

        [Header("Metas de Estrelas")]
        [SerializeField] private int scoreForStar1 = 1000;
        [SerializeField] private int scoreForStar2 = 2000;
        [SerializeField] private int scoreForStar3 = 3000;

        [Header("Objetivos da Fase")]
        [SerializeField] private List<ObjectiveEntry> objectives = new List<ObjectiveEntry>();

        public int CurrentScore { get; private set; }
        public int MovesRemaining => movesRemaining;
        public bool LevelEnded { get; private set; }
        public bool HasObjectives => objectives != null && objectives.Count > 0;
        public int ScoreForStar1 => scoreForStar1;
        public int ScoreForStar2 => scoreForStar2;
        public int ScoreForStar3 => scoreForStar3;
        public int ObjectivesCurrentTotal
        {
            get
            {
                int total = 0;
                if (objectives == null) return total;
                foreach (ObjectiveEntry objective in objectives)
                    total += Mathf.Min(objective.CurrentAmount, objective.RequiredAmount);
                return total;
            }
        }
        public int ObjectivesTotalRequired
        {
            get
            {
                int total = 0;
                if (objectives == null) return total;
                foreach (ObjectiveEntry objective in objectives)
                    total += Mathf.Max(0, objective.RequiredAmount);
                return total;
            }
        }
        public float ObjectivesProgress01 => ObjectivesTotalRequired > 0
            ? Mathf.Clamp01(ObjectivesCurrentTotal / (float)ObjectivesTotalRequired)
            : 0f;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnMovesChanged;
        public event Action<int> OnWin;   // parâmetro = número de estrelas (1 a 3)
        public event Action OnLose;
        public event Action<TileType, int> OnObjectiveProgress; // tipo + quantidade restante

        private void Awake()
        {
            // A primeira fase de demonstração precisa abrir com a faixa de
            // objetivos preenchida, mesmo antes de um LevelData ser carregado.
            // Uma fase real substitui esta lista em Configurar().
            if (objectives == null || objectives.Count == 0)
            {
                objectives = new List<ObjectiveEntry>
                {
                    new ObjectiveEntry { Type = TileType.Pao, RequiredAmount = 20 },
                    new ObjectiveEntry { Type = TileType.Pomba, RequiredAmount = 14 },
                    new ObjectiveEntry { Type = TileType.Azeite, RequiredAmount = 18 }
                };
            }
        }

        /// <summary>
        /// Configura (ou reconfigura) esta instância para uma nova fase/sessão —
        /// usado pelo GameManager tanto para carregar uma fase da Campanha quanto
        /// para preparar o Estudo Infinito (movimentos "ilimitados", sem objetivos).
        /// Sempre reinicia pontuação e progresso, mesmo reaproveitando o mesmo asset.
        /// </summary>
        public void Configurar(int moves, List<ObjectiveEntry> objectivesTemplate, int estrela1, int estrela2, int estrela3)
        {
            movesRemaining = moves;
            scoreForStar1 = estrela1;
            scoreForStar2 = estrela2;
            scoreForStar3 = estrela3;

            objectives = new List<ObjectiveEntry>();
            if (objectivesTemplate != null)
            {
                foreach (ObjectiveEntry template in objectivesTemplate)
                {
                    objectives.Add(new ObjectiveEntry
                    {
                        Type = template.Type,
                        RequiredAmount = template.RequiredAmount,
                        CurrentAmount = 0
                    });
                }
            }

            // Fases antigas ou assets placeholder podem não ter objetivos.
            // Mantemos a faixa rica na Campanha, mas preservamos os modos sem
            // metas fixas (Estudo Infinito e Contra o Relógio).
            if (objectives.Count == 0 && moves != int.MaxValue)
            {
                objectives.Add(new ObjectiveEntry { Type = TileType.Pao, RequiredAmount = 20 });
                objectives.Add(new ObjectiveEntry { Type = TileType.Pomba, RequiredAmount = 14 });
                objectives.Add(new ObjectiveEntry { Type = TileType.Azeite, RequiredAmount = 18 });
            }

            CurrentScore = 0;
            LevelEnded = false;
        }

        /// <summary>
        /// Reemite o estado atual para views que entram depois da configuração
        /// da fase, como a HUD ativada pelo ScreenNavigator.
        /// </summary>
        public void EmitCurrentState()
        {
            OnScoreChanged?.Invoke(CurrentScore);
            OnMovesChanged?.Invoke(movesRemaining);

            if (objectives == null) return;
            foreach (ObjectiveEntry objective in objectives)
                OnObjectiveProgress?.Invoke(objective.Type, objective.RequiredAmount - objective.CurrentAmount);
        }

        /// <summary>
        /// Chamado pelo BoardPhysics sempre que uma peça é destruída.
        /// </summary>
        public void AddScore(int points, TileType type)
        {
            if (LevelEnded) return;

            CurrentScore += points;
            OnScoreChanged?.Invoke(CurrentScore);

            foreach (ObjectiveEntry objective in objectives)
            {
                if (objective.Type == type && !objective.IsComplete)
                {
                    objective.CurrentAmount++;
                    OnObjectiveProgress?.Invoke(objective.Type, objective.RequiredAmount - objective.CurrentAmount);
                }
            }

            CheckWinCondition();
        }

        /// <summary>
        /// Chamado pelo poder avulso "+5 Movimentos" — estende os movimentos da fase.
        /// Se a fase já havia terminado por falta de movimento, ela é reaberta.
        /// </summary>
        public void AddMoves(int amount)
        {
            if (movesRemaining == int.MaxValue) return;
            movesRemaining = Mathf.Clamp(movesRemaining + amount, 0, int.MaxValue);

            // Só reabre a fase se ela havia terminado por falta de movimento (derrota);
            // uma fase já vencida não deve ser "desfeita" por este poder.
            if (LevelEnded && !AllObjectivesComplete())
                LevelEnded = false;

            OnMovesChanged?.Invoke(movesRemaining);
        }

        /// <summary>
        /// Chamado pelo BoardManager sempre que um movimento válido do jogador é consumido.
        /// </summary>
        public void UseMove()
        {
            if (LevelEnded) return;

            movesRemaining = Mathf.Max(0, movesRemaining - 1);
            OnMovesChanged?.Invoke(movesRemaining);

            if (movesRemaining <= 0)
                CheckLoseCondition();
        }

        /// <summary>
        /// Retorna o tipo de peça com maior quantidade pendente nos objetivos —
        /// usado pela Pomba da Paz (Pomba_Guiada) para saber o que buscar no tabuleiro.
        /// </summary>
        public TileType? GetPriorityObjectiveType()
        {
            ObjectiveEntry best = null;
            foreach (ObjectiveEntry objective in objectives)
            {
                if (objective.IsComplete) continue;

                int remaining = objective.RequiredAmount - objective.CurrentAmount;
                int bestRemaining = best != null ? best.RequiredAmount - best.CurrentAmount : -1;

                if (best == null || remaining > bestRemaining)
                    best = objective;
            }
            return best?.Type;
        }

        private bool AllObjectivesComplete()
        {
            // Sem objetivos configurados (ex: Estudo Infinito) não existe "vitória por
            // objetivo" — do contrário a primeira peça destruída já venceria a fase.
            if (objectives == null || objectives.Count == 0) return false;

            foreach (ObjectiveEntry objective in objectives)
                if (!objective.IsComplete) return false;
            return true;
        }

        private void CheckWinCondition()
        {
            if (LevelEnded || !AllObjectivesComplete()) return;

            LevelEnded = true;
            int stars = CalculateStars(CurrentScore);
            OnWin?.Invoke(stars);
        }

        /// <summary>
        /// Chamado só pelo modo Contra o Relógio quando o tempo acaba — esse é
        /// o desfecho NORMAL desse modo (bater o recorde antes do tempo passar),
        /// não uma derrota. Por isso dispara OnWin (0 estrelas, já que esse modo
        /// não usa scoreForStarX), preservando o CurrentScore para exibição.
        /// </summary>
        public void EncerrarPorTempo()
        {
            if (LevelEnded) return;
            LevelEnded = true;
            OnWin?.Invoke(0);
        }

        private void CheckLoseCondition()
        {
            if (LevelEnded) return;

            if (!AllObjectivesComplete())
            {
                LevelEnded = true;
                OnLose?.Invoke();
            }
        }

        private int CalculateStars(int score)
        {
            if (score >= scoreForStar3) return 3;
            if (score >= scoreForStar2) return 2;
            if (score >= scoreForStar1) return 1;
            return 0;
        }
    }
}
