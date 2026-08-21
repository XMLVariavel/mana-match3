using UnityEngine;
using UnityEngine.UI;

namespace BibleMatch3
{
    /// <summary>
    /// Estrela desenhada diretamente na malha da UI. Evita depender de glyphs
    /// que podem não existir no LiberationSans SDF usado pelo TextMeshPro.
    /// </summary>
    [AddComponentMenu("Maná/UI/Star Graphic")]
    public sealed class StarGraphic : Graphic
    {
        [SerializeField, Range(5, 8)] private int pontas = 5;
        [SerializeField, Range(0.25f, 0.48f)] private float raioInterno = 0.24f;
        [SerializeField, Range(0.35f, 0.5f)] private float raioExterno = 0.47f;
        [SerializeField] private float rotacaoGraus = 90f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (!IsActive()) return;

            Rect rect = rectTransform.rect;
            float raio = Mathf.Min(rect.width, rect.height) * 0.5f;
            if (raio <= 0.01f) return;

            Vector2 centro = rect.center;
            int verticesDaEstrela = pontas * 2;
            UIVertex vertice = UIVertex.simpleVert;
            vertice.color = color;
            vertice.position = centro;
            vh.AddVert(vertice);

            for (int i = 0; i <= verticesDaEstrela; i++)
            {
                float angulo = (rotacaoGraus - i * (180f / pontas)) * Mathf.Deg2Rad;
                float escala = (i & 1) == 0 ? raioExterno : raioInterno;
                Vector2 posicao = centro + new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * (raio * escala / 0.5f);
                vertice.position = posicao;
                vh.AddVert(vertice);
            }

            for (int i = 0; i < verticesDaEstrela; i++)
                vh.AddTriangle(0, i + 1, i + 2);
        }
    }
}
