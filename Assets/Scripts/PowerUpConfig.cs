using UnityEngine;

namespace BibleMatch3
{
    public enum TipoPoder
    {
        EspecialDeTabuleiro, // Espada, Tocha, Arca, Estrela Guia — evoluem por nível
        Avulso               // Martelo, Embaralhar, +5 Movimentos — comprados e usados manualmente
    }

    /// <summary>
    /// Molde de dados de um poder do jogo. Não contém lógica de efeito —
    /// apenas referencia o Strategy correspondente (EfeitoEspecialSO ou
    /// PoderAvulsoSO), que é quem sabe executar o efeito de fato.
    /// </summary>
    [CreateAssetMenu(fileName = "NovoPoder", menuName = "BibleMatch3/Poder")]
    public class PowerUpConfig : ScriptableObject
    {
        [Header("Identidade")]
        public string NomeExibicao;
        public Sprite Icone;
        [TextArea] public string Descricao;
        public TipoPoder Tipo;

        [Header("Especial de Tabuleiro (evolução por nível)")]
        [Tooltip("A qual SpecialType este config corresponde — usado pelo MatchDetector para localizar o nível atual.")]
        public SpecialType TipoEspecialAssociado;
        public EfeitoEspecialSO EfeitoDeTabuleiro;
        [Min(1)] public int NivelAtual = 1;
        [Min(1)] public int NivelMaximo = 3;
        [Tooltip("Custo em moedas para evoluir do nível N para N+1 (índice 0 = custo do nível 1 -> 2).")]
        public int[] CustoEvolucaoPorNivel;

        [Header("Poder Avulso (compra e uso manual)")]
        public PoderAvulsoSO EfeitoAvulso;
        [Tooltip("Preço em moedas para comprar um pacote deste poder.")]
        public int CustoMoedas;
        [Min(1), Tooltip("Quantidade recebida ao comprar um pacote.")]
        public int QuantidadePorCompra = 3;
        [Min(0), Tooltip("Quantidade entregue uma única vez ao iniciar uma conta nova.")]
        public int EstoqueInicial = 3;

        public bool PodeEvoluir => Tipo == TipoPoder.EspecialDeTabuleiro && NivelAtual < NivelMaximo;

        public int CustoEvolucaoProximoNivel =>
            (CustoEvolucaoPorNivel != null && NivelAtual - 1 < CustoEvolucaoPorNivel.Length)
                ? CustoEvolucaoPorNivel[NivelAtual - 1]
                : -1;

        /// <summary>
        /// Evolui o poder para o próximo nível. A dedução de moedas é
        /// responsabilidade de quem chama (ex: tela de Loja), este método só
        /// garante que o nível nunca ultrapasse o máximo configurado.
        /// </summary>
        public void Evoluir()
        {
            if (PodeEvoluir) NivelAtual++;
        }
    }
}
