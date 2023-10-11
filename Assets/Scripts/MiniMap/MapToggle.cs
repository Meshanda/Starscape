using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MapToggle : MonoBehaviour
{
    private Camera _cam;
    [SerializeField]private RenderTexture[] _renderTexture;
    [SerializeField]private float _bigMapOrthoSize = 12.0f;
    private int currentRenderTexture = 0;
    private float lastOrthoSize = 0;
    // Start is called before the first frame update
    void Start()
    {
        _cam = GetComponent<Camera>();
        _cam.targetTexture = _renderTexture[0];
    }

    public void OnMap() 
    {
        ChangeRenderTexture();
    }

    
    public void ChangeRenderTexture() 
    {
        currentRenderTexture = (currentRenderTexture + 1) % _renderTexture.Length;
        _cam.targetTexture = _renderTexture[currentRenderTexture];

        if (currentRenderTexture == _renderTexture.Length-1)
        {
            lastOrthoSize = _cam.orthographicSize;
            _cam.orthographicSize = _bigMapOrthoSize;
        }
        else
        {
            _cam.orthographicSize = lastOrthoSize;
        }
    }
}
