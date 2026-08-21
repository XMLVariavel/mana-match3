using System.Collections;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Bloqueadores de tabuleiro (dificuldade de fase — diferente dos
    /// "bloqueadores de progresso" como vidas, que ficam no LivesManager).
    /// </summary>
    public enum ObstacleType
    {
        Nenhum,
        PedraDeserto,  // Ocupa a célula sozinha (sem peça embaixo). 2 hits adjacentes para quebrar.
        Corrente,      // Trava a peça por baixo — não pode ser selecionada para troca. 1 match adjacente libera.
        Gelo,          // Camada sobre a peça. Absorve 1 hit e poupa a peça naquela passada.
        CaixaSelada    // Esconde a peça (visualmente coberta). 1 match adjacente revela/abre.
    }

    /// <summary>
    /// Representa um obstáculo numa célula específica. Não se move nem participa
    /// da troca normal de peças — quem gerencia sua vida é o ObstacleManager.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Obstacle : MonoBehaviour
    {
        public ObstacleType Type { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int HitsRemaining { get; private set; }

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject breakEffectPrefab;

        public void Setup(ObstacleType type, int x, int y, int hits, Sprite sprite)
        {
            Type = type;
            X = x;
            Y = y;
            HitsRemaining = hits;

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (sprite != null) spriteRenderer.sprite = sprite;
        }

        /// <summary>
        /// Registra um hit (match adjacente ou, no caso do Gelo, na própria célula).
        /// </summary>
        public void RegisterHit()
        {
            HitsRemaining = Mathf.Max(0, HitsRemaining - 1);
        }

        public bool IsBroken => HitsRemaining <= 0;

        public void PlayBreakEffect()
        {
            if (breakEffectPrefab != null)
                Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);

            gameObject.SetActive(false);
        }
    }
}
