using System;
using UnityEngine;

public class CraftingTable : MonoBehaviour
{
	public float distance = 1.0f;
	
	private void Update()
	{
		InventorySystem.Instance.SetCraftingActive(Vector3.Distance(GameManager.Instance.player.transform.position, transform.position) < distance);
	}

	private void OnDestroy()
	{
		InventorySystem.Instance.SetCraftingActive(false);
	}
}
