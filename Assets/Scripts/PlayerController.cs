using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // 싱글톤
    public static PlayerController instance;

    // 이동
    float x;
    float speed = Values.player_speed;

    // layer
    public LayerMask NPCLayer;
    public LayerMask CNVLayer;
    public LayerMask STCLayer;

    // collider
    Collider[] MONColliders;
    Collider[] CNVColliders;
    Collider[] DLGColliders;
    Collider[] STCColliders;

    // canvas
    public Canvas canvas;
    public TextMeshProUGUI textUI;

    // manual
    NPCController obj;
    bool isCNV;

    void Awake()  // 싱글톤
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }


    void Update()
    {
        // 이동
        x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        transform.Translate(new Vector2(x, 0));

        // TODO: Scene 이동

        Interaction();

    }


    void Interaction()
    {

        // MON, CNV 표시
        MONColliders = Physics.OverlapSphere(transform.position, Values.player_MON_radius, NPCLayer);
        for (int i = 0; i < MONColliders.Length; i++)
            StartCoroutine(MONColliders[i].GetComponent<NPCController>().ShowMON());
        CNVColliders = Physics.OverlapSphere(transform.position, Values.player_CNV_radius, CNVLayer);
        for (int i = 0; i < CNVColliders.Length; i++)
            StartCoroutine(CNVColliders[i].GetComponent<ConversationController>().showCNV());


        // DLG, STC -> manual로 표시
        isCNV = false;
        DLGColliders = Physics.OverlapSphere(transform.position, Values.player_DLG_radius, NPCLayer);
        if (DLGColliders.Length > 0)
        {
            obj = DLGColliders[0].GetComponent<NPCController>();

            if (obj.npcData.DialogData != null && obj.npcData.DialogState == obj.npcData.state)
            {
                isCNV = true;
                textUI.text = Values.manual_DLG;
                canvas.gameObject.SetActive(true);

                // f키 누르면
                if (Input.GetKeyDown(KeyCode.F))
                {
                    Debug.Log("show Dialog");
                    GameManager.instance.DialogData = obj.npcData.DialogData;
                    SceneManager.LoadScene("DialogScene");
                }
            }
        }
        if(!isCNV)
        {
            STCColliders = Physics.OverlapSphere(transform.position, Values.player_STC_radius, STCLayer);
            if (STCColliders.Length > 0)
            {
                textUI.text = Values.manual_STC;
                canvas.gameObject.SetActive(true);

                // f키 누르면
                if (Input.GetKeyDown(KeyCode.F))
                {
                    // TODO : 건물 들어가기
                }
            }
            else
            {
                canvas.gameObject.SetActive(false);
            }

        }
    }
}
