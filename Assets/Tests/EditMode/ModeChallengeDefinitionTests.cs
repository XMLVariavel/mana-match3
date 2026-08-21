using System;
using NUnit.Framework;

namespace BibleMatch3.Tests
{
    public class ModeChallengeDefinitionTests
    {
        [Test]
        public void DesafioDiario_MesmaDataProduzMesmoSeedEObjetivos()
        {
            DateTime data = new DateTime(2026, 8, 21);
            ModeChallengeDefinition primeiro = ModeChallengeDefinition.Daily(data);
            ModeChallengeDefinition segundo = ModeChallengeDefinition.Daily(data);

            Assert.AreEqual(primeiro.ChallengeId, segundo.ChallengeId);
            Assert.AreEqual(primeiro.Seed, segundo.Seed);
            Assert.AreEqual(primeiro.Objectives.Count, segundo.Objectives.Count);
            Assert.AreEqual(primeiro.Objectives[0].Type, segundo.Objectives[0].Type);
        }

        [Test]
        public void DesafioDiario_DiasDiferentesPodemUsarVariantesDiferentes()
        {
            ModeChallengeDefinition hoje = ModeChallengeDefinition.Daily(new DateTime(2026, 8, 21));
            ModeChallengeDefinition amanha = ModeChallengeDefinition.Daily(new DateTime(2026, 8, 22));

            Assert.AreNotEqual(hoje.ChallengeId, amanha.ChallengeId);
            Assert.AreNotEqual(hoje.Seed, amanha.Seed);
        }

        [Test]
        public void ContraRelogio_UsaDuracaoEIgnoraMovimentos()
        {
            ModeChallengeDefinition desafio = ModeChallengeDefinition.TimeTrial(75f);

            Assert.AreEqual(GameMode.ContraRelogio, desafio.Mode);
            Assert.AreEqual(75f, desafio.DurationSeconds);
            Assert.IsFalse(desafio.UsesMoves);
            Assert.IsTrue(desafio.UsesTimer);
            Assert.IsFalse(desafio.UsesObjectives);
        }

        [Test]
        public void RegrasCompetitivas_AumentamComboEAplicamBonusDeSequencia()
        {
            Assert.Greater(CompetitiveScoreRules.Multiplicador(4, 1f), 1f);
            Assert.AreEqual(50, CompetitiveScoreRules.BonusDeSequencia(5));
            Assert.AreEqual(125, CompetitiveScoreRules.BonusDeSequencia(10));
            Assert.AreEqual(250, CompetitiveScoreRules.BonusDeSequencia(15));
            Assert.AreEqual(125, CompetitiveScoreRules.BonusDeTempoFinal(25f));
        }
    }
}
