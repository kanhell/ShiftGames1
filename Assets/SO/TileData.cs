using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TIL_", menuName = "Map/TileData")]
public class TileData : ScriptableObject
{
    [Header("[Info]")]
    public string tileName;
    public string SceneName;

    [Header("[Size]")]
    public float MaxLeft;
    public float MaxRight;

    [Header("[LinkedTiles]")]
    public TileData leftTile;
    public TileData rightTile;
    public TileData topTile;
    public TileData bottomTile;

    [Header("[Sprites]")]
    public Sprite dawn;
    public Sprite day;
    public Sprite dusk;
    public Sprite night;
}


