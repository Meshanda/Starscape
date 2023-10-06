using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(CinemachineConfiner2D))]
public class PlayerCameraConfiner : MonoBehaviour
{
    private CinemachineConfiner2D _cinemachineConfiner;
    private PolygonCollider2D ConfinerCollider => World.Instance.ConfinerBox;

    private Tilemap GroundTilemap => World.Instance.GroundTilemap;

    private void Awake()
    {
        _cinemachineConfiner = GetComponent<CinemachineConfiner2D>();
    }


    private IEnumerator Start()
    {
        yield return null;
        
        _cinemachineConfiner.m_BoundingShape2D = ConfinerCollider;
        
        float xMin = GroundTilemap.cellBounds.xMin * 0.32f;
        float xMax = GroundTilemap.cellBounds.xMax * 0.32f;
        float yMin = GroundTilemap.cellBounds.yMin * 0.32f;

        SetConfinerBound(xMin, xMax, yMin);
    }


    private void SetConfinerBound(float xMin, float xMax, float yMin)
    {
        ConfinerCollider.transform.position = Vector2.zero;
        
        ConfinerCollider.points = new Vector2[4]
        {
            new Vector2(xMin, 400),
            new Vector2(xMax, 400),
            new Vector2(xMax, yMin),
            new Vector2(xMin, yMin),
        };
        
        _cinemachineConfiner.InvalidateCache();
    }
    
}
