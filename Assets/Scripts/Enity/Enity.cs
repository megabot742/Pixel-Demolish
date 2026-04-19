using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] //tự thêm Rigibody
public class Enity : MonoBehaviour
{
    private Rigidbody2D rigiBD;

    // Grid thông tin để kiểm tra các cube liền kề (Connected Component)
    private int[,] cubesInfoLocation;         // Mảng 2D đại diện cho grid
    private Vector3 cubesInfoStartPosition;  // Vị trí gốc (min position) của grid

    // Mảng lưu trữ tất cả PixelCube con (dùng index thay vì GetComponentsInChildren)
    private PixelCube[] pixelCube;

    private void Awake()
    {
        rigiBD = GetComponent<Rigidbody2D>();
        rigiBD.constraints = RigidbodyConstraints2D.None;
    }

    public void InitializeAfterSpawn() //Được gọi sau khi Entity được spawn từ SpawnManager.cs
    {
        if (rigiBD == null) return;
        rigiBD = GetComponent<Rigidbody2D>();
        rigiBD.mass = transform.childCount > 0 ? transform.childCount : 1;
        rigiBD.isKinematic = false;


        CollectCubes(); //Xây dựng Grid từ các PixelCube con
        RecalculateCubes(); //Kiểm tra lại Grid và tách rời nếu có
    }

    #region CollectCubes
    public void CollectCubes()
    {
        if (transform.childCount == 0)
        {
            Debug.LogWarning($"[Enity] {gameObject.name} không có child nào!");
            return;
        }

        //Bước 1: Tìm vị trí nhỏ nhất (min) và lớn nhất (max) tọa độ của tất cả pixel cube
        Vector3 min = Vector3.one * float.MaxValue; //vị trí thấp, bên trái dưới cùng Entity
        Vector3 max = Vector3.one * float.MinValue; //vị trí cao, bên phải trên cùng Entity
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            min = Vector3.Min(min, child.localPosition);
            max = Vector3.Max(max, child.localPosition);
        }

        // Bước 2: Tính kích thước grid (tìm ra chiều ngang và chiều dọc)
        Vector2Int delta = Vector2Int.RoundToInt(max - min);

        if (delta.x > 30 || delta.y > 30) // Giới hạn kích thước grid để tránh tạo mảng quá lớn, game hiện tại chứa tối đa Entity có Grid 30x30
        {
            Debug.LogError($"[Enity] Grid quá lớn! delta = {delta}");
            delta = new Vector2Int(Mathf.Min(delta.x, 30), Mathf.Min(delta.y, 30));
        }

        cubesInfoLocation = new int[delta.x + 1, delta.y + 1]; // Tạo mảng 2D
        cubesInfoStartPosition = min; // Lưu gốc tọa độ

        // Bước 3: Duyệt lại tất cả cube lần nữa để "đánh dấu" lên grid
        pixelCube = new PixelCube[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)  // Khởi tạo mảng pixelCube theo số lượng child
        {
            Transform child = transform.GetChild(i);

            // Chuyển vị trí local → tọa độ grid (ví dụ: (0,0), (1,0), (0,1)...)
            Vector2Int grid = GridPosition(child.localPosition);

            if (grid.x >= 0 && grid.x < cubesInfoLocation.GetLength(0) && grid.y >= 0 && grid.y < cubesInfoLocation.GetLength(1))
            {
                // Đánh dấu vị trí này thuộc cube thứ i+1
                cubesInfoLocation[grid.x, grid.y] = i + 1; // Lưu index + 1 (0 = trống) 

                // Lưu reference của PixelCube vào mảng
                PixelCube pc = child.GetComponent<PixelCube>();
                if (pc != null)
                {
                    pc.Id = i + 1;  //Lưu ID
                    pixelCube[i] = pc;
                }
            }
        }

        //Debug.Log($"[Enity] {gameObject.name} CollectCubes OK | Children: {transform.childCount}");
    }
    #endregion

    #region RecalculateCubes 
    private void RecalculateCubes() //Xử lí tách khối rời rạc
    {
        if (pixelCube == null || pixelCube.Length == 0)
        {
            CollectCubes();
            if (pixelCube == null || pixelCube.Length == 0) return;
        }

        // Bước 1: Lấy danh sách tất cả cube còn lại (chưa bị destroy)
        List<int> freeCubeIds = new List<int>();
        for (int i = 0; i < pixelCube.Length; i++)
        {
            if (pixelCube[i] != null)
                freeCubeIds.Add(pixelCube[i].Id);
        }

        if (freeCubeIds.Count == 0)
        {
            Destroy(gameObject); //Khi Entity hết pixel thì phá hủy
            return;
        }

        // Bước 2: Tìm tất cả các "nhóm" cube liền kề (Connected Components)
        List<CubeGroup> groups = new List<CubeGroup>();
        while (freeCubeIds.Count > 0)
        {
            CubeGroup newGroup = new CubeGroup();
            groups.Add(newGroup);

            int startID = freeCubeIds[0];
            freeCubeIds.RemoveAt(0);
            newGroup.Cubes.Add(startID);

            FillCube(startID, freeCubeIds, newGroup); // Flood fill để tìm nhóm
        }
        // Bước 3: Nếu chỉ có 1 nhóm → vẫn là một khối liền mạch → không cần làm gì
        if (groups.Count < 2) return;

        // Bước 4: Tách các nhóm thừa ra thành Entity mới
        for (int i = 1; i < groups.Count; i++)
        {
            // Tạo Entity mới
            GameObject newEntity = new GameObject("Entity");
            int firstId = groups[i].Cubes[0] - 1;
            if (firstId < 0 || firstId >= pixelCube.Length || pixelCube[firstId] == null) continue;

            var firstCube = pixelCube[firstId].transform;
            newEntity.transform.SetPositionAndRotation(firstCube.position, firstCube.rotation);
            newEntity.transform.SetParent(PoolManager.Instance.GetPoolParent());

            // Chuyển các pixelcube thuộc nhóm này sang Entity mới
            foreach (int id in groups[i].Cubes)
            {
                int idx = id - 1;
                if (idx >= 0 && idx < pixelCube.Length && pixelCube[idx] != null)
                    pixelCube[idx].transform.parent = newEntity.transform;
            }

            Enity newComp = newEntity.AddComponent<Enity>();
            newComp.InitializeAfterSpawn(); // Quan trọng: Entity mới cũng phải khởi tạo lại
        }

        // Cuối cùng cập nhật lại grid của Entity hiện tại
        CollectCubes();
    }
    #endregion
    #region FillCube
    //Hàm xử lí tìm pixelcube liền kề (trên, dưới, trái, phải)
    private void FillCube(int startID, List<int> freeCubeIds, CubeGroup currentGroup)
    {
        if (startID - 1 < 0 || startID - 1 >= pixelCube.Length || pixelCube[startID - 1] == null)
            return;

        Vector2Int gridPosition = GridPosition(pixelCube[startID - 1].transform.localPosition);

        // Kiểm tra 4 hướng
        CheckNeighbor(Vector2Int.up);     //trên
        CheckNeighbor(Vector2Int.right);  //phải
        CheckNeighbor(Vector2Int.down);   //dưới
        CheckNeighbor(Vector2Int.left);   //trái

        void CheckNeighbor(Vector2Int dir)
        {
            int neighborId = GetNeighbor(gridPosition, dir);
            // Nếu có pixelcube liền kề và cube đó chưa được thêm vào nhóm nào
            if (neighborId > 0 && freeCubeIds.Remove(neighborId))
            {
                currentGroup.Cubes.Add(neighborId);
                FillCube(neighborId, freeCubeIds, currentGroup); // Đệ quy tiếp tục tìm
            }
        }
    }
    #endregion

    private Vector2Int GridPosition(Vector3 localPos) => Vector2Int.RoundToInt(localPos - cubesInfoStartPosition);

    private int GetNeighbor(Vector2Int pos, Vector2Int dir)
    {
        Vector2Int p = pos + dir;
        if (p.x < 0 || p.x >= cubesInfoLocation.GetLength(0) ||
            p.y < 0 || p.y >= cubesInfoLocation.GetLength(1)) return 0;
        return cubesInfoLocation[p.x, p.y];
    }

    #region CleanUp
    public void EntityCleanupAndDestroy() //Gọi xử lí Enity, buộc dọn hết PixielCube để trả về Pool
    {
        if (pixelCube == null || pixelCube.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        // Tắt Rigidbody của Entity để tránh physics tính toán trong lúc dọn
        if (rigiBD != null)
            rigiBD.isKinematic = true;

        // Thả hết tất cả PixelCube về Pool (duyệt ngược để an toàn)
        for (int i = pixelCube.Length - 1; i >= 0; i--)
        {
            if (pixelCube[i] != null)
            {
                // Detouch nhưng KHÔNG gọi RecalculateCubes() trong lúc dọn dẹp hàng loạt, giúp tối ưu hiệu năng
                DetouchCubeInternal(pixelCube[i]);
            }
        }

        // Sau khi thả hết, tự hủy Entity
        Destroy(gameObject);
    }
    private void DetouchCubeInternal(PixelCube cube) //Xử lí nội bộ từng PixelCube trong Entity
    {
        if (cube == null) return;

        // Reset thông tin grid
        Vector2Int grid = GridPosition(cube.transform.localPosition);
        if (grid.x >= 0 && grid.x < cubesInfoLocation.GetLength(0) &&
            grid.y >= 0 && grid.y < cubesInfoLocation.GetLength(1))
        {
            cubesInfoLocation[grid.x, grid.y] = 0;
        }
        if (cubesInfoLocation == null) return;
    
        if (cube.Id > 0 && cube.Id - 1 < pixelCube.Length)
            pixelCube[cube.Id - 1] = null;

        // Thả pixelcube ra Pool (lúc này PixelCube vẫn còn vật lí)
        cube.transform.parent = null;
        if (PoolManager.HasInstance)
            cube.transform.SetParent(PoolManager.Instance.GetPoolParent(), true);

        // Thêm Rigidbody cho pixelcube rơi tự do
        var rb2d = cube.gameObject.GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            rb2d = cube.gameObject.AddComponent<Rigidbody2D>();
        }
            
        rb2d.constraints = RigidbodyConstraints2D.None; //Z axis
        rb2d.isKinematic = false;
    }
    // DetouchCubeFromChild đã được fix ở tin nhắn trước (SetParent về PoolParent)
    public void DetouchCubeFromEntity(PixelCube cube) //Hàm gọi xử lí PixelCube.cs khi được DetouchCube() từ Saw.cs
    {
        if (cube == null) return;

        DetouchCubeInternal(cube);
        RecalculateCubes();   // Chỉ gọi khi cắt bình thường (Saw)
    }
    #endregion
    // OnDrawGizmos và RandomizePositionZ giữ nguyên
    #region Visual Editor Check
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || cubesInfoLocation == null) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        for (int x = 0; x < cubesInfoLocation.GetLength(0); x++)
            for (int y = 0; y < cubesInfoLocation.GetLength(1); y++)
            {
                Vector3 pos = cubesInfoStartPosition + new Vector3(x, y, 0);
                Gizmos.color = cubesInfoLocation[x, y] == 0 ? Color.green : Color.red;
                Gizmos.DrawSphere(pos, cubesInfoLocation[x, y] == 0 ? 0.1f : 0.2f);
            }
    }
    #endregion
}

public class CubeGroup
{
    public List<int> Cubes = new List<int>();
}

