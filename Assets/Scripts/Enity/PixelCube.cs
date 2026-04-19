
using DG.Tweening;
using UnityEngine;

public class PixelCube : MonoBehaviour
{
    private bool detached; //default = false;

    public void ResetDetached() => detached = false; 
    public int Id { get; set; } //ID cube

    public void DetouchCube()
    {
        if (detached) return; //Kiểm tra trạng thái tách hiện tại
        detached = true; //Áp trạng thái để không còn bị gọi lại
        
        Enity parentEntity = GetComponentInParent<Enity>(); 
        //Kiểm tra Enity cha
        if (parentEntity != null)
        {
            parentEntity.DetouchCubeFromEntity(this); //Xử lí tách
        }
        GetComponent<ColorCube>().ApplyDetouchColor(); //Áp lại màu sau tách (màu sau tách sẽ đậm hơn)

    }
    public void DestroyCube()
    {
        DetouchCube(); //Gọi xử lí tách

        if (TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true; //Bật lại isKinemetic, tránh lỗi va chạm khi đang trả về Pool
        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false; //Tắt Collider

        //Animation Dotween phá hủy
        var tween = transform.DOScale(0, 0.5f).OnComplete(() =>
        {
            if (this == null || gameObject == null) return;
            
            if (CoinManager.HasInstance) CoinManager.Instance.AddCoin(1);
            if (ResultManager.HasInstance) ResultManager.Instance.AddExp(1);

            //Check pool để gửi trả về
            if (PoolManager.HasInstance)
                PoolManager.Instance.ReturnToPool(gameObject);
            else
                Destroy(gameObject); //fall back
        });
        
    }
}
