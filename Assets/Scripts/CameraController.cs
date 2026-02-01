using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    public float speed;

    float obj_x;
    float x;
    public float dis;

    public float width;

    private void Update()
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        obj_x = player.transform.position.x;
        x = transform.position.x;

        if (obj_x < x-dis && x-width > controller.limitMin)  // 왼쪽으로 이동
            transform.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
        if (obj_x > x+dis && x+width < controller.limitMax)  // 오른쪽으로 이동
            transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
    }
}
