using System.Collections;
using TMPro;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    public NPCData npcData;

    // bubble
    public Canvas convas_prefab;
    Canvas canvas;
    TextMeshProUGUI textUI;

    // MON
    public bool isMON = true;
    float MONshowTime = Values.npc_mon_time;

    // sprite
    public SpriteRenderer sr;

    void Start()
    {
        // bubble
        canvas = Instantiate(convas_prefab, transform);
        canvas.transform.localPosition = new Vector3(0, npcData.bubbleposY, 0);
        canvas.gameObject.SetActive(false);
        textUI = canvas.GetComponentInChildren<TextMeshProUGUI>();

        // direction
        ChangeDirection(npcData.direction);

    }

    public IEnumerator ShowMON()  // MON ¶ç¿ì±â
    {
        if (!isMON) yield break;

        isMON = false;
        Debug.Log(npcData.npcName + " : showMON");
        StartCoroutine(ShowBubble(npcData.monologues[npcData.state], MONshowTime));
        yield return new WaitForSeconds(MONshowTime);
        Debug.Log(npcData.npcName + " : dismissMON");
    }

    public IEnumerator ShowBubble(string msg, float time)
    {
        textUI.text = msg;
        canvas.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        canvas.gameObject.SetActive(false);
    }

    public void ChangeState(int state)
    {
        Debug.Log(npcData.npcName + " : state changed : " + state.ToString());
        this.npcData.state = state;
    }

    public void ChangeDirection(int direction)  // -1¿Þ 0Á¤¸é 1¿À 2ÈÄ
    {
        Sprite sp = npcData.front;
        float x = 0;
        if (direction == -1)
        {
            sp = npcData.left;
            x = -Values.npc_bubble_transDis;
        }
        else if (direction == 1)
        {
            sp = npcData.right;
            x = Values.npc_bubble_transDis;
        }
        else if (direction == 2)
        {
            sp = npcData.back;
        }

        // sprite, bubblepos º¯°æ
        sr.sprite = sp;
        canvas.transform.localPosition = new Vector3(0, npcData.bubbleposY, 0);
    }

}
