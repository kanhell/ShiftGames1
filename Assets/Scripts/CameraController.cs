using UnityEngine;

public class CameraController : MonoBehaviour
{
    // ΩÃ±€≈Ê
    public static CameraController instance;

    public GameObject player;
    float obj_x;
    float x;
    public float z;

    float speed = Values.camera_speed;
    float dis = Values.camera_maxDis;
    float camera_width = Values.camera_width;

    void Awake()  // ΩÃ±€≈Ê
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        obj_x = player.transform.position.x;
        x = transform.position.x;

        if (obj_x < x-dis && x-camera_width > GameManager.instance.tileData.MaxLeft)  // øﬁ¬ ¿∏∑Œ ¿Ãµø
            transform.Translate(new Vector2(-speed * Time.deltaTime, 0));
        if (obj_x > x+dis && x+camera_width < GameManager.instance.tileData.MaxRight)  // ø¿∏•¬ ¿∏∑Œ ¿Ãµø
            transform.Translate(new Vector2(speed * Time.deltaTime, 0));
    }

    public void ChangePosX(float x)
    {
        this.x = x;
        transform.position = new Vector3(x, 0, z);
    }

    public void ChangePosZ(float z)
    {
        this.z = z;
        transform.position = new Vector3(transform.position.x, 0, z);
    }

}
