using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CNV_", menuName = "NPC/ConversationData")]
public class ConversationData : ScriptableObject
{
    [Header("[Npc]")]
    public NPCData npc1;
    public NPCData npc2;

    [Header("[Conversation]")]
    public List<ConversationLine> lines;

}