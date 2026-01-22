using Unity.VisualScripting;
using UnityEngine;

public class DummyButton : MonoBehaviour
{
    public void CreateOptions()
    {
        int cnt = 3;
        string[] msgs = new string[cnt];
        msgs[0] = "1st option";
        msgs[1] = "2nd option";
        msgs[2] = "3rd option";
        DialogController.instance.MakeOptions(msgs);
    }

    public void ContinueDialogs()
    {
        DialogController.instance.ContinueDialog();
    }
}