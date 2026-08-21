using System.Collections.Generic;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Alterna entre as telas do jogo. Cada tela é um GameObject/Canvas raiz
    /// registrado aqui por nome — este script só liga/desliga o GameObject
    /// certo, não sabe nada sobre o conteúdo de cada tela.
    /// </summary>
    public class ScreenNavigator : MonoBehaviour
    {
        [System.Serializable]
        public class Tela
        {
            public string Nome;
            public GameObject Raiz;
        }

        [SerializeField] private List<Tela> telas = new List<Tela>();
        [SerializeField] private string telaInicial;

        private Dictionary<string, GameObject> mapa;
        public string TelaAtual { get; private set; }

        private void Awake()
        {
            mapa = new Dictionary<string, GameObject>();
            foreach (Tela tela in telas)
            {
                if (tela.Raiz == null) continue;
                mapa[tela.Nome] = tela.Raiz;
                tela.Raiz.SetActive(false);
            }
        }

        private void Start()
        {
            if (!string.IsNullOrEmpty(telaInicial)) Mostrar(telaInicial);
        }

        public void Mostrar(string nomeDaTela)
        {
            if (!mapa.TryGetValue(nomeDaTela, out GameObject raiz))
            {
                Debug.LogWarning($"Tela '{nomeDaTela}' não registrada no ScreenNavigator.");
                return;
            }

            if (!string.IsNullOrEmpty(TelaAtual) && mapa.TryGetValue(TelaAtual, out GameObject atual))
                atual.SetActive(false);

            raiz.SetActive(true);
            TelaAtual = nomeDaTela;
        }
    }
}
