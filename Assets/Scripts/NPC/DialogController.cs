using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogController : MonoBehaviour
{
    // 싱글톤
    public static DialogController instance;

    // dialogData
    DialogData dialogData = GameManager.instance.DialogData;
    DialogLine dialogLine;

    // GameObject
    public GameObject npc;
    public GameObject background;
    public TextMeshProUGUI text_dialog;
    public GameObject optionPrefab;

    // UI
    public Image img_OptionsBackground;
    public GameObject canvas;
    Color32 optionsBackground = new Color32(255, 248, 220, 255);
    Color32 invisible = new Color32(0, 0, 0, 0);

    // options
    int dialogIndex = -1;
    int maxIndex;
    bool isDialog = false;
    GameObject[] arr_options;
    string msg;
    string END = "END";



    void Awake()  // 싱글톤
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // sprite 설정
        npc.transform.GetComponent<Image>().sprite = dialogData.NPCData.dialog;
        background.transform.GetComponent<Image>().sprite = dialogData.background;
        // 첫 Dialog 시작
        DialogStart();
    }

    void Update()
    {
        // continueDialog
        if (isDialog && (Input.GetAxisRaw("Horizontal") == 1 || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0)))
            ContinueDialog();
    }


    void DialogStart()
    {
        isDialog = true;
        dialogLine = dialogData.DialogLines[dialogData.codes.IndexOf(dialogData.currentCode)];
        maxIndex = dialogLine.lines.Count;
        dialogIndex = -1;
        ContinueDialog();
    }


    void ContinueDialog()
    {
        dialogIndex += 1;
        msg = dialogLine.lines[dialogIndex];
        if (msg == END)  // 대화 종료
            DialogStop();
        else
        {
            text_dialog.text = msg;
            if (dialogIndex >= maxIndex -1)
                MakeOptions();
        }
    }


    void MakeOptions()  // 선택지 필요시 UI적으로 표시
    {
        isDialog = false;

        List<string> msgs = dialogLine.options;
        img_OptionsBackground.color = optionsBackground;
        int cnt = msgs.Count;

        float dis = (Values.dialog_optionPos_topY - Values.dialog_optionPos_bottomY) / cnt;
        float y = Values.dialog_optionPos_topY - (dis / 2);
        arr_options = new GameObject[cnt];
        for (int i = 0; i < cnt; i++)
        {
            GameObject tmp = Instantiate(optionPrefab, canvas.transform);
            tmp.GetComponent<RectTransform>().anchoredPosition = new Vector2(Values.dialog_optionPos_X, y);
            tmp.GetComponent<OptionController>().Setter(i, msgs[i]);
            y -= dis;
            arr_options[i] = tmp;
        }
    }

    public void OptionSelected(int idx)  // 선택지 선택시 호출됨
    {
        img_OptionsBackground.color = invisible;
        // options 삭제
        for (int i = 0; i < arr_options.Length; i++)
            Destroy(arr_options[i]);

        dialogData.currentCode = dialogLine.nextCode_byOption[idx];
        if (dialogData.currentCode == END)
            DialogStop();
        else
            DialogStart();
    }
        

    void DialogStop()
    {
        dialogData.currentCode = "00";
        SceneManager.LoadScene(GameManager.instance.tileData.SceneName);
    }
}
