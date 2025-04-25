using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TestData", menuName = "TestData")]
public class TestData : ScriptableObject
{
    public List<Vector3> data;
}
