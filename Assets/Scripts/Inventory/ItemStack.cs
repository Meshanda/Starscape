using System;
using UnityEngine;

[Serializable]
public class ItemStack
{
    [HideInInspector] public string inspectorName;
    
    public string itemID;
    public int number;

    public Item GetItem()
    {
        return GameManager.Instance.database.GetItemById(itemID);
    }

    public string ItemName => GetItem().name;
    public string ItemDescription => GetItem().description;

    public ItemStack Clone()
    {
        return new ItemStack
        {
            itemID = itemID,
            number = number
        };
    }

    public void Add(int n)
    {
        number += n;
    }

    public static bool IsValid(ItemStack stack)
    {
        return stack?.HasAnyItem() ?? false;
    }
    
    public static bool IsValidEditor(ItemStack stack, DatabaseSO database)
    {
        return stack?.HasAnyItemEditor(database) ?? false;
    }

    public bool HasAnyItem()
    {
        return GetItem() != null && number > 0;
    }
    
    public bool HasAnyItemEditor(DatabaseSO database)
    {
        return database.GetItemById(itemID) != null && number > 0;
    }
    
    public void OnValidate(DatabaseSO database)
    {
        if (ItemStack.IsValidEditor(this, database))
        {
            inspectorName = itemID + " (x" + number + ")";
        }
        else
        {
            inspectorName = "<invalid>";
        }
    }
}