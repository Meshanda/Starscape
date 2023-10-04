using UnityEngine;

public class CraftingTable : MonoBehaviour
{
	public float distance = 1.0f;
	public CraftingFlags craftingFlags;

	private bool wasInRange = false;
	
	private void Update()
	{
		bool isInRange = Vector3.Distance(GameManager.Instance.player.transform.position, transform.position) < distance;
		if (isInRange && !wasInRange)
		{
			InventorySystem.Instance.CraftingFlags |= craftingFlags;
		}
		else if (!isInRange && wasInRange)
		{
			InventorySystem.Instance.CraftingFlags &= ~craftingFlags;
		}

		wasInRange = isInRange;
	}

	private void OnDestroy()
	{
		InventorySystem.Instance.CraftingFlags &= ~craftingFlags;
	}
}
