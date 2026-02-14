// Assets/Scripts/UI/UIManager.cs

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
    
    [Header("Cooking Button")]
    [SerializeField] private GameObject cookingButtonPanel; // 요리 버튼 패널
    [SerializeField] private Button cookingButton; // 요리 버튼
    
    private bool isInventoryOpen = false;
    private bool isEquipmentOpen = false;
    
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
            Debug.Log("인벤토리 패널 초기화: 비활성화");
        }
        else
        {
            Debug.LogWarning("Inventory Panel이 연결되지 않았습니다!");
        }
        
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(false);
            Debug.Log("장비 패널 초기화: 비활성화");
        }
        else
        {
            Debug.LogWarning("Equipment Panel이 연결되지 않았습니다!");
        }
        
        if (cookingButtonPanel != null)
        {
            cookingButtonPanel.SetActive(false);
            Debug.Log("요리 버튼 패널 초기화: 비활성화");
        }
        
        // 요리 버튼 이벤트 연결
        if (cookingButton != null)
        {
            cookingButton.onClick.AddListener(OnCookingButtonClicked);
        }
    }
    
    private void Update()
    {
        // ✅ 분할 패널이 열려있을 때만 단축키 차단
        bool isSplitPanelOpen = ItemSplitManager.Instance != null && ItemSplitManager.Instance.IsOpen();
        
        // ESC 키: 최상위 패널 닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
        
        // ✅ 분할 패널이 열려있으면 아래 단축키들 무시
        if (isSplitPanelOpen)
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
        // ✅ 분할 패널이 열려있으면 최우선으로 닫기
        if (ItemSplitManager.Instance != null && ItemSplitManager.Instance.IsOpen())
        {
            ItemSplitManager.Instance.ClosePanel();
            Debug.Log("ESC: 분할 패널 닫음");
            return;
        }
        
        if (PanelStackManager.Instance != null)
        {
            bool closed = PanelStackManager.Instance.CloseTopPanel();
            
            if (closed)
            {
                Debug.Log("ESC: 최상위 패널 닫음");
            }
            else
            {
                Debug.Log("ESC: 닫을 패널이 없습니다");
            }
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
            Debug.Log($"[UIManager] 인벤토리 → {(isInventoryOpen ? "열림" : "닫힘")}");
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
                Debug.Log("[UIManager] 인벤토리 열림");
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
                Debug.Log("[UIManager] 인벤토리 닫힘");
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
            Debug.Log($"[UIManager] 장비창 → {(isEquipmentOpen ? "열림" : "닫힘")}");
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
                Debug.Log("[UIManager] 장비창 열림");
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
                Debug.Log("[UIManager] 장비창 닫힘");
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
        
        Debug.Log($"[UIManager] 인벤토리 + 장비 → {(isInventoryOpen ? "열림" : "닫힘")}");
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
            Debug.Log($"요리 버튼 {(!isActive ? "표시" : "숨김")}");
        }
    }
    
    /// <summary>
    /// 요리 버튼 클릭 시
    /// </summary>
    private void OnCookingButtonClicked()
    {
        Debug.Log("=== 요리 버튼 클릭됨 ===");
        
        // 요리 버튼 숨기기
        if (cookingButtonPanel != null)
        {
            cookingButtonPanel.SetActive(false);
            Debug.Log("요리 버튼 패널 숨김");
        }
        
        // 요리창 열기
        if (CookingManager.Instance != null)
        {
            CookingManager.Instance.OpenCookingPanel();
            Debug.Log("요리창 열림");
        }
        else
        {
            Debug.LogError("CookingManager.Instance가 null입니다!");
        }
        
        // 인벤토리만 자동으로 열기 (장비창은 제외)
        if (inventoryPanel != null && !inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(true);
            isInventoryOpen = true; // 상태 업데이트
            Debug.Log("인벤토리 패널만 자동 열림");
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
}