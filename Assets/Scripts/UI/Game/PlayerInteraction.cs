using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Click Settings")]
    [SerializeField] private LayerMask pixelCubeLayer = ~0;   // Layer of PixelCube (default Everything)
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
            Debug.LogWarning("[PlayerInteraction] Main Camera not found in current scene!");
    }

    private void Update()
    {
        // Check Camera
        if (currentCamera == null) 
            return;

        // Check click
        if (Input.GetMouseButtonDown(0) || 
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector3 inputPos = Input.GetMouseButtonDown(0) 
                ? Input.mousePosition 
                : Input.GetTouch(0).position;

            // Skip if clicking on UI (Build button, HUD, ResultPanel...)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = currentCamera.ScreenPointToRay(inputPos);
            
            if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance, pixelCubeLayer))
            {
                if (hit.collider.TryGetComponent(out PixelCube pixelCube))
                {
                    pixelCube.DetouchCube();
                    //Animation
                    pixelCube.transform.DOShakeScale(0.2f, 0.1f);
                    //Debug.Log($"[Player Click] Break PixelCube at {hit.transform.name}");
                }
            }
        }
    }
}
