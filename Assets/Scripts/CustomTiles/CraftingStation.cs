using UnityEngine;

public class CraftingStation : MonoBehaviour
{
	public float distance = 1.0f;
	public CraftingFlags craftingFlags;

	private bool wasInRange = false;
	
	private void Update()
	{
		bool isInRange = Vector3.Distance(GameManager.Instance.player.transform.position, transform.position) < distance;
		if (isInRange)
		{
			InventorySystem.Instance.CraftingFlags |= craftingFlags;
		}
		else if (wasInRange)
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
