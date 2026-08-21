using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace BibleMatch3
{
    /// <summary>
    /// Único produto da loja: "Remover Anúncios" (compra única, não-consumível).
    /// De propósito não existe nenhum outro item pago — moedas e poderes só
    /// são ganhos jogando (ver PowerUpConfig/BoosterManager).
    /// </summary>
    public class PurchaseManager : MonoBehaviour, IStoreListener
    {
        private const string ProdutoRemoverAnuncios = "remover_anuncios";

        [SerializeField] private AdsManager adsManager;
        [SerializeField] private FirebaseManager firebaseManager;

        private IStoreController storeController;
        private IExtensionProvider storeExtensions;

        public bool AnunciosRemovidos { get; private set; }
        public bool LojaPronta => storeController != null;

        public event Action OnCompraConcluida;
        public event Action<string> OnErro;

        private void Start()
        {
            InicializarCompras();
        }

        private void InicializarCompras()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct(ProdutoRemoverAnuncios, ProductType.NonConsumable);
            UnityPurchasing.Initialize(this, builder);
        }

        public void ComprarRemoverAnuncios()
        {
            if (!LojaPronta)
            {
                OnErro?.Invoke("Loja ainda não está pronta. Tente novamente em instantes.");
                return;
            }

            Product produto = storeController.products.WithID(ProdutoRemoverAnuncios);
            if (produto == null || !produto.availableToPurchase)
            {
                OnErro?.Invoke("Não foi possível encontrar o produto na loja.");
                return;
            }

            storeController.InitiatePurchase(produto);
        }

        /// <summary>
        /// Aplica um estado de compra já conhecido (ex: vindo do
        /// PlayerProgress.SemAnuncios carregado do Firestore em um aparelho
        /// novo) sem precisar repassar pela loja.
        /// </summary>
        public void AplicarEstadoSalvo(bool anunciosRemovidos)
        {
            AnunciosRemovidos = anunciosRemovidos;
            if (adsManager != null) adsManager.DefinirSemAnuncios(anunciosRemovidos);
        }

        // ---------------------------------------------------------------
        // IStoreListener
        // ---------------------------------------------------------------

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            storeController = controller;
            storeExtensions = extensions;

            // No Android, uma compra não-consumível já feita normalmente é
            // devolvida automaticamente pelo Google Play na inicialização —
            // não é preciso um botão "Restaurar Compras" separado aqui.
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"Falha ao inicializar a loja: {error}");
            OnErro?.Invoke("Não foi possível conectar à loja agora.");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"Falha ao inicializar a loja: {error} — {message}");
            OnErro?.Invoke("Não foi possível conectar à loja agora.");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            if (args.purchasedProduct.definition.id != ProdutoRemoverAnuncios)
                return PurchaseProcessingResult.Complete;

            AnunciosRemovidos = true;
            if (adsManager != null) adsManager.DefinirSemAnuncios(true);

            if (firebaseManager != null && firebaseManager.UsuarioLogado)
                firebaseManager.AtualizarCompraRemoverAnuncios(true);

            OnCompraConcluida?.Invoke();
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogWarning($"Compra falhou: {reason}");
            OnErro?.Invoke("Não foi possível concluir a compra.");
        }
    }
}
