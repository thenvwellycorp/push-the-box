using System.Collections.Generic;
using UnityEngine;
using PushTheBox.Gameplay;

namespace PushTheBox.Level
{
    /// <summary>
    /// Responsible for instantiating and cleaning up level game entities based on LevelData.
    /// Demonstrates the Factory and Single Responsibility patterns.
    /// </summary>
    public class LevelLoader : MonoBehaviour
    {
        [Header("Entity Prefabs (Optional - falls back to clean procedural setup)")]
        [SerializeField] private PlayerController playerPrefab;
        [SerializeField] private BoxController boxPrefab;
        [SerializeField] private WallController wallPrefab;
        [SerializeField] private TargetController targetPrefab;
        [SerializeField] private GameObject floorTilePrefab;

        [Header("Hierarchy Containers")]
        [SerializeField] private Transform entitiesRoot;

        private PlayerController currentPlayer;
        private readonly List<GameObject> spawnedObjects = new List<GameObject>();

        public PlayerController CurrentPlayer => currentPlayer;

        private void EnsureEntitiesRoot()
        {
            if (entitiesRoot == null)
            {
                GameObject root = new GameObject("EntitiesRoot");
                root.transform.SetParent(transform);
                entitiesRoot = root.transform;
            }
        }

        public void ClearLevel()
        {
            EnsureEntitiesRoot();

            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                if (spawnedObjects[i] != null)
                {
                    Destroy(spawnedObjects[i]);
                }
            }
            spawnedObjects.Clear();
            currentPlayer = null;

            if (GridManager.Instance != null)
            {
                GridManager.Instance.ClearGrid();
            }
        }

        public void LoadLevel(LevelData data)
        {
            ClearLevel();

            if (data == null)
            {
                Debug.LogError("[LevelLoader] LevelData is null!");
                return;
            }

            GridManager.Instance.InitializeGrid(data.width, data.height);

            // 1. Spawn Floor Tiles across the board
            SpawnFloor(data.width, data.height);

            // 2. Spawn Targets
            if (data.targets != null)
            {
                for (int i = 0; i < data.targets.Length; i++)
                {
                    SpawnTarget(data.targets[i]);
                }
            }

            // 3. Spawn Walls
            if (data.walls != null)
            {
                for (int i = 0; i < data.walls.Length; i++)
                {
                    SpawnWall(data.walls[i]);
                }
            }

            // 4. Spawn Boxes
            if (data.boxes != null)
            {
                for (int i = 0; i < data.boxes.Length; i++)
                {
                    SpawnBox(i + 1, data.boxes[i]);
                }
            }

            // 5. Spawn Player
            SpawnPlayer(data.playerPosition);
        }

        private void SpawnFloor(int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Vector3 worldPos = GridManager.Instance.GridToWorld(pos);

                    GameObject floorObj;
                    if (floorTilePrefab != null)
                    {
                        floorObj = Instantiate(floorTilePrefab, worldPos, Quaternion.identity, entitiesRoot);
                    }
                    else
                    {
                        floorObj = new GameObject($"Floor_{x}_{y}");
                        floorObj.transform.position = worldPos;
                        floorObj.transform.SetParent(entitiesRoot);
                        SpriteRenderer sr = floorObj.AddComponent<SpriteRenderer>();
                        sr.sortingOrder = -10;
                        // Checkerboard pattern tint
                        bool isEven = (x + y) % 2 == 0;
                        sr.color = isEven ? new Color(0.18f, 0.22f, 0.28f, 1f) : new Color(0.14f, 0.17f, 0.23f, 1f);
                    }
                    spawnedObjects.Add(floorObj);
                }
            }
        }

        private void SpawnWall(Vector2Int pos)
        {
            Vector3 worldPos = GridManager.Instance.GridToWorld(pos);
            WallController wall;

            if (wallPrefab != null)
            {
                wall = Instantiate(wallPrefab, worldPos, Quaternion.identity, entitiesRoot);
            }
            else
            {
                GameObject obj = new GameObject($"Wall_{pos.x}_{pos.y}");
                obj.transform.position = worldPos;
                obj.transform.SetParent(entitiesRoot);
                SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 5;
                sr.color = new Color(0.35f, 0.40f, 0.48f, 1f);
                wall = obj.AddComponent<WallController>();
            }

            wall.Initialize(pos, worldPos);
            GridManager.Instance.RegisterWall(pos);
            spawnedObjects.Add(wall.gameObject);
        }

        private void SpawnTarget(Vector2Int pos)
        {
            Vector3 worldPos = GridManager.Instance.GridToWorld(pos);
            TargetController target;

            if (targetPrefab != null)
            {
                target = Instantiate(targetPrefab, worldPos, Quaternion.identity, entitiesRoot);
            }
            else
            {
                GameObject obj = new GameObject($"Target_{pos.x}_{pos.y}");
                obj.transform.position = worldPos;
                obj.transform.SetParent(entitiesRoot);
                SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = -5;
                sr.color = new Color(0.2f, 0.8f, 0.9f, 0.85f);
                target = obj.AddComponent<TargetController>();
            }

            target.Initialize(pos, worldPos);
            GridManager.Instance.RegisterTarget(target);
            spawnedObjects.Add(target.gameObject);
        }

        private void SpawnBox(int id, Vector2Int pos)
        {
            Vector3 worldPos = GridManager.Instance.GridToWorld(pos);
            BoxController box;

            if (boxPrefab != null)
            {
                box = Instantiate(boxPrefab, worldPos, Quaternion.identity, entitiesRoot);
            }
            else
            {
                GameObject obj = new GameObject($"Box_{id}");
                obj.transform.position = worldPos;
                obj.transform.SetParent(entitiesRoot);
                SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 10;
                sr.color = new Color(0.95f, 0.72f, 0.35f, 1f);
                box = obj.AddComponent<BoxController>();
            }

            box.Initialize(id, pos, worldPos);
            GridManager.Instance.RegisterBox(box);
            spawnedObjects.Add(box.gameObject);
        }

        private void SpawnPlayer(Vector2Int pos)
        {
            Vector3 worldPos = GridManager.Instance.GridToWorld(pos);

            if (playerPrefab != null)
            {
                currentPlayer = Instantiate(playerPrefab, worldPos, Quaternion.identity, entitiesRoot);
            }
            else
            {
                GameObject obj = new GameObject("Player");
                obj.transform.position = worldPos;
                obj.transform.SetParent(entitiesRoot);
                SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 15;
                sr.color = new Color(0.2f, 0.65f, 1f, 1f);
                currentPlayer = obj.AddComponent<PlayerController>();
            }

            currentPlayer.Initialize(pos, worldPos);
            spawnedObjects.Add(currentPlayer.gameObject);
        }
    }
}
