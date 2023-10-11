using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class UsableItem
{
	protected Player GetPlayer()
	{
		return GameManager.Instance?.player;
	}
	
	protected abstract UsableItem OnConstruction(); // constructor equivalent, reset and initialize everything here. Always return the instance (this)

	protected abstract bool OnUse(ItemStack fromItemStack);
	
	public enum UsableItemType // HARDCODED
	{
		None, // No Action
		Mirror,
	}
	
	private static readonly Dictionary<UsableItemType, UsableItem> UsableItems = new() // HARDCODED
	{
		[UsableItemType.Mirror] = new UsableItemMirror().OnConstruction(),
	};

	public static void ResetAllItems()
	{
		UsableItems.Values.ToList().ForEach(v => v.OnConstruction());
	}

	// returns whether we succeeded in using the item
	public static bool TryUseItem(ItemStack fromItemStack)
	{
		if (!ItemStack.IsValid(fromItemStack))
		{
			return false;
		}

		var item = fromItemStack.GetItem();
		if (item.useType == UsableItemType.None)
		{
			return false;
		}

		return UsableItems[item.useType].OnUse(fromItemStack);
	}
	
	// returns the UsableItem for modification
	public static UsableItem GetUseItem(UsableItemType useType)
	{
		if (useType == UsableItemType.None)
		{
			return null;
		}

		return UsableItems[useType];
	}

	// resets a UsableItem, and returns it after the fact
	public static UsableItem ResetUseItem(UsableItemType useType)
	{
		return GetUseItem(useType)?.OnConstruction();
	}
}
