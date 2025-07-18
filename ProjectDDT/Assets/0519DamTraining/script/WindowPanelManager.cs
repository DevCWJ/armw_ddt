using System.Collections.Generic;
using UnityEngine;

public class WindowPanelManager : MonoBehaviour
{
    [Header("모든 패널")]
    public List<GameObject> panelss;

    [Header("초기 기본으로 켜져 있어야 할 패널")]
    public GameObject defaultPanels;

    private GameObject currentPanels;

    void Start()
    {
        // 초기화 시 기본 패널만 켜기
        foreach (var panel in panelss)
        {
            panel.SetActive(panel == defaultPanels);
        }

        currentPanels = defaultPanels;
    }

    public void OpenOnly(GameObject panelToOpen)
    {
        if (panelToOpen == null) return;

        // 이미 열려있는 패널이면 다시 열 필요 없음
        if (currentPanels == panelToOpen) return;

        foreach (GameObject panel in panelss)
        {
            panel.SetActive(panel == panelToOpen);
        }

        currentPanels = panelToOpen;
    }
}
