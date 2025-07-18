using UnityEngine;
using UnityEngine.UI;

public class AccordionItem : MonoBehaviour
{
    public GameObject content;
    public Button toggleButton;
    private AccordionManager manager;

    void Start()
    {
        manager = GetComponentInParent<AccordionManager>();
        toggleButton.onClick.AddListener(Toggle);
        content.SetActive(false);
    }

    void Toggle()
    {
        bool isActive = content.activeSelf;
        content.SetActive(!isActive);
        manager.CloseOthers(this);
    }

    public void Close()
    {
        content.SetActive(false);
    }
}

