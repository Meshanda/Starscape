using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UI;
using UnityEngine;
using UnityEngine.Tilemaps;


public delegate void Notify();
public class WaveFunction : MonoBehaviour
{
    public Vector2Int dimensions;
    public Tile[] tileObjects;
    public Tile _tileGround;
    public TileBase _tileError;

    //public List<Cell> gridComponents;
    public Tile chest;
    public List<ChestAndPos> chestList = new List<ChestAndPos>();
    public Cell cellObj;
    public Cell[,] cellArray;
    public Tilemap tileMap;
    public Tilemap tileMapDecor;
    public Tilemap tileMapBG; 
    public event Notify ProcessCompleted;
    private List<TileToPlace> _tilesToPlace = new List<TileToPlace>();
    int iterations = 0;
    private  List<Cell> baseCellToPropagate = new List<Cell>();
    public event Action OnFinishedGrotto; 
    private bool _emergencyStop = false;
    private System.Random random;
    private bool _finished = false;

    public event Action<List<TileToPlace>> GenerationComplete;
    private Cell GetCell(int x, int y) 
    {
        if (x < 0 || x >= cellArray.GetLength(0))
            return null;

		if (y < 0 || y >= cellArray.GetLength(1))
			return null;

        return cellArray[x, y];
	}

    private Cell GetRight(Cell oCell) 
    {
        return GetCell( oCell.arrayPosition.x - 1, oCell.arrayPosition.y );
	}

	private Cell GetLeft(Cell oCell)
	{
		return GetCell( oCell.arrayPosition.x + 1, oCell.arrayPosition.y );
	}

	private Cell GetUp(Cell oCell)
	{
		return GetCell( oCell.arrayPosition.x, oCell.arrayPosition.y + 1 );
	}

	private Cell GetDown(Cell oCell)
	{
		return GetCell( oCell.arrayPosition.x, oCell.arrayPosition.y - 1 );
	}

	void Awake()
    {
       
    }
    private void OnDestroy()
    {
    }
    


    public void CorrectRule()
    {
		//TODO ajouter exception "Ground"
        List<Tile> tiles = new List<Tile>(tileObjects);
        tiles = tiles.Distinct().ToList();
        
        foreach(Tile currentTile in tiles) 
        {
            foreach(Tile right in currentTile.rightNeighbours) 
            {
                if (!right.leftNeighbours.Contains(currentTile)) 
                {
                    right.leftNeighbours.Add(currentTile);
                    Debug.Log(currentTile.name + " added to " + right.name + " left");
                }
            }
            foreach (Tile up in currentTile.upNeighbours)
            {
                if (!up.downNeighbours.Contains(currentTile))
                {
                    up.downNeighbours.Add(currentTile);
                    Debug.Log(currentTile.name + " added to " + up.name + " down");
                }
            }
            foreach (Tile left in currentTile.leftNeighbours)
            {
                if (!left.rightNeighbours.Contains(currentTile))
                {
                    left.rightNeighbours.Add(currentTile);
                    Debug.Log(currentTile.name + " added to " + left.name + " right");
                }
            }
            foreach (Tile down in currentTile.downNeighbours)
            {
                if (!down.upNeighbours.Contains(currentTile))
                {
                    down.upNeighbours.Add(currentTile);
                    Debug.Log(currentTile.name + " added to " + down.name + " up");
                }
            }
        }

    }
    private void GenerationFinishedEventHandler(List<TileToPlace> listToPlace)
    {
        _tilesToPlace = listToPlace;
        placeAllTile();
        // Votre logique à exécuter lorsque la génération est terminée
    }
    public void InitializeGrid(Cell[,] cellArray)
    {
        //CorrectRule();
        foreach(var tile in tileObjects) 
        {
            tile.SOName = tile.name;
        }
        this.cellArray = cellArray;
        foreach (var cell in cellArray) 
        {
            if (cell.collapsed) 
            {
                baseCellToPropagate.Add(cell);
                cell.tileOptions = new List<Tile> { _tileGround };
                //_tilesToPlace.Add(new TileToPlace
                //{
                //    pos = cell.position,
                //    tile = _tileGround.tile,
                //    inDecor = false
                //});
            }
            else 
            {
                cell.tileOptions = new List<Tile>(tileObjects);
            }
        }



        //UpdateGeneration();
        //StartCoroutine(StartCollapse());
        List<TileToPlace> tilesToPlace= null;
        _finished = false;
        System.Threading.Thread generationThread = new System.Threading.Thread(()=> { _tilesToPlace = StartCollapseNotCoroutine(); _finished = true; });
        generationThread.Start();
        StartCoroutine(EndCheck());
        //generationThread.Join();
    }
    public IEnumerator EndCheck() 
    {
        yield return new WaitUntil(() => _finished);
        placeAllTile();
        OnFinishedGrotto?.Invoke();
    }

    public bool HasToPropagate(Cell cell) 
    {
        if((GetLeft(cell) == null || (GetLeft(cell).tileOptions.Count > 1 ))
           || (GetRight(cell) == null || GetRight(cell).tileOptions.Count > 1)
            || (GetUp(cell) == null || GetUp(cell).tileOptions.Count > 1)
            || (GetDown(cell) == null || GetDown(cell).tileOptions.Count > 1)) 
        {
            return true;
        }
        return false;
    }
    int collapsed;
    IEnumerator StartCollapse()
    {
        int i = 0;
        for(int j = 0; j < baseCellToPropagate.Count; j++)
        {
            if (!HasToPropagate(baseCellToPropagate[j])) 
            {
                continue;
            }
            Propagate(baseCellToPropagate[j]);
            if (j < baseCellToPropagate.Count / 2) ;
                yield return null;
        }
        collapsed = 0;
        i = 0;
        while (!isCollapsed() && !_emergencyStop)
        {
            i++;

            yield return null;
            //Debug.Log("working");
            Iterate();
            //placeAllTile();
        }
        //StopAllCoroutines();
        OnFinishedGrotto?.Invoke();
    }
    private List<TileToPlace> StartCollapseNotCoroutine()
    {
        random = new System.Random();
        int i = 0;
        for (int j = 0; j < baseCellToPropagate.Count; j++)
        {
            if (!HasToPropagate(baseCellToPropagate[j]))
            {
                continue;
            }
            Propagate(baseCellToPropagate[j]);
        }
        collapsed = 0;
        while (!isCollapsed() && !_emergencyStop)
        {
            //Debug.Log("working");
            Iterate();
		}

        //ProcessCompleted?.Invoke();
        return _tilesToPlace;
    }
    private bool isCollapsed()
    {
        //check if any cells contain more than one entry
        foreach (Cell c in cellArray)
        {
            if (c.collapsed == false)
                return false;
        }
        return true;
    }
    private void Iterate()
    {
        Cell cell = CheckEntropy();
        Propagate(cell);
    }

    private Cell CheckEntropy()
    {
        List<Cell> tempGrid = new List<Cell>();
        foreach(Cell cell in cellArray) 
        {
            tempGrid.Add(cell); 
        }

        tempGrid.RemoveAll(c => c.collapsed);

        tempGrid.Sort((a, b) => { return a.tileOptions.Count - b.tileOptions.Count; });

        int arrLength = tempGrid[0].tileOptions.Count;
        int stopIndex = default;

        for (int i = 1; i < tempGrid.Count; i++)
        {
            if (tempGrid[i].tileOptions.Count > arrLength)
            {
                stopIndex = i;
                break;
            }
        }

        if (stopIndex > 0)
        {
            tempGrid.RemoveRange(stopIndex, tempGrid.Count - stopIndex);
        }


        return CollapseCell(tempGrid);
    }

    Cell CollapseCell(List<Cell> tempGrid)
    {

        int randIndex = random.Next(0, tempGrid.Count); 

        Cell cellToCollapse = tempGrid[randIndex];

        cellToCollapse.collapsed = true;
        //if (cellToCollapse.tileOptions.Count == 0 || cellToCollapse.tileOptions == null)
        //{
        //    cellToCollapse.collapsed = true;
        //    Debug.Log("FF");
        //    UpdateGeneration();
        //    return;
        //}
        if (cellToCollapse.tileOptions.Count == 0)
        {
            Debug.Log("FF");
            return cellToCollapse;
        }
        Tile selectedTile = GetTileWithWeight(cellToCollapse);
        cellToCollapse.tileOptions = new List<Tile> { selectedTile };

        Tile foundTile = cellToCollapse.tileOptions[0];
        _tilesToPlace.Add(new TileToPlace
        {
            pos = cellToCollapse.position,
            tile = foundTile.tile,
            inDecor = foundTile.inDecor
        });
        collapsed++;
        return cellToCollapse;
    }

    public void placeAllTile() 
    {
        foreach (var tile in _tilesToPlace) 
        {
            Tilemap placeTilemap = World.Instance.GetTilemapsFromLayers(GameManager.Instance.database.GetItemByTile(tile.tile).tileInfo.placingRules.placeLayer)[0];
            placeTilemap.SetTile(tile.pos, tile.tile);
            
            
            if (tile.tile == chest.tile)
            {
                var Tile = placeTilemap.GetTile(tile.pos);
                
                chestList.Add(new ChestAndPos 
                { 
                    chest = tile.tile, 
                    pos = tile.pos
                });
            }

        }
        _tilesToPlace.Clear();
    }

    public List<ChestAndPos> GetChests() => chestList;
    public Tile GetTileWithWeight(Cell cell) 
    {
        float totalWeight = 0;
        foreach(var option in cell.tileOptions) 
        {
            totalWeight += option.weight;
        }
        float rngValue = (float)random.NextDouble();
        rngValue = rngValue*( totalWeight + 1f); 
        float threshold = 0;
        for(int i  = 0; i < cell.tileOptions.Count; i++) 
        {
            threshold += cell.tileOptions[i].weight;
            if (rngValue <= threshold)
                return cell.tileOptions[i];
        }
        return cell.tileOptions[cell.tileOptions.Count-1];
    }

  

    public List<Tile> GetRightPossibleNeighbor(Cell cell) 
    {
        List<Tile> values = new List<Tile>();
        foreach(Tile tile in cell.tileOptions) 
        {
            values.AddRange(tile.rightNeighbours);
        }
        return values.Distinct().ToList();
    }

    public List<Tile> GetUpPossibleNeighbor(Cell cell)
    {
        List<Tile> values = new List<Tile>();
        foreach (Tile tile in cell.tileOptions)
        {
            values.AddRange(tile.upNeighbours);
        }
        return values.Distinct().ToList();
    }
    public List<Tile> GetDownPossibleNeighbor(Cell cell)
    {
        List<Tile> values = new List<Tile>();
        foreach (Tile tile in cell.tileOptions)
        {
            values.AddRange(tile.downNeighbours);
        }
        return values.Distinct().ToList();
    }
    public List<Tile> GetLeftPossibleNeighbor(Cell cell)
    {
        List<Tile> values = new List<Tile>();
        foreach (Tile tile in cell.tileOptions)
        {
            values.AddRange(tile.leftNeighbours);
        }
        return values.Distinct().ToList();
    }
    public delegate List<Tile> GetPossibleNeighbor(Cell cell);
    
    public bool GetPossibleNeighborOption(Cell currentCell ,Cell neigborCell, GetPossibleNeighbor func , string funcName ) 
    {
        if (neigborCell == null )
            return false;

		if (neigborCell.tileOptions.Count == 1 && neigborCell.tileOptions[0] == _tileGround)
			return false;

        string debug = "Tiles Options " + currentCell.tileOptions.Count + ":";
        foreach (Tile tile in currentCell.tileOptions)
        {
            debug += tile.SOName + ", ";
        }
        debug += " PossibleConnection :";
        //Get sockets that we have available on our Right
        List<Tile> possibleConnections = func(currentCell);
        foreach (Tile tile in possibleConnections)
        {
            debug += tile.SOName + ", ";
        }
        debug += " neighbor connection : ";
        foreach (Tile tile in neigborCell.tileOptions)
        {
            debug += tile.SOName + ", ";
        }
        debug += "Function " + funcName;
        List<Tile> ogConnectionNeigbor = new List<Tile> (neigborCell.tileOptions);
        bool constrained = false;

        for (int i = 0; i < neigborCell.tileOptions.Count; i++)
        {
            //if the list of sockets that we have on the right does not contain the connector on the other cell to the left...
            if (!possibleConnections.Contains(neigborCell.tileOptions[i]))
            {
                //then that is not a valid possibility and must be removed
                neigborCell.tileOptions.RemoveAt(i);
                if(neigborCell.tileOptions.Count == 0 )
                {
                    _emergencyStop = true;
                    _tilesToPlace.Add(new TileToPlace
                    {
                        pos = neigborCell.position,
                        tile = _tileError,
                        inDecor = false
                    });

                    Debug.Log(debug);
                    return false; 
                }
                i -= 1;
                constrained = true;
            }
        }
        //if(neigborCell.tileOptions.Count == 1) 
        //{
        //    neigborCell.collapsed = true;
        //    _tilesToPlace.Add(new TileToPlace
        //    {
        //        pos = neigborCell.position,
        //        tile = neigborCell.tileOptions[0].tile,
        //        inDecor = false
        //    });
        //}

        return constrained;
    }
    private void Propagate(Cell cell)
    {
        List<Cell> cellsAffected = new List<Cell> { cell };

		while (cellsAffected.Count > 0 && !_emergencyStop)
        {
            Cell currentCell = cellsAffected[0];
            cellsAffected.Remove(currentCell);

            Cell otherCell = GetRight(currentCell);
            bool hasChanged = GetPossibleNeighborOption(currentCell, otherCell, GetRightPossibleNeighbor, "GetRightPossibleNeighbor");
            if(hasChanged ) 
            {
                cellsAffected.Add(otherCell);
            }
            otherCell = GetUp(currentCell);
            hasChanged = GetPossibleNeighborOption(currentCell, otherCell, GetUpPossibleNeighbor, "GetUpPossibleNeighbor");
            if (hasChanged)
            {
                cellsAffected.Add(otherCell);
            }

            otherCell = GetLeft(currentCell);
			hasChanged = GetPossibleNeighborOption(currentCell, otherCell, GetLeftPossibleNeighbor, "GetleftPossibleNeighbor");
            if (hasChanged)
            {
                cellsAffected.Add(otherCell);
            }

            otherCell = GetDown(currentCell);
            hasChanged = GetPossibleNeighborOption(currentCell, otherCell, GetDownPossibleNeighbor, "GetDownPossibleNeighbor");
            if (hasChanged)
            {
                cellsAffected.Add(otherCell);
            }
        }
    }

    //void CheckValidity(List<Tile> optionList, List<Tile> validOption)
    private void Print( Cell oCell )
	{
		string sDebug = oCell.position.x + ";" + oCell.position.y + ":" + oCell.tileOptions[0].name;

        sDebug += "\n";

		Cell oRight = GetRight(oCell);
		Cell oLeft = GetLeft(oCell);
		Cell oUp = GetUp(oCell);
		Cell oDown = GetDown(oCell);

		if (oRight != null)
        {
            if (oRight.tileOptions.Count != 1)
                sDebug += "error count: " + oRight.tileOptions.Count;
            sDebug += oRight.tileOptions[0].name;
        }
        else
		{
			sDebug += "NONE";
		}

		sDebug += "\n";
		if (oLeft != null)
		{
			if (oLeft.tileOptions.Count != 1)
				sDebug += "error count: " + oLeft.tileOptions.Count;
			sDebug += oLeft.tileOptions[0].name;
		}
		else
		{
			sDebug += "NONE";
		}

		sDebug += "\n";
		if (oUp != null)
		{
			if (oUp.tileOptions.Count != 1)
				sDebug += "error count: " + oUp.tileOptions.Count;
			sDebug += oUp.tileOptions[0].name;
		}
		else
		{
			sDebug += "NONE";
		}

		sDebug += "\n";
		if (oDown != null)
		{
			if (oDown.tileOptions.Count != 1)
				sDebug += "error count: " + oDown.tileOptions.Count;
			sDebug += oDown.tileOptions[0].name;
		}
		else
		{
			sDebug += "NONE";
		}

		Debug.Log(sDebug);
	}


}

public struct  TileToPlace
{
   public Vector3Int pos;
    public TileBase tile;
    public string SOname;
    public bool inDecor;
}
[Serializable]
public struct ChestAndPos 
{
    public Vector3Int pos;
    public TileBase chest;
}