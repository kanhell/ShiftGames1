using System.Collections;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ConversationController : MonoBehaviour
{
    public ConversationData CNVData;
    NPCController npc1, npc2;
    NPCController speaker;

    bool isCNV = true;

    void Start()
    {
        NPCController[] npcs = FindObjectsByType<NPCController>(FindObjectsSortMode.None);
        foreach (NPCController npc in npcs)
        {
            if (npc.npcData == CNVData.npc1) npc1 = npc;
            else if (npc.npcData == CNVData.npc2) npc2 = npc;
        }

        npc1.npcData.isMON = false;
        npc2.npcData.isMON = false;
    }

    public IEnumerator showCNV()
    {
        if (!isCNV) yield break;

        Debug.Log("showCNV");
        isCNV = false;
        npc1.ChangeDirection(1);  // 오른쪽 보기
        npc2.ChangeDirection(-1);  // 왼쪽 보기

        for (int i = 0; i < CNVData.lines.Count; i++)
        {
            ConversationLine line = CNVData.lines[i];

            speaker = npc1;
            if (line.speaker == 2) speaker=npc2;

            StartCoroutine(speaker.ShowBubble(line.line, line.time));
            yield return new WaitForSeconds(line.time);
        }

        CNVEnd();
    }
    
    public void CNVEnd()
    {
        npc1.ChangeDirection(0);
        npc2.ChangeDirection(0);
    }
}
