// Assets/Scripts/UI/ItemTooltip.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 아이템 위에 마우스 올리면 정보 표시
/// </summary>
public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance { get; private set; }
    
    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    [Header("Settings")]
    [SerializeField] private float showDelay = 1f; // 1초 대기
    [SerializeField] private Vector2 offset = new Vector2(10, -10); // 마우스에서 얼마나 떨어질지
    #endregion
    
    #region Private Fields
    private Coroutine showCoroutine;
    private RectTransform tooltipRect;
    private Canvas canvas;
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
    #endregion
    
    #region Initialization
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// 툴팁 표시 시작 (1초 후)
    /// </summary>
    public void ShowTooltip(ItemData item, bool isSellMode = false)
    {
        if (item == null) return;
        
        // 기존 코루틴 중지
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }
        
        showCoroutine = StartCoroutine(ShowTooltipDelayed(item, isSellMode));
    }
    
    /// <summary>
    /// 툴팁 숨기기
    /// </summary>
    public void HideTooltip()
    {
        // 코루틴 중지
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }
        
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
    #endregion
    
    #region Private Methods
    /// <summary>
    /// 1초 후 툴팁 표시
    /// </summary>
    private IEnumerator ShowTooltipDelayed(ItemData item, bool isSellMode)
    {
        Debug.Log($"[ItemTooltip] 1초 대기 시작... 아이템: {item.itemName}");
        yield return new WaitForSeconds(showDelay);
        
        Debug.Log($"[ItemTooltip] 툴팁 표시 시작");
        
        // 아이템 이름
        if (itemNameText != null)
        {
            itemNameText.text = item.itemName;
            Debug.Log($"[ItemTooltip] 아이템 이름 설정: {item.itemName}");
        }
        else
        {
            Debug.LogError("[ItemTooltip] itemNameText가 null입니다!");
        }
        
        // ✅ 가격 표시 (무조건 판매 가격)
        if (priceText != null)
        {
            int price = item.sellPrice;
            
            // 판매 가격 표시 (0이어도 "0 Gold"로 표시)
            priceText.text = $"{price} Gold";
            priceText.color = new Color(1f, 0.86f, 0f); // 노란색
            
            Debug.Log($"[ItemTooltip] 가격 설정: {priceText.text}");
        }
        else
        {
            Debug.LogError("[ItemTooltip] priceText가 null입니다!");
        }
        
        // 설명
        if (descriptionText != null)
        {
            descriptionText.text = string.IsNullOrEmpty(item.description) ? "설명 없음" : item.description;
            Debug.Log($"[ItemTooltip] 설명 설정: {descriptionText.text}");
        }
        else
        {
            Debug.LogError("[ItemTooltip] descriptionText가 null입니다!");
        }
        
        // 패널 표시
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            Debug.Log($"[ItemTooltip] 툴팁 패널 활성화 완료! Active: {tooltipPanel.activeSelf}");
            
            UpdateTooltipPosition();
        }
        else
        {
            Debug.LogError("[ItemTooltip] tooltipPanel이 null입니다!");
        }
    }
    
    /// <summary>
    /// 툴팁 위치 업데이트 (마우스 따라다님)
    /// </summary>
    private void UpdateTooltipPosition()
    {
        if (tooltipRect == null || canvas == null)
        {
            Debug.LogError("[ItemTooltip] tooltipRect 또는 canvas가 null입니다!");
            return;
        }
        
        // ✅ 마우스 오른쪽 하단에 표시
        Vector2 position = Input.mousePosition + new Vector3(20, -20, 0); // 오른쪽 하단
        
        Debug.Log($"[ItemTooltip] 마우스 위치: {Input.mousePosition}, 툴팁 위치: {position}");
        
        // 화면 안에 들어오도록 제한
        float halfWidth = tooltipRect.rect.width / 2;
        float halfHeight = tooltipRect.rect.height / 2;
        
        // 왼쪽 경계
        if (position.x - halfWidth < 0)
            position.x = halfWidth;
        
        // 오른쪽 경계
        if (position.x + halfWidth > Screen.width)
            position.x = Screen.width - halfWidth;
        
        // 위쪽 경계
        if (position.y + halfHeight > Screen.height)
            position.y = Screen.height - halfHeight;
        
        // 아래쪽 경계
        if (position.y - halfHeight < 0)
            position.y = halfHeight;
        
        tooltipPanel.transform.position = position;
        Debug.Log($"[ItemTooltip] 최종 툴팁 위치: {tooltipPanel.transform.position}");
    }
    #endregion
    
    #region Update
    private void Update()
    {
        // 툴팁이 표시 중이면 위치 업데이트
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            UpdateTooltipPosition();
        }
    }
    #endregion
}