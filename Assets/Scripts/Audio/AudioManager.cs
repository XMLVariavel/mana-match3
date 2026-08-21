using System;
using System.Collections.Generic;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Efeitos sonoros catalogados do jogo. O enum existe para que quem
    /// dispara o som (BoardPhysics, telas) não precise conhecer AudioClips —
    /// só o "que aconteceu". O mapeamento clipe↔evento fica no Inspector.
    /// </summary>
    public enum EfeitoSonoro
    {
        Match,
        ComboEspecial,
        EspecialCriado,
        TrocaInvalida,
        BotaoUI,
        Vitoria,
        Derrota
    }

    /// <summary>
    /// Música de fundo e efeitos sonoros com volume independente, mudo
    /// independente, e persistência local via PlayerPrefs (de propósito não
    /// vai para o Firestore: é preferência de aparelho, não de conta —
    /// o mesmo jogador pode querer som no tablet e mudo no celular).
    ///
    /// Não conhece UI: a tela de Configurações fala com ele através do
    /// ConfiguracoesController, e quem toca efeito fala pelo enum acima.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private const string ChaveVolumeMusica = "BibleMatch3_VolumeMusica";
        private const string ChaveVolumeEfeitos = "BibleMatch3_VolumeEfeitos";
        private const string ChaveMusicaMuda = "BibleMatch3_MusicaMuda";
        private const string ChaveEfeitosMudos = "BibleMatch3_EfeitosMudos";

        public static AudioManager Instance { get; private set; }

        [Serializable]
        public class ClipeDeEfeito
        {
            public EfeitoSonoro Efeito;
            public AudioClip Clipe;
            [Range(0f, 1f)] public float VolumeRelativo = 1f;
        }

        [Header("Fontes de áudio")]
        [SerializeField] private AudioSource fonteMusica;
        [SerializeField] private AudioSource fonteEfeitos;

        [Header("Biblioteca")]
        [SerializeField] private AudioClip musicaPadrao;
        [SerializeField] private List<ClipeDeEfeito> efeitos = new List<ClipeDeEfeito>();

        [Header("Anti-spam de efeitos")]
        [Tooltip("Intervalo mínimo entre dois efeitos iguais. Evita 'metralhadora' de sons durante uma cascata longa.")]
        [SerializeField] private float intervaloMinimoPorEfeito = 0.05f;

        [Header("Padrões (usados na primeira execução, antes de existir PlayerPrefs)")]
        [Range(0f, 1f)] [SerializeField] private float volumeMusicaPadrao = 0.6f;
        [Range(0f, 1f)] [SerializeField] private float volumeEfeitosPadrao = 0.8f;

        private readonly Dictionary<EfeitoSonoro, ClipeDeEfeito> mapaDeEfeitos = new Dictionary<EfeitoSonoro, ClipeDeEfeito>();
        private readonly Dictionary<EfeitoSonoro, float> ultimoToquePorEfeito = new Dictionary<EfeitoSonoro, float>();

        public float VolumeMusica { get; private set; }
        public float VolumeEfeitos { get; private set; }
        public bool MusicaMuda { get; private set; }
        public bool EfeitosMudos { get; private set; }

        public event Action OnConfiguracaoAlterada;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            GarantirFontes();
            IndexarEfeitos();
            CarregarPreferencias();
        }

        private void Start()
        {
            if (musicaPadrao != null) TocarMusica(musicaPadrao);
        }

        private void GarantirFontes()
        {
            if (fonteMusica == null)
            {
                fonteMusica = gameObject.AddComponent<AudioSource>();
                fonteMusica.playOnAwake = false;
                fonteMusica.loop = true;
            }

            if (fonteEfeitos == null)
            {
                fonteEfeitos = gameObject.AddComponent<AudioSource>();
                fonteEfeitos.playOnAwake = false;
                fonteEfeitos.loop = false;
            }
        }

        private void IndexarEfeitos()
        {
            mapaDeEfeitos.Clear();
            foreach (ClipeDeEfeito entrada in efeitos)
                if (entrada != null) mapaDeEfeitos[entrada.Efeito] = entrada;
        }

        // ---------------------------------------------------------------
        // Preferências
        // ---------------------------------------------------------------

        private void CarregarPreferencias()
        {
            VolumeMusica = PlayerPrefs.GetFloat(ChaveVolumeMusica, volumeMusicaPadrao);
            VolumeEfeitos = PlayerPrefs.GetFloat(ChaveVolumeEfeitos, volumeEfeitosPadrao);
            MusicaMuda = PlayerPrefs.GetInt(ChaveMusicaMuda, 0) == 1;
            EfeitosMudos = PlayerPrefs.GetInt(ChaveEfeitosMudos, 0) == 1;

            AplicarVolumes();
        }

        private void AplicarVolumes()
        {
            if (fonteMusica != null) fonteMusica.volume = MusicaMuda ? 0f : VolumeMusica;
            if (fonteEfeitos != null) fonteEfeitos.volume = EfeitosMudos ? 0f : VolumeEfeitos;
            OnConfiguracaoAlterada?.Invoke();
        }

        public void DefinirVolumeMusica(float volume)
        {
            VolumeMusica = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(ChaveVolumeMusica, VolumeMusica);
            PlayerPrefs.Save();
            AplicarVolumes();
        }

        public void DefinirVolumeEfeitos(float volume)
        {
            VolumeEfeitos = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(ChaveVolumeEfeitos, VolumeEfeitos);
            PlayerPrefs.Save();
            AplicarVolumes();
        }

        public void DefinirMusicaMuda(bool muda)
        {
            MusicaMuda = muda;
            PlayerPrefs.SetInt(ChaveMusicaMuda, muda ? 1 : 0);
            PlayerPrefs.Save();
            AplicarVolumes();
        }

        public void DefinirEfeitosMudos(bool mudos)
        {
            EfeitosMudos = mudos;
            PlayerPrefs.SetInt(ChaveEfeitosMudos, mudos ? 1 : 0);
            PlayerPrefs.Save();
            AplicarVolumes();
        }

        // ---------------------------------------------------------------
        // Reprodução
        // ---------------------------------------------------------------

        public void TocarMusica(AudioClip clipe)
        {
            if (fonteMusica == null || clipe == null) return;
            if (fonteMusica.clip == clipe && fonteMusica.isPlaying) return;

            fonteMusica.clip = clipe;
            fonteMusica.loop = true;
            fonteMusica.Play();
        }

        public void PararMusica()
        {
            if (fonteMusica != null) fonteMusica.Stop();
        }

        /// <summary>
        /// Toca um efeito catalogado. Silencioso (sem warning) quando o clipe
        /// ainda não foi atribuído no Inspector — durante o desenvolvimento é
        /// normal ter o evento disparando antes do áudio final existir.
        /// </summary>
        public void TocarEfeito(EfeitoSonoro efeito)
        {
            if (EfeitosMudos || fonteEfeitos == null) return;
            if (!mapaDeEfeitos.TryGetValue(efeito, out ClipeDeEfeito entrada) || entrada.Clipe == null) return;

            if (ultimoToquePorEfeito.TryGetValue(efeito, out float ultimo) &&
                Time.unscaledTime - ultimo < intervaloMinimoPorEfeito)
                return;

            ultimoToquePorEfeito[efeito] = Time.unscaledTime;
            fonteEfeitos.PlayOneShot(entrada.Clipe, VolumeEfeitos * entrada.VolumeRelativo);
        }
    }
}
