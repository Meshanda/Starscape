using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PasteGrotte : MonoBehaviour
{
    [SerializeField] private Tilemap[] _toPastePrefab;
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private Transform _parent;
    [SerializeField] private WaveFunction _waveFunction;
    [SerializeField] private float _minRange;
    [SerializeField] private Vector4 _bounds;
    [SerializeField] private GameObject _loadingScreen;
    private Vector3Int _offset = new Vector3Int(-25,-25);
    private List<Vector3Int> _toCollapse = new List<Vector3Int>();
    [SerializeField] private int _nbreOfGrotte;
    private Cell[,] _Cells ;
    private List<Cell[,]> _cells = new List<Cell[,]>();
    [SerializeField] private float _depthToEndLoading;
    [SerializeField] private GrottosChestFiller _chestFiller;

    // Start is called before the first frame update
    void Awake() => _waveFunction.OnFinishedGrotto += LaunchWaveCollapse;
    public  void LaunchWaveCollapse()
    {
        if (_cells.Count <= 0)
        {
            StartCoroutine(_chestFiller.Fill(_waveFunction.GetChests()));
            return;
        }
        var cell = _cells[0];
       
        _cells.RemoveAt(0);
        _waveFunction.InitializeGrid(cell);
    }

    public void SpawnGrotte() 
    {
        int currentGrotte = 0;
        int antiInfiniteloop = 0;
        List<Vector3> placedGrotte = new List<Vector3>();

        while (currentGrotte < _nbreOfGrotte && antiInfiniteloop < 1000) 
        {
            Vector3Int pos = Vector3Int.zero;
            pos.x =(int) Random.Range(_bounds.x, _bounds.z);
            pos.y = (int)Random.Range(_bounds.y, _bounds.w);
            if(CanBePlaced( placedGrotte, pos)) 
            {
                placedGrotte.Add(pos);
                _offset.x = pos.x;
                _offset.y = pos.y;
                _cells.Add(Copy(_toPastePrefab[Random.Range(0, _toPastePrefab.Length)]));
                currentGrotte++;
            }
            antiInfiniteloop++;
        }
        _cells = _cells.OrderByDescending(x => x[0, 0].position.y).ToList();
        LaunchWaveCollapse();
    }


    private bool CanBePlaced(List<Vector3> placedGrotte, Vector3 pos) 
    {
        foreach(Vector3 placed in placedGrotte) 
        {
            if(Vector3.Distance(placed, pos) < _minRange)
                return false;
        }
        return true;
    }
    public Vector4 getBounds(Tilemap toPastePrefab)
    {
        Vector4 bounds = new Vector4();
        bool checkLeft = false;
        bool checkTop = false;
        for (int i = toPastePrefab.cellBounds.min.x - 1; i < toPastePrefab.cellBounds.max.x + 1; i++)
        {
            for (int j = toPastePrefab.cellBounds.min.y - 1; j < toPastePrefab.cellBounds.max.y + 1; j++)
            {
                TileBase tile = toPastePrefab.GetTile(new Vector3Int(i, j));
                if(tile != null ) 
                {
                    if(bounds.x > i || !checkLeft) 
                    {
                        checkLeft =true;
                        bounds.x = i;
                    }
                    if (bounds.y > j || !checkTop)
                    {
                        checkTop = true;
                        bounds.y = j;
                    }

                    if(bounds.z < i )
                        bounds.z = i;

                    if (bounds.w < j)
                        bounds.w = j;

                }
            }
        }
        return bounds;
    }
    public Cell[,] Copy(Tilemap toPastePrefab) 
    {
        Vector4 bounds = getBounds(toPastePrefab);

        _Cells = new Cell[(int)Mathf.Abs(bounds.z- bounds.x)+3, (int)Mathf.Abs(bounds.w - bounds.y) + 3];
        for(int i = (int)bounds.x - 1; i < (int)bounds.z + 2; i++) 
        {
            for (int j = (int)bounds.y - 1; j < (int)bounds.w+2; j++)
            {
                TileBase tile = toPastePrefab.GetTile(new Vector3Int(i, j));
                Cell currentCell = new Cell(new Vector3Int(i, j) + _offset, (tile == null));
                int x = i - (int)bounds.x + 1;
                int y = j - (int)bounds.y + 1;

				_Cells[x, y] = currentCell;
				currentCell.arrayPosition.x = x;
				currentCell.arrayPosition.y = y;

				if (tile != null) 
                {
                    _toCollapse.Add(new Vector3Int(i, j));
                    _tilemap.SetTile(new Vector3Int(i, j) + _offset, null);
                }
               
            }
        }
        return(Cell[,]) _Cells.Clone();
    }

}
