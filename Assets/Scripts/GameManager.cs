using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // ΩÃ±€≈Ê
    public static GameManager instance;

    // Scene ¿Ãµø
    public string[] scenes = { "DummyLeftScene", "DummyScrollableScene", "DummyRightScene"};
    public int sceneIdx = 1;

    // npc images
    public Dictionary<string, npc> images = new Dictionary<string, npc>();
    // frontshot
    public Sprite macgregor_frontshot;
    public Sprite shopkeeper_frontshot;
    // bakcground
    public Sprite shop_background;
    public Sprite macgregor_background;

    // scene info
    public float limitMax, limitMin;

    // toDialog
    public toDialogInfo toDialog = new toDialogInfo();


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
        images["macgregor"] = new npc(macgregor_frontshot, macgregor_background);
        images["shop"] = new npc(shopkeeper_frontshot, shop_background);
    }


    public void setVaribles_DialogScene(string dialogKey, string npcKey, string preDialog_Scene, float x)
    {
        this.toDialog.npcKey = npcKey;
        this.toDialog.dialogKey = dialogKey;
        this.toDialog.preDialog_Scene = preDialog_Scene;
        this.toDialog.x = x;
    }

    public class npc
    {
        public Sprite frontshot;
        public Sprite background;
        
        public npc(Sprite frontshot, Sprite background)
        {
            this.frontshot = frontshot;
            this.background = background;
        }
    }

    public class toDialogInfo
    {
        public string dialogKey = "";
        public string npcKey = "";
        public string preDialog_Scene = "";
        public float x = 15;  // TODO : √÷√ º≥¡§ ∫Ø∞Ê
        public string background;
    }
}
