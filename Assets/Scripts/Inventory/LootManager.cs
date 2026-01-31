// Assets/Scripts/Inventory/LootManager.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전리품 시스템 관리 (인벤토리 이동 애니메이션 포함)
/// </summary>
public class LootManager : MonoBehaviour
{
    public static LootManager Instance;
    
    [Header("UI References")]
    public GameObject lootPanel;
    public Transform lootGrid;
    public RectTransform inventoryPanel; // 인벤토리 패널
    
    [Header("Slot Prefab")]
    public GameObject slotPrefab;
    
    [Header("Loot Data")]
    public List<SlotUI> lootSlots = new List<SlotUI>();
    public int maxLootSlots = 12;
    
    [Header("Animation Settings")]
    public float animationDuration = 0.3f; // 애니메이션 시간
    
    private bool isLootOpen = false;
    private Vector2 inventoryOriginalPos; // 인벤토리 원래 위치
    private Vector2 inventoryShiftedPos = new Vector2(-400f, 0f); // 왼쪽으로 이동한 위치
    
    private void Awake()
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
    
    private void Start()
    {
        InitializeLootSlots();
        lootPanel.SetActive(false);
        
        // 인벤토리 원래 위치 저장
        if (inventoryPanel != null)
        {
            inventoryOriginalPos = inventoryPanel.anchoredPosition;
        }
    }
    
    private void Update()
    {
        // P 키로 전리품 창 토글
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleLootPanel();
        }
    }
    
    /// <summary>
    /// 전리품 슬롯 초기화
    /// </summary>
    private void InitializeLootSlots()
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
        
        while (lootSlots.Count < maxLootSlots)
        {
            GameObject newSlot = Instantiate(slotPrefab, lootGrid);
            SlotUI slotUI = newSlot.GetComponent<SlotUI>();
            slotUI.slotType = SlotType.Inventory;
            lootSlots.Add(slotUI);
        }
        
        Debug.Log($"전리품 슬롯 {lootSlots.Count}개 초기화 완료");
    }
    
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
    /// 전리품 창 열기 (인벤토리 이동 애니메이션)
    /// </summary>
    public void OpenLootPanel()
    {
        isLootOpen = true;
        lootPanel.SetActive(true);
        
        // 인벤토리를 왼쪽으로 이동
        StartCoroutine(MoveInventoryPanel(inventoryShiftedPos));
        
        Debug.Log("전리품 창 열림");
    }
    
    /// <summary>
    /// 전리품 창 닫기 (인벤토리 복원 애니메이션)
    /// </summary>
    public void CloseLootPanel()
    {
        isLootOpen = false;
        
        // 인벤토리를 원래 위치로 복원
        StartCoroutine(MoveInventoryPanel(inventoryOriginalPos, () => 
        {
            lootPanel.SetActive(false);
        }));
        
        Debug.Log("전리품 창 닫힘");
    }
    
    /// <summary>
    /// 인벤토리 패널 이동 애니메이션
    /// </summary>
    private IEnumerator MoveInventoryPanel(Vector2 targetPos, System.Action onComplete = null)
    {
        if (inventoryPanel == null) yield break;
        
        Vector2 startPos = inventoryPanel.anchoredPosition;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            // 부드러운 이동 (Ease Out)
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            inventoryPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        inventoryPanel.anchoredPosition = targetPos;
        onComplete?.Invoke();
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
        
        foreach (SlotUI slot in lootSlots)
        {
            if (slot.currentItem == item && slot.quantity < item.stackSize)
            {
                int space = item.stackSize - slot.quantity;
                int addAmount = Mathf.Min(space, amount);
                
                slot.quantity += addAmount;
                slot.UpdateUI();
                
                amount -= addAmount;
                if (amount <= 0) return;
            }
        }
        
        foreach (SlotUI slot in lootSlots)
        {
            if (slot.currentItem == null)
            {
                int addAmount = Mathf.Min(item.stackSize, amount);
                slot.SetItem(item, addAmount);
                
                amount -= addAmount;
                if (amount <= 0) return;
            }
        }
        
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
}
