using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerController : MonoBehaviour
{
    // 이동
    float x;
    public float speed;

    // 대화
    public float radius = 0f;
    public LayerMask layer;
    public Collider[] colliders;

    // 안내창
    public GameObject f;
    public GameObject noMoreMap;

    // Scene 속성
    public float limitMax, limitMin;

    void Update()
    {
        // 이동
        x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        transform.Translate(new Vector3(x, 0, 0));

        // 화면 끝 -> Scene 전환
        if (transform.position.x > limitMax)
        {
            if (GameManager.instance.sceneIdx == GameManager.instance.scenes.Length)
                noMoreMap.SetActive(true);
            else SceneManager.LoadScene(GameManager.instance.scenes[++GameManager.instance.sceneIdx]);  // 새로운 limitMax,limitMin 가져오기

        }
        else if (transform.position.x < limitMin)
        {
            if (GameManager.instance.sceneIdx == 0)
                noMoreMap.SetActive(true);
            else SceneManager.LoadScene(GameManager.instance.scenes[--GameManager.instance.sceneIdx]);
        }
        else noMoreMap.SetActive(false);

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
