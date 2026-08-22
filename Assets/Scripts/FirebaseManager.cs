using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Ponto único de acesso ao Firebase: inicialização, login anônimo
    /// automático, vínculo de conta Google, leitura/escrita do progresso no
    /// Firestore, leaderboard, e uma fila local simples para não perder
    /// progresso quando o salvamento falha por falta de conexão.
    ///
    /// Plano gratuito (Spark): login anônimo/Google e Firestore cabem sem
    /// custo nas cotas diárias nesta fase do projeto. O único cuidado é não
    /// usar Firebase Storage aqui — desde fev/2026 ele exige o plano Blaze.
    /// </summary>
    public class FirebaseManager : MonoBehaviour
    {
        private const string ColecaoUsuarios = "users";
        private const string ColecaoLeaderboard = "leaderboard";
        private const string ChaveFilaLocal = "BibleMatch3_FilaSyncPendente";

        public static FirebaseManager Instance { get; private set; }

        private FirebaseAuth auth;
        private FirebaseFirestore db;
        private bool firebaseProntoParaUso;

        public bool EstaInicializado => firebaseProntoParaUso;
        public bool ModoOffline { get; private set; }
        public bool UsuarioLogado => auth != null && auth.CurrentUser != null;
        public string UserId => auth?.CurrentUser?.UserId;
        public bool ContaVinculada => auth?.CurrentUser != null && !auth.CurrentUser.IsAnonymous;

        public event Action OnLoginPronto;
        public event Action<string> OnErro; // mensagem já amigável para mostrar ao jogador
        public event Action<PlayerProgress> OnProgressoCarregado;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InicializarFirebase();
        }

        // ---------------------------------------------------------------
        // Inicialização + login anônimo automático
        // ---------------------------------------------------------------

        private void InicializarFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted || task.Result != DependencyStatus.Available)
                {
                    AtivarModoOffline("Serviços online indisponíveis. O progresso ficará salvo neste dispositivo.");
                    return;
                }

                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;
                firebaseProntoParaUso = true;

                LoginAnonimo();
            });
        }

        public void LoginAnonimo()
        {
            if (!firebaseProntoParaUso) return;

            if (auth.CurrentUser != null)
            {
                OnLoginConcluido();
                return;
            }

            auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    AtivarModoOffline("Não foi possível conectar ao servidor. Você pode continuar jogando offline.");
                    Debug.LogWarning($"Login anônimo falhou; modo offline ativado: {task.Exception?.GetBaseException().Message}");
                    return;
                }
                OnLoginConcluido();
            });
        }

        private void OnLoginConcluido()
        {
            OnLoginPronto?.Invoke();
            SincronizarFilaLocal();
            CarregarProgresso();
        }

        private void AtivarModoOffline(string mensagem)
        {
            firebaseProntoParaUso = false;
            ModoOffline = true;
            string idLocal = "offline-" + (string.IsNullOrEmpty(SystemInfo.deviceUniqueIdentifier) ? "device" : SystemInfo.deviceUniqueIdentifier);
            ProgressoAtual = PlayerProgress.Novo(idLocal);
            OnErro?.Invoke(mensagem);
            OnLoginPronto?.Invoke();
            OnProgressoCarregado?.Invoke(ProgressoAtual);
        }

        // ---------------------------------------------------------------
        // Vínculo de conta Google — recebe um idToken já obtido por um
        // plugin externo de Google Sign-In; obter esse token não é
        // responsabilidade deste script.
        // ---------------------------------------------------------------

        public void VincularContaGoogle(string idToken, Action<bool> callback = null)
        {
            if (!UsuarioLogado)
            {
                callback?.Invoke(false);
                return;
            }

            Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

            auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    OnErro?.Invoke("Não foi possível vincular sua conta Google.");
                    Debug.LogError($"Vínculo de conta falhou: {task.Exception}");
                    callback?.Invoke(false);
                    return;
                }

                callback?.Invoke(true);
            });
        }

        // ---------------------------------------------------------------
        // Progresso do jogador
        // ---------------------------------------------------------------

        public void SalvarProgresso(PlayerProgress progresso)
        {
            if (progresso == null) return;
            progresso.Uid = UserId;
            progresso.Sanitizar();

            if (!firebaseProntoParaUso || !UsuarioLogado || !AparelhoOnline())
            {
                EnfileirarParaSyncPosterior(progresso);
                return;
            }

            DocumentReference doc = db.Collection(ColecaoUsuarios).Document(UserId);
            doc.SetAsync(progresso.ParaDicionario(), SetOptions.MergeAll).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogWarning($"Falha ao salvar progresso, enfileirando para retry: {task.Exception}");
                    EnfileirarParaSyncPosterior(progresso);
                }
            });
        }

        public void CarregarProgresso()
        {
            if (!firebaseProntoParaUso || !UsuarioLogado) return;

            DocumentReference doc = db.Collection(ColecaoUsuarios).Document(UserId);
            doc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    OnErro?.Invoke("Não foi possível carregar seu progresso.");
                    Debug.LogError($"Falha ao carregar progresso: {task.Exception}");
                    return;
                }

                DocumentSnapshot snapshot = task.Result;
                PlayerProgress progresso = snapshot.Exists
                    ? PlayerProgress.DoDicionario(snapshot.ToDictionary())
                    : PlayerProgress.Novo(UserId);

                ProgressoAtual = progresso;
                OnProgressoCarregado?.Invoke(progresso);
            });
        }

        /// <summary>
        /// Última versão do progresso carregada nesta sessão — telas de UI
        /// leem daqui em vez de cada uma chamar CarregarProgresso() de novo.
        /// Pode ser nulo antes do primeiro OnProgressoCarregado.
        /// </summary>
        public PlayerProgress ProgressoAtual { get; private set; }

        /// <summary>
        /// Conveniência para telas que só precisam alterar um pedaço do
        /// progresso (ex: registrar o resultado de uma fase) sem duplicar o
        /// carregar-mutar-salvar em cada controller.
        /// </summary>
        public void AtualizarProgresso(Action<PlayerProgress> mutador)
        {
            if (ProgressoAtual == null)
            {
                OnErro?.Invoke("Progresso ainda não carregado.");
                return;
            }

            mutador(ProgressoAtual);
            ProgressoAtual.Sanitizar();
            SalvarProgresso(ProgressoAtual);
        }

        /// <summary>
        /// Atualiza só os campos de consentimento LGPD, sem precisar carregar
        /// o documento inteiro primeiro (SetOptions.MergeAll só toca nesses campos).
        /// </summary>
        public void AtualizarConsentimentoLgpd(bool aceitou)
        {
            if (!firebaseProntoParaUso || !UsuarioLogado) return;

            var dados = new Dictionary<string, object>
            {
                { "consentimentoLgpd", aceitou },
                { "consentimentoLgpdTimestampUnix", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };

            db.Collection(ColecaoUsuarios).Document(UserId).SetAsync(dados, SetOptions.MergeAll);
        }

        /// <summary>
        /// Atualiza só o campo de "Remover Anúncios" — chamado pelo
        /// PurchaseManager assim que a compra é confirmada, para sincronizar
        /// entre aparelhos sem precisar recarregar o progresso inteiro.
        /// </summary>
        public void AtualizarCompraRemoverAnuncios(bool comprado)
        {
            if (!firebaseProntoParaUso || !UsuarioLogado) return;

            var dados = new Dictionary<string, object> { { "semAnuncios", comprado } };
            db.Collection(ColecaoUsuarios).Document(UserId).SetAsync(dados, SetOptions.MergeAll);
        }

        /// <summary>
        /// Direito de eliminação (LGPD): apaga o documento de progresso, a
        /// entrada no leaderboard e a própria conta de autenticação — nessa
        /// ordem. A conta só é excluída por último porque as regras do
        /// Firestore exigem request.auth.uid == uid; excluir a conta antes
        /// invalidaria a permissão para apagar os documentos.
        /// </summary>
        public void ExcluirContaEDados(Action<bool> callback = null)
        {
            if (!UsuarioLogado)
            {
                callback?.Invoke(false);
                return;
            }

            string uid = UserId;

            db.Collection(ColecaoUsuarios).Document(uid).DeleteAsync().ContinueWithOnMainThread(taskUsuario =>
            {
                // O leaderboard agora pode ter vários documentos por jogador
                // (um por modo/temporada/desafio) — precisa consultar todos
                // antes de apagar, não dá mais para assumir um único documento
                // fixo em leaderboard/{uid}.
                db.Collection(ColecaoLeaderboard).WhereEqualTo("uid", uid).GetSnapshotAsync().ContinueWithOnMainThread(consultaTask =>
                {
                    if (!consultaTask.IsCanceled && !consultaTask.IsFaulted)
                    {
                        foreach (DocumentSnapshot doc in consultaTask.Result.Documents)
                            doc.Reference.DeleteAsync();
                    }

                    auth.CurrentUser.DeleteAsync().ContinueWithOnMainThread(taskConta =>
                    {
                        bool sucesso = !taskConta.IsCanceled && !taskConta.IsFaulted;
                        if (!sucesso)
                        {
                            Debug.LogError($"Falha ao excluir conta: {taskConta.Exception}");
                            OnErro?.Invoke("Não foi possível concluir a exclusão da conta.");
                        }
                        callback?.Invoke(sucesso);
                    });
                });
            });
        }

        // ---------------------------------------------------------------
        // Leaderboard
        // ---------------------------------------------------------------

        public void AtualizarLeaderboard(int highScore, string displayName)
        {
            AtualizarLeaderboard(highScore, displayName, AvatarCatalog.Padrao, "geral", TemporadaAtual());
        }

        public void AtualizarLeaderboard(int highScore, string displayName, string avatarId, string modo, string temporadaId)
        {
            AtualizarLeaderboard(highScore, displayName, avatarId, modo, temporadaId, 0);
        }

        public void AtualizarLeaderboard(int highScore, string displayName, string avatarId, string modo, string temporadaId, int melhorCombo)
        {
            AtualizarLeaderboard(highScore, displayName, avatarId, modo, temporadaId, melhorCombo, string.Empty);
        }

        public void AtualizarLeaderboard(int highScore, string displayName, string avatarId, string modo, string temporadaId, int melhorCombo, string challengeId)
        {
            if (!firebaseProntoParaUso || !UsuarioLogado || !AparelhoOnline()) return;

            int scoreSeguro = Mathf.Clamp(highScore, 0, 1000000000);
            string nomeSeguro = string.IsNullOrWhiteSpace(displayName) ? "Jogador" : displayName.Trim();
            if (nomeSeguro.Length > 40) nomeSeguro = nomeSeguro.Substring(0, 40);
            string avatarSeguro = AvatarCatalog.Existe(avatarId) ? avatarId.ToLowerInvariant() : AvatarCatalog.Padrao;
            string modoSeguro = string.IsNullOrWhiteSpace(modo) ? "geral" : modo.Trim();
            string temporadaSegura = string.IsNullOrWhiteSpace(temporadaId) ? TemporadaAtual() : temporadaId.Trim();
            string desafioSeguro = string.IsNullOrWhiteSpace(challengeId) ? "default" : challengeId.Trim();
            if (desafioSeguro.Length > 80) desafioSeguro = desafioSeguro.Substring(0, 80);

            var dados = new Dictionary<string, object>
            {
                { "uid", UserId },
                { "displayName", nomeSeguro },
                { "avatarId", avatarSeguro },
                { "modo", modoSeguro },
                { "temporadaId", temporadaSegura },
                { "challengeId", desafioSeguro },
                { "highScore", scoreSeguro },
                { "melhorCombo", Mathf.Clamp(melhorCombo, 0, 10000) },
                { "atualizadoEm", Timestamp.GetCurrentTimestamp() }
            };

            string documento = UserId + "_" + modoSeguro + "_" + temporadaSegura + "_" + desafioSeguro;
            db.Collection(ColecaoLeaderboard).Document(documento).SetAsync(dados, SetOptions.MergeAll);
        }

        public void BuscarTopLeaderboard(int quantidade, Action<List<(string nome, int score)>> callback)
        {
            BuscarRankingGlobal(quantidade, "geral", TemporadaAtual(), entradas =>
            {
                var legado = new List<(string nome, int score)>();
                foreach (RankingEntry entrada in entradas) legado.Add((entrada.Nome, entrada.Score));
                callback?.Invoke(legado);
            });
        }

        public void BuscarRankingGlobal(int quantidade, string modo, string temporadaId, Action<List<RankingEntry>> callback)
        {
            BuscarRankingGlobal(quantidade, modo, temporadaId, string.Empty, callback);
        }

        public void BuscarRankingGlobal(int quantidade, string modo, string temporadaId, string challengeId, Action<List<RankingEntry>> callback)
        {
            if (!firebaseProntoParaUso)
            {
                callback?.Invoke(new List<RankingEntry>());
                return;
            }

            Query consulta = db.Collection(ColecaoLeaderboard)
                .WhereEqualTo("modo", string.IsNullOrWhiteSpace(modo) ? "geral" : modo)
                .WhereEqualTo("temporadaId", string.IsNullOrWhiteSpace(temporadaId) ? TemporadaAtual() : temporadaId);

            if (!string.IsNullOrWhiteSpace(challengeId))
                consulta = consulta.WhereEqualTo("challengeId", challengeId.Trim());

            consulta = consulta.OrderByDescending("highScore")
                .Limit(Mathf.Clamp(quantidade, 1, 100));

            consulta.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                var resultado = new List<RankingEntry>();
                if (!task.IsCanceled && !task.IsFaulted)
                {
                    foreach (DocumentSnapshot doc in task.Result.Documents)
                    {
                        resultado.Add(new RankingEntry
                        {
                            Uid = doc.ContainsField("uid") ? doc.GetValue<string>("uid") : doc.Id,
                            Nome = doc.ContainsField("displayName") ? doc.GetValue<string>("displayName") : "Peregrino",
                            AvatarId = doc.ContainsField("avatarId") ? doc.GetValue<string>("avatarId") : AvatarCatalog.Padrao,
                            Modo = doc.ContainsField("modo") ? doc.GetValue<string>("modo") : "geral",
                            TemporadaId = doc.ContainsField("temporadaId") ? doc.GetValue<string>("temporadaId") : TemporadaAtual(),
                            ChallengeId = doc.ContainsField("challengeId") ? doc.GetValue<string>("challengeId") : "default",
                            Score = doc.ContainsField("highScore") ? doc.GetValue<int>("highScore") : 0,
                            MelhorCombo = doc.ContainsField("melhorCombo") ? doc.GetValue<int>("melhorCombo") : 0
                        });
                    }
                }
                callback?.Invoke(resultado);
            });
        }

        public string TemporadaAtual() => $"{System.DateTime.UtcNow:yyyy-MM}";

        // ---------------------------------------------------------------
        // Fila local — não perde progresso se o save falhar por estar offline.
        // Reaproveita PlayerPrefs por simplicidade (um único registro pendente
        // por vez é suficiente aqui: cada save novo sobrescreve o anterior).
        // ---------------------------------------------------------------

        private bool AparelhoOnline() => Application.internetReachability != NetworkReachability.NotReachable;

        private void EnfileirarParaSyncPosterior(PlayerProgress progresso)
        {
            if (progresso == null) return;
            progresso.Sanitizar();
            PlayerPrefs.SetString(ChaveFilaLocal, JsonUtility.ToJson(progresso));
            PlayerPrefs.Save();
        }

        private void SincronizarFilaLocal()
        {
            if (!PlayerPrefs.HasKey(ChaveFilaLocal)) return;
            if (!AparelhoOnline()) return;

            string json = PlayerPrefs.GetString(ChaveFilaLocal);
            PlayerPrefs.DeleteKey(ChaveFilaLocal);

            PlayerProgress pendente = JsonUtility.FromJson<PlayerProgress>(json);
            if (pendente != null) pendente.Sanitizar();
            SalvarProgresso(pendente);
        }
    }
}
