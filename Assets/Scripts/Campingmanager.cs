using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 야영 시스템 - 확률적 습격 발생
/// </summary>
public class CampingManager : MonoBehaviour
{
    public static CampingManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject campingPanel;
    [SerializeField] private Button campButton;
    [SerializeField] private TextMeshProUGUI resultText;
    
    [Header("Camping Settings")]
    [Tooltip("습격 확률 (0~100%)")]
    [SerializeField] private float attackChance = 30f;
    
    [Header("Text Settings")]
    [SerializeField] private Color safeColor = Color.green;
    [SerializeField] private Color attackColor = Color.red;
    [SerializeField] private float textDisplayDuration = 3f;
    
    private bool isCamping = false;
    
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
        // 버튼 이벤트 연결
        if (campButton != null)
        {
            campButton.onClick.AddListener(OnCampButtonClicked);
        }
        
        // 초기화
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }
        
        if (campingPanel != null)
        {
            campingPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        // C 키로 야영 패널 열기/닫기
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCampingPanel();
        }
    }
    
    /// <summary>
    /// 야영 패널 열기/닫기
    /// </summary>
    public void ToggleCampingPanel()
    {
        if (campingPanel == null) return;
        
        bool isOpen = campingPanel.activeSelf;
        campingPanel.SetActive(!isOpen);
    }
    
    /// <summary>
    /// 야영 버튼 클릭 처리
    /// </summary>
    private void OnCampButtonClicked()
    {
        if (isCamping) return;
        
        StartCoroutine(ProcessCamping());
    }
    
    /// <summary>
    /// 야영 처리 (확률 체크)
    /// </summary>
    private IEnumerator ProcessCamping()
    {
        isCamping = true;
        
        // 버튼 비활성화
        if (campButton != null)
        {
            campButton.interactable = false;
        }
        
        // 결과 텍스트 숨김
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }
        
        // 약간의 대기 (긴장감)
        yield return new WaitForSeconds(0.5f);
        
        // 습격 확률 체크
        float roll = Random.Range(0f, 100f);
        bool isAttacked = roll < attackChance;
        
        // 결과 표시
        ShowResult(isAttacked);
        
        // 텍스트 표시 시간만큼 대기
        yield return new WaitForSeconds(textDisplayDuration);
        
        // 결과 텍스트 숨김
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }
        
        // 버튼 다시 활성화
        if (campButton != null)
        {
            campButton.interactable = true;
        }
        
        isCamping = false;
    }
    
    /// <summary>
    /// 결과 텍스트 표시
    /// </summary>
    private void ShowResult(bool isAttacked)
    {
        if (resultText == null) return;
        
        resultText.gameObject.SetActive(true);
        
        if (isAttacked)
        {
            // 습격 발생
            resultText.text = "습격이 왔습니다!";
            resultText.color = attackColor;
            
            // TODO: 전투 시작 또는 피해 처리
            OnAttackOccurred();
        }
        else
        {
            // 안전
            resultText.text = " 안전하게 야영했습니다.";
            resultText.color = safeColor;
            
            // TODO: 체력 회복 등
            OnSafeCamping();
        }
    }
    
    /// <summary>
    /// 습격 발생 시 호출 (확장용)
    /// </summary>
    private void OnAttackOccurred()
    {
        // TODO: 전투 시작, 적 생성, 피해 처리 등
    }
    
    /// <summary>
    /// 안전한 야영 완료 시 호출 (확장용)
    /// </summary>
    private void OnSafeCamping()
    {
        // TODO: 체력 회복, 버프 등
    }
}