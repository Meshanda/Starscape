using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveManager : MonoBehaviour
{
    [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _backgroundTilemap;

    [SerializeField] private TileBase _tile;
    [SerializeField] private TileBase _tileGround;

    [SerializeField] private int _padding;

    [SerializeField] private List<Possibility> _possibilities;

    [SerializeField] private float _scale = 1f;
    [SerializeField] private float _threshold = 0.5f;
    [SerializeField] private Vector2 _offset = new Vector2(0, 0);

    private CellCave[,] _cells;

    private Vector3Int _dimensions;

    private System.Random _random;

    private void Start()
    {
        _random = new System.Random(1234);

        _dimensions = _groundTilemap.size;

        InitCells();

        // Collapse();w
    }

    private void Update()
    {
        for (int x = 0; x < _dimensions.x; x++)
        {
            for (int y = 0; y < _dimensions.y; y++)
            {
                PerlinNoseCave(x, y);
            }
        }
    }

    private void InitCells()
    {
        /*_cells = new CellCave[_dimensions.x, _dimensions.y];
        for (int x = 0; x < _dimensions.x; x++)
        {
            for (int y = 0; y < _dimensions.y; y++)
            {
                _cells[x, y] = new CellCave(_possibilities);
            }
        }

        _holes = new int[_dimensions.x, _dimensions.y];

        */
    }

    private void PerlinNoseCave(int x, int y)
    {
        float coodx = (float)x / _dimensions.x * _scale;
        float coody = (float)y / _dimensions.y * _scale;

        float value = Mathf.PerlinNoise(coodx + _offset.x, coody + _offset.y);
        //float value = noise.snoise( new float2(coodx + _offset.x, coody + _offset.y));
        // float2 value = noise.cellular( new float2(coodx + _offset.x, coody + _offset.y));

        if(value > _threshold)
        {
            _groundTilemap.SetTile(new Vector3Int(x, y, 0), _tile);
        }
        else
        {
            _groundTilemap.SetTile(new Vector3Int(x, y, 0), _tileGround);          
        }
    }

    private void Collapse()
    {
        /*int rnd1 = _random.Next(_padding, _dimensions.x - _padding);
        int rnd2 = _random.Next(_padding, _dimensions.y - _padding);
        */
        // _cells[rnd1, rnd2].Collapsed();
    }

}
