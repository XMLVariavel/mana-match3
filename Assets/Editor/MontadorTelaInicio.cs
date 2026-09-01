#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BibleMatch3.EditorTools
{
    /// <summary>
    /// Reconstrói SÓ o conteúdo de "TelaInicio" (dentro de [Maná] Canvas > Entrada... na
    /// verdade um irmão de Entrada, não filho dela). Não mexe em nenhuma outra tela —
    /// seguro rodar de novo quantas vezes quiser, sempre parte do zero só ali dentro.
    /// </summary>
    public static class MontadorTelaInicio
    {
        private const string PastaEntrada = "Assets/Art/UI/Entrada/";
        private const string PastaInicio = "Assets/Art/UI/Inicio/";
        private static TMP_FontAsset _fonteTematica;

        [MenuItem("Tools/Maná/Montar Tela Início")]
        public static void Montar()
        {
            GameObject telaInicio = EncontrarPorNomeIncluindoInativos("Inicio");
            if (telaInicio == null)
            {
                Debug.LogError("[Maná] Não achei o GameObject 'Inicio' na cena aberta. Abra a SampleScene primeiro.");
                return;
            }

            // Limpa tudo que já existe dentro dela (reconstrução do zero, idempotente).
            for (int i = telaInicio.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(telaInicio.transform.GetChild(i).gameObject);

            RectTransform raiz = telaInicio.GetComponent<RectTransform>();
            if (raiz == null) raiz = telaInicio.AddComponent<RectTransform>();

            Sprite fundo = Carregar<Sprite>(PastaInicio + "tela_inicio.PNG");
            Sprite logo = Carregar<Sprite>(PastaEntrada + "logo_jogo.png");
            Sprite rolo = Carregar<Sprite>(PastaInicio + "rolo_versiculo.png");
            Sprite barraMenu = Carregar<Sprite>(PastaInicio + "Bara de menu.PNG");
            Sprite iconeInicio = Carregar<Sprite>(PastaInicio + "icone_inicio.png");
            Sprite iconeJornada = Carregar<Sprite>(PastaInicio + "icone_jornada.png");
            Sprite iconeLoja = Carregar<Sprite>(PastaInicio + "icone_loja.png");
            Sprite iconePerfil = Carregar<Sprite>(PastaInicio + "icone_perfil.png");
            Sprite iconeOpcoes = Carregar<Sprite>(PastaInicio + "icone_opcoes.png");
            // A Cinzel-Regular SDF ainda não tem os caracteres acentuados do
            // português (á, é, ó, ç...) no atlas — usá-la agora troca letras
            // erradas no texto. Desligada até ser regenerada corretamente
            // (ver instruções). _fonteTematica = Carregar<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/Cinzel-Regular SDF.asset");

            // --- Fundo ---
            CriarImagemEsticada("Fundo", raiz, fundo);

            // --- Logo ---
            RectTransform rtLogo = CriarImagem("Logo", raiz, logo);
            Ancorar(rtLogo, new Vector2(0.12f, 0.86f), new Vector2(0.88f, 0.97f));

            // --- Painel do versículo ---
            RectTransform rtRolo = CriarImagem("PainelVersiculo", raiz, rolo);
            Ancorar(rtRolo, new Vector2(0.06f, 0.66f), new Vector2(0.94f, 0.85f));

            RectTransform rtTitulo = CriarTexto("TituloVersiculo", rtRolo, "VERSÍCULO DO DIA", 20, TextAlignmentOptions.Center, new Color(0.55f, 0.35f, 0.1f));
            Ancorar(rtTitulo, new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.9f));

            RectTransform rtVersiculo = CriarTexto("TextoVersiculo", rtRolo, "", 19, TextAlignmentOptions.Center, new Color(0.15f, 0.2f, 0.45f));
            Ancorar(rtVersiculo, new Vector2(0.08f, 0.2f), new Vector2(0.92f, 0.62f));

            RectTransform rtReferencia = CriarTexto("TextoReferencia", rtRolo, "", 18, TextAlignmentOptions.Center, new Color(0.15f, 0.2f, 0.45f));
            Ancorar(rtReferencia, new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.2f));
            rtReferencia.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            // --- Barra de menu (rodapé) ---
            RectTransform rtBarra = CriarImagem("BarraMenu", raiz, barraMenu);
            Ancorar(rtBarra, new Vector2(0f, 0f), new Vector2(1f, 0.12f));
            rtBarra.GetComponent<Image>().preserveAspect = false; // estica até as bordas, sem letterbox

            string[] nomes = { "Inicio", "Jornada", "Loja", "Perfil", "Opcoes" };
            string[] rotulos = { "INÍCIO", "JORNADA", "LOJA", "PERFIL", "OPÇÕES" };
            Sprite[] icones = { iconeInicio, iconeJornada, iconeLoja, iconePerfil, iconeOpcoes };

            for (int i = 0; i < nomes.Length; i++)
            {
                float x0 = 0.03f + i * 0.188f;
                float x1 = x0 + 0.17f;
                CriarBotaoDeMenu($"Botao{nomes[i]}", rtBarra, icones[i], rotulos[i], new Vector2(x0, 0.15f), new Vector2(x1, 0.85f));
            }

            // --- Serviço do versículo + View ---
            GameObject servicoGO = new GameObject("VersiculoDoDiaService");
            servicoGO.transform.SetParent(telaInicio.transform, false);
            VersiculoDoDiaService servico = servicoGO.AddComponent<VersiculoDoDiaService>();

            TelaInicioView view = telaInicio.AddComponent<TelaInicioView>();
            SerializedObject soView = new SerializedObject(view);
            soView.FindProperty("versiculoService").objectReferenceValue = servico;
            soView.FindProperty("textoVersiculo").objectReferenceValue = rtVersiculo.GetComponent<TextMeshProUGUI>();
            soView.FindProperty("textoReferencia").objectReferenceValue = rtReferencia.GetComponent<TextMeshProUGUI>();
            soView.ApplyModifiedPropertiesWithoutUndo();

            // --- Conecta os botões ao MapaDeFasesController já existente na cena ---
            // FindFirstObjectByType, por padrão, também ignora objetos desativados —
            // por isso o Include explícito aqui.
            MapaDeFasesController controller = Object.FindFirstObjectByType<MapaDeFasesController>(FindObjectsInactive.Include);
            if (controller != null)
            {
                LigarClique(raiz, "BotaoInicio", controller.AbrirInicio);
                LigarClique(raiz, "BotaoJornada", controller.AbrirJornada);
                LigarClique(raiz, "BotaoLoja", controller.AbrirLoja);
                LigarClique(raiz, "BotaoPerfil", controller.AbrirPerfil);
                LigarClique(raiz, "BotaoOpcoes", controller.AbrirConfiguracoes);
            }
            else
            {
                Debug.LogWarning("[Maná] Não achei um MapaDeFasesController na cena — os botões do menu foram criados mas sem ação de clique. Ligue manualmente no Inspector se precisar.");
            }

            EditorSceneManager.MarkSceneDirty(telaInicio.scene);
            Debug.Log("[Maná] Tela Início remontada com sucesso. Salve a cena (Ctrl+S).");
        }

        /// <summary>
        /// Busca um GameObject pelo nome em toda a hierarquia da cena aberta,
        /// incluindo objetos desativados — diferente de GameObject.Find, que
        /// ignora objetos inativos (e por isso falhava aqui: todas as telas
        /// exceto a exibida no momento ficam desativadas pelo ScreenNavigator).
        /// </summary>
        private static GameObject EncontrarPorNomeIncluindoInativos(string nome)
        {
            foreach (GameObject raizCena in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                Transform encontrado = BuscarRecursivo(raizCena.transform, nome);
                if (encontrado != null) return encontrado.gameObject;
            }
            return null;
        }

        private static Transform BuscarRecursivo(Transform atual, string nome)
        {
            if (atual.name == nome) return atual;
            for (int i = 0; i < atual.childCount; i++)
            {
                Transform resultado = BuscarRecursivo(atual.GetChild(i), nome);
                if (resultado != null) return resultado;
            }
            return null;
        }

        private static T Carregar<T>(string caminho) where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(caminho);
            if (asset == null)
                Debug.LogWarning($"[Maná] Não encontrei o arquivo em '{caminho}' — confira se ele já foi importado nesse caminho exato.");
            return asset;
        }

        private static void CriarImagemEsticada(string nome, Transform pai, Sprite sprite)
        {
            RectTransform rt = CriarImagem(nome, pai, sprite);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.GetComponent<Image>().preserveAspect = false;
        }

        private static RectTransform CriarImagem(string nome, Transform pai, Sprite sprite)
        {
            GameObject go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(pai, false);
            Image img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            return go.GetComponent<RectTransform>();
        }

        private static RectTransform CriarTexto(string nome, Transform pai, string texto, float tamanho, TextAlignmentOptions alinhamento, Color cor)
        {
            GameObject go = new GameObject(nome, typeof(RectTransform));
            go.transform.SetParent(pai, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = texto;
            tmp.fontSize = tamanho;
            tmp.alignment = alinhamento;
            tmp.color = cor;
            tmp.enableWordWrapping = true;
            if (_fonteTematica != null) tmp.font = _fonteTematica;
            return go.GetComponent<RectTransform>();
        }

        private static void CriarBotaoDeMenu(string nome, Transform pai, Sprite icone, string rotulo, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(pai, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            Ancorar(rt, anchorMin, anchorMax);

            Image imgFundo = go.GetComponent<Image>();
            imgFundo.color = new Color(1, 1, 1, 0); // fundo invisível, só a área de clique

            RectTransform rtIcone = CriarImagem("Icone", rt, icone);
            Ancorar(rtIcone, new Vector2(0.22f, 0.42f), new Vector2(0.78f, 0.98f));

            RectTransform rtRotulo = CriarTexto("Rotulo", rt, rotulo, 12, TextAlignmentOptions.Center, new Color(0.98f, 0.85f, 0.55f));
            Ancorar(rtRotulo, new Vector2(0f, 0.02f), new Vector2(1f, 0.36f));
            rtRotulo.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        }

        private static void LigarClique(RectTransform raizTela, string caminhoRelativo, UnityEngine.Events.UnityAction acao)
        {
            Transform alvo = raizTela.Find($"BarraMenu/{caminhoRelativo}");
            if (alvo == null)
            {
                Debug.LogWarning($"[Maná] Não achei '{caminhoRelativo}' para ligar o clique.");
                return;
            }

            Button botao = alvo.GetComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(botao.onClick, acao);
        }

        private static void Ancorar(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
#endif
