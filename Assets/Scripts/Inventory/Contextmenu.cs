// Assets/Scripts/Inventory/ContextMenu.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 우클릭 컨텍스트 메뉴 관리
/// </summary>
public class ContextMenu : MonoBehaviour
{
    public static ContextMenu Instance;
    
    [Header("UI References")]
    [SerializeField] private GameObject menuPanel; // 일반 메뉴 (2버튼)
    [SerializeField] private GameObject consumableMenuPanel; // 포션 메뉴 (3버튼)
    [SerializeField] private GameObject equipmentMenuPanel; // 장비 슬롯 메뉴 (2버튼)
    [SerializeField] private GameObject quickSlotMenuPanel; // 퀵슬롯 메뉴 (3버튼)
    
    [Header("확인 팝업")]
    [SerializeField] private GameObject confirmPanel; // 확인 팝업
    [SerializeField] private TextMeshProUGUI confirmMessageText; // 확인 메시지
    [SerializeField] private Button confirmYesButton; // 네 버튼
    [SerializeField] private Button confirmNoButton; // 아니요 버튼
    
    [Header("일반 메뉴 버튼")]
    [SerializeField] private Button useButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private TextMeshProUGUI useButtonText;
    
    [Header("포션 메뉴 버튼")]
    [SerializeField] private Button equipButton; // 장착하기
    [SerializeField] private Button consumeButton; // 소비하기
    [SerializeField] private Button discardButton2; // 버리기
    
    [Header("장비 슬롯 메뉴 버튼")]
    [SerializeField] private Button unequipButton; // 장착해제하기
    [SerializeField] private Button discardButton3; // 버리기
    
    [Header("퀵슬롯 메뉴 버튼")]
    [SerializeField] private Button unequipButton2; // 장착해제하기
    [SerializeField] private Button consumeButton2; // 소비하기
    [SerializeField] private Button discardButton4; // 버리기
    
    private SlotUI targetSlot;
    private RectTransform menuRect;
    private RectTransform consumableMenuRect;
    private RectTransform equipmentMenuRect;
    private RectTransform quickSlotMenuRect;
    
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
        
        menuRect = menuPanel.GetComponent<RectTransform>();
        if (consumableMenuPanel != null)
        {
            consumableMenuRect = consumableMenuPanel.GetComponent<RectTransform>();
        }
        if (equipmentMenuPanel != null)
        {
            equipmentMenuRect = equipmentMenuPanel.GetComponent<RectTransform>();
        }
        if (quickSlotMenuPanel != null)
        {
            quickSlotMenuRect = quickSlotMenuPanel.GetComponent<RectTransform>();
        }
    }
    
    private void Start()
    {
        // 일반 메뉴 버튼 이벤트 연결
        useButton.onClick.AddListener(OnUseButtonClicked);
        discardButton.onClick.AddListener(() => ShowConfirmDialog("정말 버리시겠습니까?"));
        
        // 포션 메뉴 버튼 이벤트 연결
        if (equipButton != null)
        {
            equipButton.onClick.AddListener(OnEquipButtonClicked);
        }
        if (consumeButton != null)
        {
            consumeButton.onClick.AddListener(OnConsumeButtonClicked);
        }
        if (discardButton2 != null)
        {
            discardButton2.onClick.AddListener(() => ShowConfirmDialog("정말 버리시겠습니까?"));
        }
        
        // 장비 슬롯 메뉴 버튼 이벤트 연결
        if (unequipButton != null)
        {
            unequipButton.onClick.AddListener(OnUnequipButtonClicked);
        }
        if (discardButton3 != null)
        {
            discardButton3.onClick.AddListener(() => ShowConfirmDialog("정말 버리시겠습니까?"));
        }
        
        // 퀵슬롯 메뉴 버튼 이벤트 연결
        if (unequipButton2 != null)
        {
            unequipButton2.onClick.AddListener(OnUnequipButtonClicked);
        }
        if (consumeButton2 != null)
        {
            consumeButton2.onClick.AddListener(OnConsumeButtonClicked);
        }
        if (discardButton4 != null)
        {
            discardButton4.onClick.AddListener(() => ShowConfirmDialog("정말 버리시겠습니까?"));
        }
        
        // 확인 팝업 버튼 이벤트 연결
        if (confirmYesButton != null)
        {
            confirmYesButton.onClick.AddListener(OnConfirmYes);
        }
        if (confirmNoButton != null)
        {
            confirmNoButton.onClick.AddListener(OnConfirmNo);
        }
        
        // 초기에는 비활성화
        menuPanel.SetActive(false);
        if (consumableMenuPanel != null)
        {
            consumableMenuPanel.SetActive(false);
        }
        if (equipmentMenuPanel != null)
        {
            equipmentMenuPanel.SetActive(false);
        }
        if (quickSlotMenuPanel != null)
        {
            quickSlotMenuPanel.SetActive(false);
        }
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        // 확인 팝업이 열려있으면 다른 메뉴 닫기 무시
        if (confirmPanel != null && confirmPanel.activeSelf) return;
        
        // 메뉴 외부 클릭 시 닫기
        if (Input.GetMouseButtonDown(0))
        {
            if (menuPanel.activeSelf && !RectTransformUtility.RectangleContainsScreenPoint(menuRect, Input.mousePosition))
            {
                CloseMenu();
            }
            
            if (consumableMenuPanel != null && consumableMenuPanel.activeSelf && 
                !RectTransformUtility.RectangleContainsScreenPoint(consumableMenuRect, Input.mousePosition))
            {
                CloseMenu();
            }
            
            if (equipmentMenuPanel != null && equipmentMenuPanel.activeSelf && 
                !RectTransformUtility.RectangleContainsScreenPoint(equipmentMenuRect, Input.mousePosition))
            {
                CloseMenu();
            }
            
            if (quickSlotMenuPanel != null && quickSlotMenuPanel.activeSelf && 
                !RectTransformUtility.RectangleContainsScreenPoint(quickSlotMenuRect, Input.mousePosition))
            {
                CloseMenu();
            }
        }
    }
    
    /// <summary>
    /// 컨텍스트 메뉴 열기
    /// </summary>
    public void OpenMenu(SlotUI slot, Vector2 position)
    {
        if (slot == null || slot.currentItem == null) return;
        
        targetSlot = slot;
        
        // 1. 퀵슬롯에 있는 포션 - 퀵슬롯 전용 메뉴
        if (slot.slotType == SlotType.Inventory && slot.gameObject.name.Contains("Quick") && 
            slot.currentItem.itemType == ItemType.Consumable && quickSlotMenuPanel != null)
        {
            CloseAllMenus();
            quickSlotMenuPanel.SetActive(true);
            quickSlotMenuRect.position = position;
            AdjustMenuPosition(quickSlotMenuRect);
            Debug.Log("퀵슬롯 메뉴 열림");
        }
        // 2. 장비 슬롯에 있는 장비 - 장비 슬롯 전용 메뉴
        else if (slot.slotType == SlotType.Equipment && equipmentMenuPanel != null)
        {
            CloseAllMenus();
            equipmentMenuPanel.SetActive(true);
            equipmentMenuRect.position = position;
            AdjustMenuPosition(equipmentMenuRect);
            Debug.Log("장비 슬롯 메뉴 열림");
        }
        // 3. 인벤토리에 있는 소비 아이템 (포션) - 포션 메뉴
        else if (slot.slotType == SlotType.Inventory && slot.currentItem.itemType == ItemType.Consumable && 
                 consumableMenuPanel != null)
        {
            CloseAllMenus();
            consumableMenuPanel.SetActive(true);
            consumableMenuRect.position = position;
            AdjustMenuPosition(consumableMenuRect);
            Debug.Log("포션 메뉴 열림");
        }
        // 4. 그 외 아이템 - 일반 메뉴
        else
        {
            CloseAllMenus();
            
            if (IsEquipmentItem(slot.currentItem))
            {
                useButtonText.text = "장착하기";
            }
            else
            {
                useButtonText.text = "사용하기";
            }
            
            menuPanel.SetActive(true);
            menuRect.position = position;
            AdjustMenuPosition(menuRect);
            Debug.Log("일반 메뉴 열림");
        }
    }
    
    /// <summary>
    /// 모든 메뉴 닫기
    /// </summary>
    private void CloseAllMenus()
    {
        menuPanel.SetActive(false);
        if (consumableMenuPanel != null)
        {
            consumableMenuPanel.SetActive(false);
        }
        if (equipmentMenuPanel != null)
        {
            equipmentMenuPanel.SetActive(false);
        }
        if (quickSlotMenuPanel != null)
        {
            quickSlotMenuPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 메뉴 위치 조정 (화면 밖으로 안 나가게)
    /// </summary>
    private void AdjustMenuPosition(RectTransform rectTransform)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        
        // 오른쪽 화면 밖으로 나가면 왼쪽으로 이동
        if (corners[2].x > Screen.width)
        {
            rectTransform.position += new Vector3(Screen.width - corners[2].x, 0, 0);
        }
        
        // 아래쪽 화면 밖으로 나가면 위로 이동
        if (corners[0].y < 0)
        {
            rectTransform.position += new Vector3(0, -corners[0].y, 0);
        }
    }
    
    /// <summary>
    /// 컨텍스트 메뉴 닫기
    /// </summary>
    public void CloseMenu()
    {
        CloseAllMenus();
        targetSlot = null;
    }
    
    /// <summary>
    /// 장착하기 버튼 클릭 (포션 → 퀵슬롯)
    /// </summary>
    private void OnEquipButtonClicked()
    {
        if (targetSlot == null || targetSlot.currentItem == null)
        {
            CloseMenu();
            return;
        }
        
        // 퀵슬롯으로 장착
        InventoryManager.Instance.EquipToQuickSlot(targetSlot);
        CloseMenu();
    }
    
    /// <summary>
    /// 사용하기/장착하기 버튼 클릭
    /// </summary>
    private void OnUseButtonClicked()
    {
        if (targetSlot == null || targetSlot.currentItem == null)
        {
            CloseMenu();
            return;
        }
        
        // 장비 아이템이면 장착
        if (IsEquipmentItem(targetSlot.currentItem))
        {
            InventoryManager.Instance.EquipItem(targetSlot);
        }
        else
        {
            // 기타 아이템 사용
            Debug.Log($"{targetSlot.currentItem.itemName} 사용!");
        }
        
        CloseMenu();
    }
    
    /// <summary>
    /// 소비하기 버튼 클릭 (포션 전용)
    /// </summary>
    private void OnConsumeButtonClicked()
    {
        if (targetSlot == null || targetSlot.currentItem == null)
        {
            CloseMenu();
            return;
        }
        
        UseConsumable(targetSlot);
        CloseMenu();
    }
    
    /// <summary>
    /// 소비 아이템 사용
    /// </summary>
    private void UseConsumable(SlotUI slot)
    {
        Debug.Log($"{slot.currentItem.itemName} 사용!");
        
        // TODO: 여기에 포션 효과 적용 (체력 회복 등)
        // 예: PlayerHealth.Instance.Heal(50);
        
        // 아이템 개수 감소
        slot.quantity--;
        
        if (slot.quantity <= 0)
        {
            slot.ClearSlot();
        }
        else
        {
            slot.UpdateUI();
        }
    }
    
    /// <summary>
    /// 장착해제하기 버튼 클릭
    /// </summary>
    private void OnUnequipButtonClicked()
    {
        if (targetSlot == null || targetSlot.currentItem == null)
        {
            CloseMenu();
            return;
        }
        
        // 빈 인벤토리 슬롯 찾기
        SlotUI emptySlot = InventoryManager.Instance.FindEmptyInventorySlot();
        
        if (emptySlot != null)
        {
            // 장비/퀵슬롯 → 인벤토리로 이동
            emptySlot.SetItem(targetSlot.currentItem, targetSlot.quantity);
            targetSlot.ClearSlot();
            
            emptySlot.UpdateUI();
            targetSlot.UpdateUI();
            
            Debug.Log("장착 해제됨");
        }
        else
        {
            Debug.LogWarning("인벤토리에 빈 공간이 없습니다!");
        }
        
        CloseMenu();
    }
    
    /// <summary>
    /// 확인 다이얼로그 표시
    /// </summary>
    private void ShowConfirmDialog(string message)
    {
        if (confirmPanel == null) return;
        
        // 메뉴 닫기 (targetSlot은 유지)
        CloseAllMenus();
        
        // 확인 팝업 열기
        if (confirmMessageText != null)
        {
            confirmMessageText.text = message;
        }
        
        confirmPanel.SetActive(true);
    }
    
    /// <summary>
    /// 확인 팝업 - 네 버튼
    /// </summary>
    private void OnConfirmYes()
    {
        if (targetSlot != null && targetSlot.currentItem != null)
        {
            string itemName = targetSlot.currentItem.itemName;
            int quantity = targetSlot.quantity;
            
            // 아이템 버리기
            targetSlot.ClearSlot();
            
            Debug.Log($"{itemName} x{quantity}을(를) 버렸습니다.");
        }
        
        // 팝업 닫기
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
        
        targetSlot = null;
    }
    
    /// <summary>
    /// 확인 팝업 - 아니요 버튼
    /// </summary>
    private void OnConfirmNo()
    {
        // 팝업 닫기
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
        
        targetSlot = null;
    }
    
    /// <summary>
    /// 장비 아이템인지 확인
    /// </summary>
    private bool IsEquipmentItem(ItemData item)
    {
        Debug.Log($"=== 아이템 타입 확인 ===");
        Debug.Log($"아이템 이름: {item.itemName}");
        Debug.Log($"아이템 타입: {item.itemType}");
        
        bool isEquipment = item.itemType == ItemType.Weapon ||
                          item.itemType == ItemType.Helmet ||
                          item.itemType == ItemType.Armor ||
                          item.itemType == ItemType.Shoes ||
                          item.itemType == ItemType.Bag ||
                          item.itemType == ItemType.Quiver;
        
        Debug.Log($"장비 아이템인가? {isEquipment}");
        
        return isEquipment;
    }
}