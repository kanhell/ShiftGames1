using Unity.VisualScripting;
using UnityEngine;

public class insideTileData
{
    public TileData preTile;
    public float playerPosX, cameraPosX;
    public insideTileData(TileData preTile, float playerPosX, float cameraPosX)
    {
        this.preTile = preTile;
        this.playerPosX = playerPosX;
        this.cameraPosX = cameraPosX;
    }
}
