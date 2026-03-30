using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    [Header("Spawn Zone Settings")]
    [SerializeField] private bool debugLog = false;   // Bật để xem log khi pause/resume

    private int entityCount = 0;
    private SpawnManager spawnManager;

    private void Awake()
    {
        if (SpawnManager.HasInstance)
        {
            spawnManager = SpawnManager.Instance;
        }
        else
        {
            Debug.LogWarning("[SpawnZone] SpawnManager.Instance not found!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (spawnManager == null) return;

        if (IsEntity(other))
        {
            entityCount++;
            if (entityCount == 1)
            {
                spawnManager.StopSpawning();
                if (debugLog) Debug.LogWarning("[SpawnZone] Zone occupied → Stop spawning");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (spawnManager == null) return;

        if (IsEntity(other))
        {
            if (entityCount > 0)
                entityCount--;

            if (entityCount == 0)
            {
                spawnManager.StartSpawning();
                if (debugLog) Debug.LogWarning("[SpawnZone] Zone clear → Resume spawning");
            }
        }
    }

    // Bắt cả Enity (có nhiều cube con) lẫn PixelCube bị tách rời
    private bool IsEntity(Collider other)
    {
        return other.GetComponentInParent<Enity>() != null;
    }
}
