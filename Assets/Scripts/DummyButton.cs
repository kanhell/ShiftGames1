using Unity.VisualScripting;
using UnityEngine;

public class DummyButton : MonoBehaviour
{
    public void OnButtonClick()
    {
        int cnt = 3;
        string[] msg = new string[cnt];
        msg[0] = "1st option";
        msg[1] = "2nd option";
        msg[2] = "3rd option";
        DialogManager.instance.MakeOptions(cnt, msg);
    }
}
