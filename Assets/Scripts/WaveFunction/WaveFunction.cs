using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;
using static UnityEditor.PlayerSettings;

public delegate void Notify();
public class WaveFunction : MonoBehaviour
{
    public Vector2Int dimensions;
    public Tile[] tileObjects;
    public Tile _tileGround;
    public TileBase _tileError;

    //public List<Cell> gridComponents;
    public Cell cellObj;
    public Cell[,] cellArray;
    public Tilemap tileMap;
    public Tilemap tileMapDecor; 
    public event Notify ProcessCompleted;
    private List<TileToPlace> _tilesToPlace = new List<TileToPlace>();
    int iterations = 0;
    private  List<Cell> baseCellToPropagate = new List<Cell>();
    private bool _emergencyStop = false;

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

    public void CorrectRule()
    {
		//TODO ajouter exception "Ground"
        List<Tile> tiles = new List<Tile>(tileObjects);
        tiles.Add(_tileGround);
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

    public void InitializeGrid(Cell[,] cellArray)
    {
        //CorrectRule();
        this.cellArray = cellArray;
        foreach (var cell in cellArray) 
        {
            if (cell.collapsed) 
            {
                baseCellToPropagate.Add(cell);
                cell.tileOptions = new List<Tile> { _tileGround };
            }
            else 
            {
                cell.tileOptions = new List<Tile>(tileObjects);
            }
        }

        foreach (var cell in baseCellToPropagate)
        {
            Propagate(cell);
        }

		//UpdateGeneration();
		//StartCoroutine(StartCollapse());
		StartCollapseNotCoroutine();
    }

    int collapsed;
    IEnumerator StartCollapse()
    {
        collapsed = 0;
        while (!isCollapsed() && !_emergencyStop)
        {
            Debug.Log("working");
            yield return null;
            Debug.Break();
            Iterate();
        }
        Debug.Log("done");
        placeAllTile();
        //StopAllCoroutines();
        //ProcessCompleted?.Invoke();
    }
    private void StartCollapseNotCoroutine()
    {
        collapsed = 0;
        while (!isCollapsed() && !_emergencyStop)
        {
            //Debug.Log("working");
            Iterate();
		}
		//Verif2();
		Debug.Log("done");
        placeAllTile();

		//StopAllCoroutines();
		//ProcessCompleted?.Invoke();
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
        int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);

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
        placeAllTile();
        return cellToCollapse;
    }

    public void placeAllTile() 
    {
        foreach (var tile in _tilesToPlace) 
        {
            /*
            if(tile.tile != null)
                Debug.Log(tile.tile.name);
            else
                Debug.Log("Empty");
            */
            if (tile.inDecor) 
            {
                tileMapDecor.SetTile(tile.pos, tile.tile);
            }
            else 
            {
                tileMap.SetTile(tile.pos, tile.tile);
            }

        }
        _tilesToPlace.Clear();
    }
    public Tile GetTileWithWeight(Cell cell) 
    {
        int totalWeight = 0;
        foreach(var option in cell.tileOptions) 
        {
            totalWeight += option.weight;
        }
        int rngValue = UnityEngine.Random.Range(1, totalWeight + 1);
        int threshold = 0;
        for(int i  = 0; i < cell.tileOptions.Count; i++) 
        {
            threshold += cell.tileOptions[i].weight;
            if (rngValue <= threshold)
                return cell.tileOptions[i];
        }
        Debug.Log("Help" + rngValue +" "+ totalWeight+" "+ cell.tileOptions.Count);
        return cell.tileOptions[cell.tileOptions.Count-1];
    }

    //void UpdateGeneration()
    //{
    //    Cell[,] newGenerationCell =(Cell[,])cellArray.Clone();
    //    int nbreNotCollapsed = 0;
    //    for (int y = 0; y < dimensions.y; y++)
    //    {
    //        for (int x = 0; x < dimensions.x; x++)
    //        {
    //            var index = x * dimensions.x + y * dimensions.y;
    //            if (cellArray[x,y].collapsed)
    //            {
    //                newGenerationCell[x, y] = cellArray[x, y];
    //            }
    //            else
    //            {
    //                nbreNotCollapsed++;
    //                List<Tile> options = new List<Tile>();
    //                foreach (Tile t in tileObjects)
    //                {
    //                    options.Add(t);
    //                }

    //                //update below
    //                if (y > 0)
    //                {
    //                    Cell up = cellArray[x,y-1];
    //                    List<Tile> validOptions = new List<Tile>();
                        
    //                    foreach (Tile possibleOptions in up.tileOptions)
    //                    {
    //                        var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
    //                        var valid = tileObjects[valOption].upNeighbours;

    //                        validOptions = validOptions.Concat(valid).ToList();
    //                    }

    //                    CheckValidity(options, validOptions);
    //                }

    //                //update right
    //                if (x < dimensions.x - 1)
    //                {
    //                    Cell right = cellArray[x+1 , y ];
    //                    List<Tile> validOptions = new List<Tile>();

    //                    foreach (Tile possibleOptions in right.tileOptions)
    //                    {
    //                        var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
    //                        var valid = tileObjects[valOption].leftNeighbours;

    //                        validOptions = validOptions.Concat(valid).ToList();
    //                    }

    //                    CheckValidity(options, validOptions);
    //                }

    //                //look down
    //                if (y < dimensions.y - 1)
    //                {
    //                    Cell down = cellArray[x, y+1];
    //                    List<Tile> validOptions = new List<Tile>();

    //                    foreach (Tile possibleOptions in down.tileOptions)
    //                    {
    //                        var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
    //                        var valid = tileObjects[valOption].downNeighbours;

    //                        validOptions = validOptions.Concat(valid).ToList();
    //                    }

    //                    CheckValidity(options, validOptions);
    //                }

    //                //look left
    //                if (x > 0)
    //                {
    //                    Cell left = cellArray[x - 1, y];
    //                    List<Tile> validOptions = new List<Tile>();

    //                    foreach (Tile possibleOptions in left.tileOptions)
    //                    {
    //                        var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
    //                        var valid = tileObjects[valOption].rightNeighbours;

    //                        validOptions = validOptions.Concat(valid).ToList();
    //                    }

    //                    CheckValidity(options, validOptions);
    //                }

    //                Tile[] newTileList = new Tile[options.Count];

    //                for (int i = 0; i < options.Count; i++)
    //                {
    //                    newTileList[i] = options[i];
    //                }

    //                newGenerationCell[x, y].RecreateCell(newTileList.ToList());
    //            }
    //        }
    //    }

    //    cellArray = newGenerationCell;
    //    iterations++;

       
    //    //else 
    //    //{
    //    //    placeAllTile();
    //    //    StopAllCoroutines();
    //    //    iterations = 0;
    //    //    ProcessCompleted?.Invoke();
    //    //}
    //}

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

		string debug = "Tiles Options "+ currentCell.tileOptions.Count + ":";
        foreach (Tile tile in currentCell.tileOptions)
        {
            debug += tile.name + ", ";
        }
        debug += " PossibleConnection :";
        //Get sockets that we have available on our Right
        List<Tile> possibleConnections = func(currentCell);
        foreach (Tile tile in possibleConnections) 
        {
            debug += tile.name+", ";
        }
        debug += " neighbor connection : ";
        foreach (Tile tile in neigborCell.tileOptions)
        {
            debug += tile.name + ", ";
        }
        debug +="Function "+ funcName;
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
    public bool inDecor;
}