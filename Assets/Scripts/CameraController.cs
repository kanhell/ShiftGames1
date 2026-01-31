using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    public float speed;

    float obj_x;
    float x;

    private void Update()
    {
        obj_x = player.transform.position.x;
        x = transform.position.x;

        if (obj_x < x-4)
            transform.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
        if (obj_x > x+4)
            transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
    }
}
