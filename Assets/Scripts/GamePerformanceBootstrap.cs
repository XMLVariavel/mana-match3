using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Ajustes seguros e reversíveis para o perfil mobile do jogo.
    /// A qualidade artística continua vindo dos sprites; este bootstrap apenas
    /// evita variação excessiva de frame e garante queries de toque nos triggers.
    /// </summary>
    public static class GamePerformanceBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AplicarPerfilMobile()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Physics2D.queriesHitTriggers = true;
        }
    }
}
