using TMPro;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class DialogController : MonoBehaviour
{
    // 싱글톤
    public static DialogController instance;

    // 
    public TextMeshProUGUI text_dialog;

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


    public void ChangeDialog(string msg)  // 대화 진행할 때, 메시지 내용 바꾸는 용도
    {
        text_dialog.text = msg;
    }

    public void MakeOptions(string[] msgs)  // 선택지 필요시 UI적으로 표시
    {
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
        // options 삭제
        for (int i = 0; i < arr_options.Length; i++)
            Destroy(arr_options[i]);
        DialogManager.instance.OptionSelected(idx);
    }

    // TODO 함수 : 대화가 종료된다면 dialog disappear
    // TODO 함수 : 대화가 처음 시작할 떄 dialog appear
}
