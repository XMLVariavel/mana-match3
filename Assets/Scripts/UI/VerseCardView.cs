using TMPro;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Preenche o card de versículo. Escuta o <see cref="GameManager"/>
    /// diretamente (mesma fonte que o <see cref="VerseCardModalController"/>
    /// usa para abrir o modal), de propósito: assim a view não precisa que o
    /// controller do modal ganhe API nova só para expor o texto.
    ///
    /// Deve viver num GameObject que fique ativo enquanto a Tela de Jogo está
    /// ativa — normalmente o mesmo do VerseCardModalController, fora da raiz
    /// do modal, que é desligada.
    /// </summary>
    public class VerseCardView : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        [SerializeField] private TextMeshProUGUI textoVersiculo;
        [SerializeField] private TextMeshProUGUI textoReferencia;
        [SerializeField] private TextMeshProUGUI textoReflexao;

        private void OnEnable()
        {
            if (gameManager != null) gameManager.OnVersiculoExibido += HandleVersiculo;
        }

        private void OnDisable()
        {
            if (gameManager != null) gameManager.OnVersiculoExibido -= HandleVersiculo;
        }

        private void HandleVersiculo(VerseData versiculo)
        {
            if (versiculo == null) return;

            if (textoVersiculo != null) textoVersiculo.text = $"“{versiculo.Texto}”";
            if (textoReferencia != null) textoReferencia.text = versiculo.Referencia;
            if (textoReflexao != null) textoReflexao.text = versiculo.Reflexao;
        }
    }
}
