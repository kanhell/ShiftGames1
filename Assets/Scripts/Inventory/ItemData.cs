using UnityEngine;

/// <summary>
/// 아이템 데이터를 담는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item", order = 0)]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName = "New Item";
    public Sprite itemIcon;
    
    [Header("타입")]
    public ItemType itemType = ItemType.Consumable;
    
    [Header("스택")]
    [Tooltip("최대 겹침 개수 (1 = 겹침 불가)")]
    public int stackSize = 1;
    
    [Header("설명")]
    [TextArea(3, 5)]
    public string description = "";
}
