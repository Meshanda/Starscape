using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipSystem : Singleton<TooltipSystem>
{
    public Tooltip tooltip;

    public void Show(string newHeader, string newBody = "")
    {
        tooltip.UpdateUIPosition();
        tooltip.SetText(newHeader, newBody);
        tooltip.gameObject.SetActive(true);
    }

    public void Hide()
    {
        tooltip.gameObject.SetActive(false);
    }
}
