using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // ½Ì±ÛÅæ
    public static GameManager instance;

    // Á¤º¸
    public TileData tileData;
    public DoorData doorData;
    public int tier;

    // DialogScene
    public DialogData DialogData;

    // Scene ÀÌµ¿
    public float playerPosX;
    public float cameraPosX;
    public Stack<insideTileData> tileStacks = new Stack<insideTileData>();

    void Awake()  // ½Ì±ÛÅæ
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void SwitchIn(int tier)  // tier´Â 5ÀÇ ¹è¼ö
    {
        this.tier = tier;

        float cameraPosZ = CameraController.instance.z;

        while (cameraPosZ < tier)
        {
            cameraPosZ += 0.1f;
            CameraController.instance.ChangePosZ(cameraPosZ);
            PlayerController.instance.ChangePosZ(cameraPosZ + 1);
        }
        CameraController.instance.ChangePosZ(tier);
        PlayerController.instance.ChangePosZ(tier + 1);
    }
    
    public void SwitchOut()
    {

    }
}
