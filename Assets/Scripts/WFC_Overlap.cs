using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TileCellSideData
{
    public int index;

    public int leftCount;
    public int rightCount;
    public int upCount;
    public int downCount;
}
public class CellDataNew
{
    public List<TileCellSideData> domain = new List<TileCellSideData>();
    public bool placed = false;
}

public class BigTile
{
    public List<int> tiles = new List<int>();
    public int index = -1;
    public List<int> leftAdjacencies = new List<int>();
    public List<int> rightAdjacencies = new List<int>();
    public List<int> upAdjacencies = new List<int>();
    public List<int> downAdjacencies = new List<int>();

    public int weight = 10;

    public void TestAdjacent(BigTile other)
    {
        // This checks if a given tile can be placed adjacent to this one in the 4 cardinal directions
        //can other be placed to the left of this
        bool canPlace = true;
        for (int i = 0; i < 6; i++)
        {
            if (tiles[i] != other.tiles[i + 3])
            {
                canPlace = false;
                break;
            }
        }
        if (canPlace)
        {
            leftAdjacencies.Add(other.index);
        }

        // right of this
        canPlace = true;
        for (int i = 0; i < 6; i++)
        {
            if (tiles[i+3] != other.tiles[i])
            {
                canPlace = false;
                break;
            }
        }
        if (canPlace)
        {
            rightAdjacencies.Add(other.index);
        }

        // above this
        canPlace = true;
        if (   tiles[1] == other.tiles[0] && tiles[2] == other.tiles[1]
            && tiles[4] == other.tiles[3] && tiles[5] == other.tiles[4]
            && tiles[7] == other.tiles[6] && tiles[8] == other.tiles[7])
        {
            canPlace = true;
        }
        else
        {
            canPlace= false;
        }
        if (canPlace)
        {
            upAdjacencies.Add(other.index);
        }

        // below this
        canPlace = true;
        for (int i = 0; i < 9; i++)
        {
            if (i % 3 != 2 && tiles[i] != other.tiles[i + 1])
            {
                canPlace = false;
                break;
            }
        }
        if (canPlace)
        {
            downAdjacencies.Add(other.index);
        }
    }

    public void SetWeight(float arousal)
    {
        float percentArousal = (arousal - 1) / 8;

        // finds how many tiles are floor tiles
        int floorCount = 0;
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] == 0)
            {
                floorCount++;

            }
        }

        // calculates tile weight based off of arousal
        if (floorCount == 9)
        {
            // for a full tile, weight 40-5
            weight = (int)((50-3) * (1-percentArousal)) + 3;
        }
        else if (floorCount >= 6)
        {
            // for a mostly full tile, weight 10-40
            weight = (int)((40-10) * (percentArousal)) + 10;
        }
        else if (floorCount >= 3)
        {
            // for a corridor / room start, weight 30-15
            weight = (int)((30 - 15) * (1-percentArousal)) + 15;
        }
        else
        {
            // we don't really want many black tiles apart from neccessity
            weight = (int)((15 - 5) * (percentArousal)) + 5;
        }
    }
}

public class WFC_Overlap : MonoBehaviour
{
    enum direction
    {
        up, down, left, right, none
    }

    [SerializeField] Tilemap inputMap;
    [SerializeField] Tilemap outputMap;

    [SerializeField] Vector2Int inputSize;

    List<BigTile> bigTiles = new List<BigTile>();
    public List<Tile> tiles;
    List<int> inputMapTiles = new List<int>();

    [SerializeField] Vector2Int outputSize;

    List<CellDataNew> cells = new List<CellDataNew>();

    [SerializeField] MIDIfilepath MIDIfile;

    List<int> fullTiles = new List<int>();

    int seed;
    float minEntropy = 0;

    Vector2Int minEntropyCell = new Vector2Int();

    //List<Vector2> propagation_queue = new List<Vector2>();

    // Start is called before the first frame update
    async void Start()
    {
        await Setup();
    }

    async Task Setup()
    {
        seed = MIDIfile.seed;
        Random.InitState(seed);


        // gathers all the tiles from the input map
        for (int i = 0; i < inputSize.x; i++)
        {
            for (int j = 0; j < inputSize.y; j++)
            {
                Tile tile = (Tile)inputMap.GetTile(new Vector3Int(i, j, 0));

                // Checks the tile is an accepted tile
                for (int k = 0; k < tiles.Count; k++)
                {
                    if (tile == tiles[k])
                    {
                        inputMapTiles.Add(k);
                    }
                }
            }
        }
        // Genereates the 3x3 big tiles that will be used by the WFC algorithm
        int currentIndex = 0;
        for (int i = 0; i < inputSize.x; i++)
        {
            for (int j = 0; j < inputSize.y; j++)
            {
                BigTile newGrid = new BigTile();
                //newGrid.index = j + (i * inputSize.y);
                newGrid.index = currentIndex;
                for (int x = i; x < i + 3; x++)
                {
                    int xStart = x % inputSize.x;
                    for (int y = j; y < j + 3; y++)
                    {
                        int yStart = y % inputSize.y;
                        newGrid.tiles.Add(inputMapTiles[yStart + (xStart * inputSize.y)]);
                    }
                }
                // Checks if this tile already exists, if it does, ignore it and add its weight to the existing one
                bool duplicate = false;
                int originalPos = -1;
                for (int k = 0; k < bigTiles.Count; k++)
                {
                    bool sameThis = true;
                    for (int l = 0; l < bigTiles[k].tiles.Count; l++)
                    {
                        if (bigTiles[k].tiles[l] != newGrid.tiles[l])
                        {
                            sameThis = false;
                            break;
                        }
                    }
                    if (sameThis)
                    {
                        duplicate = true;
                        originalPos = k;
                        break;
                    }
                }
                newGrid.SetWeight(MIDIfile.arousal);
                if (duplicate)
                {
                    bigTiles[originalPos].weight += newGrid.weight;
                }
                else
                {
                    bigTiles.Add(newGrid);
                    currentIndex++;
                }
            }
        }

        for (int i = 0; i < bigTiles.Count; i++)
        {
            if (bigTiles[i].weight > 10)
            {
                fullTiles.Add(i);
            }
        }

        await Task.Yield();

        // Calculates the ajacencies for each tile
        for (int i = 0; i < bigTiles.Count; i++)
        {
            for (int j = 0; j < bigTiles.Count; j++)
            {
                bigTiles[i].TestAdjacent(bigTiles[j]);
                if (j == bigTiles.Count - 1)
                {
                    await Task.Yield();
                }
            }
        }

        bool completed = false;
        while (!completed)
        {
            completed = await WFC();
            //print(completed);
        }
    }

    async Task<bool> WFC()
    {
        cells.Clear();

        // Calculates the number of ajacencies for each tile on each side in the domain of each tile, for efficiency
        int totalSize = outputSize.x * outputSize.y;

        for (int i = 0; i < totalSize; i++)
        {
            CellDataNew defaultCellData = new CellDataNew();
            for (int j = 0; j < bigTiles.Count; j++)
            {
                TileCellSideData tileAdjacencies = new TileCellSideData();
                tileAdjacencies.index = j;
                tileAdjacencies.leftCount = bigTiles[j].leftAdjacencies.Count;
                tileAdjacencies.rightCount = bigTiles[j].rightAdjacencies.Count;
                tileAdjacencies.upCount = bigTiles[j].upAdjacencies.Count;
                tileAdjacencies.downCount = bigTiles[j].downAdjacencies.Count;
                defaultCellData.domain.Add(tileAdjacencies);
            }
            cells.Add(defaultCellData);
        }


        // creates a perimeter of black tiles so that the map won't cut off at the edge and look unnatural
        for (int i = 0; i < outputSize.x; i++)
        {
            for (int j = 0; j < outputSize.y; j++)
            {
                if (j == 0 || j == outputSize.y - 1 || i == 0 || i == outputSize.x - 1)
                {
                    List<int> removedTiles = new List<int>();
                    int blankPos = -1;
                    for (int k = 0; k < cells[j * outputSize.x + i].domain.Count; k++)
                    {
                        if (cells[j * outputSize.x + i].domain[k].index != 3)
                        {
                            removedTiles.Add(cells[j * outputSize.x + i].domain[k].index);
                        }
                        else
                        {
                            blankPos = k;
                        }
                    }
                    cells[j * outputSize.x + i].placed = true;
                    cells[j * outputSize.x + i].domain = new List<TileCellSideData>() { cells[j * outputSize.x + i].domain[blankPos] };
                    SetBigTile(i, j, bigTiles[3]);
                    PropagateNew(i, j, removedTiles);
                }
            }
        }

        //WFC START

        // chooses a starting cell based on the least impact it would have on surrounding tiles
        // reduces the chance of reaching an unsolvable position
        //print("started");
        minEntropy = 9999;
        for (int i = 0; i < cells.Count; i++)
        {
            CellDataNew cell = cells[i];
            if (!cell.placed)
            {
                //float entropy = cell.domain.Count;
                float entropy = 0;
                for (int j = 0; j < cell.domain.Count; j++)
                {
                    entropy += bigTiles[cell.domain[j].index].weight * Mathf.Log10(bigTiles[cell.domain[j].index].weight);
                }

                if (entropy < minEntropy)
                {
                    minEntropyCell = new Vector2Int(i % outputSize.x, i / outputSize.y);
                    minEntropy = entropy;
                }
            }
        }

        //int start_tile = (int)(seed % total_size);
        //int start_x = start_tile % output_size.x;
        //int start_y = start_tile / output_size.y;
        //min_entropy_cell = new Vector2Int(start_x, start_y);

        // while there are still tiles to place
        while (minEntropy != 9999)
        {
            // resets entropy for the next loop
            minEntropy = 9999;

            int minEntropyCellData = minEntropyCell.y * outputSize.x + minEntropyCell.x;


            // chooses a weighted random tile from the cells domain
            int totalWeight = 0;
            for (int i = 0; i < cells[minEntropyCellData].domain.Count; i++)
            {
                totalWeight += bigTiles[cells[minEntropyCellData].domain[i].index].weight;
            }

            int randWeight = Random.Range(0, totalWeight);
            int tile = -1;

            for (int i = 0; i < cells[minEntropyCellData].domain.Count; i++)
            {
                randWeight -= bigTiles[cells[minEntropyCellData].domain[i].index].weight;
                if (randWeight < 0)
                {
                    tile = i;
                    break;
                }
            }

            // chooses a random tile from the cells domain
            //int tile = Random.Range(0, cells[minEntropyCellData].domain.Count);

            // mark the cell as placed (so that it can't be altered later)
            cells[minEntropyCellData].placed = true;

            // sets the domain of the placed cell to only contain the tile chosen
            List<int> removedTiles = new List<int>();
            for (int i = 0; i < cells[minEntropyCellData].domain.Count; i++)
            {
                if (i != tile)
                {
                    removedTiles.Add(cells[minEntropyCellData].domain[i].index);
                }
            }

            cells[minEntropyCellData].domain = new List<TileCellSideData> { cells[minEntropyCellData].domain[tile] };

            // places the tile on the visible grid
            SetBigTile(minEntropyCell.x, minEntropyCell.y, bigTiles[cells[minEntropyCellData].domain[0].index]);

            // updates surrounding cells domain based on this change
            if (!PropagateNew(minEntropyCell.x, minEntropyCell.y, removedTiles))
            {
                return false;
            }

            // recalculates the cell with the least entropy
            for (int i = 0; i < cells.Count; i++)
            {
                CellDataNew cell = cells[i];
                if (!cell.placed)
                {
                    //float entropy = cell.domain.Count;
                    float entropy = 0;
                    for (int j = 0; j < cell.domain.Count; j++)
                    {
                        entropy += bigTiles[cell.domain[j].index].weight * Mathf.Log10(bigTiles[cell.domain[j].index].weight);
                    }

                    if (entropy < minEntropy)
                    {
                        minEntropyCell = new Vector2Int(i % outputSize.x, i / outputSize.y);
                        minEntropy = entropy;
                    }
                }
            }

            await Task.Yield();
        }

        // sets up level objects
        GetComponent<AnalyseMap>().Analyse(outputSize, outputMap);
        return true;
    }

    void SetBigTile(int startX, int startY, BigTile tile)
    {
        // places the tile on the visible grid
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                outputMap.SetTile(new Vector3Int(startX + (x-1), startY + (y-1), 0), tiles[tile.tiles[3*x + y]]);
            }
        }
    }

    bool PropagateNew(int x, int y, List<int> removedTiles)
    {
        // tells the surrounding cells in each direction the change that occured
        if (x + 1 < outputSize.x)
        {
            if (!UpdateCellDataNew(x + 1, y, x, y, direction.left, removedTiles))
            {
                return false;
            }
        }
        if (x - 1 >= 0)
        {
            if (!UpdateCellDataNew(x - 1, y, x, y, direction.right, removedTiles))
            {
                return false;
            }
        }
        if (y + 1 < outputSize.y)
        {
            if (!UpdateCellDataNew(x, y + 1, x, y, direction.down, removedTiles))
            {
                return false;
            }
        }
        if (y - 1 >= 0)
        {
            if (!UpdateCellDataNew(x, y - 1, x, y, direction.up, removedTiles))
            {
                return false;
            }
        }
        return true;
    }

    bool UpdateCellDataNew(int x, int y, int startX, int startY, direction source, List<int> removedTiles)
    {
        CellDataNew cell = cells[y * outputSize.x + x];

        // if this cell has been placed, this change can be ignored
        if (cell.placed)
        {
            return true;
        }

        bool changed = false;

        List<TileCellSideData> tilesToRemove = new List<TileCellSideData>();

        // reduces the domain of this cell based on the change
        for (int i = 0; i < removedTiles.Count; i++)
        {
            // for each tile that was removed, check it against all tiles in this cells domain, reducing each tile sides count
            // if the removed tile could have been placed next to it, if a tiles side count ever reaches 0, that tile is no longer
            // possible and is removed from the domain
            for (int j = 0; j < cell.domain.Count; j++)
            {
                switch (source)
                {
                    case direction.up:
                        if (bigTiles[cell.domain[j].index].upAdjacencies.Contains(removedTiles[i]))
                        {
                            cell.domain[j].upCount--;
                            if (cell.domain[j].upCount <= 0)
                            {
                                tilesToRemove.Add(cell.domain[j]);
                                changed = true;
                            }
                        }
                        break;

                    case direction.down:
                        if (bigTiles[cell.domain[j].index].downAdjacencies.Contains(removedTiles[i]))
                        {
                            cell.domain[j].downCount--;
                            if (cell.domain[j].downCount <= 0)
                            {
                                tilesToRemove.Add(cell.domain[j]);
                                changed = true;
                            }
                        }
                        break;

                    case direction.left:
                        if (bigTiles[cell.domain[j].index].leftAdjacencies.Contains(removedTiles[i]))
                        {
                            cell.domain[j].leftCount--;
                            if (cell.domain[j].leftCount <= 0)
                            {
                                tilesToRemove.Add(cell.domain[j]);
                                changed = true;
                            }
                        }
                        break;

                    case direction.right:
                        if (bigTiles[cell.domain[j].index].rightAdjacencies.Contains(removedTiles[i]))
                        {
                            cell.domain[j].rightCount--;
                            if (cell.domain[j].rightCount <= 0)
                            {
                                tilesToRemove.Add(cell.domain[j]);
                                changed = true;
                            }
                        }
                        break;

                }
            }
        }

        // removes all the tiles that are no longer possible for this cell to be
        for (int i = 0; i < tilesToRemove.Count; i++)
        {
            cell.domain.Remove(tilesToRemove[i]);
        }

        // if a change has occured, relay this change to all surrounding tiles
        if (changed)
        {
            if (cell.domain.Count == 0)
            {
                //print("Failed");
                seed += 1;
                return false;
            }
            // places the cell if there is only one tile that can still be placed here
            if (cell.domain.Count == 1)
            {
                cell.placed = true;
                SetBigTile(x, y, bigTiles[cell.domain[0].index]);
            }
            // else recalculates entropy
            else
            {
                float entropy = cell.domain.Count;

                if (entropy < minEntropy)
                {
                    minEntropyCell = new Vector2Int(x, y);
                    minEntropy = entropy;
                }
            }

            // creates a record of the tiles removed
            List<int> removedTilesNew = new List<int>();
            for (int i = 0; i < tilesToRemove.Count; i++)
            {
                removedTilesNew.Add(tilesToRemove[i].index);
            }
            return PropagateNew(x, y, removedTilesNew);
        }
        return true;
    }
}
