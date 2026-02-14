// Assets/Scripts/UI/ItemSplitManagerAlwaysSecond.cs

using UnityEngine;

/// <summary>
/// ItemSplitManager를 항상 Canvas의 마지막에서 두 번째로 유지
/// (ContextMenu 바로 아래)
/// </summary>
public class ItemSplitManagerAlwaysSecond : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool autoPositioning = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    private Transform parentTransform;
    
    private void Start()
    {
        parentTransform = transform.parent;
        
        if (parentTransform == null)
        {
            Debug.LogError("[ItemSplitManagerAlwaysSecond] 부모(Canvas)가 없습니다!");
            enabled = false;
            return;
        }
        
        // 초기 위치 설정
        SetToSecondLast();
    }
    
    private void LateUpdate()
    {
        if (!autoPositioning || parentTransform == null) return;
        
        // 현재 위치 확인
        int currentIndex = transform.GetSiblingIndex();
        int targetIndex = parentTransform.childCount - 2; // 마지막에서 두 번째
        
        // 위치가 틀리면 조정
        if (currentIndex != targetIndex && targetIndex >= 0)
        {
            transform.SetSiblingIndex(targetIndex);
            
            if (showDebugLogs)
            {
                Debug.Log($"[ItemSplitManagerAlwaysSecond] 위치 조정: {currentIndex} → {targetIndex}");
            }
        }
    }
    
    /// <summary>
    /// 마지막에서 두 번째로 이동
    /// </summary>
    public void SetToSecondLast()
    {
        if (parentTransform == null) return;
        
        int targetIndex = parentTransform.childCount - 2;
        
        if (targetIndex >= 0)
        {
            transform.SetSiblingIndex(targetIndex);
            
            if (showDebugLogs)
            {
                Debug.Log($"[ItemSplitManagerAlwaysSecond] 마지막에서 두 번째로 설정: Index {targetIndex}");
            }
        }
    }
    
    /// <summary>
    /// ItemSplitManager가 열릴 때 호출
    /// </summary>
    public void OnPanelOpened()
    {
        SetToSecondLast();
    }
}