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
    
    // Start is called before the first frame update
    void Start()
    {
        _waveFunction.ProcessCompleted += InvokeTheCollapse;
    }
    public void InvokeTheCollapse() 
    {
        Invoke("LaunchWaveCollapse", 0.01f);
    }
    public void LaunchWaveCollapse()
    {
       
        if (_cells == null || _cells.Count == 0)
        {
            _loadingScreen.SetActive(false);
            return;
        }
        if (_cells[0][0, 0].position.y < _depthToEndLoading)
            _loadingScreen.SetActive(false);
        _waveFunction.InitializeGrid(_cells[0]);
        _cells.RemoveAt(0);
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

    public Cell[,] Copy(Tilemap toPastePrefab) 
    {
        _Cells = new Cell[toPastePrefab.size.x+2, toPastePrefab.size.y+2];
        for(int i = toPastePrefab.cellBounds.min.x-1; i < toPastePrefab.cellBounds.max.x+1; i++) 
        {
            for (int j = toPastePrefab.cellBounds.min.y-1; j < toPastePrefab.cellBounds.max.y+1; j++)
            {
                TileBase tile = toPastePrefab.GetTile(new Vector3Int(i, j));
                Cell currentCell = new Cell(new Vector3Int(i, j) + _offset, (tile == null));
                int x = i - toPastePrefab.cellBounds.min.x + 1;
                int y = j - toPastePrefab.cellBounds.min.y + 1;

				_Cells[x, y] = currentCell;
				currentCell.arrayPosition.x = x;
				currentCell.arrayPosition.y = y;

				if (i > 0)
				{
					//_Cells[i - toPastePrefab.cellBounds.min.x + 1 - 1, j - toPastePrefab.cellBounds.min.y + 1].right = currentCell;
					//currentCell.left = _Cells[i - toPastePrefab.cellBounds.min.x + 1 - 1, j - toPastePrefab.cellBounds.min.y + 1];
				}
				if (j > 0)
				{
					//_Cells[i - toPastePrefab.cellBounds.min.x + 1, j - toPastePrefab.cellBounds.min.y + 1 - 1].up = currentCell;
					//currentCell.down = _Cells[i - toPastePrefab.cellBounds.min.x + 1, j - toPastePrefab.cellBounds.min.y + 1 - 1];
				}

				if (tile != null) 
                {
                    _toCollapse.Add(new Vector3Int(i, j));
                    _tilemap.SetTile(new Vector3Int(i, j) + _offset, null);
                }
               
            }
        }
        return(Cell[,]) _Cells.Clone();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
