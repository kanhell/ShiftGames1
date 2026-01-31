using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DummyButton : MonoBehaviour
{
    public void ContinueDialogs()
    {
        DialogController.instance.ContinueDialog();
    }

    public void ToDialogScene()
    {
        GameManager.instance.setVaribles_DialogScene("dummy script", "macgregor");
        GameManager.instance.preDialog_Scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("DialogScene");
    }

}