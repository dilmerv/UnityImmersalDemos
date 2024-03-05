using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ARContentRaycast : MonoBehaviour
{
    [SerializeField] private LayerMask layersToInclude;

    private TextMeshPro lastButtonPressedText;

    [SerializeField] private UnityEvent onButtonActivated;
    
    [SerializeField] private UnityEvent onButtonDeactivated;
    
    void Update()
    {
        if (Input.touchCount == 0)
            return;

        var touch = Input.GetTouch(0);
        
        if (touch.phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.PositiveInfinity, layersToInclude))
            {
                var arContentComponents = raycastHit.transform.GetComponent<ARContentComponents>();
                if (arContentComponents.UI && arContentComponents.Arrow)
                {
                    arContentComponents.UI.SetActive(!arContentComponents.UI.activeSelf);
                    arContentComponents.Arrow.SetActive(!arContentComponents.Arrow.activeSelf);
                }
                else
                {
                    lastButtonPressedText = arContentComponents.Button.GetComponentInChildren<TextMeshPro>();
                    ToggleButtonState(arContentComponents);
                }
            }
        }
    }

    void ToggleButtonState(ARContentComponents contentComponents)
    {
        lastButtonPressedText = contentComponents.Button.GetComponentInChildren<TextMeshPro>();
        if (lastButtonPressedText.text == "PLAY")
        {
            lastButtonPressedText.text = "STOP";
            onButtonActivated?.Invoke();
        }
        else
        {
            lastButtonPressedText.text = "PLAY";
            onButtonDeactivated?.Invoke();
        }
    }
}
