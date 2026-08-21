using System;
using System.Collections;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Intensidade desejada do feedback tátil. É uma *intenção*, não um
    /// parâmetro real: <see cref="Handheld.Vibrate"/> não aceita duração nem
    /// amplitude, então o que muda entre os níveis é quantos pulsos são
    /// disparados e quais níveis o jogador deixou ligados.
    /// </summary>
    public enum IntensidadeHaptica
    {
        Leve,   // match comum
        Media,  // peça especial criada
        Forte   // combo/cruzamento de especiais
    }

    /// <summary>
    /// Vibração do aparelho. Deliberadamente simples: só um liga/desliga
    /// persistido em PlayerPrefs e <see cref="Handheld.Vibrate"/> por baixo.
    ///
    /// Limitação conhecida e aceita nesta fase: no Android, Handheld.Vibrate()
    /// dispara sempre a mesma vibração fixa (~500ms), sem controle de duração
    /// ou amplitude. Para haptics finos (tap leve x impacto forte) seria
    /// preciso um plugin nativo — quando isso existir, só o corpo de
    /// <see cref="Vibrar"/> precisa mudar; nada mais no jogo conhece a API.
    /// </summary>
    public class HapticsManager : MonoBehaviour
    {
        private const string ChaveVibracaoAtiva = "BibleMatch3_VibracaoAtiva";

        public static HapticsManager Instance { get; private set; }

        [Header("Anti-spam")]
        [Tooltip("Intervalo mínimo entre duas vibrações. Sem isso, uma cascata longa vibraria sem parar.")]
        [SerializeField] private float intervaloMinimo = 0.35f;

        [Tooltip("Vibração leve (match comum) tende a incomodar em sessões longas — desligada por padrão mesmo com a vibração ativa.")]
        [SerializeField] private bool vibrarEmMatchComum;

        public bool VibracaoAtiva { get; private set; }

        public event Action<bool> OnVibracaoAlterada;

        private float ultimaVibracao = float.NegativeInfinity;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            VibracaoAtiva = PlayerPrefs.GetInt(ChaveVibracaoAtiva, 1) == 1;
        }

        /// <summary>Toggle liga/desliga da tela de Configurações.</summary>
        public void DefinirVibracaoAtiva(bool ativa)
        {
            VibracaoAtiva = ativa;
            PlayerPrefs.SetInt(ChaveVibracaoAtiva, ativa ? 1 : 0);
            PlayerPrefs.Save();
            OnVibracaoAlterada?.Invoke(ativa);

            // Confirmação tátil imediata de que a opção foi ligada.
            if (ativa) Vibrar(IntensidadeHaptica.Media, ignorarIntervalo: true);
        }

        public void Vibrar(IntensidadeHaptica intensidade, bool ignorarIntervalo = false)
        {
            if (!VibracaoAtiva) return;
            if (intensidade == IntensidadeHaptica.Leve && !vibrarEmMatchComum) return;

            // Handheld.Vibrate() compila em todas as plataformas mas só faz algo
            // em aparelho — no Editor/PC seria só um no-op silencioso.
            if (!Application.isMobilePlatform) return;

            if (!ignorarIntervalo && Time.unscaledTime - ultimaVibracao < intervaloMinimo) return;
            ultimaVibracao = Time.unscaledTime;

            if (intensidade == IntensidadeHaptica.Forte)
            {
                StartCoroutine(PulsoDuplo());
                return;
            }

            Handheld.Vibrate();
        }

        /// <summary>
        /// Substituto pobre de "vibração forte": dois pulsos curtos seguidos.
        /// É o máximo que dá para diferenciar sem plugin nativo.
        /// </summary>
        private IEnumerator PulsoDuplo()
        {
            Handheld.Vibrate();
            yield return new WaitForSecondsRealtime(0.12f);
            Handheld.Vibrate();
        }
    }
}
