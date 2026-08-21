using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Modal de Card de Versículo (Estudo Infinito) — some por cima da Tela
    /// de Jogo quando o GameManager sinaliza que um marco de pontos foi
    /// cruzado. Não pausa o tabuleiro: o card só aparece entre destruições,
    /// nunca no meio de uma animação de troca/queda.
    /// </summary>
    public class VerseCardModalController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject raizDoModal;

        public VerseData VersiculoAtual { get; private set; }

        private void OnEnable()
        {
            if (gameManager != null) gameManager.OnVersiculoExibido += Exibir;
            if (raizDoModal != null) raizDoModal.SetActive(false);
        }

        private void OnDisable()
        {
            if (gameManager != null) gameManager.OnVersiculoExibido -= Exibir;
        }

        private void Exibir(VerseData versiculo)
        {
            VersiculoAtual = versiculo;
            raizDoModal?.SetActive(true);
        }

        /// <summary>Botão "Continuar" do modal.</summary>
        public void Fechar()
        {
            raizDoModal?.SetActive(false);
            VersiculoAtual = null;
        }
    }
}
