using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BibleMatch3
{
    /// <summary>
    /// Tela de Login/vínculo de conta. O botão "Entrar com Google" fala com o
    /// <see cref="GoogleSignInService"/> (que obtém o idToken junto ao SDK) e
    /// esta view apenas reflete sucesso/erro que o
    /// <see cref="LoginController"/> devolve.
    /// </summary>
    public class LoginView : MonoBehaviour
    {
        [Header("Origem")]
        [SerializeField] private LoginController controller;
        [SerializeField] private GoogleSignInService googleSignIn;

        [Header("UI")]
        [SerializeField] private Button botaoGoogle;
        [SerializeField] private TextMeshProUGUI textoStatus;
        [SerializeField] private TextMeshProUGUI textoMensagem;

        [SerializeField] private string mensagemEntrando = "Conectando com o Google...";
        [SerializeField] private string mensagemSucesso = "Conta vinculada! Seu progresso agora está protegido.";
        [SerializeField] private string mensagemFalha = "Não foi possível vincular a conta.";

        private void OnEnable()
        {
            if (controller != null)
            {
                controller.OnVinculoConcluido += HandleVinculo;
                controller.OnErro += HandleErro;
            }

            if (googleSignIn != null) googleSignIn.OnErro += HandleErro;

            AtualizarStatus();
            if (textoMensagem != null) textoMensagem.text = string.Empty;
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.OnVinculoConcluido -= HandleVinculo;
                controller.OnErro -= HandleErro;
            }

            if (googleSignIn != null) googleSignIn.OnErro -= HandleErro;
        }

        /// <summary>Ligado ao botão "Entrar com Google" pelo montador do Editor.</summary>
        public void EntrarComGoogle()
        {
            if (googleSignIn == null)
            {
                HandleErro("Google Sign-In não está configurado nesta build.");
                return;
            }

            if (botaoGoogle != null) botaoGoogle.interactable = false;
            if (textoMensagem != null) textoMensagem.text = mensagemEntrando;

            googleSignIn.EntrarComGoogle();
        }

        private void HandleVinculo(bool sucesso)
        {
            if (botaoGoogle != null) botaoGoogle.interactable = true;
            if (textoMensagem != null) textoMensagem.text = sucesso ? mensagemSucesso : mensagemFalha;
            AtualizarStatus();
        }

        private void HandleErro(string mensagem)
        {
            if (botaoGoogle != null) botaoGoogle.interactable = true;
            if (textoMensagem != null) textoMensagem.text = mensagem;
        }

        private void AtualizarStatus()
        {
            bool vinculada = controller != null && controller.ContaVinculada;

            if (textoStatus != null)
                textoStatus.text = vinculada
                    ? "Conta Google vinculada"
                    : "Você está jogando como convidado. Vincule uma conta para não perder o progresso.";

            if (botaoGoogle != null) botaoGoogle.gameObject.SetActive(!vinculada);
        }
    }
}
