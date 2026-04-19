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
    [SerializeField] private EntityDatabaseSO entityDatabase; //database Enity
    [SerializeField] private float spawnCountDown = 7f; //mặc định spawn sau mỗi  7 giây
    [SerializeField] private Transform spawnPoint; //vị trị để spawn
    [SerializeField] private float randomRotationRange = 80f; //Góc quay của spawn ±80

    [SerializeField] private bool canSpawn = true; //Kiểm tra điều kiện spawn
    public bool GetCanSpawn() => canSpawn;
    public void SetCanSpawn(bool value) => canSpawn = value;

    //Coroutine spawn
    private Coroutine spawnCoroutine;
    #region Setup
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
        if (scene.name == "Menu") //SpawnManager ko hoạt động ở Scene Menu
        {
            StopSpawning();
            spawnPoint = null;
            if (ResultManager.HasInstance)
            {
                ResultManager.Instance.SetLevelComplete(false);
            }
        }
        else if (IsLevelScene(scene.name)) //SpawnManager chỉ spawn ở Scene Level
        {
            FindSpawnPoint();
            canSpawn = true;
            if (ResultManager.HasInstance)
            {
                ResultManager.Instance.SetLevelComplete(false);
            }

            StopSpawning();
            StartSpawning();
        }
    }

    private bool IsLevelScene(string sceneName) => sceneName.StartsWith("Level ");

    private void FindSpawnPoint()
    {
        //Tìm kiếm vị trí spawn point
        GameObject spawnObj = GameObject.FindWithTag("SpawnPoint");
        spawnPoint = spawnObj != null ? spawnObj.transform : transform;
    }
    #endregion
    #region Start Spawning
    //Bắt đầu Spawn
    public void StartSpawning()
    {
        if (entityDatabase == null || entityDatabase.entityReferences.Count == 0)
        {
            Debug.LogWarning("SpawnManager: Chưa có entityDatabase!");
            return;
        }

        StopSpawning();
        spawnCoroutine = StartCoroutine(SpawnRoutine()); //Bắt đầu Coroutine
    }
    //Coroutine cho phép spawn
    #region Handler Coroutine 1
    private IEnumerator SpawnRoutine()
    {
        if (ResultManager.HasInstance)
        {
            while (!ResultManager.Instance.GetLevelCompleted()) //Điều kiện hoàn thành level, true = dừng
            {
                if (canSpawn && spawnPoint != null) //Điều kiện spawn và vị trí spawn
                {
                    SpawnOneEntityAsync(); //Xử lí spawn Entity
                    yield return new WaitForSeconds(spawnCountDown); //Thời gian spawn kế tiếp
                }
                else
                {
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
    }
    private void SpawnOneEntityAsync()
    {
        //Kiểm tra database
        if (entityDatabase == null || entityDatabase.entityReferences.Count == 0) return;

        //Random 1 item bất kì trong list database
        int randomIndex = Random.Range(0, entityDatabase.entityReferences.Count);
        var selectedRef = entityDatabase.entityReferences[randomIndex];

        //Load data từ Entitydata từ dưới Addressables lên 
        Addressables.LoadAssetAsync<EntityData>(selectedRef).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                StartCoroutine(CreateEntityWithValidation(handle.Result)); //Bắt đầu Coroutine khởi tạo Enity
            }
            else
            {
                Debug.LogError($"[SpawnManager] Load EntityData thất bại: {selectedRef}");
            }
        };
    }
    #endregion
    #region Handler Coroutine 2
    private IEnumerator CreateEntityWithValidation(EntityData data)
    {
        if (data == null || data.pixels.Count == 0) yield break;

        //Setup điều kiện build của mỗi Enity (biến cục bộ)
        const int maxRetry = 3; //Số vòng lập build tối đa
        int attempt = 0; //Số lần build hiện tại
        bool success = false; //Tình trạng

        if (PoolManager.HasInstance)
            PoolManager.Instance.EnsureEnoughForSpawn(data.pixels.Count);

        while (attempt < maxRetry && !success) //Mỗi Enity chỉ có tối đa 3 lần chạy build, nếu không được sẽ loại bỏ, tránh loop vô tận
        {
            attempt++;

            // Bước 1: Tạo Entity cha trong Pool, thêm Enity.cs
            GameObject entityGO = new GameObject(data.name);
            entityGO.transform.SetParent(PoolManager.Instance.GetPoolParent());
            entityGO.transform.position = spawnPoint.position;

            Enity enity = entityGO.AddComponent<Enity>(); //-> Khi spawn ra Enity.cs sẽ tự động xử lí khối lượng chung, tính mảng Grid 

            // Bước 2: Lọc qua data và tìm vị trị nhỏ nhất để tìm tâm Enity
            Vector3 minPos = Vector3.one * float.MaxValue;
            foreach (var p in data.pixels)
                minPos = Vector3.Min(minPos, p.localPosition);

            //Bước 3: Spawn các Pixel theo vị trí trong database
            for (int i = 0; i < data.pixels.Count; i++)
            {
                //Kiểm tra Pool
                if (PoolManager.HasInstance)
                {
                    PixelInfo pixel = data.pixels[i]; //Thông tin từ pixel

                    GameObject cubeObj = PoolManager.Instance.GetPixelCube();

                    //Đặt làm con của Entity cha
                    cubeObj.transform.SetParent(entityGO.transform, false); //Nơi chứa
                    cubeObj.transform.localPosition = pixel.localPosition - minPos; //Vị trí
                    cubeObj.transform.localRotation = Quaternion.identity; //Góc quay
                    cubeObj.transform.localScale = Vector3.one; //Kích thước

                    // Set màu
                    if (cubeObj.TryGetComponent(out ColorCube colorComp))
                    {
                        colorComp.CubeColor = pixel.color;
                    }
                }

            }
            // Bước 4: Đợi Unity cập nhật hierarchy của Pool
            yield return null;
            yield return new WaitForEndOfFrame();

            int currentChildCount = entityGO.transform.childCount;

            // Bước 5: Kiểm tra đủ số lượng child theo data
            if (currentChildCount == data.pixels.Count)
            {
                if (entityGO.transform.childCount > 0) //Kiểm tra child thực tế của Entity luôn lớn hơn 0 trước khi dịch chuyển 
                {
                    // Tính bounds trong LOCAL space (không dùng world position)
                    Bounds localBounds = new Bounds(entityGO.transform.GetChild(0).localPosition,Vector3.zero);

                    for (int i = 0; i < entityGO.transform.childCount; i++)
                    {
                        localBounds.Encapsulate(entityGO.transform.GetChild(i).localPosition);
                    }

                    Vector3 localCenter = localBounds.center;

                    // Di chuyển tất cả PixelCube child để tâm Enity nằm đúng tại local (0,0,0)
                    Vector3 offset = -localCenter;
                    foreach (Transform child in entityGO.transform)
                    {
                        child.localPosition += offset;
                    }

                    Debug.Log($"[SpawnManager] Centered {data.name} | Local center was {localCenter}");
                }

                // Bước 6: Random góc xoay (bây giờ xoay quanh pivot chính giữa)
                float randomZ = Random.Range(-randomRotationRange, randomRotationRange);
                entityGO.transform.rotation = Quaternion.Euler(0f, 0f, randomZ);

                // Bước 7: Đặt Entity chính xác tại spawnPoint (center sẽ rơi đúng vị trí)
                entityGO.transform.position = spawnPoint.position;

                // Bước 8: Gọi InitializeAfterSpawn() - để Enity tự xử lý Rigibody + mass + CollectCubes
                enity.InitializeAfterSpawn();

                success = true;
                Debug.Log($"[SpawnManager] Spawn {data.name} | Pixels: {data.pixels.Count} | Children: {currentChildCount}");
            }
            else //Bước 5.1: Trường hợp Entity có dấu hiệu lỗi hoặc mất child khi spawn sẽ thử lại 
            {
                Debug.LogWarning($"[SpawnManager] Spawn {data.name} failure (Attempt {attempt}): Expected {data.pixels.Count} but only {currentChildCount} children → Try again...");
                ReturnAllChildrenToPool(entityGO); //Dọn PixelCube của Entity bị lỗi để trả về Pool trước
                Destroy(entityGO); //Dọn dẹp bản thân Entity
                yield return new WaitForSeconds(0.05f); // Đợi một chút để pool update
                yield return new WaitForEndOfFrame();
            }
        }

        if (!success)
        {
            Debug.LogError($"[SpawnManager]: Enity {data.name} has an error, proceed to delete");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPaused = false;
#endif
        }
    }
    //Xử lí dọn dẹp các child của Enity lỗi
    private void ReturnAllChildrenToPool(GameObject entityGO)
    {
        if(entityGO == null) return;

        if (PoolManager.HasInstance) //Kiểm trả Pool
        {
            // Duyệt từ cuối lên để tránh lỗi khi thay đổi hierarchy
            for (int i = entityGO.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = entityGO.transform.GetChild(i);
                if (child.TryGetComponent<PixelCube>(out var pixelCube))
                {
                    PoolManager.Instance.ReturnToPool(child.gameObject);
                }
            }
            Debug.LogWarning("Returned Childs to Enity's Pool in error");
        }

    }
    #endregion
    #endregion
    #region Stop Spawning
    //Dừng spawn
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine); //Dừng Coroutine
            spawnCoroutine = null;
        }
    }
    #endregion
    [ContextMenu("Spawn One Entity Now")]
    public void SpawnOneEntityNow() => SpawnOneEntityAsync();
}
