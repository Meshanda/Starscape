using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Inventory")]
public class InventorySO : ScriptableObject
{
    public List<ItemStack> Inventory;
}

[Serializable]
public class ItemStack
{
    public int itemId;
    public int number;
    public Vector2Int position;
}