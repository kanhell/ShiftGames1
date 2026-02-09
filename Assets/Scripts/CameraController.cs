using UnityEngine;

public class CameraController : MonoBehaviour
{
    GameObject player;

    public float speed;

    float obj_x;
    float x;
    public float dis;

    public float width;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    private void Update()
    {
        obj_x = player.transform.position.x;
        x = transform.position.x;

        if (obj_x < x-dis && x-width > GameManager.instance.limitMin)  // 왼쪽으로 이동
            transform.Translate(new Vector2(-speed * Time.deltaTime, 0));
        if (obj_x > x+dis && x+width < GameManager.instance.limitMax)  // 오른쪽으로 이동
            transform.Translate(new Vector2(speed * Time.deltaTime, 0));
    }

    public void setInitialPos(float x)
    {
        transform.position = new Vector3(x, 0, -10);
    }
}
