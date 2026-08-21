using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Ponto de entrada para a UI usar um poder avulso (Martelo, Embaralhar,
    /// +5 Movimentos) ou evoluir um especial de tabuleiro (Espada/Tocha/
    /// Arca/Estrela Guia) gastando moeda. A moeda é só ganha jogando —
    /// nunca vendida — e persiste via FirebaseManager/PlayerProgress.
    /// </summary>
    public class BoosterManager : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private MatchDetector matchDetector;
        [SerializeField] private BoardPhysics boardPhysics;
        [SerializeField] private ScoreAndObjectiveManager scoreManager;
        [SerializeField] private FirebaseManager firebaseManager;
        [SerializeField] private List<PowerUpConfig> configsAvulsos = new List<PowerUpConfig>();

        private int moedas;
        private readonly Dictionary<string, int> estoque = new Dictionary<string, int>();

        public int Moedas => moedas;
        public int QuantidadeDisponivel(PowerUpConfig config)
        {
            if (config == null) return 0;
            return estoque.TryGetValue(Id(config), out int quantidade) ? quantidade : 0;
        }
        public event Action<int> OnMoedasChanged;
        public event Action<string> OnMensagem;
        public event Action OnEstoqueChanged;

        public bool PodeUsar(PowerUpConfig config)
        {
            config = RecuperarConfigAvulso(config);
            return config != null && QuantidadeDisponivel(config) > 0;
        }

        public bool ComprarAvulso(PowerUpConfig config)
        {
            config = RecuperarConfigAvulso(config);
            if (config == null || config.Tipo != TipoPoder.Avulso) return false;
            if (moedas < config.CustoMoedas)
            {
                OnMensagem?.Invoke($"Você precisa de {config.CustoMoedas} moedas para comprar {config.NomeExibicao}.");
                return false;
            }

            moedas -= config.CustoMoedas;
            estoque[Id(config)] = QuantidadeDisponivel(config) + Mathf.Max(1, config.QuantidadePorCompra);
            OnMoedasChanged?.Invoke(moedas);
            OnEstoqueChanged?.Invoke();
            Persistir();
            OnMensagem?.Invoke($"Comprado: +{config.QuantidadePorCompra} {config.NomeExibicao}.");
            return true;
        }

        private void Start()
        {
            GarantirEstoqueInicial(null);
        }

        private void OnEnable()
        {
            if (firebaseManager != null) firebaseManager.OnProgressoCarregado += HandleProgressoCarregado;
        }

        private void OnDisable()
        {
            if (firebaseManager != null) firebaseManager.OnProgressoCarregado -= HandleProgressoCarregado;
        }

        private void HandleProgressoCarregado(PlayerProgress progresso)
        {
            moedas = progresso != null ? progresso.Moedas : 0;
            estoque.Clear();
            if (progresso != null && progresso.EstoquePoderes != null)
            {
                foreach (PowerStockEntry item in progresso.EstoquePoderes)
                    if (item != null && !string.IsNullOrWhiteSpace(item.PowerId))
                        estoque[item.PowerId] = Mathf.Clamp(item.Quantidade, 0, 999);
            }
            GarantirEstoqueInicial(progresso);
            OnMoedasChanged?.Invoke(moedas);
            OnEstoqueChanged?.Invoke();
        }

        public void AdicionarMoedas(int quantidade)
        {
            if (quantidade <= 0) return;

            moedas += quantidade;
            OnMoedasChanged?.Invoke(moedas);
            Persistir();
        }

        /// <summary>
        /// Gasta moeda para evoluir um especial de tabuleiro para o próximo
        /// nível (ex: Tocha Acesa Nv.1 → Nv.2). Retorna false sem cobrar nada
        /// se faltar moeda ou o poder já estiver no nível máximo.
        /// </summary>
        public bool EvoluirPoder(PowerUpConfig config)
        {
            if (config == null || config.Tipo != TipoPoder.EspecialDeTabuleiro || !config.PodeEvoluir)
                return false;

            int custo = config.CustoEvolucaoProximoNivel;
            if (custo < 0 || moedas < custo) return false;

            moedas -= custo;
            config.Evoluir();
            OnMoedasChanged?.Invoke(moedas);
            Persistir();
            return true;
        }

        private void Persistir()
        {
            if (firebaseManager != null && firebaseManager.UsuarioLogado)
            {
                firebaseManager.AtualizarProgresso(p =>
                {
                    p.Moedas = moedas;
                    foreach (PowerUpConfig config in configsAvulsos)
                        if (config != null) p.DefinirQuantidadeDoPoder(Id(config), QuantidadeDisponivel(config));
                });
            }
        }

        private void GarantirEstoqueInicial(PlayerProgress progresso)
        {
            bool alterou = false;
            foreach (PowerUpConfig config in configsAvulsos)
            {
                if (config == null) continue;
                string id = Id(config);
                bool existePersistido = progresso != null && progresso.EstoquePoderes != null &&
                                         progresso.EstoquePoderes.Exists(item => item != null && item.PowerId == id);
                if (!estoque.ContainsKey(id)) estoque[id] = 0;
                if (!existePersistido && estoque[id] == 0 && config.EstoqueInicial > 0)
                {
                    estoque[id] = config.EstoqueInicial;
                    alterou = true;
                }
            }
            if (alterou) Persistir();
            OnEstoqueChanged?.Invoke();
        }

        private static string Id(PowerUpConfig config) =>
            !string.IsNullOrWhiteSpace(config.name) ? config.name : config.NomeExibicao;

        /// <summary>
        /// Tenta usar um poder avulso. targetX/targetY são ignorados quando
        /// config.EfeitoAvulso.RequerAlvo é falso (ex: Embaralhar, +5 Movimentos).
        /// </summary>
        public bool TentarUsar(PowerUpConfig config, int targetX = -1, int targetY = -1)
        {
            config = RecuperarConfigAvulso(config);
            if (config == null) return false;

            int disponivel = QuantidadeDisponivel(config);
            if (disponivel <= 0)
            {
                OnMensagem?.Invoke($"Você não possui {config.NomeExibicao}. Compre um pacote na Loja.");
                return false;
            }

            if (config.EfeitoAvulso.RequerAlvo && (targetX < 0 || targetY < 0))
                return false; // faltou o jogador escolher uma peça (ex: Martelo)

            estoque[Id(config)] = disponivel - 1;
            OnEstoqueChanged?.Invoke();
            Persistir();

            OnMensagem?.Invoke($"{config.NomeExibicao} ativado. Restam {disponivel - 1}.");
            StartCoroutine(UsarRoutine(config, targetX, targetY));
            return true;
        }

        /// <summary>
        /// Recupera assets antigos cujo m_Script perdeu a referência ao tipo.
        /// A correção definitiva é feita pelo montador do Editor; este fallback
        /// impede que uma sessão já montada fique inutilizável enquanto o projeto
        /// ainda está sendo reimportado.
        /// </summary>
        private PowerUpConfig RecuperarConfigAvulso(PowerUpConfig config)
        {
            if (config == null)
            {
                Debug.LogWarning("Poder avulso não configurado na HUD.");
                return null;
            }

            string nome = (config.NomeExibicao ?? string.Empty).ToLowerInvariant();
            bool nomeConhecido = nome.Contains("martelo") || nome.Contains("embaralhar") || nome.Contains("movimento");
            if (config.Tipo != TipoPoder.Avulso)
            {
                if (!nomeConhecido)
                {
                    Debug.LogWarning($"PowerUpConfig '{config.name}' não é um poder avulso.");
                    return null;
                }

                config.Tipo = TipoPoder.Avulso;
            }

            if (config.EfeitoAvulso != null) return config;

            PoderAvulsoSO efeito = null;
            if (nome.Contains("martelo")) efeito = ScriptableObject.CreateInstance<EfeitoMarteloSO>();
            else if (nome.Contains("embaralhar")) efeito = ScriptableObject.CreateInstance<EfeitoEmbaralharSO>();
            else if (nome.Contains("movimento")) efeito = ScriptableObject.CreateInstance<EfeitoMaisMovimentosSO>();

            if (efeito == null)
            {
                Debug.LogWarning($"O poder avulso '{config.name}' não possui efeito reconhecível.");
                return null;
            }

            efeito.DefinirRequerAlvo(efeito is EfeitoMarteloSO);
            config.EfeitoAvulso = efeito;
            return config;
        }

        private IEnumerator UsarRoutine(PowerUpConfig config, int targetX, int targetY)
        {
            var contexto = new PoderAvulsoContexto
            {
                Board = boardManager,
                Detector = matchDetector,
                Physics = boardPhysics,
                Score = scoreManager,
                TargetX = targetX,
                TargetY = targetY
            };

            yield return config.EfeitoAvulso.Usar(contexto);
        }
    }
}
