using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PasteGrotte : MonoBehaviour
{
    [SerializeField] private Tilemap _toPastePrefab;
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private Transform _parent;
    [SerializeField] private WaveFunction _waveFunction;
    private Vector3Int _offset = new Vector3Int(-25,-25);
    private List<Vector3Int> _toCollapse = new List<Vector3Int>();
    private Cell[,] _Cells ;
    
    // Start is called before the first frame update
    void Start()
    {
        Copy();
        _waveFunction.InitializeGrid(_Cells);
    }

    public void Copy() 
    {
        _Cells = new Cell[_toPastePrefab.size.x, _toPastePrefab.size.y];
        for(int i = _toPastePrefab.cellBounds.min.x; i < _toPastePrefab.cellBounds.max.x; i++) 
        {
            for (int j = _toPastePrefab.cellBounds.min.y; j < _toPastePrefab.cellBounds.max.y; j++)
            {
                TileBase tile = _toPastePrefab.GetTile(new Vector3Int(i, j));
                _Cells[i  -_toPastePrefab.cellBounds.min.x, j - _toPastePrefab.cellBounds.min.y] = new Cell(new Vector3Int(i, j) + _offset, (tile == null));
                if (tile != null) 
                {
                    _toCollapse.Add(new Vector3Int(i, j));
                    _tilemap.SetTile(new Vector3Int(i, j) + _offset, null);
                }
                //
                //TileData data = new TileData();
                //tile.GetTileData(new Vector3Int(j, i), go, ref data);
                //if (tile != null && data.sprite != null)
                //{
                //    _tilemap.SetTile(new Vector3Int(j, i), tile);
            }
        }  
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
