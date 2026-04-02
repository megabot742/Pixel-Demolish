
using DG.Tweening;
using UnityEngine;

public class PixelCube : MonoBehaviour
{
    private bool detached; //default = false;

    public int Id { get; set; }

    [ContextMenu("Detouch Cube")]

    public void DetouchCube()
    {
        if (detached) return; //Check detached

        detached = true;
        Enity parentEntity = GetComponentInParent<Enity>();
        if (parentEntity != null)
        {
            parentEntity.DetouchCube(this);
        }
        GetComponent<ColorCube>().ApplyDetouchColor();

    }
    public void DestroyCube()
    {
        DetouchCube();

        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Collider>().enabled = false;
        transform.DOScale(0, 0.5f).OnComplete(() =>
        {
            if (CoinManager.HasInstance) //Check coin
            {
                CoinManager.Instance.AddCoin(1); // +1 when 1 cube Destroy
            }
            if (ResultManager.HasInstance)
            {
                ResultManager.Instance.AddExp(1);
            }
            Destroy(gameObject);
        });
    }
}
