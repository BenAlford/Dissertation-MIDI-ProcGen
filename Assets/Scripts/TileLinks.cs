using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum linkType {
    open,
    closed,
    black,
    water,
    water_open
};
[CreateAssetMenu(fileName = "tilelink", menuName = "tilelink")]
public class TileLinks : ScriptableObject
{

    public TileBase tile;
    public linkType top;
    public linkType bottom;
    public linkType left;
    public linkType right;
    public List<linkType> top_link_to;
    public List<linkType> bottom_link_to;
    public List<linkType> left_link_to;
    public List<linkType> right_link_to;
    public int probability = 1;

    public bool walkable = true;
}
