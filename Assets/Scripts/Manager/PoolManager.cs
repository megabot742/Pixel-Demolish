using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolManager : BaseManager<PoolManager>
{
    [Header("PixelCube Pool Settings")]
    [SerializeField] private GameObject pixelCubePrefab; //Prefab gốc
    [SerializeField] private int initialSize = 500; //Số lượng khởi tạo ban đầu
    [SerializeField] private int expandSize = 500; //Số lượng thêm nếu bể đã đầy
    [SerializeField] private Transform pixelCubePoolParent; //Bể cha chứa các PixelCube
    public Transform GetPoolParent() => pixelCubePoolParent;
    private Queue<GameObject> pool = new Queue<GameObject>(); //Hàng đợi FIFO

    protected override void Awake()
    {
        base.Awake();

        //Kiểm tra Prefab gốc
        if (pixelCubePrefab == null)
        {
            Debug.LogError("PoolManager: Chưa gán PixelCube Prefab!");
            return;
        }

        // Tự tạo Pool Parent nếu chưa có
        if (pixelCubePoolParent == null)
        {
            GameObject parentGO = new GameObject("PixelCubePool");
            pixelCubePoolParent = parentGO.transform;
            pixelCubePoolParent.SetParent(transform);
        }

        PrewarmPool();
    }
   
    //Làm mới trạng thái pool
    public void ResetAllPool()
    {
        List<GameObject> allPooledObjects = new List<GameObject>();
        for (int i = pixelCubePoolParent.childCount - 1; i >= 0; i--)
        {
            Transform child = pixelCubePoolParent.GetChild(i);
            if (child.TryGetComponent<PixelCube>(out _)) // chỉ lấy những object thật sự là PixelCube
            {
                allPooledObjects.Add(child.gameObject);
            }
        }
        pool.Clear();
        foreach (var obj in allPooledObjects)
        {
            ResetObjectForPool(obj);
            obj.SetActive(false);
            obj.transform.SetParent(pixelCubePoolParent); // đảm bảo
            pool.Enqueue(obj);
        }
        Debug.Log($"[PoolManager] ForceResetAllPool - Total objects in pool: {pool.Count} " + $"(prewarm: {initialSize}, expanded: {pool.Count - initialSize})");
    }
    //Khởi tạo Pool
    private void PrewarmPool()
    {
        for (int i = 0; i < initialSize; i++) //Kiểm tra số lượng ban đầu
        {
            CreateNewObject();
        }
    }

    //Khởi tạo các phần tử trong Pool
    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(pixelCubePrefab, pixelCubePoolParent);
        obj.name = "PixelCube_Pooled";

        ResetObjectForPool(obj);
        obj.SetActive(false); //Tắt hoạt động
        pool.Enqueue(obj);
        return obj;
    }
    public void EnsureEnoughForSpawn(int requiredCount)
    {
        //Debug.Log($"[PoolManager] Pool: {pool.Count} x Enity Need: {requiredCount}");
        while (pool.Count < requiredCount)
        {
            Debug.LogWarning($"[PoolManager] Pool chỉ còn {pool.Count} < {requiredCount} → bổ sung thêm {expandSize} objects");
            for (int i = 0; i < expandSize; i++)
                CreateNewObject();
        }
    }
    //Lấy phần tử trong Pool
    public GameObject GetPixelCube()
    {
        GameObject obj = pool.Dequeue();
        ResetObjectForPool(obj);
        obj.SetActive(true);
        return obj;
    }
    //Trả về pool
    public void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;

        ResetObjectForPool(obj); //Reset trạng thái
        obj.SetActive(false); //Tắt object
        obj.transform.SetParent(pixelCubePoolParent); //Set vị trí trả về
        pool.Enqueue(obj); //Thêm vào cuối hàng đợi
    }
    //Reset trạng thái các phần tử trong pool
    private void ResetObjectForPool(GameObject obj)
    {
        // Reset vị trí
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        // Reset trạng thái
        if (obj.TryGetComponent(out PixelCube pixelCube))
        {
            //pixelCube.Id = 0;
            pixelCube.ResetDetached();
        }

        // Reset màu
        if (obj.TryGetComponent(out ColorCube colorCube))
        {
            colorCube.CubeColor = Color.white;
        }

        // Xóa Rigidbody2D (vì Detouch mới add)
        if (obj.TryGetComponent(out Rigidbody2D rb))
        {
            Destroy(rb);
        }

        // Reset Collider
        if (obj.TryGetComponent(out Collider2D col))
        {
            col.enabled = true;
        }
    }
}
