using System;

[Serializable]
public class ConversationLine
{
    public int speaker;  // 1이 왼쪽, 2가 오른쪽 npc

    public string line;
    public float time;
}