using UnityEngine;

public class AccordionManager : MonoBehaviour
{
    public AccordionItem[] accordionItems;

    public void CloseOthers(AccordionItem openedItem)
    {
        foreach (var item in accordionItems)
        {
            if (item != openedItem)
                item.Close();
        }
    }
}

