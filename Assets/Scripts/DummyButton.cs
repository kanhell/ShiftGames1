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
        Debug.Log("clicked");
        GameManager.instance.setVaribles_DialogScene("dummy script");
        SceneManager.LoadScene("DialogScene");
    }

}