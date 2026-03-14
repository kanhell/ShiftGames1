using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TIL_", menuName = "Map/TileData")]
public class TileData : ScriptableObject
{
    [Header("[Info]")]
    public string tileName;
    public string tileCode;
    public float MaxLeft;
    public float MaxRight;


    [Header("[Sprites]")]
    public Sprite dawn;
    public Sprite day;
    public Sprite dusk;
    public Sprite night;
}


