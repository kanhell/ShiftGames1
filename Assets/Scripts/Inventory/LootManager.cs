// Assets/Scripts/Inventory/LootManager.cs

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전리품 시스템 관리 (완전 독립)
/// </summary>
public class LootManager : MonoBehaviour
{
    public static LootManager Instance { get; private set; }
    
    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject lootPanel;
    [SerializeField] private Transform lootGrid;
    
    [Header("Slot Settings")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private int maxLootSlots = 12;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    #endregion
    
    #region Public Fields
    public List<SlotUI> lootSlots = new List<SlotUI>();
    #endregion
    
    #region Private Fields
    private bool isLootOpen = false;
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        InitializeLootSlots();
        CloseLootPanel();
    }
    
    private void Update()
    {
        HandleInput();
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
    
    private void InitializeLootSlots()
    {
        // 기존 슬롯 수집
        CollectExistingSlots();
        
        // 부족한 슬롯 생성
        CreateAdditionalSlots();
        
        LogDebug($"전리품 슬롯 {lootSlots.Count}개 초기화 완료");
    }
    
    private void CollectExistingSlots()
    {
        foreach (Transform child in lootGrid)
        {
            SlotUI slot = child.GetComponent<SlotUI>();
            if (slot != null)
            {
                slot.slotType = SlotType.Inventory;
                lootSlots.Add(slot);
            }
        }
    }
    
    private void CreateAdditionalSlots()
    {
        while (lootSlots.Count < maxLootSlots)
        {
            GameObject newSlot = Instantiate(slotPrefab, lootGrid);
            SlotUI slotUI = newSlot.GetComponent<SlotUI>();
            
            if (slotUI != null)
            {
                slotUI.slotType = SlotType.Inventory;
                lootSlots.Add(slotUI);
            }
        }
    }
    #endregion
    
    #region Input Handling
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleLootPanel();
        }
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// 전리품 창 열기/닫기
    /// </summary>
    public void ToggleLootPanel()
    {
        if (isLootOpen)
        {
            CloseLootPanel();
        }
        else
        {
            OpenLootPanel();
        }
    }
    
    /// <summary>
    /// 전리품 창 열기
    /// </summary>
    public void OpenLootPanel()
    {
        if (!ValidateLootPanel()) return;
        
        isLootOpen = true;
        lootPanel.SetActive(true);
        
        LogDebug("전리품 창 열림");
    }
    
    /// <summary>
    /// 전리품 창 닫기
    /// </summary>
    public void CloseLootPanel()
    {
        if (!ValidateLootPanel()) return;
        
        isLootOpen = false;
        lootPanel.SetActive(false);
        
        LogDebug("전리품 창 닫힘");
    }
    
    /// <summary>
    /// 전리품에 아이템 추가
    /// </summary>
    public void AddLoot(ItemData item, int amount = 1)
    {
        if (item == null)
        {
            Debug.LogWarning("AddLoot: item이 null입니다!");
            return;
        }
        
        // 기존 슬롯에 스택 가능하면 추가
        amount = TryStackToExistingSlots(item, amount);
        
        // 남은 수량을 빈 슬롯에 추가
        amount = TryAddToEmptySlots(item, amount);
        
        // 공간 부족 경고
        if (amount > 0)
        {
            Debug.LogWarning($"{item.itemName} {amount}개를 추가할 전리품 공간이 부족합니다!");
        }
    }
    
    /// <summary>
    /// 전리품 전체 획득
    /// </summary>
    public void TakeAllLoot()
    {
        foreach (SlotUI slot in lootSlots)
        {
            if (slot.currentItem != null && slot.quantity > 0)
            {
                InventoryManager.Instance.AddItem(slot.currentItem, slot.quantity);
                slot.ClearSlot();
            }
        }
        
        Debug.Log("모든 전리품을 획득했습니다!");
        CloseLootPanel();
    }
    
    /// <summary>
    /// 전리품 전체 삭제
    /// </summary>
    public void ClearAllLoot()
    {
        foreach (SlotUI slot in lootSlots)
        {
            slot.ClearSlot();
        }
        
        Debug.Log("전리품을 모두 비웠습니다.");
    }
    #endregion
    
    #region Private Methods
    private bool ValidateLootPanel()
    {
        if (lootPanel == null)
        {
            Debug.LogError("lootPanel이 null입니다!");
            return false;
        }
        return true;
    }
    
    private int TryStackToExistingSlots(ItemData item, int amount)
    {
        foreach (SlotUI slot in lootSlots)
        {
            if (slot.currentItem == item && slot.quantity < item.stackSize)
            {
                int space = item.stackSize - slot.quantity;
                int addAmount = Mathf.Min(space, amount);
                
                slot.quantity += addAmount;
                slot.UpdateUI();
                
                amount -= addAmount;
                if (amount <= 0) break;
            }
        }
        
        return amount;
    }
    
    private int TryAddToEmptySlots(ItemData item, int amount)
    {
        foreach (SlotUI slot in lootSlots)
        {
            if (slot.currentItem == null)
            {
                int addAmount = Mathf.Min(item.stackSize, amount);
                slot.SetItem(item, addAmount);
                
                amount -= addAmount;
                if (amount <= 0) break;
            }
        }
        
        return amount;
    }
    
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[LootManager] {message}");
        }
    }
    #endregion
}