using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BibleMatch3.EditorTools
{
    /// <summary>
    /// Cria os ScriptableObjects mínimos para o jogo ser jogável: efeitos,
    /// poderes, fases de campanha e versículos.
    ///
    /// Tudo aqui é CONTEÚDO PLACEHOLDER de balanceamento — números redondos
    /// escolhidos para o fluxo poder ser testado ponta a ponta, não para ser
    /// o design final. Assets já existentes nunca são sobrescritos, então dá
    /// para ajustar os valores no Inspector e rodar o montador de novo sem
    /// perder o ajuste.
    /// </summary>
    internal static class ManaAssets
    {
        public const string PastaDados = "Assets/GameData";
        public const string PastaPoderes = PastaDados + "/Poderes";
        public const string PastaEfeitos = PastaDados + "/Efeitos";
        public const string PastaFases = PastaDados + "/Fases";
        public const string PastaVersiculos = PastaDados + "/Versiculos";

        internal sealed class Catalogo
        {
            public readonly List<PowerUpConfig> EspeciaisDeTabuleiro = new List<PowerUpConfig>();
            public readonly List<PowerUpConfig> Avulsos = new List<PowerUpConfig>();
            public readonly List<LevelData> Fases = new List<LevelData>();
            public readonly List<VerseData> Versiculos = new List<VerseData>();

            public PowerUpConfig Martelo;
            public PowerUpConfig Embaralhar;
            public PowerUpConfig MaisMovimentos;
        }

        public static Catalogo Gerar()
        {
            GarantirPastas();
            var catalogo = new Catalogo();

            CriarEspeciais(catalogo);
            CriarAvulsos(catalogo);
            CriarFases(catalogo);
            CriarVersiculos(catalogo);

            AssetDatabase.SaveAssets();
            return catalogo;
        }

        // ---------------------------------------------------------------
        // Especiais de tabuleiro (evoluíveis)
        // ---------------------------------------------------------------

        private static void CriarEspeciais(Catalogo catalogo)
        {
            catalogo.EspeciaisDeTabuleiro.Add(Especial<EfeitoEspadaLinhaSO>(
                "EspadaLinha", "Espada da Palavra (Linha)",
                "Corta a linha inteira. Nasce de uma combinação de 4 na horizontal.",
                SpecialType.Espada_Linha, ManaArte.NomesDeEspeciais[(int)SpecialType.Espada_Linha]));

            catalogo.EspeciaisDeTabuleiro.Add(Especial<EfeitoEspadaColunaSO>(
                "EspadaColuna", "Espada da Palavra (Coluna)",
                "Corta a coluna inteira. Nasce de uma combinação de 4 na vertical.",
                SpecialType.Espada_Coluna, ManaArte.NomesDeEspeciais[(int)SpecialType.Espada_Coluna]));

            catalogo.EspeciaisDeTabuleiro.Add(Especial<EfeitoTochaAcesaSO>(
                "TochaAcesa", "Tocha Acesa",
                "Explode em área ao redor. A área cresce a cada nível.",
                SpecialType.Tocha_Acesa, ManaArte.NomesDeEspeciais[(int)SpecialType.Tocha_Acesa]));

            catalogo.EspeciaisDeTabuleiro.Add(Especial<EfeitoArcaAliancaSO>(
                "ArcaAlianca", "Arca da Aliança",
                "Remove do tabuleiro todas as peças do tipo escolhido.",
                SpecialType.Arca_Alianca, ManaArte.NomesDeEspeciais[(int)SpecialType.Arca_Alianca]));

            catalogo.EspeciaisDeTabuleiro.Add(Especial<EfeitoEstrelaGuiaSO>(
                "EstrelaGuia", "Estrela Guia",
                "Busca as peças que ainda faltam para o objetivo da fase.",
                SpecialType.Estrela_Guia, ManaArte.NomesDeEspeciais[(int)SpecialType.Estrela_Guia]));
        }

        private static PowerUpConfig Especial<TEfeito>(
            string arquivo, string nome, string descricao, SpecialType tipo, string nomeDoSprite)
            where TEfeito : EfeitoEspecialSO
        {
            var efeito = CriarOuCarregar<TEfeito>($"{PastaEfeitos}/Efeito{arquivo}.asset");
            var config = CriarOuCarregar<PowerUpConfig>($"{PastaPoderes}/Poder{arquivo}.asset");

            if (string.IsNullOrEmpty(config.NomeExibicao))
            {
                config.NomeExibicao = nome;
                config.Descricao = descricao;
                config.NivelAtual = 1;
                config.NivelMaximo = 3;
                config.CustoEvolucaoPorNivel = new[] { 150, 400 }; // nv1→2 e nv2→3
                EditorUtility.SetDirty(config);
            }

            // Estes são sempre reaplicados: são ligações estruturais, não
            // balanceamento, e precisam sobreviver a uma remontagem.
            config.Tipo = TipoPoder.EspecialDeTabuleiro;
            config.TipoEspecialAssociado = tipo;
            config.EfeitoDeTabuleiro = efeito;
            config.Icone = ManaArte.Carregar(nomeDoSprite);
            EditorUtility.SetDirty(config);

            return config;
        }

        // ---------------------------------------------------------------
        // Poderes avulsos
        // ---------------------------------------------------------------

        private static void CriarAvulsos(Catalogo catalogo)
        {
            catalogo.Martelo = Avulso<EfeitoMarteloSO>(
                "Martelo", "Martelo",
                "Quebra uma peça à sua escolha. Toque no tabuleiro para mirar.", 50);

            catalogo.Embaralhar = Avulso<EfeitoEmbaralharSO>(
                "Embaralhar", "Embaralhar",
                "Reorganiza todas as peças do tabuleiro.", 75);

            catalogo.MaisMovimentos = Avulso<EfeitoMaisMovimentosSO>(
                "MaisMovimentos", "+5 Movimentos",
                "Ganhe cinco movimentos extras nesta fase.", 100);

            catalogo.Avulsos.Add(catalogo.Martelo);
            catalogo.Avulsos.Add(catalogo.Embaralhar);
            catalogo.Avulsos.Add(catalogo.MaisMovimentos);
        }

        private static PowerUpConfig Avulso<TEfeito>(string arquivo, string nome, string descricao, int custo)
            where TEfeito : PoderAvulsoSO
        {
            var efeito = CriarOuCarregar<TEfeito>($"{PastaEfeitos}/Efeito{arquivo}.asset");
            var config = CriarOuCarregar<PowerUpConfig>($"{PastaPoderes}/Poder{arquivo}.asset");

            if (string.IsNullOrEmpty(config.NomeExibicao))
            {
                config.NomeExibicao = nome;
                config.Descricao = descricao;
                config.CustoMoedas = custo;
                EditorUtility.SetDirty(config);
            }

            config.Tipo = TipoPoder.Avulso;
            config.CustoMoedas = custo;
            efeito.DefinirRequerAlvo(typeof(TEfeito) == typeof(EfeitoMarteloSO));
            config.EfeitoAvulso = efeito;
            EditorUtility.SetDirty(efeito);
            EditorUtility.SetDirty(config);

            return config;
        }

        // ---------------------------------------------------------------
        // Fases da campanha
        // ---------------------------------------------------------------

        private static void CriarFases(Catalogo catalogo)
        {
            catalogo.Fases.Add(Fase(1, "O Maná no Deserto", 25,
                new[] { (TileType.Pao, 20) },
                1000, 2200, 3500,
                null));

            catalogo.Fases.Add(Fase(2, "Os Peixes e os Pães", 22,
                new[] { (TileType.Pao, 18), (TileType.Peixe, 18) },
                1200, 2600, 4000,
                new[] { (ObstacleType.Gelo, 3, 3), (ObstacleType.Gelo, 4, 4) }));

            catalogo.Fases.Add(Fase(3, "A Videira Verdadeira", 20,
                new[] { (TileType.Uva, 24) },
                1400, 3000, 4600,
                new[] { (ObstacleType.PedraDeserto, 0, 4), (ObstacleType.PedraDeserto, 7, 4) }));

            catalogo.Fases.Add(Fase(4, "A Ceifa Abundante", 20,
                new[] { (TileType.Espiga, 22), (TileType.Azeite, 14) },
                1600, 3200, 5000,
                new[] { (ObstacleType.Corrente, 2, 2), (ObstacleType.Corrente, 5, 5) }));

            catalogo.Fases.Add(Fase(5, "A Pomba sobre as Águas", 18,
                new[] { (TileType.Pomba, 20), (TileType.Peixe, 16) },
                1800, 3600, 5600,
                new[] { (ObstacleType.CaixaSelada, 3, 6), (ObstacleType.CaixaSelada, 4, 6) }));
        }

        private static LevelData Fase(
            int numero, string nome, int movimentos,
            (TileType tipo, int quantidade)[] objetivos,
            int estrela1, int estrela2, int estrela3,
            (ObstacleType tipo, int x, int y)[] obstaculos)
        {
            var fase = CriarOuCarregar<LevelData>($"{PastaFases}/Fase{numero:00}.asset");

            // Só popula fases recém-criadas: um design já ajustado à mão
            // não pode ser apagado por uma remontagem da UI.
            if (fase.Numero != 0) return fase;

            fase.Numero = numero;
            fase.Nome = nome;
            fase.Movimentos = movimentos;
            fase.EstrelaScore1 = estrela1;
            fase.EstrelaScore2 = estrela2;
            fase.EstrelaScore3 = estrela3;

            fase.Objetivos = new List<ObjectiveEntry>();
            foreach ((TileType tipo, int quantidade) in objetivos)
                fase.Objetivos.Add(new ObjectiveEntry { Type = tipo, RequiredAmount = quantidade });

            fase.Obstaculos = new List<ObstaculoPosicionado>();
            if (obstaculos != null)
                foreach ((ObstacleType tipo, int x, int y) in obstaculos)
                    fase.Obstaculos.Add(new ObstaculoPosicionado { Tipo = tipo, X = x, Y = y });

            EditorUtility.SetDirty(fase);
            return fase;
        }

        // ---------------------------------------------------------------
        // Versículos do Estudo Infinito
        // ---------------------------------------------------------------

        private static void CriarVersiculos(Catalogo catalogo)
        {
            catalogo.Versiculos.Add(Versiculo("Joao_3_16",
                "Porque Deus amou o mundo de tal maneira que deu o seu Filho unigênito.",
                "João 3:16",
                "O ponto de partida de tudo é um amor que se entrega primeiro."));

            catalogo.Versiculos.Add(Versiculo("Salmo_23_1",
                "O Senhor é o meu pastor; nada me faltará.",
                "Salmos 23:1",
                "Confiança não é ausência de falta, é presença de cuidado."));

            catalogo.Versiculos.Add(Versiculo("Mateus_6_11",
                "O pão nosso de cada dia nos dá hoje.",
                "Mateus 6:11",
                "O maná era diário. Sustento se recebe hoje, não se estoca."));

            catalogo.Versiculos.Add(Versiculo("Filipenses_4_13",
                "Posso todas as coisas naquele que me fortalece.",
                "Filipenses 4:13",
                "A força vem de fora de você, e por isso não se esgota em você."));

            catalogo.Versiculos.Add(Versiculo("Isaias_40_31",
                "Os que esperam no Senhor renovarão as suas forças.",
                "Isaías 40:31",
                "Esperar aqui não é ficar parado — é renovar-se enquanto caminha."));
        }

        private static VerseData Versiculo(string arquivo, string texto, string referencia, string reflexao)
        {
            var versiculo = CriarOuCarregar<VerseData>($"{PastaVersiculos}/{arquivo}.asset");

            if (!string.IsNullOrEmpty(versiculo.Texto)) return versiculo;

            versiculo.Texto = texto;
            versiculo.Referencia = referencia;
            versiculo.Reflexao = reflexao;
            EditorUtility.SetDirty(versiculo);

            return versiculo;
        }

        // ---------------------------------------------------------------
        // Infra
        // ---------------------------------------------------------------

        private static T CriarOuCarregar<T>(string caminho) where T : ScriptableObject
        {
            var existente = AssetDatabase.LoadAssetAtPath<T>(caminho);
            if (existente != null) return existente;

            // Se o YAML foi criado com m_Script inválido ou perdeu a referência
            // ao tipo, o LoadAssetAtPath retorna null, mas o arquivo ainda existe.
            // Remover e recriar é seguro para estes assets gerados pelo montador
            // e evita que a HUD receba EfeitoAvulso nulo.
            if (File.Exists(caminho))
            {
                AssetDatabase.DeleteAsset(caminho);
                AssetDatabase.Refresh();
            }

            var novo = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(novo, caminho);
            return novo;
        }

        private static void GarantirPastas()
        {
            GarantirPasta(PastaDados);
            GarantirPasta(PastaPoderes);
            GarantirPasta(PastaEfeitos);
            GarantirPasta(PastaFases);
            GarantirPasta(PastaVersiculos);
        }

        public static void GarantirPasta(string caminho)
        {
            if (AssetDatabase.IsValidFolder(caminho)) return;

            string pai = Path.GetDirectoryName(caminho).Replace('\\', '/');
            string nome = Path.GetFileName(caminho);

            if (!AssetDatabase.IsValidFolder(pai)) GarantirPasta(pai);
            AssetDatabase.CreateFolder(pai, nome);
        }
    }
}
