using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerController : MonoBehaviour
{
    float x;
    public float speed;

    public float radius = 0f;
    public LayerMask layer;
    public Collider[] colliders;
    public GameObject f;

    // Update is called once per frame
    void Update()
    {
        // 이동
        x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        transform.Translate(new Vector3(x, 0, 0));

        // 근처 npc : 걍 무조건 하나라고 가정했음.
        colliders = Physics.OverlapSphere(transform.position, radius, layer);
        if (colliders.Length >0 ) f.SetActive(true);
        else f.SetActive(false);
            


        if (Input.GetKeyDown(KeyCode.F) && colliders.Length >0)
        {
            ToDialogScene(colliders[0].name);
        }
    }

    public void ToDialogScene(string name)
    {
        GameManager.instance.setVaribles_DialogScene("dummy script", name);
        GameManager.instance.preDialog_Scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("DialogScene");
    }
}
