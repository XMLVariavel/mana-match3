using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Desenha o catálogo da Loja a partir do <see cref="LojaController"/>:
    /// especiais de tabuleiro (evoluíveis) e avulsos (informativos). Remonta a
    /// lista sempre que um poder evolui, para o nível/custo exibidos nunca
    /// ficarem desatualizados em relação ao asset.
    /// </summary>
    public class LojaView : MonoBehaviour
    {
        [Header("Origem")]
        [SerializeField] private LojaController controller;

        [Header("Listas")]
        [SerializeField] private Transform containerEspeciais;
        [SerializeField] private Transform containerAvulsos;
        [SerializeField] private ItemLojaUI itemPrefab;

        [Header("Textos")]
        [SerializeField] private TextMeshProUGUI textoMoedas;
        [SerializeField] private TextMeshProUGUI textoMensagem;
        [SerializeField] private TextMeshProUGUI textoStatusAnuncios;

        [SerializeField] private string mensagemMoedaInsuficiente = "Moedas insuficientes para evoluir este poder.";
        [SerializeField] private string mensagemEvoluiu = "Poder evoluído!";

        private readonly List<ItemLojaUI> itens = new List<ItemLojaUI>();

        private void OnEnable()
        {
            if (controller == null) return;

            controller.OnMoedasAtualizadas += HandleMoedas;
            controller.OnPoderEvoluido += HandlePoderEvoluido;
            controller.OnAvulsoComprado += HandleAvulsoComprado;
            controller.OnEstoqueAtualizado += HandleEstoqueAtualizado;
            controller.OnMensagem += HandleMensagem;
            controller.OnMoedaInsuficiente += HandleMoedaInsuficiente;

            HandleMoedas(controller.MoedasAtuais);
            AtualizarStatusAnuncios();
            LimparMensagem();
            Montar();
        }

        private void OnDisable()
        {
            if (controller == null) return;

            controller.OnMoedasAtualizadas -= HandleMoedas;
            controller.OnPoderEvoluido -= HandlePoderEvoluido;
            controller.OnAvulsoComprado -= HandleAvulsoComprado;
            controller.OnEstoqueAtualizado -= HandleEstoqueAtualizado;
            controller.OnMensagem -= HandleMensagem;
            controller.OnMoedaInsuficiente -= HandleMoedaInsuficiente;

            Limpar();
        }

        private void Montar()
        {
            Limpar();
            if (itemPrefab == null) return;

            MontarLista(controller.EspeciaisDeTabuleiro, containerEspeciais, evoluivel: true);
            MontarLista(controller.Avulsos, containerAvulsos, evoluivel: false);
        }

        private void MontarLista(IReadOnlyList<PowerUpConfig> configs, Transform container, bool evoluivel)
        {
            if (configs == null || container == null) return;

            foreach (PowerUpConfig config in configs)
            {
                if (config == null) continue;

                ItemLojaUI item = Instantiate(itemPrefab, container);
                PowerUpConfig capturado = config; // evita capturar a variável do laço
                System.Action acao = evoluivel
                    ? () => controller.TentarEvoluir(capturado)
                    : () => controller.ComprarAvulso(capturado);
                item.Configurar(capturado, acao, controller.EstoqueDe(capturado));
                itens.Add(item);
            }
        }

        private void Limpar()
        {
            foreach (ItemLojaUI item in itens)
                if (item != null) Destroy(item.gameObject);

            itens.Clear();
        }

        private void HandleMoedas(int moedas)
        {
            if (textoMoedas != null) textoMoedas.text = moedas.ToString();
        }

        private void HandlePoderEvoluido(PowerUpConfig config)
        {
            if (textoMensagem != null) textoMensagem.text = mensagemEvoluiu;
            Montar(); // nível/custo mudaram — remonta para refletir o novo estado
        }

        private void HandleAvulsoComprado(PowerUpConfig config)
        {
            if (textoMensagem != null) textoMensagem.text = $"{config.NomeExibicao} comprado.";
            Montar();
        }

        private void HandleEstoqueAtualizado() => Montar();

        private void HandleMensagem(string mensagem)
        {
            if (textoMensagem != null) textoMensagem.text = mensagem;
        }

        private void HandleMoedaInsuficiente()
        {
            if (textoMensagem != null) textoMensagem.text = mensagemMoedaInsuficiente;
        }

        private void LimparMensagem()
        {
            if (textoMensagem != null) textoMensagem.text = string.Empty;
        }

        private void AtualizarStatusAnuncios()
        {
            if (textoStatusAnuncios == null) return;

            textoStatusAnuncios.text = controller.AnunciosRemovidos
                ? "Anúncios já removidos nesta conta."
                : "Remover Anúncios — compra única";
        }
    }
}
