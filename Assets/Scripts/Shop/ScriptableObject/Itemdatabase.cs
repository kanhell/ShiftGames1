using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 게임 내 모든 아이템을 관리하는 싱글톤 데이터베이스
/// </summary>
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Shop/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("전체 아이템 목록")]
    [Tooltip("게임에 존재하는 모든 아이템")]
    public List<ItemData> allItems = new List<ItemData>();
    
    #region 필터링 메서드
    
    /// <summary>
    /// 특정 Tier의 아이템만 필터링
    /// </summary>
    public List<ItemData> GetItemsByTier(int tier)
    {
        return allItems.Where(item => item != null && item.tier == tier).ToList();
    }
    
    /// <summary>
    /// 여러 Tier의 아이템 필터링
    /// </summary>
    /*
    public List<ItemData> GetItemsByTiers(params int[] tiers)
    {
        HashSet<int> tierSet = new HashSet<int>(tiers);
        return allItems.Where(item => item != null && tierSet.Contains(item.tier)).ToList();
    }
    */
    
    /// <summary>
    /// 특정 타입의 아이템만 필터링
    /// </summary>
    /*
    public List<ItemData> GetItemsByType(ItemType itemType)
    {
        return allItems.Where(item => item != null && item.itemType == itemType).ToList();
    }
    */
    
    /// <summary>
    /// Tier와 타입으로 동시 필터링
    /// </summary>
    /*
    public List<ItemData> GetItemsByTierAndType(int tier, ItemType itemType)
    {
        return allItems.Where(item => 
            item != null && 
            item.tier == tier && 
            item.itemType == itemType
        ).ToList();
    }
    */
    
    /// <summary>
    /// 최대 Tier 반환
    /// </summary>
    public int GetMaxTier()
    {
        if (allItems == null || allItems.Count == 0) return 1;
        return allItems.Max(item => item != null ? item.tier : 1);
    }
    
    /// <summary>
    /// 랜덤 아이템 가져오기 (단일)
    /// </summary>
    /*
    public ItemData GetRandomItem()
    {
        if (allItems == null || allItems.Count == 0) return null;
        return allItems[Random.Range(0, allItems.Count)];
    }
    */
    
    #endregion
    
    #region 데이터 검증
    
    /// <summary>
    /// 데이터베이스 유효성 검사 (에디터용)
    /// </summary>
    /*
    public void ValidateDatabase()
    {
        if (allItems == null)
        {
            return;
        }
        
        int nullCount = allItems.Count(item => item == null);
    }
    */
    
    #endregion
}