using System;

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
        return new ItemStack()
        {
            itemID = this.itemID,
            number = this.number,
        };
    }

    public void Add(int n)
    {
        number += n;
    }
}