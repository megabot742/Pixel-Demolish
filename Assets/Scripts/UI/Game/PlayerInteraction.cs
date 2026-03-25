using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Click Settings")]
    [SerializeField] private LayerMask pixelCubeLayer = ~0;   // Layer của PixelCube (mặc định Everything)
    [SerializeField] private float maxClickDistance = 100f;

    private Camera currentCamera;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindCurrentCamera();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindCurrentCamera();
    }

    private void FindCurrentCamera()
    {
        currentCamera = Camera.main;
        
        if (currentCamera == null)
            Debug.LogWarning("[PlayerInteraction] Không tìm thấy Main Camera trong scene hiện tại!");
    }

    private void Update()
    {
        // Nếu chưa có camera thì không làm gì
        if (currentCamera == null) 
            return;

        // Kiểm tra click (chuột hoặc touch)
        if (Input.GetMouseButtonDown(0) || 
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector3 inputPos = Input.GetMouseButtonDown(0) 
                ? Input.mousePosition 
                : Input.GetTouch(0).position;

            // Bỏ qua nếu click vào UI (nút Build, HUD, ResultPanel...)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = currentCamera.ScreenPointToRay(inputPos);
            
            if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance, pixelCubeLayer))
            {
                if (hit.collider.TryGetComponent(out PixelCube pixelCube))
                {
                    pixelCube.DetouchCube();        // Phá giống Saw

                    // Tùy chọn: thêm hiệu ứng nhỏ
                    // pixelCube.transform.DOShakeScale(0.2f, 0.1f);

                    Debug.Log($"[Player Click] Phá PixelCube tại {hit.transform.name}");
                }
            }
        }
    }
}
