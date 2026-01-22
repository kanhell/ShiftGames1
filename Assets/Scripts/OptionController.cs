using TMPro;
using UnityEngine;

public class OptionController : MonoBehaviour
{
    public int idx;
    public TextMeshProUGUI text_option;

    public void Setter(int _idx, string _msg)
    {
        idx = _idx;
        text_option.text = _msg;
    }
    
    public void OnClick()
    {
        DialogController.instance.OptionSelected(idx);
    }
}
