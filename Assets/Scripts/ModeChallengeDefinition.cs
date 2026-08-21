using System;
using System.Collections.Generic;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Configuração runtime de uma sessão. Mantém regras, identidade e variação
    /// do desafio fora da HUD e evita espalhar números fixos pelos controllers.
    /// </summary>
    [Serializable]
    public sealed class ModeChallengeDefinition
    {
        public GameMode Mode;
        public string ChallengeId;
        public int Seed;
        public int Moves;
        public float DurationSeconds;
        public int Star1;
        public int Star2;
        public int Star3;
        public bool UsesObjectives;
        public bool UsesMoves;
        public bool UsesTimer;
        public string Briefing;
        public List<ObjectiveEntry> Objectives = new List<ObjectiveEntry>();

        public static ModeChallengeDefinition Campaign(LevelData level)
        {
            return new ModeChallengeDefinition
            {
                Mode = GameMode.Campanha,
                ChallengeId = level != null ? $"campaign-{level.Numero}" : "campaign-demo",
                Moves = level != null ? level.Movimentos : 20,
                Star1 = level != null ? level.EstrelaScore1 : 1000,
                Star2 = level != null ? level.EstrelaScore2 : 2000,
                Star3 = level != null ? level.EstrelaScore3 : 3000,
                UsesObjectives = true,
                UsesMoves = true,
                UsesTimer = false,
                Briefing = "Complete os objetivos da fase antes que os movimentos acabem.",
                Objectives = ClonarObjetivos(level != null ? level.Objetivos : null)
            };
        }

        public static ModeChallengeDefinition Daily(DateTime date)
        {
            int seed = date.Year * 10000 + date.Month * 100 + date.Day;
            int variant = Mathf.Abs(seed) % 3;
            var objetivos = new List<ObjectiveEntry>();

            switch (variant)
            {
                case 0:
                    objetivos.Add(new ObjectiveEntry { Type = TileType.Pao, RequiredAmount = 20 });
                    objetivos.Add(new ObjectiveEntry { Type = TileType.Pomba, RequiredAmount = 14 });
                    objetivos.Add(new ObjectiveEntry { Type = TileType.Azeite, RequiredAmount = 18 });
                    break;
                case 1:
                    objetivos.Add(new ObjectiveEntry { Type = TileType.Peixe, RequiredAmount = 18 });
                    objetivos.Add(new ObjectiveEntry { Type = TileType.Uva, RequiredAmount = 16 });
                    objetivos.Add(new ObjectiveEntry { Type = TileType.Pao, RequiredAmount = 12 });
                    break;
                default:
                    objetivos.Add(new ObjectiveEntry { Type = TileType.Azeite, RequiredAmount = 22 });
                    objetivos.Add(new ObjectiveEntry { Type = TileType.Espiga, RequiredAmount = 18 });
                    objetivos.Add(new ObjectiveEntry { Type = TileType.Pomba, RequiredAmount = 12 });
                    break;
            }

            return new ModeChallengeDefinition
            {
                Mode = GameMode.DesafioDiario,
                ChallengeId = $"daily-{date:yyyyMMdd}-v{variant + 1}",
                Seed = seed,
                Moves = 30,
                Star1 = 2500,
                Star2 = 5000,
                Star3 = 8000,
                UsesObjectives = true,
                UsesMoves = true,
                UsesTimer = false,
                Briefing = "A mesma missão vale para todos os jogadores durante o dia.",
                Objectives = objetivos
            };
        }

        public static ModeChallengeDefinition TimeTrial(float durationSeconds)
        {
            return new ModeChallengeDefinition
            {
                Mode = GameMode.ContraRelogio,
                ChallengeId = "time-trial-standard",
                Moves = int.MaxValue,
                DurationSeconds = Mathf.Max(1f, durationSeconds),
                UsesObjectives = false,
                UsesMoves = false,
                UsesTimer = true,
                Briefing = "Faça o maior score possível antes que o tempo termine."
            };
        }

        public static ModeChallengeDefinition Guardian(DateTime date)
        {
            int week = WeekOfYear(date);
            bool varianteObjetivos = week % 2 == 0;
            var objetivos = varianteObjetivos
                ? new List<ObjectiveEntry>
                {
                    new ObjectiveEntry { Type = TileType.Pao, RequiredAmount = 12 },
                    new ObjectiveEntry { Type = TileType.Peixe, RequiredAmount = 12 }
                }
                : new List<ObjectiveEntry>
                {
                    new ObjectiveEntry { Type = TileType.Pomba, RequiredAmount = 12 },
                    new ObjectiveEntry { Type = TileType.Azeite, RequiredAmount = 12 }
                };

            return new ModeChallengeDefinition
            {
                Mode = GameMode.GuardiaoDaPalavra,
                ChallengeId = $"guardian-week-{week:00}",
                Seed = week,
                Moves = 35,
                Star1 = 2500,
                Star2 = 5000,
                Star3 = 8000,
                UsesObjectives = true,
                UsesMoves = true,
                UsesTimer = false,
                Briefing = varianteObjetivos
                    ? "Proteja a missão coletando Pão e Peixe."
                    : "Proteja a missão coletando Pomba e Azeite.",
                Objectives = objetivos
            };
        }

        private static List<ObjectiveEntry> ClonarObjetivos(List<ObjectiveEntry> origem)
        {
            var resultado = new List<ObjectiveEntry>();
            if (origem == null) return resultado;
            foreach (ObjectiveEntry item in origem)
            {
                if (item == null) continue;
                resultado.Add(new ObjectiveEntry
                {
                    Type = item.Type,
                    RequiredAmount = item.RequiredAmount,
                    CurrentAmount = 0
                });
            }
            return resultado;
        }

        private static int WeekOfYear(DateTime date)
        {
            return (date.DayOfYear - 1) / 7 + 1;
        }
    }
}
