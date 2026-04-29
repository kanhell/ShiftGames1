using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 개별 슬롯 UI 관리 (드래그 앤 드롭 + 더블클릭 지원)
/// </summary>
public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI priceText;
    
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
    private bool canDrag = false; // 드래그 가능 여부
    
    // 더블클릭 관련
    private float lastClickTime = 0f;
    private int clickCount = 0;
    
    private void Start()
    {
        if (iconImage == null)
        {
            iconImage = transform.Find("ItemIcon")?.GetComponent<Image>();
                    }
        
        if (quantityText == null)
        {
            quantityText = transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
            if (quantityText == null)
            {
                quantityText = GetComponentInChildren<TextMeshProUGUI>();
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
            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = currentItem.itemIcon;
                iconImage.color = Color.white;
            }
            
            if (quantityText != null)
            {
                bool isConsumableOrIngredient = currentItem.itemType == ItemType.Consumable || 
                                                 currentItem.itemType == ItemType.Ingredient;
                
                if (isConsumableOrIngredient && quantity > 1)
                {
                    // Consumable/Ingredient는 실제 수량 표시
                    quantityText.text = quantity.ToString();
                    quantityText.gameObject.SetActive(true);
                }
                else
                {
                    // 장비류는 수량 표시 안 함 (내부적으로 여러 개여도 1개로 보임)
                    quantityText.gameObject.SetActive(false);
                }
            }
            
            if (priceText != null)
            {
                bool isInShopPanel = IsInShopPanel();
                
                if (isInShopPanel)
                {
                    bool isInShopGrid = IsInShopGrid();
                    
                    if (isInShopGrid)
                    {
                        priceText.gameObject.SetActive(true);
                        priceText.text = $"{currentItem.buyPrice}G";
                    }
                    else
                    {
                        priceText.gameObject.SetActive(true);
                        priceText.text = $"{currentItem.sellPrice}G";
                    }
                }
                else
                {
                    priceText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }
            
            if (quantityText != null)
            {
                quantityText.gameObject.SetActive(false);
            }
            
            if (priceText != null)
            {
                priceText.gameObject.SetActive(false);
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
            return canAccept;
        }
        
        // 퀵슬롯은 소비 아이템만 수용 (최대 5개)
        if (gameObject.name.Contains("Quick"))
        {
            bool canAccept = item.itemType == ItemType.Consumable;
            return canAccept;
        }
        
        if (slotType == SlotType.Inventory)
        {
            return true; // 인벤토리는 모든 아이템 수용
        }
        else // SlotType.Equipment
        {
            bool canAccept = item.itemType == allowedItemType;
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
        if (ItemSplitManager.Instance != null && ItemSplitManager.Instance.IsOpen())
        {
            canDrag = false;
            return;
        }
        
        if (ItemCursorFollower.Instance != null && ItemCursorFollower.Instance.IsHolding())
        {
            canDrag = false;
            return;
        }
        
        if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen())
        {
            if (transform.parent != null)
            {
                string parentName = transform.parent.name;
                
                // ShopGrid 또는 InventoryGrid의 자식이면 드래그 차단
                if (parentName == "ShopGrid" || parentName == "InventoryGrid")
                {
                    canDrag = false;
                    return;
                }
            }
        }
        
        if (currentItem == null)
        {
            canDrag = false;
            return;
        }
        
        // Shift + 드래그는 별도 처리하지 않음 (OnEndDrag에서 처리)
        canDrag = true;
        
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
        
        // 드롭 대상 찾기
        SlotUI targetSlot = GetSlotUnderMouse(eventData);
        
        if (targetSlot != null)
        {
            
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
            
            // Raycast 결과 전체 출력 (디버깅용)
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            foreach (var result in results)
            {
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
                return slot;
            }
            
            // 부모에서 SlotUI 찾기 (슬롯의 자식 오브젝트를 클릭한 경우)
            slot = result.gameObject.GetComponentInParent<SlotUI>();
            if (slot != null)
            {
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
        if (ItemSplitManager.Instance != null && ItemSplitManager.Instance.IsOpen())
        {
            return;
        }
        
        if (ItemCursorFollower.Instance != null && ItemCursorFollower.Instance.IsHolding())
        {
            return;
        }
        
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen())
            {
                return;
            }
            
            if (currentItem != null && ContextMenu.Instance != null)
            {
                ContextMenu.Instance.OpenMenu(this, eventData.position);
            }
            return;
        }
        
        // 좌클릭: Shift 키 체크
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen())
            {
                HandleShopSelection();
                return;
            }
            
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
        if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen())
        {
            return;
        }
        
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
        
        // 소비 아이템 → 퀵슬롯에 장착
        if (currentItem.itemType == ItemType.Consumable)
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.EquipToQuickSlot(this);
            }
            return;
        }
        
        // 재료 아이템 + 요리창 열려있음 → 요리창에 추가
        if (currentItem.itemType == ItemType.Ingredient && IsCookingPanelOpen())
        {
            AddToCookingPanel();
            return;
        }
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
            return;
        }
        
        LootManager.Instance.TransferLootToInventory(this, 0); // 0 = 전부
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
            return;
        }
        
        InventoryManager.Instance.EquipItem(this);
    }
    
    /// <summary>
    /// 장비 해제하고 인벤토리로
    /// </summary>
    private void UnequipToInventory()
    {
        if (InventoryManager.Instance == null)
        {
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
            return;
        }
        
        // 재료 추가 시도
        bool success = CookingManager.Instance.TryAddIngredient(currentItem, this);
        
    }
    
    /// <summary>
    /// 요리 슬롯에서 인벤토리로 되돌리기
    /// </summary>
    private void ReturnToInventory()
    {
        if (InventoryManager.Instance == null)
        {
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
            return;
        }
        
        // 스택 가능한 아이템만 분할 가능
        if (currentItem.stackSize <= 1)
        {
            return;
        }
        
        // 분할 패널 열기
        if (ItemSplitManager.Instance != null)
        {
            ItemSplitManager.Instance.OpenForClick(this);
        }
    }
    
    /// <summary>
    /// Shift + 드래그 처리
    /// </summary>
    private void HandleShiftDrag(SlotUI targetSlot)
    {
        if (currentItem == null || quantity <= 1)
        {
            return;
        }
        
        // 스택 가능한 아이템만 분할 가능
        if (currentItem.stackSize <= 1)
        {
            return;
        }
        
        // 대상 슬롯이 비어있어야 함
        if (targetSlot.currentItem != null)
        {
            return;
        }
        
        // 분할 패널 열기
        if (ItemSplitManager.Instance != null)
        {
            ItemSplitManager.Instance.OpenForDrag(this, targetSlot);
        }
    }
    
    // ─────────────────────────────────────────────
    // 툴팁 (마우스 호버)
    // ─────────────────────────────────────────────
    
    /// <summary>
    /// 마우스가 슬롯 위에 올라갔을 때
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null && ItemTooltip.Instance != null)
        {
            if (ContextMenu.Instance != null && ContextMenu.Instance.IsMenuOpen())
            {
                return;
            }
            
            ItemTooltip.PanelSource panelSource = DeterminePanelSource();
            
            // 상점이 열려있는지 확인
            bool isShopOpen = ShopManager.Instance != null && ShopManager.Instance.IsShopOpen();
            bool isSellMode = isShopOpen && ShopManager.Instance.IsSellMode();
            bool isBuyMode = isShopOpen && !ShopManager.Instance.IsSellMode();
            
            // 구매 모드인지 확인 (ShopGrid의 자식이면 상점 아이템)
            bool isShopItem = panelSource == ItemTooltip.PanelSource.Shop;
            
            ItemTooltip.Instance.ShowTooltip(currentItem, panelSource, isSellMode, isBuyMode && isShopItem);
        }
    }
    
    /// <summary>
    /// 마우스가 슬롯에서 벗어났을 때
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideTooltip();
        }
    }
    
    /// <summary>
    /// 이 슬롯이 어느 창에 속해있는지 판단
    /// </summary>
    private ItemTooltip.PanelSource DeterminePanelSource()
    {
        Transform current = transform;
        
        // 최대 5단계까지 부모를 확인
        for (int i = 0; i < 5 && current != null; i++)
        {
            string nodeName = current.name;
            
            if (nodeName.Contains("ShopPanel") || nodeName == "ShopPanel")
            {
                return ItemTooltip.PanelSource.Shop;
            }
            
            // 2순위: LootPanel
            if (nodeName.Contains("LootPanel") || nodeName.Contains("Loot"))
            {
                return ItemTooltip.PanelSource.Loot;
            }
            
            // 3순위: EquipmentPanel
            if (nodeName.Contains("EquipmentPanel") || nodeName.Contains("Equipment"))
            {
                return ItemTooltip.PanelSource.Equipment;
            }
            
            // 4순위: InventoryPanel (정확히 InventoryPanel일 때만)
            if (nodeName == "InventoryPanel")
            {
                return ItemTooltip.PanelSource.Inventory;
            }
            
            current = current.parent;
        }
        
        // ===== 폴백: 직접 부모 이름으로 판단 =====
        if (transform.parent != null)
        {
            string parentName = transform.parent.name;
            
            if (parentName.Contains("Shop"))
            {
                return ItemTooltip.PanelSource.Shop;
            }
            else if (parentName.Contains("Loot"))
            {
                return ItemTooltip.PanelSource.Loot;
            }
            else if (parentName.Contains("Equipment") || slotType == SlotType.Equipment)
            {
                return ItemTooltip.PanelSource.Equipment;
            }
            else if (parentName.Contains("Inventory"))
            {
                return ItemTooltip.PanelSource.Inventory;
            }
        }
        
        // 기본값: 인벤토리
        return ItemTooltip.PanelSource.Inventory;
    }
    
    /// <summary>
    /// 이 슬롯이 상점 그리드에 있는지 확인
    /// </summary>
    private bool IsInShopGrid()
    {
        // 상위 계층 확인 (ShopGrid 또는 ShopPanel 내부인지)
        Transform current = transform;
        
        for (int i = 0; i < 5 && current != null; i++)
        {
            string nodeName = current.name;
            
            // ShopGrid에 속해있으면 true
            if (nodeName == "ShopGrid" || nodeName.Contains("ShopGrid"))
            {
                return true;
            }
            
            current = current.parent;
        }
        
        return false;
    }
    
    /// <summary>
    /// 이 슬롯이 상점 패널 안에 있는지 확인 (ShopGrid 또는 InventoryGrid)
    /// </summary>
    private bool IsInShopPanel()
    {
        // 상위 계층 확인
        Transform current = transform;
        
        for (int i = 0; i < 5 && current != null; i++)
        {
            string nodeName = current.name;
            
            // ShopPanel에 속해있으면 true
            if (nodeName == "ShopPanel" || nodeName.Contains("ShopPanel"))
            {
                return true;
            }
            
            current = current.parent;
        }
        
        return false;
    }
    
    /// <summary>
    /// 상점에서 아이템 선택 처리
    /// </summary>
    private void HandleShopSelection()
    {
        if (currentItem == null)
        {
            return;
        }
        
        if (ShopManager.Instance == null)
        {
            return;
        }
        
        // 슬롯이 ShopGrid의 자식이면 상점 슬롯
        bool isShopSlot = transform.parent != null && transform.parent.name == "ShopGrid";
        
        // ShopManager에 선택 요청
        ShopManager.Instance.OnSlotClicked(this, isShopSlot);
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