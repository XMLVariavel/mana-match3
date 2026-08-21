using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BibleMatch3
{
    /// <summary>
    /// Cria o tabuleiro (configurável no Inspector), captura o input do jogador
    /// (clique/arrasto) e coordena a troca de peças vizinhas, delegando a detecção
    /// de combinações ao MatchDetector e a queda/reabastecimento ao BoardPhysics.
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [Header("Configuração da Grade")]
        [SerializeField] private int width = 8;
        [SerializeField] private int height = 8;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Transform boardOrigin;
        [SerializeField] private Transform tilesParent;

        [Header("Prefab e Sprites")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Sprite[] tileSprites;     // índice = (int)TileType
        [SerializeField] private Sprite[] specialSprites;  // índice = (int)SpecialType

        [Header("Input")]
        [SerializeField] private float swipeThreshold = 0.4f; // em unidades de mundo

        [Header("Managers")]
        [SerializeField] private MatchDetector matchDetector;
        [SerializeField] private BoardPhysics boardPhysics;
        [SerializeField] private ScoreAndObjectiveManager scoreManager;
        [SerializeField] private GameManager gameManager;

        [Header("Obstáculos (opcional — deixe vazio se a fase não usa bloqueadores)")]
        [SerializeField] private ObstacleManager obstacleManager;

        private Tile[,] grid;
        private Camera mainCamera;
        private Tile selectedTile;
        private Vector3 dragStartWorld;
        private bool inputLocked;
        private bool gestureConsumed;

        // --- Acessores públicos usados por MatchDetector / BoardPhysics ---
        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;
        public Tile[,] Grid => grid;
        public GameObject TilePrefab => tilePrefab;
        public Transform TilesParent => tilesParent;
        public Sprite GetSpriteFor(TileType type) => tileSprites[(int)type];
        public Sprite GetSpecialSprite(SpecialType type) => specialSprites[(int)type];

        // Tipos de peça sorteáveis na geração/reabastecimento. Por padrão, todos —
        // mas o GameManager pode restringir isso no Estudo Infinito para começar
        // mais fácil (menos tipos) e ir liberando mais conforme a pontuação sobe.
        private List<TileType> activeTypes;
        public IReadOnlyList<TileType> ActiveTypes => activeTypes;

        public void DefinirTiposAtivos(List<TileType> tipos)
        {
            if (tipos == null || tipos.Count == 0) return;
            activeTypes = new List<TileType>(tipos);
        }

        private void Awake()
        {
            mainCamera = Camera.main;
            grid = new Tile[width, height];
            activeTypes = new List<TileType>((TileType[])System.Enum.GetValues(typeof(TileType)));
            obstacleManager?.Initialize(width, height);
        }

        private void Start()
        {
            GenerateBoard();
        }

        /// <summary>
        /// Recria o tabuleiro quando o jogador troca de modo ou inicia uma nova fase.
        /// Isso garante que a quantidade de tipos ativos do Estudo Infinito seja
        /// aplicada imediatamente, em vez de apenas nas peças reabastecidas.
        /// </summary>
        public void ReiniciarTabuleiro()
        {
            if (grid == null)
                grid = new Tile[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y] != null)
                    {
                        grid[x, y].gameObject.SetActive(false);
                        Destroy(grid[x, y].gameObject);
                        grid[x, y] = null;
                    }
                }
            }

            obstacleManager?.ClearAll();
            GenerateBoard();
        }

        private void Update()
        {
            if (inputLocked) return;
            HandleInput();
        }

        // ---------------------------------------------------------------
        // Geração inicial do tabuleiro (garante ausência de matches prontos)
        // ---------------------------------------------------------------

        private void GenerateBoard()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TileType type = GetRandomTypeWithoutMatch(x, y);
                    SpawnTileAt(x, y, type);
                }
            }
        }

        /// <summary>
        /// Escolhe um tipo aleatório para a célula (x, y) excluindo os tipos que
        /// formariam imediatamente uma sequência de 3 (checa as 2 peças à
        /// esquerda e as 2 peças abaixo, que já estarão preenchidas nesse ponto).
        /// </summary>
        private TileType GetRandomTypeWithoutMatch(int x, int y)
        {
            List<TileType> possible = new List<TileType>(activeTypes);

            if (x >= 2 && grid[x - 1, y] != null && grid[x - 2, y] != null &&
                grid[x - 1, y].Type == grid[x - 2, y].Type)
            {
                possible.Remove(grid[x - 1, y].Type);
            }

            if (y >= 2 && grid[x, y - 1] != null && grid[x, y - 2] != null &&
                grid[x, y - 1].Type == grid[x, y - 2].Type)
            {
                possible.Remove(grid[x, y - 1].Type);
            }

            // Salvaguarda: se as restrições esvaziarem a lista (possível com poucos
            // tipos ativos), volta a permitir qualquer um dos tipos ativos.
            if (possible.Count == 0) possible.AddRange(activeTypes);

            return possible[Random.Range(0, possible.Count)];
        }

        private void SpawnTileAt(int x, int y, TileType type)
        {
            GameObject obj = Instantiate(tilePrefab, WorldPosition(x, y), Quaternion.identity, tilesParent);
            Tile tile = obj.GetComponent<Tile>();
            tile.Setup(type, x, y, GetSpriteFor(type));
            tile.FitToCell(cellSize);
            grid[x, y] = tile;
        }

        public Vector3 WorldPosition(int x, int y)
        {
            Vector3 origin = boardOrigin != null ? boardOrigin.position : Vector3.zero;
            return origin + new Vector3(x * cellSize, y * cellSize, 0f);
        }

        /// <summary>
        /// Redistribui os tipos das peças já existentes no tabuleiro (usado pelo
        /// poder avulso "Embaralhar Tabuleiro"). Não mexe em posição nem cria
        /// objetos novos — só reatribui TileType + sprite, célula por célula,
        /// evitando montar um match já pronto no resultado final.
        /// </summary>
        public void EmbaralharTabuleiro()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[x, y] == null) continue; // célula bloqueada por Pedra, por exemplo

                    TileType type = GetRandomTypeWithoutMatch(x, y);
                    grid[x, y].Setup(type, x, y, GetSpriteFor(type));
                    grid[x, y].FitToCell(cellSize);
                }
            }
        }

        /// <summary>
        /// Remove a peça de uma célula e a marca como bloqueada — usado por um
        /// carregador de fase para posicionar uma Pedra do Deserto antes do
        /// início da partida. Chame obstacleManager.PlaceObstacle logo em
        /// seguida para de fato criar o bloqueio visual/lógico na célula.
        /// </summary>
        public void RemoverPecaEBloquear(int x, int y)
        {
            Tile tile = grid[x, y];
            if (tile == null) return;

            tile.gameObject.SetActive(false);
            grid[x, y] = null;
        }

        // ---------------------------------------------------------------
        // Input: clique/arrasto para trocar peças vizinhas
        // ---------------------------------------------------------------
        // OBS: no Android, o Input Manager legado do Unity mapeia o toque
        // primário automaticamente para Input.mousePosition/GetMouseButton,
        // então este mesmo código funciona sem alteração no editor, PC e celular.

        public bool ModoMiraAtivo { get; private set; }
        public event System.Action<Tile> OnTileEscolhidaNoModoMira;

        /// <summary>
        /// Ativa o "modo mira": o próximo toque no tabuleiro não inicia uma
        /// troca — em vez disso, dispara OnTileEscolhidaNoModoMira com a peça
        /// tocada. Usado pelo poder avulso Martelo (escolher qual peça remover).
        /// </summary>
        public void AtivarModoMira()
        {
            ModoMiraAtivo = true;
            selectedTile = null;
        }

        private void HandleInput()
        {
            if (ModoMiraAtivo)
            {
                if (PointerDownThisFrame())
                {
                    Tile tile = GetTileUnderPointer();
                    ModoMiraAtivo = false;
                    if (tile != null) OnTileEscolhidaNoModoMira?.Invoke(tile);
                }
                return;
            }

            if (PointerDownThisFrame())
            {
                Tile tile = GetTileUnderPointer();
                if (tile != null && !IsLocked(tile))
                {
                    // Segundo toque em uma peça vizinha: troca por toque,
                    // sem exigir que o jogador faça um arrasto preciso.
                    if (selectedTile != null && selectedTile != tile && SaoVizinhas(selectedTile, tile))
                    {
                        TrySwap(selectedTile, tile);
                        LimparSelecao();
                    }
                    else
                    {
                        LimparSelecao();
                        selectedTile = tile;
                        selectedTile.SetSelected(true);
                        dragStartWorld = mainCamera.ScreenToWorldPoint(PointerPosition());
                        gestureConsumed = false;
                    }
                }
            }
            else if (PointerHeld() && selectedTile != null)
            {
                Vector3 current = mainCamera.ScreenToWorldPoint(PointerPosition());
                Vector3 delta = current - dragStartWorld;

                if (delta.magnitude >= swipeThreshold)
                {
                    gestureConsumed = true;
                    Vector2Int direction = GetSwipeDirection(delta);
                    Tile neighbor = GetNeighbor(selectedTile, direction);

                    if (neighbor != null)
                        TrySwap(selectedTile, neighbor);

                    LimparSelecao(); // evita disparar mais de uma troca no mesmo gesto
                }
            }
            else if (PointerUpThisFrame())
            {
                // Se o gesto foi um toque curto, permite selecionar uma peça e
                // tocar em uma vizinha no próximo toque. No arrasto, a troca já
                // foi disparada no limiar do gesto.
                if (!gestureConsumed && selectedTile != null)
                {
                    Tile tile = GetTileUnderPointer();
                    if (tile != null && tile != selectedTile && SaoVizinhas(selectedTile, tile))
                    {
                        TrySwap(selectedTile, tile);
                        LimparSelecao();
                    }
                }

                gestureConsumed = false;
            }
        }

        private bool PointerDownThisFrame()
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
            if (Mouse.current != null) return Mouse.current.leftButton.wasPressedThisFrame;
            return Input.GetMouseButtonDown(0);
        }

        private bool PointerHeld()
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) return true;
            if (Mouse.current != null) return Mouse.current.leftButton.isPressed;
            return Input.GetMouseButton(0);
        }

        private bool PointerUpThisFrame()
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame) return true;
            if (Mouse.current != null) return Mouse.current.leftButton.wasReleasedThisFrame;
            return Input.GetMouseButtonUp(0);
        }

        private Vector2 PointerPosition()
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                return Touchscreen.current.primaryTouch.position.ReadValue();
            if (Mouse.current != null) return Mouse.current.position.ReadValue();
            return Input.mousePosition;
        }

        private Tile GetTileUnderPointer()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return null;

            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(PointerPosition());
            worldPoint.z = 0f;

            // OverlapPoint é apropriado para toque/clique em uma célula. O
            // Raycast com direção zero não é consistente entre versões do Unity.
            Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);
            foreach (Collider2D hit in hits)
            {
                Tile tile = hit != null ? hit.GetComponentInParent<Tile>() : null;
                if (tile != null) return tile;
            }

            return null;
        }

        private bool SaoVizinhas(Tile a, Tile b) =>
            a != null && b != null && Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y) == 1;

        private void LimparSelecao()
        {
            if (selectedTile != null) selectedTile.SetSelected(false);
            selectedTile = null;
        }

        private Vector2Int GetSwipeDirection(Vector3 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                return delta.x > 0 ? Vector2Int.right : Vector2Int.left;

            return delta.y > 0 ? Vector2Int.up : Vector2Int.down;
        }

        private Tile GetNeighbor(Tile tile, Vector2Int direction)
        {
            int nx = tile.X + direction.x;
            int ny = tile.Y + direction.y;

            if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                return null;

            return grid[nx, ny];
        }

        // ---------------------------------------------------------------
        // Troca de peças, validação de match e reversão
        // ---------------------------------------------------------------

        private void TrySwap(Tile a, Tile b)
        {
            if (inputLocked || a == null || b == null) return;
            if (!SaoVizinhas(a, b)) return;
            if (IsLocked(a) || IsLocked(b)) return; // peça presa por Corrente não pode ser movida
            a.SetSelected(false);
            b.SetSelected(false);
            StartCoroutine(SwapRoutine(a, b));
        }

        private bool IsLocked(Tile tile) =>
            obstacleManager != null && obstacleManager.IsLocked(tile.X, tile.Y);

        private IEnumerator SwapRoutine(Tile a, Tile b)
        {
            inputLocked = true;

            yield return StartCoroutine(SwapVisualAndLogic(a, b));

            MatchResult result = EvaluateSwapResult(a, b);

            if (result.TilesToDestroy.Count == 0)
            {
                // Troca inválida: desfaz a movimentação (volta ao estado anterior).
                yield return StartCoroutine(SwapVisualAndLogic(a, b));
                inputLocked = false;
                yield break;
            }

            gameManager?.RegistrarJogadaValida();
            if (gameManager == null || gameManager.ModoUsaLimiteDeMovimentos)
                scoreManager.UseMove();

            yield return StartCoroutine(boardPhysics.ResolveBoard(matchDetector, scoreManager, result));

            inputLocked = false;
        }

        /// <summary>
        /// Decide o resultado de uma troca: cruzamento de especiais, match comum,
        /// ativação de uma peça especial isolada, ou troca inválida (result vazio).
        /// </summary>
        private MatchResult EvaluateSwapResult(Tile a, Tile b)
        {
            if (a.Special != SpecialType.Nenhum && b.Special != SpecialType.Nenhum)
                return matchDetector.ResolveSpecialCombo(grid, width, height, a, b);

            MatchResult result = matchDetector.FindMatches(grid, width, height);
            if (result.TilesToDestroy.Count > 0)
                return result;

            if (a.Special != SpecialType.Nenhum)
                return matchDetector.ActivateSpecial(grid, width, height, a);

            if (b.Special != SpecialType.Nenhum)
                return matchDetector.ActivateSpecial(grid, width, height, b);

            return result; // vazio => troca inválida, será revertida
        }

        private IEnumerator SwapVisualAndLogic(Tile a, Tile b)
        {
            int ax = a.X, ay = a.Y;
            int bx = b.X, by = b.Y;

            grid[ax, ay] = b;
            grid[bx, by] = a;

            bool doneA = false, doneB = false;
            a.MoveToGridPosition(bx, by, WorldPosition(bx, by), () => doneA = true);
            b.MoveToGridPosition(ax, ay, WorldPosition(ax, ay), () => doneB = true);

            while (!doneA || !doneB)
                yield return null;
        }
    }
}
