using System.Collections;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Tipos de peças "comuns" do tabuleiro, temática bíblica.
    /// </summary>
    public enum TileType
    {
        Pao,
        Peixe,
        Uva,
        Espiga,
        Azeite,
        Pomba
    }

    /// <summary>
    /// Tipos de peças especiais geradas por combinações.
    /// </summary>
    public enum SpecialType
    {
        Nenhum,
        Espada_Linha,   // Limpa a linha (row) inteira
        Espada_Coluna,  // Limpa a coluna inteira
        Tocha_Acesa,    // Explosão em área (3x3 no nível 1, 4x4+ nos seguintes)
        Arca_Alianca,   // Remove todas as peças do mesmo tipo do tabuleiro
        Estrela_Guia    // Busca peças do tipo-objetivo restante da fase
    }

    /// <summary>
    /// Representa uma peça individual no tabuleiro.
    /// Responsável apenas pelos próprios dados e apresentação visual —
    /// toda a lógica de tabuleiro fica em BoardManager / MatchDetector / BoardPhysics.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class Tile : MonoBehaviour
    {
        [Header("Dados da Peça")]
        public TileType Type;
        public SpecialType Special = SpecialType.Nenhum;

        [Header("Posição Lógica na Grade")]
        public int X;
        public int Y;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject destroyEffectPrefab; // partícula opcional
        [SerializeField] private float moveSpeed = 10f;          // unidades/seg usadas no Lerp

        public bool IsMoving { get; private set; }

        [Header("Layout Responsivo")]
        [Tooltip("Percentual máximo da célula ocupado pelo desenho. O colisor continua cobrindo a célula inteira.")]
        [SerializeField, Range(0.60f, 0.96f)] private float ocupacaoVisualDaCelula = 0.96f;

        private Coroutine moveRoutine;
        private Vector3 escalaBase = Vector3.one;
        private bool selecionada;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Define os dados lógicos e o visual da peça.
        /// Chamado pelo BoardManager (criação inicial) e pelo BoardPhysics (reabastecimento).
        /// </summary>
        public void Setup(TileType type, int x, int y, Sprite sprite)
        {
            Type = type;
            X = x;
            Y = y;
            Special = SpecialType.Nenhum;

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = Color.white;
            }

            escalaBase = Vector3.one;
            selecionada = false;
            transform.localScale = escalaBase;
        }

        /// <summary>
        /// Ajusta o desenho para caber dentro da célula mesmo quando os PNGs têm
        /// dimensões ou pixels-por-unidade diferentes. O collider é compensado
        /// para continuar ocupando uma célula inteira e o toque não fica menor.
        /// </summary>
        public void FitToCell(float cellSize)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                transform.localScale = Vector3.one;
                return;
            }

            float maiorLado = Mathf.Max(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y);
            float alvo = Mathf.Max(0.01f, cellSize * ocupacaoVisualDaCelula);
            float escala = maiorLado > 0.001f ? alvo / maiorLado : 1f;
            escalaBase = new Vector3(escala, escala, 1f);
            transform.localScale = selecionada ? escalaBase * 1.06f : escalaBase;

            BoxCollider2D colisor = GetComponent<BoxCollider2D>();
            if (colisor != null)
            {
                float escalaSegura = Mathf.Max(escala, 0.0001f);
                colisor.size = Vector2.one / escalaSegura;
            }
        }

        /// <summary>
        /// Destaca a peça selecionada sem alterar seu tamanho lógico na grade.
        /// </summary>
        public void SetSelected(bool selected)
        {
            selecionada = selected;
            transform.localScale = selected ? escalaBase * 1.06f : escalaBase;
            if (spriteRenderer != null)
                spriteRenderer.color = selected ? new Color(1f, 0.92f, 0.58f, 1f) : Color.white;
        }

        /// <summary>
        /// Promove a peça para um tipo especial, trocando o visual (ícone/skin).
        /// </summary>
        public void PromoteToSpecial(SpecialType special, Sprite specialSprite)
        {
            Special = special;
            if (specialSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = specialSprite;
                // O chamador aplica FitToCell quando conhece o tamanho da grade.
                transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// Move a peça suavemente até a posição de mundo correspondente à célula (newX, newY).
        /// Implementado com Coroutine + Lerp — pode ser trocado por LeanTween/DOTween
        /// substituindo apenas o corpo deste método (ex: transform.DOMove(worldTarget, duration)).
        /// </summary>
        public void MoveToGridPosition(int newX, int newY, Vector3 worldTarget, System.Action onComplete = null)
        {
            X = newX;
            Y = newY;

            if (moveRoutine != null)
                StopCoroutine(moveRoutine);

            moveRoutine = StartCoroutine(MoveRoutine(worldTarget, onComplete));
        }

        private IEnumerator MoveRoutine(Vector3 target, System.Action onComplete)
        {
            IsMoving = true;
            Vector3 start = transform.position;
            float distance = Vector3.Distance(start, target);

            // Evita divisão por zero e movimentos "instantâneos" perceptíveis.
            float duration = Mathf.Max(distance / moveSpeed, 0.05f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // SmoothStep dá uma sensação de "encaixe" mais agradável que um Lerp linear puro.
                transform.position = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            transform.position = target;
            IsMoving = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Efeito visual de destruição (escala até zero + partícula opcional),
        /// seguido da desativação do objeto para reuso via pool.
        /// </summary>
        public void PlayDestroyEffect(System.Action onComplete = null)
        {
            StartCoroutine(DestroyRoutine(onComplete));
        }

        private IEnumerator DestroyRoutine(System.Action onComplete)
        {
            if (destroyEffectPrefab != null)
                Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);

            const float duration = 0.18f;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }

            gameObject.SetActive(false);
            onComplete?.Invoke();
        }
    }
}
