using System;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Esqueleto TÉCNICO de conformidade com a LGPD: registra a decisão de
    /// consentimento, expõe exportação e exclusão de dados. Este script não
    /// decide o texto da tela de consentimento nem da Política de
    /// Privacidade — isso é conteúdo, e conteúdo jurídico precisa de revisão
    /// por advogado antes do lançamento (ver docs/LGPD_Checklist.md).
    /// </summary>
    public class PrivacyManager : MonoBehaviour
    {
        [SerializeField] private FirebaseManager firebaseManager;

        private const string ChaveConsentimentoLocal = "BibleMatch3_ConsentimentoLGPD";

        /// <summary>Se já existe alguma decisão registrada neste aparelho (aceite ou recusa).</summary>
        public bool ConsentimentoJaRegistrado => PlayerPrefs.HasKey(ChaveConsentimentoLocal);
        public bool ConsentimentoAceito => PlayerPrefs.GetInt(ChaveConsentimentoLocal, 0) == 1;

        public event Action<bool> OnConsentimentoAtualizado; // true = aceitou
        public event Action<string> OnDadosExportados;       // JSON pronto para compartilhar/salvar
        public event Action OnDadosExcluidos;
        public event Action<string> OnErro;

        /// <summary>
        /// Chamado pela tela de onboarding com a decisão do jogador — este
        /// método só grava o resultado, a UI que apresenta a decisão de
        /// forma clara e ANTES de qualquer coleta é responsabilidade de quem
        /// constrói a tela (ver seção "UI completa" do roadmap).
        /// </summary>
        public void RegistrarConsentimento(bool aceitou)
        {
            PlayerPrefs.SetInt(ChaveConsentimentoLocal, aceitou ? 1 : 0);
            PlayerPrefs.Save();

            if (firebaseManager != null && firebaseManager.UsuarioLogado)
                firebaseManager.AtualizarConsentimentoLgpd(aceitou);

            OnConsentimentoAtualizado?.Invoke(aceitou);
        }

        /// <summary>
        /// Direito de acesso: monta um JSON com todos os dados do jogador
        /// para a tela "Meus Dados" oferecer como download/compartilhamento.
        /// </summary>
        public void ExportarMeusDados()
        {
            if (firebaseManager == null || !firebaseManager.UsuarioLogado)
            {
                OnErro?.Invoke("Você precisa estar conectado para exportar seus dados.");
                return;
            }

            void HandleCarregado(PlayerProgress progresso)
            {
                firebaseManager.OnProgressoCarregado -= HandleCarregado;
                string json = JsonUtility.ToJson(progresso, true);
                OnDadosExportados?.Invoke(json);
            }

            firebaseManager.OnProgressoCarregado += HandleCarregado;
            firebaseManager.CarregarProgresso();
        }

        /// <summary>
        /// Direito de eliminação: apaga a conta e todos os dados associados.
        /// Ação irreversível — a confirmação ("tem certeza?") é
        /// responsabilidade da UI, não deste método.
        /// </summary>
        public void ExcluirMinhaContaEDados()
        {
            if (firebaseManager == null || !firebaseManager.UsuarioLogado)
            {
                OnErro?.Invoke("Você precisa estar conectado para excluir sua conta.");
                return;
            }

            firebaseManager.ExcluirContaEDados(sucesso =>
            {
                if (sucesso)
                {
                    PlayerPrefs.DeleteKey(ChaveConsentimentoLocal);
                    OnDadosExcluidos?.Invoke();
                }
                else
                {
                    OnErro?.Invoke("Não foi possível excluir sua conta agora. Tente novamente.");
                }
            });
        }
    }
}
