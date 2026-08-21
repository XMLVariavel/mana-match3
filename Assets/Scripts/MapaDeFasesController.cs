using System.Collections.Generic;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Tela Home: mostra a trilha de fases da Campanha (travada/liberada,
    /// estrelas obtidas), permite entrar numa fase ou no Estudo Infinito, e
    /// navega para Loja/Perfil/Ranking/Configurações. Não desenha nada —
    /// cada botão de fase é instanciado a partir de um prefab fornecido.
    /// </summary>
    public class MapaDeFasesController : MonoBehaviour
    {
        [Header("Dados")]
        [Tooltip("Todas as fases da Campanha, na ordem em que aparecem na trilha.")]
        [SerializeField] private List<LevelData> fasesDaCampanha;

        [Header("Managers")]
        [SerializeField] private FirebaseManager firebaseManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private LivesManager livesManager;
        [SerializeField] private ScreenNavigator navigator;
        [SerializeField] private GameHUDController hudController;

        [Header("UI")]
        [SerializeField] private Transform containerDaTrilha;
        [SerializeField] private BotaoFasePrefab botaoFasePrefab;

        [Header("Nomes das telas")]
        [SerializeField] private string telaInicio = "Inicio";
        [SerializeField] private string telaMapa = "MapaDeFases";
        [SerializeField] private string telaDesafios = "Desafios";
        [SerializeField] private string telaJogo = "TelaJogo";
        [SerializeField] private string telaLoja = "Loja";
        [SerializeField] private string telaPerfil = "Perfil";
        [SerializeField] private string telaRanking = "Ranking";
        [SerializeField] private string telaConfiguracoes = "Configuracoes";

        private readonly List<BotaoFasePrefab> botoesInstanciados = new List<BotaoFasePrefab>();
        private LevelData faseAtualCampanha;

        public bool PossuiProximaFase
        {
            get
            {
                if (faseAtualCampanha == null || fasesDaCampanha == null) return false;
                int indice = fasesDaCampanha.IndexOf(faseAtualCampanha);
                return indice >= 0 && indice + 1 < fasesDaCampanha.Count && fasesDaCampanha[indice + 1] != null;
            }
        }

        private void OnEnable()
        {
            if (firebaseManager != null) firebaseManager.OnProgressoCarregado += HandleProgressoCarregado;
            MontarTrilha(firebaseManager != null ? firebaseManager.ProgressoAtual : null);
        }

        private void OnDisable()
        {
            if (firebaseManager != null) firebaseManager.OnProgressoCarregado -= HandleProgressoCarregado;
        }

        private void HandleProgressoCarregado(PlayerProgress progresso)
        {
            MontarTrilha(progresso);
        }

        private void MontarTrilha(PlayerProgress progresso)
        {
            LimparTrilha();
            if (fasesDaCampanha == null || botaoFasePrefab == null || containerDaTrilha == null) return;

            for (int i = 0; i < fasesDaCampanha.Count; i++)
            {
                LevelData fase = fasesDaCampanha[i];
                if (fase == null) continue;

                bool primeiraFase = i == 0;
                bool anteriorConcluida = !primeiraFase && progresso != null &&
                                          progresso.EstrelasDaFase(fasesDaCampanha[i - 1].Numero) > 0;
                bool liberada = primeiraFase || anteriorConcluida;
                int estrelas = progresso != null ? progresso.EstrelasDaFase(fase.Numero) : 0;

                BotaoFasePrefab botao = Instantiate(botaoFasePrefab, containerDaTrilha);
                botao.Configurar(fase, liberada, estrelas, () => EntrarNaFase(fase));
                botoesInstanciados.Add(botao);
            }
        }

        private void LimparTrilha()
        {
            foreach (BotaoFasePrefab botao in botoesInstanciados)
                if (botao != null) Destroy(botao.gameObject);
            botoesInstanciados.Clear();
        }

        private void EntrarNaFase(LevelData fase)
        {
            if (livesManager != null && !livesManager.TentarConsumirVida())
                return; // sem vidas — a tela decide se mostra oferta de vídeo/compra

            // Guardamos a fase para que o botão "Próximo nível" possa avançar
            // diretamente após uma vitória, sem obrigar o jogador a voltar ao mapa.
            faseAtualCampanha = fase;
            hudController?.DefinirFaseAtual(fase);

            gameManager.IniciarCampanha(fase);
            navigator?.Mostrar(telaJogo);
        }

        public void EntrarNaPrimeiraFase()
        {
            if (fasesDaCampanha == null || fasesDaCampanha.Count == 0 || fasesDaCampanha[0] == null) return;
            EntrarNaFase(fasesDaCampanha[0]);
        }

        public void EntrarNaProximaFase()
        {
            if (!PossuiProximaFase)
            {
                navigator?.Mostrar(telaMapa);
                return;
            }

            int indice = fasesDaCampanha.IndexOf(faseAtualCampanha);
            EntrarNaFase(fasesDaCampanha[indice + 1]);
        }

        /// <summary>Botão "Estudo Infinito" da Home.</summary>
        public void EntrarNoEstudoInfinito()
        {
            hudController?.DefinirFaseAtual(null); // Estudo Infinito não tem fase a registrar
            gameManager.IniciarEstudoInfinito();
            navigator?.Mostrar(telaJogo);
        }

        public void EntrarNoDesafioDiario()
        {
            hudController?.DefinirFaseAtual(null);
            gameManager.IniciarDesafioDiario();
            navigator?.Mostrar(telaJogo);
        }

        public void EntrarNoContraRelogio()
        {
            hudController?.DefinirFaseAtual(null);
            gameManager.IniciarContraRelogio();
            navigator?.Mostrar(telaJogo);
        }

        public void EntrarNoGuardiaoDaPalavra()
        {
            hudController?.DefinirFaseAtual(null);
            gameManager.IniciarGuardiaoDaPalavra();
            navigator?.Mostrar(telaJogo);
        }

        public void AbrirInicio() => navigator?.Mostrar(telaInicio);
        public void AbrirJornada() => navigator?.Mostrar(telaMapa);
        public void AbrirDesafios() => navigator?.Mostrar(telaDesafios);
        public void AbrirLoja() => navigator?.Mostrar(telaLoja);
        public void AbrirPerfil() => navigator?.Mostrar(telaPerfil);
        public void AbrirRanking() => navigator?.Mostrar(telaRanking);
        public void AbrirConfiguracoes() => navigator?.Mostrar(telaConfiguracoes);
    }

    /// <summary>
    /// Representa um botão de fase individual na trilha. Implementação real
    /// (texto, ícone de cadeado, estrelas) fica no prefab da UI — aqui só a
    /// interface mínima que o MapaDeFasesController precisa para configurá-lo.
    /// </summary>
    public abstract class BotaoFasePrefab : MonoBehaviour
    {
        public abstract void Configurar(LevelData fase, bool liberada, int estrelas, System.Action aoClicar);
    }
}
