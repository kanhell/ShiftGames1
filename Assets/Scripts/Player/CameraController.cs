using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    float obj_x;
    float x;

    public TileData tile;

    float speed = Values.camera_speed;
    float dis = Values.camera_maxDis;
    float camera_width = Values.camera_width;  // 대충 이정도 됨

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        obj_x = player.transform.position.x;
        x = transform.position.x;

        if (obj_x < x-dis && x-camera_width > tile.MaxLeft)  // 왼쪽으로 이동
            transform.Translate(new Vector2(-speed * Time.deltaTime, 0));
        if (obj_x > x+dis && x+camera_width < tile.MaxRight)  // 오른쪽으로 이동
            transform.Translate(new Vector2(speed * Time.deltaTime, 0));
    }
}
