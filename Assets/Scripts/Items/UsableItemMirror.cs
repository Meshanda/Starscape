using System.Collections;
using UnityEngine;

public class UsableItemMirror : UsableItem
{
    protected override UsableItem OnConstruction()
    {
        return this;
    }

    protected override bool OnUse(ItemStack fromItemStack)
    {
        if (!GameManager.Instance?.rocketTeleportSocket || !GetPlayer() || InventorySystem.Instance.IsInventoryOpen)
        {
            return false;
        }

        var playerTransform = GetPlayer().transform;
        var newPos = GameManager.Instance.rocketTeleportSocket.position;
        newPos.z = playerTransform.position.z;
        playerTransform.position = newPos;
        return true;
    }
}
