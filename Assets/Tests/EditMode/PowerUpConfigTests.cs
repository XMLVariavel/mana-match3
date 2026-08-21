using NUnit.Framework;
using UnityEngine;

namespace BibleMatch3.Tests
{
    public class PowerUpConfigTests
    {
        private PowerUpConfig config;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<PowerUpConfig>();
            config.Tipo = TipoPoder.EspecialDeTabuleiro;
            config.NivelAtual = 1;
            config.NivelMaximo = 3;
            config.CustoEvolucaoPorNivel = new[] { 100, 250 };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
        }

        [Test]
        public void PodeEvoluir_VerdadeiroAbaixoDoNivelMaximo()
        {
            Assert.IsTrue(config.PodeEvoluir);
        }

        [Test]
        public void Evoluir_IncrementaNivelAteOMaximoENaoUltrapassa()
        {
            config.Evoluir(); // 1 -> 2
            Assert.AreEqual(2, config.NivelAtual);

            config.Evoluir(); // 2 -> 3
            Assert.AreEqual(3, config.NivelAtual);
            Assert.IsFalse(config.PodeEvoluir);

            config.Evoluir(); // já no máximo — não deve mudar
            Assert.AreEqual(3, config.NivelAtual);
        }

        [Test]
        public void CustoEvolucaoProximoNivel_UsaOIndiceCorreto()
        {
            Assert.AreEqual(100, config.CustoEvolucaoProximoNivel); // nível 1 -> 2

            config.Evoluir();
            Assert.AreEqual(250, config.CustoEvolucaoProximoNivel); // nível 2 -> 3
        }

        [Test]
        public void PoderAvulso_NuncaPodeEvoluir()
        {
            config.Tipo = TipoPoder.Avulso;
            Assert.IsFalse(config.PodeEvoluir);
        }
    }
}
