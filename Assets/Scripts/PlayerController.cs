using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // 싱글톤
    public static PlayerController instance;

    // 이동
    Rigidbody2D rb;
    float x;
    float speed = Values.player_speed;
    public float y;
    public float z;

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

        rb = GetComponent<Rigidbody2D>();

    }


    void Update()
    {
        isManual = false;
        MoveTile();
        if (!isManual)
            canvas.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        // 이동
        x = Input.GetAxis("Horizontal") * speed;
        rb.linearVelocity = new Vector2(x, 0);
    }

    void MoveTile()
    {
        string direction = "";
        bool isEnd = true;
        // tile 이동
        if (transform.position.x > GameManager.instance.tileData.MaxRight)
        {
            ChangePosX(GameManager.instance.tileData.MaxRight);
            targetTileData = GameManager.instance.tileData.rightTile;
            direction = "right";
        }
        else if (transform.position.x < GameManager.instance.tileData.MaxLeft)
        {
            ChangePosX(GameManager.instance.tileData.MaxLeft);
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
    
        
    private void OnTriggerEnter2D(Collider2D collision)  // MON, CNV 표시
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("npc"))
        {
            StartCoroutine(collision.gameObject.GetComponent<NPCController>().ShowMON());

        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("conversation"))
        {
            StartCoroutine(collision.gameObject.GetComponent<ConversationController>().showCNV());
        }
    }



    private void OnCollisionEnter2D(Collision2D collision)  // DLG, DOR, STC -> manual로 표시
{
        if (collision.gameObject.layer == LayerMask.NameToLayer("npc"))
        {
            isManual = true;
            obj = collision.gameObject.GetComponent<NPCController>();

            if (obj.npcData.DialogData != null)
            {
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
        else if (collision.gameObject.layer == LayerMask.NameToLayer("door"))
        {
            textUI.text = Values.manual_DOR;
            canvas.gameObject.SetActive(true);

            // f키 누르면
            if (Input.GetKeyDown(KeyCode.F))
            {
                GameManager.instance.doorData = collision.gameObject.GetComponent<DoorController>().doorData;
                GameManager.instance.SwitchIn(GameManager.instance.tier + 5);
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("structure"))
        {
            textUI.text = Values.manual_STC;
            canvas.gameObject.SetActive(true);

            StructureController obj = collision.gameObject.GetComponent<StructureController>();

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
            isManual = false;
            canvas.gameObject.SetActive(false);
        }

    }



    public void ChangePosX(float x)
    {
        this.x = x;
        transform.position = new Vector3(x, y, z);
    }

    public void ChangePosZ(float z)
    {
        this.z = z;
        transform.position = new Vector3(x, y, z);
    }
}
