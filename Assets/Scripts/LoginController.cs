using System;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Tela de Login: mostra se a conta está anônima ou vinculada ao Google
    /// e dispara o vínculo. Não implementa o fluxo de Google Sign-In em si —
    /// recebe o idToken já pronto de um plugin externo (ver FirebaseManager).
    /// </summary>
    public class LoginController : MonoBehaviour
    {
        [SerializeField] private FirebaseManager firebaseManager;

        public bool ContaVinculada => firebaseManager != null && firebaseManager.ContaVinculada;

        public event Action<bool> OnVinculoConcluido; // true = sucesso
        public event Action<string> OnErro;

        /// <summary>
        /// Chame depois de obter o idToken por um plugin de Google Sign-In
        /// (ex: Google Play Games, Google Sign-In para Unity) — fora do
        /// escopo deste script.
        /// </summary>
        public void VincularComGoogle(string idToken)
        {
            if (firebaseManager == null)
            {
                OnErro?.Invoke("Serviço de conta indisponível no momento.");
                return;
            }

            if (string.IsNullOrEmpty(idToken))
            {
                OnErro?.Invoke("Não foi possível obter sua conta Google.");
                return;
            }

            firebaseManager.VincularContaGoogle(idToken, sucesso =>
            {
                OnVinculoConcluido?.Invoke(sucesso);
                if (!sucesso) OnErro?.Invoke("Não foi possível vincular sua conta Google.");
            });
        }
    }
}
