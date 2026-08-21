using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace BibleMatch3
{
    /// <summary>
    /// Reproduz a abertura cinematográfica em uma RawImage. A textura é criada
    /// em runtime para não deixar RenderTextures temporárias no projeto.
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    public class SplashVideoPlayer : MonoBehaviour
    {
        [SerializeField] private VideoClip clip;
        [SerializeField] private RawImage alvo;
        [SerializeField] private Color corDoVideo = Color.white;

        private RenderTexture textura;
        private VideoPlayer player;

        private void Awake()
        {
            player = GetComponent<VideoPlayer>();
            player.playOnAwake = false;
            player.isLooping = false;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.audioOutputMode = VideoAudioOutputMode.None;
            player.skipOnDrop = true;

            if (clip == null || alvo == null) return;

            textura = new RenderTexture(720, 1280, 0, RenderTextureFormat.ARGB32)
            {
                name = "Maná_Intro_RenderTexture",
                filterMode = FilterMode.Bilinear
            };
            textura.Create();
            player.clip = clip;
            player.targetTexture = textura;
            alvo.texture = textura;
            alvo.color = corDoVideo;
            player.Prepare();
        }

        private void OnEnable()
        {
            if (player == null) player = GetComponent<VideoPlayer>();
            player.loopPointReached += HandleFim;
        }

        private void Start()
        {
            if (player != null && clip != null) player.Play();
        }

        private void OnDisable()
        {
            if (player != null) player.loopPointReached -= HandleFim;
        }

        private void OnDestroy()
        {
            if (textura == null) return;
            if (textura.IsCreated()) textura.Release();
            Destroy(textura);
        }

        private void HandleFim(VideoPlayer source)
        {
            source.Stop();
        }
    }
}
