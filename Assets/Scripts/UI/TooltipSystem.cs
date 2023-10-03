public class TooltipSystem : Singleton<TooltipSystem>
{
    public Tooltip tooltip;
    private bool _active = true;

    public void Show(string newHeader, string newBody = "")
    {
        if (!_active) return;

        tooltip.UpdateUIPosition();
        tooltip.SetText(newHeader, newBody);
        tooltip.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (!_active) return;
        
        tooltip.gameObject.SetActive(false);
    }

    public void SetActive(bool status)
    {
        _active = status;

        if (!_active)
            tooltip.gameObject.SetActive(false);
    }
}
