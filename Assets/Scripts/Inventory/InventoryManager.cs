// Assets/Scripts/Inventory/InventoryManager.cs

using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 인벤토리 전체 관리 (싱글톤)
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 24;
    [SerializeField] private int goldAmount = 0;
    
    [Header("UI References")]
    [SerializeField] private Transform inventoryGridParent;
    [SerializeField] private GameObject inventorySlotPrefab;
    
    [Header("Equipment Slots")]
    [SerializeField] private SlotUI weaponSlot;
    [SerializeField] private SlotUI helmetSlot;
    [SerializeField] private SlotUI armorSlot;
    [SerializeField] private SlotUI shoesSlot;
    [SerializeField] private SlotUI bagSlot;
    [SerializeField] private SlotUI quiverSlot;
    
    [Header("Quick Slot")]
    [SerializeField] private SlotUI quickSlot;
    
    [Header("Money Display")]
    [SerializeField] private TextMeshProUGUI goldText;
    
    // 런타임 데이터
    private List<SlotUI> inventorySlots = new List<SlotUI>();
    
    private void Awake()
    {
        // 싱글톤
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        InitializeInventorySlots();
        UpdateGoldUI();
    }
    
    /// <summary>
    /// 인벤토리 슬롯 생성
    /// </summary>
    private void InitializeInventorySlots()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, inventoryGridParent);
            SlotUI slot = slotObj.GetComponent<SlotUI>();
            
            if (slot != null)
            {
                slot.slotType = SlotType.Inventory;
                inventorySlots.Add(slot);
            }
        }
    }
    
    /// <summary>
    /// 아이템 추가
    /// </summary>
    public bool AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return false;
        
        // 1) 스택 가능한 아이템이면 기존 슬롯에 추가 시도
        if (item.stackSize > 1)
        {
            foreach (var slot in inventorySlots)
            {
                if (slot.currentItem == item && slot.quantity < item.stackSize)
                {
                    int addable = Mathf.Min(amount, item.stackSize - slot.quantity);
                    slot.quantity += addable;
                    slot.UpdateUI();
                    amount -= addable;
                    
                    if (amount <= 0)
                    {
                        Debug.Log($"아이템 추가 완료: {item.itemName}");
                        return true;
                    }
                }
            }
        }
        
        // 2) 빈 슬롯에 추가
        while (amount > 0)
        {
            SlotUI emptySlot = FindEmptySlot();
            if (emptySlot == null)
            {
                Debug.LogWarning("인벤토리가 가득 찼습니다!");
                return false;
            }
            
            int addAmount = Mathf.Min(amount, item.stackSize);
            emptySlot.SetItem(item, addAmount);
            amount -= addAmount;
        }
        
        Debug.Log($"아이템 추가 완료: {item.itemName}");
        return true;
    }
    
    /// <summary>
    /// 아이템 제거
    /// </summary>
    public bool RemoveItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return false;
        
        int remaining = amount;
        
        // 인벤토리에서 해당 아이템을 찾아서 제거
        foreach (var slot in inventorySlots)
        {
            if (slot.currentItem == item && slot.quantity > 0)
            {
                int removeAmount = Mathf.Min(remaining, slot.quantity);
                slot.quantity -= removeAmount;
                remaining -= removeAmount;
                
                // 수량이 0이 되면 슬롯 비우기
                if (slot.quantity <= 0)
                {
                    slot.ClearSlot();
                }
                else
                {
                    slot.UpdateUI();
                }
                
                if (remaining <= 0)
                {
                    Debug.Log($"아이템 제거 완료: {item.itemName} x{amount}");
                    return true;
                }
            }
        }
        
        if (remaining > 0)
        {
            Debug.LogWarning($"{item.itemName}이(가) 부족합니다! (필요: {amount}, 부족: {remaining})");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 특정 아이템의 총 개수 확인
    /// </summary>
    public int GetItemCount(ItemData item)
    {
        if (item == null) return 0;
        
        int total = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot.currentItem == item)
            {
                total += slot.quantity;
            }
        }
        return total;
    }
    
    /// <summary>
    /// 특정 슬롯의 아이템 제거
    /// </summary>
    public void RemoveItemFromSlot(SlotUI slot, int amount)
    {
        if (slot == null || slot.currentItem == null) return;
        
        amount = Mathf.Min(amount, slot.quantity);
        slot.quantity -= amount;
        
        if (slot.quantity <= 0)
        {
            slot.ClearSlot();
        }
        else
        {
            slot.UpdateUI();
        }
        
        Debug.Log($"슬롯에서 아이템 제거: {amount}개");
    }
    
    /// <summary>
    /// 빈 슬롯 찾기 (Public)
    /// </summary>
    public SlotUI FindEmptyInventorySlot()
    {
        return FindEmptySlot();
    }
    
    /// <summary>
    /// 빈 슬롯 찾기
    /// </summary>
    private SlotUI FindEmptySlot()
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.currentItem == null || slot.quantity <= 0)
            {
                return slot;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 골드 추가
    /// </summary>
    public void AddGold(int amount)
    {
        goldAmount += amount;
        UpdateGoldUI();
        Debug.Log($"골드 +{amount} (총: {goldAmount})");
    }
    
    /// <summary>
    /// 골드 제거
    /// </summary>
    public bool RemoveGold(int amount)
    {
        if (goldAmount < amount)
        {
            Debug.LogWarning("골드가 부족합니다!");
            return false;
        }
        
        goldAmount -= amount;
        UpdateGoldUI();
        Debug.Log($"골드 -{amount} (총: {goldAmount})");
        return true;
    }
    
    /// <summary>
    /// 골드 UI 갱신
    /// </summary>
    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = $"{goldAmount} Gold";
        }
    }
    
    // ─────────────────────────────────────────────
    // 슬롯 이벤트 (드래그 앤 드롭)
    // ─────────────────────────────────────────────
    
    /// <summary>
    /// 드래그로 슬롯 간 아이템 이동/교환
    /// </summary>
    public void TryMoveOrSwapDrag(SlotUI from, SlotUI to)
    {
        if (from == null || to == null || from == to) return;
        
        // 대상 슬롯이 아이템을 받을 수 있는지 확인
        if (!to.CanAcceptItem(from.currentItem))
        {
            Debug.LogWarning($"{to.name}에 {from.currentItem.itemName}을(를) 장착할 수 없습니다!");
            return;
        }
        
        // 같은 아이템이고 스택 가능하면 합치기
        if (to.currentItem == from.currentItem && to.currentItem.stackSize > 1)
        {
            int spaceInTarget = to.currentItem.stackSize - to.quantity;
            if (spaceInTarget > 0)
            {
                int transferAmount = Mathf.Min(from.quantity, spaceInTarget);
                to.quantity += transferAmount;
                from.quantity -= transferAmount;
                
                if (from.quantity <= 0)
                {
                    from.ClearSlot();
                }
                else
                {
                    from.UpdateUI();
                }
                
                to.UpdateUI();
                Debug.Log($"아이템 합치기 완료: {transferAmount}개");
                return;
            }
        }
        
        // 교환
        ItemData tempItem = to.currentItem;
        int tempQuantity = to.quantity;
        
        to.SetItem(from.currentItem, from.quantity);
        from.SetItem(tempItem, tempQuantity);
        
        Debug.Log($"아이템 이동/교환 완료");
    }
    
    /// <summary>
    /// 우클릭: 빠른 장착/사용
    /// </summary>
    public void OnSlotRightClick(SlotUI clickedSlot)
    {
        if (clickedSlot.currentItem == null) return;
        
        // 인벤토리 → 장비 슬롯 자동 장착
        if (clickedSlot.slotType == SlotType.Inventory)
        {
            SlotUI targetEquipSlot = GetEquipmentSlotByType(clickedSlot.currentItem.itemType);
            if (targetEquipSlot != null)
            {
                TryMoveOrSwapDrag(clickedSlot, targetEquipSlot);
            }
            else
            {
                Debug.Log($"{clickedSlot.currentItem.itemName}은(는) 장비 아이템이 아닙니다.");
            }
        }
        // 장비 슬롯 → 인벤토리로 복귀
        else
        {
            SlotUI emptySlot = FindEmptySlot();
            if (emptySlot != null)
            {
                TryMoveOrSwapDrag(clickedSlot, emptySlot);
            }
        }
    }
    
    /// <summary>
    /// 아이템 장착 (컨텍스트 메뉴용)
    /// </summary>
    public void EquipItem(SlotUI inventorySlot)
    {
        if (inventorySlot == null || inventorySlot.currentItem == null) return;
        
        // 장비 슬롯 찾기
        SlotUI equipSlot = GetEquipmentSlotByType(inventorySlot.currentItem.itemType);
        
        if (equipSlot == null)
        {
            Debug.LogWarning($"{inventorySlot.currentItem.itemName}은(는) 장비 아이템이 아닙니다!");
            return;
        }
        
        // 장비 슬롯에 이미 아이템이 있으면 교체
        if (equipSlot.currentItem != null)
        {
            // 교체: 인벤토리 아이템 → 장비 슬롯, 장비 슬롯 → 인벤토리
            ItemData tempItem = equipSlot.currentItem;
            int tempQuantity = equipSlot.quantity;
            
            equipSlot.SetItem(inventorySlot.currentItem, inventorySlot.quantity);
            inventorySlot.SetItem(tempItem, tempQuantity);
            
            // 명시적으로 UI 업데이트
            equipSlot.UpdateUI();
            inventorySlot.UpdateUI();
            
            Debug.Log($"장비 교체: {equipSlot.currentItem.itemName} ↔ {inventorySlot.currentItem.itemName}");
        }
        else
        {
            // 빈 슬롯에 장착
            equipSlot.SetItem(inventorySlot.currentItem, inventorySlot.quantity);
            inventorySlot.ClearSlot();
            
            // 명시적으로 UI 업데이트
            equipSlot.UpdateUI();
            inventorySlot.UpdateUI();
            
            Debug.Log($"장비 장착: {equipSlot.currentItem.itemName}");
        }
    }
    
    /// <summary>
    /// 퀵슬롯에 소비 아이템 장착
    /// </summary>
    public void EquipToQuickSlot(SlotUI inventorySlot)
    {
        if (inventorySlot == null || inventorySlot.currentItem == null)
        {
            Debug.LogWarning("inventorySlot이 null입니다!");
            return;
        }
        
        if (quickSlot == null)
        {
            Debug.LogWarning("QuickSlot이 연결되지 않았습니다!");
            return;
        }
        
        // 퀵슬롯에 이미 아이템이 있으면 교체
        if (quickSlot.currentItem != null)
        {
            // 교체
            ItemData tempItem = quickSlot.currentItem;
            int tempQuantity = quickSlot.quantity;
            
            quickSlot.SetItem(inventorySlot.currentItem, inventorySlot.quantity);
            inventorySlot.SetItem(tempItem, tempQuantity);
            
            quickSlot.UpdateUI();
            inventorySlot.UpdateUI();
            
            Debug.Log($"퀵슬롯 교체: {quickSlot.currentItem.itemName}");
        }
        else
        {
            // 빈 슬롯에 장착
            quickSlot.SetItem(inventorySlot.currentItem, inventorySlot.quantity);
            inventorySlot.ClearSlot();
            
            quickSlot.UpdateUI();
            inventorySlot.UpdateUI();
            
            Debug.Log($"퀵슬롯 장착: {quickSlot.currentItem.itemName}");
        }
    }
    
    /// <summary>
    /// 아이템 타입에 맞는 장비 슬롯 가져오기
    /// </summary>
    private SlotUI GetEquipmentSlotByType(ItemType type)
    {
        Debug.Log($"=== 장비 슬롯 찾기 ===");
        Debug.Log($"찾는 타입: {type}");
        
        SlotUI result = null;
        
        switch (type)
        {
            case ItemType.Weapon: 
                result = weaponSlot;
                Debug.Log($"Weapon 슬롯: {(weaponSlot != null ? weaponSlot.name : "NULL")}");
                break;
            case ItemType.Helmet: 
                result = helmetSlot;
                Debug.Log($"Helmet 슬롯: {(helmetSlot != null ? helmetSlot.name : "NULL")}");
                break;
            case ItemType.Armor: 
                result = armorSlot;
                Debug.Log($"Armor 슬롯: {(armorSlot != null ? armorSlot.name : "NULL")}");
                break;
            case ItemType.Shoes: 
                result = shoesSlot;
                Debug.Log($"Shoes 슬롯: {(shoesSlot != null ? shoesSlot.name : "NULL")}");
                break;
            case ItemType.Bag: 
                result = bagSlot;
                Debug.Log($"Bag 슬롯: {(bagSlot != null ? bagSlot.name : "NULL")}");
                break;
            case ItemType.Quiver: 
                result = quiverSlot;
                Debug.Log($"Quiver 슬롯: {(quiverSlot != null ? quiverSlot.name : "NULL")}");
                break;
            default: 
                Debug.LogWarning($"알 수 없는 타입: {type}");
                break;
        }
        
        return result;
    }
}