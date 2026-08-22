using System.Collections;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Primeira tela do app: pede consentimento LGPD (só se ainda não foi
    /// registrado neste aparelho) antes de qualquer login, e navega para a
    /// próxima tela quando tudo estiver resolvido. Não decide layout nem
    /// texto — só a lógica de fluxo entre consentimento, login e navegação.
    /// </summary>
    public class SplashOnboardingController : MonoBehaviour
    {
        [SerializeField] private PrivacyManager privacyManager;
        [SerializeField] private FirebaseManager firebaseManager;
        [SerializeField] private ScreenNavigator navigator;

        [Header("Nomes das telas (registradas no ScreenNavigator)")]
        [SerializeField] private string telaConsentimento = "TelaConsentimento";
        [SerializeField] private string telaCarregando = "TelaCarregando";
        [SerializeField] private string telaSeguinte = "MapaDeFases";
        [SerializeField, Min(0f)] private float duracaoDaAbertura = 5f;

        private bool aberturaConcluida;
        private bool loginPronto;

        private void OnEnable()
        {
            if (firebaseManager != null) firebaseManager.OnLoginPronto += HandleLoginPronto;
        }

        private void OnDisable()
        {
            if (firebaseManager != null) firebaseManager.OnLoginPronto -= HandleLoginPronto;
        }

        private void Start()
        {
            navigator?.Mostrar("Splash");
            StartCoroutine(AguardarAbertura());
        }

        private IEnumerator AguardarAbertura()
        {
            yield return new WaitForSecondsRealtime(duracaoDaAbertura);
            aberturaConcluida = true;

            // A tela de entrada reúne a escolha do jogador, o aceite legal
            // e os dois caminhos de autenticação. Assim, o usuário vê
            // Convidado/Gmail logo após a abertura, inclusive em aparelhos novos.
            navigator?.Mostrar(telaSeguinte);
        }

        /// <summary>Chamado pelo botão "Aceitar" da tela de consentimento.</summary>
        public void AceitarConsentimento()
        {
            privacyManager.RegistrarConsentimento(true);
            navigator?.Mostrar(telaCarregando);
            firebaseManager?.LoginAnonimo();
        }

        /// <summary>
        /// Chamado pelo botão "Recusar". O jogo não força aceite para
        /// funcionar: sem consentimento, simplesmente não fazemos login nem
        /// sincronizamos nada com o Firebase — o jogador segue para o jogo
        /// numa sessão local, sem progresso salvo na nuvem. O texto que
        /// explica essa consequência ao jogador é responsabilidade de quem
        /// monta a tela (ver LGPD_Checklist.md).
        /// </summary>
        public void RecusarConsentimento()
        {
            privacyManager.RegistrarConsentimento(false);
            navigator?.Mostrar(telaSeguinte);
        }

        private void HandleLoginPronto()
        {
            loginPronto = true;
            // O login pode concluir antes ou depois do vídeo; a Entrada só
            // avança quando o jogador escolhe um caminho.
        }
    }
}
