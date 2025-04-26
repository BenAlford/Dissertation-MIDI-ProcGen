using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.Tilemaps;

public class CellData
{
    public List<int> domain = new List<int>();
    public bool placed = false;

    // a list for each direction, the tiles that can be adjacent, calculated at the start
    // updated everytime the domain is updated, the removed tile number is passed through to this and removed from that list
}

public class ProcGenTest : MonoBehaviour
{
    enum direction
    {
        up,down,left,right,none
    }
    public List<TileLinks> tiles;
    public Vector2Int size;
    public long seed;
    public Tilemap map;

    List<CellData> cells;

    float min_entropy = 0;
    Vector2Int min_entropy_cell = new Vector2Int();

    int seed_divider = 1;

    // Start is called before the first frame update
    void Start()
    {
        // creates the data structure for the cells
        int total_size = size.x * size.y;
        cells = new List<CellData>();
        for (int i = 0; i < total_size; i++)
        {
            CellData default_cell_data = new CellData();
            for (int j = 0; j < tiles.Count; j++)
            {
                default_cell_data.domain.Add(j);
            }
            cells.Add(default_cell_data);
        }

        // creates a ring of wall around the designated area
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                if (j == 0 || j == size.y - 1 || i == 0 || i == size.x - 1)
                {
                    cells[j * size.x + i].placed = true;
                    cells[j * size.x + i].domain = new List<int> { 3 };
                    map.SetTile(new Vector3Int(i, j, 0), tiles[cells[j * size.x + i].domain[0]].tile);
                    Propagate(i, j);
                }
            }
        }

        // chooses a random tile to place first
        int start_tile = (int)(seed % total_size);
        int start_x = start_tile % size.x;
        int start_y = start_tile / size.y;
        min_entropy_cell = new Vector2Int(start_x, start_y);

        // while there are still tiles to place
        while (min_entropy != 9999)
        {
            min_entropy = 9999;
            int min_entropy_cell_pos = min_entropy_cell.y * size.x + min_entropy_cell.x;

            // calculates which tile to place based on probability
            List<int> test = new List<int>();
            for (int i = 0; i < cells[min_entropy_cell_pos].domain.Count; i++)
            {
                for (int j = 0; j < tiles[cells[min_entropy_cell_pos].domain[i]].probability; j++)
                {
                    test.Add(i);
                }
            }

            int tile = test[(int)((seed / seed_divider) % test.Count)];

            // places the tile and mark that cell as placed
            cells[min_entropy_cell_pos].placed = true;
            cells[min_entropy_cell_pos].domain = new List<int> { cells[min_entropy_cell_pos].domain[tile] };

            // sets the tile on the visible grid and relay the change to the neighbouring cells
            map.SetTile(new Vector3Int(min_entropy_cell.x, min_entropy_cell.y, 0), tiles[cells[min_entropy_cell_pos].domain[0]].tile);
            Propagate(min_entropy_cell.x, min_entropy_cell.y);

            // calculates the cell that would make the least impact if placed (minimum entropy) to place next
            for (int i = 0; i < cells.Count; i++)
            {
                CellData cell = cells[i];
                if (!cell.placed)
                {
                    float entropy = 0;
                    for (int j = 0; j < cell.domain.Count; j++)
                    {
                        entropy += tiles[cell.domain[j]].probability * Mathf.Log10(tiles[cell.domain[j]].probability);
                    }
                    if (entropy < min_entropy)
                    {
                        min_entropy_cell = new Vector2Int(i % size.x, i / size.y);
                        min_entropy = entropy;
                    }
                }
            }
            seed_divider++;
        }
    }

    void Propagate(int x, int y)
    {
        // relays the change to each adjacent cell in the 4 cardinal direction
        if (x+1 < size.x)
            UpdateCellData(x+1, y, x, y, direction.left);
        if (x-1 >= 0)
            UpdateCellData(x-1, y, x, y, direction.right);
        if (y+1 < size.y)
            UpdateCellData(x, y+1, x, y, direction.down);
        if (y-1 >= 0)
            UpdateCellData(x, y-1, x, y, direction.up);
    }

    void UpdateCellData(int x, int y, int start_x, int start_y, direction source)
    {
        CellData cell = cells[y * size.x + x];

        // ignore if the cell is placed as this would have no effect
        if (cell.placed)
        {
            return;
        }

        // gets the cell data for the changed cell
        CellData sourceCell = cells[start_y * size.x + start_x];
        bool changed = false;

        List<int> tiles_to_remove = new List<int>();

        // for each tile in this cells domain, calculate if there is a cell in the changed cells domain that can be placed
        // ajacent, if there is not, this tile is now impossible, so remove it from the domain
        for (int i = 0; i < cell.domain.Count; i++)
        {
            bool matched = false;
            for (int j = 0; j < sourceCell.domain.Count; j++)
            {
                // get the type of tile side each tile can be placed next to, based on direction
                linkType source_link = linkType.open;
                List<linkType> this_links = new List<linkType>();
                switch (source)
                {
                    case direction.up:
                        source_link = tiles[sourceCell.domain[j]].bottom;
                        this_links = tiles[cell.domain[i]].top_link_to;
                        break;
                    case direction.down:
                        source_link = tiles[sourceCell.domain[j]].top;
                        this_links = tiles[cell.domain[i]].bottom_link_to;
                        break;
                    case direction.left:
                        source_link = tiles[sourceCell.domain[j]].right;
                        this_links = tiles[cell.domain[i]].left_link_to;
                        break;
                    case direction.right:
                        source_link = tiles[sourceCell.domain[j]].left;
                        this_links = tiles[cell.domain[i]].right_link_to;
                        break;
                }

                // if these tiles can link, this cells tile can still be placed, so don't remove it
                for (int k = 0; k < this_links.Count; k++)
                {
                    if (source_link == this_links[k])
                    {
                        matched = true;
                        break;
                    }
                }
                if (matched)
                {
                    break;
                }
            }

            // if there was no match, no tiles in the changed cells domain can have this tile next to them, so remove it
            if (!matched)
            {
                //tile impossible, remove from domain
                changed = true;

                tiles_to_remove.Add(cell.domain[i]);
            }
        }

        // removes the marked tiles from the domain
        for (int i = 0; i < tiles_to_remove.Count; i++)
        {
            cell.domain.Remove(tiles_to_remove[i]);
        }

        // if this cells domain has changed, relay this to this cells neighbours
        if (changed)
        {
            // if there is only one tile left, place this tile
            if (cell.domain.Count == 1)
            {
                cell.placed = true;
                map.SetTile(new Vector3Int(x, y, 0), tiles[cell.domain[0]].tile);
            }
            Propagate(x, y);
        }
    }

    public bool canWalk(int x, int y)
    {
        // if the player tries to move onto a walkable tile, return true, else, return false
        if (x >=0 && x < size.x &&  y >=0 && y < size.y)
            return tiles[cells[y * size.x + x].domain[0]].walkable;
        return false;
    }
}
