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
    /// 현재 골드 가져오기
    /// </summary>
    public int GetGold()
    {
        return goldAmount;
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
    /// ✅ 드래그 앤 드롭으로 아이템 이동, 교환, 또는 합치기
    /// </summary>
    public void TryMoveOrSwapDrag(SlotUI fromSlot, SlotUI toSlot)
    {
        // 자기 자신에게 드롭한 경우 무시
        if (fromSlot == toSlot)
        {
            Debug.Log("같은 슬롯에 드롭 - 무시");
            return;
        }
        
        Debug.Log($"=== 드래그 이동 시도 ===");
        Debug.Log($"From: {fromSlot.name} - {(fromSlot.currentItem != null ? fromSlot.currentItem.itemName : "빈 슬롯")}");
        Debug.Log($"To: {toSlot.name} - {(toSlot.currentItem != null ? toSlot.currentItem.itemName : "빈 슬롯")}");
        
        // ✅ 1. 같은 아이템이면 합치기 시도
        if (fromSlot.currentItem != null && toSlot.currentItem != null && 
            fromSlot.currentItem == toSlot.currentItem)
        {
            TryStackItems(fromSlot, toSlot);
            return;
        }
        
        // ✅ 2. 둘 다 아이템이 있는 경우 → 교환
        if (fromSlot.currentItem != null && toSlot.currentItem != null)
        {
            SwapItems(fromSlot, toSlot);
            return;
        }
        
        // ✅ 3. 대상 슬롯이 비어있는 경우 → 이동
        if (toSlot.currentItem == null)
        {
            MoveItem(fromSlot, toSlot);
            return;
        }
        
        // ✅ 4. 출발 슬롯이 비어있는 경우 (이론상 발생 안 함)
        Debug.LogWarning("출발 슬롯이 비어있습니다!");
    }
    
    /// <summary>
    /// ✅ 같은 아이템을 합치기
    /// </summary>
    private void TryStackItems(SlotUI fromSlot, SlotUI toSlot)
    {
        Debug.Log($"같은 아이템 합치기: {fromSlot.currentItem.itemName}");
        
        int maxStack = fromSlot.currentItem.stackSize;
        int availableSpace = maxStack - toSlot.quantity;
        
        if (availableSpace <= 0)
        {
            Debug.LogWarning("대상 슬롯이 가득 찼습니다!");
            return;
        }
        
        // 합칠 수 있는 만큼 이동
        int amountToMove = Mathf.Min(fromSlot.quantity, availableSpace);
        
        toSlot.quantity += amountToMove;
        fromSlot.quantity -= amountToMove;
        
        // 원본 슬롯이 비었으면 제거
        if (fromSlot.quantity <= 0)
        {
            fromSlot.ClearSlot();
        }
        else
        {
            fromSlot.UpdateUI();
        }
        
        toSlot.UpdateUI();
        
        Debug.Log($"합치기 완료: {amountToMove}개 이동 (대상: {toSlot.quantity}/{maxStack})");
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
        
        // 드래그 앤 드롭 로직 재사용
        TryMoveOrSwapDrag(inventorySlot, equipSlot);
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
        
        // 소비 아이템만 퀵슬롯에 장착 가능
        if (inventorySlot.currentItem.itemType != ItemType.Consumable)
        {
            Debug.LogWarning($"{inventorySlot.currentItem.itemName}은(는) 소비 아이템이 아닙니다!");
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
            // 같은 아이템이면 합치기 (최대 5개)
            if (quickSlot.currentItem == inventorySlot.currentItem)
            {
                int maxQuickSlot = 5;
                int spaceLeft = maxQuickSlot - quickSlot.quantity;
                
                if (spaceLeft > 0)
                {
                    int addAmount = Mathf.Min(inventorySlot.quantity, spaceLeft);
                    quickSlot.quantity += addAmount;
                    inventorySlot.quantity -= addAmount;
                    
                    if (inventorySlot.quantity <= 0)
                    {
                        inventorySlot.ClearSlot();
                    }
                    else
                    {
                        inventorySlot.UpdateUI();
                    }
                    
                    // ✅ QuickSlot UI 업데이트 (중요!)
                    quickSlot.UpdateUI();
                    Debug.Log($"퀵슬롯에 추가: {addAmount}개 (총 {quickSlot.quantity}개)");
                }
                else
                {
                    Debug.LogWarning("퀵슬롯이 가득 찼습니다! (최대 5개)");
                }
                return;
            }
            
            // 다른 아이템이면 교체
            ItemData tempItem = quickSlot.currentItem;
            int tempQuantity = quickSlot.quantity;
            
            // 최대 5개까지만
            int swapAmount = Mathf.Min(inventorySlot.quantity, 5);
            quickSlot.SetItem(inventorySlot.currentItem, swapAmount);
            
            inventorySlot.quantity -= swapAmount;
            if (inventorySlot.quantity <= 0)
            {
                inventorySlot.SetItem(tempItem, tempQuantity);
            }
            else
            {
                inventorySlot.UpdateUI();
                // 교체된 아이템을 인벤토리에 추가
                AddItem(tempItem, tempQuantity);
            }
            
            quickSlot.UpdateUI();
            Debug.Log($"퀵슬롯 교체: {quickSlot.currentItem.itemName}");
        }
        else
        {
            // 빈 슬롯에 장착 (최대 5개)
            int moveAmount = Mathf.Min(inventorySlot.quantity, 5);
            quickSlot.SetItem(inventorySlot.currentItem, moveAmount);
            inventorySlot.quantity -= moveAmount;
            
            if (inventorySlot.quantity <= 0)
            {
                inventorySlot.ClearSlot();
            }
            else
            {
                inventorySlot.UpdateUI();
            }
            
            quickSlot.UpdateUI();
            Debug.Log($"퀵슬롯 장착: {quickSlot.currentItem.itemName} x{moveAmount}");
        }
    }
    
    /// <summary>
    /// 아이템 타입에 맞는 장비 슬롯 가져오기
    /// </summary>
    private SlotUI GetEquipmentSlotByType(ItemType type)
    {
        switch (type)
        {
            case ItemType.Weapon: return weaponSlot;
            case ItemType.Helmet: return helmetSlot;
            case ItemType.Armor: return armorSlot;
            case ItemType.Shoes: return shoesSlot;
            case ItemType.Bag: return bagSlot;
            case ItemType.Quiver: return quiverSlot;
            default: return null;
        }
    }
    
    // ─────────────────────────────────────────────
    // ✅ 아이템 교환 기능
    // ─────────────────────────────────────────────
    
    /// <summary>
    /// ✅ 두 슬롯의 아이템 교환
    /// </summary>
    private void SwapItems(SlotUI slotA, SlotUI slotB)
    {
        Debug.Log($"아이템 교환 시도: {slotA.currentItem.itemName} ↔ {slotB.currentItem.itemName}");
        
        // ✅ 교환 가능 여부 확인
        bool canSwap = CanSwapItems(slotA, slotB);
        
        if (!canSwap)
        {
            Debug.LogWarning("이 슬롯에는 해당 아이템을 놓을 수 없습니다!");
            return;
        }
        
        // 임시 저장
        ItemData tempItem = slotA.currentItem;
        int tempQuantity = slotA.quantity;
        
        // A → B 데이터 복사
        slotA.SetItem(slotB.currentItem, slotB.quantity);
        
        // 임시 → B
        slotB.SetItem(tempItem, tempQuantity);
        
        // UI 업데이트
        slotA.UpdateUI();
        slotB.UpdateUI();
        
        Debug.Log("교환 완료!");
    }
    
    /// <summary>
    /// ✅ 아이템을 다른 슬롯으로 이동
    /// </summary>
    private void MoveItem(SlotUI fromSlot, SlotUI toSlot)
    {
        Debug.Log($"아이템 이동 시도: {fromSlot.currentItem.itemName} → {toSlot.name}");
        
        // ✅ 이동 가능 여부 확인
        if (!toSlot.CanAcceptItem(fromSlot.currentItem))
        {
            Debug.LogWarning($"{toSlot.name}에는 {fromSlot.currentItem.itemName}을(를) 놓을 수 없습니다!");
            return;
        }
        
        // ✅ 같은 아이템이면 스택
        if (toSlot.currentItem == fromSlot.currentItem)
        {
            // 스택 가능한지 확인
            int maxStack = fromSlot.currentItem.stackSize;
            int availableSpace = maxStack - toSlot.quantity;
            
            if (availableSpace > 0)
            {
                int amountToMove = Mathf.Min(fromSlot.quantity, availableSpace);
                
                toSlot.quantity += amountToMove;
                fromSlot.quantity -= amountToMove;
                
                if (fromSlot.quantity <= 0)
                {
                    fromSlot.ClearSlot();
                }
                else
                {
                    fromSlot.UpdateUI();
                }
                
                toSlot.UpdateUI();
                
                Debug.Log($"스택 완료: {amountToMove}개 이동");
                return;
            }
            else
            {
                Debug.LogWarning("스택이 가득 찼습니다!");
                return;
            }
        }
        
        // ✅ 다른 아이템이면 이동
        toSlot.SetItem(fromSlot.currentItem, fromSlot.quantity);
        fromSlot.ClearSlot();
        
        toSlot.UpdateUI();
        fromSlot.UpdateUI();
        
        Debug.Log("이동 완료!");
    }
    
    /// <summary>
    /// ✅ 두 슬롯의 아이템 교환이 가능한지 확인
    /// </summary>
    private bool CanSwapItems(SlotUI slotA, SlotUI slotB)
    {
        // A의 아이템이 B 슬롯에 들어갈 수 있는지
        bool aToB = slotB.CanAcceptItem(slotA.currentItem);
        
        // B의 아이템이 A 슬롯에 들어갈 수 있는지
        bool bToA = slotA.CanAcceptItem(slotB.currentItem);
        
        if (!aToB)
        {
            Debug.LogWarning($"{slotB.name}은(는) {slotA.currentItem.itemName}을(를) 받을 수 없습니다!");
        }
        
        if (!bToA)
        {
            Debug.LogWarning($"{slotA.name}은(는) {slotB.currentItem.itemName}을(를) 받을 수 없습니다!");
        }
        
        return aToB && bToA;
    }
    
    /// <summary>
    /// 모든 인벤토리 아이템 가져오기 (상점에서 사용)
    /// </summary>
    public List<(ItemData item, int quantity)> GetAllItems()
    {
        List<(ItemData, int)> items = new List<(ItemData, int)>();
        
        foreach (var slot in inventorySlots)
        {
            if (slot.currentItem != null)
            {
                items.Add((slot.currentItem, slot.quantity));
            }
            else
            {
                // 빈 슬롯도 추가 (null, 0)
                items.Add((null, 0));
            }
        }
        
        return items;
    }
}