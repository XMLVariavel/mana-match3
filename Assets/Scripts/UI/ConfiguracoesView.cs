using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BibleMatch3
{
    /// <summary>
    /// Tela de Configurações: sliders de volume, toggles de som/vibração e o
    /// bloco de LGPD. Os controles são inicializados com
    /// <c>SetValueWithoutNotify</c> para que preencher a tela com o estado
    /// salvo não dispare de volta uma gravação em PlayerPrefs.
    /// </summary>
    public class ConfiguracoesView : MonoBehaviour
    {
        [Header("Origem")]
        [SerializeField] private ConfiguracoesController controller;

        [Header("Som e vibração")]
        [SerializeField] private Slider sliderMusica;
        [SerializeField] private Slider sliderEfeitos;
        [SerializeField] private Toggle toggleMusica;
        [SerializeField] private Toggle toggleEfeitos;
        [SerializeField] private Toggle toggleVibracao;

        [Header("Conta e compras")]
        [SerializeField] private TextMeshProUGUI textoStatusConta;
        [SerializeField] private TextMeshProUGUI textoStatusAnuncios;
        [SerializeField] private Button botaoVincularConta;
        [SerializeField] private Button botaoRemoverAnuncios;

        [Header("LGPD")]
        [SerializeField] private GameObject painelDadosExportados;
        [SerializeField] private TextMeshProUGUI textoDadosExportados;
        [SerializeField] private GameObject painelConfirmarExclusao;
        [SerializeField] private TextMeshProUGUI textoMensagem;

        private void OnEnable()
        {
            if (controller == null) return;

            controller.OnDadosExportados += HandleDadosExportados;
            controller.OnContaExcluida += HandleContaExcluida;
            controller.OnErro += HandleErro;

            SincronizarControles();
            AtualizarStatus();

            if (painelDadosExportados != null) painelDadosExportados.SetActive(false);
            if (painelConfirmarExclusao != null) painelConfirmarExclusao.SetActive(false);
            if (textoMensagem != null) textoMensagem.text = string.Empty;
        }

        private void OnDisable()
        {
            if (controller == null) return;

            controller.OnDadosExportados -= HandleDadosExportados;
            controller.OnContaExcluida -= HandleContaExcluida;
            controller.OnErro -= HandleErro;
        }

        private void SincronizarControles()
        {
            if (sliderMusica != null) sliderMusica.SetValueWithoutNotify(controller.VolumeMusica);
            if (sliderEfeitos != null) sliderEfeitos.SetValueWithoutNotify(controller.VolumeEfeitos);
            if (toggleMusica != null) toggleMusica.SetIsOnWithoutNotify(!controller.MusicaMuda);
            if (toggleEfeitos != null) toggleEfeitos.SetIsOnWithoutNotify(!controller.EfeitosMudos);
            if (toggleVibracao != null) toggleVibracao.SetIsOnWithoutNotify(controller.VibracaoAtiva);
        }

        private void AtualizarStatus()
        {
            bool vinculada = controller.ContaVinculada;
            bool semAnuncios = controller.AnunciosRemovidos;

            if (textoStatusConta != null)
                textoStatusConta.text = vinculada ? "Conta Google vinculada" : "Jogando como convidado";

            if (textoStatusAnuncios != null)
                textoStatusAnuncios.text = semAnuncios ? "Anúncios removidos" : "Anúncios ativos";

            if (botaoVincularConta != null) botaoVincularConta.gameObject.SetActive(!vinculada);
            if (botaoRemoverAnuncios != null) botaoRemoverAnuncios.gameObject.SetActive(!semAnuncios);
        }

        /// <summary>Abre a confirmação de exclusão — ligada ao botão "Excluir minha conta".</summary>
        public void PedirConfirmacaoDeExclusao()
        {
            if (painelConfirmarExclusao != null) painelConfirmarExclusao.SetActive(true);
        }

        /// <summary>Cancela a exclusão — ligada ao botão "Cancelar" do painel de confirmação.</summary>
        public void CancelarExclusao()
        {
            if (painelConfirmarExclusao != null) painelConfirmarExclusao.SetActive(false);
        }

        /// <summary>Fecha o painel com o JSON exportado.</summary>
        public void FecharDadosExportados()
        {
            if (painelDadosExportados != null) painelDadosExportados.SetActive(false);
        }

        private void HandleDadosExportados(string json)
        {
            if (textoDadosExportados != null) textoDadosExportados.text = json;
            if (painelDadosExportados != null) painelDadosExportados.SetActive(true);
        }

        private void HandleContaExcluida()
        {
            if (painelConfirmarExclusao != null) painelConfirmarExclusao.SetActive(false);
        }

        private void HandleErro(string mensagem)
        {
            if (textoMensagem != null) textoMensagem.text = mensagem;
        }
    }
}
