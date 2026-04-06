
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enity : MonoBehaviour
{
    private Rigidbody rigiBD;
    private int[,] cubesInfoLocation;
    private Vector3 cubesInfoStartPosition;
    private PixelCube[] pixelCube;

    private void Awake()
    {
        rigiBD = GetComponent<Rigidbody>();
        //FrezzeRoation 
        rigiBD.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        //Parent Mass = childCount (with each child = 1)
        rigiBD.mass = transform.childCount;

        CollectCubes(); //Collect cube form child
        RecalculateCubes(); //Check cube and ungroup if necessary (after hit saw or gear)
    }
    #region CollectCubes
    public void CollectCubes()
    {
        //Find the bounding box cube child
        Vector3 min = Vector3.one * float.MaxValue;
        Vector3 max = Vector3.one * float.MinValue;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            min = Vector3.Min(min, child.localPosition);
            max = Vector3.Max(max, child.localPosition);
        }
        // Calculate grid size
        Vector2Int delta = Vector2Int.RoundToInt(max - min);
        cubesInfoLocation = new int[delta.x + 1, delta.y + 1];

        //Save start point (0,0) left bottom corner Gird
        cubesInfoStartPosition = min;
        pixelCube = GetComponentsInChildren<PixelCube>();

        //Loop, check and add ID to Grid 
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Vector2Int grid = GridPosition(child.localPosition);

            // ID statr form 1 to n
            cubesInfoLocation[grid.x, grid.y] = i + 1;

            // Assign it back to Cube for easy reference later
            pixelCube[i].Id = i + 1;
        }
    }
    #endregion
    #region RecalculateCubes
    private void RecalculateCubes()
    {
        // Collect all existing cube ID
        List<int> freeCubeIds = new List<int>();
        for (int i = 0; i < pixelCube.Length; i++)
        {
            if (pixelCube[i] != null)
            {
                freeCubeIds.Add(pixelCube[i].Id);
            }
        }
        // If there are no more cubes -> Destroy Enity
        if (freeCubeIds.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        // List of cube groups after detached
        List<CubeGroup> groups = new List<CubeGroup>();
        int currentGroup = 0;

        //Loop find all 4-way bond groups
        while (freeCubeIds.Count > 0)
        {
            CubeGroup newGroups = new CubeGroup();
            groups.Add(newGroups);

            int startID = freeCubeIds[0];
            freeCubeIds.Remove(startID);
            newGroups.Cubes.Add(startID);

            //Call for pass new list to edit
            FillCube(startID, freeCubeIds, newGroups);

            currentGroup++;
        }
        //If there is only 1 group → still solid, no need to separate
        if (groups.Count < 2)
            return;


        // Split into many new Entities (from the 2nd group onwards)
        for (int i = 1; i < groups.Count; i++)
        {
            GameObject newEntity = new GameObject("Entity");
            var firstCube = pixelCube[groups[i].Cubes[0] - 1].transform;

            // Set the same position & rotation as the first cube in the group
            newEntity.transform.SetPositionAndRotation(firstCube.position, firstCube.rotation);

            // Make all cubes in this group children of the new Entity
            foreach (int id in groups[i].Cubes)
            {
                pixelCube[id - 1].transform.parent = newEntity.transform;
            }

            // Add Entity component → it will automatically Awake() and CollectCubes() again
            newEntity.AddComponent<Enity>();
        }

        // After splitting, update the mesh for the current Entity
        CollectCubes();
    }
    private void FillCube(int startID, List<int> freeCubeIds, CubeGroup currentGroup)
    {
        Vector2Int gridPosition = GridPosition(pixelCube[startID - 1].transform.localPosition);

        CheckNeighbor(Vector2Int.up);
        CheckNeighbor(Vector2Int.right);
        CheckNeighbor(Vector2Int.down);
        CheckNeighbor(Vector2Int.left);

        void CheckNeighbor(Vector2Int direction)
        {
            int neighborId = GetNeighbor(gridPosition, direction);
            if (neighborId > 0 && freeCubeIds.Remove(neighborId))
            {
                currentGroup.Cubes.Add(neighborId);
                FillCube(neighborId, freeCubeIds, currentGroup);  // đệ quy
            }
        }
    }
    #endregion
    #region DetouchCube
    //Cube is split
    public void DetouchCube(PixelCube cube)
    {
        Vector2Int grid = GridPosition(cube.transform.localPosition);

        // Remove from grid
        cubesInfoLocation[grid.x, grid.y] = 0;

        // Delete reference
        pixelCube[cube.Id - 1] = null;

        // Detached from parent (current Entity)
        cube.transform.parent = null;

        // Add Rigidbody for individual cube (so it falls independently)
        var rb = cube.gameObject.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionZ;

        // Recheck the entire structure
        RecalculateCubes();
    }
    // Switch localPosition → grid coordinates (grid)
    private Vector2Int GridPosition(Vector3 localPosition)
    {
        return Vector2Int.RoundToInt(localPosition - cubesInfoStartPosition);
    }
    // Get the ID of the adjacent cube by direction
    private int GetNeighbor(Vector2Int position, Vector2Int direction)
    {
        Vector2Int gridPosition = position + direction;
        // Check the boundary
        if (gridPosition.x < 0 || gridPosition.x >= cubesInfoLocation.GetLength(0) ||
            gridPosition.y < 0 || gridPosition.y >= cubesInfoLocation.GetLength(1))
            return 0;

        return cubesInfoLocation[gridPosition.x, gridPosition.y];
    }
    public void ForceDisassembleAllCubes()
    {
        if (pixelCube == null || pixelCube.Length == 0)
            return;

        // Tách từng cube còn lại ra
        for (int i = 0; i < pixelCube.Length; i++)
        {
            if (pixelCube[i] != null)
            {
                PixelCube cube = pixelCube[i];
                if (cube != null)
                {
                    // Gọi DetouchCube để tách ra và thêm Rigidbody riêng
                    DetouchCube(cube);
                }
            }
        }

        // Sau khi tách hết, Enity sẽ tự Destroy() trong RecalculateCubes()
    }
    #endregion

    #region Visual Editor Check
    //Draw mesh in scene View
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.matrix = transform.localToWorldMatrix;
        for (int x = 0; x < cubesInfoLocation.GetLength(0); x++)
        {
            for (int y = 0; y < cubesInfoLocation.GetLength(1); y++)
            {
                Vector3 position = cubesInfoStartPosition + new Vector3(x, y, 0);
                if (cubesInfoLocation[x, y] == 0)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(position, 0.1f); //Green = Empty
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(position, 0.2f); //Red = Have cube
                }

            }
        }
    }
    //Small random z to create a "concave" effect
    [ContextMenu("Randomize Position Z")]
    private void RandomizePositionZ()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Vector3 localPosition = child.localPosition;
            localPosition.z = Random.Range(-0.15f, 0.15f);
            child.localPosition = localPosition;
        }
    }
    #endregion
}
