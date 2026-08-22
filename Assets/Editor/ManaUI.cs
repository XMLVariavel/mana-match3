using TMPro;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BibleMatch3.EditorTools
{
    /// <summary>
    /// Fábrica de widgets de UI usada pelo montador. Existe para que
    /// <see cref="MontadorDeUI"/> descreva *o que* cada tela tem, sem se perder
    /// em cálculo de RectTransform — a hierarquia interna de Slider, Toggle e
    /// InputField (que a Unity normalmente cria pelo menu GameObject > UI)
    /// é reproduzida aqui na mão, uma vez só.
    /// </summary>
    internal static class ManaUI
    {
        // Paleta: azul-noturno, pergaminho, madeira e dourado queimado.
        public static readonly Color Fundo = new Color(0.024f, 0.071f, 0.118f, 1f);
        public static readonly Color Painel = new Color(0.055f, 0.125f, 0.184f, 0.97f);
        public static readonly Color PainelClaro = new Color(0.137f, 0.188f, 0.220f, 0.98f);
        public static readonly Color Dourado = new Color(0.949f, 0.714f, 0.255f, 1f);
        public static readonly Color TextoClaro = new Color(0.965f, 0.945f, 0.886f, 1f);
        public static readonly Color TextoFraco = new Color(0.690f, 0.757f, 0.776f, 1f);
        public static readonly Color BotaoPrimario = new Color(0.118f, 0.412f, 0.404f, 1f);
        public static readonly Color BotaoSecundario = new Color(0.110f, 0.188f, 0.247f, 1f);
        public static readonly Color Perigo = new Color(0.667f, 0.243f, 0.212f, 1f);
        public static readonly Color Transparente = new Color(0f, 0f, 0f, 0f);

        private static Sprite spriteUI;
        private static Sprite spriteFundoArredondado;
        private static Sprite spriteKnob;
        private static Sprite spriteCheck;

        public static Sprite SpriteUI => spriteUI != null
            ? spriteUI
            : spriteUI = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        public static Sprite SpriteFundo => spriteFundoArredondado != null
            ? spriteFundoArredondado
            : spriteFundoArredondado = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

        public static Sprite SpriteKnob => spriteKnob != null
            ? spriteKnob
            : spriteKnob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        public static Sprite SpriteCheck => spriteCheck != null
            ? spriteCheck
            : spriteCheck = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");

        // ---------------------------------------------------------------
        // Blocos básicos
        // ---------------------------------------------------------------

        public static RectTransform Vazio(string nome, Transform pai)
        {
            var go = new GameObject(nome, typeof(RectTransform));
            go.transform.SetParent(pai, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Ancora o retângulo aos quatro cantos do pai, sem margem.</summary>
        public static RectTransform Esticar(RectTransform rt, float margem = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(margem, margem);
            rt.offsetMax = new Vector2(-margem, -margem);
            return rt;
        }

        public static RectTransform Faixa(RectTransform rt, float alturaDoTopo, float altura)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(0f, -alturaDoTopo - altura);
            rt.offsetMax = new Vector2(0f, -alturaDoTopo);
            return rt;
        }

        public static RectTransform FaixaInferior(RectTransform rt, float alturaDaBase, float altura)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.offsetMin = new Vector2(0f, alturaDaBase);
            rt.offsetMax = new Vector2(0f, alturaDaBase + altura);
            return rt;
        }

        public static Image Painel_(string nome, Transform pai, Color cor, bool bloqueiaToque = true)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(pai, false);
            Esticar((RectTransform)go.transform);

            var img = go.GetComponent<Image>();
            img.sprite = SpriteUI;
            img.type = Image.Type.Sliced;
            img.color = cor;
            img.raycastTarget = bloqueiaToque;
            return img;
        }

        /// <summary>
        /// Cria o fundo ilustrado das telas. Se a arte ainda não tiver sido
        /// reimportada pelo menu do Editor, cai automaticamente no painel sólido.
        /// </summary>
        public static Image PainelCabecalho(string nome, Transform pai, Color fallback, bool bloqueiaToque = true)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(pai, false);
            Esticar((RectTransform)go.transform);

            var img = go.GetComponent<Image>();
            Sprite ornamento = ManaArte.Carregar("card_panel");
            img.sprite = ornamento != null ? ornamento : SpriteUI;
            img.type = Image.Type.Sliced;
            img.preserveAspect = false;
            img.color = ornamento != null ? Color.white : fallback;
            img.raycastTarget = bloqueiaToque;
            return img;
        }

        public static Image PainelOrnamentado(string nome, Transform pai, Color fallback, bool bloqueiaToque = true)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(pai, false);
            Esticar((RectTransform)go.transform);

            var img = go.GetComponent<Image>();
            Sprite ornamento = ManaArte.Carregar("card_panel");
            img.sprite = ornamento != null ? ornamento : SpriteUI;
            img.type = Image.Type.Sliced;
            img.preserveAspect = false;
            img.color = ornamento != null ? Color.white : fallback;
            img.raycastTarget = bloqueiaToque;
            return img;
        }

        public static Image PainelEstatistica(string nome, Transform pai, Color fallback, bool bloqueiaToque = true)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(pai, false);
            Esticar((RectTransform)go.transform);

            var img = go.GetComponent<Image>();
            Sprite painel = ManaArte.Carregar("stat_card_panel");
            img.sprite = painel != null ? painel : SpriteUI;
            img.type = Image.Type.Sliced;
            img.preserveAspect = false;
            img.color = painel != null ? Color.white : fallback;
            img.raycastTarget = bloqueiaToque;
            return img;
        }

        public static Image PainelNavegacao(string nome, Transform pai, Color fallback, bool bloqueiaToque = true)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(pai, false);
            Esticar((RectTransform)go.transform);

            var img = go.GetComponent<Image>();
            Sprite ornamento = ManaArte.Carregar("bottom_navigation");
            img.sprite = ornamento != null ? ornamento : SpriteUI;
            img.type = Image.Type.Sliced;
            img.preserveAspect = false;
            img.color = ornamento != null ? Color.white : fallback;
            img.raycastTarget = bloqueiaToque;
            return img;
        }

        public static Image PainelPergaminho(string nome, Transform pai, bool bloqueiaToque = false)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(pai, false);
            Esticar((RectTransform)go.transform);

            var img = go.GetComponent<Image>();
            img.sprite = SpriteFundo;
            img.type = Image.Type.Sliced;
            img.color = new Color(0.86f, 0.69f, 0.43f, 0.98f);
            img.raycastTarget = bloqueiaToque;
            return img;
        }

        /// <summary>
        /// Faixa de pergaminho aberta, com as pontas roladas preservadas pelo
        /// 9-slice horizontal. Usada na barra de objetivos da fase.
        /// </summary>
        public static Image FaixaDePergaminho(string nome, Transform pai, bool bloqueiaToque = false)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(pai, false);
            Esticar((RectTransform)go.transform);

            var img = go.GetComponent<Image>();
            Sprite faixa = ManaArte.Carregar(ManaArte.FaixaPergaminho);
            img.sprite = faixa != null ? faixa : SpriteFundo;
            img.type = Image.Type.Sliced;
            img.preserveAspect = false;
            img.color = faixa != null ? Color.white : new Color(0.86f, 0.69f, 0.43f, 0.98f);
            img.raycastTarget = bloqueiaToque;
            return img;
        }

        /// <summary>Imagem decorativa simples, esticada no pai e sem captar toque.</summary>
        public static Image Arte(string nome, Transform pai, string sprite, bool preservarProporcao = true)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(pai, false);
            Esticar((RectTransform)go.transform);

            var img = go.GetComponent<Image>();
            Sprite arte = ManaArte.Carregar(sprite);
            img.sprite = arte;
            img.type = Image.Type.Simple;
            img.preserveAspect = preservarProporcao;
            img.color = arte != null ? Color.white : Transparente;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>
        /// Botão cuja aparência vem inteira de um sprite recortado (seta de
        /// voltar, medalhão de poder). Sem rótulo: o desenho já diz tudo.
        /// </summary>
        public static Button BotaoDeArte(string nome, Transform pai, string sprite, Color fallback)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(pai, false);

            var img = go.GetComponent<Image>();
            Sprite arte = ManaArte.Carregar(sprite);
            img.sprite = arte != null ? arte : SpriteUI;
            img.type = arte != null ? Image.Type.Simple : Image.Type.Sliced;
            img.preserveAspect = arte != null;
            img.color = arte != null ? Color.white : fallback;

            var botao = go.GetComponent<Button>();
            botao.targetGraphic = img;
            var cores = botao.colors;
            cores.normalColor = Color.white;
            cores.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
            cores.pressedColor = new Color(0.76f, 0.76f, 0.76f, 1f);
            cores.fadeDuration = 0.08f;
            botao.colors = cores;
            return botao;
        }

        public static Image PainelIlustrado(string nome, Transform pai, Color fallback, bool bloqueiaToque = false)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(pai, false);
            Esticar((RectTransform)go.transform);

            var img = go.GetComponent<Image>();
            Sprite ilustracao = ManaArte.Carregar(ManaArte.FundoCelestial);
            if (ilustracao == null) ilustracao = ManaArte.Carregar(ManaArte.FundoJornada);
            img.sprite = ilustracao != null ? ilustracao : SpriteUI;
            img.type = ilustracao != null ? Image.Type.Simple : Image.Type.Sliced;
            img.preserveAspect = false;
            img.color = ilustracao != null ? Color.white : fallback;
            img.raycastTarget = bloqueiaToque;
            return img;
        }

        public static TextMeshProUGUI Texto(
            string nome,
            Transform pai,
            string conteudo,
            float tamanho = 28f,
            TextAlignmentOptions alinhamento = TextAlignmentOptions.Center,
            Color? cor = null,
            FontStyles estilo = FontStyles.Normal,
            bool cortar = true)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(pai, false);
            Esticar((RectTransform)go.transform);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = conteudo;
            tmp.fontSize = tamanho;
            tmp.alignment = alinhamento;
            tmp.color = cor ?? TextoClaro;
            tmp.fontStyle = estilo;
            tmp.raycastTarget = false;
            // Truncate esconde a linha inteira quando o retângulo é mais baixo
            // que a altura da fonte — por isso títulos grandes pedem Overflow.
            tmp.overflowMode = cortar ? TextOverflowModes.Truncate : TextOverflowModes.Overflow;

            if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
            return tmp;
        }

        public static RectTransform IconeOpcao(string nome, Transform pai, string tipo, float tamanho = 64f)
        {
            RectTransform raiz = Vazio(nome, pai);
            raiz.sizeDelta = new Vector2(tamanho, tamanho);
            var layout = raiz.gameObject.AddComponent<LayoutElement>();
            layout.minWidth = tamanho;
            layout.preferredWidth = tamanho;
            layout.minHeight = tamanho;
            layout.preferredHeight = tamanho;
            Image halo = Forma("Halo", raiz, new Vector2(tamanho, tamanho), Vector2.zero,
                new Color(0.045f, 0.19f, 0.23f, 0.98f), true);
            halo.raycastTarget = false;

            Color dourado = Dourado;
            switch (tipo)
            {
                case "music":
                    Forma("Nota", raiz, new Vector2(18f, 18f), new Vector2(-5f, -8f), dourado, true);
                    Forma("Haste", raiz, new Vector2(6f, 30f), new Vector2(6f, 7f), dourado, false);
                    Forma("Barra", raiz, new Vector2(22f, 6f), new Vector2(8f, 20f), dourado, false, -8f);
                    break;
                case "effects":
                    Forma("Caixa", raiz, new Vector2(16f, 20f), new Vector2(-10f, 0f), dourado, false);
                    Forma("Cone", raiz, new Vector2(9f, 27f), new Vector2(1f, 0f), dourado, false, 45f);
                    Forma("Onda1", raiz, new Vector2(5f, 20f), new Vector2(14f, 0f), dourado, false);
                    Forma("Onda2", raiz, new Vector2(4f, 30f), new Vector2(23f, 0f), dourado, false);
                    break;
                case "vibration":
                    Forma("Telefone", raiz, new Vector2(20f, 32f), Vector2.zero, dourado, false);
                    Forma("OndaEsq", raiz, new Vector2(4f, 24f), new Vector2(-19f, 0f), dourado, false, -8f);
                    Forma("OndaDir", raiz, new Vector2(4f, 24f), new Vector2(19f, 0f), dourado, false, 8f);
                    break;
                case "account":
                    Forma("Cabeca", raiz, new Vector2(18f, 18f), new Vector2(0f, 11f), dourado, true);
                    Forma("Ombros", raiz, new Vector2(34f, 18f), new Vector2(0f, -12f), dourado, true);
                    break;
                case "privacy":
                    Forma("Escudo", raiz, new Vector2(28f, 34f), Vector2.zero, dourado, false, 45f);
                    Forma("Centro", raiz, new Vector2(8f, 16f), new Vector2(0f, -2f), new Color(0.04f, 0.15f, 0.20f, 1f), false);
                    break;
                case "export":
                    Forma("Folha", raiz, new Vector2(27f, 34f), new Vector2(0f, -2f), dourado, false);
                    Forma("Seta", raiz, new Vector2(7f, 26f), new Vector2(0f, 11f), TextoClaro, false);
                    Forma("Base", raiz, new Vector2(24f, 6f), new Vector2(0f, -16f), TextoClaro, false);
                    break;
                case "delete":
                    Forma("Lixeira", raiz, new Vector2(25f, 28f), new Vector2(0f, -3f), dourado, false);
                    Forma("Tampa", raiz, new Vector2(33f, 5f), new Vector2(0f, 14f), dourado, false);
                    Forma("Alca", raiz, new Vector2(12f, 5f), new Vector2(0f, 20f), dourado, false);
                    break;
                case "home":
                    Forma("Telhado", raiz, new Vector2(28f, 22f), new Vector2(0f, 9f), dourado, false, 45f);
                    Forma("Casa", raiz, new Vector2(26f, 22f), new Vector2(0f, -7f), dourado, false);
                    Forma("Porta", raiz, new Vector2(7f, 12f), new Vector2(0f, -12f), new Color(0.04f, 0.15f, 0.20f, 1f), false);
                    break;
                case "journey":
                    Forma("Caminho", raiz, new Vector2(8f, 34f), Vector2.zero, dourado, false, 18f);
                    Forma("Estrela", raiz, new Vector2(20f, 20f), new Vector2(0f, 10f), dourado, false, 45f);
                    break;
                case "challenge":
                    Forma("Escudo", raiz, new Vector2(28f, 34f), Vector2.zero, dourado, false, 45f);
                    Forma("Cruz", raiz, new Vector2(5f, 20f), Vector2.zero, TextoClaro, false);
                    Forma("Cruz2", raiz, new Vector2(18f, 5f), new Vector2(0f, 3f), TextoClaro, false);
                    break;
                case "shop":
                    Forma("Saco", raiz, new Vector2(30f, 24f), new Vector2(0f, -4f), dourado, false);
                    Forma("Alca", raiz, new Vector2(16f, 8f), new Vector2(0f, 13f), dourado, false);
                    break;
                case "settings":
                    for (int i = 0; i < 8; i++)
                    {
                        float angulo = i * 45f;
                        Forma("Dente" + i, raiz, new Vector2(8f, 18f),
                            new Vector2(Mathf.Sin(angulo * Mathf.Deg2Rad) * 19f, Mathf.Cos(angulo * Mathf.Deg2Rad) * 19f), dourado, false, -angulo);
                    }
                    Forma("Centro", raiz, new Vector2(22f, 22f), Vector2.zero, dourado, true);
                    Forma("Furo", raiz, new Vector2(9f, 9f), Vector2.zero, new Color(0.045f, 0.19f, 0.23f, 1f), true);
                    break;
                default:
                    Forma("Marca", raiz, new Vector2(26f, 26f), Vector2.zero, dourado, true);
                    break;
            }
            return raiz;
        }

        private static Image Forma(string nome, Transform pai, Vector2 tamanho, Vector2 posicao, Color cor, bool circular, float rotacao = 0f)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(pai, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = tamanho;
            rt.anchoredPosition = posicao;
            rt.localRotation = Quaternion.Euler(0f, 0f, rotacao);
            var img = go.GetComponent<Image>();
            img.sprite = circular ? SpriteKnob : SpriteUI;
            img.type = circular ? Image.Type.Simple : Image.Type.Sliced;
            img.color = cor;
            img.raycastTarget = false;
            return img;
        }

        public static RectTransform SecaoComIcone(string nome, Transform pai, string titulo, string tipo)
        {
            RectTransform linha = Vazio(nome, pai);
            Altura(linha.gameObject, 64f);
            IconeOpcao("Icone", linha, tipo, 46f).anchoredPosition = new Vector2(28f, 0f);
            var texto = Texto("Titulo", linha, titulo, 25f, TextAlignmentOptions.MidlineLeft, Dourado, FontStyles.Bold);
            texto.rectTransform.anchorMin = new Vector2(0f, 0f);
            texto.rectTransform.anchorMax = new Vector2(1f, 1f);
            texto.rectTransform.offsetMin = new Vector2(72f, 0f);
            texto.rectTransform.offsetMax = new Vector2(-8f, 0f);
            return linha;
        }

        public static RectTransform CartaoOpcao(string nome, Transform pai, string titulo, string descricao, string tipo, out RectTransform conteudo)
        {
            RectTransform cartao = Vazio(nome, pai);
            Altura(cartao.gameObject, 192f);
            PainelOrnamentado("Fundo", cartao, Painel, false).transform.SetAsFirstSibling();
            RectTransform icone = IconeOpcao("Icone", cartao, tipo, 64f);
            icone.anchorMin = new Vector2(0f, 0.5f);
            icone.anchorMax = new Vector2(0f, 0.5f);
            icone.pivot = new Vector2(0f, 0.5f);
            icone.anchoredPosition = new Vector2(20f, 0f);

            conteudo = Vazio("Conteudo", cartao);
            conteudo.anchorMin = new Vector2(0f, 0f);
            conteudo.anchorMax = new Vector2(1f, 1f);
            conteudo.offsetMin = new Vector2(102f, 10f);
            conteudo.offsetMax = new Vector2(-18f, -10f);
            var coluna = Coluna(conteudo.gameObject, 2f, 0);
            coluna.padding = new RectOffset(0, 0, 0, 0);
            coluna.childForceExpandHeight = false;
            var tituloTmp = Texto("Titulo", conteudo, titulo, 22f, TextAlignmentOptions.MidlineLeft, Dourado, FontStyles.Bold);
            Altura(tituloTmp.gameObject, 30f);
            if (!string.IsNullOrWhiteSpace(descricao))
            {
                var detalhe = Texto("Descricao", conteudo, descricao, 15f, TextAlignmentOptions.TopLeft, TextoFraco);
                detalhe.enableWordWrapping = true;
                Altura(detalhe.gameObject, 28f);
            }
            return cartao;
        }

        public static Button Botao(string nome, Transform pai, string rotulo, Color cor, float tamanhoFonte = 26f)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(pai, false);

            var img = go.GetComponent<Image>();
            Sprite arteBotao = null;
            if (cor == BotaoPrimario) arteBotao = ManaArte.Carregar("button_primary");
            else if (cor == BotaoSecundario) arteBotao = ManaArte.Carregar("button_secondary");
            img.sprite = arteBotao != null ? arteBotao : SpriteUI;
            img.type = Image.Type.Sliced;
            img.preserveAspect = false;
            img.color = arteBotao != null ? Color.white : cor;

            var botao = go.GetComponent<Button>();
            botao.targetGraphic = img;

            var cores = botao.colors;
            cores.highlightedColor = Color.Lerp(cor, Color.white, 0.15f);
            cores.pressedColor = Color.Lerp(cor, Color.black, 0.2f);
            cores.disabledColor = new Color(cor.r, cor.g, cor.b, 0.35f);
            cores.fadeDuration = 0.08f;
            botao.colors = cores;

            string tipoIcone = IconeDoBotao(nome);
            if (!string.IsNullOrEmpty(tipoIcone))
            {
                RectTransform icone = IconeOpcao("Icone", go.transform, tipoIcone, Mathf.Min(46f, tamanhoFonte + 18f));
                icone.anchorMin = new Vector2(0.5f, 1f);
                icone.anchorMax = new Vector2(0.5f, 1f);
                icone.pivot = new Vector2(0.5f, 1f);
                icone.anchoredPosition = new Vector2(0f, -6f);

                var label = Texto("Rotulo", go.transform, rotulo, Mathf.Min(tamanhoFonte, 18f), TextAlignmentOptions.Center, TextoClaro, FontStyles.Bold);
                label.rectTransform.anchorMin = new Vector2(0.02f, 0f);
                label.rectTransform.anchorMax = new Vector2(0.98f, 0.42f);
                label.rectTransform.offsetMin = Vector2.zero;
                label.rectTransform.offsetMax = Vector2.zero;
                label.enableAutoSizing = true;
                label.fontSizeMin = 10f;
                label.fontSizeMax = Mathf.Min(tamanhoFonte, 18f);
            }
            else
            {
                Texto("Rotulo", go.transform, rotulo, tamanhoFonte);
            }
            return botao;
        }

        public static Button BotaoLink(string nome, Transform pai, string rotulo, float tamanhoFonte = 16f)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(pai, false);
            var imagem = go.GetComponent<Image>();
            imagem.color = Transparente;
            imagem.raycastTarget = true;

            var botao = go.GetComponent<Button>();
            botao.targetGraphic = imagem;
            var cores = botao.colors;
            cores.normalColor = Color.white;
            cores.highlightedColor = new Color(1f, 0.88f, 0.42f, 1f);
            cores.pressedColor = new Color(0.82f, 0.65f, 0.20f, 1f);
            cores.fadeDuration = 0.08f;
            botao.colors = cores;

            var texto = Texto("Texto", go.transform, rotulo, tamanhoFonte,
                TextAlignmentOptions.Center, Dourado, FontStyles.Underline);
            texto.enableAutoSizing = true;
            texto.fontSizeMin = 11f;
            texto.fontSizeMax = tamanhoFonte;
            texto.rectTransform.anchorMin = Vector2.zero;
            texto.rectTransform.anchorMax = Vector2.one;
            texto.rectTransform.offsetMin = Vector2.zero;
            texto.rectTransform.offsetMax = Vector2.zero;
            return botao;
        }

        private static string IconeDoBotao(string nome)
        {
            if (nome.IndexOf("Inicio", StringComparison.OrdinalIgnoreCase) >= 0) return "home";
            if (nome.IndexOf("Jornada", StringComparison.OrdinalIgnoreCase) >= 0) return "journey";
            if (nome.IndexOf("Desafios", StringComparison.OrdinalIgnoreCase) >= 0) return "challenge";
            if (nome.IndexOf("Loja", StringComparison.OrdinalIgnoreCase) >= 0) return "shop";
            if (nome.IndexOf("Perfil", StringComparison.OrdinalIgnoreCase) >= 0) return "account";
            if (nome.IndexOf("Opcoes", StringComparison.OrdinalIgnoreCase) >= 0 || nome.IndexOf("Opções", StringComparison.OrdinalIgnoreCase) >= 0) return "settings";
            if (nome.IndexOf("Musica", StringComparison.OrdinalIgnoreCase) >= 0) return "music";
            if (nome.IndexOf("Efeitos", StringComparison.OrdinalIgnoreCase) >= 0) return "effects";
            if (nome.IndexOf("Vibracao", StringComparison.OrdinalIgnoreCase) >= 0) return "vibration";
            if (nome.IndexOf("Vincular", StringComparison.OrdinalIgnoreCase) >= 0) return "account";
            if (nome.IndexOf("Exportar", StringComparison.OrdinalIgnoreCase) >= 0) return "export";
            if (nome.IndexOf("Excluir", StringComparison.OrdinalIgnoreCase) >= 0) return "delete";
            return null;
        }

        public static Button BotaoPoder(
            string nome,
            Transform pai,
            string titulo,
            string detalhe,
            string simbolo,
            Color corIcone)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(pai, false);

            var rt = (RectTransform)go.transform;
            var layout = go.GetComponent<LayoutElement>();
            layout.minHeight = 112f;
            layout.preferredHeight = 112f;
            layout.minWidth = 0f;
            layout.preferredWidth = 300f;
            layout.flexibleWidth = 1f;

            var fundo = go.GetComponent<Image>();
            Sprite artePoder = ManaArte.Carregar("button_secondary");
            fundo.sprite = artePoder != null ? artePoder : SpriteUI;
            fundo.type = Image.Type.Sliced;
            fundo.color = artePoder != null ? Color.white : new Color(0.10f, 0.15f, 0.20f, 0.98f);

            var botao = go.GetComponent<Button>();
            botao.targetGraphic = fundo;
            var cores = botao.colors;
            cores.normalColor = Color.white;
            cores.highlightedColor = new Color(1f, 1f, 1f, 0.96f);
            cores.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            cores.fadeDuration = 0.08f;
            botao.colors = cores;

            var circulo = new GameObject("Icone", typeof(RectTransform), typeof(Image));
            circulo.transform.SetParent(go.transform, false);
            var circuloRt = (RectTransform)circulo.transform;
            circuloRt.anchorMin = new Vector2(0f, 0.5f);
            circuloRt.anchorMax = new Vector2(0f, 0.5f);
            circuloRt.pivot = new Vector2(0f, 0.5f);
            circuloRt.anchoredPosition = new Vector2(14f, 0f);
            circuloRt.sizeDelta = new Vector2(86f, 86f);
            var circuloImg = circulo.GetComponent<Image>();
            circuloImg.sprite = SpriteKnob;
            circuloImg.type = Image.Type.Simple;
            circuloImg.preserveAspect = true;
            circuloImg.color = corIcone;
            circuloImg.raycastTarget = false;

            var badge = new GameObject("Badge", typeof(RectTransform), typeof(Image));
            badge.transform.SetParent(go.transform, false);
            var badgeRt = (RectTransform)badge.transform;
            badgeRt.anchorMin = new Vector2(0f, 0f);
            badgeRt.anchorMax = new Vector2(0f, 0f);
            badgeRt.pivot = new Vector2(0f, 0f);
            badgeRt.anchoredPosition = new Vector2(64f, 8f);
            badgeRt.sizeDelta = new Vector2(28f, 28f);
            var badgeImg = badge.GetComponent<Image>();
            badgeImg.sprite = SpriteKnob;
            badgeImg.preserveAspect = true;
            badgeImg.color = Dourado;
            badgeImg.raycastTarget = false;
            var badgeTexto = Texto("Numero", badge.transform, "3", 14f, TextAlignmentOptions.Center, Fundo, FontStyles.Bold);
            badgeTexto.raycastTarget = false;

            string nomeIcone = titulo.Contains("MARTELO") ? "power_hammer" :
                titulo.Contains("EMBARALHAR") ? "power_shuffle" : "power_plus5";
            Sprite artePower = ManaArte.Carregar(nomeIcone);
            if (artePower != null)
            {
                var arteGo = new GameObject("Arte", typeof(RectTransform), typeof(Image));
                arteGo.transform.SetParent(circulo.transform, false);
                var arteRt = (RectTransform)arteGo.transform;
                Esticar(arteRt, 10f);
                var arteImg = arteGo.GetComponent<Image>();
                arteImg.sprite = artePower;
                arteImg.preserveAspect = true;
                arteImg.raycastTarget = false;
            }

            var simboloTmp = Texto("Simbolo", circulo.transform, simbolo, 31f, TextAlignmentOptions.Center, TextoClaro, FontStyles.Bold);
            simboloTmp.raycastTarget = false;
            simboloTmp.gameObject.SetActive(artePower == null);

            var tituloTmp = Texto("Titulo", go.transform, titulo, 18f, TextAlignmentOptions.MidlineLeft, Dourado, FontStyles.Bold);
            tituloTmp.rectTransform.anchorMin = new Vector2(0.34f, 0.52f);
            tituloTmp.rectTransform.anchorMax = new Vector2(0.98f, 0.84f);
            tituloTmp.rectTransform.offsetMin = Vector2.zero;
            tituloTmp.rectTransform.offsetMax = Vector2.zero;
            tituloTmp.enableAutoSizing = true;
            tituloTmp.fontSizeMin = 13f;
            tituloTmp.fontSizeMax = 18f;

            var detalheTmp = Texto("Detalhe", go.transform, detalhe, 14f, TextAlignmentOptions.TopLeft, TextoFraco);
            detalheTmp.rectTransform.anchorMin = new Vector2(0.34f, 0.12f);
            detalheTmp.rectTransform.anchorMax = new Vector2(0.98f, 0.48f);
            detalheTmp.rectTransform.offsetMin = Vector2.zero;
            detalheTmp.rectTransform.offsetMax = Vector2.zero;
            detalheTmp.enableAutoSizing = true;
            detalheTmp.fontSizeMin = 10f;
            detalheTmp.fontSizeMax = 14f;
            detalheTmp.enableWordWrapping = true;

            return botao;
        }

        /// <summary>
        /// Botão de poder no formato da referência: medalhão circular dourado
        /// com o ícone dentro, contador em cima e rótulo em duas linhas abaixo.
        /// </summary>
        public static Button BotaoPoderCircular(
            string nome,
            Transform pai,
            string titulo,
            string detalhe,
            string simbolo,
            string icone,
            Color corIcone,
            out TextMeshProUGUI contador)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(pai, false);

            var layout = go.GetComponent<LayoutElement>();
            layout.minWidth = 0f;
            layout.preferredWidth = 220f;
            layout.flexibleWidth = 1f;

            // O alvo de toque é a coluna inteira, mas invisível: quem aparece
            // é o medalhão desenhado logo abaixo.
            var area = go.GetComponent<Image>();
            area.color = Transparente;
            area.raycastTarget = true;

            var botao = go.GetComponent<Button>();

            RectTransform medalhao = Vazio("Medalhao", go.transform);
            medalhao.anchorMin = new Vector2(0.5f, 1f);
            medalhao.anchorMax = new Vector2(0.5f, 1f);
            medalhao.pivot = new Vector2(0.5f, 1f);
            medalhao.anchoredPosition = new Vector2(0f, 0f);
            medalhao.sizeDelta = new Vector2(134f, 134f);

            Image fundoMedalhao = Arte("Fundo", medalhao, ManaArte.BotaoCircular);
            if (fundoMedalhao.sprite == null)
            {
                fundoMedalhao.sprite = SpriteKnob;
                fundoMedalhao.color = corIcone;
            }
            botao.targetGraphic = fundoMedalhao;
            botao.transition = Selectable.Transition.ColorTint;
            var cores = botao.colors;
            cores.normalColor = Color.white;
            cores.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
            cores.pressedColor = new Color(0.74f, 0.74f, 0.74f, 1f);
            cores.fadeDuration = 0.08f;
            botao.colors = cores;

            Sprite artePoder = ManaArte.Carregar(icone);
            if (artePoder != null)
            {
                Image arte = Arte("Icone", medalhao, icone);
                Esticar(arte.rectTransform, 30f);
            }
            else
            {
                var simboloTmp = Texto("Simbolo", medalhao, simbolo, 46f, TextAlignmentOptions.Center, TextoClaro, FontStyles.Bold, false);
                Esticar(simboloTmp.rectTransform, 22f);
            }

            RectTransform selo = Vazio("Contador", medalhao);
            selo.anchorMin = new Vector2(1f, 1f);
            selo.anchorMax = new Vector2(1f, 1f);
            selo.pivot = new Vector2(1f, 1f);
            selo.anchoredPosition = new Vector2(4f, 4f);
            selo.sizeDelta = new Vector2(48f, 48f);
            var seloImg = new GameObject("Fundo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            seloImg.transform.SetParent(selo, false);
            Esticar(seloImg.rectTransform);
            seloImg.sprite = SpriteKnob;
            seloImg.preserveAspect = true;
            seloImg.color = Dourado;
            seloImg.raycastTarget = false;
            contador = Texto("Numero", selo, "3", 24f, TextAlignmentOptions.Center, Fundo, FontStyles.Bold, false);

            var tituloTmp = Texto("Titulo", go.transform, titulo, 24f, TextAlignmentOptions.Center, Dourado, FontStyles.Bold, false);
            tituloTmp.rectTransform.anchorMin = new Vector2(0f, 1f);
            tituloTmp.rectTransform.anchorMax = new Vector2(1f, 1f);
            tituloTmp.rectTransform.pivot = new Vector2(0.5f, 1f);
            tituloTmp.rectTransform.offsetMin = new Vector2(0f, -172f);
            tituloTmp.rectTransform.offsetMax = new Vector2(0f, -138f);

            var detalheTmp = Texto("Detalhe", go.transform, detalhe, 16f, TextAlignmentOptions.Top, TextoFraco);
            detalheTmp.rectTransform.anchorMin = new Vector2(0f, 1f);
            detalheTmp.rectTransform.anchorMax = new Vector2(1f, 1f);
            detalheTmp.rectTransform.pivot = new Vector2(0.5f, 1f);
            detalheTmp.rectTransform.offsetMin = new Vector2(4f, -218f);
            detalheTmp.rectTransform.offsetMax = new Vector2(-4f, -174f);
            detalheTmp.enableWordWrapping = true;

            return botao;
        }

        public static Button CardModo(
            string nome,
            Transform pai,
            string icone,
            string titulo,
            string descricao,
            Color corDestaque)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(pai, false);

            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(0f, 190f);

            var fundo = go.GetComponent<Image>();
            Sprite painel = ManaArte.Carregar("card_panel");
            fundo.sprite = painel != null ? painel : SpriteUI;
            fundo.type = painel != null ? Image.Type.Simple : Image.Type.Sliced;
            fundo.preserveAspect = false;
            fundo.color = painel != null ? Color.white : Painel;
            fundo.raycastTarget = true;

            var layout = go.GetComponent<LayoutElement>();
            layout.minHeight = 190f;
            layout.preferredHeight = 190f;
            layout.flexibleWidth = 1f;

            var botao = go.GetComponent<Button>();
            botao.targetGraphic = fundo;
            var cores = botao.colors;
            cores.normalColor = Color.white;
            cores.highlightedColor = new Color(1f, 1f, 1f, 0.95f);
            cores.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            cores.fadeDuration = 0.08f;
            botao.colors = cores;

            var iconeGo = new GameObject("Icone", typeof(RectTransform), typeof(Image));
            iconeGo.transform.SetParent(go.transform, false);
            var iconeRt = (RectTransform)iconeGo.transform;
            iconeRt.anchorMin = new Vector2(0f, 0.12f);
            iconeRt.anchorMax = new Vector2(0.30f, 0.88f);
            iconeRt.offsetMin = new Vector2(24f, 0f);
            iconeRt.offsetMax = new Vector2(-8f, 0f);
            var iconeImg = iconeGo.GetComponent<Image>();
            iconeImg.sprite = ManaArte.Carregar(icone);
            iconeImg.preserveAspect = true;
            iconeImg.color = Color.white;
            iconeImg.raycastTarget = false;

            var tituloTmp = Texto("Titulo", go.transform, titulo, 25f, TextAlignmentOptions.MidlineLeft, corDestaque, FontStyles.Bold);
            tituloTmp.rectTransform.anchorMin = new Vector2(0.32f, 0.48f);
            tituloTmp.rectTransform.anchorMax = new Vector2(0.96f, 0.86f);
            tituloTmp.rectTransform.offsetMin = Vector2.zero;
            tituloTmp.rectTransform.offsetMax = Vector2.zero;
            tituloTmp.enableAutoSizing = true;
            tituloTmp.fontSizeMin = 18f;
            tituloTmp.fontSizeMax = 25f;

            var descricaoTmp = Texto("Descricao", go.transform, descricao, 17f, TextAlignmentOptions.TopLeft, TextoClaro);
            descricaoTmp.rectTransform.anchorMin = new Vector2(0.32f, 0.13f);
            descricaoTmp.rectTransform.anchorMax = new Vector2(0.96f, 0.52f);
            descricaoTmp.rectTransform.offsetMin = Vector2.zero;
            descricaoTmp.rectTransform.offsetMax = Vector2.zero;
            descricaoTmp.enableAutoSizing = true;
            descricaoTmp.fontSizeMin = 13f;
            descricaoTmp.fontSizeMax = 17f;
            descricaoTmp.enableWordWrapping = true;

            var linha = new GameObject("LinhaDestaque", typeof(RectTransform), typeof(Image));
            linha.transform.SetParent(go.transform, false);
            var linhaRt = (RectTransform)linha.transform;
            linhaRt.anchorMin = new Vector2(0.32f, 0.08f);
            linhaRt.anchorMax = new Vector2(0.88f, 0.10f);
            linhaRt.offsetMin = Vector2.zero;
            linhaRt.offsetMax = Vector2.zero;
            linha.GetComponent<Image>().color = corDestaque;
            linha.GetComponent<Image>().raycastTarget = false;

            return botao;
        }

        public static Slider Slider_(string nome, Transform pai, float valorInicial)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(pai, false);

            var fundo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            fundo.transform.SetParent(go.transform, false);
            var fundoRt = (RectTransform)fundo.transform;
            fundoRt.anchorMin = new Vector2(0f, 0.3f);
            fundoRt.anchorMax = new Vector2(1f, 0.7f);
            fundoRt.offsetMin = Vector2.zero;
            fundoRt.offsetMax = Vector2.zero;
            var fundoImg = fundo.GetComponent<Image>();
            fundoImg.sprite = SpriteFundo;
            fundoImg.type = Image.Type.Sliced;
            fundoImg.color = BotaoSecundario;

            var areaPreenchimento = Vazio("Fill Area", go.transform);
            areaPreenchimento.anchorMin = new Vector2(0f, 0.3f);
            areaPreenchimento.anchorMax = new Vector2(1f, 0.7f);
            areaPreenchimento.offsetMin = new Vector2(10f, 0f);
            areaPreenchimento.offsetMax = new Vector2(-20f, 0f);

            var preenchimento = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            preenchimento.transform.SetParent(areaPreenchimento, false);
            var preenchimentoRt = (RectTransform)preenchimento.transform;
            preenchimentoRt.anchorMin = Vector2.zero;
            preenchimentoRt.anchorMax = new Vector2(0f, 1f);
            preenchimentoRt.sizeDelta = new Vector2(10f, 0f);
            var preenchimentoImg = preenchimento.GetComponent<Image>();
            preenchimentoImg.sprite = SpriteUI;
            preenchimentoImg.type = Image.Type.Sliced;
            preenchimentoImg.color = Dourado;

            var areaHandle = Vazio("Handle Slide Area", go.transform);
            areaHandle.anchorMin = Vector2.zero;
            areaHandle.anchorMax = Vector2.one;
            areaHandle.offsetMin = new Vector2(10f, 0f);
            areaHandle.offsetMax = new Vector2(-10f, 0f);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(areaHandle, false);
            var handleRt = (RectTransform)handle.transform;
            handleRt.anchorMin = new Vector2(0f, 0f);
            handleRt.anchorMax = new Vector2(0f, 1f);
            handleRt.sizeDelta = new Vector2(28f, 0f);
            var handleImg = handle.GetComponent<Image>();
            handleImg.sprite = SpriteKnob;
            handleImg.color = TextoClaro;

            var slider = go.GetComponent<Slider>();
            slider.fillRect = preenchimentoRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.SetValueWithoutNotify(valorInicial);

            return slider;
        }

        public static Toggle Toggle_(string nome, Transform pai, string rotulo, bool ligado)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Toggle));
            go.transform.SetParent(pai, false);

            var fundo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            fundo.transform.SetParent(go.transform, false);
            var fundoRt = (RectTransform)fundo.transform;
            fundoRt.anchorMin = new Vector2(0f, 0.5f);
            fundoRt.anchorMax = new Vector2(0f, 0.5f);
            fundoRt.pivot = new Vector2(0f, 0.5f);
            fundoRt.anchoredPosition = new Vector2(8f, 0f);
            fundoRt.sizeDelta = new Vector2(40f, 40f);
            var fundoImg = fundo.GetComponent<Image>();
            fundoImg.sprite = SpriteUI;
            fundoImg.type = Image.Type.Sliced;
            fundoImg.color = BotaoSecundario;

            var check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            check.transform.SetParent(fundo.transform, false);
            var checkRt = (RectTransform)check.transform;
            Esticar(checkRt, 6f);
            var checkImg = check.GetComponent<Image>();
            checkImg.sprite = SpriteCheck;
            checkImg.color = Dourado;

            var label = Texto("Rotulo", go.transform, rotulo, 24f, TextAlignmentOptions.MidlineLeft);
            var labelRt = label.rectTransform;
            labelRt.offsetMin = new Vector2(60f, 0f);
            labelRt.offsetMax = Vector2.zero;

            var toggle = go.GetComponent<Toggle>();
            toggle.targetGraphic = fundoImg;
            toggle.graphic = checkImg;
            toggle.SetIsOnWithoutNotify(ligado);
            check.SetActive(ligado);

            return toggle;
        }

        public static TMP_InputField Campo(string nome, Transform pai, string dica, string valorInicial = "")
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            go.transform.SetParent(pai, false);

            var img = go.GetComponent<Image>();
            img.sprite = SpriteUI;
            img.type = Image.Type.Sliced;
            img.color = PainelClaro;

            var area = Vazio("Text Area", go.transform);
            Esticar(area, 12f);
            area.gameObject.AddComponent<RectMask2D>();

            var placeholder = Texto("Placeholder", area, dica, 24f, TextAlignmentOptions.MidlineLeft, TextoFraco, FontStyles.Italic);
            var texto = Texto("Text", area, valorInicial, 24f, TextAlignmentOptions.MidlineLeft);

            var campo = go.GetComponent<TMP_InputField>();
            campo.targetGraphic = img;
            campo.textViewport = area;
            campo.textComponent = texto;
            campo.placeholder = placeholder;
            campo.lineType = TMP_InputField.LineType.SingleLine;
            campo.characterLimit = 24;
            campo.SetTextWithoutNotify(valorInicial);

            return campo;
        }

        /// <summary>
        /// Área rolável padrão. Devolve o ScrollRect e entrega em
        /// <paramref name="conteudo"/> o RectTransform onde os itens entram.
        /// </summary>
        public static ScrollRect Rolagem(string nome, Transform pai, out RectTransform conteudo, bool grade = false)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            go.transform.SetParent(pai, false);
            Esticar((RectTransform)go.transform);

            var img = go.GetComponent<Image>();
            img.color = Transparente;

            var viewport = Vazio("Viewport", go.transform);
            Esticar(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();

            conteudo = Vazio("Content", viewport);
            conteudo.anchorMin = new Vector2(0f, 1f);
            conteudo.anchorMax = new Vector2(1f, 1f);
            conteudo.pivot = new Vector2(0.5f, 1f);
            conteudo.offsetMin = Vector2.zero;
            conteudo.offsetMax = Vector2.zero;

            if (grade)
            {
                var layout = conteudo.gameObject.AddComponent<GridLayoutGroup>();
                layout.cellSize = new Vector2(150f, 170f);
                layout.spacing = new Vector2(24f, 24f);
                layout.padding = new RectOffset(24, 24, 24, 24);
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                layout.constraintCount = 3;
            }
            else
            {
                var layout = conteudo.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 12f;
                layout.padding = new RectOffset(16, 16, 16, 16);
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = true;
                layout.childControlHeight = true;
                layout.childControlWidth = true;
            }

            var ajuste = conteudo.gameObject.AddComponent<ContentSizeFitter>();
            ajuste.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ajuste.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var scroll = go.GetComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = conteudo;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f;

            return scroll;
        }

        // ---------------------------------------------------------------
        // Layout
        // ---------------------------------------------------------------

        public static VerticalLayoutGroup Coluna(GameObject alvo, float espaco = 16f, int margem = 24)
        {
            var layout = alvo.AddComponent<VerticalLayoutGroup>();
            layout.spacing = espaco;
            layout.padding = new RectOffset(margem, margem, margem, margem);
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childAlignment = TextAnchor.UpperCenter;
            return layout;
        }

        public static HorizontalLayoutGroup Linha(GameObject alvo, float espaco = 12f, int margem = 0)
        {
            var layout = alvo.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = espaco;
            layout.padding = new RectOffset(margem, margem, margem, margem);
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childAlignment = TextAnchor.MiddleCenter;
            return layout;
        }

        public static LayoutElement Altura(GameObject alvo, float altura)
        {
            var elemento = alvo.GetComponent<LayoutElement>() ?? alvo.AddComponent<LayoutElement>();
            elemento.minHeight = altura;
            elemento.preferredHeight = altura;
            return elemento;
        }

        public static LayoutElement Largura(GameObject alvo, float largura)
        {
            var elemento = alvo.GetComponent<LayoutElement>() ?? alvo.AddComponent<LayoutElement>();
            elemento.minWidth = largura;
            elemento.preferredWidth = largura;
            elemento.flexibleWidth = 0f;
            return elemento;
        }
    }
}
