using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class DialogController : MonoBehaviour
{
    // 싱글톤
    public static DialogController instance;

    // text
    public TextMeshProUGUI text_dialog;
    bool isDialog = false;
    public Image img_OptionsBackground;
    Color32 optionsBackground = new Color32(255, 248, 220, 255);
    Color32 invisible = new Color32(0,0,0,0);

    // image
    public GameObject npc;
    public GameObject background;

    // options
    public GameObject optionPrefab;
    public GameObject[] arr_options;
    bool isOptions = false;

    // options - position
    public int y_top;
    public int y_bottom;
    public int x;


    void Awake()  // 싱글톤
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        npc.transform.GetComponent<Image>().sprite = GameManager.instance.images[GameManager.instance.toDialog.npcKey].frontshot;
        background.transform.GetComponent<Image>().sprite = GameManager.instance.images[GameManager.instance.toDialog.npcKey].background;
        DialogStart(GameManager.instance.toDialog.dialogKey);
    }

    void Update()
    {
        // continueDialog
        if (isDialog && (Input.GetAxisRaw("Horizontal") == 1 || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0)))
            ContinueDialog();
    }

    public void ChangeDialog(string msg)  // 대화 진행할 때, 메시지 내용 바꾸는 용도
    {
        text_dialog.text = msg;
    }

    public void MakeOptions(string[] msgs)  // 선택지 필요시 UI적으로 표시
    {
        img_OptionsBackground.color = optionsBackground;
        int cnt = msgs.Length;
        isOptions = true;
        Transform canvas = GameObject.Find("Canvas").transform;
        float dis = (y_top - y_bottom) / cnt;
        float y = y_top - (dis / 2);
        arr_options = new GameObject[cnt];
        for (int i = 0; i < cnt; i++)
        {
            GameObject tmp = Instantiate(optionPrefab, canvas);
            tmp.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            tmp.GetComponent<OptionController>().Setter(i, msgs[i]);
            y -= dis;
            arr_options[i] = tmp;
        }
    }

    public void ContinueDialog()
    {
        if (!isOptions)
        {
            DialogManager.instance.ContinueDialog();
        }
    }

    public void OptionSelected(int idx)  // 선택지 선택시 호출됨
    {
        isOptions = false;
        img_OptionsBackground.color = invisible;
        // options 삭제
        for (int i = 0; i < arr_options.Length; i++)
            Destroy(arr_options[i]);
        DialogManager.instance.OptionSelected(idx);
    }

    public void DialogStart(string dialogKey)
    {
        isDialog = true;
        DialogManager.instance.dialogKey = dialogKey;
        DialogManager.instance.dialogIndex = -1;
        ContinueDialog();
    }

    public void DialogStop()
    {
        SceneManager.LoadScene(GameManager.instance.toDialog.preDialog_Scene);
        Destroy(DialogManager.instance.gameObject);
        Destroy(DialogController.instance.gameObject);
    }



    // 글씨만 ""로 바꾸는게 아니라 실제
    // TODO 함수 : 대화가 종료된다면 dialog disappear
    // TODO 함수 : 대화가 시작될 때의 상황을 dialogKey로 바꿔주기
}
