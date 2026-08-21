using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Desenha o Top N do <see cref="RankingController"/>. O controller já
    /// dispara CarregarRanking() no OnEnable, então esta view só precisa
    /// escutar o resultado e o erro.
    /// </summary>
    public class RankingView : MonoBehaviour
    {
        [Header("Origem")]
        [SerializeField] private RankingController controller;
        [SerializeField] private FirebaseManager firebaseManager;

        [Header("Lista")]
        [SerializeField] private Transform container;
        [SerializeField] private ItemRankingUI itemPrefab;

        [Header("Textos")]
        [SerializeField] private TextMeshProUGUI textoMensagem;

        [SerializeField] private string mensagemCarregando = "Carregando placar...";
        [SerializeField] private string mensagemVazio = "Ninguém no placar ainda. Seja o primeiro!";

        private readonly List<ItemRankingUI> itens = new List<ItemRankingUI>();

        private void OnEnable()
        {
            if (controller == null) return;

            controller.OnRankingCarregado += HandleRanking;
            controller.OnErro += HandleErro;

            Limpar();
            if (textoMensagem != null) textoMensagem.text = mensagemCarregando;
        }

        private void OnDisable()
        {
            if (controller == null) return;

            controller.OnRankingCarregado -= HandleRanking;
            controller.OnErro -= HandleErro;

            Limpar();
        }

        private void HandleRanking(List<RankingEntry> entradas)
        {
            Limpar();

            if (entradas == null || entradas.Count == 0)
            {
                if (textoMensagem != null) textoMensagem.text = mensagemVazio;
                return;
            }

            if (textoMensagem != null) textoMensagem.text = string.Empty;
            if (container == null || itemPrefab == null) return;

            string meuUid = firebaseManager != null && firebaseManager.ProgressoAtual != null
                ? firebaseManager.ProgressoAtual.Uid
                : null;

            for (int i = 0; i < entradas.Count; i++)
            {
                RankingEntry entrada = entradas[i];
                ItemRankingUI item = Instantiate(itemPrefab, container);
                bool ehVoce = !string.IsNullOrEmpty(meuUid) && entrada.Uid == meuUid;
                item.Configurar(i + 1, entrada.Nome, entrada.Score, entrada.AvatarId, entrada.Modo, ehVoce);
                itens.Add(item);
            }
        }

        private void HandleErro(string mensagem)
        {
            Limpar();
            if (textoMensagem != null) textoMensagem.text = mensagem;
        }

        private void Limpar()
        {
            foreach (ItemRankingUI item in itens)
                if (item != null) Destroy(item.gameObject);

            itens.Clear();
        }
    }
}
