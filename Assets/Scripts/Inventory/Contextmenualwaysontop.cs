// Assets/Scripts/UI/ContextMenuAlwaysOnTop.cs

using UnityEngine;

/// <summary>
/// ContextMenuManager를 항상 최상위에 유지
/// 이벤트 기반으로 효율적으로 동작
/// </summary>
public class ContextMenuAlwaysOnTop : MonoBehaviour
{
    #region Serialized Fields
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Fallback")]
    [Tooltip("이벤트가 작동하지 않을 경우 LateUpdate 사용")]
    [SerializeField] private bool useLateUpdateFallback = true;
    #endregion
    
    #region Unity Lifecycle
    private void Start()
    {
        BringToFront();
        SubscribeToPanelEvents();
    }
    
    private void OnEnable()
    {
        BringToFront();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromPanelEvents();
    }
    
    private void LateUpdate()
    {
        if (!useLateUpdateFallback) return;
        
        // 최상위가 아니면 강제 이동 (안전장치)
        if (transform.parent != null && 
            transform.GetSiblingIndex() != transform.parent.childCount - 1)
        {
            BringToFront();
            
            if (showDebugLogs)
            {
                Debug.LogWarning("[ContextMenu] 최상위가 아니어서 강제 이동!");
            }
        }
    }
    #endregion
    
    #region Event Subscription
    private void SubscribeToPanelEvents()
    {
        DraggablePanel[] panels = FindObjectsOfType<DraggablePanel>();
        
        int subscribed = 0;
        
        foreach (var panel in panels)
        {
            panel.OnPanelDragged += BringToFront;
            subscribed++;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[ContextMenu] {subscribed}개 패널에 이벤트 구독 완료");
        }
        
        if (subscribed == 0)
        {
            Debug.LogWarning("[ContextMenu] 구독된 이벤트가 없습니다! DraggablePanel을 확인하세요.");
        }
    }
    
    private void UnsubscribeFromPanelEvents()
    {
        DraggablePanel[] panels = FindObjectsOfType<DraggablePanel>();
        
        foreach (var panel in panels)
        {
            if (panel != null)
            {
                panel.OnPanelDragged -= BringToFront;
            }
        }
    }
    #endregion
    
    #region Helper Methods
    private void BringToFront()
    {
        if (transform.parent == null)
        {
            Debug.LogError("[ContextMenu] 부모가 없습니다! Canvas 아래에 있어야 합니다.");
            return;
        }
        
        int oldIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();
        int newIndex = transform.GetSiblingIndex();
        
        if (showDebugLogs && oldIndex != newIndex)
        {
            Debug.Log($"[ContextMenu] 최상위로 이동: Index {oldIndex} → {newIndex}");
        }
    }
    #endregion
}