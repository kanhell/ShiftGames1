using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerController : MonoBehaviour
{
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
    Collider[] DLGNColliders;
    Collider[] STCColliders;

    // manual
    public Canvas canvas;
    public TextMeshProUGUI textUI;
    NPCController obj;
    bool isCNV;

    private void Awake()
    {
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
        DLGNColliders = Physics.OverlapSphere(transform.position, Values.player_DLG_radius, NPCLayer);
        if (DLGNColliders.Length > 0)
        {
            obj = DLGNColliders[0].GetComponent<NPCController>();

            if (obj.npcData.isDialog)
            {
                isCNV = true;
                textUI.text = Values.manual_DLG;
                canvas.gameObject.SetActive(true);

                // f키 누르면
                if (Input.GetKeyDown(KeyCode.F))
                {
                    // TODO : 대화 ㄱ
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
