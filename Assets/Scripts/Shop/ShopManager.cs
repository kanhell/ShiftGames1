// 최종 수정 버전 - 중복 제거 및 오류 수정

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }
    
    #region Serialized Fields
    [Header("데이터베이스")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("마을 레벨")]
    [Tooltip("0 = 초보, 1 = 발전, 2 = 도시, 3+ = 대도시")]
    [SerializeField] private int village = 0;

    [Header("상점 설정")]
    [Tooltip("상점에 진열될 총 슬롯 개수 (아이템 종류 수). 예: 10이면 최대 10종류의 아이템이 상점에 나타남")]
    [SerializeField] private int shopItemSlotCount = 10;
    
    [Tooltip("각 아이템의 최소 판매 수량 (기본값, Village 레벨에 따라 최댓값은 증가함)")]
    [SerializeField] private int minQuantityPerItem = 1;
    
    [Tooltip("각 아이템의 기본 최대 판매 수량 (Village 0일 때 기준)")]
    [SerializeField] private int baseMaxQuantityPerItem = 3;

    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform shopGrid;
    [SerializeField] private Transform inventoryGrid;
    [SerializeField] private TMPro.TextMeshProUGUI goldText;

    [Header("Tab Buttons")]
    [SerializeField] private GameObject buyTab;
    [SerializeField] private GameObject sellTab;

    [Header("Action Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button sellButton;

    [Header("Quantity Panel")]
    [SerializeField] private GameObject quantityPanel;
    [SerializeField] private TMPro.TextMeshProUGUI quantityText;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;

    [Header("Price Panel")]
    [SerializeField] private GameObject pricePanel;
    [SerializeField] private TMPro.TextMeshProUGUI priceText;

    [Header("Refresh Timer")]
    [SerializeField] private TMPro.TextMeshProUGUI refreshTimerText;
    [SerializeField] private float refreshInterval = 10f;

    [Header("Prefabs")]
    [SerializeField] private GameObject shopSlotPrefab;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    #endregion
    
    #region Private Fields
    private bool isShopOpen = false;
    private ShopMode currentMode = ShopMode.Buy;
    private List<SlotUI> shopSlots = new List<SlotUI>();
    private List<SlotUI> inventorySlots = new List<SlotUI>();
    
    // 선택 시스템
    private SlotUI singleSelectedSlot = null;
    private List<SlotUI> multiSelectedSlots = new List<SlotUI>();
    private SelectionType currentSelectionType = SelectionType.None;
    private bool isInventorySelection = false;
    
    // 수량
    private int selectedQuantity = 1;
    private int maxSelectableQuantity = 1;
    
    // 타이머
    private float refreshTimer = 0f;
    private bool needsRefresh = false;
    
    // 색상
    private Dictionary<SlotUI, Color> originalColors = new Dictionary<SlotUI, Color>();
    private Color selectedColor = new Color(0.8f, 0.8f, 1f, 1f);
    #endregion
    
    #region Enums
    private enum ShopMode { Buy, Sell }
    private enum SelectionType { None, ConsumableIngredient, Equipment }
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    private void Start()
    {
        InitializeShop();
    }
    
    private void Update()
    {
        UpdateRefreshTimer();
        HandleInput();
    }
    #endregion
    
    #region Initialization
    private void InitializeShop()
    {
        if (itemDatabase == null)
        {
            return;
        }
        
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        
        // 버튼 이벤트
        if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
        if (buyButton != null) buyButton.onClick.AddListener(OnBuyButtonClicked);
        if (sellButton != null) sellButton.onClick.AddListener(OnSellButtonClicked);
        if (decreaseButton != null) decreaseButton.onClick.AddListener(DecreaseQuantity);
        if (increaseButton != null) increaseButton.onClick.AddListener(IncreaseQuantity);
        
        // 슬롯 생성
        CreateShopSlots(36);
        CreateInventorySlots(36);
        
        // 패널 숨기기
        if (quantityPanel != null) quantityPanel.SetActive(false);
        if (pricePanel != null) pricePanel.SetActive(false);
        if (buyButton != null) buyButton.gameObject.SetActive(false);
        if (sellButton != null) sellButton.gameObject.SetActive(false);
        
        // 타이머
        refreshTimer = refreshInterval;
        GenerateRandomShopItems();
        
        LogDebug($"상점 초기화 완료 (Village: {village})");
    }
    
    private void CreateShopSlots(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject slotObj = Instantiate(shopSlotPrefab, shopGrid);
            SlotUI slot = slotObj.GetComponent<SlotUI>();
            
            if (slot != null)
            {
                slot.slotType = SlotType.Inventory;
                shopSlots.Add(slot);
            }
        }
    }
    
    private void CreateInventorySlots(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject slotObj = Instantiate(shopSlotPrefab, inventoryGrid);
            SlotUI slot = slotObj.GetComponent<SlotUI>();
            
            if (slot != null)
            {
                slot.slotType = SlotType.Inventory;
                inventorySlots.Add(slot);
            }
        }
    }
    #endregion
    
    #region Input
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ToggleShop();
        }
    }
    #endregion
    
    #region Shop Open/Close
    public void ToggleShop()
    {
        if (isShopOpen) CloseShop();
        else OpenShop();
    }
    
    public void OpenShop()
    {
        if (shopPanel == null) return;
        
        if (needsRefresh)
        {
            RefreshShop();
            needsRefresh = false;
        }
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseAllPanels();
        }
        
        isShopOpen = true;
        shopPanel.SetActive(true);
        
        UpdateGoldDisplay();
        UpdateInventoryDisplay();
        SetMode(ShopMode.Buy);
        
        LogDebug("상점 열림");
    }
    
    public void CloseShop()
    {
        if (shopPanel == null) return;
        
        isShopOpen = false;
        shopPanel.SetActive(false);
        
        DeselectAll();
        
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideTooltipIfFromPanel(ItemTooltip.PanelSource.Shop);
        }
        
        LogDebug("상점 닫힘");
    }
    
    public bool IsShopOpen() => isShopOpen;
    public bool IsSellMode() => currentMode == ShopMode.Sell;
    #endregion
    
    #region Selection System
    
    public void OnSlotClicked(SlotUI slot, bool isShopSlot)
    {
        if (slot == null || slot.currentItem == null) return;
        
        ItemData item = slot.currentItem;
        bool isConsumableOrIngredient = item.itemType == ItemType.Consumable || 
                                         item.itemType == ItemType.Ingredient;
        
        if (isConsumableOrIngredient)
        {
            if (currentSelectionType == SelectionType.Equipment)
            {
                return;
            }
            
            if (singleSelectedSlot != null)
            {
                bool prevIsShop = IsSlotInShopGrid(singleSelectedSlot);
                if (prevIsShop != isShopSlot)
                {
                    return;
                }
            }
            
            SelectConsumableIngredient(slot, isShopSlot);
        }
        else
        {
            if (currentSelectionType == SelectionType.ConsumableIngredient)
            {
                return;
            }
            
            SelectEquipment(slot, isShopSlot);
        }
    }
    
    private void SelectConsumableIngredient(SlotUI slot, bool isShopSlot)
    {
        if (singleSelectedSlot == slot)
        {
            DeselectAll();
            return;
        }
        
        DeselectAll();
        
        singleSelectedSlot = slot;
        currentSelectionType = SelectionType.ConsumableIngredient;
        isInventorySelection = !isShopSlot;
        
        maxSelectableQuantity = slot.quantity;
        selectedQuantity = 1;
        
        ApplySelectionColor(slot);
        
        ShowQuantityPanel();
        ShowPricePanel();
        UpdateQuantityText();
        UpdatePriceText();
        ShowAppropriateButton();
        
        LogDebug($"Consumable/Ingredient 선택: {slot.currentItem.itemName}");
    }
    
    private void SelectEquipment(SlotUI slot, bool isShopSlot)
    {
        if (multiSelectedSlots.Count > 0)
        {
            bool prevIsShop = IsSlotInShopGrid(multiSelectedSlots[0]);
            if (prevIsShop != isShopSlot)
            {
                return;
            }
        }
        
        if (multiSelectedSlots.Contains(slot))
        {
            multiSelectedSlots.Remove(slot);
            RemoveSelectionColor(slot);
            
            if (multiSelectedSlots.Count == 0)
            {
                DeselectAll();
                return;
            }
        }
        else
        {
            multiSelectedSlots.Add(slot);
            ApplySelectionColor(slot);
            
            currentSelectionType = SelectionType.Equipment;
            isInventorySelection = !isShopSlot;
        }
        
        HideQuantityPanel();
        ShowPricePanel();
        UpdatePriceText();
        ShowAppropriateButton();
        
        LogDebug($"장비 선택: {multiSelectedSlots.Count}개");
    }
    
    private void DeselectAll()
    {
        if (singleSelectedSlot != null)
        {
            RemoveSelectionColor(singleSelectedSlot);
            singleSelectedSlot = null;
        }
        
        foreach (var slot in multiSelectedSlots)
        {
            RemoveSelectionColor(slot);
        }
        multiSelectedSlots.Clear();
        
        currentSelectionType = SelectionType.None;
        isInventorySelection = false;
        selectedQuantity = 1;
        maxSelectableQuantity = 1;
        
        HideQuantityPanel();
        HidePricePanel();
        HideAllButtons();
        
        LogDebug("모든 선택 해제");
    }
    
    private bool IsSlotInShopGrid(SlotUI slot)
    {
        return slot.transform.parent != null && slot.transform.parent.name == "ShopGrid";
    }
    
    private void ApplySelectionColor(SlotUI slot)
    {
        Image slotImage = slot.GetComponent<Image>();
        if (slotImage != null)
        {
            if (!originalColors.ContainsKey(slot))
            {
                originalColors[slot] = slotImage.color;
            }
            slotImage.color = selectedColor;
        }
    }
    
    private void RemoveSelectionColor(SlotUI slot)
    {
        Image slotImage = slot.GetComponent<Image>();
        if (slotImage != null && originalColors.ContainsKey(slot))
        {
            slotImage.color = originalColors[slot];
            originalColors.Remove(slot);
        }
    }
    
    #endregion
    
    #region UI Updates
    
    private void ShowQuantityPanel()
    {
        if (quantityPanel != null)
        {
            quantityPanel.SetActive(true);
        }
    }
    
    private void HideQuantityPanel()
    {
        if (quantityPanel != null)
        {
            quantityPanel.SetActive(false);
        }
    }
    
    private void ShowPricePanel()
    {
        if (pricePanel != null)
        {
            pricePanel.SetActive(true);
        }
    }
    
    private void HidePricePanel()
    {
        if (pricePanel != null)
        {
            pricePanel.SetActive(false);
        }
    }
    
    private void ShowAppropriateButton()
    {
        if (isInventorySelection)
        {
            if (sellButton != null) sellButton.gameObject.SetActive(true);
            if (buyButton != null) buyButton.gameObject.SetActive(false);
        }
        else
        {
            if (buyButton != null) buyButton.gameObject.SetActive(true);
            if (sellButton != null) sellButton.gameObject.SetActive(false);
        }
    }
    
    private void HideAllButtons()
    {
        if (buyButton != null) buyButton.gameObject.SetActive(false);
        if (sellButton != null) sellButton.gameObject.SetActive(false);
    }
    
    private void UpdateQuantityText()
    {
        if (quantityText == null) return;
        
        if (singleSelectedSlot != null && singleSelectedSlot.currentItem != null)
        {
            int actualQuantity = singleSelectedSlot.quantity;
            quantityText.text = $"수량: {selectedQuantity} / {actualQuantity}";
        }
    }
    
    private void UpdatePriceText()
    {
        if (priceText == null) return;
        
        int totalPrice = 0;
        
        if (currentSelectionType == SelectionType.ConsumableIngredient && singleSelectedSlot != null)
        {
            ItemData item = singleSelectedSlot.currentItem;
            int pricePerItem = isInventorySelection ? item.sellPrice : item.buyPrice;
            totalPrice = pricePerItem * selectedQuantity;
        }
        else if (currentSelectionType == SelectionType.Equipment)
        {
            foreach (var slot in multiSelectedSlots)
            {
                if (slot.currentItem != null)
                {
                    int pricePerItem = isInventorySelection ? slot.currentItem.sellPrice : slot.currentItem.buyPrice;
                    totalPrice += pricePerItem * slot.quantity;
                }
            }
        }
        
        priceText.text = $"총 가격: {totalPrice} Gold";
    }
    
    #endregion
    
    #region Quantity Control
    
    private void DecreaseQuantity()
    {
        if (selectedQuantity > 1)
        {
            selectedQuantity--;
            UpdateQuantityText();
            UpdatePriceText();
        }
    }
    
    private void IncreaseQuantity()
    {
        if (selectedQuantity < maxSelectableQuantity)
        {
            selectedQuantity++;
            UpdateQuantityText();
            UpdatePriceText();
        }
    }
    
    #endregion
    
    #region Buy/Sell
    
    private void OnBuyButtonClicked()
    {
        if (currentSelectionType == SelectionType.ConsumableIngredient && singleSelectedSlot != null)
        {
            BuyConsumableIngredient();
        }
        else if (currentSelectionType == SelectionType.Equipment)
        {
            BuyEquipment();
        }
    }
    
    private void OnSellButtonClicked()
    {
        if (currentSelectionType == SelectionType.ConsumableIngredient && singleSelectedSlot != null)
        {
            SellConsumableIngredient();
        }
        else if (currentSelectionType == SelectionType.Equipment)
        {
            SellEquipment();
        }
    }
    
    private void BuyConsumableIngredient()
    {
        if (singleSelectedSlot == null || singleSelectedSlot.currentItem == null) return;
        
        ItemData item = singleSelectedSlot.currentItem;
        int totalCost = item.buyPrice * selectedQuantity;
        
        if (InventoryManager.Instance.GetGold() < totalCost)
        {
            return;
        }
        
        bool added = InventoryManager.Instance.AddItem(item, selectedQuantity);
        
        if (added)
        {
            InventoryManager.Instance.AddGold(-totalCost);
            
            singleSelectedSlot.quantity -= selectedQuantity;
            if (singleSelectedSlot.quantity <= 0)
            {
                singleSelectedSlot.ClearSlot();
            }
            else
            {
                singleSelectedSlot.UpdateUI();
            }
            
            DeselectAll();
            SortShopGrid();
            UpdateInventoryDisplay();
            UpdateGoldDisplay();
            
        }
    }
    
    private void BuyEquipment()
    {
        int totalCost = 0;
        List<(ItemData item, int quantity)> itemsToBuy = new List<(ItemData, int)>();
        
        foreach (var slot in multiSelectedSlots)
        {
            if (slot.currentItem != null)
            {
                totalCost += slot.currentItem.buyPrice * slot.quantity;
                itemsToBuy.Add((slot.currentItem, slot.quantity));
            }
        }
        
        if (InventoryManager.Instance.GetGold() < totalCost)
        {
            return;
        }
        
        foreach (var (item, quantity) in itemsToBuy)
        {
            InventoryManager.Instance.AddItem(item, quantity);
        }
        
        InventoryManager.Instance.AddGold(-totalCost);
        
        foreach (var slot in multiSelectedSlots)
        {
            slot.ClearSlot();
        }
        
        DeselectAll();
        SortShopGrid();
        UpdateInventoryDisplay();
        UpdateGoldDisplay();
        
    }
    
    private void SellConsumableIngredient()
    {
        if (singleSelectedSlot == null || singleSelectedSlot.currentItem == null) return;
        
        ItemData item = singleSelectedSlot.currentItem;
        int totalPrice = item.sellPrice * selectedQuantity;
        
        bool removed = InventoryManager.Instance.RemoveItem(item, selectedQuantity);
        
        if (removed)
        {
            InventoryManager.Instance.AddGold(totalPrice);
            
            DeselectAll();
            UpdateInventoryDisplay();
            UpdateGoldDisplay();
            
        }
    }
    
    private void SellEquipment()
    {
        int totalPrice = 0;
        List<(ItemData item, int quantity)> itemsToSell = new List<(ItemData, int)>();
        
        foreach (var slot in multiSelectedSlots)
        {
            if (slot.currentItem != null)
            {
                totalPrice += slot.currentItem.sellPrice * slot.quantity;
                itemsToSell.Add((slot.currentItem, slot.quantity));
            }
        }
        
        foreach (var (item, quantity) in itemsToSell)
        {
            InventoryManager.Instance.RemoveItem(item, quantity);
        }
        
        InventoryManager.Instance.AddGold(totalPrice);
        
        DeselectAll();
        UpdateInventoryDisplay();
        UpdateGoldDisplay();
        
    }
    
    #endregion
    
    #region Random Shop Item Generation
    
    private void GenerateRandomShopItems()
    {
        foreach (var slot in shopSlots)
        {
            slot.ClearSlot();
        }
        
        if (itemDatabase == null || itemDatabase.allItems.Count == 0)
        {
            return;
        }
        
        int maxTier = itemDatabase.GetMaxTier();
        var tierWeights = WeightedRandomSelector.CalculateTierWeights(village, maxTier);
        
        LogDebug($"=== 상점 생성 (Village {village}) ===");
        foreach (var kvp in tierWeights)
        {
            LogDebug($"  Tier {kvp.Key}: {kvp.Value:F1}%");
        }
        
        List<(ItemData item, int quantity)> generatedItems = new List<(ItemData, int)>();
        
        for (int i = 0; i < shopItemSlotCount; i++)
        {
            int selectedTier = WeightedRandomSelector.SelectTierByWeight(tierWeights);
            List<ItemData> tierItems = itemDatabase.GetItemsByTier(selectedTier);
            
            if (tierItems.Count == 0)
            {
                LogDebug($"Tier {selectedTier} 아이템이 없습니다. 건너뜁니다.");
                continue;
            }
            
            ItemData selectedItem = WeightedRandomSelector.SelectRandom(tierItems);
            if (selectedItem == null) continue;
            
            int quantity = CalculateItemQuantity(selectedItem);
            
            bool isDuplicate = false;
            for (int j = 0; j < generatedItems.Count; j++)
            {
                if (generatedItems[j].item == selectedItem)
                {
                    generatedItems[j] = (generatedItems[j].item, generatedItems[j].quantity + quantity);
                    isDuplicate = true;
                    break;
                }
            }
            
            if (!isDuplicate)
            {
                generatedItems.Add((selectedItem, quantity));
            }
            
            LogDebug($"  생성: [T{selectedTier}] {selectedItem.itemName} x{quantity}");
        }
        
        for (int i = 0; i < generatedItems.Count && i < shopSlots.Count; i++)
        {
            var (item, quantity) = generatedItems[i];
            shopSlots[i].SetItem(item, quantity);
            shopSlots[i].UpdateUI();
        }
        
        LogDebug($"상점 아이템 {generatedItems.Count}종류 생성 완료!");
    }
    
    private void RefreshShop()
    {
        DeselectAll();
        GenerateRandomShopItems();
        LogDebug("🔄 상점 재고 새로고침!");
    }
    
    /// <summary>
    /// 아이템 판매 수량 계산 (Village 레벨에 따라 동적 증가)
    /// </summary>
    /// <param name="item">계산할 아이템</param>
    /// <returns>해당 아이템의 판매 수량</returns>
    private int CalculateItemQuantity(ItemData item)
    {
        bool isConsumableOrIngredient = item.itemType == ItemType.Consumable || 
                                         item.itemType == ItemType.Ingredient;
        
        if (isConsumableOrIngredient)
        {
            // Consumable/Ingredient: Village 레벨에 따라 최대 수량 증가
            // 공식: 최대 수량 = baseMaxQuantityPerItem + (village * 2)
            // 예: village 0 = 3개, village 1 = 5개, village 2 = 7개, village 3 = 9개
            int dynamicMaxQuantity = baseMaxQuantityPerItem + (village * 2);
            return WeightedRandomSelector.RandomQuantity(minQuantityPerItem, dynamicMaxQuantity);
        }
        else
        {
            // 장비류: 항상 1개로 고정
            return 1;
        }
    }
    
    #endregion
    
    #region Timer System
    
    private void UpdateRefreshTimer()
    {
        refreshTimer -= Time.deltaTime;
        
        if (refreshTimer <= 0f)
        {
            if (isShopOpen)
            {
                RefreshShop();
            }
            else
            {
                needsRefresh = true;
            }
            
            refreshTimer = refreshInterval;
        }
        
        if (isShopOpen && refreshTimerText != null)
        {
            int totalSeconds = Mathf.CeilToInt(refreshTimer);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            
            // 분:초 형식으로 표시 (예: 01:40)
            refreshTimerText.text = $"재고 새로고침: {minutes:00}:{seconds:00}";
        }
    }
    
    #endregion
    
    #region Shop Management
    
    public void OnBuyTabClicked()
    {
        SetMode(ShopMode.Buy);
    }
    
    public void OnSellTabClicked()
    {
        SetMode(ShopMode.Sell);
    }
    
    private void SetMode(ShopMode mode)
    {
        currentMode = mode;
        DeselectAll();
        
        if (mode == ShopMode.Buy)
        {
            GenerateRandomShopItems();
            LogDebug("구매 모드");
        }
        else
        {
            ClearShopSlots();
            LogDebug("판매 모드");
        }
    }
    
    private void ClearShopSlots()
    {
        foreach (var slot in shopSlots)
        {
            slot.ClearSlot();
        }
    }
    
    private void SortShopGrid()
    {
        List<(ItemData item, int quantity)> items = new List<(ItemData, int)>();
        
        foreach (var slot in shopSlots)
        {
            if (slot.currentItem != null)
            {
                items.Add((slot.currentItem, slot.quantity));
            }
        }
        
        foreach (var slot in shopSlots)
        {
            slot.ClearSlot();
        }
        
        for (int i = 0; i < items.Count && i < shopSlots.Count; i++)
        {
            shopSlots[i].SetItem(items[i].item, items[i].quantity);
            shopSlots[i].UpdateUI();
        }
    }
    
    private void UpdateInventoryDisplay()
    {
        if (InventoryManager.Instance == null) return;
        
        foreach (var slot in inventorySlots)
        {
            slot.ClearSlot();
        }
        
        var items = InventoryManager.Instance.GetAllItems();
        
        List<(ItemData item, int quantity)> sortedItems = new List<(ItemData, int)>();
        foreach (var itemData in items)
        {
            if (itemData.item != null)
            {
                sortedItems.Add(itemData);
            }
        }
        
        for (int i = 0; i < sortedItems.Count && i < inventorySlots.Count; i++)
        {
            inventorySlots[i].SetItem(sortedItems[i].item, sortedItems[i].quantity);
            inventorySlots[i].UpdateUI();
        }
    }
    
    private void UpdateGoldDisplay()
    {
        if (goldText == null) return;
        if (InventoryManager.Instance == null) return;
        
        int currentGold = InventoryManager.Instance.GetGold();
        goldText.text = $"{currentGold} Gold";
    }
    
    #endregion
    
    #region Utility
    
    public void SetVillageLevel(int newVillage)
    {
        village = newVillage;
        if (isShopOpen && currentMode == ShopMode.Buy)
        {
            RefreshShop();
        }
        LogDebug($"Village 레벨 변경: {village}");
    }
    
    public int GetVillageLevel()
    {
        return village;
    }
    
    private void LogDebug(string message)
    {
            }
    
    #endregion
}