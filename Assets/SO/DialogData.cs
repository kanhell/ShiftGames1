using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

[CreateAssetMenu(fileName = "DLG_", menuName = "NPC/DialogData")]
public class DialogData : ScriptableObject
{
    [Header("[Info]")]
    [TextArea] public string discreption;
    public NPCData NPCData;
    public Sprite background;
    public int NPCState;

    [Header("[Dialog]")]
    public List<DialogLine> DialogLines;
    public List<string> codes;
    public string currentCode;
}

[System.Serializable]
public class DialogLine
{
    public string code;
    public List<string> lines;
    public List<string> options;
    public List<string> nextCode_byOption;
}

