using System;
using UnityEngine;

#if MANA_GOOGLE_SIGNIN
using Google;
using Firebase.Extensions;
#endif

namespace BibleMatch3
{
    /// <summary>
    /// Obtém o idToken do Google e entrega ao <see cref="LoginController"/>,
    /// que por sua vez chama <see cref="FirebaseManager.VincularContaGoogle"/>.
    ///
    /// IMPORTANTE — compilação condicional:
    /// o corpo real depende do plugin googlesignin-unity, que não faz parte do
    /// projeto por padrão. Todo o código que toca no SDK está sob o define
    /// <c>MANA_GOOGLE_SIGNIN</c>. Sem esse símbolo o projeto compila normalmente
    /// e o botão de login apenas informa que o recurso não está disponível —
    /// isso evita que a ausência de um plugin externo quebre a build inteira.
    ///
    /// Para ativar:
    /// 1. Importe o plugin googlesignin-unity (ver Docs/Integracoes_SDK.md).
    /// 2. Project Settings > Player > Scripting Define Symbols (Android):
    ///    adicione MANA_GOOGLE_SIGNIN.
    /// 3. Preencha o Web Client ID abaixo com o do seu projeto no Google Cloud.
    /// </summary>
    public class GoogleSignInService : MonoBehaviour
    {
        [SerializeField] private LoginController loginController;

        [Tooltip("Web Client ID do OAuth (tipo 'Web application') do seu projeto no Google Cloud Console. " +
                 "NÃO é o Android Client ID.")]
        [SerializeField] private string webClientId = "";

        /// <summary>Erros já em linguagem de jogador, prontos para a tela.</summary>
        public event Action<string> OnErro;

        /// <summary>Disponível somente quando o plugin foi importado e o define ligado.</summary>
        public bool Disponivel
        {
#if MANA_GOOGLE_SIGNIN
            get { return !string.IsNullOrEmpty(webClientId); }
#else
            get { return false; }
#endif
        }

        public void EntrarComGoogle()
        {
#if MANA_GOOGLE_SIGNIN
            if (string.IsNullOrEmpty(webClientId))
            {
                OnErro?.Invoke("Login com Google não configurado (Web Client ID vazio).");
                return;
            }

            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                WebClientId = webClientId,
                RequestIdToken = true, // é o idToken que o Firebase precisa
                RequestEmail = true,
                UseGameSignIn = false
            };

            GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(tarefa =>
            {
                if (tarefa.IsCanceled)
                {
                    OnErro?.Invoke("Login cancelado.");
                    return;
                }

                if (tarefa.IsFaulted)
                {
                    Debug.LogWarning($"[GoogleSignIn] Falha: {tarefa.Exception}");
                    OnErro?.Invoke("Não foi possível entrar com o Google. Tente novamente.");
                    return;
                }

                string idToken = tarefa.Result != null ? tarefa.Result.IdToken : null;
                if (string.IsNullOrEmpty(idToken))
                {
                    OnErro?.Invoke("O Google não devolveu um token válido.");
                    return;
                }

                if (loginController == null)
                {
                    Debug.LogError("[GoogleSignIn] LoginController não atribuído.");
                    OnErro?.Invoke("Erro interno ao vincular a conta.");
                    return;
                }

                loginController.VincularComGoogle(idToken);
            });
#else
            Debug.LogWarning(
                "[GoogleSignIn] Plugin ausente. Importe googlesignin-unity e adicione o define " +
                "MANA_GOOGLE_SIGNIN em Player Settings para habilitar o login com Google.");
            OnErro?.Invoke("Login com Google indisponível nesta build.");
#endif
        }

        /// <summary>
        /// Encerra a sessão local do Google. Não desvincula a conta do Firebase —
        /// serve para permitir escolher outra conta no próximo login.
        /// </summary>
        public void Sair()
        {
#if MANA_GOOGLE_SIGNIN
            GoogleSignIn.DefaultInstance.SignOut();
#endif
        }
    }
}
