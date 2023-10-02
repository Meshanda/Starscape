using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveFunction : MonoBehaviour
{
    public Vector2Int dimensions;
    public Tile[] tileObjects;
    public Tile _tileGround;

    //public List<Cell> gridComponents;
    public Cell cellObj;
    public Cell[,] cellArray;
    public Tilemap tileMap;
    public Tilemap tileMapDecor;

    int iterations = 0;

    void Awake()
    {
    }

    public void InitializeGrid(Cell[,] cellArray)
    {

        this.cellArray = cellArray;
        dimensions.x = cellArray.GetLength(0);
        dimensions.y = cellArray.GetLength(1);
        foreach (var cell in cellArray) 
        {
            if (cell.collapsed) 
            {
                cell.tileOptions = new Tile[1] { _tileGround };
            }
            else 
            {
                cell.tileOptions = (Tile[])tileObjects.Clone();
            }
        }
        //for (int y = 0; y < dimensions; y++)
        //{
        //    for (int x = 0; x < dimensions; x++)
        //    {
        //        Cell newCell = Instantiate(cellObj, new Vector2(x, y), Quaternion.identity);
        //        newCell.CreateCell(false, tileObjects);
        //        gridComponents.Add(newCell);
        //    }
        //}

        //StartCoroutine(CheckEntropy());
        UpdateGeneration();
    }


    IEnumerator CheckEntropy()
    {
        List<Cell> tempGrid = new List<Cell>();
        foreach(Cell cell in cellArray) 
        {
            tempGrid.Add(cell); 
        }

        tempGrid.RemoveAll(c => c.collapsed);

        tempGrid.Sort((a, b) => { return a.tileOptions.Length - b.tileOptions.Length; });

        int arrLength = tempGrid[0].tileOptions.Length;
        int stopIndex = default;

        for (int i = 1; i < tempGrid.Count; i++)
        {
            if (tempGrid[i].tileOptions.Length > arrLength)
            {
                stopIndex = i;
                break;
            }
        }

        if (stopIndex > 0)
        {
            tempGrid.RemoveRange(stopIndex, tempGrid.Count - stopIndex);
        }

        yield return null;

        CollapseCell(tempGrid);
    }

    void CollapseCell(List<Cell> tempGrid)
    {
        int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);

        Cell cellToCollapse = tempGrid[randIndex];

        cellToCollapse.collapsed = true;
        if (cellToCollapse.tileOptions.Length == 0 || cellToCollapse.tileOptions == null)
        {
            cellToCollapse.collapsed = true;
            Debug.Log("FF");
            UpdateGeneration();
            return;
        }
        //Tile selectedTile = cellToCollapse.tileOptions[UnityEngine.Random.Range(0, cellToCollapse.tileOptions.Length)];
        Tile selectedTile = GetTileWithWeight(cellToCollapse);
        cellToCollapse.tileOptions = new Tile[] { selectedTile };

        Tile foundTile = cellToCollapse.tileOptions[0];
        //Instantiate(foundTile, cellToCollapse.transform.position, Quaternion.identity);
        if(foundTile.inDecor)
            tileMapDecor.SetTile(cellToCollapse.position, foundTile.tile);
        else
            tileMap.SetTile(cellToCollapse.position, foundTile.tile);
        UpdateGeneration();
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
        for(int i  = 0; i < cell.tileOptions.Length; i++) 
        {
            threshold += cell.tileOptions[i].weight;
            if (rngValue <= threshold)
                return cell.tileOptions[i];
        }
        Debug.Log("Help");
        return cell.tileOptions[cell.tileOptions.Length-1];
    }

    void UpdateGeneration()
    {
        Cell[,] newGenerationCell =(Cell[,])cellArray.Clone();
        int nbreNotCollapsed = 0;
        for (int y = 0; y < dimensions.y; y++)
        {
            for (int x = 0; x < dimensions.x; x++)
            {
                var index = x * dimensions.x + y * dimensions.y;
                if (cellArray[x,y].collapsed)
                {
                    newGenerationCell[x, y] = cellArray[x, y];
                }
                else
                {
                    nbreNotCollapsed++;
                    List<Tile> options = new List<Tile>();
                    foreach (Tile t in tileObjects)
                    {
                        options.Add(t);
                    }

                    //update below
                    if (y > 0)
                    {
                        Cell up = cellArray[x,y-1];
                        List<Tile> validOptions = new List<Tile>();
                        
                        foreach (Tile possibleOptions in up.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].upNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //update right
                    if (x < dimensions.x - 1)
                    {
                        Cell right = cellArray[x+1 , y ];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in right.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].leftNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look down
                    if (y < dimensions.y - 1)
                    {
                        Cell down = cellArray[x, y+1];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in down.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].downNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look left
                    if (x > 0)
                    {
                        Cell left = cellArray[x - 1, y];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in left.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].rightNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    Tile[] newTileList = new Tile[options.Count];

                    for (int i = 0; i < options.Count; i++)
                    {
                        newTileList[i] = options[i];
                    }

                    newGenerationCell[x, y].RecreateCell(newTileList);
                }
            }
        }

        cellArray = newGenerationCell;
        iterations++;

        if (iterations < dimensions.x * dimensions.y && nbreNotCollapsed != 0)
        {
            StartCoroutine(CheckEntropy());
        }

    }

    void CheckValidity(List<Tile> optionList, List<Tile> validOption)
    {
        for (int x = optionList.Count - 1; x >= 0; x--)
        {
            var element = optionList[x];
            if (!validOption.Contains(element))
            {
                optionList.RemoveAt(x);
            }
        }
    }
}