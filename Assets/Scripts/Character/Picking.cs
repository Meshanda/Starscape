using UnityEngine;

public class Picking : MonoBehaviour
{
    [SerializeField] private float _pickUpDistance = 2.0f;
    
    private Vector2 _mousePosition => _cam.ScreenToWorldPoint(Input.mousePosition);
    private float _distanceFromPlayer => Mathf.Abs(Vector2.Distance(transform.position, _mousePosition));
    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void OnPickUp()
    {
        if (_distanceFromPlayer > _pickUpDistance)
            return;


        var cellPos = World.Instance.GroundTilemap.WorldToCell(_cam.ScreenToWorldPoint(Input.mousePosition));
        var tile = World.Instance.GroundTilemap.GetTile(cellPos);

        // _tile = tile;
        
        // TODO: Stocker la tile.
    }
}
