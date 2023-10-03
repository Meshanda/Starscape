using System;
using Cinemachine;

[Serializable]
public class ItemStack
{
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
}