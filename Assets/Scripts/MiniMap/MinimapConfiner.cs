using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MinimapConfiner : MonoBehaviour
{
    [SerializeField] private Transform _playerFollow;
    private Tilemap _GroundMap => World.Instance.GroundTilemap;
    private Camera _minimapCamera;
    private float _orthographicSize;

    private float _lastXPos;
    private float _lastYPos;

    private float xMin;
    private float xMax;
    private float yMin;

    private void Awake()
    {
        _minimapCamera = GetComponent<Camera>();
        _orthographicSize = _minimapCamera.orthographicSize;

    }

    private IEnumerator Start()
    {
        yield return null;
        InitBounds();
    }


    private void Update()
    {
        if (((_playerFollow.position.x - _orthographicSize) <= xMin) || ((_playerFollow.position.x + _orthographicSize) >= xMax))
        {
            var currentPos = transform.position;
            currentPos.x = _lastXPos;
            transform.SetPositionAndRotation(currentPos, transform.rotation);
        }
        else
        {
            var currentPos = transform.localPosition;
            currentPos.x = 0;
            transform.SetLocalPositionAndRotation(currentPos, transform.localRotation);
            
            _lastXPos = transform.position.x;
        }

        if ((_playerFollow.position.y - _orthographicSize) < yMin)
        {
            var currentPos = transform.position;
            currentPos.y = _lastYPos;
            transform.SetPositionAndRotation(currentPos, transform.rotation);
        }
        else
        {
            var currentPos = transform.localPosition;
            currentPos.y = 0;
            transform.SetLocalPositionAndRotation(currentPos, transform.localRotation);
            
            _lastYPos = transform.position.y;
        }
    }

    void InitBounds()
    {
        xMin = _GroundMap.cellBounds.xMin * 0.32f;
        xMax = _GroundMap.cellBounds.xMax * 0.32f;
        yMin = _GroundMap.cellBounds.yMin * 0.32f;
    }
}
