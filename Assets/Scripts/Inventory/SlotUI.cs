// Assets/Scripts/Inventory/SlotUI.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 개별 슬롯 UI 관리 (드래그 앤 드롭 지원)
/// </summary>
public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI quantityText;
    
    [Header("Slot Data")]
    public ItemData currentItem;
    public int quantity = 0;
    
    [Header("Slot Type")]
    public SlotType slotType = SlotType.Inventory;
    public ItemType allowedItemType = ItemType.Weapon; // Equipment 슬롯일 때만 사용
    
    // 드래그 관련
    private GameObject draggedIcon;
    private Canvas canvas;
    private Transform originalParent;
    private int originalSiblingIndex;
    
    private void Start()
    {
        UpdateUI();
        canvas = GetComponentInParent<Canvas>();
    }
    
    /// <summary>
    /// UI 갱신
    /// </summary>
    public void UpdateUI()
    {
        if (currentItem != null && quantity > 0)
        {
            // 아이콘 활성화
            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = currentItem.itemIcon;
                iconImage.color = Color.white;
                Debug.Log($"[{gameObject.name}] 아이콘 표시: {currentItem.itemName}");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] iconImage가 null입니다!");
            }
            
            // 수량 표시
            if (quantityText != null)
            {
                if (quantity > 1)
                {
                    quantityText.text = quantity.ToString();
                    quantityText.gameObject.SetActive(true);
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // 빈 슬롯
            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }
            
            if (quantityText != null)
            {
                quantityText.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// 아이템 추가/설정
    /// </summary>
    public void SetItem(ItemData item, int amount)
    {
        currentItem = item;
        quantity = amount;
        UpdateUI();
    }
    
    /// <summary>
    /// 슬롯 비우기
    /// </summary>
    public void ClearSlot()
    {
        currentItem = null;
        quantity = 0;
        UpdateUI();
    }
    
    /// <summary>
    /// 이 슬롯이 해당 아이템을 받을 수 있는지 확인
    /// </summary>
    public bool CanAcceptItem(ItemData item)
    {
        if (item == null) return false;
        
        if (slotType == SlotType.Inventory)
        {
            return true; // 인벤토리는 모든 아이템 수용
        }
        else // SlotType.Equipment
        {
            return item.itemType == allowedItemType;
        }
    }
    
    // ─────────────────────────────────────────────
    // 드래그 앤 드롭 이벤트
    // ─────────────────────────────────────────────
    
    /// <summary>
    /// 드래그 시작
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;
        
        // 드래그할 아이콘 생성
        draggedIcon = new GameObject("DraggedIcon");
        draggedIcon.transform.SetParent(canvas.transform, false);
        draggedIcon.transform.SetAsLastSibling(); // 최상위로
        
        Image dragImage = draggedIcon.AddComponent<Image>();
        dragImage.sprite = currentItem.itemIcon;
        dragImage.raycastTarget = false; // 레이캐스트 무시
        
        // 반투명 효과
        Color color = dragImage.color;
        color.a = 0.6f;
        dragImage.color = color;
        
        // 크기 설정
        RectTransform rectTransform = draggedIcon.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(64, 64);
        
        // 원본 아이콘 반투명 처리
        if (iconImage != null)
        {
            Color iconColor = iconImage.color;
            iconColor.a = 0.3f;
            iconImage.color = iconColor;
        }
        
        Debug.Log($"드래그 시작: {currentItem.itemName}");
    }
    
    /// <summary>
    /// 드래그 중
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            draggedIcon.transform.position = eventData.position;
        }
    }
    
    /// <summary>
    /// 드래그 종료
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 아이콘 제거
        if (draggedIcon != null)
        {
            Destroy(draggedIcon);
        }
        
        // 원본 아이콘 복원
        if (iconImage != null)
        {
            Color iconColor = iconImage.color;
            iconColor.a = 1f;
            iconImage.color = iconColor;
        }
        
        // 드롭 대상 찾기
        SlotUI targetSlot = GetSlotUnderMouse(eventData);
        
        if (targetSlot != null && targetSlot != this)
        {
            // 아이템 이동/교환
            InventoryManager.Instance.TryMoveOrSwapDrag(this, targetSlot);
        }
        
        Debug.Log("드래그 종료");
    }
    
    /// <summary>
    /// 마우스 아래의 슬롯 찾기
    /// </summary>
    private SlotUI GetSlotUnderMouse(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (var result in results)
        {
            SlotUI slot = result.gameObject.GetComponent<SlotUI>();
            if (slot != null)
            {
                return slot;
            }
        }
        
        return null;
    }
    
    // ─────────────────────────────────────────────
    // 마우스 클릭 이벤트 (우클릭용)
    // ─────────────────────────────────────────────
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 우클릭: 컨텍스트 메뉴 열기
            if (currentItem != null && ContextMenu.Instance != null)
            {
                ContextMenu.Instance.OpenMenu(this, eventData.position);
            }
        }
    }
}

/// <summary>
/// 슬롯 타입
/// </summary>
public enum SlotType
{
    Inventory,  // 인벤토리 슬롯
    Equipment   // 장비 슬롯
}