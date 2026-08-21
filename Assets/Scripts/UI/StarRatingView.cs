using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Controla uma fileira de estrelas desenhadas por <see cref="StarGraphic"/>.
    /// A cor e a geometria ficam no componente gráfico; aqui só se altera a
    /// quantidade de estrelas preenchidas.
    /// </summary>
    public sealed class StarRatingView : MonoBehaviour
    {
        [SerializeField] private Color corAtiva = new Color(0.949f, 0.714f, 0.255f, 1f);
        [SerializeField] private Color corInativa = new Color(0.20f, 0.25f, 0.29f, 0.90f);

        [SerializeField] private StarGraphic[] estrelas = new StarGraphic[0];
        [SerializeField, Range(0, 8)] private int avaliacao;

        private void Awake()
        {
            if (estrelas == null || estrelas.Length == 0)
                estrelas = GetComponentsInChildren<StarGraphic>(true);
            AplicarCores();
        }

        public void Configurar(StarGraphic[] graficos, int valorInicial = 0)
        {
            estrelas = graficos ?? new StarGraphic[0];
            avaliacao = Mathf.Max(0, valorInicial);
            AplicarCores();
        }

        public void Definir(int valor)
        {
            // Guarda a avaliação mesmo se o painel ainda estiver inativo; o
            // Awake encontrará os filhos quando o painel for ativado.
            avaliacao = Mathf.Max(0, valor);
            AplicarCores();
        }

        private void AplicarCores()
        {
            int valorAplicado = Mathf.Clamp(avaliacao, 0, estrelas.Length);
            for (int i = 0; i < estrelas.Length; i++)
            {
                if (estrelas[i] == null) continue;
                estrelas[i].color = i < valorAplicado ? corAtiva : corInativa;
                estrelas[i].SetVerticesDirty();
            }
        }
    }
}
