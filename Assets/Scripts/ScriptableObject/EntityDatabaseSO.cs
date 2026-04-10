using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


[CreateAssetMenu(menuName = "Enity Data/Entity Database", fileName = "EntityDatabase")]
public class EntityDatabaseSO : ScriptableObject
{
    [Header("List Enity")]
    public List<AssetReferenceEntityData> entityReferences = new List<AssetReferenceEntityData>();
}
