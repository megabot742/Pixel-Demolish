using System.Collections.Generic;
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
        if (currentCamera == null) return;

        bool isTouch = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        bool isMouse = Input.GetMouseButtonDown(0);

        if (!isTouch && !isMouse) return;

        Vector3 inputPos = isMouse ? Input.mousePosition : Input.GetTouch(0).position;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;  // vẫn giữ để an toàn
        }

        // Kiểm tra bổ sung cho touch (rất hiệu quả)
        if (isTouch && EventSystem.current != null)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = inputPos;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            if (results.Count > 0)
            {
                // Có UI element nào bị chạm → bỏ qua
                return;
            }
        }

        Ray ray = currentCamera.ScreenPointToRay(inputPos);

        if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance, pixelCubeLayer))
        {
            if (hit.collider.TryGetComponent(out PixelCube pixelCube))
            {
                pixelCube.DetouchCube();
                pixelCube.transform.DOShakeScale(0.2f, 0.1f);
            }
        }
    }
}
