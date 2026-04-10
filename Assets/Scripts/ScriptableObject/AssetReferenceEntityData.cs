using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class AssetReferenceEntityData : AssetReferenceT<EntityData>
{
    public AssetReferenceEntityData(string guid) : base(guid) { }
}
