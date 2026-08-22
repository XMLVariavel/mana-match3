using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BibleMatch3.EditorTools
{
    /// <summary>
    /// Localiza e prepara a arte placeholder. Os PNGs são procurados por nome
    /// em todo o projeto (e não por caminho fixo) de propósito: assim a pasta
    /// Art/Placeholder pode ser movida para dentro de Assets/ em qualquer lugar
    /// sem quebrar o montador.
    /// </summary>
    internal static class ManaArte
    {
        /// <summary>Nomes na ordem exata do enum <see cref="TileType"/>.</summary>
        public static readonly string[] NomesDePecas =
        {
            "peca_pao", "peca_peixe", "peca_uva", "peca_espiga", "peca_azeite", "peca_pomba"
        };

        /// <summary>
        /// Nomes na ordem exata do enum <see cref="SpecialType"/>.
        /// O índice 0 (Nenhum) fica vazio de propósito — BoardManager indexa
        /// o array direto por (int)SpecialType.
        /// </summary>
        public static readonly string[] NomesDeEspeciais =
        {
            null, "especial_espada_linha", "especial_espada_coluna",
            "especial_tocha", "especial_arca", "especial_estrela"
        };

        public const string PedraDeserto = "obstaculo_pedra";
        public const string Corrente = "obstaculo_corrente";
        public const string Gelo = "obstaculo_gelo";
        public const string CaixaSelada = "obstaculo_caixa";
        public const string FundoJornada = "fundo_jornada";
        public const string FundoCelestial = "fundo_celestial";
        public const string FundoTelaEntrada = "fundo_tela_entrada";
        public const string PainelBemVindo = "painel_bem_vindo";
        public const string LogoJogo = "logo_jogo";
        public const string MolduraTabuleiro = "moldura_tabuleiro";
        public static readonly string[] NomesDeAvatares = { "avatar_davi", "avatar_ester", "avatar_daniel", "avatar_rute", "avatar_moises" };
        public static readonly string[] NomesDeOrnamentos =
        {
            "header_ornament", "button_primary", "button_secondary", "card_panel", "stat_card_panel", "bottom_navigation"
        };
        public static readonly string[] NomesDeIconesDeModo =
        {
            "campaign", "infinite", "daily", "time", "guardian"
        };
        public static readonly string[] NomesDeIconesDePoder =
        {
            "power_hammer", "power_shuffle", "power_plus5"
        };

        // --- Kit de UI da tela de jogo (arte com alpha, recortada) -------
        public const string LogoMana = "logo_mana";
        public const string FaixaPergaminho = "faixa_pergaminho";
        public const string BotaoVoltar = "botao_voltar";
        public const string BotaoCircular = "botao_circular";
        public const string BadgeLivro = "badge_livro";
        public const string Moeda = "moeda";

        public static readonly string[] NomesDoKit =
        {
            LogoMana, FaixaPergaminho, BotaoVoltar, BotaoCircular, BadgeLivro, Moeda
        };

        /// <summary>
        /// Bordas de 9-slice por sprite. Só a faixa de pergaminho estica:
        /// as pontas roladas precisam ficar intactas nas laterais.
        /// </summary>
        private static readonly Dictionary<string, Vector4> BordasDoKit = new Dictionary<string, Vector4>
        {
            { FaixaPergaminho, new Vector4(78f, 0f, 78f, 0f) }
        };

        /// <summary>Pixels por unidade: com maxTextureSize 256 isso dá 1 peça = 1 célula.</summary>
        public const float PixelsPorUnidade = 256f;

        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Ajusta o import de todos os PNGs placeholder para sprite transparente
        /// de 256px. Sem isso, num projeto criado pelo template 3D as texturas
        /// entrariam como Texture e nada apareceria no tabuleiro.
        /// </summary>
        public static int PrepararImportacao()
        {
            // Arte recém-copiada para Assets/ só entra no AssetDatabase depois
            // de um refresh; sem ele a primeira montagem não acharia o kit.
            AssetDatabase.Refresh();
            cache.Clear();
            int ajustados = 0;

            foreach (string nome in TodosOsNomes())
            {
                string caminho = CaminhoDe(nome);
                if (caminho == null) continue;

                var importador = AssetImporter.GetAtPath(caminho) as TextureImporter;
                if (importador == null) continue;

                bool eFundo = nome == FundoJornada || nome == FundoCelestial || nome == FundoTelaEntrada;
                bool eEntrada = nome == FundoTelaEntrada || nome == PainelBemVindo || nome == LogoJogo;
                bool eMoldura = nome == MolduraTabuleiro;
                bool eAvatar = Array.IndexOf(NomesDeAvatares, nome) >= 0;
                bool eOrnamento = Array.IndexOf(NomesDeOrnamentos, nome) >= 0;
                bool eIconeDeModo = Array.IndexOf(NomesDeIconesDeModo, nome) >= 0;
                bool eIconeDePoder = Array.IndexOf(NomesDeIconesDePoder, nome) >= 0;
                bool eKit = Array.IndexOf(NomesDoKit, nome) >= 0;
                float pixelsPorUnidade = eFundo || eMoldura || eOrnamento || eKit || eEntrada ? 100f : eAvatar || eIconeDeModo || eIconeDePoder ? 512f : PixelsPorUnidade;
                int tamanhoMaximo = eFundo || eMoldura || eAvatar || eOrnamento || eIconeDeModo || eIconeDePoder || eKit || eEntrada ? 2048 : 256;

                Vector4 borda = eOrnamento ? BordaDeOrnamento(nome)
                    : eKit && BordasDoKit.TryGetValue(nome, out Vector4 bordaKit) ? bordaKit
                    : Vector4.zero;
                bool precisaReimportar =
                    importador.textureType != TextureImporterType.Sprite ||
                    importador.spriteImportMode != SpriteImportMode.Single ||
                    !importador.alphaIsTransparency ||
                    importador.mipmapEnabled ||
                    !Mathf.Approximately(importador.spritePixelsPerUnit, pixelsPorUnidade) ||
                    importador.maxTextureSize != tamanhoMaximo ||
                    importador.spriteBorder != borda;

                if (!precisaReimportar) continue;

                importador.textureType = TextureImporterType.Sprite;
                importador.spriteImportMode = SpriteImportMode.Single;
                importador.alphaIsTransparency = true;
                importador.alphaSource = TextureImporterAlphaSource.FromInput;
                importador.mipmapEnabled = false;
                importador.filterMode = FilterMode.Bilinear;
                importador.spritePixelsPerUnit = pixelsPorUnidade;
                importador.maxTextureSize = tamanhoMaximo;
                importador.spriteBorder = borda;
                importador.textureCompression = TextureImporterCompression.Compressed;
                importador.SaveAndReimport();

                ajustados++;
            }

            return ajustados;
        }

        /// <summary>
        /// O painel de estatística é bem menor que os demais ornamentos:
        /// com borda 96 as laterais se sobrepõem e o card vira um borrão.
        /// </summary>
        private static Vector4 BordaDeOrnamento(string nome)
        {
            if (nome == "stat_card_panel") return new Vector4(52f, 52f, 52f, 52f);
            return new Vector4(96f, 96f, 96f, 96f);
        }

        public static Sprite Carregar(string nome)
        {
            if (string.IsNullOrEmpty(nome)) return null;
            if (cache.TryGetValue(nome, out Sprite emCache) && emCache != null) return emCache;

            string caminho = CaminhoDe(nome);
            if (caminho == null)
            {
                Debug.LogWarning($"[Maná] Sprite '{nome}' não encontrado. " +
                                 "A pasta Art/Placeholder está dentro de Assets/?");
                return null;
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
            if (sprite != null) cache[nome] = sprite;
            return sprite;
        }

        public static Object[] SpritesDePecas() => CarregarVarios(NomesDePecas);
        public static Object[] SpritesDeEspeciais() => CarregarVarios(NomesDeEspeciais);

        private static Object[] CarregarVarios(string[] nomes)
        {
            var resultado = new Object[nomes.Length];
            for (int i = 0; i < nomes.Length; i++) resultado[i] = Carregar(nomes[i]);
            return resultado;
        }

        private static IEnumerable<string> TodosOsNomes()
        {
            foreach (string n in NomesDePecas) yield return n;
            foreach (string n in NomesDeEspeciais) if (n != null) yield return n;
            yield return PedraDeserto;
            yield return Corrente;
            yield return Gelo;
            yield return CaixaSelada;
            yield return FundoJornada;
            yield return FundoCelestial;
            yield return MolduraTabuleiro;
            foreach (string avatar in NomesDeAvatares) yield return avatar;
            foreach (string ornamento in NomesDeOrnamentos) yield return ornamento;
            foreach (string icone in NomesDeIconesDeModo) yield return icone;
            foreach (string icone in NomesDeIconesDePoder) yield return icone;
            foreach (string peca in NomesDoKit) yield return peca;
            yield return FundoTelaEntrada;
            yield return PainelBemVindo;
            yield return LogoJogo;
        }

        /// <summary>
        /// Resolve o caminho do asset pelo nome do arquivo. FindAssets faz busca
        /// aproximada, então o nome exato é conferido depois para não pegar,
        /// por exemplo, "peca_pao_final" quando pedimos "peca_pao".
        /// </summary>
        private static string CaminhoDe(string nome)
        {
            string[] guids = AssetDatabase.FindAssets($"{nome} t:Texture2D");

            foreach (string guid in guids)
            {
                string caminho = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(caminho) == nome) return caminho;
            }

            return null;
        }
    }
}
