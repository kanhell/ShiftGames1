// Assets/Scripts/Shop/ShopItemSelector.cs

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점에서 아이템 선택 관리
/// </summary>
public class ShopItemSelector : MonoBehaviour
{
    public static ShopItemSelector Instance { get; private set; }
    
    #region Private Fields
    private SlotUI selectedInventorySlot = null; // 선택된 인벤토리 슬롯
    private SlotUI selectedShopSlot = null; // 선택된 상점 슬롯
    
    private Color originalInventoryColor; // 인벤토리 슬롯의 원래 색상
    private Color originalShopColor; // 상점 슬롯의 원래 색상
    
    private Color selectedColor = new Color(0.8f, 0.8f, 1f, 1f); // 연한 파란색
    #endregion
    
    #region Unity Lifecycle
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
    #endregion
    
    #region Public Methods
    /// <summary>
    /// 인벤토리 슬롯 선택/해제
    /// </summary>
    public void SelectInventorySlot(SlotUI slot)
    {
        // 이미 선택된 슬롯을 다시 누르면 해제
        if (selectedInventorySlot == slot)
        {
            DeselectInventorySlot();
            return;
        }
        
        // 상점 슬롯이 선택되어 있으면 선택 불가
        if (selectedShopSlot != null)
        {
            Debug.Log("상점 아이템이 선택되어 있어서 인벤토리 아이템을 선택할 수 없습니다.");
            return;
        }
        
        // 기존 선택 해제
        DeselectInventorySlot();
        
        // 새로운 슬롯 선택
        selectedInventorySlot = slot;
        ApplySelectionColorToInventory(slot);
        
        Debug.Log($"인벤토리 아이템 선택: {slot.currentItem?.itemName}");
    }
    
    /// <summary>
    /// 상점 슬롯 선택/해제
    /// </summary>
    public void SelectShopSlot(SlotUI slot)
    {
        // 이미 선택된 슬롯을 다시 누르면 해제
        if (selectedShopSlot == slot)
        {
            DeselectShopSlot();
            return;
        }
        
        // 인벤토리 슬롯이 선택되어 있으면 선택 불가
        if (selectedInventorySlot != null)
        {
            Debug.Log("인벤토리 아이템이 선택되어 있어서 상점 아이템을 선택할 수 없습니다.");
            return;
        }
        
        // 기존 선택 해제
        DeselectShopSlot();
        
        // 새로운 슬롯 선택
        selectedShopSlot = slot;
        ApplySelectionColorToShop(slot);
        
        Debug.Log($"상점 아이템 선택: {slot.currentItem?.itemName}");
    }
    
    /// <summary>
    /// 인벤토리 선택 해제
    /// </summary>
    public void DeselectInventorySlot()
    {
        if (selectedInventorySlot != null)
        {
            RemoveSelectionColorFromInventory(selectedInventorySlot);
            selectedInventorySlot = null;
        }
    }
    
    /// <summary>
    /// 상점 선택 해제
    /// </summary>
    public void DeselectShopSlot()
    {
        if (selectedShopSlot != null)
        {
            RemoveSelectionColorFromShop(selectedShopSlot);
            selectedShopSlot = null;
        }
    }
    
    /// <summary>
    /// 모든 선택 해제
    /// </summary>
    public void DeselectAll()
    {
        DeselectInventorySlot();
        DeselectShopSlot();
    }
    
    /// <summary>
    /// 선택된 인벤토리 슬롯 가져오기
    /// </summary>
    public SlotUI GetSelectedInventorySlot()
    {
        return selectedInventorySlot;
    }
    
    /// <summary>
    /// 선택된 상점 슬롯 가져오기
    /// </summary>
    public SlotUI GetSelectedShopSlot()
    {
        return selectedShopSlot;
    }
    #endregion
    
    #region Private Methods
    /// <summary>
    /// 선택 색상 적용 (인벤토리 슬롯)
    /// </summary>
    private void ApplySelectionColorToInventory(SlotUI slot)
    {
        Image slotImage = slot.GetComponent<Image>();
        if (slotImage != null)
        {
            originalInventoryColor = slotImage.color; // 원래 색상 저장
            slotImage.color = selectedColor;
        }
    }
    
    /// <summary>
    /// 선택 색상 적용 (상점 슬롯)
    /// </summary>
    private void ApplySelectionColorToShop(SlotUI slot)
    {
        Image slotImage = slot.GetComponent<Image>();
        if (slotImage != null)
        {
            originalShopColor = slotImage.color; // 원래 색상 저장
            slotImage.color = selectedColor;
        }
    }
    
    /// <summary>
    /// 선택 색상 제거 (인벤토리 슬롯)
    /// </summary>
    private void RemoveSelectionColorFromInventory(SlotUI slot)
    {
        Image slotImage = slot.GetComponent<Image>();
        if (slotImage != null)
        {
            slotImage.color = originalInventoryColor; // 원래 색상 복원
        }
    }
    
    /// <summary>
    /// 선택 색상 제거 (상점 슬롯)
    /// </summary>
    private void RemoveSelectionColorFromShop(SlotUI slot)
    {
        Image slotImage = slot.GetComponent<Image>();
        if (slotImage != null)
        {
            slotImage.color = originalShopColor; // 원래 색상 복원
        }
    }
    #endregion
}