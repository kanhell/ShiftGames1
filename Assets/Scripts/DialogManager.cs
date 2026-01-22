using NUnit.Framework.Constraints;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    // 싱글톤
    public static DialogManager instance;

    // dialog repository
    Dictionary<string, string[]> dialogs = new Dictionary<string, string[]>();
    Dictionary<string, string[]> options = new Dictionary<string, string[]>();
    Dictionary<string, string[]> keys = new Dictionary<string, string[]>();
    string dialogKey = "dummy script";  // dummy
    int dialogIndex = -1;  // dummy
    string endKey = "대화종료";

    void Awake()  // 싱글톤
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()  // dummy
    {

        // 최초 대화
        dialogs.Add(
            "dummy script",
            new string[]
            {
                "어서 와. 이 시간에 여행자는 드문데… 무슨 일이지?"
            }
        );

        options.Add(
            "dummy script",
            new string[]
            {
                "이 마을에 대해 좀 물어봐도 될까요?",
                "특별히 수상한 일은 없었나요?"
            }
        );

        keys.Add(
            "dummy script",
            new string[]
            {
                "dummy script_option_1",
                "dummy script_option_2"
            }
        );

        // 선택지 1번 이후 대화
        dialogs.Add(
            "dummy script_option_1",
            new string[]
            {
                "이 마을 말이지… 겉보기엔 조용해 보여도 그렇지만은 않아.",
                "북쪽 숲에는 아무도 잘 가지 않지.",
                "이유를 굳이 묻는 사람도 없고 말이야."
            }
        );

        options.Add(
            "dummy script_option_1",
            new string[]
            {
                "왜 아무도 안 가죠?",
                "알겠습니다. 조심할게요."
            }
        );

        keys.Add(
            "dummy script_option_1",
            new string[]
            {
                "밤이 되면 숲에서 이상한 소리가 들린다고들 해.",
                endKey
            }
        );

        // 선택지 1-1번 이후
        dialogs.Add(
            "dummy script_option_1_1",
            new string[]
            {
                "밤이 되면 숲에서 이상한 소리가 들린다고들 해.",
                "믿든 말든 네 자유지만 말이야.",
                endKey
            }
        );

        // 선택지 2번 이후 대화
        dialogs.Add(
            "dummy script_option_2",
            new string[]
            {
                "그런 질문은 보통 여기서 안 하지.",
                "(주변을 힐끗 살핀다)",
                "하지만… 최근 사람들이 하나둘 사라지고 있어."
            }
        );

        options.Add(
            "dummy script_option_2",
            new string[]
            {
                "사라졌다고요?",
                "괜한 질문을 했군요."
            }
        );

        keys.Add(
            "dummy script_option_2",
            new string[]
            {
                "dummy script_option_2_1",
                endKey
            }
        );

        // 선택지 2-1번 이후
        dialogs.Add(
            "dummy script_option_2_1",
            new string[]
            {
                "마지막으로 목격된 곳은 늘 같아.",
                "북쪽 숲 근처지.",
                endKey
            }
        );

    }

    public void OptionSelected(int idx)
    {
        dialogKey = keys[dialogKey][idx];
        dialogIndex = -1;
        ContinueDialog();
    }

    public void ContinueDialog()
    {
        dialogIndex += 1;
        string msg = dialogs[dialogKey][dialogIndex];
        if (msg == endKey)
            // 대화종료
            // 함수 종료
        DialogController.instance.ChangeDialog(msg);
        if (dialogIndex == dialogs[dialogKey].Length-1)
            DialogController.instance.MakeOptions(options[dialogKey]);
    }

    // TODO : repository for dialogs and options
    // TODO : maybe gonna have functions for adjust states of player or something
    // TODO : save dialog progression
}
