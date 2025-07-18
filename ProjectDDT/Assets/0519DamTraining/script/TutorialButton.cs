using UnityEngine;
using UnityEngine.UI;

public class TutorialButton : MonoBehaviour
{
    public int stepNumber;                  // 이 버튼이 맞는 단계 번호
    public TutorialManager tutorialManager;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            tutorialManager.CheckStep(stepNumber);
        });
    }
}
