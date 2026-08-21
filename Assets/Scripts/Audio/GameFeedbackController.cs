using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Ponte entre o que acontece no tabuleiro e o feedback sensorial
    /// (som + vibração). Existe para que nem o BoardPhysics nem o
    /// ScoreAndObjectiveManager precisem conhecer AudioManager/HapticsManager —
    /// eles só disparam eventos, e este componente decide o que isso "soa" e
    /// "sente". Trocar a regra de feedback é mexer só aqui.
    /// </summary>
    public class GameFeedbackController : MonoBehaviour
    {
        [Header("Origem dos eventos")]
        [SerializeField] private BoardPhysics boardPhysics;
        [SerializeField] private ScoreAndObjectiveManager scoreManager;

        [Header("Saída")]
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private HapticsManager hapticsManager;

        [Header("Regras")]
        [Tooltip("A partir de quantas peças numa única passada o match conta como 'combo' (feedback forte).")]
        [SerializeField] private int pecasParaCombo = 6;

        private AudioManager Audio => audioManager != null ? audioManager : AudioManager.Instance;
        private HapticsManager Haptics => hapticsManager != null ? hapticsManager : HapticsManager.Instance;

        private void OnEnable()
        {
            if (boardPhysics != null)
            {
                boardPhysics.OnMatchDestruido += HandleMatchDestruido;
                boardPhysics.OnEspecialCriado += HandleEspecialCriado;
                boardPhysics.OnCascataAvancou += HandleCascataAvancou;
            }

            if (scoreManager != null)
            {
                scoreManager.OnWin += HandleWin;
                scoreManager.OnLose += HandleLose;
            }
        }

        private void OnDisable()
        {
            if (boardPhysics != null)
            {
                boardPhysics.OnMatchDestruido -= HandleMatchDestruido;
                boardPhysics.OnEspecialCriado -= HandleEspecialCriado;
                boardPhysics.OnCascataAvancou -= HandleCascataAvancou;
            }

            if (scoreManager != null)
            {
                scoreManager.OnWin -= HandleWin;
                scoreManager.OnLose -= HandleLose;
            }
        }

        private void HandleMatchDestruido(int quantidade)
        {
            bool combo = quantidade >= pecasParaCombo;

            Audio?.TocarEfeito(combo ? EfeitoSonoro.ComboEspecial : EfeitoSonoro.Match);
            Haptics?.Vibrar(combo ? IntensidadeHaptica.Forte : IntensidadeHaptica.Leve);
        }

        private void HandleEspecialCriado(SpecialType tipo)
        {
            Audio?.TocarEfeito(EfeitoSonoro.EspecialCriado);
            Haptics?.Vibrar(IntensidadeHaptica.Media);
        }

        private void HandleCascataAvancou(int passada)
        {
            // A cascata em si já dispara OnMatchDestruido a cada passada; aqui
            // só reforçamos o tátil quando o encadeamento fica realmente longo,
            // para não virar vibração contínua.
            if (passada >= 3) Haptics?.Vibrar(IntensidadeHaptica.Forte);
        }

        private void HandleWin(int estrelas) => Audio?.TocarEfeito(EfeitoSonoro.Vitoria);
        private void HandleLose() => Audio?.TocarEfeito(EfeitoSonoro.Derrota);
    }
}
