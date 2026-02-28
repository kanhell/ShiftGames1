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
    
    /// <summary>
    /// Tier 기반 가격 자동 계산 (기본 가격 * Tier 승수)
    /// </summary>
    public int GetCalculatedBuyPrice(int basePrice = 100)
    {
        // Tier가 높을수록 가격이 기하급수적으로 증가
        // Tier 1: x1, Tier 2: x2.5, Tier 3: x6, Tier 4: x15, Tier 5: x35
        float multiplier = Mathf.Pow(2.5f, tier - 1);
        return Mathf.RoundToInt(basePrice * multiplier);
    }
    
    /// <summary>
    /// Tier 기반 스탯 보정 계산 (게임 밸런스용)
    /// </summary>
    public float GetTierStatMultiplier()
    {
        // Tier 1: x1.0, Tier 2: x1.5, Tier 3: x2.25, Tier 4: x3.4, Tier 5: x5.0
        return Mathf.Pow(1.5f, tier - 1);
    }
}