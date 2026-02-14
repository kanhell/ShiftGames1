// Assets/Scripts/Inventory/SlotUI.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 개별 슬롯 UI 관리 (드래그 앤 드롭 + 더블클릭 지원)
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
    
    [Header("Double Click Settings")]
    [Tooltip("더블클릭으로 인정할 시간 간격 (초)")]
    [SerializeField] private float doubleClickTime = 0.3f;
    
    // 드래그 관련
    private GameObject draggedIcon;
    private Canvas canvas;
    private Transform originalParent;
    private int originalSiblingIndex;
    private bool canDrag = false; // 드래그 가능 여부
    
    // 더블클릭 관련
    private float lastClickTime = 0f;
    private int clickCount = 0;
    
    private void Start()
    {
        // ✅ Inspector에 연결 안 되어 있으면 자동으로 찾기
        if (iconImage == null)
        {
            iconImage = transform.Find("ItemIcon")?.GetComponent<Image>();
            if (iconImage == null)
            {
                Debug.LogWarning($"[{gameObject.name}] iconImage를 찾을 수 없습니다!");
            }
        }
        
        if (quantityText == null)
        {
            quantityText = transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
            if (quantityText == null)
            {
                // 자식의 자식에서도 찾기 시도
                quantityText = GetComponentInChildren<TextMeshProUGUI>();
                if (quantityText == null)
                {
                    Debug.LogWarning($"[{gameObject.name}] quantityText를 찾을 수 없습니다!");
                }
            }
        }
        
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
            
            // ✅ 수량 표시 (중요!)
            if (quantityText != null)
            {
                if (quantity > 1)
                {
                    quantityText.text = quantity.ToString();
                    quantityText.gameObject.SetActive(true);
                    Debug.Log($"[{gameObject.name}] 수량 표시: {quantity}");
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                    Debug.Log($"[{gameObject.name}] 수량 1개 - 텍스트 숨김");
                }
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] quantityText가 null입니다! 수량을 표시할 수 없습니다.");
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
        // ✅ 분할 패널이 열려있으면 드래그 차단
        if (ItemSplitManager.Instance != null && ItemSplitManager.Instance.IsOpen())
        {
            canDrag = false;
            return;
        }
        
        if (currentItem == null)
        {
            canDrag = false;
            return;
        }
        
        // Shift + 드래그는 별도 처리하지 않음 (OnEndDrag에서 처리)
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
            
            // Shift + 드래그 = 아이템 분할 (대상이 빈 슬롯일 때만)
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && 
                targetSlot.currentItem == null && 
                quantity > 1)
            {
                HandleShiftDrag(targetSlot);
            }
            else
            {
                // 일반 드래그 = 아이템 이동/교환
                InventoryManager.Instance.TryMoveOrSwapDrag(this, targetSlot);
            }
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
    // 마우스 클릭 이벤트 (우클릭 + 더블클릭)
    // ─────────────────────────────────────────────
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // ✅ 분할 패널이 열려있으면 클릭 차단
        if (ItemSplitManager.Instance != null && ItemSplitManager.Instance.IsOpen())
        {
            return;
        }
        
        // 우클릭: 컨텍스트 메뉴
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (currentItem != null && ContextMenu.Instance != null)
            {
                ContextMenu.Instance.OpenMenu(this, eventData.position);
            }
            return;
        }
        
        // 좌클릭: Shift 키 체크
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Shift + 클릭 = 아이템 분할
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                HandleShiftClick();
            }
            else
            {
                // 일반 클릭 = 더블클릭 체크
                HandleLeftClick();
            }
        }
    }
    
    // ─────────────────────────────────────────────
    // 더블클릭 처리
    // ─────────────────────────────────────────────
    
    /// <summary>
    /// 좌클릭 처리 (더블클릭 감지 포함)
    /// </summary>
    private void HandleLeftClick()
    {
        if (currentItem == null) return;
        
        float timeSinceLastClick = Time.time - lastClickTime;
        
        if (timeSinceLastClick <= doubleClickTime)
        {
            clickCount++;
            
            if (clickCount >= 2)
            {
                // 더블클릭 감지!
                OnDoubleClickDetected();
                clickCount = 0;
            }
        }
        else
        {
            clickCount = 1;
        }
        
        lastClickTime = Time.time;
    }
    
    /// <summary>
    /// 더블클릭 감지 시 실행
    /// </summary>
    private void OnDoubleClickDetected()
    {
        Debug.Log($"더블클릭: {currentItem.itemName} (슬롯 타입: {slotType})");
        
        // 1. 인벤토리 슬롯
        if (slotType == SlotType.Inventory)
        {
            HandleInventoryDoubleClick();
        }
        // 2. 장비 슬롯
        else if (slotType == SlotType.Equipment)
        {
            HandleEquipmentDoubleClick();
        }
        // 3. 요리 재료 슬롯
        else if (slotType == SlotType.Cooking)
        {
            HandleCookingSlotDoubleClick();
        }
    }
    
    /// <summary>
    /// 인벤토리 슬롯 더블클릭 처리
    /// </summary>
    private void HandleInventoryDoubleClick()
    {
        // 전리품 슬롯인지 확인
        if (IsLootSlot())
        {
            TransferLootToInventory();
            return;
        }
        
        // 장비 아이템 → 자동 장착
        if (IsEquipmentItem(currentItem))
        {
            EquipItemAuto();
            return;
        }
        
        // 재료 아이템 + 요리창 열려있음 → 요리창에 추가
        if (currentItem.itemType == ItemType.Ingredient && IsCookingPanelOpen())
        {
            AddToCookingPanel();
            return;
        }
        
        // 그 외: 아무 동작 안 함
        Debug.Log($"{currentItem.itemName}은(는) 더블클릭 동작이 없습니다.");
    }
    
    /// <summary>
    /// 장비 슬롯 더블클릭 처리
    /// </summary>
    private void HandleEquipmentDoubleClick()
    {
        // 장비 슬롯 → 인벤토리로 되돌리기
        UnequipToInventory();
    }
    
    /// <summary>
    /// 요리 슬롯 더블클릭 처리
    /// </summary>
    private void HandleCookingSlotDoubleClick()
    {
        // 요리 슬롯 → 인벤토리로 되돌리기
        ReturnToInventory();
    }
    
    /// <summary>
    /// 전리품 슬롯인지 확인
    /// </summary>
    private bool IsLootSlot()
    {
        if (LootManager.Instance != null && LootManager.Instance.lootSlots != null)
        {
            return LootManager.Instance.lootSlots.Contains(this);
        }
        return false;
    }
    
    /// <summary>
    /// 전리품을 인벤토리로 옮기기
    /// </summary>
    private void TransferLootToInventory()
    {
        if (LootManager.Instance == null)
        {
            Debug.LogError("LootManager.Instance가 null입니다!");
            return;
        }
        
        LootManager.Instance.TransferLootToInventory(this, 0); // 0 = 전부
        Debug.Log($"전리품 → 인벤토리: {currentItem.itemName}");
    }
    
    /// <summary>
    /// 장비 아이템인지 확인
    /// </summary>
    private bool IsEquipmentItem(ItemData item)
    {
        return item.itemType == ItemType.Weapon ||
               item.itemType == ItemType.Helmet ||
               item.itemType == ItemType.Armor ||
               item.itemType == ItemType.Shoes ||
               item.itemType == ItemType.Bag ||
               item.itemType == ItemType.Quiver;
    }
    
    /// <summary>
    /// 장비 자동 장착
    /// </summary>
    private void EquipItemAuto()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance가 null입니다!");
            return;
        }
        
        InventoryManager.Instance.EquipItem(this);
        Debug.Log($"자동 장착: {currentItem.itemName}");
    }
    
    /// <summary>
    /// 장비 해제하고 인벤토리로
    /// </summary>
    private void UnequipToInventory()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance가 null입니다!");
            return;
        }
        
        // 빈 인벤토리 슬롯 찾기
        SlotUI emptySlot = InventoryManager.Instance.FindEmptyInventorySlot();
        
        if (emptySlot != null)
        {
            // 아이템 이동
            ItemData item = currentItem;
            int qty = quantity;
            
            emptySlot.SetItem(item, qty);
            emptySlot.UpdateUI();
            
            ClearSlot();
            
            Debug.Log($"장비 해제 → 인벤토리: {item.itemName}");
        }
        else
        {
            Debug.LogWarning("인벤토리에 빈 공간이 없습니다!");
        }
    }
    
    /// <summary>
    /// 요리창이 열려있는지 확인
    /// </summary>
    private bool IsCookingPanelOpen()
    {
        if (CookingManager.Instance == null) return false;
        return CookingManager.Instance.IsCookingOpen();
    }
    
    /// <summary>
    /// 요리창에 재료 추가
    /// </summary>
    private void AddToCookingPanel()
    {
        if (CookingManager.Instance == null)
        {
            Debug.LogError("CookingManager.Instance가 null입니다!");
            return;
        }
        
        // 재료 추가 시도
        bool success = CookingManager.Instance.TryAddIngredient(currentItem, this);
        
        if (success)
        {
            Debug.Log($"요리창에 추가: {currentItem.itemName}");
        }
        else
        {
            Debug.LogWarning("요리 슬롯이 가득 찼습니다!");
        }
    }
    
    /// <summary>
    /// 요리 슬롯에서 인벤토리로 되돌리기
    /// </summary>
    private void ReturnToInventory()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance가 null입니다!");
            return;
        }
        
        // 빈 인벤토리 슬롯 찾기
        SlotUI emptySlot = InventoryManager.Instance.FindEmptyInventorySlot();
        
        if (emptySlot != null)
        {
            // 아이템 이동
            ItemData item = currentItem;
            int qty = quantity;
            
            emptySlot.SetItem(item, qty);
            emptySlot.UpdateUI();
            
            ClearSlot();
            
            Debug.Log($"인벤토리로 되돌림: {item.itemName} x{qty}");
        }
        else
        {
            Debug.LogWarning("인벤토리에 빈 공간이 없습니다!");
        }
    }
    
    // ─────────────────────────────────────────────
    // Shift 키 처리 (아이템 분할)
    // ─────────────────────────────────────────────
    
    /// <summary>
    /// Shift + 클릭 처리
    /// </summary>
    private void HandleShiftClick()
    {
        if (currentItem == null || quantity <= 1)
        {
            Debug.Log("분할할 수 없는 아이템입니다.");
            return;
        }
        
        // 스택 가능한 아이템만 분할 가능
        if (currentItem.stackSize <= 1)
        {
            Debug.Log("이 아이템은 분할할 수 없습니다.");
            return;
        }
        
        Debug.Log($"Shift + 클릭: {currentItem.itemName} 분할");
        
        // 분할 패널 열기
        if (ItemSplitManager.Instance != null)
        {
            ItemSplitManager.Instance.OpenForClick(this);
        }
        else
        {
            Debug.LogError("ItemSplitManager.Instance가 null입니다!");
        }
    }
    
    /// <summary>
    /// Shift + 드래그 처리
    /// </summary>
    private void HandleShiftDrag(SlotUI targetSlot)
    {
        if (currentItem == null || quantity <= 1)
        {
            Debug.Log("분할할 수 없는 아이템입니다.");
            return;
        }
        
        // 스택 가능한 아이템만 분할 가능
        if (currentItem.stackSize <= 1)
        {
            Debug.Log("이 아이템은 분할할 수 없습니다.");
            return;
        }
        
        // 대상 슬롯이 비어있어야 함
        if (targetSlot.currentItem != null)
        {
            Debug.Log("대상 슬롯이 비어있어야 합니다.");
            return;
        }
        
        Debug.Log($"Shift + 드래그: {currentItem.itemName} → {targetSlot.name}");
        
        // 분할 패널 열기
        if (ItemSplitManager.Instance != null)
        {
            ItemSplitManager.Instance.OpenForDrag(this, targetSlot);
        }
        else
        {
            Debug.LogError("ItemSplitManager.Instance가 null입니다!");
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