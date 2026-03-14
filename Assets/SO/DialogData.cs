using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DLG_npc_context", menuName = "NPC/DialogData")]
public class DialogData : ScriptableObject
{
    [Header("[Info]")]
    public string npcName;  // NPC 이름
    public bool isDialog;  // 대화를 걸 수 있는지

    [Header("[Monologue]")]
    public List<string> monologues;  // 혼잣말 또는 지나가는 말
    public List<string> statesDescription;  // monologue 상태


    [Header("[Sprites]")]
    public Sprite front;
    public Sprite back;
    public Sprite left;
    public Sprite dialog;
}


