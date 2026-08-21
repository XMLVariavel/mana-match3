using System;
using System.Collections.Generic;
using UnityEngine;

namespace BibleMatch3
{
    public enum GameMode
    {
        Campanha,
        EstudoInfinito,
        DesafioDiario,
        ContraRelogio,
        GuardiaoDaPalavra
    }

    /// <summary>
    /// Orquestra o ciclo de jogo: carrega uma fase da Campanha OU inicia o
    /// Estudo Infinito, e — só no modo Infinito — escuta a pontuação para
    /// escalar a dificuldade (mais tipos de peça, obstáculos mais frequentes)
    /// e disparar o Card de Versículo a cada marco de pontos.
    /// Não conhece UI diretamente — expõe eventos para o Canvas se inscrever.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private ScoreAndObjectiveManager scoreManager;
        [SerializeField] private ObstacleManager obstacleManager;

        [Header("Contra o Relógio")]
        [SerializeField, Min(1f)] private float duracaoContraRelogio = 90f;

        [Header("Estudo Infinito — Variedade de Peças")]
        [Tooltip("Quantos tipos de peça ficam ativos no início do Estudo Infinito (os primeiros N do enum TileType).")]
        [SerializeField] private int tiposIniciaisInfinito = 3;
        [SerializeField] private int pontosPorEscalonamento = 500;

        [Header("Estudo Infinito — Obstáculos Dinâmicos")]
        [SerializeField] private float chanceBaseDeObstaculo = 0.05f; // por match, no nível de dificuldade 0
        [SerializeField] private float incrementoChancePorNivel = 0.02f;
        [SerializeField] private ObstacleType[] obstaculosSorteaveis = { ObstacleType.Gelo, ObstacleType.Corrente, ObstacleType.CaixaSelada };
        [SerializeField, Min(0)] private int limiteObstaculosInfinito = 10;
        [SerializeField, Min(0f)] private float intervaloMinimoEntreObstaculos = 0.35f;
        [SerializeField, Min(0.1f)] private float janelaDoCombo = 0.75f;

        [Header("Estudo Infinito — Versículos")]
        [SerializeField] private int pontosPorVersiculo = 1000;
        [SerializeField] private List<VerseData> versiculosDisponiveis;
        [SerializeField] private int xpBonusPorVersiculo = 50;

        public GameMode ModoAtual { get; private set; }
        public ModeChallengeDefinition DesafioAtual { get; private set; }
        public string ChallengeId => DesafioAtual != null ? DesafioAtual.ChallengeId : "unknown";
        public string BriefingAtual => DesafioAtual != null ? DesafioAtual.Briefing : string.Empty;
        public int XpAcumulado { get; private set; }
        public int NivelDificuldadeAtual { get; private set; }

        public event Action<VerseData> OnVersiculoExibido;
        public event Action<int> OnDificuldadeAumentou; // parâmetro = novo nível de dificuldade
        public event Action<int> OnXpChanged;
        public event Action<float> OnTempoAlterado;
        public event Action<int> OnComboAlterado;

        public float TempoRestante { get; private set; }
        public int ComboAtual { get; private set; }
        public int MelhorCombo { get; private set; }
        public float SegundosDesdeJogadaValida { get; private set; } = float.PositiveInfinity;
        public int UltimoBonusCompetitivo { get; private set; }
        public bool ModoTemporizado => ModoAtual == GameMode.ContraRelogio;
        public bool ModoUsaLimiteDeMovimentos => ModoAtual == GameMode.Campanha ||
                                                   ModoAtual == GameMode.DesafioDiario ||
                                                   ModoAtual == GameMode.GuardiaoDaPalavra;
        public float DuracaoModoTemporizado => duracaoContraRelogio;

        private int ultimoMarcoVersiculo;
        private int ultimoMarcoDificuldade;
        private float proximoObstaculoPermitido;
        private float ultimoPontoEm = -100f;
        private bool timerAtivo;
        private List<VerseData> versiculosRestantes;

        private void OnEnable()
        {
            if (scoreManager != null) scoreManager.OnScoreChanged += HandleScoreChanged;
        }

        private void OnDisable()
        {
            if (scoreManager != null) scoreManager.OnScoreChanged -= HandleScoreChanged;
        }

        private void Update()
        {
            if (ComboAtual > 0 && Time.unscaledTime - ultimoPontoEm > janelaDoCombo)
            {
                ComboAtual = 0;
                OnComboAlterado?.Invoke(ComboAtual);
            }

            if (!ModoTemporizado || !timerAtivo || scoreManager == null || scoreManager.LevelEnded) return;

            TempoRestante = Mathf.Max(0f, TempoRestante - Time.unscaledDeltaTime);
            OnTempoAlterado?.Invoke(TempoRestante);
            if (TempoRestante <= 0f)
            {
                timerAtivo = false;
                scoreManager.EncerrarPorTempo();
            }
        }

        /// <summary>
        /// Carrega uma fase da Campanha: configura movimentos/objetivos/estrelas
        /// e posiciona os obstáculos definidos no LevelData.
        /// </summary>
        public void IniciarCampanha(LevelData fase)
        {
            if (fase == null)
            {
                Debug.LogWarning("IniciarCampanha chamado com LevelData nulo.");
                return;
            }

            DesafioAtual = ModeChallengeDefinition.Campaign(fase);
            ModoAtual = GameMode.Campanha;
            timerAtivo = false;
            ResetarCombo();
            boardManager.DefinirTiposAtivos(new List<TileType>((TileType[])Enum.GetValues(typeof(TileType))));
            boardManager.ReiniciarTabuleiro();
            scoreManager.Configurar(DesafioAtual.Moves, DesafioAtual.Objectives, DesafioAtual.Star1, DesafioAtual.Star2, DesafioAtual.Star3);
            AplicarObstaculosDaFase(fase);
        }

        /// <summary>
        /// Inicia o Estudo Infinito: sem limite de movimentos nem objetivos fixos,
        /// começando com poucos tipos de peça e escalando a dificuldade pela pontuação.
        /// </summary>
        public void IniciarEstudoInfinito()
        {
            ModoAtual = GameMode.EstudoInfinito;
            DesafioAtual = new ModeChallengeDefinition
            {
                Mode = ModoAtual,
                ChallengeId = $"infinite-{System.DateTime.UtcNow:yyyyMMddHHmmss}",
                UsesMoves = false,
                UsesObjectives = false,
                UsesTimer = false,
                Briefing = "Aumente o combo, ganhe XP e descubra versículos."
            };
            timerAtivo = false;
            TempoRestante = 0f;
            ResetarCombo();

            ultimoMarcoVersiculo = 0;
            ultimoMarcoDificuldade = 0;
            proximoObstaculoPermitido = 0f;
            NivelDificuldadeAtual = 0;
            versiculosRestantes = versiculosDisponiveis != null ? new List<VerseData>(versiculosDisponiveis) : new List<VerseData>();

            scoreManager.Configurar(int.MaxValue, null, 0, 0, 0);
            AtualizarVariedadeDePecas();
            boardManager.ReiniciarTabuleiro();
        }

        public void IniciarDesafioDiario()
        {
            DesafioAtual = ModeChallengeDefinition.Daily(System.DateTime.UtcNow.Date);
            ModoAtual = GameMode.DesafioDiario;
            timerAtivo = false;
            ResetarCombo();
            UnityEngine.Random.InitState(DesafioAtual.Seed);
            TempoRestante = 0f;
            scoreManager.Configurar(DesafioAtual.Moves, DesafioAtual.Objectives, DesafioAtual.Star1, DesafioAtual.Star2, DesafioAtual.Star3);
            boardManager.DefinirTiposAtivos(new List<TileType>((TileType[])Enum.GetValues(typeof(TileType))));
            boardManager.ReiniciarTabuleiro();
        }

        public void IniciarContraRelogio()
        {
            DesafioAtual = ModeChallengeDefinition.TimeTrial(duracaoContraRelogio);
            ModoAtual = GameMode.ContraRelogio;
            ResetarCombo();
            TempoRestante = DesafioAtual.DurationSeconds;
            timerAtivo = true;
            scoreManager.Configurar(DesafioAtual.Moves, null, 0, 0, 0);
            boardManager.DefinirTiposAtivos(new List<TileType>((TileType[])Enum.GetValues(typeof(TileType))));
            boardManager.ReiniciarTabuleiro();
            OnTempoAlterado?.Invoke(TempoRestante);
        }

        public void IniciarGuardiaoDaPalavra()
        {
            DesafioAtual = ModeChallengeDefinition.Guardian(System.DateTime.UtcNow.Date);
            ModoAtual = GameMode.GuardiaoDaPalavra;
            timerAtivo = false;
            ResetarCombo();
            TempoRestante = 0f;
            scoreManager.Configurar(DesafioAtual.Moves, DesafioAtual.Objectives, DesafioAtual.Star1, DesafioAtual.Star2, DesafioAtual.Star3);
            boardManager.DefinirTiposAtivos(new List<TileType>((TileType[])Enum.GetValues(typeof(TileType))));
            boardManager.ReiniciarTabuleiro();
        }

        private int SementeDoDia()
        {
            System.DateTime hoje = System.DateTime.UtcNow.Date;
            return hoje.Year * 10000 + hoje.Month * 100 + hoje.Day;
        }

        private void AplicarObstaculosDaFase(LevelData fase)
        {
            if (fase.Obstaculos == null || obstacleManager == null) return;

            foreach (ObstaculoPosicionado obstaculo in fase.Obstaculos)
            {
                if (obstaculo.Tipo == ObstacleType.PedraDeserto)
                    boardManager.RemoverPecaEBloquear(obstaculo.X, obstaculo.Y);

                obstacleManager.PlaceObstacle(obstaculo.Tipo, obstaculo.X, obstaculo.Y,
                    boardManager.WorldPosition(obstaculo.X, obstaculo.Y));
            }
        }

        private void HandleScoreChanged(int novoScore)
        {
            if (ModoAtual != GameMode.EstudoInfinito) return;

            VerificarMarcoDeVersiculo(novoScore);
            VerificarEscalonamentoDeDificuldade(novoScore);
            TentarInserirObstaculoAleatorio();
        }

        public void RegistrarJogadaValida()
        {
            float agora = Time.unscaledTime;
            SegundosDesdeJogadaValida = ultimoPontoEm > -50f
                ? Mathf.Max(0f, agora - ultimoPontoEm)
                : float.PositiveInfinity;
            AtualizarCombo(agora);
        }

        private void AtualizarCombo(float agora)
        {
            ComboAtual = SegundosDesdeJogadaValida <= janelaDoCombo ? ComboAtual + 1 : 1;
            ultimoPontoEm = agora;
            MelhorCombo = Mathf.Max(MelhorCombo, ComboAtual);
            UltimoBonusCompetitivo = CompetitiveScoreRules.BonusDeSequencia(ComboAtual);
            OnComboAlterado?.Invoke(ComboAtual);
        }

        private void ResetarCombo()
        {
            ComboAtual = 0;
            MelhorCombo = 0;
            UltimoBonusCompetitivo = 0;
            SegundosDesdeJogadaValida = float.PositiveInfinity;
            ultimoPontoEm = -100f;
            OnComboAlterado?.Invoke(ComboAtual);
        }

        // ---------------------------------------------------------------
        // Card de Versículo
        // ---------------------------------------------------------------

        private void VerificarMarcoDeVersiculo(int score)
        {
            if (pontosPorVersiculo <= 0) return;

            int marcoAtual = score / pontosPorVersiculo;
            if (marcoAtual <= ultimoMarcoVersiculo) return;

            ultimoMarcoVersiculo = marcoAtual;
            ExibirProximoVersiculo();
        }

        private void ExibirProximoVersiculo()
        {
            if (versiculosRestantes == null || versiculosRestantes.Count == 0)
            {
                if (versiculosDisponiveis == null || versiculosDisponiveis.Count == 0) return;
                versiculosRestantes = new List<VerseData>(versiculosDisponiveis); // recomeça o ciclo
            }

            int indice = UnityEngine.Random.Range(0, versiculosRestantes.Count);
            VerseData versiculo = versiculosRestantes[indice];
            versiculosRestantes.RemoveAt(indice);

            XpAcumulado += xpBonusPorVersiculo;
            OnXpChanged?.Invoke(XpAcumulado);
            OnVersiculoExibido?.Invoke(versiculo);
        }

        // ---------------------------------------------------------------
        // Dificuldade dinâmica: mais tipos de peça
        // ---------------------------------------------------------------

        private void VerificarEscalonamentoDeDificuldade(int score)
        {
            if (pontosPorEscalonamento <= 0) return;

            int marcoAtual = score / pontosPorEscalonamento;
            if (marcoAtual <= ultimoMarcoDificuldade) return;

            ultimoMarcoDificuldade = marcoAtual;
            NivelDificuldadeAtual++;

            AtualizarVariedadeDePecas();
            OnDificuldadeAumentou?.Invoke(NivelDificuldadeAtual);
        }

        private void AtualizarVariedadeDePecas()
        {
            var todos = (TileType[])Enum.GetValues(typeof(TileType));
            int quantidadeAtiva = Mathf.Clamp(tiposIniciaisInfinito + NivelDificuldadeAtual, 1, todos.Length);

            var ativos = new List<TileType>();
            for (int i = 0; i < quantidadeAtiva; i++) ativos.Add(todos[i]);

            boardManager.DefinirTiposAtivos(ativos);
        }

        // ---------------------------------------------------------------
        // Dificuldade dinâmica: obstáculos mais frequentes
        // ---------------------------------------------------------------

        private void TentarInserirObstaculoAleatorio()
        {
            if (obstacleManager == null || obstaculosSorteaveis == null || obstaculosSorteaveis.Length == 0) return;
            if (limiteObstaculosInfinito > 0 && obstacleManager.QuantidadeAtiva >= limiteObstaculosInfinito) return;
            if (Time.unscaledTime < proximoObstaculoPermitido) return;

            float chance = Mathf.Clamp01(chanceBaseDeObstaculo + incrementoChancePorNivel * NivelDificuldadeAtual);
            if (UnityEngine.Random.value > chance) return;

            int x = UnityEngine.Random.Range(0, boardManager.Width);
            int y = UnityEngine.Random.Range(0, boardManager.Height);

            if (boardManager.Grid[x, y] == null) return;           // célula vazia/bloqueada
            if (obstacleManager.GetObstacle(x, y) != null) return; // já tem obstáculo ali

            ObstacleType tipo = obstaculosSorteaveis[UnityEngine.Random.Range(0, obstaculosSorteaveis.Length)];
            obstacleManager.PlaceObstacle(tipo, x, y, boardManager.WorldPosition(x, y));
            proximoObstaculoPermitido = Time.unscaledTime + intervaloMinimoEntreObstaculos;
        }
    }
}
