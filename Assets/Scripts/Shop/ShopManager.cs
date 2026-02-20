// Assets/Scripts/Shop/ShopManager.cs

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점 시스템 관리
/// Y 키로 열기/닫기
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }
    
    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform shopGrid; // 상점 아이템 그리드
    
    [Header("Tab Buttons")]
    [SerializeField] private GameObject buyTab;
    [SerializeField] private GameObject sellTab;
    
    [Header("Shop Items")]
    [SerializeField] private ItemData[] shopItems; // 상점에서 판매할 아이템들
    [SerializeField] private GameObject shopSlotPrefab;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    #endregion
    
    #region Private Fields
    private bool isShopOpen = false;
    private ShopMode currentMode = ShopMode.Buy;
    private List<SlotUI> shopSlots = new List<SlotUI>();
    #endregion
    
    #region Enums
    private enum ShopMode
    {
        Buy,  // 구매 모드
        Sell  // 판매 모드
    }
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        InitializeShop();
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
    
    private void InitializeShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        
        // 상점 슬롯 생성 (6x6 = 36칸)
        CreateShopSlots(36);
        
        // 구매 모드로 시작
        SetMode(ShopMode.Buy);
    }
    
    private void CreateShopSlots(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject slotObj = Instantiate(shopSlotPrefab, shopGrid);
            SlotUI slot = slotObj.GetComponent<SlotUI>();
            
            if (slot != null)
            {
                slot.slotType = SlotType.Inventory; // 상점 슬롯도 인벤토리 타입
                shopSlots.Add(slot);
            }
        }
        
        LogDebug($"상점 슬롯 {count}개 생성 완료");
    }
    #endregion
    
    #region Input Handling
    private void HandleInput()
    {
        // Y 키: 상점 열기/닫기
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ToggleShop();
        }
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// 상점 열기/닫기
    /// </summary>
    public void ToggleShop()
    {
        if (isShopOpen)
        {
            CloseShop();
        }
        else
        {
            OpenShop();
        }
    }
    
    /// <summary>
    /// 상점 열기
    /// </summary>
    public void OpenShop()
    {
        if (shopPanel == null)
        {
            Debug.LogError("shopPanel이 null입니다!");
            return;
        }
        
        isShopOpen = true;
        shopPanel.SetActive(true);
        
        // 인벤토리도 함께 열기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenInventoryOnly();
        }
        
        // 구매 모드로 설정
        SetMode(ShopMode.Buy);
        
        LogDebug("상점 열림");
    }
    
    /// <summary>
    /// 상점 닫기
    /// </summary>
    public void CloseShop()
    {
        if (shopPanel == null)
        {
            Debug.LogError("shopPanel이 null입니다!");
            return;
        }
        
        isShopOpen = false;
        shopPanel.SetActive(false);
        
        LogDebug("상점 닫힘");
    }
    
    /// <summary>
    /// 구매 탭 클릭
    /// </summary>
    public void OnBuyTabClicked()
    {
        SetMode(ShopMode.Buy);
    }
    
    /// <summary>
    /// 판매 탭 클릭
    /// </summary>
    public void OnSellTabClicked()
    {
        SetMode(ShopMode.Sell);
    }
    
    /// <summary>
    /// 상점이 열려있는지 확인
    /// </summary>
    public bool IsShopOpen()
    {
        return isShopOpen;
    }
    
    /// <summary>
    /// 현재 판매 모드인지 확인
    /// </summary>
    public bool IsSellMode()
    {
        return currentMode == ShopMode.Sell;
    }
    #endregion
    
    #region Private Methods
    /// <summary>
    /// 모드 설정 (구매/판매)
    /// </summary>
    private void SetMode(ShopMode mode)
    {
        currentMode = mode;
        
        if (mode == ShopMode.Buy)
        {
            // 구매 모드: 상점 아이템 표시
            DisplayShopItems();
            LogDebug("구매 모드");
        }
        else
        {
            // 판매 모드: 상점 슬롯 비우기
            ClearShopSlots();
            LogDebug("판매 모드");
        }
    }
    
    /// <summary>
    /// 상점 아이템 표시
    /// </summary>
    private void DisplayShopItems()
    {
        ClearShopSlots();
        
        if (shopItems == null || shopItems.Length == 0)
        {
            LogDebug("판매할 아이템이 없습니다.");
            return;
        }
        
        for (int i = 0; i < shopItems.Length && i < shopSlots.Count; i++)
        {
            if (shopItems[i] != null)
            {
                shopSlots[i].SetItem(shopItems[i], 1);
                shopSlots[i].UpdateUI();
            }
        }
        
        LogDebug($"상점 아이템 {shopItems.Length}개 표시");
    }
    
    /// <summary>
    /// 상점 슬롯 비우기
    /// </summary>
    private void ClearShopSlots()
    {
        foreach (var slot in shopSlots)
        {
            slot.ClearSlot();
        }
    }
    
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[ShopManager] {message}");
        }
    }
    #endregion
}