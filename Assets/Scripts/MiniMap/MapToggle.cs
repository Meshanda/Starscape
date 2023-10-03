using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MapToggle : MonoBehaviour
{
    private Camera _cam;
    [SerializeField]private RenderTexture[] _renderTexture;
    private int currentRenderTexture = 0;
    // Start is called before the first frame update
    void Start()
    {
        _cam = GetComponent<Camera>();
        _cam.targetTexture = _renderTexture[0];
    }

    public void OnMap() 
    {
//        Debug.Log("aaaa");
        ChangeRenderTexture();
    }

    public void ChangeRenderTexture() 
    {
        currentRenderTexture = (currentRenderTexture + 1) % _renderTexture.Length;
        _cam.targetTexture = _renderTexture[currentRenderTexture];
    }
}
