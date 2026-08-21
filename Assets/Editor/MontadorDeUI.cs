using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace BibleMatch3.EditorTools
{
    /// <summary>
    /// Monta a cena inteira do jogo — managers, tabuleiro, Canvas, as 9 telas,
    /// os prefabs e as ligações entre tudo — com um clique em
    /// <c>Tools > Maná > Montar cena completa</c>.
    ///
    /// Por que um montador em vez de arquivos .unity/.prefab escritos à mão:
    /// o formato YAML da Unity depende de fileIDs e GUIDs gerados pelo próprio
    /// Editor. Escrever isso fora da Unity produz cenas corrompidas com
    /// facilidade. Aqui, quem cria cada objeto é a Unity — o resultado é
    /// sempre válido, e o processo é repetível.
    ///
    /// É idempotente: rodar de novo apaga o que este montador criou antes
    /// (raízes com o prefixo "[Maná]") e reconstrói, sem tocar em nada mais
    /// da cena. Assets de dados já existentes são preservados.
    /// </summary>
    public static class MontadorDeUI
    {
        private const string Menu = "Tools/Maná/";
        private const string Prefixo = "[Maná] ";

        private const int LarguraDeReferencia = 1080;
        // As referências do produto e o Game View principal usam proporção 3:4.
        // 1080x1440 evita que a UI seja reduzida como se fosse uma tela 9:16.
        private const int AlturaDeReferencia = 1440;

        // Nomes de tela — precisam bater exatamente com os defaults dos controllers.
        private const string TelaSplash = "Splash";
        private const string TelaConsentimento = "TelaConsentimento";
        private const string TelaCarregando = "TelaCarregando";
        private const string TelaInicio = "Inicio";
        private const string TelaMapa = "MapaDeFases";
        private const string TelaDesafios = "Desafios";
        private const string TelaJogo = "TelaJogo";
        private const string TelaLoja = "Loja";
        private const string TelaPerfil = "Perfil";
        private const string TelaRanking = "Ranking";
        private const string TelaConfiguracoes = "Configuracoes";
        private const string TelaLogin = "Login";

        private sealed class Contexto
        {
            public Canvas Canvas;
            public ManaAssets.Catalogo Catalogo;
            public ManaPrefabs.Conjunto Prefabs;
            public readonly List<(string nome, GameObject raiz)> Telas = new List<(string, GameObject)>();

            public FirebaseManager Firebase;
            public PrivacyManager Privacidade;
            public AdsManager Anuncios;
            public PurchaseManager Compras;
            public LivesManager Vidas;
            public BoosterManager Boosters;
            public ScreenNavigator Navegador;
            public GoogleSignInService Google;
            public AudioManager Audio;
            public HapticsManager Haptics;

            public BoardManager Tabuleiro;
            public MatchDetector Detector;
            public BoardPhysics Fisica;
            public ScoreAndObjectiveManager Pontuacao;
            public ObstacleManager Obstaculos;
            public GameManager Jogo;
            public GameFeedbackController Feedback;

            public GameHUDController Hud;
            public MapaDeFasesController Mapa;
        }

        // ---------------------------------------------------------------
        // Menus
        // ---------------------------------------------------------------

        [MenuItem(Menu + "Montar cena completa", priority = 0)]
        public static void MontarTudo()
        {
            bool confirmou = EditorUtility.DisplayDialog(
                "Montar a cena do Maná",
                "Isto vai (re)criar na CENA ABERTA:\n\n" +
                "• Managers, tabuleiro e Canvas com as 9 telas\n" +
                "• Prefabs em Assets/Prefabs/\n" +
                "• Dados de exemplo em Assets/GameData/\n\n" +
                "Objetos com o prefixo \"[Maná]\" criados numa execução anterior serão " +
                "substituídos. O resto da cena não é tocado.\n\n" +
                "Continuar?",
                "Montar", "Cancelar");

            if (!confirmou) return;

            try
            {
                EditorUtility.DisplayProgressBar("Maná", "Preparando a arte...", 0.1f);
                int ajustados = ManaArte.PrepararImportacao();

                EditorUtility.DisplayProgressBar("Maná", "Gerando dados de exemplo...", 0.25f);
                var ctx = new Contexto { Catalogo = ManaAssets.Gerar() };

                EditorUtility.DisplayProgressBar("Maná", "Gerando prefabs...", 0.4f);
                ctx.Prefabs = ManaPrefabs.Gerar();

                EditorUtility.DisplayProgressBar("Maná", "Montando a cena...", 0.6f);
                LimparMontagemAnterior();
                PrepararCamera();
                PrepararEventSystem();
                CriarSistemas(ctx);
                CriarTabuleiro(ctx);
                CriarCanvas(ctx);

                EditorUtility.DisplayProgressBar("Maná", "Montando as telas...", 0.8f);
                MontarTelas(ctx);

                EditorUtility.DisplayProgressBar("Maná", "Ligando tudo...", 0.95f);
                LigarNavegador(ctx);
                LigarMapaAoHud(ctx);

                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                AssetDatabase.SaveAssets();

                Debug.Log($"[Maná] Cena montada. {ajustados} textura(s) reimportada(s) como sprite. " +
                          "Salve a cena (Ctrl+S) e dê Play para testar.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem(Menu + "Montar cena automática _F9", priority = 1)]
        public static void MontarTudoAutomatico()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Maná", "Preparando a arte...", 0.1f);
                int ajustados = ManaArte.PrepararImportacao();

                EditorUtility.DisplayProgressBar("Maná", "Gerando dados de exemplo...", 0.25f);
                var ctx = new Contexto { Catalogo = ManaAssets.Gerar() };

                EditorUtility.DisplayProgressBar("Maná", "Gerando prefabs...", 0.4f);
                ctx.Prefabs = ManaPrefabs.Gerar();

                EditorUtility.DisplayProgressBar("Maná", "Montando a cena...", 0.6f);
                LimparMontagemAnterior();
                PrepararCamera();
                PrepararEventSystem();
                CriarSistemas(ctx);
                CriarTabuleiro(ctx);
                CriarCanvas(ctx);

                EditorUtility.DisplayProgressBar("Maná", "Montando as telas...", 0.8f);
                MontarTelas(ctx);

                EditorUtility.DisplayProgressBar("Maná", "Ligando tudo...", 0.95f);
                LigarNavegador(ctx);
                LigarMapaAoHud(ctx);

                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                AssetDatabase.SaveAssets();
                Debug.Log($"[Maná] Cena remontada automaticamente. {ajustados} textura(s) reimportada(s).");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static void MontarTudoAutomaticoBatch()
        {
            const string caminhoCena = "Assets/Scenes/SampleScene.unity";
            if (SceneManager.GetActiveScene().path != caminhoCena)
                EditorSceneManager.OpenScene(caminhoCena, OpenSceneMode.Single);

            MontarTudoAutomatico();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), caminhoCena);
            AssetDatabase.SaveAssets();
            Debug.Log("[Maná] Montagem batch concluída e SampleScene salva.");
        }

        [MenuItem(Menu + "Só preparar arte placeholder", priority = 20)]
        public static void SoArte()
        {
            int n = ManaArte.PrepararImportacao();
            Debug.Log($"[Maná] {n} textura(s) reimportada(s) como sprite 256px.");
        }

        [MenuItem(Menu + "Só gerar prefabs", priority = 21)]
        public static void SoPrefabs()
        {
            ManaArte.PrepararImportacao();
            ManaPrefabs.Gerar();
            Debug.Log("[Maná] Prefabs gerados em Assets/Prefabs/.");
        }

        [MenuItem(Menu + "Só gerar dados de exemplo", priority = 22)]
        public static void SoDados()
        {
            ManaAssets.Gerar();
            Debug.Log("[Maná] Dados de exemplo gerados em Assets/GameData/.");
        }

        // ---------------------------------------------------------------
        // Cena: base
        // ---------------------------------------------------------------

        private static void LimparMontagemAnterior()
        {
            foreach (GameObject raiz in SceneManager.GetActiveScene().GetRootGameObjects())
                if (raiz.name.StartsWith(Prefixo)) Object.DestroyImmediate(raiz);
        }

        private static void PrepararCamera()
        {
            Camera camera = Camera.main;

            if (camera == null)
            {
                var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener))
                {
                    tag = "MainCamera"
                };
                camera = go.GetComponent<Camera>();
            }

            // O tabuleiro é 8x8 com célula de 1 unidade, centrado na origem.
            // O componente AdaptiveBoardCamera recalcula o enquadramento no Play;
            // 4.8 é um preview confortável para o Game View paisagem.
            camera.orthographic = true;
            camera.orthographicSize = 4.8f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = ManaUI.Fundo;

            var adaptativa = camera.GetComponent<AdaptiveBoardCamera>();
            if (adaptativa == null) adaptativa = camera.gameObject.AddComponent<AdaptiveBoardCamera>();
            using (var l = new Ligador(adaptativa))
            {
                l.Decimal("larguraDoTabuleiroComMoldura", 9.5f)
                 .Decimal("margemHorizontal", 0.20f)
                 .Decimal("orthoMinimoEmPaisagem", 4.6f);
            }
        }

        private static void PrepararEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;

            // StandaloneInputModule (Input Manager clássico) de propósito: o
            // BoardManager lê o toque via UnityEngine.Input, então UI e
            // tabuleiro precisam usar o mesmo sistema de entrada.
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static GameObject Raiz(string nome)
        {
            return new GameObject(Prefixo + nome);
        }

        private static void CriarSistemas(Contexto ctx)
        {
            // Áudio fica numa raiz própria porque AudioManager/HapticsManager
            // chamam DontDestroyOnLoad, que só funciona em GameObject raiz.
            GameObject audio = Raiz("Áudio");
            ctx.Audio = audio.AddComponent<AudioManager>();
            ctx.Haptics = audio.AddComponent<HapticsManager>();

            // Duas fontes separadas: música em loop e efeitos em PlayOneShot,
            // com volumes independentes. Se não forem ligadas aqui, o
            // AudioManager cria as suas no Awake — ligar agora deixa os
            // volumes visíveis e ajustáveis no Inspector.
            AudioSource fonteMusica = audio.AddComponent<AudioSource>();
            fonteMusica.playOnAwake = false;
            fonteMusica.loop = true;

            AudioSource fonteEfeitos = audio.AddComponent<AudioSource>();
            fonteEfeitos.playOnAwake = false;
            fonteEfeitos.loop = false;

            using (var l = new Ligador(ctx.Audio))
                l.Ref("fonteMusica", fonteMusica).Ref("fonteEfeitos", fonteEfeitos);

            GameObject sistemas = Raiz("Sistemas");
            ctx.Firebase = sistemas.AddComponent<FirebaseManager>();
            ctx.Privacidade = sistemas.AddComponent<PrivacyManager>();
            ctx.Anuncios = sistemas.AddComponent<AdsManager>();
            ctx.Compras = sistemas.AddComponent<PurchaseManager>();
            ctx.Vidas = sistemas.AddComponent<LivesManager>();
            ctx.Boosters = sistemas.AddComponent<BoosterManager>();
            ctx.Navegador = sistemas.AddComponent<ScreenNavigator>();
            ctx.Google = sistemas.AddComponent<GoogleSignInService>();

            using (var l = new Ligador(ctx.Privacidade)) l.Ref("firebaseManager", ctx.Firebase);
            using (var l = new Ligador(ctx.Compras)) l.Ref("adsManager", ctx.Anuncios).Ref("firebaseManager", ctx.Firebase);
        }

        private static void CriarTabuleiro(Contexto ctx)
        {
            GameObject raiz = Raiz("Tabuleiro");

            ctx.Tabuleiro = raiz.AddComponent<BoardManager>();
            ctx.Detector = raiz.AddComponent<MatchDetector>();
            ctx.Fisica = raiz.AddComponent<BoardPhysics>();
            ctx.Pontuacao = raiz.AddComponent<ScoreAndObjectiveManager>();
            ctx.Obstaculos = raiz.AddComponent<ObstacleManager>();
            ctx.Jogo = raiz.AddComponent<GameManager>();
            ctx.Feedback = raiz.AddComponent<GameFeedbackController>();

            // Origem no canto inferior-esquerdo: com 8 células de 1 unidade,
            // -3.5 deixa o tabuleiro centrado em (0,0).
            var origem = new GameObject("Origem").transform;
            origem.SetParent(raiz.transform, false);
            const float tamanhoCelula = 1.12f;
            float origemX = -(8f * tamanhoCelula) * 0.5f + tamanhoCelula * 0.5f;
            float origemY = origemX - 0.70f;
            origem.position = new Vector3(origemX, origemY, 0f);

            var pecas = new GameObject("Pecas").transform;
            pecas.SetParent(raiz.transform, false);

            var slots = new GameObject("SlotsDasCelulas").transform;
            slots.SetParent(raiz.transform, false);
            Sprite slotSprite = ManaUI.SpriteUI;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    var slot = new GameObject($"Slot_{x}_{y}", typeof(SpriteRenderer));
                    slot.transform.SetParent(slots, false);
                    slot.transform.position = origem.position + new Vector3(x * tamanhoCelula, y * tamanhoCelula, 0.05f);
                    slot.transform.localScale = Vector3.one * (tamanhoCelula * 0.96f);
                    var slotRenderer = slot.GetComponent<SpriteRenderer>();
                    slotRenderer.sprite = slotSprite;
                    slotRenderer.color = new Color(0.055f, 0.12f, 0.19f, 0.96f);
                    slotRenderer.sortingOrder = 5;
                }
            }

            var obstaculos = new GameObject("Obstaculos").transform;
            obstaculos.SetParent(raiz.transform, false);

            // Fundo bíblico da cena: fica atrás da moldura e das peças, ocupando
            // o espaço que a referência reserva para pedra, madeira e luz ambiente.
            Sprite fundoJornada = ManaArte.Carregar(ManaArte.FundoJornada);
            if (fundoJornada != null)
            {
                var fundo = new GameObject("FundoBiblico", typeof(SpriteRenderer));
                fundo.transform.SetParent(raiz.transform, false);
                fundo.transform.position = new Vector3(0f, -0.70f, 1f);
                var renderizadorFundo = fundo.GetComponent<SpriteRenderer>();
                renderizadorFundo.sprite = fundoJornada;
                renderizadorFundo.sortingOrder = -10;
                float larguraFundo = fundoJornada.bounds.size.x;
                float alturaFundo = fundoJornada.bounds.size.y;
                float escalaPorLargura = larguraFundo > 0.001f ? 10.8f / larguraFundo : 1f;
                float escalaPorAltura = alturaFundo > 0.001f ? 15.0f / alturaFundo : 1f;
                fundo.transform.localScale = Vector3.one * Mathf.Max(escalaPorLargura, escalaPorAltura);
            }

            // Moldura ilustrada: fica atrás das peças e cria uma área jogável
            // claramente separada do fundo da câmera.
            Sprite molduraSprite = ManaArte.Carregar(ManaArte.MolduraTabuleiro);
            if (molduraSprite != null)
            {
                var moldura = new GameObject("MolduraTabuleiro", typeof(SpriteRenderer));
                moldura.transform.SetParent(raiz.transform, false);
                moldura.transform.position = new Vector3(0f, -0.70f, 0f);

                var renderizadorMoldura = moldura.GetComponent<SpriteRenderer>();
                renderizadorMoldura.sprite = molduraSprite;
                renderizadorMoldura.sortingOrder = 0;

                float maiorLado = Mathf.Max(molduraSprite.bounds.size.x, molduraSprite.bounds.size.y);
                if (maiorLado > 0.001f)
                    moldura.transform.localScale = Vector3.one * (10.2f / maiorLado);
            }

            using (var l = new Ligador(ctx.Tabuleiro))
            {
                l.Ref("boardOrigin", origem)
                 .Numero("width", 8)
                 .Numero("height", 8)
                 .Decimal("cellSize", tamanhoCelula)
                 .Ref("tilesParent", pecas)
                 .Ref("tilePrefab", ctx.Prefabs.Peca)
                 .Ref("matchDetector", ctx.Detector)
                 .Ref("boardPhysics", ctx.Fisica)
                 .Ref("scoreManager", ctx.Pontuacao)
                 .Ref("gameManager", ctx.Jogo)
                 .Ref("obstacleManager", ctx.Obstaculos)
                 .Lista("tileSprites", ManaArte.SpritesDePecas())
                 .Lista("specialSprites", ManaArte.SpritesDeEspeciais());
            }

            using (var l = new Ligador(ctx.Detector))
            {
                l.Ref("objectiveManager", ctx.Pontuacao)
                 .Lista("especiaisDeTabuleiro", ctx.Catalogo.EspeciaisDeTabuleiro.ConvertAll(c => (Object)c));
            }

            using (var l = new Ligador(ctx.Fisica))
                l.Ref("boardManager", ctx.Tabuleiro)
                 .Ref("gameManager", ctx.Jogo)
                 .Ref("obstacleManager", ctx.Obstaculos);

            using (var l = new Ligador(ctx.Obstaculos))
            {
                l.Ref("obstaclePrefab", ctx.Prefabs.Obstaculo)
                 .Ref("obstaclesParent", obstaculos)
                 .Ref("pedraSprite", ManaArte.Carregar(ManaArte.PedraDeserto))
                 .Ref("correnteSprite", ManaArte.Carregar(ManaArte.Corrente))
                 .Ref("geloSprite", ManaArte.Carregar(ManaArte.Gelo))
                 .Ref("caixaSprite", ManaArte.Carregar(ManaArte.CaixaSelada));
            }

            using (var l = new Ligador(ctx.Jogo))
            {
                l.Ref("boardManager", ctx.Tabuleiro)
                 .Ref("scoreManager", ctx.Pontuacao)
                 .Ref("obstacleManager", ctx.Obstaculos)
                 .Lista("versiculosDisponiveis", ctx.Catalogo.Versiculos.ConvertAll(v => (Object)v));
            }

            using (var l = new Ligador(ctx.Feedback))
            {
                l.Ref("boardPhysics", ctx.Fisica)
                 .Ref("scoreManager", ctx.Pontuacao)
                 .Ref("audioManager", ctx.Audio)
                 .Ref("hapticsManager", ctx.Haptics);
            }

            using (var l = new Ligador(ctx.Boosters))
            {
                l.Ref("boardManager", ctx.Tabuleiro)
                 .Ref("matchDetector", ctx.Detector)
                 .Ref("boardPhysics", ctx.Fisica)
                 .Ref("scoreManager", ctx.Pontuacao)
                 .Ref("firebaseManager", ctx.Firebase)
                 .Lista("configsAvulsos", ctx.Catalogo.Avulsos.ConvertAll(c => (Object)c));
            }
        }

        private static void CriarCanvas(Contexto ctx)
        {
            GameObject raiz = Raiz("Canvas");
            raiz.layer = LayerMask.NameToLayer("UI");

            var canvas = raiz.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10; // acima dos sprites do tabuleiro

            var escala = raiz.AddComponent<CanvasScaler>();
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(LarguraDeReferencia, AlturaDeReferencia);
            escala.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            escala.matchWidthOrHeight = 0.5f;

            raiz.AddComponent<GraphicRaycaster>();
            ctx.Canvas = canvas;
        }

        // ---------------------------------------------------------------
        // Telas
        // ---------------------------------------------------------------

        private static RectTransform NovaTela(Contexto ctx, string nome, bool comFundo = true)
        {
            RectTransform tela = ManaUI.Vazio(nome, ctx.Canvas.transform);
            ManaUI.Esticar(tela);

            if (comFundo)
            {
                var fundo = ManaUI.PainelIlustrado("Fundo", tela, ManaUI.Fundo);
                fundo.transform.SetAsFirstSibling();
                var contraste = ManaUI.Painel_("Contraste", tela, new Color(0.012f, 0.045f, 0.085f, 0.62f), false);
                contraste.transform.SetSiblingIndex(1);
            }

            ctx.Telas.Add((nome, tela.gameObject));
            return tela;
        }

        private static RectTransform Cabecalho(RectTransform tela, string titulo)
        {
            RectTransform barra = ManaUI.Vazio("Cabecalho", tela);
            ManaUI.Faixa(barra, 0f, 140f);
            barra.gameObject.AddComponent<SafeAreaHeader>();

            var fundo = ManaUI.PainelCabecalho("Fundo", barra, ManaUI.Painel);
            fundo.transform.SetAsFirstSibling();

            var ornamentoSuperior = new GameObject("OrnamentoSuperior", typeof(RectTransform), typeof(Image));
            ornamentoSuperior.transform.SetParent(barra, false);
            var ornamentoSuperiorRt = ornamentoSuperior.GetComponent<RectTransform>();
            ornamentoSuperiorRt.anchorMin = new Vector2(0.22f, 0f);
            ornamentoSuperiorRt.anchorMax = new Vector2(0.78f, 0f);
            ornamentoSuperiorRt.offsetMin = new Vector2(0f, 18f);
            ornamentoSuperiorRt.offsetMax = new Vector2(0f, 21f);
            ornamentoSuperior.GetComponent<Image>().color = ManaUI.Dourado;

            var texto = ManaUI.Texto("Titulo", barra, titulo, 44f, TextAlignmentOptions.Center, ManaUI.Dourado, FontStyles.Bold);
            texto.rectTransform.offsetMin = new Vector2(180f, 24f);
            texto.rectTransform.offsetMax = new Vector2(-180f, -26f);

            return barra;
        }

        private static Button BotaoVoltar(Contexto ctx, RectTransform cabecalho, string destino)
        {
            Button botao = ManaUI.BotaoDeArte("BotaoVoltar", cabecalho, ManaArte.BotaoVoltar, ManaUI.BotaoSecundario);
            var rt = botao.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.offsetMin = new Vector2(18f, 16f);
            rt.offsetMax = new Vector2(112f, -16f);

            Eventos.AoClicarIrPara(botao, ctx.Navegador, destino);
            return botao;
        }

        private static void MontarTelas(Contexto ctx)
        {
            MontarSplashEConsentimento(ctx);
            MontarMapaDeFases(ctx);
            MontarInicio(ctx);
            MontarDesafios(ctx);
            MontarTelaDeJogo(ctx);
            MontarLoja(ctx);
            MontarPerfil(ctx);
            MontarRanking(ctx);
            MontarConfiguracoes(ctx);
            MontarLogin(ctx);
        }

        // --- Splash / Consentimento / Carregando -----------------------

        private static void MontarSplashEConsentimento(Contexto ctx)
        {
            RectTransform splash = NovaTela(ctx, TelaSplash);
            var controller = splash.gameObject.AddComponent<SplashOnboardingController>();

            var videoAlvo = splash.gameObject.AddComponent<RawImage>();
            videoAlvo.name = "VideoAbertura";
            ManaUI.Esticar(videoAlvo.rectTransform);
            videoAlvo.raycastTarget = false;
            videoAlvo.color = Color.white;
            videoAlvo.transform.SetAsFirstSibling();

            var videoPlayer = splash.gameObject.AddComponent<SplashVideoPlayer>();
            VideoClip videoClip = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/Video/intro_mana_jesus_te_ama.mp4");

            var titulo = ManaUI.Texto("Titulo", splash, "MANÁ", 92f, TextAlignmentOptions.Center, ManaUI.Dourado, FontStyles.Bold);
            titulo.rectTransform.anchorMin = new Vector2(0f, 0.64f);
            titulo.rectTransform.anchorMax = new Vector2(1f, 0.78f);
            titulo.rectTransform.offsetMin = Vector2.zero;
            titulo.rectTransform.offsetMax = Vector2.zero;

            var mensagem = ManaUI.Texto("MensagemJesus", splash, "Jesus Te Ama", 50f,
                TextAlignmentOptions.Center, ManaUI.TextoClaro, FontStyles.Bold);
            mensagem.rectTransform.anchorMin = new Vector2(0.08f, 0.43f);
            mensagem.rectTransform.anchorMax = new Vector2(0.92f, 0.55f);
            mensagem.rectTransform.offsetMin = Vector2.zero;
            mensagem.rectTransform.offsetMax = Vector2.zero;

            var subtitulo = ManaUI.Texto("Subtitulo", splash, "Alimente-se da Palavra, um match de cada vez.",
                24f, TextAlignmentOptions.Top, ManaUI.TextoClaro);
            subtitulo.rectTransform.anchorMin = new Vector2(0.1f, 0.35f);
            subtitulo.rectTransform.anchorMax = new Vector2(0.9f, 0.41f);
            subtitulo.rectTransform.offsetMin = Vector2.zero;
            subtitulo.rectTransform.offsetMax = Vector2.zero;

            // --- Consentimento ---
            RectTransform consentimento = NovaTela(ctx, TelaConsentimento);
            Cabecalho(consentimento, "Privacidade");

            RectTransform corpo = ManaUI.Vazio("Corpo", consentimento);
            corpo.anchorMin = new Vector2(0.06f, 0.2f);
            corpo.anchorMax = new Vector2(0.94f, 0.88f);
            corpo.offsetMin = Vector2.zero;
            corpo.offsetMax = Vector2.zero;

            ManaUI.Texto("Explicacao", corpo,
                "Para salvar seu progresso na nuvem e mostrar o placar, precisamos criar uma conta " +
                "anônima e guardar dados do seu jogo (fases, pontuação e versículos coletados).\n\n" +
                "Você pode recusar: o jogo funciona normalmente, mas seu progresso fica apenas neste " +
                "aparelho e não entra no ranking.\n\n" +
                "A qualquer momento, em Configurações, você pode exportar ou apagar seus dados.",
                24f, TextAlignmentOptions.TopLeft);

            RectTransform botoes = ManaUI.Vazio("Botoes", consentimento);
            ManaUI.FaixaInferior(botoes, 60f, 180f);
            var layout = ManaUI.Coluna(botoes.gameObject, 16f, 40);
            layout.childForceExpandHeight = true;

            Button aceitar = ManaUI.Botao("BotaoAceitar", botoes, "Aceitar e continuar", ManaUI.BotaoPrimario);
            Button recusar = ManaUI.Botao("BotaoRecusar", botoes, "Continuar sem salvar na nuvem", ManaUI.BotaoSecundario, 22f);

            Eventos.AoClicar(aceitar, controller.AceitarConsentimento);
            Eventos.AoClicar(recusar, controller.RecusarConsentimento);

            // --- Carregando ---
            RectTransform carregando = NovaTela(ctx, TelaCarregando);
            ManaUI.Texto("Mensagem", carregando, "Preparando sua jornada...", 32f, TextAlignmentOptions.Center, ManaUI.TextoFraco);

            using (var l = new Ligador(controller))
            {
                l.Ref("privacyManager", ctx.Privacidade)
                 .Ref("firebaseManager", ctx.Firebase)
                 .Ref("navigator", ctx.Navegador)
                 .Texto("telaConsentimento", TelaConsentimento)
                 .Texto("telaCarregando", TelaCarregando)
                 .Texto("telaSeguinte", TelaInicio);
            }

            using (var l = new Ligador(videoPlayer))
            {
                l.Ref("clip", videoClip)
                 .Ref("alvo", videoAlvo);
            }
        }

        // --- Mapa de Fases ---------------------------------------------

        private static void MontarMapaDeFases(Contexto ctx)
        {
            RectTransform tela = NovaTela(ctx, TelaMapa);
            var controller = tela.gameObject.AddComponent<MapaDeFasesController>();
            ctx.Mapa = controller;

            RectTransform cabecalho = Cabecalho(tela, "MODOS DE JOGO");
            RecursosNoCabecalho(ctx, tela, cabecalho);

            RectTransform area = ManaUI.Vazio("Trilha", tela);
            area.anchorMin = new Vector2(0f, 0f);
            area.anchorMax = new Vector2(1f, 1f);
            area.offsetMin = new Vector2(0f, 156f);
            area.offsetMax = new Vector2(0f, -140f);

            ManaUI.Rolagem("Rolagem", area, out RectTransform conteudo, grade: false);

            Button campanha = ManaUI.CardModo("CardCampanha", conteudo, "campaign", "CAMPANHA", "Avance pelos episódios e desbloqueie novos capítulos.", ManaUI.Dourado);
            Button estudo = ManaUI.CardModo("CardEstudoInfinito", conteudo, "infinite", "ESTUDO INFINITO", "Faça combos, conquiste XP e descubra novos versículos.", ManaUI.BotaoPrimario);
            Button diario = ManaUI.CardModo("CardDesafioDiario", conteudo, "daily", "DESAFIO DIÁRIO", "Uma missão bíblica renovada a cada dia.", ManaUI.Dourado);
            Button relogio = ManaUI.CardModo("CardContraRelogio", conteudo, "time", "CONTRA O RELÓGIO", "Pontue o máximo antes que a areia termine.", ManaUI.BotaoPrimario);
            Button guardiao = ManaUI.CardModo("CardGuardiao", conteudo, "guardian", "GUARDIÃO DA PALAVRA", "Proteja a sequência e responda ao desafio da Palavra.", ManaUI.Dourado);

            RectTransform rodape = ManaUI.Vazio("Rodape", tela);
            ManaUI.FaixaInferior(rodape, 0f, 150f);
            var fundoRodape = ManaUI.PainelNavegacao("Fundo", rodape, ManaUI.Painel);
            fundoRodape.transform.SetAsFirstSibling();

            RectTransform linhaMenu = ManaUI.Vazio("Menu", rodape);
            ManaUI.Faixa(linhaMenu, 18f, 106f);
            linhaMenu.offsetMin = new Vector2(12f, linhaMenu.offsetMin.y);
            linhaMenu.offsetMax = new Vector2(-12f, linhaMenu.offsetMax.y);
            ManaUI.Linha(linhaMenu.gameObject, 8f);

            Button inicio = ManaUI.Botao("BotaoInicio", linhaMenu, "Início", ManaUI.BotaoSecundario, 16f);
            Button jornada = ManaUI.Botao("BotaoJornada", linhaMenu, "Jornada", ManaUI.BotaoPrimario, 16f);
            Button desafios = ManaUI.Botao("BotaoDesafios", linhaMenu, "Desafios", ManaUI.BotaoSecundario, 16f);
            Button loja = ManaUI.Botao("BotaoLoja", linhaMenu, "Loja", ManaUI.BotaoSecundario, 15f);
            Button perfil = ManaUI.Botao("BotaoPerfil", linhaMenu, "Perfil", ManaUI.BotaoSecundario, 15f);
            Button opcoes = ManaUI.Botao("BotaoOpcoes", linhaMenu, "Opções", ManaUI.BotaoSecundario, 15f);

            Eventos.AoClicar(campanha, controller.EntrarNaPrimeiraFase);
            Eventos.AoClicar(estudo, controller.EntrarNoEstudoInfinito);
            Eventos.AoClicar(diario, controller.EntrarNoDesafioDiario);
            Eventos.AoClicar(relogio, controller.EntrarNoContraRelogio);
            Eventos.AoClicar(guardiao, controller.EntrarNoGuardiaoDaPalavra);
            Eventos.AoClicar(inicio, controller.AbrirInicio);
            Eventos.AoClicar(jornada, controller.AbrirJornada);
            Eventos.AoClicar(desafios, controller.AbrirDesafios);
            Eventos.AoClicar(loja, controller.AbrirLoja);
            Eventos.AoClicar(perfil, controller.AbrirPerfil);
            Eventos.AoClicar(opcoes, controller.AbrirConfiguracoes);

            using (var l = new Ligador(controller))
            {
                l.Ref("firebaseManager", ctx.Firebase)
                 .Ref("gameManager", ctx.Jogo)
                 .Ref("livesManager", ctx.Vidas)
                 .Ref("hudController", ctx.Hud)
                 .Ref("navigator", ctx.Navegador)
                 .Ref("containerDaTrilha", conteudo)
                 .Ref("botaoFasePrefab", ctx.Prefabs.BotaoFase)
                 .Texto("telaInicio", TelaInicio)
                 .Texto("telaDesafios", TelaDesafios)
                 .Texto("telaJogo", TelaJogo)
                 .Texto("telaLoja", TelaLoja)
                 .Texto("telaPerfil", TelaPerfil)
                 .Texto("telaRanking", TelaRanking)
                 .Texto("telaConfiguracoes", TelaConfiguracoes)
                 .Lista("fasesDaCampanha", ctx.Catalogo.Fases.ConvertAll(f => (Object)f));
            }
        }

        private static void MontarInicio(Contexto ctx)
        {
            RectTransform tela = NovaTela(ctx, TelaInicio);
            RectTransform cabecalho = Cabecalho(tela, "INÍCIO");
            RecursosNoCabecalho(ctx, tela, cabecalho);

            RectTransform corpo = ManaUI.Vazio("CorpoInicio", tela);
            corpo.anchorMin = new Vector2(0.08f, 0f);
            corpo.anchorMax = new Vector2(0.92f, 1f);
            corpo.offsetMin = new Vector2(0f, 170f);
            corpo.offsetMax = new Vector2(0f, -160f);
            var coluna = ManaUI.Coluna(corpo.gameObject, 18f, 0);
            coluna.childForceExpandHeight = false;

            var boasVindas = ManaUI.Texto("BoasVindas", corpo,
                "Alimente-se da Palavra, um match de cada vez.", 28f,
                TextAlignmentOptions.Center, ManaUI.TextoClaro, FontStyles.Bold);
            ManaUI.Altura(boasVindas.gameObject, 74f);

            var explicacao = ManaUI.Texto("ExplicacaoInicio", corpo,
                "Escolha uma experiência para jogar, acompanhe sua jornada e personalize seu perfil.",
                20f, TextAlignmentOptions.Center, ManaUI.TextoFraco);
            ManaUI.Altura(explicacao.gameObject, 62f);

            Button jornada = ManaUI.Botao("BotaoJornada", corpo, "MODOS DE JOGO", ManaUI.BotaoPrimario, 24f);
            ManaUI.Altura(jornada.gameObject, 76f);
            Button desafios = ManaUI.Botao("BotaoDesafios", corpo, "DESAFIOS", ManaUI.BotaoSecundario, 24f);
            ManaUI.Altura(desafios.gameObject, 76f);
            Button opcoes = ManaUI.Botao("BotaoOpcoes", corpo, "OPÇÕES", ManaUI.BotaoSecundario, 24f);
            ManaUI.Altura(opcoes.gameObject, 76f);

            Eventos.AoClicar(jornada, ctx.Mapa.AbrirJornada);
            Eventos.AoClicar(desafios, ctx.Mapa.AbrirDesafios);
            Eventos.AoClicar(opcoes, ctx.Mapa.AbrirConfiguracoes);

            MontarNavegacao(ctx, tela, ctx.Mapa);
        }

        private static void MontarDesafios(Contexto ctx)
        {
            RectTransform tela = NovaTela(ctx, TelaDesafios);
            RectTransform cabecalho = Cabecalho(tela, "DESAFIOS");
            RecursosNoCabecalho(ctx, tela, cabecalho);

            RectTransform area = ManaUI.Vazio("ListaDesafios", tela);
            area.anchorMin = new Vector2(0.06f, 0f);
            area.anchorMax = new Vector2(0.94f, 1f);
            area.offsetMin = new Vector2(0f, 170f);
            area.offsetMax = new Vector2(0f, -150f);
            ManaUI.Rolagem("Rolagem", area, out RectTransform conteudo, grade: false);

            Button diario = ManaUI.CardModo("CardDesafioDiario", conteudo, "daily", "DESAFIO DIÁRIO", "Uma missão bíblica renovada a cada dia.", ManaUI.Dourado);
            Button relogio = ManaUI.CardModo("CardContraRelogio", conteudo, "time", "CONTRA O RELÓGIO", "Pontue o máximo antes que o tempo termine.", ManaUI.BotaoPrimario);
            Button guardiao = ManaUI.CardModo("CardGuardiao", conteudo, "guardian", "GUARDIÃO DA PALAVRA", "Proteja objetivos e complete a missão da Palavra.", ManaUI.Dourado);

            Eventos.AoClicar(diario, ctx.Mapa.EntrarNoDesafioDiario);
            Eventos.AoClicar(relogio, ctx.Mapa.EntrarNoContraRelogio);
            Eventos.AoClicar(guardiao, ctx.Mapa.EntrarNoGuardiaoDaPalavra);

            MontarNavegacao(ctx, tela, ctx.Mapa);
        }

        private static void MontarNavegacao(Contexto ctx, RectTransform tela, MapaDeFasesController controller)
        {
            RectTransform rodape = ManaUI.Vazio("Rodape", tela);
            ManaUI.FaixaInferior(rodape, 0f, 150f);
            var fundo = ManaUI.PainelNavegacao("Fundo", rodape, ManaUI.Painel);
            fundo.transform.SetAsFirstSibling();
            RectTransform linha = ManaUI.Vazio("Menu", rodape);
            ManaUI.Faixa(linha, 18f, 106f);
            linha.offsetMin = new Vector2(12f, linha.offsetMin.y);
            linha.offsetMax = new Vector2(-12f, linha.offsetMax.y);
            ManaUI.Linha(linha.gameObject, 8f);

            Button inicio = ManaUI.Botao("BotaoInicio", linha, "Início", ManaUI.BotaoSecundario, 16f);
            Button jornada = ManaUI.Botao("BotaoJornada", linha, "Jornada", ManaUI.BotaoPrimario, 16f);
            Button desafios = ManaUI.Botao("BotaoDesafios", linha, "Desafios", ManaUI.BotaoSecundario, 16f);
            Button loja = ManaUI.Botao("BotaoLoja", linha, "Loja", ManaUI.BotaoSecundario, 15f);
            Button perfil = ManaUI.Botao("BotaoPerfil", linha, "Perfil", ManaUI.BotaoSecundario, 15f);
            Button opcoes = ManaUI.Botao("BotaoOpcoes", linha, "Opções", ManaUI.BotaoSecundario, 15f);
            Eventos.AoClicar(inicio, controller.AbrirInicio);
            Eventos.AoClicar(jornada, controller.AbrirJornada);
            Eventos.AoClicar(desafios, controller.AbrirDesafios);
            Eventos.AoClicar(loja, controller.AbrirLoja);
            Eventos.AoClicar(perfil, controller.AbrirPerfil);
            Eventos.AoClicar(opcoes, controller.AbrirConfiguracoes);
        }

        private static void RecursosNoCabecalho(Contexto ctx, RectTransform tela, RectTransform cabecalho)
        {
            RectTransform faixa = ManaUI.Vazio("Recursos", cabecalho);
            faixa.anchorMin = new Vector2(0f, 0f);
            faixa.anchorMax = new Vector2(1f, 0f);
            faixa.pivot = new Vector2(0.5f, 0f);
            faixa.offsetMin = new Vector2(24f, 8f);
            faixa.offsetMax = new Vector2(-24f, 48f);
            ManaUI.Linha(faixa.gameObject, 24f);

            var vidas = ManaUI.Texto("Vidas", faixa, "5", 24f, TextAlignmentOptions.MidlineLeft);
            var proxima = ManaUI.Texto("ProximaVida", faixa, "Cheio", 22f, TextAlignmentOptions.Center, ManaUI.TextoFraco);
            var moedas = ManaUI.Texto("Moedas", faixa, "0", 24f, TextAlignmentOptions.MidlineRight, ManaUI.Dourado);

            var view = tela.gameObject.AddComponent<RecursosView>();
            using (var l = new Ligador(view))
            {
                l.Ref("livesManager", ctx.Vidas)
                 .Ref("boosterManager", ctx.Boosters)
                 .Ref("textoVidas", vidas)
                 .Ref("textoMoedas", moedas)
                 .Ref("textoProximaVida", proxima);
            }
        }

        // --- Tela de Jogo ----------------------------------------------

        private static void MontarTelaDeJogo(Contexto ctx)
        {
            // Sem fundo de tela cheia: o tabuleiro é desenhado por
            // SpriteRenderers no mundo, atrás do Canvas. Um painel opaco aqui
            // esconderia o jogo inteiro.
            RectTransform tela = NovaTela(ctx, TelaJogo, comFundo: false);

            var hud = tela.gameObject.AddComponent<GameHUDController>();
            var view = tela.gameObject.AddComponent<GameHUDView>();
            var modal = tela.gameObject.AddComponent<VerseCardModalController>();
            var versiculoView = tela.gameObject.AddComponent<VerseCardView>();
            ctx.Hud = hud;

            // --- Barra superior ---
            RectTransform topo = ManaUI.Vazio("BarraSuperior", tela);
            // A tela de jogo também respeita a área segura. Sem este componente,
            // o notch pode cobrir o logo e os primeiros pixels do cabeçalho.
            ManaUI.Faixa(topo, 0f, 480f);
            topo.gameObject.AddComponent<SafeAreaHeader>();
            var fundoTopo = ManaUI.PainelCabecalho("Fundo", topo, ManaUI.Painel);
            fundoTopo.transform.SetAsFirstSibling();

            var marca = ManaUI.Texto("Marca", topo, "MANÁ", 78f, TextAlignmentOptions.Center, ManaUI.Dourado, FontStyles.Bold, false);
            marca.outlineWidth = 0.25f;
            marca.outlineColor = ManaUI.Fundo;
            marca.rectTransform.anchorMin = new Vector2(0.20f, 1f);
            marca.rectTransform.anchorMax = new Vector2(0.80f, 1f);
            marca.rectTransform.pivot = new Vector2(0.5f, 1f);
            marca.rectTransform.offsetMin = new Vector2(0f, -122f);
            marca.rectTransform.offsetMax = new Vector2(0f, -18f);

            var subtitulo = ManaUI.Texto("Subtitulo", topo, "BUSQUE O CÉU, VIVA A PALAVRA", 20f, TextAlignmentOptions.Center, ManaUI.TextoClaro, FontStyles.Bold, false);
            subtitulo.rectTransform.anchorMin = new Vector2(0.14f, 1f);
            subtitulo.rectTransform.anchorMax = new Vector2(0.86f, 1f);
            subtitulo.rectTransform.pivot = new Vector2(0.5f, 1f);
            subtitulo.rectTransform.offsetMin = new Vector2(0f, -156f);
            subtitulo.rectTransform.offsetMax = new Vector2(0f, -126f);

            // Brasão da referência. O par de textos acima continua no projeto
            // como fallback: se a arte não estiver importada, a marca aparece
            // mesmo assim em vez de o topo ficar vazio.
            RectTransform logo = ManaUI.Vazio("Logo", topo);
            logo.anchorMin = new Vector2(0.5f, 1f);
            logo.anchorMax = new Vector2(0.5f, 1f);
            logo.pivot = new Vector2(0.5f, 1f);
            logo.anchoredPosition = new Vector2(0f, -4f);
            logo.sizeDelta = new Vector2(232f, 180f);
            Image arteLogo = ManaUI.Arte("Arte", logo, ManaArte.LogoMana);
            bool temLogo = arteLogo.sprite != null;
            logo.gameObject.SetActive(temLogo);
            marca.gameObject.SetActive(!temLogo);
            subtitulo.gameObject.SetActive(!temLogo);

            Button voltar = ManaUI.BotaoDeArte("BotaoSair", topo, ManaArte.BotaoVoltar, ManaUI.BotaoSecundario);
            var voltarRt = voltar.GetComponent<RectTransform>();
            voltarRt.anchorMin = new Vector2(0f, 1f);
            voltarRt.anchorMax = new Vector2(0f, 1f);
            voltarRt.pivot = new Vector2(0f, 1f);
            voltarRt.anchoredPosition = new Vector2(20f, -16f);
            voltarRt.sizeDelta = new Vector2(94f, 94f);

            RectTransform versiculo = ManaUI.Vazio("VersiculoBadge", topo);
            versiculo.anchorMin = new Vector2(1f, 1f);
            versiculo.anchorMax = new Vector2(1f, 1f);
            versiculo.pivot = new Vector2(1f, 1f);
            versiculo.anchoredPosition = new Vector2(-16f, -14f);
            versiculo.sizeDelta = new Vector2(172f, 120f);
            Image fundoVersiculo = ManaUI.Arte("Fundo", versiculo, ManaArte.BadgeLivro);
            if (fundoVersiculo.sprite == null)
            {
                ManaUI.PainelPergaminho("FundoLiso", versiculo, false).transform.SetAsFirstSibling();
            }
            var textoVersiculo = ManaUI.Texto("Texto", versiculo, "SALMOS\n23:1", 19f, TextAlignmentOptions.Center,
                new Color(0.243f, 0.153f, 0.075f, 1f), FontStyles.Bold, false);
            textoVersiculo.rectTransform.offsetMin = new Vector2(20f, 24f);
            textoVersiculo.rectTransform.offsetMax = new Vector2(-20f, -16f);

            RectTransform cartaoScore = ManaUI.Vazio("CartaoScore", topo);
            cartaoScore.anchorMin = new Vector2(0.025f, 1f);
            cartaoScore.anchorMax = new Vector2(0.335f, 1f);
            cartaoScore.pivot = new Vector2(0.5f, 1f);
            cartaoScore.offsetMin = new Vector2(0f, -336f);
            cartaoScore.offsetMax = new Vector2(0f, -192f);
            ManaUI.PainelEstatistica("Fundo", cartaoScore, ManaUI.Painel).transform.SetAsFirstSibling();
            var rotuloScore = ManaUI.Texto("Rotulo", cartaoScore, "PONTOS", 16f, TextAlignmentOptions.Center, ManaUI.TextoFraco, FontStyles.Bold);
            rotuloScore.rectTransform.anchorMin = new Vector2(0f, 0.70f);
            rotuloScore.rectTransform.anchorMax = new Vector2(1f, 0.94f);
            rotuloScore.rectTransform.offsetMin = Vector2.zero;
            rotuloScore.rectTransform.offsetMax = Vector2.zero;
            RectTransform estrelasScore = ManaUI.Vazio("Estrelas", cartaoScore);
            estrelasScore.anchorMin = new Vector2(0.10f, 0.06f);
            estrelasScore.anchorMax = new Vector2(0.90f, 0.30f);
            estrelasScore.offsetMin = Vector2.zero;
            estrelasScore.offsetMax = Vector2.zero;
            CriarFileiraDeEstrelas(estrelasScore, "Estrela", 3, 30f, 28f, 3);

            RectTransform cartaoMovimentos = ManaUI.Vazio("CartaoMovimentos", topo);
            cartaoMovimentos.anchorMin = new Vector2(0.35f, 1f);
            cartaoMovimentos.anchorMax = new Vector2(0.65f, 1f);
            cartaoMovimentos.pivot = new Vector2(0.5f, 1f);
            cartaoMovimentos.offsetMin = new Vector2(0f, -336f);
            cartaoMovimentos.offsetMax = new Vector2(0f, -192f);
            ManaUI.PainelEstatistica("Fundo", cartaoMovimentos, ManaUI.Painel).transform.SetAsFirstSibling();
            var rotuloMovimentos = ManaUI.Texto("Rotulo", cartaoMovimentos, "MOVIMENTOS", 15f, TextAlignmentOptions.Center, ManaUI.TextoFraco, FontStyles.Bold);
            rotuloMovimentos.rectTransform.anchorMin = new Vector2(0f, 0.70f);
            rotuloMovimentos.rectTransform.anchorMax = new Vector2(1f, 0.94f);
            rotuloMovimentos.rectTransform.offsetMin = Vector2.zero;
            rotuloMovimentos.rectTransform.offsetMax = Vector2.zero;

            RectTransform cartaoProgresso = ManaUI.Vazio("CartaoProgresso", topo);
            cartaoProgresso.anchorMin = new Vector2(0.665f, 1f);
            cartaoProgresso.anchorMax = new Vector2(0.975f, 1f);
            cartaoProgresso.pivot = new Vector2(0.5f, 1f);
            cartaoProgresso.offsetMin = new Vector2(0f, -336f);
            cartaoProgresso.offsetMax = new Vector2(0f, -192f);
            ManaUI.PainelEstatistica("Fundo", cartaoProgresso, ManaUI.Painel).transform.SetAsFirstSibling();
            var rotuloProgresso = ManaUI.Texto("Rotulo", cartaoProgresso, "PROGRESSO", 15f, TextAlignmentOptions.Center, ManaUI.TextoFraco, FontStyles.Bold);
            rotuloProgresso.rectTransform.anchorMin = new Vector2(0f, 0.68f);
            rotuloProgresso.rectTransform.anchorMax = new Vector2(1f, 0.94f);
            rotuloProgresso.rectTransform.offsetMin = Vector2.zero;
            rotuloProgresso.rectTransform.offsetMax = Vector2.zero;

            var barraXp = new GameObject("BarraXP", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            barraXp.transform.SetParent(cartaoProgresso, false);
            var barraXpRt = (RectTransform)barraXp.transform;
            barraXpRt.anchorMin = new Vector2(0.10f, 0.20f);
            barraXpRt.anchorMax = new Vector2(0.90f, 0.34f);
            barraXpRt.offsetMin = Vector2.zero;
            barraXpRt.offsetMax = Vector2.zero;
            var barraXpImg = barraXp.GetComponent<Image>();
            barraXpImg.sprite = ManaUI.SpriteUI;
            barraXpImg.type = Image.Type.Sliced;
            barraXpImg.color = new Color(0.02f, 0.06f, 0.10f, 1f);
            barraXpImg.raycastTarget = false;

            var preenchimentoXp = new GameObject("Preenchimento", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            preenchimentoXp.transform.SetParent(barraXp.transform, false);
            var preenchimentoXpRt = (RectTransform)preenchimentoXp.transform;
            ManaUI.Esticar(preenchimentoXpRt, 3f);
            var preenchimentoXpImg = preenchimentoXp.GetComponent<Image>();
            preenchimentoXpImg.sprite = ManaUI.SpriteUI;
            preenchimentoXpImg.type = Image.Type.Filled;
            preenchimentoXpImg.fillMethod = Image.FillMethod.Horizontal;
            preenchimentoXpImg.fillOrigin = 0;
            preenchimentoXpImg.fillAmount = 0f;
            preenchimentoXpImg.color = ManaUI.Dourado;
            preenchimentoXpImg.raycastTarget = false;

            var score = ManaUI.Texto("Score", cartaoScore, "0", 40f, TextAlignmentOptions.Center, ManaUI.Dourado, FontStyles.Bold, false);
            score.rectTransform.anchorMin = new Vector2(0f, 0.28f);
            score.rectTransform.anchorMax = new Vector2(1f, 0.72f);
            score.rectTransform.offsetMin = Vector2.zero;
            score.rectTransform.offsetMax = Vector2.zero;

            var movimentos = ManaUI.Texto("Movimentos", cartaoMovimentos, "20", 52f, TextAlignmentOptions.Center, ManaUI.TextoClaro, FontStyles.Bold, false);
            movimentos.rectTransform.anchorMin = new Vector2(0f, 0.12f);
            movimentos.rectTransform.anchorMax = new Vector2(1f, 0.70f);
            movimentos.rectTransform.offsetMin = Vector2.zero;
            movimentos.rectTransform.offsetMax = Vector2.zero;

            var tempo = ManaUI.Texto("Tempo", cartaoProgresso, "01:30", 24f, TextAlignmentOptions.Center, ManaUI.TextoClaro, FontStyles.Bold, false);
            tempo.rectTransform.anchorMin = new Vector2(0.08f, 0.36f);
            tempo.rectTransform.anchorMax = new Vector2(0.92f, 0.64f);
            tempo.rectTransform.offsetMin = Vector2.zero;
            tempo.rectTransform.offsetMax = Vector2.zero;
            tempo.gameObject.SetActive(false);

            var combo = ManaUI.Texto("Combo", topo, "", 22f, TextAlignmentOptions.Center, ManaUI.Dourado, FontStyles.Bold, false);
            combo.rectTransform.anchorMin = new Vector2(0.02f, 1f);
            combo.rectTransform.anchorMax = new Vector2(0.34f, 1f);
            combo.rectTransform.pivot = new Vector2(0.5f, 1f);
            combo.rectTransform.offsetMin = new Vector2(0f, -190f);
            combo.rectTransform.offsetMax = new Vector2(0f, -160f);
            combo.gameObject.SetActive(false);

            var xp = ManaUI.Texto("Xp", cartaoProgresso, "", 22f, TextAlignmentOptions.Center, ManaUI.Dourado, FontStyles.Bold, false);
            xp.rectTransform.anchorMin = new Vector2(0f, 0.36f);
            xp.rectTransform.anchorMax = new Vector2(1f, 0.66f);
            xp.rectTransform.offsetMin = Vector2.zero;
            xp.rectTransform.offsetMax = Vector2.zero;
            xp.gameObject.SetActive(false);

            var metaProgresso = ManaUI.Texto("MetaProgresso", cartaoProgresso, "0 / 20.000", 15f, TextAlignmentOptions.Center, ManaUI.TextoClaro, FontStyles.Bold, false);
            metaProgresso.rectTransform.anchorMin = new Vector2(0.04f, 0.02f);
            metaProgresso.rectTransform.anchorMax = new Vector2(0.96f, 0.19f);
            metaProgresso.rectTransform.offsetMin = Vector2.zero;
            metaProgresso.rectTransform.offsetMax = Vector2.zero;

            var modo = ManaUI.Texto("Modo", topo, "CAMPANHA  •  OBJETIVOS DA FASE", 17f,
                TextAlignmentOptions.Center, ManaUI.Dourado, FontStyles.Bold, false);
            modo.rectTransform.anchorMin = new Vector2(0.08f, 1f);
            modo.rectTransform.anchorMax = new Vector2(0.92f, 1f);
            modo.rectTransform.pivot = new Vector2(0.5f, 1f);
            modo.rectTransform.offsetMin = new Vector2(0f, -364f);
            modo.rectTransform.offsetMax = new Vector2(0f, -338f);

            var briefing = ManaUI.Texto("Briefing", topo, "", 12f,
                TextAlignmentOptions.Center, ManaUI.TextoFraco, FontStyles.Normal, false);
            briefing.rectTransform.anchorMin = new Vector2(0.10f, 1f);
            briefing.rectTransform.anchorMax = new Vector2(0.90f, 1f);
            briefing.rectTransform.pivot = new Vector2(0.5f, 1f);
            briefing.rectTransform.offsetMin = new Vector2(0f, -386f);
            briefing.rectTransform.offsetMax = new Vector2(0f, -364f);

            RectTransform objetivos = ManaUI.Vazio("Objetivos", topo);
            objetivos.anchorMin = new Vector2(0f, 1f);
            objetivos.anchorMax = new Vector2(1f, 1f);
            objetivos.pivot = new Vector2(0.5f, 1f);
            objetivos.offsetMin = new Vector2(26f, -478f);
            objetivos.offsetMax = new Vector2(-26f, -384f);
            var fundoObjetivos = ManaUI.FaixaDePergaminho("Fundo", objetivos, false);
            // As pontas roladas do pergaminho são mais altas que a linha de
            // itens: sem essa folga elas ficariam achatadas contra o texto.
            fundoObjetivos.rectTransform.offsetMin = new Vector2(0f, -16f);
            fundoObjetivos.rectTransform.offsetMax = new Vector2(0f, 16f);
            fundoObjetivos.transform.SetAsFirstSibling();

            RectTransform linhaObjetivos = ManaUI.Vazio("Itens", objetivos);
            ManaUI.Esticar(linhaObjetivos);
            linhaObjetivos.offsetMin = new Vector2(74f, 2f);
            linhaObjetivos.offsetMax = new Vector2(-74f, -2f);
            var layoutObjetivos = ManaUI.Linha(linhaObjetivos.gameObject, 14f);
            layoutObjetivos.childForceExpandWidth = false;
            layoutObjetivos.childControlWidth = false;

            // --- Barra inferior (poderes avulsos) ---
            RectTransform rodape = ManaUI.Vazio("BarraInferior", tela);
            ManaUI.FaixaInferior(rodape, 0f, 268f);
            var fundoRodape = ManaUI.PainelNavegacao("Fundo", rodape, ManaUI.Painel);
            fundoRodape.transform.SetAsFirstSibling();

            var statusPoderes = ManaUI.Texto("StatusPoderes", rodape,
                "Escolha uma jogada especial", 18f, TextAlignmentOptions.Center, ManaUI.TextoFraco, FontStyles.Normal, false);
            statusPoderes.rectTransform.anchorMin = new Vector2(0f, 1f);
            statusPoderes.rectTransform.anchorMax = new Vector2(1f, 1f);
            statusPoderes.rectTransform.pivot = new Vector2(0.5f, 1f);
            statusPoderes.rectTransform.offsetMin = new Vector2(20f, -34f);
            statusPoderes.rectTransform.offsetMax = new Vector2(-20f, -6f);

            RectTransform linhaPoderes = ManaUI.Vazio("Poderes", rodape);
            ManaUI.Esticar(linhaPoderes);
            linhaPoderes.offsetMin = new Vector2(16f, 12f);
            linhaPoderes.offsetMax = new Vector2(-16f, -38f);
            var layoutPoderes = ManaUI.Linha(linhaPoderes.gameObject, 10f);
            layoutPoderes.childForceExpandWidth = true;

            Button martelo = ManaUI.BotaoPoderCircular("BotaoMartelo", linhaPoderes, "MARTELO",
                "Quebra um bloco", "✣", "power_hammer", new Color(0.16f, 0.35f, 0.74f, 1f), out TextMeshProUGUI contadorMartelo);
            Button embaralhar = ManaUI.BotaoPoderCircular("BotaoEmbaralhar", linhaPoderes, "EMBARALHAR",
                "Mistura o tabuleiro", "↻", "power_shuffle", new Color(0.12f, 0.55f, 0.48f, 1f), out TextMeshProUGUI contadorEmbaralhar);
            Button movimentosExtra = ManaUI.BotaoPoderCircular("BotaoMaisMovimentos", linhaPoderes, "+5 MOV.",
                "Adiciona 5 movimentos", "+5", "power_plus5", new Color(0.85f, 0.54f, 0.12f, 1f), out TextMeshProUGUI contadorMaisMovimentos);

            Eventos.AoClicar(voltar, hud.VoltarParaMapa);
            Eventos.AoClicar(martelo, hud.AtivarMartelo);
            Eventos.AoClicar(embaralhar, hud.UsarEmbaralhar);
            Eventos.AoClicar(movimentosExtra, hud.UsarMaisMovimentos);

            // --- Painéis de fim de fase ---
            GameObject painelVitoria = PainelDeFim(ctx, tela, "PainelVitoria", "Fase concluída!", out StarRatingView estrelas, out TextMeshProUGUI detalhesResultado, out Button proximaFase, hud);
            GameObject painelDerrota = PainelDeFim(ctx, tela, "PainelDerrota", "O desafio terminou", out _, out _, out _, hud);
            painelVitoria.SetActive(false);
            painelDerrota.SetActive(false);

            // --- Modal de versículo ---
            GameObject raizModal = MontarModalDeVersiculo(tela, modal, versiculoView, ctx);

            using (var l = new Ligador(hud))
            {
                l.Ref("scoreManager", ctx.Pontuacao)
                 .Ref("gameManager", ctx.Jogo)
                 .Ref("boardManager", ctx.Tabuleiro)
                 .Ref("boosterManager", ctx.Boosters)
                 .Ref("adsManager", ctx.Anuncios)
                 .Ref("firebaseManager", ctx.Firebase)
                 .Ref("navigator", ctx.Navegador)
                 .Ref("configMartelo", ctx.Catalogo.Martelo)
                 .Ref("configEmbaralhar", ctx.Catalogo.Embaralhar)
                 .Ref("configMaisMovimentos", ctx.Catalogo.MaisMovimentos)
                 .Texto("telaMapaDeFases", TelaMapa);
            }

            using (var l = new Ligador(view))
            {
                l.Ref("controller", hud)
                 .Ref("textoScore", score)
                 .Ref("textoMovimentos", movimentos)
                 .Ref("textoMovimentosTitulo", rotuloMovimentos)
                 .Ref("textoTempo", tempo)
                 .Ref("textoCombo", combo)
                 .Ref("textoXp", xp)
                 .Ref("textoProgressoTitulo", rotuloProgresso)
                 .Ref("textoBriefing", briefing)
                 .Ref("contadorMartelo", contadorMartelo)
                 .Ref("contadorEmbaralhar", contadorEmbaralhar)
                 .Ref("contadorMaisMovimentos", contadorMaisMovimentos)
                 .Ref("barraProgresso", preenchimentoXpImg)
                 .Ref("textoMetaProgresso", metaProgresso)
                 .Ref("botaoMaisMovimentos", movimentosExtra)
                 .Ref("painelObjetivos", objetivos.gameObject)
                 .Ref("textoModo", modo)
                 .Ref("textoStatusPoderes", statusPoderes)
                 .Ref("containerObjetivos", linhaObjetivos)
                 .Ref("itemObjetivoPrefab", ctx.Prefabs.ItemObjetivo)
                 .Ref("painelVitoria", painelVitoria)
                 .Ref("painelDerrota", painelDerrota)
                 .Ref("estrelasVitoria", estrelas)
                 .Ref("textoResultadoDetalhes", detalhesResultado)
                 .Ref("botaoProximaFase", proximaFase)
                 .Lista("iconesPorTipo", ManaArte.SpritesDePecas());
            }

            using (var l = new Ligador(modal))
                l.Ref("gameManager", ctx.Jogo).Ref("raizDoModal", raizModal);
        }

        private static GameObject PainelDeFim(
            Contexto ctx, RectTransform tela, string nome, string titulo,
            out StarRatingView estrelas, out TextMeshProUGUI detalhes, out Button proximaFase, GameHUDController hud)
        {
            RectTransform painel = ManaUI.Vazio(nome, tela);
            ManaUI.Esticar(painel);

            var veu = ManaUI.Painel_("Veu", painel, new Color(0f, 0f, 0f, 0.72f));
            veu.transform.SetAsFirstSibling();

            RectTransform caixa = ManaUI.Vazio("Caixa", painel);
            caixa.anchorMin = new Vector2(0.1f, 0.34f);
            caixa.anchorMax = new Vector2(0.9f, 0.66f);
            caixa.offsetMin = Vector2.zero;
            caixa.offsetMax = Vector2.zero;
            ManaUI.PainelOrnamentado("Fundo", caixa, ManaUI.Painel);
            ManaUI.Coluna(caixa.gameObject, 18f, 32);

            var textoTitulo = ManaUI.Texto("Titulo", caixa, titulo, 40f, TextAlignmentOptions.Center, ManaUI.Dourado, FontStyles.Bold);
            ManaUI.Altura(textoTitulo.gameObject, 60f);

            RectTransform linhaEstrelas = ManaUI.Vazio("Estrelas", caixa);
            ManaUI.Altura(linhaEstrelas.gameObject, 60f);
            estrelas = CriarFileiraDeEstrelas(linhaEstrelas, "EstrelaVitoria", 3, 42f, 42f, 0);

            detalhes = ManaUI.Texto("Detalhes", caixa, "", 16f, TextAlignmentOptions.Center, ManaUI.TextoClaro, FontStyles.Normal, false);
            ManaUI.Altura(detalhes.gameObject, 104f);
            detalhes.enableWordWrapping = true;

            bool permiteProxima = nome == "PainelVitoria";
            proximaFase = ManaUI.Botao("BotaoProximaFase", caixa, "Próxima fase", ManaUI.BotaoPrimario);
            ManaUI.Altura(proximaFase.gameObject, 68f);
            proximaFase.gameObject.SetActive(permiteProxima);
            Eventos.AoClicar(proximaFase, hud.AvancarParaProximaFase);

            Button voltar = ManaUI.Botao("BotaoVoltarAoMapa", caixa, "Voltar ao mapa", ManaUI.BotaoSecundario);
            ManaUI.Altura(voltar.gameObject, 72f);
            Eventos.AoClicar(voltar, hud.VoltarParaMapa);

            return painel.gameObject;
        }

        private static StarRatingView CriarFileiraDeEstrelas(
            RectTransform pai, string prefixo, int quantidade, float largura, float altura, int ativas)
        {
            var layout = pai.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            var graficos = new StarGraphic[Mathf.Max(0, quantidade)];
            for (int i = 0; i < graficos.Length; i++)
            {
                // O construtor de GameObject não resolve [RequireComponent],
                // então o CanvasRenderer que todo Graphic precisa tem de vir
                // declarado aqui — sem ele a estrela lança MissingComponent.
                var go = new GameObject($"{prefixo}{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(StarGraphic), typeof(LayoutElement));
                go.transform.SetParent(pai, false);
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(largura, altura);

                var item = go.GetComponent<LayoutElement>();
                item.minWidth = largura;
                item.preferredWidth = largura;
                item.minHeight = altura;
                item.preferredHeight = altura;
                item.flexibleWidth = 0f;
                item.flexibleHeight = 0f;
                graficos[i] = go.GetComponent<StarGraphic>();
            }

            var rating = pai.gameObject.AddComponent<StarRatingView>();
            rating.Configurar(graficos, ativas);
            return rating;
        }

        private static GameObject MontarModalDeVersiculo(
            RectTransform tela, VerseCardModalController modal, VerseCardView view, Contexto ctx)
        {
            RectTransform raiz = ManaUI.Vazio("ModalVersiculo", tela);
            ManaUI.Esticar(raiz);

            var veu = ManaUI.Painel_("Veu", raiz, new Color(0f, 0f, 0f, 0.78f));
            veu.transform.SetAsFirstSibling();

            RectTransform caixa = ManaUI.Vazio("Caixa", raiz);
            caixa.anchorMin = new Vector2(0.08f, 0.28f);
            caixa.anchorMax = new Vector2(0.92f, 0.72f);
            caixa.offsetMin = Vector2.zero;
            caixa.offsetMax = Vector2.zero;
            ManaUI.PainelOrnamentado("Fundo", caixa, ManaUI.PainelClaro);
            ManaUI.Coluna(caixa.gameObject, 14f, 32);

            var texto = ManaUI.Texto("Versiculo", caixa, "", 28f, TextAlignmentOptions.Center, ManaUI.TextoClaro, FontStyles.Italic);
            var referencia = ManaUI.Texto("Referencia", caixa, "", 24f, TextAlignmentOptions.Center, ManaUI.Dourado, FontStyles.Bold);
            ManaUI.Altura(referencia.gameObject, 36f);
            var reflexao = ManaUI.Texto("Reflexao", caixa, "", 22f, TextAlignmentOptions.Center, ManaUI.TextoFraco);

            Button fechar = ManaUI.Botao("BotaoFechar", caixa, "Continuar", ManaUI.BotaoPrimario);
            ManaUI.Altura(fechar.gameObject, 68f);
            Eventos.AoClicar(fechar, modal.Fechar);

            using (var l = new Ligador(view))
            {
                l.Ref("gameManager", ctx.Jogo)
                 .Ref("textoVersiculo", texto)
                 .Ref("textoReferencia", referencia)
                 .Ref("textoReflexao", reflexao);
            }

            raiz.gameObject.SetActive(false);
            return raiz.gameObject;
        }

        // --- Loja -------------------------------------------------------

        private static void MontarLoja(Contexto ctx)
        {
            RectTransform tela = NovaTela(ctx, TelaLoja);
            var controller = tela.gameObject.AddComponent<LojaController>();
            var view = tela.gameObject.AddComponent<LojaView>();

            RectTransform cabecalho = Cabecalho(tela, "Loja");
            BotaoVoltar(ctx, cabecalho, TelaMapa);

            RectTransform carteira = ManaUI.Vazio("Carteira", cabecalho);
            carteira.anchorMin = new Vector2(1f, 0.5f);
            carteira.anchorMax = new Vector2(1f, 0.5f);
            carteira.pivot = new Vector2(1f, 0.5f);
            carteira.anchoredPosition = new Vector2(-24f, -12f);
            carteira.sizeDelta = new Vector2(180f, 60f);

            RectTransform disco = ManaUI.Vazio("Moeda", carteira);
            disco.anchorMin = new Vector2(0f, 0.5f);
            disco.anchorMax = new Vector2(0f, 0.5f);
            disco.pivot = new Vector2(0f, 0.5f);
            disco.anchoredPosition = Vector2.zero;
            disco.sizeDelta = new Vector2(52f, 52f);
            ManaUI.Arte("Arte", disco, ManaArte.Moeda);

            var moedas = ManaUI.Texto("Moedas", carteira, "0", 30f, TextAlignmentOptions.MidlineRight, ManaUI.Dourado, FontStyles.Bold, false);
            moedas.rectTransform.offsetMin = new Vector2(58f, 0f);
            moedas.rectTransform.offsetMax = Vector2.zero;

            RectTransform area = ManaUI.Vazio("Catalogo", tela);
            area.anchorMin = Vector2.zero;
            area.anchorMax = Vector2.one;
            area.offsetMin = new Vector2(0f, 170f);
            area.offsetMax = new Vector2(0f, -140f);
            ManaUI.Rolagem("Rolagem", area, out RectTransform conteudo);

            var instrucoes = ManaUI.Texto("InstrucoesPoderes", conteudo,
                "Como usar: ganhe moedas vencendo fases. Compre pacotes aqui e use os poderes na tela de jogo. Cada pacote adiciona 3 unidades.",
                15f, TextAlignmentOptions.TopLeft, ManaUI.TextoFraco, FontStyles.Normal, false);
            ManaUI.Altura(instrucoes.gameObject, 58f);
            instrucoes.enableWordWrapping = true;

            var tituloEspeciais = ManaUI.Texto("TituloEspeciais", conteudo, "Poderes do tabuleiro", 26f, TextAlignmentOptions.MidlineLeft, ManaUI.Dourado, FontStyles.Bold);
            ManaUI.Altura(tituloEspeciais.gameObject, 44f);

            RectTransform containerEspeciais = ManaUI.Vazio("Especiais", conteudo);
            var colunaEspeciais = ManaUI.Coluna(containerEspeciais.gameObject, 12f, 0);
            colunaEspeciais.childForceExpandHeight = false;

            var tituloAvulsos = ManaUI.Texto("TituloAvulsos", conteudo, "Poderes avulsos", 26f, TextAlignmentOptions.MidlineLeft, ManaUI.Dourado, FontStyles.Bold);
            ManaUI.Altura(tituloAvulsos.gameObject, 44f);

            RectTransform containerAvulsos = ManaUI.Vazio("Avulsos", conteudo);
            var colunaAvulsos = ManaUI.Coluna(containerAvulsos.gameObject, 12f, 0);
            colunaAvulsos.childForceExpandHeight = false;

            var mensagem = ManaUI.Texto("Mensagem", tela, "", 22f, TextAlignmentOptions.Center, ManaUI.Dourado);
            ManaUI.FaixaInferior(mensagem.rectTransform, 150f, 40f);

            RectTransform rodape = ManaUI.Vazio("Rodape", tela);
            ManaUI.FaixaInferior(rodape, 0f, 140f);
            var fundoRodape = ManaUI.PainelNavegacao("Fundo", rodape, ManaUI.Painel);
            fundoRodape.transform.SetAsFirstSibling();
            ManaUI.Coluna(rodape.gameObject, 8f, 20);

            var statusAnuncios = ManaUI.Texto("StatusAnuncios", rodape, "Remover Anúncios — compra única", 20f, TextAlignmentOptions.Center, ManaUI.TextoFraco);
            ManaUI.Altura(statusAnuncios.gameObject, 28f);

            Button comprar = ManaUI.Botao("BotaoRemoverAnuncios", rodape, "Remover Anúncios", ManaUI.BotaoPrimario);
            ManaUI.Altura(comprar.gameObject, 68f);
            Eventos.AoClicar(comprar, controller.ComprarRemoverAnuncios);

            using (var l = new Ligador(controller))
            {
                l.Ref("boosterManager", ctx.Boosters)
                 .Ref("purchaseManager", ctx.Compras)
                 .Lista("especiaisDeTabuleiro", ctx.Catalogo.EspeciaisDeTabuleiro.ConvertAll(c => (Object)c))
                 .Lista("avulsos", ctx.Catalogo.Avulsos.ConvertAll(c => (Object)c));
            }

            using (var l = new Ligador(view))
            {
                l.Ref("controller", controller)
                 .Ref("containerEspeciais", containerEspeciais)
                 .Ref("containerAvulsos", containerAvulsos)
                 .Ref("itemPrefab", ctx.Prefabs.ItemLoja)
                 .Ref("textoMoedas", moedas)
                 .Ref("textoMensagem", mensagem)
                 .Ref("textoStatusAnuncios", statusAnuncios);
            }
        }

        // --- Perfil -----------------------------------------------------

        private static void MontarPerfil(Contexto ctx)
        {
            RectTransform tela = NovaTela(ctx, TelaPerfil);
            var controller = tela.gameObject.AddComponent<PerfilController>();
            var view = tela.gameObject.AddComponent<PerfilView>();
            var avatarPicker = tela.gameObject.AddComponent<AvatarPickerView>();

            RectTransform cabecalho = Cabecalho(tela, "Perfil");
            BotaoVoltar(ctx, cabecalho, TelaMapa);

            RectTransform corpo = ManaUI.Vazio("Corpo", tela);
            corpo.anchorMin = new Vector2(0.06f, 0f);
            corpo.anchorMax = new Vector2(0.94f, 1f);
            corpo.offsetMin = new Vector2(0f, 60f);
            corpo.offsetMax = new Vector2(0f, -170f);
            var coluna = ManaUI.Coluna(corpo.gameObject, 16f, 0);
            coluna.childAlignment = TextAnchor.UpperCenter;

            var avatarPreviewGo = new GameObject("AvatarPreview", typeof(RectTransform), typeof(Image));
            avatarPreviewGo.transform.SetParent(corpo, false);
            var avatarPreview = avatarPreviewGo.GetComponent<Image>();
            avatarPreview.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Avatars/avatar_davi.png");
            avatarPreview.preserveAspect = true;
            ManaUI.Altura(avatarPreviewGo, 180f);

            var avatarNome = ManaUI.Texto("AvatarNome", corpo, "Avatar: Davi", 22f, TextAlignmentOptions.Center, ManaUI.Dourado, FontStyles.Bold);
            ManaUI.Altura(avatarNome.gameObject, 32f);

            RectTransform avatarContainer = ManaUI.Vazio("Avatares", corpo);
            ManaUI.Altura(avatarContainer.gameObject, 280f);

            var rotuloNome = ManaUI.Texto("RotuloNome", corpo, "Seu nome no placar", 22f, TextAlignmentOptions.MidlineLeft, ManaUI.TextoFraco);
            ManaUI.Altura(rotuloNome.gameObject, 30f);

            TMP_InputField campoNome = ManaUI.Campo("CampoNome", corpo, "Peregrino");
            ManaUI.Altura(campoNome.gameObject, 70f);

            Button salvar = ManaUI.Botao("BotaoSalvarNome", corpo, "Salvar nome", ManaUI.BotaoPrimario, 22f);
            ManaUI.Altura(salvar.gameObject, 64f);

            var mensagem = ManaUI.Texto("Mensagem", corpo, "", 20f, TextAlignmentOptions.Center, ManaUI.Dourado);
            ManaUI.Altura(mensagem.gameObject, 30f);

            var nivel = ManaUI.Texto("Nivel", corpo, "Nível 1", 34f, TextAlignmentOptions.Center, ManaUI.Dourado, FontStyles.Bold);
            ManaUI.Altura(nivel.gameObject, 46f);

            var xp = ManaUI.Texto("Xp", corpo, "0 XP", 24f, TextAlignmentOptions.Center);
            ManaUI.Altura(xp.gameObject, 34f);

            var recorde = ManaUI.Texto("HighScore", corpo, "0", 24f, TextAlignmentOptions.Center);
            ManaUI.Altura(recorde.gameObject, 34f);

            var versiculos = ManaUI.Texto("Versiculos", corpo, "0 versículos coletados", 22f, TextAlignmentOptions.Center, ManaUI.TextoFraco);
            ManaUI.Altura(versiculos.gameObject, 34f);

            var statusConta = ManaUI.Texto("StatusConta", corpo, "Conta padrão", 20f, TextAlignmentOptions.Center, ManaUI.TextoFraco);
            ManaUI.Altura(statusConta.gameObject, 30f);

            Eventos.AoClicar(salvar, view.SalvarNome);

            using (var l = new Ligador(controller)) l.Ref("firebaseManager", ctx.Firebase);

            using (var l = new Ligador(avatarPicker))
            {
                l.Ref("controller", controller)
                 .Ref("container", avatarContainer)
                 .Ref("preview", avatarPreview)
                 .Ref("textoAvatar", avatarNome);
            }

            using (var l = new Ligador(view))
            {
                l.Ref("controller", controller)
                 .Ref("campoNome", campoNome)
                 .Ref("textoNivel", nivel)
                 .Ref("textoXp", xp)
                 .Ref("textoHighScore", recorde)
                 .Ref("textoVersiculos", versiculos)
                 .Ref("textoStatusConta", statusConta)
                 .Ref("textoMensagem", mensagem);
            }
        }

        // --- Ranking ----------------------------------------------------

        private static void MontarRanking(Contexto ctx)
        {
            RectTransform tela = NovaTela(ctx, TelaRanking);
            var controller = tela.gameObject.AddComponent<RankingController>();
            var view = tela.gameObject.AddComponent<RankingView>();

            RectTransform cabecalho = Cabecalho(tela, "Placar");
            BotaoVoltar(ctx, cabecalho, TelaMapa);

            RectTransform area = ManaUI.Vazio("Lista", tela);
            area.anchorMin = Vector2.zero;
            area.anchorMax = Vector2.one;
            area.offsetMin = new Vector2(24f, 120f);
            area.offsetMax = new Vector2(-24f, -230f);
            ManaUI.Rolagem("Rolagem", area, out RectTransform conteudo);

            RectTransform filtros = ManaUI.Vazio("FiltrosRanking", tela);
            ManaUI.Faixa(filtros, 150f, 64f);
            filtros.offsetMin = new Vector2(24f, filtros.offsetMin.y);
            filtros.offsetMax = new Vector2(-24f, filtros.offsetMax.y);
            ManaUI.Linha(filtros.gameObject, 8f);
            Button filtroGeral = ManaUI.Botao("FiltroGeral", filtros, "GERAL", ManaUI.BotaoSecundario, 18f);
            Button filtroInfinito = ManaUI.Botao("FiltroInfinito", filtros, "INFINITO", ManaUI.BotaoSecundario, 18f);
            Button filtroDiario = ManaUI.Botao("FiltroDiario", filtros, "DIÁRIO", ManaUI.BotaoSecundario, 18f);
            Button filtroRelogio = ManaUI.Botao("FiltroRelogio", filtros, "TEMPO", ManaUI.BotaoSecundario, 18f);
            Button filtroGuardiao = ManaUI.Botao("FiltroGuardiao", filtros, "GUARDIÃO", ManaUI.BotaoSecundario, 18f);
            ManaUI.Altura(filtroGeral.gameObject, 58f);
            ManaUI.Altura(filtroInfinito.gameObject, 58f);
            ManaUI.Altura(filtroDiario.gameObject, 58f);
            ManaUI.Altura(filtroRelogio.gameObject, 58f);
            ManaUI.Altura(filtroGuardiao.gameObject, 58f);
            Eventos.AoClicar(filtroGeral, controller.DefinirModoGeral);
            Eventos.AoClicar(filtroInfinito, controller.DefinirModoInfinito);
            Eventos.AoClicar(filtroDiario, controller.DefinirModoDiario);
            Eventos.AoClicar(filtroRelogio, controller.DefinirModoTempo);
            Eventos.AoClicar(filtroGuardiao, controller.DefinirModoGuardiao);

            var mensagem = ManaUI.Texto("Mensagem", tela, "Carregando placar...", 24f, TextAlignmentOptions.Center, ManaUI.TextoFraco);
            mensagem.rectTransform.anchorMin = new Vector2(0.08f, 0.43f);
            mensagem.rectTransform.anchorMax = new Vector2(0.92f, 0.57f);
            mensagem.rectTransform.offsetMin = Vector2.zero;
            mensagem.rectTransform.offsetMax = Vector2.zero;

            Button recarregar = ManaUI.Botao("BotaoRecarregar", tela, "Atualizar", ManaUI.BotaoSecundario, 22f);
            var recarregarRt = recarregar.GetComponent<RectTransform>();
            ManaUI.FaixaInferior(recarregarRt, 8f, 56f);
            recarregarRt.offsetMin = new Vector2(300f, recarregarRt.offsetMin.y);
            recarregarRt.offsetMax = new Vector2(-300f, recarregarRt.offsetMax.y);
            Eventos.AoClicar(recarregar, controller.CarregarRanking);

            using (var l = new Ligador(controller)) l.Ref("firebaseManager", ctx.Firebase);

            using (var l = new Ligador(view))
            {
                l.Ref("controller", controller)
                 .Ref("firebaseManager", ctx.Firebase)
                 .Ref("container", conteudo)
                 .Ref("itemPrefab", ctx.Prefabs.ItemRanking)
                 .Ref("textoMensagem", mensagem);
            }
        }

        // --- Configurações ----------------------------------------------

        private static void MontarConfiguracoes(Contexto ctx)
        {
            RectTransform tela = NovaTela(ctx, TelaConfiguracoes);
            var controller = tela.gameObject.AddComponent<ConfiguracoesController>();
            var view = tela.gameObject.AddComponent<ConfiguracoesView>();

            RectTransform cabecalho = Cabecalho(tela, "Opções");
            BotaoVoltar(ctx, cabecalho, TelaMapa);

            RectTransform area = ManaUI.Vazio("Corpo", tela);
            area.anchorMin = Vector2.zero;
            area.anchorMax = Vector2.one;
            area.offsetMin = new Vector2(24f, 24f);
            area.offsetMax = new Vector2(-24f, -140f);
            ManaUI.Rolagem("Rolagem", area, out RectTransform conteudo);

            // --- Som e vibração ---
            ManaUI.SecaoComIcone("SecaoSom", conteudo, "SOM E VIBRAÇÃO", "settings");

            RectTransform musicaConteudo;
            ManaUI.CartaoOpcao("CartaoMusica", conteudo, "Música", "Volume da trilha sonora", "music", out musicaConteudo);
            Slider sliderMusica = ManaUI.Slider_("SliderMusica", musicaConteudo, 0.6f);
            ManaUI.Altura(sliderMusica.gameObject, 42f);
            Toggle toggleMusica = ManaUI.Toggle_("ToggleMusica", musicaConteudo, "Música ligada", true);
            ManaUI.Altura(toggleMusica.gameObject, 42f);

            RectTransform efeitosConteudo;
            ManaUI.CartaoOpcao("CartaoEfeitos", conteudo, "Efeitos sonoros", "Sons de combinações e poderes", "effects", out efeitosConteudo);
            Slider sliderEfeitos = ManaUI.Slider_("SliderEfeitos", efeitosConteudo, 0.8f);
            ManaUI.Altura(sliderEfeitos.gameObject, 42f);
            Toggle toggleEfeitos = ManaUI.Toggle_("ToggleEfeitos", efeitosConteudo, "Efeitos ligados", true);
            ManaUI.Altura(toggleEfeitos.gameObject, 42f);

            RectTransform vibracaoConteudo;
            ManaUI.CartaoOpcao("CartaoVibracao", conteudo, "Vibração", "Resposta tátil ao tocar nas peças", "vibration", out vibracaoConteudo);
            Toggle toggleVibracao = ManaUI.Toggle_("ToggleVibracao", vibracaoConteudo, "Vibração ligada", true);
            ManaUI.Altura(toggleVibracao.gameObject, 42f);

            Eventos.AoMudarFloat(sliderMusica, controller.DefinirVolumeMusica);
            Eventos.AoMudarFloat(sliderEfeitos, controller.DefinirVolumeEfeitos);
            Eventos.AoMudarBool(toggleMusica, controller.DefinirMusicaLigada);
            Eventos.AoMudarBool(toggleEfeitos, controller.DefinirEfeitosLigados);
            Eventos.AoMudarBool(toggleVibracao, controller.DefinirVibracaoAtiva);

            // --- Conta ---
            ManaUI.SecaoComIcone("SecaoConta", conteudo, "CONTA E PROGRESSO", "account");
            RectTransform contaConteudo;
            ManaUI.CartaoOpcao("CartaoConta", conteudo, "Conta", "Sincronize o progresso e participe do placar", "account", out contaConteudo);
            var statusConta = ManaUI.Texto("StatusConta", contaConteudo, "Jogando como convidado", 16f, TextAlignmentOptions.MidlineLeft, ManaUI.TextoFraco);
            ManaUI.Altura(statusConta.gameObject, 26f);
            Button vincular = ManaUI.Botao("BotaoVincularConta", contaConteudo, "Vincular conta Google", ManaUI.BotaoPrimario, 18f);
            ManaUI.Altura(vincular.gameObject, 58f);
            Eventos.AoClicar(vincular, controller.AbrirVinculoDeConta);

            RectTransform anunciosConteudo;
            ManaUI.CartaoOpcao("CartaoAnuncios", conteudo, "Anúncios", "Gerencie a experiência sem anúncios", "settings", out anunciosConteudo);
            var statusAnuncios = ManaUI.Texto("StatusAnuncios", anunciosConteudo, "Anúncios ativos", 16f, TextAlignmentOptions.MidlineLeft, ManaUI.TextoFraco);
            ManaUI.Altura(statusAnuncios.gameObject, 26f);
            Button removerAnuncios = ManaUI.Botao("BotaoRemoverAnuncios", anunciosConteudo, "Remover anúncios", ManaUI.BotaoSecundario, 18f);
            ManaUI.Altura(removerAnuncios.gameObject, 58f);
            Eventos.AoClicar(removerAnuncios, controller.ComprarRemoverAnuncios);

            // --- Privacidade ---
            ManaUI.SecaoComIcone("SecaoPrivacidade", conteudo, "MEUS DADOS", "privacy");
            RectTransform exportarConteudo;
            ManaUI.CartaoOpcao("CartaoExportar", conteudo, "Exportar dados", "Baixe uma cópia das informações do jogo", "export", out exportarConteudo);
            Button exportar = ManaUI.Botao("BotaoExportar", exportarConteudo, "Exportar meus dados", ManaUI.BotaoSecundario, 18f);
            ManaUI.Altura(exportar.gameObject, 58f);
            Eventos.AoClicar(exportar, controller.ExportarMeusDados);

            RectTransform excluirConteudo;
            ManaUI.CartaoOpcao("CartaoExcluir", conteudo, "Excluir dados", "Apague definitivamente conta e progresso", "delete", out excluirConteudo);
            Button excluir = ManaUI.Botao("BotaoExcluir", excluirConteudo, "Excluir conta e dados", ManaUI.Perigo, 18f);
            ManaUI.Altura(excluir.gameObject, 58f);
            Eventos.AoClicar(excluir, view.PedirConfirmacaoDeExclusao);

            var mensagem = ManaUI.Texto("Mensagem", conteudo, "", 20f, TextAlignmentOptions.Center, ManaUI.Perigo);
            ManaUI.Altura(mensagem.gameObject, 40f);

            // --- Modais ---
            GameObject painelDados = PainelDeDados(tela, view, out TextMeshProUGUI textoJson);
            GameObject painelExclusao = PainelDeExclusao(tela, view, controller);

            using (var l = new Ligador(controller))
            {
                l.Ref("privacyManager", ctx.Privacidade)
                 .Ref("purchaseManager", ctx.Compras)
                 .Ref("firebaseManager", ctx.Firebase)
                 .Ref("navigator", ctx.Navegador)
                 .Ref("audioManager", ctx.Audio)
                 .Ref("hapticsManager", ctx.Haptics)
                 .Texto("telaLogin", TelaLogin)
                 .Texto("telaSplash", TelaSplash);
            }

            using (var l = new Ligador(view))
            {
                l.Ref("controller", controller)
                 .Ref("sliderMusica", sliderMusica)
                 .Ref("sliderEfeitos", sliderEfeitos)
                 .Ref("toggleMusica", toggleMusica)
                 .Ref("toggleEfeitos", toggleEfeitos)
                 .Ref("toggleVibracao", toggleVibracao)
                 .Ref("textoStatusConta", statusConta)
                 .Ref("textoStatusAnuncios", statusAnuncios)
                 .Ref("botaoVincularConta", vincular)
                 .Ref("botaoRemoverAnuncios", removerAnuncios)
                 .Ref("painelDadosExportados", painelDados)
                 .Ref("textoDadosExportados", textoJson)
                 .Ref("painelConfirmarExclusao", painelExclusao)
                 .Ref("textoMensagem", mensagem);
            }
        }

        private static void Secao(RectTransform conteudo, string titulo)
        {
            var texto = ManaUI.Texto($"Secao_{titulo}", conteudo, titulo, 28f,
                TextAlignmentOptions.MidlineLeft, ManaUI.Dourado, FontStyles.Bold);
            ManaUI.Altura(texto.gameObject, 56f);
        }

        private static GameObject PainelDeDados(RectTransform tela, ConfiguracoesView view, out TextMeshProUGUI json)
        {
            RectTransform painel = ManaUI.Vazio("PainelDadosExportados", tela);
            ManaUI.Esticar(painel);
            var veu = ManaUI.Painel_("Veu", painel, new Color(0f, 0f, 0f, 0.8f));
            veu.transform.SetAsFirstSibling();

            RectTransform caixa = ManaUI.Vazio("Caixa", painel);
            caixa.anchorMin = new Vector2(0.06f, 0.15f);
            caixa.anchorMax = new Vector2(0.94f, 0.85f);
            caixa.offsetMin = Vector2.zero;
            caixa.offsetMax = Vector2.zero;
            ManaUI.PainelOrnamentado("Fundo", caixa, ManaUI.Painel);
            ManaUI.Coluna(caixa.gameObject, 14f, 24);

            var titulo = ManaUI.Texto("Titulo", caixa, "Seus dados", 30f, TextAlignmentOptions.Center, ManaUI.Dourado, FontStyles.Bold);
            ManaUI.Altura(titulo.gameObject, 44f);

            json = ManaUI.Texto("Json", caixa, "", 16f, TextAlignmentOptions.TopLeft, ManaUI.TextoFraco);

            Button fechar = ManaUI.Botao("BotaoFechar", caixa, "Fechar", ManaUI.BotaoSecundario, 22f);
            ManaUI.Altura(fechar.gameObject, 64f);
            Eventos.AoClicar(fechar, view.FecharDadosExportados);

            painel.gameObject.SetActive(false);
            return painel.gameObject;
        }

        private static GameObject PainelDeExclusao(RectTransform tela, ConfiguracoesView view, ConfiguracoesController controller)
        {
            RectTransform painel = ManaUI.Vazio("PainelConfirmarExclusao", tela);
            ManaUI.Esticar(painel);
            var veu = ManaUI.Painel_("Veu", painel, new Color(0f, 0f, 0f, 0.8f));
            veu.transform.SetAsFirstSibling();

            RectTransform caixa = ManaUI.Vazio("Caixa", painel);
            caixa.anchorMin = new Vector2(0.08f, 0.32f);
            caixa.anchorMax = new Vector2(0.92f, 0.68f);
            caixa.offsetMin = Vector2.zero;
            caixa.offsetMax = Vector2.zero;
            ManaUI.PainelOrnamentado("Fundo", caixa, ManaUI.Painel);
            ManaUI.Coluna(caixa.gameObject, 14f, 28);

            var aviso = ManaUI.Texto("Aviso", caixa,
                "Isso apaga sua conta e todo o progresso salvo na nuvem. Não dá para desfazer.",
                24f, TextAlignmentOptions.Center);

            Button confirmar = ManaUI.Botao("BotaoConfirmar", caixa, "Apagar tudo", ManaUI.Perigo, 22f);
            ManaUI.Altura(confirmar.gameObject, 64f);
            Eventos.AoClicar(confirmar, controller.ExcluirMinhaContaEDados);

            Button cancelar = ManaUI.Botao("BotaoCancelar", caixa, "Cancelar", ManaUI.BotaoSecundario, 22f);
            ManaUI.Altura(cancelar.gameObject, 64f);
            Eventos.AoClicar(cancelar, view.CancelarExclusao);

            painel.gameObject.SetActive(false);
            return painel.gameObject;
        }

        // --- Login ------------------------------------------------------

        private static void MontarLogin(Contexto ctx)
        {
            RectTransform tela = NovaTela(ctx, TelaLogin);
            var controller = tela.gameObject.AddComponent<LoginController>();
            var view = tela.gameObject.AddComponent<LoginView>();

            RectTransform cabecalho = Cabecalho(tela, "Sua conta");
            BotaoVoltar(ctx, cabecalho, TelaConfiguracoes);

            RectTransform corpo = ManaUI.Vazio("Corpo", tela);
            corpo.anchorMin = new Vector2(0.08f, 0.3f);
            corpo.anchorMax = new Vector2(0.92f, 0.75f);
            corpo.offsetMin = Vector2.zero;
            corpo.offsetMax = Vector2.zero;
            ManaUI.Coluna(corpo.gameObject, 20f, 0);

            var status = ManaUI.Texto("Status", corpo,
                "Você está jogando como convidado. Vincule uma conta para não perder o progresso.",
                24f, TextAlignmentOptions.Center);
            ManaUI.Altura(status.gameObject, 120f);

            Button google = ManaUI.Botao("BotaoGoogle", corpo, "Entrar com Google", ManaUI.BotaoPrimario);
            ManaUI.Altura(google.gameObject, 76f);

            var mensagem = ManaUI.Texto("Mensagem", corpo, "", 20f, TextAlignmentOptions.Center, ManaUI.Dourado);
            ManaUI.Altura(mensagem.gameObject, 60f);

            Eventos.AoClicar(google, view.EntrarComGoogle);

            using (var l = new Ligador(controller)) l.Ref("firebaseManager", ctx.Firebase);
            using (var l = new Ligador(ctx.Google)) l.Ref("loginController", controller);

            using (var l = new Ligador(view))
            {
                l.Ref("controller", controller)
                 .Ref("googleSignIn", ctx.Google)
                 .Ref("botaoGoogle", google)
                 .Ref("textoStatus", status)
                 .Ref("textoMensagem", mensagem);
            }
        }

        // ---------------------------------------------------------------
        // Ligações finais
        // ---------------------------------------------------------------

        private static void LigarNavegador(Contexto ctx)
        {
            var serializado = new SerializedObject(ctx.Navegador);
            SerializedProperty lista = serializado.FindProperty("telas");
            lista.arraySize = ctx.Telas.Count;

            for (int i = 0; i < ctx.Telas.Count; i++)
            {
                SerializedProperty entrada = lista.GetArrayElementAtIndex(i);
                entrada.FindPropertyRelative("Nome").stringValue = ctx.Telas[i].nome;
                entrada.FindPropertyRelative("Raiz").objectReferenceValue = ctx.Telas[i].raiz;
            }

            serializado.FindProperty("telaInicial").stringValue = TelaSplash;
            serializado.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Ligação feita no fim porque o HUD só existe depois que a Tela de
        /// Jogo é montada, e o Mapa é montado antes dela.
        /// </summary>
        private static void LigarMapaAoHud(Contexto ctx)
        {
            if (ctx.Mapa == null || ctx.Hud == null) return;

            using (var l = new Ligador(ctx.Mapa))
                l.Ref("hudController", ctx.Hud);

            using (var l = new Ligador(ctx.Hud))
                l.Ref("mapaController", ctx.Mapa);
        }
    }
}
