using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ΩÃ±€≈Ê
    public static GameManager instance;

    // to DialogScene
    public string dialogKey = "";



    void Awake()  // ΩÃ±€≈Ê
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    

    public void setVaribles_DialogScene(string dialogKey)
    {
        this.dialogKey = dialogKey;
    }
}
