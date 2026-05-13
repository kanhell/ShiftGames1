using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DOR_", menuName = "Map/DoorData")]
public class DoorData : ScriptableObject
{
    [Header("[Size]")]
    public float MaxLeft;
    public float MaxRight;

    [Header("[Organizations]")]
    public float DoorPos;
    // furnitures list : pos and type and what's inside

}


