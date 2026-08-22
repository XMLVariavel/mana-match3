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
        [SerializeField] private TextMeshProUGUI textoStatus;
        [SerializeField] private TextMeshProUGUI textoVersao;

        private void OnEnable()
        {
            if (controller != null)
            {
                controller.OnStatus += HandleStatus;
                controller.OnProcessamento += HandleProcessamento;
            }

            if (textoStatus != null) textoStatus.text = string.Empty;
            if (textoVersao != null) textoVersao.text = "Versão 1.0.0 | 2026 PalaVivaGames";
            HandleProcessamento(false);
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.OnStatus -= HandleStatus;
                controller.OnProcessamento -= HandleProcessamento;
            }
        }

        private void HandleStatus(string mensagem)
        {
            if (textoStatus != null) textoStatus.text = mensagem ?? string.Empty;
        }

        private void HandleProcessamento(bool processando)
        {
            if (botaoGmail != null) botaoGmail.interactable = !processando;
            if (botaoConvidado != null) botaoConvidado.interactable = !processando;
        }
    }
}
