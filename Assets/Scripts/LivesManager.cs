using System;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Controla vidas/energia e a regeneração por tempo. Não usa um timer
    /// interno rodando toda hora (custo de bateria em Android) — em vez
    /// disso, recalcula a regeneração pendente sob demanda (lazy), sempre
    /// que algo pergunta o estado atual.
    /// </summary>
    public class LivesManager : MonoBehaviour
    {
        [SerializeField] private int vidasMaximas = 5;
        [SerializeField] private int minutosPorVida = 20;

        public int VidasAtuais { get; private set; }
        private DateTime referenciaRegeneracao; // início da contagem da vida atual em regeneração

        public event Action<int> OnVidasChanged;
        public event Action OnSemVidas;

        /// <summary>
        /// Chamado ao carregar o PlayerProgress do Firestore/fila local —
        /// aplica a regeneração que aconteceu enquanto o jogador estava fora.
        /// </summary>
        public void Inicializar(int vidasSalvas, long referenciaRegeneracaoUnix)
        {
            VidasAtuais = Mathf.Clamp(vidasSalvas, 0, vidasMaximas);
            referenciaRegeneracao = DateTimeOffset.FromUnixTimeSeconds(referenciaRegeneracaoUnix).UtcDateTime;
            AplicarRegeneracaoPendente();
        }

        private void AplicarRegeneracaoPendente()
        {
            if (VidasAtuais >= vidasMaximas) return;

            TimeSpan decorrido = DateTime.UtcNow - referenciaRegeneracao;
            int vidasGanhas = (int)(decorrido.TotalMinutes / minutosPorVida);
            if (vidasGanhas <= 0) return;

            VidasAtuais = Mathf.Min(vidasMaximas, VidasAtuais + vidasGanhas);
            // Reancora no "resto" não aproveitado, para não perder progresso da próxima vida.
            referenciaRegeneracao = referenciaRegeneracao.AddMinutes(vidasGanhas * minutosPorVida);
            OnVidasChanged?.Invoke(VidasAtuais);
        }

        /// <summary>
        /// Quanto tempo falta para a próxima vida (TimeSpan.Zero se já está cheio).
        /// Chame quando for exibir/atualizar a UI — não há timer automático aqui.
        /// </summary>
        public TimeSpan TempoAteProximaVida()
        {
            AplicarRegeneracaoPendente();
            if (VidasAtuais >= vidasMaximas) return TimeSpan.Zero;

            DateTime proxima = referenciaRegeneracao.AddMinutes(minutosPorVida);
            TimeSpan restante = proxima - DateTime.UtcNow;
            return restante > TimeSpan.Zero ? restante : TimeSpan.Zero;
        }

        public bool TentarConsumirVida()
        {
            AplicarRegeneracaoPendente();

            if (VidasAtuais <= 0)
            {
                OnSemVidas?.Invoke();
                return false;
            }

            bool estavaCheio = VidasAtuais == vidasMaximas;
            VidasAtuais--;
            if (estavaCheio) referenciaRegeneracao = DateTime.UtcNow;

            OnVidasChanged?.Invoke(VidasAtuais);
            return true;
        }

        /// <summary>
        /// Chamado após o jogador assistir a um vídeo recompensado (a
        /// integração com o SDK de anúncios fica na Fase D — este método só
        /// aplica o resultado).
        /// </summary>
        public void GanharVidaPorAnuncio()
        {
            if (VidasAtuais >= vidasMaximas) return;
            VidasAtuais++;
            OnVidasChanged?.Invoke(VidasAtuais);
        }

        /// <summary>
        /// Valor a persistir em PlayerProgress.LastLifeTimestampUnix.
        /// </summary>
        public long ReferenciaRegeneracaoUnix() =>
            new DateTimeOffset(DateTime.SpecifyKind(referenciaRegeneracao, DateTimeKind.Utc)).ToUnixTimeSeconds();
    }
}
