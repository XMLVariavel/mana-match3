using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BibleMatch3
{
    /// <summary>
    /// Uma entrada da Loja: um poder, seu nível/custo e o botão de evoluir.
    /// Poderes avulsos aparecem só como informação (eles são usados ao vivo
    /// na Tela de Jogo, não comprados como estoque) — nesse caso o botão some.
    /// </summary>
    public class ItemLojaUI : MonoBehaviour
    {
        [SerializeField] private Image icone;
        [SerializeField] private TextMeshProUGUI textoNome;
        [SerializeField] private TextMeshProUGUI textoDescricao;
        [SerializeField] private TextMeshProUGUI textoNivel;
        [SerializeField] private Button botaoAcao;
        [SerializeField] private TextMeshProUGUI textoBotao;

        public PowerUpConfig Config { get; private set; }

        public void Configurar(PowerUpConfig config, Action aoClicar, int estoqueAtual = 0)
        {
            Config = config;
            if (config == null) return;

            if (icone != null)
            {
                icone.sprite = config.Icone;
                icone.enabled = config.Icone != null;
            }

            if (textoNome != null)
                textoNome.text = string.IsNullOrEmpty(config.NomeExibicao) ? config.name : config.NomeExibicao;

            if (textoDescricao != null) textoDescricao.text = config.Descricao;

            bool evoluivel = config.Tipo == TipoPoder.EspecialDeTabuleiro;
            bool avulso = config.Tipo == TipoPoder.Avulso;

            if (textoNivel != null)
                textoNivel.text = evoluivel
                    ? $"Nv. {config.NivelAtual}/{config.NivelMaximo}"
                    : $"Estoque: {estoqueAtual}  •  pacote +{config.QuantidadePorCompra}";

            if (botaoAcao != null)
            {
                botaoAcao.gameObject.SetActive(evoluivel || avulso);
                botaoAcao.onClick.RemoveAllListeners();

                if (evoluivel || avulso)
                {
                    botaoAcao.interactable = evoluivel ? config.PodeEvoluir : aoClicar != null;
                    if (aoClicar != null) botaoAcao.onClick.AddListener(() => aoClicar());

                    if (textoBotao != null)
                        textoBotao.text = evoluivel
                            ? (config.PodeEvoluir ? $"Evoluir · {config.CustoEvolucaoProximoNivel}" : "Nível máximo")
                            : $"Comprar +{config.QuantidadePorCompra} · {config.CustoMoedas}";
                }
            }

            gameObject.name = $"ItemLoja_{config.name}";
        }
    }
}
