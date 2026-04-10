
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
        //rigiBD.mass = transform.childCount;

        // CollectCubes(); //Collect cube form child
        // RecalculateCubes(); //Check cube and ungroup if necessary (after hit saw or gear)
    }
    public void InitializeAfterSpawn()
    {
        // Set mass dựa trên số child thực tế
        if (rigiBD == null)
            rigiBD = GetComponent<Rigidbody>();
        rigiBD.mass = transform.childCount > 0 ? transform.childCount : 1;

        CollectCubes();        // Lần này mới có child
        RecalculateCubes();    // Kiểm tra group nếu cần
    }
    #region CollectCubes
    public void CollectCubes()
    {
        if (transform.childCount == 0)
        {
            Debug.LogWarning($"[Enity] {gameObject.name} không có child nào!");
            return;
        }

        // Tìm min và max
        Vector3 min = Vector3.one * float.MaxValue;
        Vector3 max = Vector3.one * float.MinValue;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            min = Vector3.Min(min, child.localPosition);
            max = Vector3.Max(max, child.localPosition);
        }

        // === SỬA Ở ĐÂY: Giới hạn kích thước grid để tránh overflow ===
        Vector2Int delta = Vector2Int.RoundToInt(max - min);

        // Bảo vệ an toàn
        if (delta.x > 100 || delta.y > 100)
        {
            Debug.LogError($"[Enity] Grid quá lớn! delta = {delta}. Kiểm tra vị trí pixel.");
            delta = new Vector2Int(Mathf.Min(delta.x, 100), Mathf.Min(delta.y, 100));
        }

        cubesInfoLocation = new int[delta.x + 1, delta.y + 1];

        cubesInfoStartPosition = min;
        pixelCube = GetComponentsInChildren<PixelCube>();

        // Gán ID
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Vector2Int grid = GridPosition(child.localPosition);

            // Bảo vệ index không vượt mảng
            if (grid.x >= 0 && grid.x < cubesInfoLocation.GetLength(0) &&
                grid.y >= 0 && grid.y < cubesInfoLocation.GetLength(1))
            {
                cubesInfoLocation[grid.x, grid.y] = i + 1;
                if (i < pixelCube.Length)
                    pixelCube[i].Id = i + 1;
            }
        }

        //Debug.Log($"[Enity] {gameObject.name} CollectCubes thành công | Grid: {delta.x + 1} x {delta.y + 1}");
    }
    #endregion
    #region RecalculateCubes
    private void RecalculateCubes()
    {
        // Bảo vệ nếu pixelCube chưa được khởi tạo
        if (pixelCube == null || pixelCube.Length == 0)
        {
            // Nếu vừa mới tạo từ data, ta sẽ gọi CollectCubes() trước
            CollectCubes();
            if (pixelCube == null || pixelCube.Length == 0)
                return;
        }

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

        // Update lại sau khi split
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
    #region DetouchCubeFromChild
    //Cube is split
    public void DetouchCubeFromChild(PixelCube cube)
    {
        if (cube == null) return;

        // Bảo vệ trường hợp Enity chưa được khởi tạo đầy đủ
        if (cubesInfoLocation == null || pixelCube == null)
        {
            InitializeAfterSpawn();
            if (pixelCube == null) return;
        }

        Vector2Int grid = GridPosition(cube.transform.localPosition);

        // Remove from grid
        if (grid.x >= 0 && grid.x < cubesInfoLocation.GetLength(0) &&
            grid.y >= 0 && grid.y < cubesInfoLocation.GetLength(1))
        {
            cubesInfoLocation[grid.x, grid.y] = 0;
        }

        // Xóa reference an toàn
        if (cube.Id > 0 && cube.Id - 1 < pixelCube.Length)
        {
            pixelCube[cube.Id - 1] = null;
        }

        // Tách ra khỏi parent
        cube.transform.parent = null;

        // Thêm Rigidbody riêng cho cube rơi tự do
        var rb = cube.gameObject.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionZ;

        // Kiểm tra lại cấu trúc sau khi tách
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
                    DetouchCubeFromChild(cube);
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
        if (!Application.isPlaying) return;
        if (cubesInfoLocation == null) return;

        Gizmos.matrix = transform.localToWorldMatrix;

        for (int x = 0; x < cubesInfoLocation.GetLength(0); x++)
        {
            for (int y = 0; y < cubesInfoLocation.GetLength(1); y++)
            {
                Vector3 position = cubesInfoStartPosition + new Vector3(x, y, 0);

                if (cubesInfoLocation[x, y] == 0)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(position, 0.1f);
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(position, 0.2f);
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
