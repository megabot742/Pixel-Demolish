using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.VisualScripting;

public class SpawnManager : BaseManager<SpawnManager>
{
    [Header("Spawn Setting")]
    [SerializeField] private EntityDatabaseSO entityDatabase;
    [SerializeField] private float spawnCountDown = 7f; //default 7 second
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float randomRotationRange = 80f; // ±80

    [Header("Pixel Prefab Reference")]
    [SerializeField] private GameObject pixelCubeBasePrefab;
    public bool IsLevelCompleted() => isLevelCompleted;
    private bool canSpawn = true;
    private bool isLevelCompleted = false;

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
            isLevelCompleted = false;
        }
        else if (IsLevelScene(scene.name))
        {
            FindSpawnPoint();
            isLevelCompleted = false;
            canSpawn = true;

            StopSpawning(); //Dừng coroutine của scene trước
            StartSpawning(); //Chạy Coroutine của scene mới
        }
    }
    #region Handle
    private bool IsLevelScene(string sceneName)
    {
        return sceneName.StartsWith("Level "); //Kiểm tra level scene
    }
    private void FindSpawnPoint()
    {
        GameObject spawnObj = GameObject.FindWithTag("SpawnPoint");

        if (spawnObj != null)
        {
            spawnPoint = spawnObj.transform; //Setup vị trí spawn đã thiết lập từ trước
        }
        else
        {
            spawnPoint = transform; //Sài vị trí hiện tại của SpawnManager
        }
    }
    public void StartSpawning() //Bắt đầu spawn
    {
        if (entityDatabase == null) //Checck enity trong spawn
        {
            Debug.LogWarning("SpawnManager: Chưa có entityDatabase nào!");
            return;
        }

        StopSpawning();// Đảm bảo không chạy 2 coroutine
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning() //Dừng spawn
    {
        if (spawnCoroutine != null) //Dừng couroutine
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    public void SetCanSpawn(bool value)//Kiểm tra cho phép spawn
    {
        canSpawn = value;
    }
    public void CompleteLevel()//Kiểm tra độ hoàn thành level
    {
        isLevelCompleted = true;
        StopSpawning();
    }

    private IEnumerator SpawnRoutine()
    {
        while (!isLevelCompleted)
        {
            if (canSpawn && spawnPoint != null)
            {
                SpawnOneEntityAsync();
                yield return new WaitForSeconds(spawnCountDown);   // Đếm ngược để spawn
            }
            else
            {
                // Khu vực bị chiếm → tạm dừng đếm, nhưng không reset timer
                yield return new WaitForSeconds(0.2f);   // Kiểm tra trạng thái cho phép spwan mỗi 0.2s
            }
        }
    }

    private void SpawnOneEntityAsync()
    {
        if (entityDatabase == null || entityDatabase.entityReferences.Count == 0) return;

        int randomIndex = Random.Range(0, entityDatabase.entityReferences.Count);
        var selectedRef = entityDatabase.entityReferences[randomIndex];

        Addressables.LoadAssetAsync<EntityData>(selectedRef).Completed += handle =>
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            CreateEntityFromData(handle.Result);
        }
        else
        {
            Debug.LogError($"[SpawnManager] Load EntityData thất bại: {selectedRef}");
        }
    };
    }
    private void CreateEntityFromData(EntityData data)
    {
        if (data == null || data.pixels.Count == 0) return;

        GameObject entityGO = new GameObject(data.name);
        entityGO.transform.position = spawnPoint.position;

        Enity enity = entityGO.AddComponent<Enity>();

        Vector3 minPos = Vector3.one * float.MaxValue;
        foreach (var p in data.pixels)
            minPos = Vector3.Min(minPos, p.localPosition);

        foreach (var pixel in data.pixels)
        {
            GameObject cubeObj = Instantiate(pixelCubeBasePrefab, entityGO.transform);
            cubeObj.transform.localPosition = pixel.localPosition - minPos;

            ColorCube colorComp = cubeObj.GetComponent<ColorCube>();
            if (colorComp != null)
                colorComp.CubeColor = pixel.color;
        }

        // Random rotation Z
        float randomZ = Random.Range(-randomRotationRange, randomRotationRange);
        entityGO.transform.rotation = Quaternion.Euler(0f, 0f, randomZ);

        Rigidbody rb = entityGO.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        // === QUAN TRỌNG: Chỉ gọi khởi tạo SAU khi đã tạo hết cube ===
        enity.InitializeAfterSpawn();

        //Debug.Log($"[SpawnManager] Spawn thành công Entity: {data.entityName} với {data.pixels.Count} pixel");
    }
    private GameObject PixelCubeBasePrefab
    {
        get
        {
            if (pixelCubeBasePrefab == null)
            {
                Debug.LogError("SpawnManager: Chưa gán PixelCubeBasePrefab!");
            }
            return pixelCubeBasePrefab;
        }
    }
    #endregion
    #region ContextMenu
    [ContextMenu("Spawn One Entity Now")]
    public void SpawnOneEntityNow()
    {
        SpawnOneEntityAsync();
    }
    #endregion
}
