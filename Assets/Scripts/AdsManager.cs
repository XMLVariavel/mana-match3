using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Camada fina sobre o Google Mobile Ads Unity SDK. Duas responsabilidades:
    /// vídeo recompensado (sempre por escolha do jogador) e intersticial
    /// apenas após derrota, com teto de frequência para nunca virar "agressivo".
    /// Se o jogador tiver o plano sem anúncios ativo, este manager simplesmente
    /// não carrega/mostra nada — ver <see cref="DefinirSemAnuncios"/>.
    /// </summary>
    public class AdsManager : MonoBehaviour
    {
        // ATENÇÃO: os valores abaixo são as unidades de TESTE oficiais do Google
        // (documentadas em developers.google.com/admob/unity/test-ads). Elas
        // servem para validar o fluxo de anúncios sem risco de banimento por
        // cliques inválidos, mas NÃO geram receita. Substitua pelos IDs reais
        // da sua conta AdMob antes de publicar — e troque também o App ID no
        // GoogleMobileAdsSettings (Assets > Google Mobile Ads > Settings).
        [Header("IDs de unidade de anúncio (padrão = unidades de TESTE do Google)")]
        [SerializeField] private string idAnuncioRecompensado = "ca-app-pub-3940256099942544/5224354917";
        [SerializeField] private string idAnuncioIntersticial = "ca-app-pub-3940256099942544/1033173712";

        [Header("Teto de frequência do intersticial")]
        [Tooltip("Só mostra intersticial a cada N derrotas (não a cada derrota).")]
        [SerializeField] private int derrotasPorIntersticial = 3;

        private RewardedAd anuncioRecompensado;
        private InterstitialAd anuncioIntersticial;
        private bool semAnuncios; // true quando o jogador comprou "Remover Anúncios"
        private int derrotasDesdeUltimoIntersticial;

        public bool RecompensadoProntoParaExibir => !semAnuncios && anuncioRecompensado != null && anuncioRecompensado.CanShowAd();

        public event Action OnRecompensaConquistada;
        public event Action<string> OnErro;

        private void Start()
        {
            MobileAds.Initialize(_ =>
            {
                CarregarRecompensado();
                CarregarIntersticial();
            });
        }

        /// <summary>
        /// Chamado uma vez ao carregar o progresso do jogador (via
        /// FirebaseManager/PlayerProgress ou o resultado do IAP local) —
        /// desliga completamente os anúncios se ele já comprou o plano.
        /// </summary>
        public void DefinirSemAnuncios(bool ativo)
        {
            semAnuncios = ativo;
        }

        // ---------------------------------------------------------------
        // Vídeo recompensado — sempre iniciado pelo jogador (botão explícito)
        // ---------------------------------------------------------------

        private void CarregarRecompensado()
        {
            if (semAnuncios) return;

            RewardedAd.Load(idAnuncioRecompensado, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"Falha ao carregar vídeo recompensado: {error}");
                    return;
                }

                anuncioRecompensado = ad;
                RegistrarEventosDeFechamento(anuncioRecompensado, CarregarRecompensado);
            });
        }

        /// <summary>
        /// Exibe o vídeo recompensado. Só chame isto a partir de um botão que
        /// o próprio jogador tocou (ex: "assistir vídeo por +1 vida") — nunca
        /// de forma automática.
        /// </summary>
        public void ExibirRecompensado()
        {
            if (semAnuncios)
            {
                OnErro?.Invoke("Anúncios estão desativados neste plano.");
                return;
            }

            if (anuncioRecompensado == null || !anuncioRecompensado.CanShowAd())
            {
                OnErro?.Invoke("O vídeo ainda não carregou. Tente novamente em instantes.");
                CarregarRecompensado();
                return;
            }

            anuncioRecompensado.Show(_ => OnRecompensaConquistada?.Invoke());
        }

        // ---------------------------------------------------------------
        // Intersticial — só chamado após derrota, com teto de frequência
        // ---------------------------------------------------------------

        private void CarregarIntersticial()
        {
            if (semAnuncios) return;

            InterstitialAd.Load(idAnuncioIntersticial, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"Falha ao carregar intersticial: {error}");
                    return;
                }

                anuncioIntersticial = ad;
                RegistrarEventosDeFechamento(anuncioIntersticial, CarregarIntersticial);
            });
        }

        /// <summary>
        /// Chame isto no evento de derrota (ex: ScoreAndObjectiveManager.OnLose).
        /// Só exibe de fato quando o teto de frequência é atingido — o resto
        /// das vezes só incrementa o contador e sai.
        /// </summary>
        public void NotificarDerrota()
        {
            if (semAnuncios) return;

            derrotasDesdeUltimoIntersticial++;
            if (derrotasDesdeUltimoIntersticial < derrotasPorIntersticial) return;

            if (anuncioIntersticial != null && anuncioIntersticial.CanShowAd())
            {
                derrotasDesdeUltimoIntersticial = 0;
                anuncioIntersticial.Show();
            }
            // Se ainda não carregou, simplesmente não mostra desta vez — não
            // vale a pena atrasar o jogador esperando um anúncio.
        }

        // ---------------------------------------------------------------

        private void RegistrarEventosDeFechamento(RewardedAd ad, Action recarregar)
        {
            ad.OnAdFullScreenContentClosed += recarregar;
            ad.OnAdFullScreenContentFailed += _ => recarregar();
        }

        private void RegistrarEventosDeFechamento(InterstitialAd ad, Action recarregar)
        {
            ad.OnAdFullScreenContentClosed += recarregar;
            ad.OnAdFullScreenContentFailed += _ => recarregar();
        }
    }
}
