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
    public LayerMask NPClayer;
    public Collider[] NPCcolliders;

    // 구조물
    public LayerMask structureLayer;
    public Collider[] structureColliders;

    // 안내창
    public GameObject f_npc;
    public GameObject f_con;
    public GameObject noMoreMap;

    void Update()
    {
        // 이동
        x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        transform.Translate(new Vector2(x, 0));

        // 화면 끝 -> Scene 전환
        if (transform.position.x > GameManager.instance.limitMax)
        {
            if (GameManager.instance.sceneIdx == GameManager.instance.scenes.Length)
                noMoreMap.SetActive(true);
            else SceneManager.LoadScene(GameManager.instance.scenes[++GameManager.instance.sceneIdx]);

        }
        else if (transform.position.x < GameManager.instance.limitMin)
        {
            if (GameManager.instance.sceneIdx == 0)
                noMoreMap.SetActive(true);
            else SceneManager.LoadScene(GameManager.instance.scenes[--GameManager.instance.sceneIdx]);
        }
        else noMoreMap.SetActive(false);

        // 근처 npc : 걍 무조건 하나라고 가정했음.
        NPCcolliders = Physics.OverlapSphere(transform.position, radius, NPClayer);
        if (NPCcolliders.Length >0 ) f_npc.SetActive(true);
        else f_npc.SetActive(false);

        // 근처 구조물
        structureColliders = Physics.OverlapSphere(transform.position, radius, structureLayer);
        if (structureColliders.Length > 0) f_con.SetActive(true);
        else f_con.SetActive(false);



        // f키 누르면
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (NPCcolliders.Length > 0) ToDialogScene(NPCcolliders[0].name);
            if (structureColliders.Length > 0) ToDialogScene(structureColliders[0].name);
        }
    }

    public void ToDialogScene(string name)
    {
        GameManager.instance.setVaribles_DialogScene(
            dialogKey: "dummy script",
            npcKey: name,
            preDialog_Scene: SceneManager.GetActiveScene().name,
            x: transform.position.x
            );
        SceneManager.LoadScene("DialogScene");
    }

}
