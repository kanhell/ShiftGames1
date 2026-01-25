using Unity.VisualScripting;
using UnityEngine;

public class DummyButton : MonoBehaviour
{
    public void ContinueDialogs()
    {
        DialogController.instance.ContinueDialog();
    }

    public void DialogStart()
    {
        DialogController.instance.DialogStart("dummy script");
    }
}