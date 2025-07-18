using System.Collections.Generic;
using UnityEngine;

public class ExclusivePanelManager : MonoBehaviour
{
    [Header("모든 부모 패널")]
    public List<GameObject> panels;

    private GameObject currentPanel;

    public void OpenOnly(GameObject panelToOpen)
    {
        if (panelToOpen == null) return;
        if (currentPanel == panelToOpen) return;

        foreach (GameObject panel in panels)
        {
            bool isTarget = panel == panelToOpen;
            panel.SetActive(isTarget); // 지정한 패널만 켬, 나머지 끔

            if (isTarget)
            {
                TurnOnFirstChildOnly(panel);
                currentPanel = panel;
            }
        }
    }

    private void TurnOnFirstChildOnly(GameObject parent)
    {
        int i = 0;
        foreach (Transform child in parent.transform)
        {
            child.gameObject.SetActive(i == 0); // 첫 번째 자식만 켜기
            i++;
        }
    }
}
