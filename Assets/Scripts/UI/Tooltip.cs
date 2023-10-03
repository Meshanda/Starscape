using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Tooltip : MonoBehaviour
{
    public TextMeshProUGUI header;
    public TextMeshProUGUI body;
    public LayoutElement layoutElement;
    
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
        var headerLength = header.text.Length;
        var bodyLength = body.text.Length;

        layoutElement.enabled = headerLength > characterWrapLimit || bodyLength > characterWrapLimit;
    }
}
