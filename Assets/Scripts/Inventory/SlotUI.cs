// Assets/Scripts/Inventory/SlotUI.cs

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
    public TextMeshProUGUI priceText; // ✅ 가격 표시용 (상점에서만)
    
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
            
            // ✅ 수량 표시 로직 (Consumable/Ingredient만 표시)
            if (quantityText != null)
            {
                bool isConsumableOrIngredient = currentItem.itemType == ItemType.Consumable || 
                                                 currentItem.itemType == ItemType.Ingredient;
                
                if (isConsumableOrIngredient && quantity > 1)
                {
                    // Consumable/Ingredient는 실제 수량 표시
                    quantityText.text = quantity.ToString();
                    quantityText.gameObject.SetActive(true);
                    Debug.Log($"[{gameObject.name}] 수량 표시: {quantity}");
                }
                else
                {
                    // 장비류는 수량 표시 안 함 (내부적으로 여러 개여도 1개로 보임)
                    quantityText.gameObject.SetActive(false);
                    Debug.Log($"[{gameObject.name}] 장비 - 수량 텍스트 숨김");
                }
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] quantityText가 null입니다! 수량을 표시할 수 없습니다.");
            }
            
            // ✅ 가격 표시 로직 (상점 패널 안에 있을 때)
            if (priceText != null)
            {
                bool isInShopPanel = IsInShopPanel();
                
                if (isInShopPanel)
                {
                    // 어느 그리드에 있는지 확인
                    bool isInShopGrid = IsInShopGrid();
                    
                    if (isInShopGrid)
                    {
                        // ShopGrid: 구매 가격 표시
                        priceText.gameObject.SetActive(true);
                        priceText.text = $"{currentItem.buyPrice}G";
                        Debug.Log($"[{gameObject.name}] 구매 가격 표시: {currentItem.buyPrice}G");
                    }
                    else
                    {
                        // InventoryGrid (상점 안): 판매 가격 표시
                        priceText.gameObject.SetActive(true);
                        priceText.text = $"{currentItem.sellPrice}G";
                        Debug.Log($"[{gameObject.name}] 판매 가격 표시: {currentItem.sellPrice}G");
                    }
                }
                else
                {
                    // 상점 밖: 가격 숨김
                    priceText.gameObject.SetActive(false);
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
        
        // ✅ ItemCursorFollower가 아이템을 들고 있으면 드래그 차단
        if (ItemCursorFollower.Instance != null && ItemCursorFollower.Instance.IsHolding())
        {
            canDrag = false;
            Debug.Log("ItemCursorFollower가 아이템을 들고 있어서 드래그 차단");
            return;
        }
        
        // ✅ 상점이 열려있으면 ShopGrid와 InventoryGrid 모두 드래그 차단
        if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen())
        {
            if (transform.parent != null)
            {
                string parentName = transform.parent.name;
                
                // ShopGrid 또는 InventoryGrid의 자식이면 드래그 차단
                if (parentName == "ShopGrid" || parentName == "InventoryGrid")
                {
                    canDrag = false;
                    Debug.Log("상점이 열려있을 때는 드래그할 수 없습니다.");
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
        
        // ✅ ItemCursorFollower가 아이템을 들고 있으면 클릭 차단
        if (ItemCursorFollower.Instance != null && ItemCursorFollower.Instance.IsHolding())
        {
            Debug.Log("ItemCursorFollower가 아이템을 들고 있어서 클릭 차단");
            return;
        }
        
        // ✅ 상점이 열려있으면 우클릭 차단
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen())
            {
                Debug.Log("상점이 열려있을 때는 우클릭이 작동하지 않습니다.");
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
            // ✅ 상점이 열려있으면 선택 시스템 사용
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
        // ✅ 상점이 열려있으면 더블클릭 차단 (구매/판매는 클릭으로만)
        if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen())
        {
            Debug.Log("상점이 열려있을 때는 더블클릭이 작동하지 않습니다.");
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
    
    /// <summary>
    /// 상점에 아이템 판매
    /// </summary>
    private void SellItemToShop()
    {
        if (currentItem == null || quantity <= 0)
        {
            Debug.LogWarning("판매할 아이템이 없습니다.");
            return;
        }
        
        // ✅ 판매 가격 체크 제거 - 0 Gold여도 판매 가능
        
        // 골드 추가 (0이어도 추가)
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddGold(currentItem.sellPrice);
        }
        
        // 아이템 1개 제거
        quantity--;
        
        if (quantity <= 0)
        {
            ClearSlot();
        }
        else
        {
            UpdateUI();
        }
        
        Debug.Log($"{currentItem.itemName} 판매 완료! +{currentItem.sellPrice}G");
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
            // ✅ 컨텍스트 메뉴가 열려있으면 툴팁 표시 안 함
            if (ContextMenu.Instance != null && ContextMenu.Instance.IsMenuOpen())
            {
                return;
            }
            
            // ✅ 이 슬롯이 어느 창에 속해있는지 판단
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
        // ✅ 핵심: 상위 계층을 따라 올라가면서 체크
        Transform current = transform;
        
        // 최대 5단계까지 부모를 확인
        for (int i = 0; i < 5 && current != null; i++)
        {
            string nodeName = current.name;
            
            // ✅ 1순위: ShopPanel - 상점창 안의 모든 것은 상점 툴팁
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
            Debug.Log("빈 슬롯은 선택할 수 없습니다.");
            return;
        }
        
        if (ShopManager.Instance == null)
        {
            Debug.LogError("ShopManager.Instance가 null입니다!");
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