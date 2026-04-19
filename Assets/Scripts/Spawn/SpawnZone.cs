using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    [Header("Spawn Zone Settings")]
    [SerializeField] private float requiredStayTime = 3f;
    [SerializeField] private bool debugLog = false;

    [Header("Overlap Settings")]
    [SerializeField] private Vector2 zoneSize = new Vector2(30f, 40f);   // Điều chỉnh kích thước vùng
    [SerializeField] private LayerMask pixelCubeLayer = ~0;

    private float stayTimer = 0f;
    private bool isBlocked = false;

    private Collider2D[] results = new Collider2D[128]; // Cache để tránh alloc

    private void FixedUpdate()   // Dùng FixedUpdate cho Physics
    {
        if (ResultManager.HasInstance && ResultManager.Instance.GetLevelCompleted())
        {
            SetSpawnState(false);
            return;
        }

        // Kiểm tra có PixelCube nào trong vùng không
        int count = Physics2D.OverlapBoxNonAlloc(transform.position, zoneSize, 0f, results, pixelCubeLayer);

        bool hasPixelCube = count > 0;

        if (hasPixelCube)
        {
            stayTimer += Time.fixedDeltaTime;

            if (stayTimer >= requiredStayTime && !isBlocked)
            {
                SetSpawnState(false);
                if (debugLog) Debug.LogWarning($"[SpawnZone] Blocked: {count} PixelCubes stayed too long");
            }
        }
        else
        {
            stayTimer = 0f;
            if (isBlocked)
            {
                SetSpawnState(true);
                if (debugLog) Debug.LogWarning("[SpawnZone] Clear → RESUME spawning");
            }
        }
    }

    private void SetSpawnState(bool canSpawn)
    {
        if (SpawnManager.HasInstance && SpawnManager.Instance.GetCanSpawn() != canSpawn)
        {
            SpawnManager.Instance.SetCanSpawn(canSpawn);
            isBlocked = !canSpawn;
        }
    }

    // Vẫn giữ để ResultManager gọi khi hoàn thành level
    public void ForceReset()
    {
        stayTimer = 0f;
        isBlocked = false;
        if (SpawnManager.HasInstance)
            SpawnManager.Instance.SetCanSpawn(true);

        if (debugLog) Debug.LogWarning("[SpawnZone] Force reset");
    }

    // Debug visual trong Scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, zoneSize);
    }
}
