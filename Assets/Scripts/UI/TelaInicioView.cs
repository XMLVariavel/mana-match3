using TMPro;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>Apresentação da Tela Início: só escuta o versículo do dia e desenha. Sem regra de negócio.</summary>
    public class TelaInicioView : MonoBehaviour
    {
        [SerializeField] private VersiculoDoDiaService versiculoService;
        [SerializeField] private TextMeshProUGUI textoVersiculo;
        [SerializeField] private TextMeshProUGUI textoReferencia;

        private void OnEnable()
        {
            if (versiculoService != null)
            {
                versiculoService.OnVersiculoPronto += HandleVersiculoPronto;
                versiculoService.CarregarEExibir(); // reconsulta ao reabrir a tela (usa cache do dia se já tiver)
            }
        }

        private void OnDisable()
        {
            if (versiculoService != null) versiculoService.OnVersiculoPronto -= HandleVersiculoPronto;
        }

        private void HandleVersiculoPronto(VersiculoDoDia versiculo)
        {
            if (textoVersiculo != null) textoVersiculo.text = $"\"{versiculo.Texto}\"";
            if (textoReferencia != null) textoReferencia.text = $"- {versiculo.Referencia}";
        }
    }
}
