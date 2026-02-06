// Assets/Scripts/Cooking/CookingManager.cs

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 요리 시스템 관리
/// GameManager에 붙여주세요
/// </summary>
public class CookingManager : MonoBehaviour
{
    public static CookingManager Instance { get; private set; }
    
    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject cookingPanel;
    [SerializeField] private Button cookButton;
    
    [Header("재료 슬롯")]
    [SerializeField] private SlotUI[] ingredientSlots = new SlotUI[4];
    
    [Header("결과 표시")]
    [SerializeField] private Image spoonImage;
    [SerializeField] private TextMeshProUGUI resultText;
    #endregion
    
    #region Private Fields
    private bool isCookingOpen = false;
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        InitializeUI();
        InitializeIngredientSlots();
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
            Destroy(this);
        }
    }
    
    private void InitializeUI()
    {
        if (cookButton != null)
        {
            cookButton.onClick.AddListener(TryCook);
        }
        
        if (cookingPanel != null)
        {
            cookingPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("CookingPanel이 연결되지 않았습니다!");
        }
        
        ClearResult();
    }
    
    private void InitializeIngredientSlots()
    {
        for (int i = 0; i < ingredientSlots.Length; i++)
        {
            if (ingredientSlots[i] != null)
            {
                ingredientSlots[i].slotType = SlotType.Cooking;
            }
        }
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// 요리 창 열기/닫기 토글
    /// </summary>
    public void ToggleCookingPanel()
    {
        if (isCookingOpen)
        {
            CloseCookingPanel();
        }
        else
        {
            OpenCookingPanel();
        }
    }
    
    /// <summary>
    /// 요리 창 열기
    /// </summary>
    public void OpenCookingPanel()
    {
        if (cookingPanel == null)
        {
            Debug.LogError("cookingPanel이 null입니다!");
            return;
        }
        
        isCookingOpen = true;
        cookingPanel.SetActive(true);
        ClearResult();
        
        Debug.Log("요리 창 열림");
    }
    
    /// <summary>
    /// 요리 창 닫기 (재료 인벤토리로 반환)
    /// </summary>
    public void CloseCookingPanel()
    {
        if (cookingPanel == null)
        {
            Debug.LogError("cookingPanel이 null입니다!");
            return;
        }
        
        isCookingOpen = false;
        cookingPanel.SetActive(false);
        
        ReturnIngredientsToInventory();
        
        Debug.Log("요리 창 닫힘");
    }
    
    /// <summary>
    /// 요리 창이 열려있는지 확인
    /// </summary>
    public bool IsCookingOpen()
    {
        return isCookingOpen;
    }
    
    /// <summary>
    /// ✅ 재료 추가 시도 (같은 재료면 스택)
    /// </summary>
    public bool TryAddIngredient(ItemData ingredient, SlotUI fromSlot)
    {
        if (!IsValidIngredient(ingredient))
        {
            Debug.LogWarning("재료 아이템이 아닙니다!");
            return false;
        }
        
        // ✅ 1. 같은 재료가 이미 있는지 찾기
        SlotUI existingSlot = FindSlotWithSameIngredient(ingredient);
        
        if (existingSlot != null)
        {
            // ✅ 같은 재료가 있으면 스택
            existingSlot.quantity++;
            existingSlot.UpdateUI();
            
            // 인벤토리에서 1개 제거
            fromSlot.quantity--;
            if (fromSlot.quantity <= 0)
            {
                fromSlot.ClearSlot();
            }
            else
            {
                fromSlot.UpdateUI();
            }
            
            Debug.Log($"재료 스택: {ingredient.itemName} (개수: {existingSlot.quantity})");
            return true;
        }
        
        // ✅ 2. 같은 재료가 없으면 빈 슬롯에 추가
        SlotUI emptySlot = FindEmptyIngredientSlot();
        
        if (emptySlot == null)
        {
            Debug.LogWarning("재료 슬롯이 가득 찼습니다!");
            return false;
        }
        
        // 재료를 재료 슬롯으로 이동 (1개씩만)
        emptySlot.SetItem(ingredient, 1);
        emptySlot.UpdateUI();
        
        // 인벤토리에서 1개 제거
        fromSlot.quantity--;
        if (fromSlot.quantity <= 0)
        {
            fromSlot.ClearSlot();
        }
        else
        {
            fromSlot.UpdateUI();
        }
        
        Debug.Log($"재료 추가: {ingredient.itemName}");
        return true;
    }
    
    /// <summary>
    /// 재료 슬롯인지 확인
    /// </summary>
    public bool IsIngredientSlot(SlotUI slot)
    {
        for (int i = 0; i < ingredientSlots.Length; i++)
        {
            if (ingredientSlots[i] == slot)
            {
                return true;
            }
        }
        return false;
    }
    #endregion
    
    #region Private Methods
    /// <summary>
    /// ✅ 같은 재료가 있는 슬롯 찾기
    /// </summary>
    private SlotUI FindSlotWithSameIngredient(ItemData ingredient)
    {
        foreach (var slot in ingredientSlots)
        {
            if (slot != null && slot.currentItem == ingredient)
            {
                return slot;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 요리 슬롯의 재료를 인벤토리로 반환
    /// </summary>
    private void ReturnIngredientsToInventory()
    {
        foreach (var slot in ingredientSlots)
        {
            if (slot != null && slot.currentItem != null)
            {
                if (InventoryManager.Instance != null)
                {
                    bool success = InventoryManager.Instance.AddItem(slot.currentItem, slot.quantity);
                    
                    if (success)
                    {
                        Debug.Log($"재료 반환: {slot.currentItem.itemName} x{slot.quantity}");
                    }
                    else
                    {
                        Debug.LogWarning($"인벤토리 공간 부족: {slot.currentItem.itemName}");
                    }
                }
                
                slot.ClearSlot();
            }
        }
        
        ClearResult();
    }
    
    /// <summary>
    /// 요리 시도
    /// </summary>
    private void TryCook()
    {
        List<ItemData> ingredients = GetIngredients();
        
        if (ingredients.Count < 2)
        {
            ShowResult("재료가 부족합니다! (최소 2개 필요)", Color.yellow);
            return;
        }
        
        if (!ValidateIngredients(ingredients))
        {
            ShowResult("재료가 아닌 아이템이 포함되어 있습니다!", Color.red);
            return;
        }
        
        // TODO: 레시피 시스템 구현
        CookFailed(ingredients);
    }
    
    private List<ItemData> GetIngredients()
    {
        List<ItemData> ingredients = new List<ItemData>();
        
        foreach (var slot in ingredientSlots)
        {
            if (slot != null && slot.currentItem != null)
            {
                ingredients.Add(slot.currentItem);
            }
        }
        
        return ingredients;
    }
    
    private bool ValidateIngredients(List<ItemData> ingredients)
    {
        foreach (var item in ingredients)
        {
            if (item.itemType != ItemType.Ingredient)
            {
                return false;
            }
        }
        return true;
    }
    
    private void CookFailed(List<ItemData> ingredients)
    {
        ClearAllIngredients();
        
        string ingredientNames = string.Join(", ", ingredients.ConvertAll(i => i.itemName));
        ShowResult($"요리 실패!\n재료: {ingredientNames}\n망한 음식이 나왔습니다...", Color.red);
        
        Debug.Log("요리 실패: 망한 음식");
    }
    
    private SlotUI FindEmptyIngredientSlot()
    {
        foreach (var slot in ingredientSlots)
        {
            if (slot != null && slot.currentItem == null)
            {
                return slot;
            }
        }
        return null;
    }
    
    private bool IsValidIngredient(ItemData ingredient)
    {
        return ingredient != null && ingredient.itemType == ItemType.Ingredient;
    }
    
    private void ClearAllIngredients()
    {
        foreach (var slot in ingredientSlots)
        {
            if (slot != null)
            {
                slot.ClearSlot();
            }
        }
        
        ClearResult();
    }
    
    private void ShowResult(string message, Color color)
    {
        if (resultText != null)
        {
            resultText.text = message;
            resultText.color = color;
        }
    }
    
    private void ClearResult()
    {
        if (resultText != null)
        {
            resultText.text = "";
        }
    }
    #endregion
}