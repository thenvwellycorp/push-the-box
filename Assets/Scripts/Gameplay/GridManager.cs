using System.Collections.Generic;
using UnityEngine;

namespace PushTheBox.Gameplay
{
    /// <summary>
    /// Manages the logical grid, coordinates translation, spatial queries,
    /// and dynamic board layout / camera framing.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Grid Configuration")]
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private float cameraPadding = 1.8f;

        private Vector2 gridOriginOffset = Vector2.zero;
        private int boardWidth;
        private int boardHeight;

        private readonly HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        private readonly Dictionary<Vector2Int, BoxController> boxLookup = new Dictionary<Vector2Int, BoxController>();
        private readonly Dictionary<Vector2Int, TargetController> targetLookup = new Dictionary<Vector2Int, TargetController>();
        private readonly List<BoxController> allBoxes = new List<BoxController>();
        private readonly List<TargetController> allTargets = new List<TargetController>();

        public float CellSize => cellSize;
        public IReadOnlyList<BoxController> AllBoxes => allBoxes;
        public IReadOnlyList<TargetController> AllTargets => allTargets;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this && Application.isPlaying)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        /// <summary>
        /// Initializes grid dimensions and centers coordinate system around world (0,0).
        /// </summary>
        public void InitializeGrid(int width, int height)
        {
            Instance = this;
            boardWidth = width;
            boardHeight = height;

            // Center grid around (0,0) world coordinates
            gridOriginOffset = new Vector2(
                -(width - 1) * 0.5f * cellSize,
                -(height - 1) * 0.5f * cellSize
            );

            ClearGrid();
            AdjustCameraFraming();
        }

        public void ClearGrid()
        {
            wallPositions.Clear();
            boxLookup.Clear();
            targetLookup.Clear();
            allBoxes.Clear();
            allTargets.Clear();
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float worldX = gridOriginOffset.x + (gridPos.x * cellSize);
            float worldY = gridOriginOffset.y + (gridPos.y * cellSize);
            return new Vector3(worldX, worldY, 0f);
        }

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int gx = Mathf.RoundToInt((worldPos.x - gridOriginOffset.x) / cellSize);
            int gy = Mathf.RoundToInt((worldPos.y - gridOriginOffset.y) / cellSize);
            return new Vector2Int(gx, gy);
        }

        public void RegisterWall(Vector2Int pos)
        {
            wallPositions.Add(pos);
        }

        public void RegisterBox(BoxController box)
        {
            allBoxes.Add(box);
            boxLookup[box.GridPosition] = box;
        }

        public void RegisterTarget(TargetController target)
        {
            allTargets.Add(target);
            targetLookup[target.GridPosition] = target;
        }

        public void UpdateBoxPosition(BoxController box, Vector2Int oldPos, Vector2Int newPos)
        {
            if (boxLookup.TryGetValue(oldPos, out BoxController current) && current == box)
            {
                boxLookup.Remove(oldPos);
            }
            boxLookup[newPos] = box;
        }

        public bool IsWall(Vector2Int pos)
        {
            return wallPositions.Contains(pos);
        }

        public BoxController GetBoxAt(Vector2Int pos)
        {
            boxLookup.TryGetValue(pos, out BoxController box);
            return box;
        }

        public TargetController GetTargetAt(Vector2Int pos)
        {
            targetLookup.TryGetValue(pos, out TargetController target);
            return target;
        }

        public bool IsTarget(Vector2Int pos)
        {
            return targetLookup.ContainsKey(pos);
        }

        public bool IsCellWalkable(Vector2Int pos)
        {
            // Walkable if no wall and no box
            return !IsWall(pos) && GetBoxAt(pos) == null;
        }

        public bool AreAllBoxesOnTargets()
        {
            if (allBoxes.Count == 0 || allTargets.Count == 0)
                return false;

            int onTargetCount = 0;
            for (int i = 0; i < allBoxes.Count; i++)
            {
                if (IsTarget(allBoxes[i].GridPosition))
                {
                    onTargetCount++;
                }
            }

            return onTargetCount == allBoxes.Count;
        }

        public int GetBoxesOnTargetCount()
        {
            int count = 0;
            for (int i = 0; i < allBoxes.Count; i++)
            {
                if (IsTarget(allBoxes[i].GridPosition))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Automatically adjusts the orthographic camera so the grid is neatly centered
        /// and fits well in both portrait and landscape screen orientations.
        /// </summary>
        public void AdjustCameraFraming()
        {
            Camera cam = Camera.main;
            if (cam == null || !cam.orthographic)
                return;

            cam.transform.position = new Vector3(0f, 0.4f, -10f); // slight upward bias for bottom D-pad / UI

            float screenAspect = (float)Screen.width / Screen.height;
            float worldWidth = (boardWidth * cellSize) + (cameraPadding * 2f);
            float worldHeight = (boardHeight * cellSize) + (cameraPadding * 2f) + 1.5f; // Extra space for HUD

            float verticalSize = worldHeight * 0.5f;
            float horizontalSize = (worldWidth * 0.5f) / screenAspect;

            cam.orthographicSize = Mathf.Max(verticalSize, horizontalSize, 4.5f);
        }
    }
}
