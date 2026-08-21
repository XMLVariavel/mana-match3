using System;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Tela de Perfil/Progresso: XP, nível, high score e versículos
    /// coletados. Conquistas/achievements não aparecem aqui porque esse
    /// sistema ainda não foi construído em nenhuma fase do projeto — quando
    /// existir, este é o lugar natural de conectá-lo.
    /// </summary>
    public class PerfilController : MonoBehaviour
    {
        [SerializeField] private FirebaseManager firebaseManager;

        public event Action<PlayerProgress> OnPerfilCarregado;

        private void OnEnable()
        {
            if (firebaseManager == null) return;

            firebaseManager.OnProgressoCarregado += HandleProgressoCarregado;
            if (firebaseManager.ProgressoAtual != null) HandleProgressoCarregado(firebaseManager.ProgressoAtual);
        }

        private void OnDisable()
        {
            if (firebaseManager != null) firebaseManager.OnProgressoCarregado -= HandleProgressoCarregado;
        }

        private const string ChaveNomeLocal = "Mana.Perfil.Nome";
        private const string ChaveAvatarLocal = "Mana.Perfil.Avatar";

        private void HandleProgressoCarregado(PlayerProgress progresso)
        {
            AplicarPreferenciasLocais(progresso);
            OnPerfilCarregado?.Invoke(progresso);
        }

        private void AplicarPreferenciasLocais(PlayerProgress progresso)
        {
            if (progresso == null) return;
            string nomeLocal = PlayerPrefs.GetString(ChaveNomeLocal, string.Empty);
            string avatarLocal = PlayerPrefs.GetString(ChaveAvatarLocal, string.Empty);
            bool alterou = false;
            if (!string.IsNullOrWhiteSpace(nomeLocal) && nomeLocal.Length >= 2 && progresso.DisplayName != nomeLocal.Trim())
            {
                progresso.DisplayName = nomeLocal.Trim();
                alterou = true;
            }
            if (AvatarCatalog.Existe(avatarLocal) && progresso.AvatarId != avatarLocal.ToLowerInvariant())
            {
                progresso.AvatarId = avatarLocal.ToLowerInvariant();
                alterou = true;
            }
            if (alterou && firebaseManager != null) firebaseManager.SalvarProgresso(progresso);
        }

        /// <summary>Botão de editar o nome de exibição.</summary>
        public void AtualizarNomeDeExibicao(string novoNome)
        {
            if (string.IsNullOrWhiteSpace(novoNome) || firebaseManager == null) return;

            string nome = novoNome.Trim();
            PlayerPrefs.SetString(ChaveNomeLocal, nome);
            PlayerPrefs.Save();

            if (firebaseManager.ProgressoAtual != null)
            {
                firebaseManager.AtualizarProgresso(p => p.DisplayName = nome);
                OnPerfilCarregado?.Invoke(firebaseManager.ProgressoAtual);
            }
        }

        public void SelecionarAvatar(string avatarId)
        {
            if (!AvatarCatalog.Existe(avatarId) || firebaseManager == null) return;
            string avatar = avatarId.ToLowerInvariant();
            PlayerPrefs.SetString(ChaveAvatarLocal, avatar);
            PlayerPrefs.Save();

            if (firebaseManager.ProgressoAtual != null)
            {
                firebaseManager.AtualizarProgresso(p => p.AvatarId = avatar);
                OnPerfilCarregado?.Invoke(firebaseManager.ProgressoAtual);
            }
        }
    }
}
