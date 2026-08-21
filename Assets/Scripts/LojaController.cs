using System;
using System.Collections.Generic;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Tela Loja/Poderes: catálogo de especiais de tabuleiro (evoluíveis com
    /// moeda ganha jogando) e dos avulsos (aqui só informativos — são usados
    /// ao vivo na Tela de Jogo, não comprados como estoque). Também expõe o
    /// atalho para a única compra real do jogo, "Remover Anúncios".
    /// </summary>
    public class LojaController : MonoBehaviour
    {
        [SerializeField] private List<PowerUpConfig> especiaisDeTabuleiro;
        [SerializeField] private List<PowerUpConfig> avulsos;
        [SerializeField] private BoosterManager boosterManager;
        [SerializeField] private PurchaseManager purchaseManager;

        public IReadOnlyList<PowerUpConfig> EspeciaisDeTabuleiro => especiaisDeTabuleiro;
        public IReadOnlyList<PowerUpConfig> Avulsos => avulsos;
        public int MoedasAtuais => boosterManager != null ? boosterManager.Moedas : 0;
        public bool AnunciosRemovidos => purchaseManager != null && purchaseManager.AnunciosRemovidos;

        public event Action<int> OnMoedasAtualizadas;
        public event Action<PowerUpConfig> OnPoderEvoluido;
        public event Action<PowerUpConfig> OnAvulsoComprado;
        public event Action OnEstoqueAtualizado;
        public event Action OnMoedaInsuficiente;
        public event Action<string> OnMensagem;

        private void OnEnable()
        {
            if (boosterManager != null)
            {
                boosterManager.OnMoedasChanged += HandleMoedasChanged;
                boosterManager.OnEstoqueChanged += HandleEstoqueChanged;
            }
            if (purchaseManager != null)
            {
                purchaseManager.OnCompraConcluida += HandleCompraConcluida;
                purchaseManager.OnErro += HandleErroCompra;
            }
        }

        private void OnDisable()
        {
            if (boosterManager != null)
            {
                boosterManager.OnMoedasChanged -= HandleMoedasChanged;
                boosterManager.OnEstoqueChanged -= HandleEstoqueChanged;
            }
            if (purchaseManager != null)
            {
                purchaseManager.OnCompraConcluida -= HandleCompraConcluida;
                purchaseManager.OnErro -= HandleErroCompra;
            }
        }

        private void HandleMoedasChanged(int moedas) => OnMoedasAtualizadas?.Invoke(moedas);

        private void HandleEstoqueChanged() => OnEstoqueAtualizado?.Invoke();

        public int EstoqueDe(PowerUpConfig config) => boosterManager != null ? boosterManager.QuantidadeDisponivel(config) : 0;

        /// <summary>Botão "Evoluir" de um especial de tabuleiro na lista da loja.</summary>
        public void ComprarAvulso(PowerUpConfig config)
        {
            if (boosterManager == null || config == null) return;
            if (boosterManager.ComprarAvulso(config))
            {
                OnAvulsoComprado?.Invoke(config);
                OnMensagem?.Invoke($"{config.NomeExibicao} adicionado ao estoque.");
            }
            else
            {
                OnMoedaInsuficiente?.Invoke();
            }
        }

        public void TentarEvoluir(PowerUpConfig config)
        {
            if (boosterManager == null) return;

            bool sucesso = boosterManager.EvoluirPoder(config);
            if (sucesso)
            {
                OnPoderEvoluido?.Invoke(config);
                OnMensagem?.Invoke($"{config.NomeExibicao} evoluído para o nível {config.NivelAtual}.");
            }
            else
            {
                OnMoedaInsuficiente?.Invoke(); // moeda insuficiente OU já no nível máximo
            }
        }

        public void ComprarRemoverAnuncios() => purchaseManager?.ComprarRemoverAnuncios();

        private void HandleCompraConcluida() => OnMensagem?.Invoke("Compra concluída. Os anúncios foram removidos.");
        private void HandleErroCompra(string mensagem) => OnMensagem?.Invoke(mensagem);
    }
}
