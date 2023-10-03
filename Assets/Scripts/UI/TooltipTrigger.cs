using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string body;
    public string header;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipSystem.Instance.Show(header, body);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Instance.Hide();
    }
}
