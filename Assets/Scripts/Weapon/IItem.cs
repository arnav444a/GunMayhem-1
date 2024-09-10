using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
[ExecuteInEditMode]
public class IItem : MonoBehaviour
{
    public string itemName;
    public ItemType itemType;

    [SerializeReference, Core.SerializableClass]
    public ItemUsage usage;

    public bool Set(PlayerStateMachine entity, out ItemUsage itemUsage)
    {
        usage.InitializeItem(this);
        return usage.SetItem(entity, out itemUsage);
    }
}

[System.Serializable]
public abstract class ItemUsage
{
    public IItem CurrentItem { set; get; }

    public virtual void UseItem() { }
    public virtual void InitializeItem(IItem item) { }
    public virtual bool SetItem(PlayerStateMachine entity, out ItemUsage usage)
    {
        usage = null;
        return false;
    }
    public ItemUsage CreateInstance(IItem currentItem)
    {
        var usage = MemberwiseClone() as ItemUsage;
        usage.CurrentItem = currentItem;
        return usage;
    }
}
public enum ItemType
{
    Consumable,
    Throwable,
    Gun
}
