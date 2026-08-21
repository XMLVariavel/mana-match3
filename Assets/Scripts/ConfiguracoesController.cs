using System;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Tela de Configurações: áudio (música/efeitos), vibração, gestão de
    /// privacidade/dados (LGPD) e status da compra "Remover Anúncios".
    ///
    /// Áudio e vibração são preferências de aparelho e ficam em PlayerPrefs
    /// (dentro de AudioManager/HapticsManager), não no Firestore — o mesmo
    /// jogador pode querer som no tablet e mudo no celular.
    /// </summary>
    public class ConfiguracoesController : MonoBehaviour
    {
        [SerializeField] private PrivacyManager privacyManager;
        [SerializeField] private PurchaseManager purchaseManager;
        [SerializeField] private FirebaseManager firebaseManager;
        [SerializeField] private ScreenNavigator navigator;

        [Header("Som e vibração")]
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private HapticsManager hapticsManager;

        [Header("Nomes das telas")]
        [SerializeField] private string telaLogin = "Login";
        [SerializeField] private string telaSplash = "Splash"; // após excluir a conta, volta pro início

        public bool AnunciosRemovidos => purchaseManager != null && purchaseManager.AnunciosRemovidos;
        public bool ContaVinculada => firebaseManager != null && firebaseManager.ContaVinculada;

        public event Action<string> OnDadosExportados; // JSON pronto pra compartilhar/salvar
        public event Action OnContaExcluida;
        public event Action<string> OnErro;

        // ---------------------------------------------------------------
        // Som e vibração
        // ---------------------------------------------------------------

        private AudioManager Audio => audioManager != null ? audioManager : AudioManager.Instance;
        private HapticsManager Haptics => hapticsManager != null ? hapticsManager : HapticsManager.Instance;

        public float VolumeMusica => Audio != null ? Audio.VolumeMusica : 0f;
        public float VolumeEfeitos => Audio != null ? Audio.VolumeEfeitos : 0f;
        public bool MusicaMuda => Audio != null && Audio.MusicaMuda;
        public bool EfeitosMudos => Audio != null && Audio.EfeitosMudos;
        public bool VibracaoAtiva => Haptics != null && Haptics.VibracaoAtiva;

        /// <summary>Slider de volume da música (0 a 1).</summary>
        public void DefinirVolumeMusica(float volume) => Audio?.DefinirVolumeMusica(volume);

        /// <summary>Slider de volume dos efeitos sonoros (0 a 1).</summary>
        public void DefinirVolumeEfeitos(float volume) => Audio?.DefinirVolumeEfeitos(volume);

        /// <summary>
        /// Toggle "Música" da UI. O toggle mostra o estado LIGADO, então o
        /// valor é invertido antes de chegar no AudioManager, que raciocina
        /// em termos de "mudo".
        /// </summary>
        public void DefinirMusicaLigada(bool ligada) => Audio?.DefinirMusicaMuda(!ligada);

        /// <summary>Toggle "Efeitos sonoros" da UI (true = ligado).</summary>
        public void DefinirEfeitosLigados(bool ligados) => Audio?.DefinirEfeitosMudos(!ligados);

        /// <summary>Toggle "Vibração" da UI.</summary>
        public void DefinirVibracaoAtiva(bool ativa) => Haptics?.DefinirVibracaoAtiva(ativa);

        // ---------------------------------------------------------------
        // Privacidade / conta
        // ---------------------------------------------------------------

        private void OnEnable()
        {
            if (privacyManager != null)
            {
                privacyManager.OnDadosExportados += HandleDadosExportados;
                privacyManager.OnDadosExcluidos += HandleDadosExcluidos;
                privacyManager.OnErro += HandleErro;
            }
        }

        private void OnDisable()
        {
            if (privacyManager != null)
            {
                privacyManager.OnDadosExportados -= HandleDadosExportados;
                privacyManager.OnDadosExcluidos -= HandleDadosExcluidos;
                privacyManager.OnErro -= HandleErro;
            }
        }

        public void AbrirVinculoDeConta() => navigator?.Mostrar(telaLogin);
        public void ExportarMeusDados() => privacyManager?.ExportarMeusDados();
        public void ComprarRemoverAnuncios() => purchaseManager?.ComprarRemoverAnuncios();

        /// <summary>Botão "Excluir minha conta" — a confirmação ("tem certeza?") é da UI, não daqui.</summary>
        public void ExcluirMinhaContaEDados() => privacyManager?.ExcluirMinhaContaEDados();

        private void HandleDadosExportados(string json) => OnDadosExportados?.Invoke(json);
        private void HandleErro(string mensagem) => OnErro?.Invoke(mensagem);

        private void HandleDadosExcluidos()
        {
            OnContaExcluida?.Invoke();
            navigator?.Mostrar(telaSplash);
        }
    }
}
