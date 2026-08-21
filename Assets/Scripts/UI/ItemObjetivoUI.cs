using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BibleMatch3
{
    /// <summary>
    /// Uma linha de objetivo no HUD ("faltam 8 x Pão"). Instanciado sob demanda
    /// pelo <see cref="GameHUDView"/>, um por TileType que a fase pede.
    /// </summary>
    public class ItemObjetivoUI : MonoBehaviour
    {
        [SerializeField] private Image icone;
        [SerializeField] private TextMeshProUGUI textoRestante;

        public TileType Tipo { get; private set; }

        public void Configurar(TileType tipo, Sprite sprite, int restante)
        {
            Tipo = tipo;

            if (icone != null)
            {
                icone.sprite = sprite;
                icone.enabled = sprite != null;
            }

            Atualizar(restante);
            gameObject.name = $"Objetivo_{tipo}";
        }

        public void Atualizar(int restante)
        {
            int mostrar = Mathf.Max(0, restante);
            if (textoRestante != null) textoRestante.text = mostrar > 0 ? mostrar.ToString() : "OK";
        }
    }
}
