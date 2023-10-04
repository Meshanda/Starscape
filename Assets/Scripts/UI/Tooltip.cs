using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Tooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI header;
    [SerializeField] private TextMeshProUGUI body;
    [SerializeField] private LayoutElement layoutElement;

    [SerializeField] private RectTransform _rtLayout;
    [SerializeField] private RectTransform _rt;
    
    public int characterWrapLimit;

    private void Update()
    {
        if (Application.isEditor)
            UpdateUISize();
        
        UpdateUIPosition();
    }

    public void UpdateUIPosition()
    {
        var position = Input.mousePosition;
        transform.position = position;
    }

    public void SetText(string newHeader, string newBody = "")
    {
        body.gameObject.SetActive(!string.IsNullOrEmpty(newBody));
        
        body.text = newBody;
        header.text = newHeader;

        UpdateUISize();
    }
    
    private void UpdateUISize()
    {
        _rt.sizeDelta = _rtLayout.sizeDelta;
        var headerLength = header.text.Length;
        var bodyLength = body.text.Length;

        layoutElement.enabled = headerLength > characterWrapLimit || bodyLength > characterWrapLimit;
    }
}
