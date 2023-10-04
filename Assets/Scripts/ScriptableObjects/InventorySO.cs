using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Inventory")]
public class InventorySO : ScriptableObject
{
    public List<ItemStack> Inventory;
}

