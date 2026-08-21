using TMPro;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Cabeçalho de recursos (vidas, moedas e tempo até a próxima vida),
    /// reaproveitado pelo Mapa de Fases e pela Loja. O LivesManager não roda
    /// timer próprio por decisão de bateria, então o contador regressivo é
    /// puxado aqui, uma vez por segundo, só enquanto a tela está visível.
    /// </summary>
    public class RecursosView : MonoBehaviour
    {
        [SerializeField] private LivesManager livesManager;
        [SerializeField] private BoosterManager boosterManager;

        [SerializeField] private TextMeshProUGUI textoVidas;
        [SerializeField] private TextMeshProUGUI textoMoedas;
        [SerializeField] private TextMeshProUGUI textoProximaVida;

        [SerializeField] private float intervaloDeAtualizacao = 1f;

        private float proximaAtualizacao;

        private void OnEnable()
        {
            if (livesManager != null) livesManager.OnVidasChanged += HandleVidas;
            if (boosterManager != null) boosterManager.OnMoedasChanged += HandleMoedas;

            if (livesManager != null) HandleVidas(livesManager.VidasAtuais);
            if (boosterManager != null) HandleMoedas(boosterManager.Moedas);

            proximaAtualizacao = 0f;
        }

        private void OnDisable()
        {
            if (livesManager != null) livesManager.OnVidasChanged -= HandleVidas;
            if (boosterManager != null) boosterManager.OnMoedasChanged -= HandleMoedas;
        }

        private void Update()
        {
            if (livesManager == null || textoProximaVida == null) return;
            if (Time.unscaledTime < proximaAtualizacao) return;

            proximaAtualizacao = Time.unscaledTime + intervaloDeAtualizacao;

            System.TimeSpan restante = livesManager.TempoAteProximaVida();
            textoProximaVida.text = restante <= System.TimeSpan.Zero
                ? "Cheio"
                : $"{(int)restante.TotalMinutes:00}:{restante.Seconds:00}";
        }

        private void HandleVidas(int vidas)
        {
            if (textoVidas != null) textoVidas.text = vidas.ToString();
        }

        private void HandleMoedas(int moedas)
        {
            if (textoMoedas != null) textoMoedas.text = moedas.ToString();
        }
    }
}
