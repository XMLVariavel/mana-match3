using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Contexto passado a um poder avulso no momento do uso.
    /// </summary>
    public struct PoderAvulsoContexto
    {
        public BoardManager Board;
        public MatchDetector Detector;
        public BoardPhysics Physics;
        public ScoreAndObjectiveManager Score;
        public int TargetX; // -1 quando o poder não precisa de alvo
        public int TargetY;
    }

    /// <summary>
    /// Base ScriptableObject para poderes de uso avulso (comprados com moeda,
    /// usados antes ou durante a partida). Mesmo padrão Strategy dos efeitos
    /// de tabuleiro, mas sem noção de "nível" — só de custo em moedas.
    /// </summary>
    public abstract class PoderAvulsoSO : ScriptableObject
    {
        [Header("Uso")]
        [SerializeField] private bool requerAlvo;
        public bool RequerAlvo => requerAlvo;

        /// <summary>
        /// Configuração estrutural usada pelo montador de conteúdo. Mantém a
        /// regra de alvo no próprio efeito e evita depender de edição manual do
        /// YAML do ScriptableObject.
        /// </summary>
        public void DefinirRequerAlvo(bool valor) => requerAlvo = valor;

        public abstract IEnumerator Usar(PoderAvulsoContexto contexto);
    }

    /// <summary>
    /// Martelo: remove uma única peça escolhida pelo jogador, sem consumir movimento.
    /// </summary>
    [CreateAssetMenu(fileName = "EfeitoMartelo", menuName = "BibleMatch3/Efeitos/Martelo")]
    public class EfeitoMarteloSO : PoderAvulsoSO
    {
        public override IEnumerator Usar(PoderAvulsoContexto c)
        {
            Tile alvo = c.Board.Grid[c.TargetX, c.TargetY];
            if (alvo == null) yield break;

            var result = new MatchResult();
            result.TilesToDestroy.Add(alvo);

            // Reaproveita o mesmo pipeline de destruição + gravidade + cascata do BoardPhysics.
            yield return c.Physics.ResolveBoard(c.Detector, c.Score, result);
        }
    }

    /// <summary>
    /// Embaralhar Tabuleiro: redistribui os tipos das peças existentes,
    /// garantindo que o resultado não comece já com um match pronto.
    /// </summary>
    [CreateAssetMenu(fileName = "EfeitoEmbaralhar", menuName = "BibleMatch3/Efeitos/Embaralhar Tabuleiro")]
    public class EfeitoEmbaralharSO : PoderAvulsoSO
    {
        public override IEnumerator Usar(PoderAvulsoContexto c)
        {
            c.Board.EmbaralharTabuleiro();
            yield break;
        }
    }

    /// <summary>
    /// +5 Movimentos: não mexe no tabuleiro, só estende os movimentos da fase.
    /// </summary>
    [CreateAssetMenu(fileName = "EfeitoMaisMovimentos", menuName = "BibleMatch3/Efeitos/Mais Movimentos")]
    public class EfeitoMaisMovimentosSO : PoderAvulsoSO
    {
        [SerializeField] private int quantidade = 5;

        public override IEnumerator Usar(PoderAvulsoContexto c)
        {
            c.Score.AddMoves(quantidade);
            yield break;
        }
    }
}
