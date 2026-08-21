using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Regras determinísticas de pontuação competitiva. A Campanha, o Diário e
    /// o Guardião continuam usando a pontuação base; o multiplicador é aplicado
    /// somente pelo BoardPhysics quando o modo é Contra o Relógio.
    /// </summary>
    public static class CompetitiveScoreRules
    {
        public const int PontosSequencia5 = 50;
        public const int PontosSequencia10 = 125;
        public const int PontosSequencia15 = 250;

        public static float Multiplicador(int combo, float segundosDesdeJogada)
        {
            float multiplicadorCombo = Mathf.Min(1f + Mathf.Max(0, combo - 1) * 0.25f, 3f);
            float multiplicadorVelocidade = segundosDesdeJogada <= 1f
                ? 1.50f
                : segundosDesdeJogada <= 2f ? 1.20f : 1f;
            return multiplicadorCombo * multiplicadorVelocidade;
        }

        public static int BonusDeSequencia(int combo)
        {
            if (combo > 0 && combo % 15 == 0) return PontosSequencia15;
            if (combo > 0 && combo % 10 == 0) return PontosSequencia10;
            if (combo > 0 && combo % 5 == 0) return PontosSequencia5;
            return 0;
        }

        public static int BonusDeTempoFinal(float segundosRestantes)
        {
            return Mathf.Clamp(Mathf.FloorToInt(Mathf.Max(0f, segundosRestantes) / 5f) * 25, 0, 500);
        }
    }
}
