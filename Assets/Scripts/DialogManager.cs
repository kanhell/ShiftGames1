using TMPro;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class DialogManager : MonoBehaviour
{
    // 싱글톤
    public static DialogManager instance;

    // 
    public TextMeshProUGUI text_dialog;

    // options
    public GameObject optionPrefab;
    public GameObject[] arr_options;

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

    public void MakeOptions(int cnt, string[] msg)  // 선택지 필요시 UI적으로 표시
    {
        Transform canvas = GameObject.Find("Canvas").transform;
        float dis = (y_top - y_bottom) / cnt;
        float y = y_top - (dis / 2);
        for (int i = 0; i < cnt; i++)
        {
            GameObject tmp = Instantiate(optionPrefab, canvas);
            tmp.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            tmp.GetComponent<OptionController>().Setter(i, msg[i]);
            y -= dis;
        }
    }

    public void OptionSelected(int idx)  // 선택지 선택시 호출됨
    {
        // nothing
        // 아마 dialog 바꾸고 스토리 진행하지 않을까
    }
}
