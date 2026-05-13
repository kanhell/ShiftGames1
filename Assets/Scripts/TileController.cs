using UnityEngine;

public class TileController : MonoBehaviour
{
    public TileData tileData;

    void Start()
    {
        GameManager.instance.tileData = tileData;
        PlayerController.instance.ChangePosX(GameManager.instance.playerPosX);
        CameraController.instance.ChangePosX(GameManager.instance.cameraPosX);
    }

}
