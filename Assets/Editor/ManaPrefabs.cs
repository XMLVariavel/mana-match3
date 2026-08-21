using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BibleMatch3.EditorTools
{
    /// <summary>
    /// Constrói os prefabs reutilizáveis e os salva em Assets/Prefabs/.
    ///
    /// Cada prefab é montado como GameObject temporário na cena, tem seus
    /// campos privados preenchidos via <see cref="Ligador"/>, é salvo com
    /// <see cref="PrefabUtility.SaveAsPrefabAsset"/> e então destruído — assim
    /// o arquivo .prefab é sempre gerado pela própria Unity, nunca escrito
    /// como YAML na mão.
    /// </summary>
    internal static class ManaPrefabs
    {
        public const string Pasta = "Assets/Prefabs";

        internal sealed class Conjunto
        {
            public GameObject Peca;
            public GameObject Obstaculo;
            public BotaoFaseUI BotaoFase;
            public ItemRankingUI ItemRanking;
            public ItemLojaUI ItemLoja;
            public ItemObjetivoUI ItemObjetivo;
        }

        public static Conjunto Gerar()
        {
            ManaAssets.GarantirPasta(Pasta);

            var conjunto = new Conjunto
            {
                Peca = CriarPeca(),
                Obstaculo = CriarObstaculo(),
                BotaoFase = CriarBotaoFase(),
                ItemRanking = CriarItemRanking(),
                ItemLoja = CriarItemLoja(),
                ItemObjetivo = CriarItemObjetivo()
            };

            AssetDatabase.SaveAssets();
            return conjunto;
        }

        // ---------------------------------------------------------------
        // Mundo (SpriteRenderer)
        // ---------------------------------------------------------------

        private static GameObject CriarPeca()
        {
            var raiz = new GameObject("Peca", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Tile));

            var renderizador = raiz.GetComponent<SpriteRenderer>();
            renderizador.sprite = ManaArte.Carregar(ManaArte.NomesDePecas[0]);
            renderizador.sortingOrder = 10;

            // O colisor é o que permite ao BoardManager identificar a peça
            // tocada via Physics2D.Raycast — sem ele, o toque não seleciona nada.
            var colisor = raiz.GetComponent<BoxCollider2D>();
            colisor.size = Vector2.one;
            colisor.isTrigger = true;

            using (var l = new Ligador(raiz.GetComponent<Tile>()))
                l.Ref("spriteRenderer", renderizador);

            return Salvar(raiz, "Peca");
        }

        private static GameObject CriarObstaculo()
        {
            var raiz = new GameObject("Obstaculo", typeof(SpriteRenderer), typeof(Obstacle));

            var renderizador = raiz.GetComponent<SpriteRenderer>();
            renderizador.sprite = ManaArte.Carregar(ManaArte.PedraDeserto);
            renderizador.sortingOrder = 20; // acima das peças

            using (var l = new Ligador(raiz.GetComponent<Obstacle>()))
                l.Ref("spriteRenderer", renderizador);

            return Salvar(raiz, "Obstaculo");
        }

        // ---------------------------------------------------------------
        // UI
        // ---------------------------------------------------------------

        private static BotaoFaseUI CriarBotaoFase()
        {
            var raiz = new GameObject("BotaoFase", typeof(RectTransform), typeof(Image), typeof(Button), typeof(BotaoFaseUI));
            var rt = (RectTransform)raiz.transform;
            rt.sizeDelta = new Vector2(150f, 170f);

            var imagem = raiz.GetComponent<Image>();
            imagem.sprite = ManaUI.SpriteUI;
            imagem.type = Image.Type.Sliced;
            imagem.color = ManaUI.Dourado;

            var botao = raiz.GetComponent<Button>();
            botao.targetGraphic = imagem;

            var numero = ManaUI.Texto("Numero", rt, "1", 46f, TextAlignmentOptions.Center, ManaUI.Fundo, FontStyles.Bold);
            numero.rectTransform.anchorMin = new Vector2(0f, 0.32f);
            numero.rectTransform.anchorMax = new Vector2(1f, 1f);
            numero.rectTransform.offsetMin = Vector2.zero;
            numero.rectTransform.offsetMax = Vector2.zero;

            // "Cadeado" como véu escuro por cima: legível como travado mesmo
            // sem ícone dedicado, e some sozinho quando a fase é liberada.
            var cadeado = new GameObject("Cadeado", typeof(RectTransform), typeof(Image));
            cadeado.transform.SetParent(rt, false);
            ManaUI.Esticar((RectTransform)cadeado.transform);
            var cadeadoImg = cadeado.GetComponent<Image>();
            cadeadoImg.sprite = ManaUI.SpriteUI;
            cadeadoImg.type = Image.Type.Sliced;
            cadeadoImg.color = new Color(0.05f, 0.04f, 0.03f, 0.62f);
            cadeadoImg.raycastTarget = false;

            var faixaEstrelas = ManaUI.Vazio("Estrelas", rt);
            faixaEstrelas.anchorMin = new Vector2(0f, 0f);
            faixaEstrelas.anchorMax = new Vector2(1f, 0.3f);
            faixaEstrelas.offsetMin = new Vector2(10f, 8f);
            faixaEstrelas.offsetMax = new Vector2(-10f, 0f);
            var layout = ManaUI.Linha(faixaEstrelas.gameObject, 6f);
            layout.childForceExpandWidth = false;
            layout.childControlWidth = false;

            Sprite spriteEstrela = ManaArte.Carregar(ManaArte.NomesDeEspeciais[(int)SpecialType.Estrela_Guia]);
            var estrelas = new Object[3];

            for (int i = 0; i < 3; i++)
            {
                var estrela = new GameObject($"Estrela{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                estrela.transform.SetParent(faixaEstrelas, false);
                ((RectTransform)estrela.transform).sizeDelta = new Vector2(30f, 30f);

                var img = estrela.GetComponent<Image>();
                img.sprite = spriteEstrela;
                img.preserveAspect = true;
                img.raycastTarget = false;
                estrelas[i] = img;
            }

            var componente = raiz.GetComponent<BotaoFaseUI>();
            using (var l = new Ligador(componente))
            {
                l.Ref("botao", botao)
                 .Ref("textoNumero", numero)
                 .Ref("iconeCadeado", cadeado)
                 .Lista("estrelas", estrelas);
            }

            return Salvar(raiz, "BotaoFase").GetComponent<BotaoFaseUI>();
        }

        private static ItemRankingUI CriarItemRanking()
        {
            var raiz = new GameObject("ItemRanking", typeof(RectTransform), typeof(Image), typeof(ItemRankingUI));
            var rt = (RectTransform)raiz.transform;
            rt.sizeDelta = new Vector2(600f, 64f);

            var fundo = raiz.GetComponent<Image>();
            fundo.sprite = ManaUI.SpriteUI;
            fundo.type = Image.Type.Sliced;
            fundo.color = ManaUI.Painel;

            ManaUI.Linha(raiz, 10f, 12);
            ManaUI.Altura(raiz, 64f);

            var posicao = ManaUI.Texto("Posicao", rt, "1º", 24f, TextAlignmentOptions.MidlineLeft, ManaUI.Dourado, FontStyles.Bold);
            ManaUI.Largura(posicao.gameObject, 70f);

            var nome = ManaUI.Texto("Nome", rt, "Peregrino", 24f, TextAlignmentOptions.MidlineLeft);

            var score = ManaUI.Texto("Score", rt, "0", 24f, TextAlignmentOptions.MidlineRight);
            ManaUI.Largura(score.gameObject, 130f);

            using (var l = new Ligador(raiz.GetComponent<ItemRankingUI>()))
                l.Ref("textoPosicao", posicao).Ref("textoNome", nome).Ref("textoScore", score);

            return Salvar(raiz, "ItemRanking").GetComponent<ItemRankingUI>();
        }

        private static ItemLojaUI CriarItemLoja()
        {
            var raiz = new GameObject("ItemLoja", typeof(RectTransform), typeof(Image), typeof(ItemLojaUI));
            var rt = (RectTransform)raiz.transform;
            rt.sizeDelta = new Vector2(600f, 150f);

            var fundo = raiz.GetComponent<Image>();
            fundo.sprite = ManaUI.SpriteUI;
            fundo.type = Image.Type.Sliced;
            fundo.color = ManaUI.Painel;
            ManaUI.Altura(raiz, 150f);

            var icone = new GameObject("Icone", typeof(RectTransform), typeof(Image));
            icone.transform.SetParent(rt, false);
            var iconeRt = (RectTransform)icone.transform;
            iconeRt.anchorMin = new Vector2(0f, 0.5f);
            iconeRt.anchorMax = new Vector2(0f, 0.5f);
            iconeRt.pivot = new Vector2(0f, 0.5f);
            iconeRt.anchoredPosition = new Vector2(16f, 0f);
            iconeRt.sizeDelta = new Vector2(100f, 100f);
            var iconeImg = icone.GetComponent<Image>();
            iconeImg.preserveAspect = true;
            iconeImg.raycastTarget = false;

            var nome = ManaUI.Texto("Nome", rt, "Poder", 26f, TextAlignmentOptions.TopLeft, ManaUI.Dourado, FontStyles.Bold);
            AncorarBloco(nome.rectTransform, 130f, 200f, 14f, 34f);

            var descricao = ManaUI.Texto("Descricao", rt, "Descrição do poder.", 20f, TextAlignmentOptions.TopLeft, ManaUI.TextoFraco);
            AncorarBloco(descricao.rectTransform, 130f, 200f, 52f, 56f);

            var nivel = ManaUI.Texto("Nivel", rt, "Nv. 1/3", 20f, TextAlignmentOptions.TopLeft);
            AncorarBloco(nivel.rectTransform, 130f, 200f, 112f, 28f);

            var botao = ManaUI.Botao("BotaoAcao", rt, "Evoluir", ManaUI.BotaoPrimario, 20f);
            var botaoRt = botao.GetComponent<RectTransform>();
            botaoRt.anchorMin = new Vector2(1f, 0.5f);
            botaoRt.anchorMax = new Vector2(1f, 0.5f);
            botaoRt.pivot = new Vector2(1f, 0.5f);
            botaoRt.anchoredPosition = new Vector2(-16f, 0f);
            botaoRt.sizeDelta = new Vector2(180f, 64f);
            var rotuloBotao = botao.GetComponentInChildren<TextMeshProUGUI>();

            using (var l = new Ligador(raiz.GetComponent<ItemLojaUI>()))
            {
                l.Ref("icone", iconeImg)
                 .Ref("textoNome", nome)
                 .Ref("textoDescricao", descricao)
                 .Ref("textoNivel", nivel)
                 .Ref("botaoAcao", botao)
                 .Ref("textoBotao", rotuloBotao);
            }

            return Salvar(raiz, "ItemLoja").GetComponent<ItemLojaUI>();
        }

        private static ItemObjetivoUI CriarItemObjetivo()
        {
            var raiz = new GameObject("ItemObjetivo", typeof(RectTransform), typeof(ItemObjetivoUI), typeof(LayoutElement));
            var rt = (RectTransform)raiz.transform;
            rt.sizeDelta = new Vector2(168f, 104f);
            var layoutItem = raiz.GetComponent<LayoutElement>();
            layoutItem.minWidth = 168f;
            layoutItem.preferredWidth = 168f;
            layoutItem.minHeight = 104f;
            layoutItem.preferredHeight = 104f;
            ManaUI.Largura(raiz, 168f);

            // O item mora em cima da faixa de pergaminho: um segundo painel
            // claro aqui empilharia dois fundos. Só o disco escuro atrás do
            // ícone, como na referência.
            var disco = new GameObject("Disco", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            disco.transform.SetParent(rt, false);
            var discoRt = (RectTransform)disco.transform;
            discoRt.anchorMin = new Vector2(0f, 0.5f);
            discoRt.anchorMax = new Vector2(0f, 0.5f);
            discoRt.pivot = new Vector2(0f, 0.5f);
            discoRt.anchoredPosition = new Vector2(6f, 0f);
            discoRt.sizeDelta = new Vector2(78f, 78f);
            var discoImg = disco.GetComponent<Image>();
            discoImg.sprite = ManaUI.SpriteKnob;
            discoImg.preserveAspect = true;
            discoImg.color = new Color(0.10f, 0.16f, 0.23f, 0.90f);
            discoImg.raycastTarget = false;

            var icone = new GameObject("Icone", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            icone.transform.SetParent(disco.transform, false);
            ManaUI.Esticar((RectTransform)icone.transform, 12f);
            var iconeImg = icone.GetComponent<Image>();
            iconeImg.preserveAspect = true;
            iconeImg.raycastTarget = false;

            var restante = ManaUI.Texto("Restante", rt, "0", 34f, TextAlignmentOptions.MidlineLeft,
                new Color(0.243f, 0.153f, 0.075f, 1f), FontStyles.Bold, false);
            restante.rectTransform.anchorMin = new Vector2(0f, 0f);
            restante.rectTransform.anchorMax = new Vector2(1f, 1f);
            restante.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            restante.rectTransform.offsetMin = new Vector2(94f, 0f);
            restante.rectTransform.offsetMax = new Vector2(-6f, 0f);

            using (var l = new Ligador(raiz.GetComponent<ItemObjetivoUI>()))
                l.Ref("icone", iconeImg).Ref("textoRestante", restante);

            return Salvar(raiz, "ItemObjetivo").GetComponent<ItemObjetivoUI>();
        }

        // ---------------------------------------------------------------
        // Infra
        // ---------------------------------------------------------------

        /// <summary>
        /// Ancora um bloco de texto no topo do item, medindo
        /// <paramref name="distanciaDoTopo"/> para baixo a partir da borda
        /// superior. Evita repetir o mesmo cálculo de offset em cada linha.
        /// </summary>
        private static void AncorarBloco(RectTransform rt, float esquerda, float direita, float distanciaDoTopo, float altura)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(esquerda, -distanciaDoTopo - altura);
            rt.offsetMax = new Vector2(-direita, -distanciaDoTopo);
        }

        private static GameObject Salvar(GameObject temporario, string nome)
        {
            string caminho = $"{Pasta}/{nome}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temporario, caminho);
            Object.DestroyImmediate(temporario);

            if (prefab == null) Debug.LogError($"[Maná] Falha ao salvar o prefab '{nome}'.");
            return prefab;
        }
    }
}
