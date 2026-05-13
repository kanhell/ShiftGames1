using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPC_", menuName = "NPC/NPCData")]
public class NPCData : ScriptableObject
{
    [Header("[Info]")]
    public string npcName;
    public DialogData DialogData;
    public int direction;  // bubble, sprite 변경  // -1왼 0정면 1오
    public int state;
    public List<string> statesDescription;  // monologue 상태, 개발자용

    [Header("[Monologue]")]
    public bool isMON;
    public List<string> monologues;  // 혼잣말 또는 지나가는 말
    public float bubbleposY;


    [Header("[Sprite]")]
    public Sprite front;
    public Sprite left;
    public Sprite right;
    public Sprite back;
    public Sprite dialog;
}


