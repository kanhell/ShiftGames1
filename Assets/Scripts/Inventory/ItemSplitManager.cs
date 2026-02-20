// Assets/Scripts/UI/ItemSplitManager.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 아이템 수량 분할 UI 관리자
/// Shift + 클릭/드래그 시 표시
/// </summary>
public class ItemSplitManager : MonoBehaviour
{
    public static ItemSplitManager Instance { get; private set; }
    
    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject panelObject;
    [SerializeField] private Slider quantitySlider;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    
    [Header("Input Blocker")]
    [SerializeField] private GameObject inputBlocker; // 투명한 전체 화면 배경
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    #endregion
    
    #region Private Fields
    private SlotUI sourceSlot;
    private SlotUI targetSlot;
    private SplitMode currentMode;
    private int maxQuantity;
    private int selectedQuantity = 1;
    #endregion
    
    #region Enums
    private enum SplitMode
    {
        ClickToEmpty,      // Shift + 클릭 → 빈 슬롯에 분할
        DragToEmpty        // Shift + 드래그 → 빈 슬롯에 분할
    }
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        InitializeUI();
    }
    
    private void Update()
    {
        HandleEscapeKey();
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
    
    private void InitializeUI()
    {
        // 버튼 이벤트 연결
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => {
                Debug.Log("★★★★★ 확인 버튼 클릭 감지됨!");
                OnConfirmClicked();
            });
        }
        
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => {
                Debug.Log("★★★★★ 취소 버튼 클릭 감지됨!");
                OnCancelClicked();
            });
        }
        
        // 슬라이더 이벤트 연결
        if (quantitySlider != null)
        {
            quantitySlider.onValueChanged.RemoveAllListeners();
            quantitySlider.onValueChanged.AddListener((value) => {
                Debug.Log($"★★★★★ 슬라이더 값 변경: {value}");
                OnSliderValueChanged(value);
            });
            
            // ✅ 슬라이더 기본 설정
            quantitySlider.minValue = 1; // ✅ 1부터 시작
            quantitySlider.maxValue = 10;
            quantitySlider.wholeNumbers = true;
            quantitySlider.value = 1; // ✅ 초기값 1
            
            LogDebug("슬라이더 초기화 완료");
        }
        else
        {
            Debug.LogError("QuantitySlider가 연결되지 않았습니다!");
        }
        
        /* ✅ 초기 비활성화: InputBlocker (임시로 주석)
        if (inputBlocker != null)
        {
            inputBlocker.SetActive(false);
        }
        */
        
        // ✅ 초기 비활성화: ItemSplitPanel
        if (panelObject != null)
        {
            panelObject.SetActive(false);
        }
        
        LogDebug("UI 초기화 완료 - ItemSplitPanel 비활성화");
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// Shift + 클릭으로 분할 패널 열기
    /// </summary>
    public void OpenForClick(SlotUI source)
    {
        if (source == null || source.currentItem == null || source.quantity <= 1)
        {
            LogDebug("분할할 수 없는 슬롯입니다.");
            return;
        }
        
        sourceSlot = source;
        targetSlot = null;
        currentMode = SplitMode.ClickToEmpty;
        
        ShowPanel();
    }
    
    /// <summary>
    /// Shift + 드래그로 분할 패널 열기
    /// </summary>
    public void OpenForDrag(SlotUI source, SlotUI target)
    {
        if (source == null || source.currentItem == null || source.quantity <= 1)
        {
            LogDebug("분할할 수 없는 슬롯입니다.");
            return;
        }
        
        if (target == null || target.currentItem != null)
        {
            LogDebug("대상 슬롯이 비어있어야 합니다.");
            return;
        }
        
        sourceSlot = source;
        targetSlot = target;
        currentMode = SplitMode.DragToEmpty;
        
        ShowPanel();
    }
    
    /// <summary>
    /// 패널 닫기
    /// </summary>
    public void ClosePanel()
    {
        // ✅ ItemSplitPanel 비활성화
        if (panelObject != null)
        {
            panelObject.SetActive(false);
            LogDebug("ItemSplitPanel 비활성화");
        }
        
        /* ✅ InputBlocker 비활성화 (임시로 주석)
        if (inputBlocker != null)
        {
            inputBlocker.SetActive(false);
            LogDebug("InputBlocker 비활성화");
        }
        */
        
        // 데이터 초기화
        sourceSlot = null;
        targetSlot = null;
        selectedQuantity = 1;
        
        LogDebug("패널 닫힘");
    }
    
    /// <summary>
    /// 패널이 열려있는지 확인
    /// </summary>
    public bool IsOpen()
    {
        // ✅ panelObject가 활성화되어 있으면 열린 상태
        return panelObject != null && panelObject.activeSelf;
    }
    #endregion
    
    #region Private Methods
    private void ShowPanel()
    {
        if (panelObject == null)
        {
            Debug.LogError("panelObject가 null입니다!");
            return;
        }
        
        // ✅ 최대 개수를 원본 수량으로 설정
        maxQuantity = sourceSlot.quantity;
        
        // ✅ ItemSplitPanel 먼저 활성화
        panelObject.SetActive(true);
        LogDebug("ItemSplitPanel 활성화");
        
        /* ✅ InputBlocker 임시로 비활성화 (디버깅용)
        if (inputBlocker != null)
        {
            inputBlocker.SetActive(true);
            
            // InputBlocker를 ItemSplitPanel 바로 뒤로 이동
            int panelIndex = panelObject.transform.GetSiblingIndex();
            inputBlocker.transform.SetSiblingIndex(panelIndex - 1);
            
            LogDebug($"InputBlocker 활성화 (Index: {inputBlocker.transform.GetSiblingIndex()})");
        }
        else
        {
            Debug.LogError("inputBlocker가 null입니다!");
        }
        */
        
        // 슬라이더 설정
        if (quantitySlider != null)
        {
            quantitySlider.minValue = 1; // ✅ 최소 1개
            quantitySlider.maxValue = maxQuantity;
            quantitySlider.value = 1; // ✅ 초기값 1
        }
        
        // 아이템 정보 표시
        if (itemNameText != null)
        {
            itemNameText.text = sourceSlot.currentItem.itemName;
        }
        
        if (itemIconImage != null)
        {
            itemIconImage.sprite = sourceSlot.currentItem.itemIcon;
            itemIconImage.gameObject.SetActive(true);
        }
        
        // 수량 텍스트 업데이트
        UpdateQuantityText();
        
        LogDebug($"패널 열림 - 최대 수량: {maxQuantity}");
    }
    
    private void OnSliderValueChanged(float value)
    {
        selectedQuantity = Mathf.RoundToInt(value);
        UpdateQuantityText();
    }
    
    private void UpdateQuantityText()
    {
        if (quantityText != null)
        {
            // ✅ 간단하게 "1 / 4" 형식으로만 표시
            quantityText.text = $"{selectedQuantity} / {sourceSlot.quantity}";
        }
    }
    
    private void OnConfirmClicked()
    {
        if (sourceSlot == null)
        {
            ClosePanel();
            return;
        }
        
        // ✅ 1개 이하를 선택하면 아무것도 안 함
        if (selectedQuantity <= 0)
        {
            LogDebug("0개는 분할할 수 없습니다.");
            ClosePanel();
            return;
        }
        
        LogDebug($"분할 확인: {selectedQuantity}개");
        
        // ✅ 아이템 정보를 미리 저장 (ClearSlot 전에!)
        ItemData itemToHold = sourceSlot.currentItem;
        
        // ✅ 원본에서 수량 감소
        sourceSlot.quantity -= selectedQuantity;
        
        if (sourceSlot.quantity <= 0)
        {
            sourceSlot.ClearSlot();
        }
        else
        {
            sourceSlot.UpdateUI();
        }
        
        // ✅ ItemCursorFollower로 아이템 전달 (저장한 아이템 사용)
        if (ItemCursorFollower.Instance != null)
        {
            ItemCursorFollower.Instance.StartHolding(itemToHold, selectedQuantity, sourceSlot);
            LogDebug($"ItemCursorFollower.StartHolding 호출 완료");
        }
        else
        {
            Debug.LogError("ItemCursorFollower.Instance가 null입니다! Canvas에 ItemCursorFollower가 있는지 확인하세요.");
        }
        
        // 패널 닫기
        ClosePanel();
    }
    
    private void OnCancelClicked()
    {
        LogDebug("분할 취소");
        ClosePanel();
    }
    
    private void SplitToEmptySlot()
    {
        // 빈 슬롯 찾기
        SlotUI emptySlot = FindEmptySlotNear(sourceSlot);
        
        if (emptySlot == null)
        {
            Debug.LogWarning("빈 슬롯을 찾을 수 없습니다!");
            return;
        }
        
        // 아이템 분할
        PerformSplit(sourceSlot, emptySlot, selectedQuantity);
    }
    
    private void SplitToTargetSlot()
    {
        if (targetSlot == null)
        {
            Debug.LogWarning("대상 슬롯이 null입니다!");
            return;
        }
        
        // 아이템 분할
        PerformSplit(sourceSlot, targetSlot, selectedQuantity);
    }
    
    private void PerformSplit(SlotUI from, SlotUI to, int amount)
    {
        if (from == null)
        {
            Debug.LogError("출발 슬롯이 null입니다!");
            return;
        }
        
        LogDebug($"PerformSplit 호출: {from.currentItem.itemName} x{amount}");
        
        // 원본에서 수량 감소
        from.quantity -= amount;
        
        // ✅ 원본 슬롯 UI 업데이트
        if (from.quantity <= 0)
        {
            from.ClearSlot();
        }
        else
        {
            from.UpdateUI();
        }
        
        // ✅ ItemCursorFollower로 아이템 전달 (마우스 따라다니기)
        if (ItemCursorFollower.Instance != null)
        {
            LogDebug($"ItemCursorFollower.StartHolding 호출");
            ItemCursorFollower.Instance.StartHolding(from.currentItem, amount, from);
            Debug.Log($"아이템 분할 완료: {from.currentItem.itemName} x{amount} (마우스 따라다님)");
        }
        else
        {
            Debug.LogError("ItemCursorFollower.Instance가 null입니다! Canvas에 ItemCursorFollower가 있는지 확인하세요.");
            
            // ItemCursorFollower가 없으면 기존 방식 (직접 대상 슬롯에 넣기)
            if (to != null)
            {
                to.SetItem(from.currentItem, amount);
                to.UpdateUI();
                Debug.Log($"아이템 분할 완료: {from.currentItem.itemName} x{amount}");
            }
            else
            {
                Debug.LogError("ItemCursorFollower와 대상 슬롯이 모두 null입니다!");
            }
        }
    }
    
    private SlotUI FindEmptySlotNear(SlotUI source)
    {
        // 인벤토리 매니저에서 빈 슬롯 찾기
        if (InventoryManager.Instance != null)
        {
            return InventoryManager.Instance.FindEmptyInventorySlot();
        }
        
        return null;
    }
    
    private void HandleEscapeKey()
    {
        // ✅ ESC 키: 분할창만 닫기 (다른 창은 그대로)
        if (IsOpen() && Input.GetKeyDown(KeyCode.Escape))
        {
            OnCancelClicked();
            // ✅ ESC 이벤트 소비 (다른 곳에서 처리 못하게)
            Input.ResetInputAxes();
        }
    }
    
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[ItemSplitManager] {message}");
        }
    }
    #endregion
}