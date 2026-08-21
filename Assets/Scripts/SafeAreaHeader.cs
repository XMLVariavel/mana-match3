using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Mantém um cabeçalho ancorado abaixo da área de recorte do dispositivo.
    /// O fundo continua em tela cheia; apenas o conteúdo superior respeita o notch.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHeader : MonoBehaviour
    {
        [Tooltip("Altura fixa da faixa. Deixe 0 para preservar a altura de projeto do RectTransform.")]
        [SerializeField] private float alturaReferencia;
        [SerializeField] private float margemExtra = 8f;

        private RectTransform rt;
        private RectTransform canvasRt;
        private float alturaDeProjeto;
        private Rect ultimaArea;
        private Vector2 ultimaTela;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            canvasRt = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();

            // A altura precisa vir do próprio retângulo: a barra da tela de jogo
            // tem 480 e a dos menus 140. Um valor fixo aqui achataria o cabeçalho
            // do jogo e jogaria cards e objetivos para fora do lugar.
            alturaDeProjeto = rt != null ? rt.offsetMax.y - rt.offsetMin.y : 0f;
        }

        private void LateUpdate()
        {
            if (rt == null || canvasRt == null) return;
            Rect safe = Screen.safeArea;
            if (safe == ultimaArea && ultimaTela == new Vector2(Screen.width, Screen.height)) return;

            ultimaArea = safe;
            ultimaTela = new Vector2(Screen.width, Screen.height);
            float escalaY = canvasRt.rect.height / Mathf.Max(1f, Screen.height);
            float topo = (Screen.height - safe.yMax) * escalaY + margemExtra;
            float altura = alturaReferencia > 0f ? alturaReferencia : alturaDeProjeto;
            if (altura <= 0f) return;

            rt.offsetMax = new Vector2(rt.offsetMax.x, -topo);
            rt.offsetMin = new Vector2(rt.offsetMin.x, -topo - altura);
        }
    }
}
