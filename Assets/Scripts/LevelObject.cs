using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILevelObject
{
    public void Activate();

    public void Deactivate();

    public Vector3Int GetPos();
}
