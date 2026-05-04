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
    
    [Header("티어 (등급)")]
    [Tooltip("아이템 등급: 1 = 일반, 2 = 고급, 3 = 희귀, 4 = 영웅, 5 = 전설 등")]
    [Range(1, 10)]
    public int tier = 1;
    
    [Header("스택")]
    [Tooltip("최대 겹침 개수 (1 = 겹침 불가)")]
    public int stackSize = 1;
    
    [Header("가격")]
    [Tooltip("구매 가격 (0 = 구매 불가)")]
    public int buyPrice = 100;
    
    [Tooltip("판매 가격 (0 = 판매 불가)")]
    public int sellPrice = 50;
    
    [Header("설명")]
    [TextArea(3, 5)]
    public string description = "";
}