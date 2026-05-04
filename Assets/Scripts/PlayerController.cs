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

    // Scene
    public TileData targetTileData;
    float tileGap = 2f;

    // manual
    NPCController obj;
    bool isManual;

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

        isManual = false;
        MoveTile();
        Interaction();
        if (!isManual)
            canvas.gameObject.SetActive(false);

    }

    void MoveTile()
    {
        string direction = "";
        bool isEnd = true;
        // tile 이동
        if (transform.position.x > GameManager.instance.tileData.MaxRight)
        {
            ChangePos(GameManager.instance.tileData.MaxRight);
            targetTileData = GameManager.instance.tileData.rightTile;
            direction = "right";
        }
        else if (transform.position.x < GameManager.instance.tileData.MaxLeft)
        {
            ChangePos(GameManager.instance.tileData.MaxLeft);
            targetTileData = GameManager.instance.tileData.leftTile;
            direction = "left";
        }
        else
            isEnd = false;
        // TODO : 위, 아래


        if (isEnd)
        {
            Debug.Log("player reached " + direction);

            if (targetTileData == null)
            {
                isManual = true;
                textUI.text = Values.manual_noLoad;
                canvas.gameObject.SetActive(true);
            }
            else if (targetTileData.tileName == Values.out_TileData_tileName)
            {
                insideTileData insideTileData = GameManager.instance.tileStacks.Pop();
                GameManager.instance.playerPosX = insideTileData.playerPosX;
                GameManager.instance.cameraPosX = insideTileData.cameraPosX;
                SceneManager.LoadScene(insideTileData.preTile.SceneName);
            }
            else
            {
                CalandSetPos(direction);
                SceneManager.LoadScene(targetTileData.SceneName);
            }
        }


    }

    void CalandSetPos(string direction)
    {
        if (direction == "right")
        {
            GameManager.instance.playerPosX = targetTileData.MaxLeft + tileGap;
            GameManager.instance.cameraPosX = targetTileData.MaxLeft + Values.camera_width;
        }
        else if (direction == "left")
        {
            GameManager.instance.playerPosX = targetTileData.MaxRight - tileGap;
            GameManager.instance.cameraPosX = targetTileData.MaxRight - Values.camera_width;
        }
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
        if (!isManual)
        {
            DLGColliders = Physics.OverlapSphere(transform.position, Values.player_DLG_radius, NPCLayer);
            if (DLGColliders.Length > 0)
            {
                obj = DLGColliders[0].GetComponent<NPCController>();

                if (obj.npcData.DialogData != null && obj.npcData.DialogState == obj.npcData.state)
                {
                    isManual = true;
                    textUI.text = Values.manual_DLG;
                    canvas.gameObject.SetActive(true);

                    // f키 누르면
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        Debug.Log("show Dialog");
                        GameManager.instance.DialogData = obj.npcData.DialogData;
                        GameManager.instance.playerPosX = transform.position.x;
                        GameManager.instance.cameraPosX = CameraController.instance.transform.position.x;
                        SceneManager.LoadScene("DialogScene");
                    }
                }
            }
        }
        if(!isManual)
        {
            STCColliders = Physics.OverlapSphere(transform.position, Values.player_STC_radius, STCLayer);
            if (STCColliders.Length > 0)
            {
                isManual = true;
                textUI.text = Values.manual_STC;
                canvas.gameObject.SetActive(true);

                StructureController obj = STCColliders[0].GetComponent<StructureController>();

                // f키 누르면
                if (Input.GetKeyDown(KeyCode.F))
                {
                    // 건물 들어가기
                    targetTileData = obj.tileData;
                    GameManager.instance.tileStacks.Push(new insideTileData(GameManager.instance.tileData, transform.position.x, CameraController.instance.transform.position.x));
                    GameManager.instance.tileData = targetTileData;
                    CalandSetPos(obj.direction);
                    SceneManager.LoadScene(targetTileData.SceneName);
                }
            }
            else
            {
                canvas.gameObject.SetActive(false);
            }

        }
    }

    public void ChangePos(float x)
    {
        transform.position = new Vector3(x, Values.player_posY, 0);
    }

}
