using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // ΩÃ±€≈Ê
    public static GameManager instance;

    // ¡§∫∏
    public TileData tileData;

    // DialogScene
    public DialogData DialogData;

    void Awake()  // ΩÃ±€≈Ê
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
}
