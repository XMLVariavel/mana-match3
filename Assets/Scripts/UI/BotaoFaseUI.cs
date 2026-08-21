using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BibleMatch3
{
    /// <summary>
    /// Implementação de UI do contrato <see cref="BotaoFasePrefab"/>: um botão
    /// da trilha do Mapa de Fases, com número, cadeado quando travada e até
    /// três estrelas preenchidas conforme o resultado já obtido.
    ///
    /// O prefab correspondente vive em Assets/Prefabs/BotaoFase.prefab e é
    /// gerado pelo montador do Editor (Tools/Maná).
    /// </summary>
    public class BotaoFaseUI : BotaoFasePrefab
    {
        [SerializeField] private Button botao;
        [SerializeField] private TextMeshProUGUI textoNumero;
        [SerializeField] private GameObject iconeCadeado;
        [SerializeField] private Image[] estrelas = new Image[3];

        [Header("Cores")]
        [SerializeField] private Color corLiberada = new Color(0.98f, 0.85f, 0.45f);
        [SerializeField] private Color corTravada = new Color(0.55f, 0.55f, 0.58f);
        [SerializeField] private Color corEstrelaCheia = new Color(1f, 0.82f, 0.25f);
        [SerializeField] private Color corEstrelaVazia = new Color(0.35f, 0.35f, 0.38f);

        public override void Configurar(LevelData fase, bool liberada, int estrelasObtidas, Action aoClicar)
        {
            if (textoNumero != null)
                textoNumero.text = fase != null ? fase.Numero.ToString() : "?";

            if (iconeCadeado != null)
                iconeCadeado.SetActive(!liberada);

            if (botao != null)
            {
                botao.interactable = liberada;

                // O prefab é instanciado a cada remontagem da trilha, mas
                // limpamos mesmo assim: o MapaDeFasesController pode reconfigurar
                // o mesmo botão quando o progresso chega do Firestore.
                botao.onClick.RemoveAllListeners();
                if (liberada && aoClicar != null) botao.onClick.AddListener(() => aoClicar());

                var imagem = botao.GetComponent<Image>();
                if (imagem != null) imagem.color = liberada ? corLiberada : corTravada;
            }

            for (int i = 0; i < estrelas.Length; i++)
            {
                if (estrelas[i] == null) continue;
                estrelas[i].color = i < estrelasObtidas ? corEstrelaCheia : corEstrelaVazia;
            }

            gameObject.name = fase != null ? $"BotaoFase_{fase.Numero}" : "BotaoFase";
        }
    }
}
