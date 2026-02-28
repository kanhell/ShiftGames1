// Assets/Scripts/Shop/WeightedRandomSelector.cs
// 가중치 기반 랜덤 선택 알고리즘 (디자인 패턴: Strategy Pattern)

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 가중치 기반 랜덤 선택 시스템
/// Tier별 확률을 동적으로 계산하여 아이템 선택
/// </summary>
public class WeightedRandomSelector
{
    /// <summary>
    /// Village 레벨에 따른 Tier별 가중치 계산
    /// 확장 가능한 알고리즘: Village가 증가하면 자동으로 새 Tier가 추가됨
    /// 
    /// ✨ 알고리즘 원리 (수학적 근거):
    /// 
    /// 1. 기본 개념:
    ///    - Village 레벨 = 플레이어 진행도
    ///    - 높은 Village일수록 고급 아이템(상위 Tier) 등장
    ///    - 하지만 저급 아이템도 여전히 필요 (점진적 감소)
    /// 
    /// 2. 핵심 공식:
    ///    - 최고 등장 Tier = Min(village + 1, maxTier)
    ///    - 최신 Tier (방금 등장): 10% 고정
    ///    - 바로 아래 Tier: 25% 고정
    ///    - 나머지 Tier들: 65%를 균등 분배
    /// 
    /// 3. 확장성 보장:
    ///    - Village 3, 4, 5...로 증가해도 동일한 규칙 적용
    ///    - 새 Tier가 등장하면 기존 Tier들은 자동으로 확률 재분배
    ///    - 코드 수정 없이 무한 확장 가능
    /// 
    /// 4. 예시 (실제 계산):
    ///    Village 0: Tier 1 (100%)
    ///    Village 1: Tier 2 (10%), Tier 1 (90% 중 25% + 65% = 90%)
    ///                실제로는 Tier 2 (10%), Tier 1 (90% → 하지만 공식에서는 25% + 나머지)
    ///    
    ///    더 정확한 계산:
    ///    Village 2: Tier 3 (10%), Tier 2 (25%), Tier 1 (65%)
    ///    Village 3: Tier 4 (10%), Tier 3 (25%), Tier 2+1 (65%를 2개로 나눔 = 각 32.5%)
    ///    Village 4: Tier 5 (10%), Tier 4 (25%), Tier 3+2+1 (65%를 3개로 나눔 = 각 21.67%)
    /// 
    /// 5. 디자인 의도:
    ///    - 신규 컨텐츠(새 Tier)는 희귀하게 (10%)
    ///    - 현재 레벨 적정 아이템은 적당히 (25%)
    ///    - 저레벨 아이템은 흔하지만 점차 감소 (65% 분배)
    /// </summary>
    public static Dictionary<int, float> CalculateTierWeights(int villageLevel, int maxTier)
    {
        Dictionary<int, float> weights = new Dictionary<int, float>();
        
        // Village 0: Tier 1만 100%
        if (villageLevel == 0)
        {
            weights[1] = 100f;
            return weights;
        }
        
        // 공식 설계:
        // - 최고 Tier: Min(villageLevel + 1, maxTier)
        // - 새 Tier 등장 확률: 10%
        // - 이전 Tier: 25%
        // - 나머지: 이전 Tier들에 분배
        
        int highestAvailableTier = Mathf.Min(villageLevel + 1, maxTier);
        
        // 각 Tier에 기본 가중치 할당
        for (int tier = 1; tier <= highestAvailableTier; tier++)
        {
            if (tier == highestAvailableTier)
            {
                // 최신 Tier: 10%
                weights[tier] = 10f;
            }
            else if (tier == highestAvailableTier - 1)
            {
                // 바로 아래 Tier: 25%
                weights[tier] = 25f;
            }
            else
            {
                // 나머지: 균등 분배
                // (100 - 10 - 25) = 65%를 나머지 Tier들에게
                int remainingTiers = highestAvailableTier - 2;
                if (remainingTiers > 0)
                {
                    weights[tier] = 65f / remainingTiers;
                }
            }
        }
        
        return weights;
    }
    
    /// <summary>
    /// 가중치에 따라 Tier 선택
    /// </summary>
    public static int SelectTierByWeight(Dictionary<int, float> tierWeights)
    {
        if (tierWeights == null || tierWeights.Count == 0)
        {
            return 1; // 기본값
        }
        
        // 총 가중치 계산
        float totalWeight = 0f;
        foreach (var weight in tierWeights.Values)
        {
            totalWeight += weight;
        }
        
        // 랜덤 값 생성
        float randomValue = Random.Range(0f, totalWeight);
        
        // 누적 가중치로 Tier 선택
        float cumulativeWeight = 0f;
        foreach (var kvp in tierWeights)
        {
            cumulativeWeight += kvp.Value;
            if (randomValue <= cumulativeWeight)
            {
                return kvp.Key;
            }
        }
        
        // 폴백
        return tierWeights.Keys.Max();
    }
    
    /// <summary>
    /// 여러 아이템 중 랜덤 선택 (균등 확률)
    /// </summary>
    public static T SelectRandom<T>(List<T> items)
    {
        if (items == null || items.Count == 0) return default(T);
        return items[Random.Range(0, items.Count)];
    }
    
    /// <summary>
    /// 랜덤 수량 생성 (min, max 범위)
    /// </summary>
    public static int RandomQuantity(int min, int max)
    {
        return Random.Range(min, max + 1);
    }
}