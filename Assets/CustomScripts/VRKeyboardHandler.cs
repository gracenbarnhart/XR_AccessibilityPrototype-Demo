using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class VRKeyboardHandler : MonoBehaviour, IPointerClickHandler
{
    TMP_InputField _input;
    TouchScreenKeyboard _kb;

    void Awake()
    {
        _input = GetComponent<TMP_InputField>();
    }

    void Update()
    {
        
        if (_kb != null)
        {
            _input.text = _kb.text;

            
            if (_kb.status == TouchScreenKeyboard.Status.Done ||
                _kb.status == TouchScreenKeyboard.Status.Canceled)
            {
                _kb = null;
                _input.onEndEdit?.Invoke(_input.text);
                _input.DeactivateInputField();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
        _kb = TouchScreenKeyboard.Open(
            _input.text,
            TouchScreenKeyboardType.Default,
            autocorrection: false,
            multiline: false,
            secure: false,
            alert: false,
            textPlaceholder: _input.placeholder.GetComponent<TMP_Text>().text
        );

        if (_kb == null)
        {
            
            _input.ActivateInputField();
        }
        else
        {
            
            _input.DeactivateInputField();
        }
    }
}
