using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LightingManager : MonoBehaviour
{
    [SerializeField] Tilemap shadowMap;
    List<RoomData> rooms;
    [SerializeField] Tile blackTile;
    [SerializeField] Tile wallTile;
    [SerializeField] Tilemap map;

    int currentRoom = 0;

    Vector2Int prevPosition = new Vector2Int();

    List<ILevelObject> objects = new List<ILevelObject>();

    public void SetRooms(List<RoomData> _rooms, int playerRoom)
    {
        rooms = _rooms;
        currentRoom = playerRoom;

        // lights up the room the player spawns in
        FillRoom(rooms[playerRoom], true);

        // Hides all level objects
        var a = FindObjectsOfType<MonoBehaviour>().OfType<ILevelObject>();
        foreach (ILevelObject obj in a)
        {
            obj.Deactivate();
            objects.Add(obj);
        }

        // shows all level objects not hidden by the shadow map
        foreach (ILevelObject obj in objects)
        {
            if (shadowMap.GetTile(obj.GetPos()) == null)
            {
                obj.Activate();
            }
            else
            {
                obj.Deactivate();
            }
        }
    }

    public void UpdateLighting(int x, int y)
    {
        // if the player is marked as being in a room
        if (currentRoom != -1)
        {
            // if the player is no longer in a room, darken that room
            if (!InRoom(x, y, rooms[currentRoom]))
            {
                FillRoom(rooms[currentRoom], false);
                currentRoom = -1;
            }
        }
        // else checks if the player is any room, if they are, light up the room
        else
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (InRoom(x, y, rooms[i]))
                {
                    currentRoom = i;
                    FillRoom(rooms[i], true);
                    break;
                }
            }
        }
        // darkens the area the player used to be in
        FillSquare(prevPosition.x, prevPosition.y, false);

        // lights up the area the player is now
        FillSquare(x, y, true);

        // marks the previous position
        prevPosition = new Vector2Int(x, y);

        // hides objects that are in the dark, shows objects that are in the light
        foreach (ILevelObject obj in objects)
        {
            if (shadowMap.GetTile(obj.GetPos()) == null)
            {
                obj.Activate();
            }
            else
            {
                obj.Deactivate();
            }
        }
    }

    public void FillRoom(RoomData room, bool on)
    {
        // sets the room to be lit up / darkened using a tilemap of translucent black
        for (int x = room.startPos.x-1; x <= room.startPos.x + room.size.x+1; x++)
        {
            for (int y = room.startPos.y-1; y <= room.startPos.y + room.size.y+1; y++)
            {
                if (on)
                {
                    shadowMap.SetTile(new Vector3Int(x, y), null);
                }
                else
                {
                    shadowMap.SetTile(new Vector3Int(x, y), blackTile);
                }
            }
        }
    }

    public void FillSquare(int xStart, int yStart, bool on)
    {
        // lights up / darkens tiles within 2 tiles of the position, given that the tile is not a wall
        if (on)
        {
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) < 3)
                    {
                        if (map.GetTile(new Vector3Int(xStart + x, yStart + y)) != wallTile)
                        {
                            shadowMap.SetTile(new Vector3Int(xStart + x, yStart + y), null);
                        }
                    }
                }
            }
        }
        else
        {
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) < 3)
                    {
                        if (currentRoom == -1 || !InRoom(xStart + x, yStart + y, rooms[currentRoom]))
                        {
                            shadowMap.SetTile(new Vector3Int(xStart + x, yStart + y), blackTile);
                        }
                    }
                }
            }
        }
    }

    public bool InRoom(int x, int y, RoomData room)
    {
        // checks if the player is within the bounds of the room
        if (x >= room.startPos.x-1 && x <= room.startPos.x + room.size.x+1 &&
            y >= room.startPos.y-1 && y <= room.startPos.y + room.size.y+1)
        {
            return true;
        }
        return false;
    }
}
