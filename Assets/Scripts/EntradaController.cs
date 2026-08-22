using System;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Fluxo da primeira tela interativa depois da abertura.
    /// O jogador escolhe continuar como convidado ou vincular o progresso ao Google.
    /// As URLs legais ficam configuráveis no Inspector para serem substituídas quando
    /// as páginas oficiais da PalaVivaGames estiverem publicadas.
    /// </summary>
    public class EntradaController : MonoBehaviour
    {
        [SerializeField] private PrivacyManager privacyManager;
        [SerializeField] private FirebaseManager firebaseManager;
        [SerializeField] private ScreenNavigator navigator;
        [SerializeField] private LoginController loginController;
        [SerializeField] private GoogleSignInService googleSignIn;

        [Header("Navegação")]
        [SerializeField] private string telaSeguinte = "Inicio";

        [Header("Páginas legais — substitua quando publicar")]
        [SerializeField] private string termosUrl = "https://palavivagames.com/termos";
        [SerializeField] private string privacidadeUrl = "https://palavivagames.com/privacidade";

        public event Action<string> OnStatus;
        public event Action<bool> OnProcessamento;

        public string TermosUrl => termosUrl;
        public string PrivacidadeUrl => privacidadeUrl;

        private void OnEnable()
        {
            if (loginController != null) loginController.OnVinculoConcluido += HandleVinculo;
            if (loginController != null) loginController.OnErro += HandleErro;
            if (googleSignIn != null) googleSignIn.OnErro += HandleErro;
        }

        private void OnDisable()
        {
            if (loginController != null) loginController.OnVinculoConcluido -= HandleVinculo;
            if (loginController != null) loginController.OnErro -= HandleErro;
            if (googleSignIn != null) googleSignIn.OnErro -= HandleErro;
        }

        /// <summary>Registra o aceite e entra imediatamente em modo convidado.</summary>
        public void JogarComoConvidado()
        {
            RegistrarAceite();
            OnStatus?.Invoke("Preparando seu perfil de convidado...");
            OnProcessamento?.Invoke(true);

            // O Firebase já tenta o login anônimo na inicialização. Esta chamada
            // é idempotente e também cobre o caso de o usuário tocar antes dele terminar.
            firebaseManager?.LoginAnonimo();
            navigator?.Mostrar(telaSeguinte);
        }

        /// <summary>Registra o aceite e inicia o vínculo com a conta Google.</summary>
        public void EntrarComGmail()
        {
            RegistrarAceite();
            if (googleSignIn == null)
            {
                HandleErro("Login com Gmail indisponível nesta build.");
                return;
            }

            OnStatus?.Invoke("Conectando com o Gmail...");
            OnProcessamento?.Invoke(true);
            googleSignIn.EntrarComGoogle();
        }

        public void AbrirTermos()
        {
            AbrirPagina(termosUrl, "Termos de Uso");
        }

        public void AbrirPrivacidade()
        {
            AbrirPagina(privacidadeUrl, "Política de Privacidade");
        }

        private void RegistrarAceite()
        {
            privacyManager?.RegistrarConsentimento(true);
        }

        private void AbrirPagina(string url, string nome)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                OnStatus?.Invoke($"Página de {nome} ainda não configurada.");
                return;
            }

            Application.OpenURL(url);
        }

        private void HandleVinculo(bool sucesso)
        {
            OnProcessamento?.Invoke(false);
            if (!sucesso)
            {
                HandleErro("Não foi possível vincular sua conta Gmail.");
                return;
            }

            OnStatus?.Invoke("Conta Gmail vinculada. Progresso protegido!");
            navigator?.Mostrar(telaSeguinte);
        }

        private void HandleErro(string mensagem)
        {
            OnProcessamento?.Invoke(false);
            OnStatus?.Invoke(mensagem);
        }
    }
}
