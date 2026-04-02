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
        return sceneName.StartsWith("Level ");
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
    public void StartSpawning()
    {
        if (entityPrefabs.Count == 0) return;

        StopSpawning();// Đảm bảo không chạy 2 coroutine
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    public void SetCanSpawn(bool value)    // <-- HÀM MỚI: SpawnZone sẽ gọi cái này
    {
        canSpawn = value;
    }
    public void CompleteLevel()
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
                SpawnOneEntity();
                yield return new WaitForSeconds(spawnCountDown);   // Đếm đúng 7s
            }
            else
            {
                // Khu vực bị chiếm → tạm dừng đếm, nhưng không reset timer
                yield return new WaitForSeconds(0.2f);   // Kiểm tra mỗi 0.2s (nhẹ)
            }
        }
    }

    private void SpawnOneEntity()
    {
        //Random Enity Prefab
        int randomIndex = Random.Range(0, entityPrefabs.Count);
        GameObject prefabToSpawn = entityPrefabs[randomIndex];

        //Khởi tạo Spawn
        GameObject newEntity = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity, spawnPoint);

        //Random trục Z 
        float randomZ = Random.Range(-randomRotationRange, randomRotationRange);
        newEntity.transform.rotation = Quaternion.Euler(0f, 0f, randomZ);

        //Check Entity Rigidbody
        Rigidbody rb = newEntity.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; //áp vật lí để rơi
        }

        //Debug.Log($"Spawn Entity: {prefabToSpawn.name} | Z Rotation: {randomZ:F1}°");
    }
    #endregion
    #region ContextMenu
    [ContextMenu("Spawn One Entity Now")]
    public void SpawnOneEntityNow()
    {
        SpawnOneEntity();
    }
    #endregion
}
