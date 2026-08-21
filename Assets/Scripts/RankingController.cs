using System;
using System.Collections.Generic;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Tela de Ranking: busca o top N do leaderboard ao abrir a tela e
    /// repassa pronto pra UI desenhar a lista. Não desenha nada.
    /// </summary>
    public class RankingController : MonoBehaviour
    {
        [SerializeField] private FirebaseManager firebaseManager;
        [SerializeField, Range(10, 100)] private int quantidadeExibida = 50;
        [SerializeField] private string modoFiltro = "geral";
        [SerializeField] private string temporadaId;

        public event Action<List<RankingEntry>> OnRankingCarregado;
        public event Action<string> OnErro;

        private void OnEnable()
        {
            CarregarRanking();
        }

        public void CarregarRanking()
        {
            if (firebaseManager == null)
            {
                OnErro?.Invoke("Placar indisponível nesta sessão.");
                return;
            }

            if (firebaseManager.ModoOffline || !firebaseManager.EstaInicializado)
            {
                OnErro?.Invoke("Você está offline. O placar global volta quando a conta estiver conectada.");
                return;
            }

            string temporada = string.IsNullOrWhiteSpace(temporadaId)
                ? firebaseManager.TemporadaAtual()
                : temporadaId;
            firebaseManager.BuscarRankingGlobal(quantidadeExibida, modoFiltro, temporada,
                resultado => OnRankingCarregado?.Invoke(resultado));
        }

        public void DefinirModo(string novoModo)
        {
            modoFiltro = string.IsNullOrWhiteSpace(novoModo) ? "geral" : novoModo;
            CarregarRanking();
        }

        public void DefinirModoGeral() => DefinirModo("geral");
        public void DefinirModoInfinito() => DefinirModo(GameMode.EstudoInfinito.ToString());
        public void DefinirModoDiario() => DefinirModo(GameMode.DesafioDiario.ToString());
        public void DefinirModoTempo() => DefinirModo(GameMode.ContraRelogio.ToString());
        public void DefinirModoGuardiao() => DefinirModo(GameMode.GuardiaoDaPalavra.ToString());
    }
}
