
using DG.Tweening;
using UnityEngine;

public class PixelCube : MonoBehaviour
{
    private bool detached; //default = false;

    public int Id{ get; set;}

    [ContextMenu("Detouch Cube")]

    public void DetouchCube()
    {
        if(detached) return; //Check detached

        detached = true;
        GetComponentInParent<Enity>().DetouchCube(this);
        GetComponent<ColorCube>().ApplyDetouchColor();
        
    }
    public void DestroyCube()
    {
        DetouchCube();

        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Collider>().enabled = false;
        transform.DOScale(0, 0.5f).OnComplete(() => Destroy(gameObject));
    }
}
