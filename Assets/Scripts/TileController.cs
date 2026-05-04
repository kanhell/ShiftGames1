using UnityEngine;

public class TileController : MonoBehaviour
{
    public TileData tileData;

    void Start()
    {
        GameManager.instance.tileData = tileData;
        PlayerController.instance.ChangePos(GameManager.instance.playerPosX);
        CameraController.instance.ChangePos(GameManager.instance.cameraPosX);
    }

}
