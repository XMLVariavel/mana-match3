using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BibleMatch3
{
    /// <summary>Apresentação da tela de entrada; não contém regra de autenticação.</summary>
    public class EntradaView : MonoBehaviour
    {
        [SerializeField] private EntradaController controller;
        [SerializeField] private Button botaoGmail;
        [SerializeField] private Button botaoConvidado;
        [SerializeField] private Toggle toggleAceite;
        [SerializeField] private TextMeshProUGUI textoStatus;
        [SerializeField] private TextMeshProUGUI textoVersao;

        private bool processando;

        private void OnEnable()
        {
            if (controller != null)
            {
                controller.OnStatus += HandleStatus;
                controller.OnProcessamento += HandleProcessamento;
            }

            if (toggleAceite != null)
            {
                toggleAceite.isOn = false; // nunca começa pré-marcado
                toggleAceite.onValueChanged.AddListener(HandleToggleAceite);
            }

            if (textoStatus != null) textoStatus.text = string.Empty;
            if (textoVersao != null) textoVersao.text = "Versão 1.0.0 | 2026 PalaVivaGames";
            processando = false;
            AtualizarBotoes();
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.OnStatus -= HandleStatus;
                controller.OnProcessamento -= HandleProcessamento;
            }

            if (toggleAceite != null) toggleAceite.onValueChanged.RemoveListener(HandleToggleAceite);
        }

        private void HandleStatus(string mensagem)
        {
            if (textoStatus != null) textoStatus.text = mensagem ?? string.Empty;
        }

        private void HandleProcessamento(bool novoProcessando)
        {
            processando = novoProcessando;
            AtualizarBotoes();
        }

        private void HandleToggleAceite(bool _)
        {
            AtualizarBotoes();
        }

        /// <summary>
        /// Os botões só ficam clicáveis quando o jogador marcou o aceite dos
        /// Termos/Política de Privacidade (obrigatório) e nada está em
        /// processamento. Sem o Toggle atribuído, mantém o comportamento
        /// anterior (não trava um projeto que ainda não montou a caixinha).
        /// </summary>
        private void AtualizarBotoes()
        {
            bool aceitou = toggleAceite == null || toggleAceite.isOn;
            bool podeClicar = !processando && aceitou;

            if (botaoGmail != null) botaoGmail.interactable = podeClicar;
            if (botaoConvidado != null) botaoConvidado.interactable = podeClicar;
        }
    }
}
