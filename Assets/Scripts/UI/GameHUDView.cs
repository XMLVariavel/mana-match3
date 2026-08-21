using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BibleMatch3
{
    /// <summary>
    /// Camada de desenho do HUD da Tela de Jogo. Só escuta os eventos do
    /// <see cref="GameHUDController"/> e escreve nos textos/painéis — nenhuma
    /// regra de jogo mora aqui, e por isso este script pode ser trocado sem
    /// tocar em nada de gameplay.
    /// </summary>
    public class GameHUDView : MonoBehaviour
    {
        [Header("Origem")]
        [SerializeField] private GameHUDController controller;

        [Header("Textos do HUD")]
        [SerializeField] private TextMeshProUGUI textoScore;
        [SerializeField] private TextMeshProUGUI textoMovimentos;
        [SerializeField] private TextMeshProUGUI textoMovimentosTitulo;
        [SerializeField] private TextMeshProUGUI textoXp;
        [SerializeField] private TextMeshProUGUI textoModo;
        [SerializeField] private TextMeshProUGUI textoBriefing;
        [SerializeField] private TextMeshProUGUI textoProgressoTitulo;
        [SerializeField] private TextMeshProUGUI textoTempo;
        [SerializeField] private TextMeshProUGUI textoCombo;
        [SerializeField] private TextMeshProUGUI textoStatusPoderes;
        [SerializeField] private TextMeshProUGUI contadorMartelo;
        [SerializeField] private TextMeshProUGUI contadorEmbaralhar;
        [SerializeField] private TextMeshProUGUI contadorMaisMovimentos;
        [SerializeField] private Image barraProgresso;
        [SerializeField] private TextMeshProUGUI textoMetaProgresso;
        [SerializeField] private Button botaoMaisMovimentos;
        [SerializeField] private GameObject painelObjetivos;
        [SerializeField] private int metaProgresso = 20000;
        [SerializeField] private float metaTempo = 90f;

        [Header("Objetivos")]
        [SerializeField] private Transform containerObjetivos;
        [SerializeField] private ItemObjetivoUI itemObjetivoPrefab;
        [Tooltip("Ícone de cada peça, na ordem do enum TileType (Pao, Peixe, Uva, Espiga, Azeite, Pomba).")]
        [SerializeField] private Sprite[] iconesPorTipo = new Sprite[6];

        [Header("Fim de fase")]
        [SerializeField] private GameObject painelVitoria;
        [SerializeField] private GameObject painelDerrota;
        [SerializeField] private StarRatingView estrelasVitoria;
        [SerializeField] private TextMeshProUGUI textoResultadoDetalhes;
        [SerializeField] private Button botaoProximaFase;

        [Header("Formatação")]
        [SerializeField] private string formatoScore = "{0}";
        [SerializeField] private string formatoMovimentos = "{0}";
        [SerializeField] private string formatoXp = "XP {0}";
        [SerializeField] private string textoMovimentosIlimitados = "∞";

        private readonly Dictionary<TileType, ItemObjetivoUI> itensDeObjetivo = new Dictionary<TileType, ItemObjetivoUI>();

        private void OnEnable()
        {
            if (controller == null) return;

            controller.OnScoreAtualizado += HandleScore;
            controller.OnMovimentosAtualizados += HandleMovimentos;
            controller.OnObjetivoAtualizado += HandleObjetivo;
            controller.OnXpAtualizado += HandleXp;
            controller.OnTempoAtualizado += HandleTempo;
            controller.OnComboAtualizado += HandleCombo;
            controller.OnMensagemPoder += HandleMensagemPoder;
            controller.OnEstoqueAtualizado += AtualizarEstoque;
            controller.OnVitoria += HandleVitoria;
            controller.OnDerrota += HandleDerrota;

            EsconderPaineisDeFim();
            AtualizarModo();
            controller.EmitirEstadoAtual();
        }

        private void OnDisable()
        {
            if (controller == null) return;

            controller.OnScoreAtualizado -= HandleScore;
            controller.OnMovimentosAtualizados -= HandleMovimentos;
            controller.OnObjetivoAtualizado -= HandleObjetivo;
            controller.OnXpAtualizado -= HandleXp;
            controller.OnTempoAtualizado -= HandleTempo;
            controller.OnComboAtualizado -= HandleCombo;
            controller.OnMensagemPoder -= HandleMensagemPoder;
            controller.OnEstoqueAtualizado -= AtualizarEstoque;
            controller.OnVitoria -= HandleVitoria;
            controller.OnDerrota -= HandleDerrota;

            LimparObjetivos();
        }

        private void EsconderPaineisDeFim()
        {
            if (painelVitoria != null) painelVitoria.SetActive(false);
            if (painelDerrota != null) painelDerrota.SetActive(false);
        }

        private void HandleScore(int score)
        {
            if (textoScore != null) textoScore.text = string.Format(formatoScore, score);
            AtualizarIndicadorDeProgresso(score);
        }

        private void AtualizarModo()
        {
            if (controller == null) return;

            GameMode modo = controller.ModoAtual;
            if (textoModo != null)
            {
                textoModo.text = modo switch
                {
                    GameMode.EstudoInfinito => "ESTUDO INFINITO  •  PONTOS + XP",
                    GameMode.DesafioDiario => "DESAFIO DIÁRIO  •  MISSÃO DE HOJE",
                    GameMode.ContraRelogio => $"CONTRA O RELÓGIO  •  {Mathf.CeilToInt(controller.TempoTotalModo)} SEGUNDOS",
                    GameMode.GuardiaoDaPalavra => "GUARDIÃO DA PALAVRA  •  CUMPRA OS OBJETIVOS",
                    _ => "CAMPANHA  •  OBJETIVOS DA FASE"
                };
            }

            if (textoBriefing != null)
                textoBriefing.text = controller.BriefingAtual;

            bool usaMovimentos = controller.ModoUsaLimiteDeMovimentos;
            if (textoMovimentosTitulo != null)
                textoMovimentosTitulo.text = usaMovimentos ? "MOVIMENTOS" : "COMBO";
            if (textoMovimentos != null)
                textoMovimentos.gameObject.SetActive(usaMovimentos || modo == GameMode.EstudoInfinito || modo == GameMode.ContraRelogio);
            if (botaoMaisMovimentos != null)
            {
                botaoMaisMovimentos.interactable = usaMovimentos;
                botaoMaisMovimentos.gameObject.SetActive(usaMovimentos);
            }
            if (painelObjetivos != null)
                painelObjetivos.SetActive(controller.TemObjetivos);
            if (textoTempo != null)
                textoTempo.gameObject.SetActive(modo == GameMode.ContraRelogio);
            if (textoProgressoTitulo != null)
                textoProgressoTitulo.text = modo switch
                {
                    GameMode.ContraRelogio => "TEMPO",
                    GameMode.EstudoInfinito => "XP",
                    GameMode.GuardiaoDaPalavra => "OBJETIVOS",
                    _ => "PROGRESSO"
                };
            if (textoStatusPoderes != null)
                textoStatusPoderes.text = modo switch
                {
                    GameMode.ContraRelogio => "Faça combos antes que o tempo termine",
                    GameMode.EstudoInfinito => "Aumente o combo para ganhar XP",
                    GameMode.DesafioDiario => "Complete a missão de hoje",
                    GameMode.GuardiaoDaPalavra => "Cumpra todos os objetivos da Palavra",
                    _ => "Escolha uma jogada especial"
                };

            AtualizarIndicadorDeProgresso(0);
        }

        private void AtualizarIndicadorDeProgresso(int score)
        {
            if (controller == null) return;

            switch (controller.ModoAtual)
            {
                case GameMode.ContraRelogio:
                    AtualizarTempoVisual(controller.TempoRestante);
                    break;
                case GameMode.GuardiaoDaPalavra:
                    if (barraProgresso != null) barraProgresso.fillAmount = controller.ProgressoObjetivos;
                    if (textoMetaProgresso != null)
                        textoMetaProgresso.text = $"{controller.ObjetivosAtual} / {controller.ObjetivosTotal}";
                    break;
                case GameMode.EstudoInfinito:
                    if (barraProgresso != null) barraProgresso.fillAmount = Mathf.Clamp01(score / (float)metaProgresso);
                    if (textoMetaProgresso != null) textoMetaProgresso.text = $"{score:N0} XP";
                    break;
                default:
                    int meta = controller.ScoreMetaFinal > 0 ? controller.ScoreMetaFinal : metaProgresso;
                    if (barraProgresso != null) barraProgresso.fillAmount = meta > 0 ? Mathf.Clamp01(score / (float)meta) : 0f;
                    if (textoMetaProgresso != null) textoMetaProgresso.text = $"{score:N0} / {meta:N0}";
                    break;
            }
        }

        private void HandleTempo(float tempo)
        {
            AtualizarTempoVisual(tempo);
        }

        private void AtualizarTempoVisual(float tempo)
        {
            if (controller == null || controller.ModoAtual != GameMode.ContraRelogio) return;
            int segundos = Mathf.Max(0, Mathf.CeilToInt(tempo));
            if (textoTempo != null)
            {
                textoTempo.text = $"{segundos / 60:00}:{segundos % 60:00}";
                textoTempo.gameObject.SetActive(segundos > 0);
            }
            float duracao = controller.TempoTotalModo > 0f ? controller.TempoTotalModo : metaTempo;
            if (barraProgresso != null)
                barraProgresso.fillAmount = duracao > 0f ? Mathf.Clamp01(tempo / duracao) : 0f;
            if (textoMetaProgresso != null)
                textoMetaProgresso.text = $"{segundos}s RESTANTES";
        }

        private void HandleCombo(int combo)
        {
            if (controller != null && !controller.ModoUsaLimiteDeMovimentos && textoMovimentos != null)
                textoMovimentos.text = $"x{Mathf.Max(0, combo)}";

            if (textoCombo == null) return;
            textoCombo.text = combo > 1 ? $"COMBO x{combo}" : string.Empty;
            textoCombo.gameObject.SetActive(combo > 1 && controller != null && controller.ModoUsaLimiteDeMovimentos);
        }

        private void HandleMensagemPoder(string mensagem)
        {
            if (textoStatusPoderes != null) textoStatusPoderes.text = mensagem;
        }

        private void AtualizarEstoque()
        {
            if (controller == null) return;
            if (contadorMartelo != null) contadorMartelo.text = controller.EstoqueMartelo.ToString();
            if (contadorEmbaralhar != null) contadorEmbaralhar.text = controller.EstoqueEmbaralhar.ToString();
            if (contadorMaisMovimentos != null) contadorMaisMovimentos.text = controller.EstoqueMaisMovimentos.ToString();
        }

        private void HandleMovimentos(int movimentos)
        {
            if (textoMovimentos == null || controller == null) return;

            if (!controller.ModoUsaLimiteDeMovimentos)
            {
                // Estudo Infinito e Contra o Relógio mostram combo no card
                // central; nenhum deles possui contador de jogadas.
                textoMovimentos.text = "x0";
                return;
            }

            textoMovimentos.text = string.Format(formatoMovimentos, Mathf.Max(0, movimentos));
        }

        private void HandleXp(int xp)
        {
            if (textoXp != null) textoXp.text = string.Format(formatoXp, xp);
        }

        private void HandleObjetivo(TileType tipo, int restante)
        {
            if (containerObjetivos == null || itemObjetivoPrefab == null) return;

            if (!itensDeObjetivo.TryGetValue(tipo, out ItemObjetivoUI item) || item == null)
            {
                item = Instantiate(itemObjetivoPrefab, containerObjetivos);
                item.Configurar(tipo, IconePara(tipo), restante);
                itensDeObjetivo[tipo] = item;
                return;
            }

            item.Atualizar(restante);
        }

        private Sprite IconePara(TileType tipo)
        {
            int indice = (int)tipo;
            if (iconesPorTipo == null || indice < 0 || indice >= iconesPorTipo.Length) return null;
            return iconesPorTipo[indice];
        }

        private void LimparObjetivos()
        {
            foreach (KeyValuePair<TileType, ItemObjetivoUI> par in itensDeObjetivo)
                if (par.Value != null) Destroy(par.Value.gameObject);

            itensDeObjetivo.Clear();
        }

        private void HandleVitoria(int estrelas)
        {
            if (estrelasVitoria != null)
                estrelasVitoria.Definir(estrelas);
            AtualizarResumoSessao("DESAFIO CONCLUÍDO");
            if (botaoProximaFase != null)
                botaoProximaFase.gameObject.SetActive(controller != null && controller.PodeAvancarCampanha);
            if (painelVitoria != null) painelVitoria.SetActive(true);
        }

        private void HandleDerrota()
        {
            AtualizarResumoSessao("SESSÃO ENCERRADA");
            if (botaoProximaFase != null) botaoProximaFase.gameObject.SetActive(false);
            if (painelDerrota != null) painelDerrota.SetActive(true);
        }

        private void AtualizarResumoSessao(string estado)
        {
            if (textoResultadoDetalhes == null || controller == null) return;
            textoResultadoDetalhes.text =
                $"{estado}\n" +
                $"{controller.BriefingAtual}\n\n" +
                $"PONTOS  {controller.ScoreAtual:N0}   •   MELHOR COMBO  x{controller.MelhorComboAtual}\n" +
                $"XP  {controller.XpAtual:N0}   •   MOEDAS  +{controller.RecompensaMoedasAtual}";
        }
    }
}
