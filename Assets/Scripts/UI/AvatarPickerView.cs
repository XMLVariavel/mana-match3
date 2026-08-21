using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BibleMatch3
{
    public class AvatarPickerView : MonoBehaviour
    {
        [SerializeField] private PerfilController controller;
        [SerializeField] private Transform container;
        [SerializeField] private Image preview;
        [SerializeField] private TextMeshProUGUI textoAvatar;

        private readonly List<Button> botoes = new List<Button>();
        private PlayerProgress progresso;

        private void OnEnable()
        {
            if (controller != null) controller.OnPerfilCarregado += HandlePerfil;
            MontarOpcoes();
        }

        private void OnDisable()
        {
            if (controller != null) controller.OnPerfilCarregado -= HandlePerfil;
        }

        private void HandlePerfil(PlayerProgress valor)
        {
            progresso = valor;
            AtualizarPreview();
        }

        private void MontarOpcoes()
        {
            if (container == null) return;

            var grid = container.GetComponent<GridLayoutGroup>() ?? container.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(112f, 132f);
            grid.spacing = new Vector2(10f, 10f);
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.UpperCenter;

            foreach (Button antigo in botoes)
                if (antigo != null) Destroy(antigo.gameObject);
            botoes.Clear();

            foreach (AvatarOption opcao in AvatarCatalog.Opcoes)
            {
                var raiz = new GameObject("Avatar_" + opcao.Id, typeof(RectTransform), typeof(Image), typeof(Button));
                raiz.transform.SetParent(container, false);

                var fundo = raiz.GetComponent<Image>();
                fundo.color = new Color(0.055f, 0.125f, 0.184f, 0.98f);

                var botao = raiz.GetComponent<Button>();
                botao.targetGraphic = fundo;
                var cores = botao.colors;
                cores.highlightedColor = new Color(0.25f, 0.42f, 0.52f, 1f);
                cores.pressedColor = new Color(0.12f, 0.3f, 0.35f, 1f);
                botao.colors = cores;

                var imagem = new GameObject("Retrato", typeof(RectTransform), typeof(Image));
                imagem.transform.SetParent(raiz.transform, false);
                var imagemRt = imagem.GetComponent<RectTransform>();
                imagemRt.anchorMin = new Vector2(0.08f, 0.18f);
                imagemRt.anchorMax = new Vector2(0.92f, 0.96f);
                imagemRt.offsetMin = Vector2.zero;
                imagemRt.offsetMax = Vector2.zero;
                var imagemComponente = imagem.GetComponent<Image>();
                imagemComponente.sprite = Resources.Load<Sprite>("Avatars/avatar_" + opcao.Id);
                imagemComponente.preserveAspect = true;
                imagemComponente.raycastTarget = false;

                var nome = new GameObject("Nome", typeof(RectTransform), typeof(TextMeshProUGUI));
                nome.transform.SetParent(raiz.transform, false);
                var nomeRt = nome.GetComponent<RectTransform>();
                nomeRt.anchorMin = new Vector2(0f, 0f);
                nomeRt.anchorMax = new Vector2(1f, 0.2f);
                nomeRt.offsetMin = new Vector2(4f, 2f);
                nomeRt.offsetMax = new Vector2(-4f, -2f);
                var textoNome = nome.GetComponent<TextMeshProUGUI>();
                textoNome.text = opcao.Nome;
                textoNome.fontSize = 14f;
                textoNome.alignment = TextAlignmentOptions.Center;
                textoNome.color = new Color(0.95f, 0.82f, 0.42f, 1f);
                textoNome.raycastTarget = false;

                botao.onClick.AddListener(() => Selecionar(opcao.Id));
                botoes.Add(botao);
            }

            AtualizarPreview();
        }

        private void Selecionar(string avatarId)
        {
            if (!AvatarCatalog.Existe(avatarId)) return;
            controller?.SelecionarAvatar(avatarId);
            if (progresso != null) progresso.AvatarId = avatarId;
            AtualizarPreview();
        }

        private void AtualizarPreview()
        {
            string avatarId = progresso != null && AvatarCatalog.Existe(progresso.AvatarId)
                ? progresso.AvatarId
                : AvatarCatalog.Padrao;
            AvatarOption opcao = AvatarCatalog.Obter(avatarId);
            if (preview != null)
            {
                preview.sprite = Resources.Load<Sprite>("Avatars/avatar_" + opcao.Id);
                preview.preserveAspect = true;
            }
            if (textoAvatar != null) textoAvatar.text = "Avatar: " + opcao.Nome;
        }
    }
}
