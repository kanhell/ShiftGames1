// Assets/Scripts/UI/PanelZOrderManager.cs

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 패널의 렌더링 순서(Z-Order)를 관리
/// 클릭 시 최상위로 이동 (우클릭 제외)
/// </summary>
public class PanelZOrderManager : MonoBehaviour, IPointerDownHandler
{
    #region Serialized Fields
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    #endregion
    
    #region Private Fields
    private Transform topLevelParent;
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        FindTopLevelParent();
    }
    #endregion
    
    #region Initialization
    /// <summary>
    /// Canvas 바로 아래의 최상위 부모 찾기
    /// </summary>
    private void FindTopLevelParent()
    {
        Transform current = transform;
        Transform parent = current.parent;
        
        while (parent != null)
        {
            // 부모가 Canvas이면 current가 최상위
            if (parent.GetComponent<Canvas>() != null)
            {
                topLevelParent = current;
                LogDebug($"최상위 부모 찾음: {topLevelParent.name}");
                return;
            }
            
            current = parent;
            parent = current.parent;
        }
        
        // Canvas를 못 찾았으면 자기 자신 사용
        topLevelParent = transform;
        Debug.LogWarning($"[{gameObject.name}] Canvas를 찾지 못했습니다. 자기 자신을 사용합니다.");
    }
    #endregion
    
    #region Event Handlers
    public void OnPointerDown(PointerEventData eventData)
    {
        // 우클릭은 무시 (ContextMenu 방해 안 함)
        if (eventData.button == PointerEventData.InputButton.Right)
            return;
        
        // 좌클릭만 최상위로
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            BringToFront();
        }
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// 이 패널을 최상위로 올림
    /// </summary>
    public void BringToFront()
    {
        if (topLevelParent == null)
        {
            Debug.LogError($"[{gameObject.name}] topLevelParent가 null입니다!");
            return;
        }
        
        topLevelParent.SetAsLastSibling();
        LogDebug($"최상위로 이동 (실제 이동: {topLevelParent.name})");
    }
    #endregion
    
    #region Helper Methods
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[{gameObject.name}] {message}");
        }
    }
    #endregion
}