using System;
using System.Collections.Generic;

namespace BibleMatch3
{
    /// <summary>
    /// Progresso do jogador numa fase específica da Campanha.
    /// </summary>
    [Serializable]
    public class LevelProgressEntry
    {
        public int Numero;
        public int Estrelas; // 0 a 3
    }

    [Serializable]
    public class PowerStockEntry
    {
        public string PowerId;
        public int Quantidade;
    }

    /// <summary>
    /// Espelha o documento da coleção "users" no Firestore. Também é usado
    /// como formato da fila local de sincronização pendente (JsonUtility).
    /// </summary>
    [Serializable]
    public class PlayerProgress
    {
        public string Uid;
        public string DisplayName;
        public string AvatarId;
        public int Xp;
        public int Level;
        public int HighScore;
        public int MelhorCombo;
        public int MelhorScoreEstudoInfinito;
        public int SequenciaDesafioDiario;
        public string UltimoDesafioDiario;
        public List<string> UnlockedVerses = new List<string>();
        public int LivesCount;
        public long LastLifeTimestampUnix;
        public bool SemAnuncios; // true depois da compra única "Remover Anúncios"
        public int Moedas; // moeda ganha jogando — nunca vendida (ver decisão de monetização)
        public List<PowerStockEntry> EstoquePoderes = new List<PowerStockEntry>();
        public List<LevelProgressEntry> Fases = new List<LevelProgressEntry>();

        // Presente no schema desde já para não exigir migração depois, mas a
        // captura de verdade (tela de consentimento) é entregue na Fase D.
        public bool ConsentimentoLgpd;
        public long ConsentimentoLgpdTimestampUnix;

        public static PlayerProgress Novo(string uid) => new PlayerProgress
        {
            Uid = uid,
            DisplayName = "Jogador",
            AvatarId = AvatarCatalog.Padrao,
            Xp = 0,
            Level = 1,
            HighScore = 0,
            UnlockedVerses = new List<string>(),
            LivesCount = 5,
            LastLifeTimestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ConsentimentoLgpd = false,
            ConsentimentoLgpdTimestampUnix = 0
        };

        public Dictionary<string, object> ParaDicionario()
        {
            Sanitizar();
            return new Dictionary<string, object>
            {
            { "uid", Uid },
            { "displayName", DisplayName },
            { "avatarId", AvatarId },
            { "xp", Xp },
            { "level", Level },
            { "highScore", HighScore },
            { "melhorCombo", MelhorCombo },
            { "melhorScoreEstudoInfinito", MelhorScoreEstudoInfinito },
            { "sequenciaDesafioDiario", SequenciaDesafioDiario },
            { "ultimoDesafioDiario", UltimoDesafioDiario },
            { "unlockedVerses", UnlockedVerses },
            { "livesCount", LivesCount },
            { "lastLifeTimestampUnix", LastLifeTimestampUnix },
            { "semAnuncios", SemAnuncios },
                { "moedas", Moedas },
                { "estoquePoderes", EstoqueParaLista() },
                { "fases", FasesParaLista() },
            { "consentimentoLgpd", ConsentimentoLgpd },
            { "consentimentoLgpdTimestampUnix", ConsentimentoLgpdTimestampUnix }
            };
        }

        /// <summary>
        /// Valida os limites de economia e ranking antes de qualquer sincronização.
        /// O cliente não deve conseguir propagar valores negativos ou absurdos.
        /// </summary>
        public void Sanitizar()
        {
            Uid = LimitarTexto(Uid, 128);
            DisplayName = string.IsNullOrWhiteSpace(DisplayName) ? "Jogador" : LimitarTexto(DisplayName.Trim(), 40);
            AvatarId = AvatarCatalog.Existe(AvatarId) ? AvatarId.ToLowerInvariant() : AvatarCatalog.Padrao;
            Xp = Math.Max(0, Math.Min(Xp, 1000000000));
            Level = Math.Max(1, Math.Min(Level, 9999));
            HighScore = Math.Max(0, Math.Min(HighScore, 1000000000));
            MelhorCombo = Math.Max(0, Math.Min(MelhorCombo, 10000));
            MelhorScoreEstudoInfinito = Math.Max(0, Math.Min(MelhorScoreEstudoInfinito, 1000000000));
            SequenciaDesafioDiario = Math.Max(0, Math.Min(SequenciaDesafioDiario, 10000));
            UltimoDesafioDiario = LimitarTexto(UltimoDesafioDiario, 32);
            LivesCount = Math.Max(0, Math.Min(LivesCount, 5));
            Moedas = Math.Max(0, Math.Min(Moedas, 1000000000));

            if (EstoquePoderes == null) EstoquePoderes = new List<PowerStockEntry>();
            for (int i = EstoquePoderes.Count - 1; i >= 0; i--)
            {
                PowerStockEntry estoque = EstoquePoderes[i];
                if (estoque == null || string.IsNullOrWhiteSpace(estoque.PowerId))
                {
                    EstoquePoderes.RemoveAt(i);
                    continue;
                }
                estoque.PowerId = LimitarTexto(estoque.PowerId.Trim(), 80);
                estoque.Quantidade = Math.Max(0, Math.Min(estoque.Quantidade, 999));
            }

            if (UnlockedVerses == null) UnlockedVerses = new List<string>();
            for (int i = UnlockedVerses.Count - 1; i >= 0; i--)
            {
                string verso = UnlockedVerses[i];
                if (string.IsNullOrWhiteSpace(verso)) UnlockedVerses.RemoveAt(i);
                else UnlockedVerses[i] = LimitarTexto(verso.Trim(), 120);
            }

            if (Fases == null) Fases = new List<LevelProgressEntry>();
            for (int i = Fases.Count - 1; i >= 0; i--)
            {
                LevelProgressEntry fase = Fases[i];
                if (fase == null || fase.Numero <= 0)
                {
                    Fases.RemoveAt(i);
                    continue;
                }

                fase.Numero = Math.Min(fase.Numero, 9999);
                fase.Estrelas = Math.Max(0, Math.Min(fase.Estrelas, 3));
            }
        }

        private static string LimitarTexto(string texto, int limite)
        {
            if (string.IsNullOrEmpty(texto)) return texto;
            return texto.Length <= limite ? texto : texto.Substring(0, limite);
        }

        private List<object> EstoqueParaLista()
        {
            var lista = new List<object>();
            foreach (PowerStockEntry estoque in EstoquePoderes)
            {
                lista.Add(new Dictionary<string, object>
                {
                    { "powerId", estoque.PowerId },
                    { "quantidade", estoque.Quantidade }
                });
            }
            return lista;
        }

        private List<object> FasesParaLista()
        {
            var lista = new List<object>();
            foreach (LevelProgressEntry fase in Fases)
            {
                lista.Add(new Dictionary<string, object>
                {
                    { "numero", fase.Numero },
                    { "estrelas", fase.Estrelas }
                });
            }
            return lista;
        }

        /// <summary>
        /// Retorna as estrelas da fase indicada (0 se ainda não jogada).
        /// </summary>
        public int EstrelasDaFase(int numero)
        {
            foreach (LevelProgressEntry fase in Fases)
                if (fase.Numero == numero) return fase.Estrelas;
            return 0;
        }

        /// <summary>
        /// Registra o resultado de uma fase, sem nunca diminuir a melhor
        /// pontuação em estrelas já obtida antes.
        /// </summary>
        public int QuantidadeDoPoder(string powerId)
        {
            foreach (PowerStockEntry estoque in EstoquePoderes)
                if (estoque.PowerId == powerId) return estoque.Quantidade;
            return 0;
        }

        public void DefinirQuantidadeDoPoder(string powerId, int quantidade)
        {
            if (string.IsNullOrWhiteSpace(powerId)) return;
            foreach (PowerStockEntry estoque in EstoquePoderes)
            {
                if (estoque.PowerId != powerId) continue;
                estoque.Quantidade = Math.Max(0, quantidade);
                return;
            }
            EstoquePoderes.Add(new PowerStockEntry { PowerId = powerId, Quantidade = Math.Max(0, quantidade) });
        }

        public void RegistrarResultadoDaFase(int numero, int estrelas)
        {
            foreach (LevelProgressEntry fase in Fases)
            {
                if (fase.Numero != numero) continue;
                fase.Estrelas = Math.Max(fase.Estrelas, estrelas);
                return;
            }
            Fases.Add(new LevelProgressEntry { Numero = numero, Estrelas = estrelas });
        }

        /// <summary>
        /// Reconstrói a partir de DocumentSnapshot.ToDictionary() do Firestore.
        /// Usa conversões defensivas porque números inteiros costumam voltar
        /// como long, não int.
        /// </summary>
        public static PlayerProgress DoDicionario(Dictionary<string, object> dados)
        {
            var progresso = new PlayerProgress
            {
                Uid = LerString(dados, "uid"),
                DisplayName = LerString(dados, "displayName"),
                AvatarId = LerString(dados, "avatarId"),
                Xp = LerInt(dados, "xp"),
                Level = LerInt(dados, "level"),
                HighScore = LerInt(dados, "highScore"),
                MelhorCombo = LerInt(dados, "melhorCombo"),
                MelhorScoreEstudoInfinito = LerInt(dados, "melhorScoreEstudoInfinito"),
                SequenciaDesafioDiario = LerInt(dados, "sequenciaDesafioDiario"),
                UltimoDesafioDiario = LerString(dados, "ultimoDesafioDiario"),
                LivesCount = LerInt(dados, "livesCount"),
                LastLifeTimestampUnix = LerLong(dados, "lastLifeTimestampUnix"),
                SemAnuncios = dados.ContainsKey("semAnuncios") && Convert.ToBoolean(dados["semAnuncios"]),
                Moedas = LerInt(dados, "moedas"),
                EstoquePoderes = new List<PowerStockEntry>(),
                ConsentimentoLgpd = dados.ContainsKey("consentimentoLgpd") && Convert.ToBoolean(dados["consentimentoLgpd"]),
                ConsentimentoLgpdTimestampUnix = LerLong(dados, "consentimentoLgpdTimestampUnix"),
                UnlockedVerses = new List<string>()
            };

            if (dados.TryGetValue("unlockedVerses", out object versos) && versos is List<object> lista)
            {
                foreach (object item in lista) progresso.UnlockedVerses.Add(item.ToString());
            }

            progresso.EstoquePoderes = new List<PowerStockEntry>();
            if (dados.TryGetValue("estoquePoderes", out object estoqueObj) && estoqueObj is List<object> estoqueLista)
            {
                foreach (object item in estoqueLista)
                {
                    if (item is Dictionary<string, object> mapa)
                    {
                        progresso.EstoquePoderes.Add(new PowerStockEntry
                        {
                            PowerId = mapa.TryGetValue("powerId", out object id) ? id?.ToString() : null,
                            Quantidade = mapa.TryGetValue("quantidade", out object quantidade) ? Convert.ToInt32(quantidade) : 0
                        });
                    }
                }
            }

            progresso.Fases = new List<LevelProgressEntry>();
            if (dados.TryGetValue("fases", out object fasesObj) && fasesObj is List<object> fasesLista)
            {
                foreach (object item in fasesLista)
                {
                    if (item is Dictionary<string, object> mapa)
                    {
                        progresso.Fases.Add(new LevelProgressEntry
                        {
                            Numero = mapa.TryGetValue("numero", out object n) ? Convert.ToInt32(n) : 0,
                            Estrelas = mapa.TryGetValue("estrelas", out object e) ? Convert.ToInt32(e) : 0
                        });
                    }
                }
            }

            progresso.Sanitizar();
            return progresso;
        }

        private static string LerString(Dictionary<string, object> dados, string chave) =>
            dados.TryGetValue(chave, out object valor) ? valor?.ToString() : null;

        private static int LerInt(Dictionary<string, object> dados, string chave) =>
            dados.TryGetValue(chave, out object valor) ? Convert.ToInt32(valor) : 0;

        private static long LerLong(Dictionary<string, object> dados, string chave) =>
            dados.TryGetValue(chave, out object valor) ? Convert.ToInt64(valor) : 0L;
    }
}
