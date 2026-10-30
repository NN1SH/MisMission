using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObjectSpawner : MonoBehaviour
{
    public enum ObjectType
    {
        SmallStar,
        BigStar,
        Enemy
    }
    public Tilemap tilemap;
    public GameObject[] objectPrefabs; // SmallStar, BigStar, และ placeholder สำหรับ Enemy
    public GameObject[] enemyPrefabs; // List สำหรับ Enemy 3 ตัว
    public float bigStarProbability = 0.1f; // 10% chance for a big star
    public float enemyProbability = 0.1f; // 10% chance for an enemy
    public int maxObjects = 10; // Maximum number of objects to spawn
    public float Starlifetime = 10f; // Lifetime of stars in seconds
    public float spawnInterval = 0.5f;

    private List<Vector3> validSpawnPosition = new List<Vector3>();
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private bool isSpawning = false;

    void Start()
    {
        GameController.OnResetGame += LevelChange;
        // ตรวจสอบ enemyPrefabs ใน Start
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("EnemyPrefabs array is empty or null in ObjectSpawner!");
        }
        else
        {
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (enemyPrefabs[i] == null)
                {
                    Debug.LogError($"EnemyPrefabs[{i}] is null in ObjectSpawner!");
                }
                else
                {
                    Debug.Log($"EnemyPrefabs[{i}]: {enemyPrefabs[i].name}");
                }
            }
        }
        InitializeSpawner();
    }

    void OnDestroy()
    {
        GameController.OnResetGame -= LevelChange;
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("ObjectSpawner is inactive, cannot spawn objects!");
            return;
        }

        if (!tilemap || !tilemap.gameObject.activeInHierarchy)
        {
            StartCoroutine(FindTilemapAndInitialize());
        }
        if (!isSpawning && ActiveObjectCount() < maxObjects && validSpawnPosition.Count > 0)
        {
            StartCoroutine(SpawnObjectsIfNeeded());
        }
    }

    private void InitializeSpawner()
    {
        StopAllCoroutines(); // หยุด Coroutine ทั้งหมด
        isSpawning = false;
        DestroyAllSpawnedObject();
        StartCoroutine(FindTilemapAndInitialize());
    }

    private void LevelChange(Tilemap newTilemap)
    {
        Debug.Log("ObjectSpawner: LevelChange called with Tilemap: " + (newTilemap != null ? newTilemap.name : "null"));
        tilemap = newTilemap;
        InitializeSpawner();
    }

    private IEnumerator FindTilemapAndInitialize()
    {
        if (tilemap != null && tilemap.gameObject.activeInHierarchy)
        {
            GatherValidPositions();
            if (validSpawnPosition.Count > 0)
            {
                StartCoroutine(SpawnObjectsIfNeeded());
            }
            else
            {
                Debug.LogWarning("No valid spawn positions found for Tilemap!");
            }
            yield break;
        }

        tilemap = null;
        for (int i = 0; i < 10; i++) // ลองค้นหา 10 ครั้ง
        {
            tilemap = GameObject.Find("Ground")?.GetComponent<Tilemap>();
            if (tilemap != null && tilemap.gameObject.activeInHierarchy)
            {
                GatherValidPositions();
                if (validSpawnPosition.Count > 0)
                {
                    StartCoroutine(SpawnObjectsIfNeeded());
                }
                else
                {
                    Debug.LogWarning("No valid spawn positions found for Tilemap!");
                }
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        Debug.LogError("Tilemap 'Ground' not found or inactive after retrying!");
    }

    private int ActiveObjectCount()
    {
        spawnedObjects.RemoveAll(item => item == null);
        return spawnedObjects.Count;
    }

    private IEnumerator SpawnObjectsIfNeeded()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Cannot start SpawnObjectsIfNeeded: ObjectSpawner is inactive!");
            yield break;
        }

        isSpawning = true;
        while (ActiveObjectCount() < maxObjects && validSpawnPosition.Count > 0)
        {
            SpawnObject();
            yield return new WaitForSeconds(spawnInterval);
        }
        isSpawning = false;
    }

    private bool PositionHasObject(Vector3 positionToCheck)
    {
        return spawnedObjects.Any(checkedObj => checkedObj && Vector3.Distance(checkedObj.transform.position, positionToCheck) < 1.0f);
    }

    private ObjectType RandomObjectType()
    {
        float randomChoice = Random.value;

        if (randomChoice <= enemyProbability)
        {
            return ObjectType.Enemy;
        }
        else if (randomChoice <= (enemyProbability + bigStarProbability))
        {
            return ObjectType.BigStar;
        }
        else
        {
            return ObjectType.SmallStar;
        }
    }

    private void SpawnObject()
    {
        if (validSpawnPosition.Count == 0)
        {
            Debug.LogWarning("No valid spawn positions available!");
            return;
        }

        Vector3 spawnPosition = Vector3.zero;
        bool validPositionFound = false;

        while (!validPositionFound && validSpawnPosition.Count > 0)
        {
            int randomIndex = Random.Range(0, validSpawnPosition.Count);
            Vector3 potentialPosition = validSpawnPosition[randomIndex];
            Vector3 leftPosition = potentialPosition + Vector3.left;
            Vector3 rightPosition = potentialPosition + Vector3.right;

            if (!PositionHasObject(leftPosition) && !PositionHasObject(rightPosition))
            {
                spawnPosition = potentialPosition;
                validPositionFound = true;
            }
            validSpawnPosition.RemoveAt(randomIndex);
        }

        if (validPositionFound)
        {
            ObjectType objectType = RandomObjectType();
            GameObject prefabToSpawn;

            if (objectType == ObjectType.Enemy)
            {
                if (enemyPrefabs == null || enemyPrefabs.Length == 0)
                {
                    Debug.LogError("EnemyPrefabs array is empty or null!");
                    return;
                }
                // ตรวจสอบว่าไม่มี Prefab เป็น null
                List<GameObject> validEnemyPrefabs = enemyPrefabs.Where(prefab => prefab != null).ToList();
                if (validEnemyPrefabs.Count == 0)
                {
                    Debug.LogError("All EnemyPrefabs are null!");
                    return;
                }
                prefabToSpawn = validEnemyPrefabs[Random.Range(0, validEnemyPrefabs.Count)];
            }
            else
            {
                prefabToSpawn = objectPrefabs[(int)objectType];
                if (prefabToSpawn == null)
                {
                    Debug.LogError($"ObjectPrefabs[{(int)objectType}] is null for {objectType}!");
                    return;
                }
            }

            GameObject gameObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            spawnedObjects.Add(gameObject);

            if (objectType != ObjectType.Enemy)
            {
                StartCoroutine(DestroyObjectAfterTime(gameObject, Starlifetime));
            }
            Debug.Log($"Spawned {objectType} ({gameObject.name}) at {spawnPosition}");
        }
    }

    private IEnumerator DestroyObjectAfterTime(GameObject gameObject, float time)
    {
        yield return new WaitForSeconds(time);

        if (gameObject)
        {
            spawnedObjects.Remove(gameObject);
            validSpawnPosition.Add(gameObject.transform.position);
            Destroy(gameObject);
            Debug.Log($"Destroyed {gameObject.name} after {time} seconds");
        }
    }

    private void DestroyAllSpawnedObject()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
        Debug.Log("All spawned objects destroyed");
    }

    private void GatherValidPositions()
    {
        validSpawnPosition.Clear();
        if (tilemap == null)
        {
            Debug.LogWarning("Tilemap is null in GatherValidPositions!");
            return;
        }

        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);
        Vector3 start = tilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    Vector3 place = start + new Vector3(x + 0.5f, y + 2f, 0);
                    validSpawnPosition.Add(place);
                }
            }
        }
        Debug.Log($"Found {validSpawnPosition.Count} valid spawn positions for Tilemap: {tilemap.name}");
    }
}