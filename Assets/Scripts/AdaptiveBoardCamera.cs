using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Mantém o tabuleiro legível em diferentes proporções de tela.
    /// Em retrato, calcula o tamanho pela largura; em paisagem, evita que a
    /// grade fique minúscula e usa uma altura mínima confortável.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class AdaptiveBoardCamera : MonoBehaviour
    {
        [SerializeField] private float larguraDoTabuleiroComMoldura = 8.8f;
        [SerializeField] private float margemHorizontal = 0.35f;
        [SerializeField] private float orthoMinimoEmPaisagem = 4.8f;

        private Camera cameraLocal;
        private int larguraAnterior;
        private int alturaAnterior;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstalarNaCameraPrincipal()
        {
            Camera principal = Camera.main;
            if (principal != null && principal.GetComponent<AdaptiveBoardCamera>() == null)
                principal.gameObject.AddComponent<AdaptiveBoardCamera>();
        }

        private void Awake()
        {
            cameraLocal = GetComponent<Camera>();
            AtualizarEnquadramento(true);
        }

        private void LateUpdate()
        {
            AtualizarEnquadramento(false);
        }

        private void AtualizarEnquadramento(bool forcado)
        {
            if (cameraLocal == null || !cameraLocal.orthographic) return;
            if (!forcado && larguraAnterior == Screen.width && alturaAnterior == Screen.height) return;

            larguraAnterior = Screen.width;
            alturaAnterior = Screen.height;

            float aspecto = Mathf.Max(0.1f, (float)Screen.width / Mathf.Max(1, Screen.height));
            float metadeComMargem = larguraDoTabuleiroComMoldura * 0.5f + margemHorizontal;
            float porLargura = metadeComMargem / aspecto;
            cameraLocal.orthographicSize = Mathf.Max(orthoMinimoEmPaisagem, porLargura);
        }
    }
}
