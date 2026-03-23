using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Setting")]
    [SerializeField] private List<GameObject> entityPrefabs = new List<GameObject>();
    [SerializeField] private float spawnCountDown = 7f; //default 3 second
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float randomRotationRange = 80f; // ±30 độ là đẹp nhất

    private void Start()
    {
        if (entityPrefabs.Count == 0)
        {
            Debug.LogError("SpawnManager: Not Enity Prefab");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("SpawnManager: Unassigned spawnPoint will use SpawnManager's location instead");
            spawnPoint = transform;
        }

        //Spawn
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnOneEntity();
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
