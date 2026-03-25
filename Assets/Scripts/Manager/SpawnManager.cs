using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : BaseManager<SpawnManager>
{
    [Header("Spawn Setting")]
    [SerializeField] private List<GameObject> entityPrefabs = new List<GameObject>();
    [SerializeField] private float spawnCountDown = 7f; //default 7 second
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float randomRotationRange = 80f; // ±80
    [SerializeField] private bool isSpawning = true;

    private Coroutine spawnCoroutine;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopSpawning();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu")
        {
            StopSpawning();//Stop spawn
            spawnPoint = null;
        }
        else if (IsLevelScene(scene.name))
        {
            FindSpawnPoint();
            StartSpawning();
        }
    }
    private bool IsLevelScene(string sceneName)
    {
        return sceneName.StartsWith("Level ");
    }
    private void FindSpawnPoint()
    {
        GameObject spawnObj = GameObject.FindWithTag("SpawnPoint");

        if (spawnObj != null)
        {
            spawnPoint = spawnObj.transform;
            Debug.Log($"[SpawnManager] Tìm thấy SpawnPoint tại {spawnPoint.position} trong scene {SceneManager.GetActiveScene().name}");
        }
        else
        {
            spawnPoint = transform; // fallback: dùng vị trí của chính SpawnManager
            Debug.LogWarning("[SpawnManager] Không tìm thấy GameObject có tag 'SpawnPoint'! Sử dụng vị trí mặc định của SpawnManager.");
        }
    }
    public void StartSpawning()
    {
        if (entityPrefabs.Count == 0)
        {
            Debug.LogError("[SpawnManager] Chưa gán entityPrefabs!");
            return;
        }

        StopSpawning(); // Dừng coroutine cũ trước

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnRoutine());
        Debug.Log("[SpawnManager] Bắt đầu spawn Entity");
    }

    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            if (spawnPoint != null)        // Kiểm tra an toàn
            {
                SpawnOneEntity();
            }
            else
            {
                Debug.LogWarning("[SpawnManager] spawnPoint bị null → Dừng spawn");
                yield break;
            }

            yield return new WaitForSeconds(spawnCountDown);
        }
    }

    private void SpawnOneEntity()
    {
        //Random Prefab
        int randomIndex = Random.Range(0, entityPrefabs.Count);
        GameObject prefabToSpawn = entityPrefabs[randomIndex];

        //Instantiate Spawn
        GameObject newEntity = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);

        //Random Z Rotation
        float randomZ = Random.Range(-randomRotationRange, randomRotationRange);
        newEntity.transform.rotation = Quaternion.Euler(0f, 0f, randomZ);

        //Check Entity Rigidbody
        Rigidbody rb = newEntity.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // cho rơi
        }

        //Debug.Log($"Spawn Entity: {prefabToSpawn.name} | Z Rotation: {randomZ:F1}°");
    }

    // ====================== CÁC HÀM DEBUG & TEST ======================
    #region ContextMenu
    [ContextMenu("Spawn One Entity Now")]
    public void SpawnOneEntityNow()
    {
        SpawnOneEntity();
    }

    [ContextMenu("Spawn 5 Entities Quickly")]
    public void SpawnFiveQuickly()
    {
        for (int i = 0; i < 5; i++)
        {
            SpawnOneEntity();
        }
    }
    #endregion
}
