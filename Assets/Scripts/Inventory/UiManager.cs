using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전체 UI 관리 (인벤토리, 장비, 요리 등)
/// ESC 키로 최상위 패널부터 닫기 지원
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("UI Panels")]
    [SerializeField] private GameObject inventoryPanel; // 인벤토리 패널
    [SerializeField] private GameObject equipmentPanel; // 장비 패널
    [SerializeField] private GameObject cookingPanel; // 요리 패널
    
    [Header("Cooking Button")]
    [SerializeField] private GameObject cookingButtonPanel; // 요리 버튼 패널
    [SerializeField] private Button cookingButton; // 요리 버튼
    
    private bool isInventoryOpen = false;
    private bool isEquipmentOpen = false;
    private bool isCookingOpen = false;
    private bool isCookingButtonOpen = false;
    
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
        // 초기에는 모두 비활성화
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(false);
        }
        
        if (cookingButtonPanel != null)
        {
            cookingButtonPanel.SetActive(false);
        }
        
        // 요리 버튼 이벤트 연결
        if (cookingButton != null)
        {
            cookingButton.onClick.AddListener(OnCookingButtonClicked);
        }
    }
    
    private void Update()
    {
        bool isSplitPanelOpen = ItemSplitManager.Instance != null && ItemSplitManager.Instance.IsOpen();
        
        bool isShopOpen = ShopManager.Instance != null && ShopManager.Instance.IsShopOpen();
        
        // ESC 키: 항상 처리
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
            return;
        }
        
        if (isSplitPanelOpen || isShopOpen)
        {
            return;
        }
        
        // I 키: 인벤토리만 토글
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventoryOnly();
        }
        
        // G 키: 장비창만 토글
        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleEquipmentOnly();
        }
        
        // H 키: 요리 버튼 토글
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleCookingButton();
        }
    }
    
    /// <summary>
    /// ESC 키 처리 - 최상위 패널 닫기
    /// </summary>
    private void HandleEscapeKey()
    {
        if (ItemSplitManager.Instance != null && ItemSplitManager.Instance.IsOpen())
        {
            ItemSplitManager.Instance.ClosePanel();
            return;
        }
        
        if (PanelStackManager.Instance != null)
        {
            bool closed = PanelStackManager.Instance.CloseTopPanel();
            
                                }
    }
    
    /// <summary>
    /// 인벤토리만 열기/닫기
    /// </summary>
    public void ToggleInventoryOnly()
    {
        isInventoryOpen = !isInventoryOpen;
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
        }
    }
    
    /// <summary>
    /// 인벤토리만 열기 (외부 호출용)
    /// </summary>
    public void OpenInventoryOnly()
    {
        if (!isInventoryOpen)
        {
            isInventoryOpen = true;
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// 인벤토리만 닫기 (외부 호출용)
    /// </summary>
    public void CloseInventoryOnly()
    {
        if (isInventoryOpen)
        {
            isInventoryOpen = false;
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
            
            if (ItemTooltip.Instance != null)
            {
                ItemTooltip.Instance.HideTooltipIfFromPanel(ItemTooltip.PanelSource.Inventory);
            }
        }
    }
    
    /// <summary>
    /// 장비창만 열기/닫기
    /// </summary>
    public void ToggleEquipmentOnly()
    {
        isEquipmentOpen = !isEquipmentOpen;
        
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(isEquipmentOpen);
        }
    }
    
    /// <summary>
    /// 장비창만 열기 (외부 호출용)
    /// </summary>
    public void OpenEquipmentOnly()
    {
        if (!isEquipmentOpen)
        {
            isEquipmentOpen = true;
            if (equipmentPanel != null)
            {
                equipmentPanel.SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// 장비창만 닫기 (외부 호출용)
    /// </summary>
    public void CloseEquipmentOnly()
    {
        if (isEquipmentOpen)
        {
            isEquipmentOpen = false;
            if (equipmentPanel != null)
            {
                equipmentPanel.SetActive(false);
            }
            
            if (ItemTooltip.Instance != null)
            {
                ItemTooltip.Instance.HideTooltipIfFromPanel(ItemTooltip.PanelSource.Equipment);
            }
        }
    }
    
    /// <summary>
    /// 인벤토리 + 장비창 열기/닫기 (기존 메서드 - 호환용)
    /// </summary>
    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        isEquipmentOpen = isInventoryOpen;
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
        }
        
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(isInventoryOpen);
        }
        
    }
    
    /// <summary>
    /// 요리 버튼 표시/숨김
    /// </summary>
    public void ToggleCookingButton()
    {
        if (cookingButtonPanel != null)
        {
            bool isActive = cookingButtonPanel.activeSelf;
            cookingButtonPanel.SetActive(!isActive);
        }
    }
    
    /// <summary>
    /// 요리 버튼 클릭 시
    /// </summary>
    private void OnCookingButtonClicked()
    {
        
        // 요리 버튼 숨기기
        if (cookingButtonPanel != null)
        {
            cookingButtonPanel.SetActive(false);
        }
        
        // 요리창 열기
        if (CookingManager.Instance != null)
        {
            CookingManager.Instance.OpenCookingPanel();
        }
                // 인벤토리만 자동으로 열기 (장비창은 제외)
        if (inventoryPanel != null && !inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(true);
            isInventoryOpen = true; // 상태 업데이트
        }
    }
    
    /// <summary>
    /// 인벤토리 열기 (외부 호출용)
    /// </summary>
    public void OpenInventory()
    {
        if (!isInventoryOpen)
        {
            ToggleInventory();
        }
    }
    
    /// <summary>
    /// 인벤토리 닫기 (외부 호출용)
    /// </summary>
    public void CloseInventory()
    {
        if (isInventoryOpen)
        {
            ToggleInventory();
        }
    }
    
    /// <summary>
    /// 모든 패널 닫기 (상점 열 때 사용)
    /// </summary>
    public void CloseAllPanels()
    {
        // 인벤토리 닫기
        if (isInventoryOpen && inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isInventoryOpen = false;
        }
        
        // 장비창 닫기
        if (isEquipmentOpen && equipmentPanel != null)
        {
            equipmentPanel.SetActive(false);
            isEquipmentOpen = false;
        }
        
        // 요리창 닫기
        if (isCookingOpen && cookingPanel != null)
        {
            cookingPanel.SetActive(false);
            isCookingOpen = false;
        }
        
        // 요리 버튼 닫기
        if (isCookingButtonOpen && cookingButtonPanel != null)
        {
            cookingButtonPanel.SetActive(false);
            isCookingButtonOpen = false;
        }
        
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideTooltip();
        }
        
    }
    
    /// <summary>
    /// 아무 패널이라도 열려있는지 확인
    /// </summary>
    public bool IsAnyPanelOpen()
    {
        return isInventoryOpen || isEquipmentOpen || isCookingOpen;
    }
    
    /// <summary>
    /// 인벤토리가 열려있는지 확인
    /// </summary>
    public bool IsInventoryOpen()
    {
        return isInventoryOpen;
    }
    
    /// <summary>
    /// 장비창이 열려있는지 확인
    /// </summary>
    public bool IsEquipmentOpen()
    {
        return isEquipmentOpen;
    }
}