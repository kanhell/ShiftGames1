using TMPro.Examples;
using UnityEngine;

public class BackgroundInfo : MonoBehaviour
{
    public float limitMin, limitMax;
    public GameObject player;
    public GameObject camera;
    CameraController camera_controller;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        player.transform.position = new Vector2(GameManager.instance.toScene.x, -2);
        GameManager.instance.limitMin = limitMin;
        GameManager.instance.limitMax = limitMax;

        camera = GameObject.FindWithTag("MainCamera");
        camera_controller = camera.GetComponent<CameraController>();
        camera_controller.setInitialPos(player.transform.position.x);

    }

}
