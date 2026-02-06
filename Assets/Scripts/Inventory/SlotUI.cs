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
    private bool canDrag = false; // 드래그 가능 여부
    
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
        
        // 요리 재료 슬롯 체크
        if (slotType == SlotType.Cooking)
        {
            bool canAccept = item.itemType == ItemType.Ingredient;
            Debug.Log($"[{gameObject.name}] 요리 슬롯 - 아이템 타입: {item.itemType}, 수용 가능: {canAccept}");
            return canAccept;
        }
        
        // 퀵슬롯은 소비 아이템만 수용 (최대 5개)
        if (gameObject.name.Contains("Quick"))
        {
            bool canAccept = item.itemType == ItemType.Consumable;
            Debug.Log($"[{gameObject.name}] 퀵슬롯 - 아이템 타입: {item.itemType}, 수용 가능: {canAccept}");
            return canAccept;
        }
        
        if (slotType == SlotType.Inventory)
        {
            Debug.Log($"[{gameObject.name}] 인벤토리 슬롯 - 모든 아이템 수용 가능");
            return true; // 인벤토리는 모든 아이템 수용
        }
        else // SlotType.Equipment
        {
            bool canAccept = item.itemType == allowedItemType;
            Debug.Log($"[{gameObject.name}] 장비 슬롯 - 필요 타입: {allowedItemType}, 아이템 타입: {item.itemType}, 수용 가능: {canAccept}");
            return canAccept;
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
        if (currentItem == null)
        {
            canDrag = false;
            return;
        }
        
        // ✅ 요리 중 드래그 제한 제거 - 모든 슬롯에서 드래그 가능
        canDrag = true;
        Debug.Log($"드래그 시작: {currentItem.itemName}");
        
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
    }
    
    /// <summary>
    /// 드래그 중
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 가능하고 드래그 아이콘이 있을 때만 이동
        if (canDrag && draggedIcon != null)
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
            draggedIcon = null;
        }
        
        // 원본 아이콘 복원
        if (iconImage != null)
        {
            Color iconColor = iconImage.color;
            iconColor.a = 1f;
            iconImage.color = iconColor;
        }
        
        // 드래그 불가 상태였으면 종료
        if (!canDrag)
        {
            canDrag = false;
            return;
        }
        
        Debug.Log("드래그 종료 - 슬롯 찾는 중...");
        
        // 드롭 대상 찾기
        SlotUI targetSlot = GetSlotUnderMouse(eventData);
        
        if (targetSlot != null)
        {
            Debug.Log($"대상 슬롯 찾음: {targetSlot.name}");
            // 아이템 이동/교환
            InventoryManager.Instance.TryMoveOrSwapDrag(this, targetSlot);
        }
        else
        {
            Debug.LogWarning("드롭 대상 슬롯을 찾을 수 없습니다!");
            
            // Raycast 결과 전체 출력 (디버깅용)
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            Debug.Log($"Raycast 결과 개수: {results.Count}");
            foreach (var result in results)
            {
                Debug.Log($"  - {result.gameObject.name} (depth: {result.depth})");
            }
        }
        
        // 드래그 플래그 리셋
        canDrag = false;
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
            // 직접 SlotUI 컴포넌트가 있는지 확인
            SlotUI slot = result.gameObject.GetComponent<SlotUI>();
            if (slot != null)
            {
                Debug.Log($"슬롯 찾음 (직접): {result.gameObject.name}");
                return slot;
            }
            
            // 부모에서 SlotUI 찾기 (슬롯의 자식 오브젝트를 클릭한 경우)
            slot = result.gameObject.GetComponentInParent<SlotUI>();
            if (slot != null)
            {
                Debug.Log($"슬롯 찾음 (부모): {slot.gameObject.name}");
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
            // ✅ 우클릭: 컨텍스트 메뉴 열기 (제한 없음)
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
    Equipment,  // 장비 슬롯
    Cooking     // 요리 재료 슬롯
}