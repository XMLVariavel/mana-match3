using System;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Tela de Jogo (HUD): repassa os eventos de ScoreAndObjectiveManager e
    /// GameManager para a UI, expõe os botões de poder avulso, e cuida do
    /// que acontece ao vencer/perder (salvar resultado, notificar anúncio).
    /// Não desenha nada — a UI (texto, barras, ícones) se inscreve nos eventos.
    /// </summary>
    public class GameHUDController : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private ScoreAndObjectiveManager scoreManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private BoosterManager boosterManager;
        [SerializeField] private AdsManager adsManager;
        [SerializeField] private FirebaseManager firebaseManager;
        [SerializeField] private ScreenNavigator navigator;
        [SerializeField] private MapaDeFasesController mapaController;

        [Header("Poderes avulsos disponíveis nesta tela")]
        [SerializeField] private PowerUpConfig configMartelo;
        [SerializeField] private PowerUpConfig configEmbaralhar;
        [SerializeField] private PowerUpConfig configMaisMovimentos;

        [Header("Recompensa por vitória")]
        [SerializeField] private int moedasPorEstrela = 10;

        [Header("Nomes das telas")]
        [SerializeField] private string telaMapaDeFases = "MapaDeFases";

        public event Action<int> OnScoreAtualizado;
        public event Action<int> OnMovimentosAtualizados;
        public event Action<TileType, int> OnObjetivoAtualizado;
        public event Action<int> OnVitoria; // parâmetro = estrelas
        public event Action OnDerrota;
        public event Action<int> OnXpAtualizado;
        public event Action<string> OnMensagemPoder;
        public event Action<float> OnTempoAtualizado;
        public event Action<int> OnComboAtualizado;
        public event Action OnEstoqueAtualizado;
        public GameMode ModoAtual => gameManager != null ? gameManager.ModoAtual : GameMode.Campanha;
        public string BriefingAtual => gameManager != null ? gameManager.BriefingAtual : string.Empty;
        public string ChallengeId => gameManager != null ? gameManager.ChallengeId : "unknown";
        public bool ModoTemporizado => gameManager != null && gameManager.ModoTemporizado;
        public bool ModoUsaLimiteDeMovimentos => gameManager != null && gameManager.ModoUsaLimiteDeMovimentos;
        public float TempoRestante => gameManager != null ? gameManager.TempoRestante : 0f;
        public float TempoTotalModo => gameManager != null ? gameManager.DuracaoModoTemporizado : 90f;
        public int ScoreMetaFinal => scoreManager != null ? scoreManager.ScoreForStar3 : 0;
        public float ProgressoObjetivos => scoreManager != null ? scoreManager.ObjectivesProgress01 : 0f;
        public int ObjetivosAtual => scoreManager != null ? scoreManager.ObjectivesCurrentTotal : 0;
        public int ObjetivosTotal => scoreManager != null ? scoreManager.ObjectivesTotalRequired : 0;
        public bool TemObjetivos => scoreManager != null && scoreManager.HasObjectives;
        public int ScoreAtual => scoreManager != null ? scoreManager.CurrentScore : 0;
        public int MelhorComboAtual => gameManager != null ? gameManager.MelhorCombo : 0;
        public int XpAtual => gameManager != null ? gameManager.XpAcumulado : 0;
        public int RecompensaMoedasAtual { get; private set; }
        public bool PodeAvancarCampanha => mapaController != null && mapaController.PossuiProximaFase;
        public int EstoqueDe(PowerUpConfig config) => boosterManager != null ? boosterManager.QuantidadeDisponivel(config) : 0;
        public int EstoqueMartelo => EstoqueDe(configMartelo);
        public int EstoqueEmbaralhar => EstoqueDe(configEmbaralhar);
        public int EstoqueMaisMovimentos => EstoqueDe(configMaisMovimentos);

        private LevelData faseAtual;

        private void OnEnable()
        {
            scoreManager.OnScoreChanged += HandleScoreChanged;
            scoreManager.OnMovesChanged += HandleMovesChanged;
            scoreManager.OnObjectiveProgress += HandleObjectiveProgress;
            scoreManager.OnWin += HandleWin;
            scoreManager.OnLose += HandleLose;
            if (boosterManager != null) boosterManager.OnEstoqueChanged += HandleEstoqueChanged;

            if (gameManager != null)
            {
                gameManager.OnXpChanged += HandleXpChanged;
                gameManager.OnTempoAlterado += HandleTempoAlterado;
                gameManager.OnComboAlterado += HandleComboAlterado;
            }
            if (boardManager != null) boardManager.OnTileEscolhidaNoModoMira += HandleTileEscolhidaParaMartelo;
            if (boosterManager != null) boosterManager.OnMensagem += HandleMensagemPoder;

            // A configuração acontece antes de o ScreenNavigator mostrar a tela.
            // Reemitir aqui garante que score, movimentos e objetivos nunca
            // apareçam zerados por terem sido emitidos enquanto a HUD estava inativa.
            scoreManager.EmitCurrentState();
            if (gameManager != null) OnXpAtualizado?.Invoke(gameManager.XpAcumulado);
        }

        private void OnDisable()
        {
            scoreManager.OnScoreChanged -= HandleScoreChanged;
            scoreManager.OnMovesChanged -= HandleMovesChanged;
            scoreManager.OnObjectiveProgress -= HandleObjectiveProgress;
            scoreManager.OnWin -= HandleWin;
            scoreManager.OnLose -= HandleLose;
            if (boosterManager != null) boosterManager.OnEstoqueChanged -= HandleEstoqueChanged;

            if (gameManager != null)
            {
                gameManager.OnXpChanged -= HandleXpChanged;
                gameManager.OnTempoAlterado -= HandleTempoAlterado;
                gameManager.OnComboAlterado -= HandleComboAlterado;
            }
            if (boardManager != null) boardManager.OnTileEscolhidaNoModoMira -= HandleTileEscolhidaParaMartelo;
            if (boosterManager != null) boosterManager.OnMensagem -= HandleMensagemPoder;
        }

        public void EmitirEstadoAtual()
        {
            OnEstoqueAtualizado?.Invoke();
            if (scoreManager != null) scoreManager.EmitCurrentState();
            if (gameManager != null)
            {
                OnXpAtualizado?.Invoke(gameManager.XpAcumulado);
                OnTempoAtualizado?.Invoke(gameManager.TempoRestante);
                OnComboAtualizado?.Invoke(gameManager.ComboAtual);
            }
        }

        /// <summary>Chamado pelo MapaDeFasesController ao entrar numa fase da Campanha (nulo no Estudo Infinito).</summary>
        public void DefinirFaseAtual(LevelData fase) => faseAtual = fase;

        private void HandleScoreChanged(int score) => OnScoreAtualizado?.Invoke(score);
        private void HandleMovesChanged(int moves) => OnMovimentosAtualizados?.Invoke(moves);
        private void HandleObjectiveProgress(TileType tipo, int restante) => OnObjetivoAtualizado?.Invoke(tipo, restante);
        private void HandleXpChanged(int xp) => OnXpAtualizado?.Invoke(xp);
        private void HandleTempoAlterado(float tempo) => OnTempoAtualizado?.Invoke(tempo);
        private void HandleComboAlterado(int combo) => OnComboAtualizado?.Invoke(combo);
        private void HandleEstoqueChanged() => OnEstoqueAtualizado?.Invoke();
        private void HandleMensagemPoder(string mensagem) => OnMensagemPoder?.Invoke(mensagem);

        private void HandleWin(int estrelas)
        {
            if (firebaseManager != null)
            {
                firebaseManager.AtualizarProgresso(p =>
                {
                    if (faseAtual != null) p.RegistrarResultadoDaFase(faseAtual.Numero, estrelas);
                    if (scoreManager.CurrentScore > p.HighScore) p.HighScore = scoreManager.CurrentScore;
                });
                PublicarScoreCompetitivo();
            }

            RecompensaMoedasAtual = estrelas * moedasPorEstrela;
            if (gameManager != null && gameManager.ModoAtual == GameMode.Campanha)
                RecompensaMoedasAtual += Mathf.Min(10, Mathf.Max(0, scoreManager.MovesRemaining) * 2);

            OnVitoria?.Invoke(estrelas);
            boosterManager?.AdicionarMoedas(RecompensaMoedasAtual);
            OnMensagemPoder?.Invoke($"Recompensa: +{RecompensaMoedasAtual} moedas");
        }

        private void HandleLose()
        {
            adsManager?.NotificarDerrota();
            PublicarScoreCompetitivo();
            OnDerrota?.Invoke();
        }

        private void PublicarScoreCompetitivo()
        {
            if (firebaseManager == null || firebaseManager.ProgressoAtual == null || scoreManager == null) return;
            string modo = gameManager != null ? gameManager.ModoAtual.ToString() : "geral";
            string challengeId = gameManager != null ? gameManager.ChallengeId : "default";
            PlayerProgress progresso = firebaseManager.ProgressoAtual;
            string temporada = firebaseManager.TemporadaAtual();
            int melhorCombo = gameManager != null ? gameManager.MelhorCombo : 0;

            firebaseManager.AtualizarProgresso(p =>
            {
                p.MelhorCombo = Mathf.Max(p.MelhorCombo, melhorCombo);
                if (gameManager != null && gameManager.ModoAtual == GameMode.EstudoInfinito)
                    p.MelhorScoreEstudoInfinito = Mathf.Max(p.MelhorScoreEstudoInfinito, scoreManager.CurrentScore);
                if (gameManager != null && gameManager.ModoAtual == GameMode.DesafioDiario)
                {
                    string hoje = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
                    if (p.UltimoDesafioDiario != hoje)
                    {
                        p.SequenciaDesafioDiario++;
                        p.UltimoDesafioDiario = hoje;
                    }
                }
            });
            firebaseManager.AtualizarLeaderboard(
                scoreManager.CurrentScore,
                progresso.DisplayName,
                progresso.AvatarId,
                modo,
                temporada,
                melhorCombo,
                challengeId);

            if (modo != "geral")
            {
                firebaseManager.AtualizarLeaderboard(
                    scoreManager.CurrentScore,
                    progresso.DisplayName,
                    progresso.AvatarId,
                    "geral",
                    temporada,
                    melhorCombo,
                challengeId);
            }
        }

        /// <summary>Botão de sair/voltar (também usado pelos modais de vitória/derrota).</summary>
        public void VoltarParaMapa()
        {
            faseAtual = null;
            navigator?.Mostrar(telaMapaDeFases);
        }

        public void AvancarParaProximaFase()
        {
            if (mapaController != null && mapaController.PossuiProximaFase)
                mapaController.EntrarNaProximaFase();
            else
                VoltarParaMapa();
        }

        // ---------------------------------------------------------------
        // Poderes avulsos
        // ---------------------------------------------------------------

        public void UsarEmbaralhar()
        {
            if (boosterManager == null || configEmbaralhar == null)
            {
                OnMensagemPoder?.Invoke("Embaralhar está indisponível nesta sessão.");
                return;
            }

            boosterManager.TentarUsar(configEmbaralhar);
        }

        public void UsarMaisMovimentos()
        {
            if (ModoAtual == GameMode.EstudoInfinito || ModoAtual == GameMode.ContraRelogio)
            {
                OnMensagemPoder?.Invoke("Este desafio não usa limite de movimentos.");
                return;
            }

            if (boosterManager == null || configMaisMovimentos == null)
            {
                OnMensagemPoder?.Invoke("+5 Movimentos está indisponível nesta sessão.");
                return;
            }

            boosterManager.TentarUsar(configMaisMovimentos);
        }

        /// <summary>
        /// Botão do Martelo: entra em "modo mira" no tabuleiro — o próximo
        /// toque escolhe a peça-alvo (ver BoardManager.AtivarModoMira).
        /// </summary>
        public void AtivarMartelo()
        {
            if (boosterManager == null || !boosterManager.PodeUsar(configMartelo))
            {
                OnMensagemPoder?.Invoke("O Martelo está indisponível ou sem moedas.");
                return;
            }

            boardManager?.AtivarModoMira();
            OnMensagemPoder?.Invoke("Toque em uma peça para usar o Martelo.");
        }

        private void HandleTileEscolhidaParaMartelo(Tile tile)
        {
            if (configMartelo == null || tile == null || boosterManager == null) return;
            boosterManager.TentarUsar(configMartelo, tile.X, tile.Y);
        }
    }
}
