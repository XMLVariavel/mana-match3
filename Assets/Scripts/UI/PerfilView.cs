using TMPro;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Tela de Perfil: identidade do jogador, progresso e versículos coletados.
    /// A troca de nome é confirmada por botão (e não a cada tecla) porque
    /// <see cref="PerfilController.AtualizarNomeDeExibicao"/> escreve no
    /// Firestore — um write por caractere seria desperdício de cota.
    /// </summary>
    public class PerfilView : MonoBehaviour
    {
        [Header("Origem")]
        [SerializeField] private PerfilController controller;

        [Header("Campos")]
        [SerializeField] private TMP_InputField campoNome;
        [SerializeField] private TextMeshProUGUI textoNivel;
        [SerializeField] private TextMeshProUGUI textoXp;
        [SerializeField] private TextMeshProUGUI textoHighScore;
        [SerializeField] private TextMeshProUGUI textoVersiculos;
        [SerializeField] private TextMeshProUGUI textoStatusConta;
        [SerializeField] private TextMeshProUGUI textoMensagem;

        [SerializeField] private string mensagemNomeSalvo = "Nome atualizado.";
        [SerializeField] private string mensagemNomeInvalido = "Escolha um nome com pelo menos 2 caracteres.";

        private void OnEnable()
        {
            if (controller == null) return;

            controller.OnPerfilCarregado += HandlePerfil;
            LimparMensagem();
        }

        private void OnDisable()
        {
            if (controller == null) return;
            controller.OnPerfilCarregado -= HandlePerfil;
        }

        /// <summary>Ligado ao botão "Salvar nome" pelo montador do Editor.</summary>
        public void SalvarNome()
        {
            if (controller == null || campoNome == null) return;

            string novo = campoNome.text != null ? campoNome.text.Trim() : string.Empty;
            if (novo.Length < 2)
            {
                if (textoMensagem != null) textoMensagem.text = mensagemNomeInvalido;
                return;
            }

            controller.AtualizarNomeDeExibicao(novo);
            if (textoMensagem != null) textoMensagem.text = mensagemNomeSalvo;
        }

        private void HandlePerfil(PlayerProgress progresso)
        {
            if (progresso == null) return;

            if (campoNome != null) campoNome.SetTextWithoutNotify(progresso.DisplayName);
            if (textoNivel != null) textoNivel.text = $"Nível {progresso.Level}";
            if (textoXp != null) textoXp.text = $"{progresso.Xp} XP";
            if (textoHighScore != null) textoHighScore.text = progresso.HighScore.ToString();

            if (textoVersiculos != null)
            {
                int total = progresso.UnlockedVerses != null ? progresso.UnlockedVerses.Count : 0;
                textoVersiculos.text = total == 1 ? "1 versículo coletado" : $"{total} versículos coletados";
            }

            if (textoStatusConta != null)
                textoStatusConta.text = progresso.SemAnuncios ? "Sem anúncios · ativo" : "Conta padrão";
        }

        private void LimparMensagem()
        {
            if (textoMensagem != null) textoMensagem.text = string.Empty;
        }
    }
}
