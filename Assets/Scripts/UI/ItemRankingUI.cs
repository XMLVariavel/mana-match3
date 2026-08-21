using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BibleMatch3
{
    /// <summary>Uma linha do placar: posição, nome e pontuação.</summary>
    public class ItemRankingUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textoPosicao;
        [SerializeField] private TextMeshProUGUI textoNome;
        [SerializeField] private TextMeshProUGUI textoScore;
        [SerializeField] private TextMeshProUGUI textoModo;
        [SerializeField] private Image imagemAvatar;

        [Header("Destaque do próprio jogador")]
        [SerializeField] private Color corNormal = new Color(0.92f, 0.92f, 0.95f);
        [SerializeField] private Color corDestaque = new Color(1f, 0.85f, 0.35f);

        public void Configurar(int posicao, string nome, int score, bool ehVoce)
        {
            Configurar(posicao, nome, score, AvatarCatalog.Padrao, "geral", ehVoce);
        }

        public void Configurar(int posicao, string nome, int score, string avatarId, string modo, bool ehVoce)
        {
            if (textoPosicao != null) textoPosicao.text = $"{posicao}º";
            if (textoNome != null) textoNome.text = string.IsNullOrWhiteSpace(nome) ? "Peregrino" : nome;
            if (textoScore != null) textoScore.text = score.ToString();
            if (textoModo != null) textoModo.text = string.IsNullOrWhiteSpace(modo) ? "GERAL" : modo.ToUpperInvariant();
            if (imagemAvatar != null)
            {
                string id = AvatarCatalog.Existe(avatarId) ? avatarId : AvatarCatalog.Padrao;
                imagemAvatar.sprite = Resources.Load<Sprite>("Avatars/avatar_" + id);
                imagemAvatar.preserveAspect = true;
            }

            Color cor = ehVoce ? corDestaque : corNormal;
            if (textoPosicao != null) textoPosicao.color = cor;
            if (textoNome != null) textoNome.color = cor;
            if (textoScore != null) textoScore.color = cor;

            gameObject.name = $"ItemRanking_{posicao}";
        }
    }
}
