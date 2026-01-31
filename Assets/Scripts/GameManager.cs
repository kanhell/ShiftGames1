using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // ΩÃ±€≈Ê
    public static GameManager instance;

    // to DialogScene
    public string dialogKey = "";
    public string npcKey = "";

    // npc images
   public  Dictionary<string, npc> images = new Dictionary<string, npc>();
    public Sprite macgregor_frontshot;



    void Awake()  // ΩÃ±€≈Ê
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        images["macgregor"] = new npc(macgregor_frontshot);
    }


    public void setVaribles_DialogScene(string dialogKey, string npcKey)
    {
        this.dialogKey = dialogKey;
        this.npcKey = npcKey;
    }

    public class npc
    {
        public Sprite frontshot;
        
        public npc(Sprite frontshot)
        {
            this.frontshot = frontshot;
        }
    }
}
