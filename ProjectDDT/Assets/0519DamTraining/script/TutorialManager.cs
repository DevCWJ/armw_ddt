using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private GameObject interactCanvas;

    public GameObject[] tutorialHints;      // 31단계 가이드 패널들
    public int currentStep = 0;             // 현재 튜토리얼 진행 단계

    public VoiceGuide voiceGuide;           // VoiceGuide 스크립트 참조

    void Start()
    {
        ResetTutorial();
    }

    void ShowCurrentHint()
    {
        for (int i = 0; i < tutorialHints.Length; i++)
        {
            tutorialHints[i].SetActive(i == currentStep);
        }
    }
    public void ResetTutorial()
    {
        currentStep = 0;
        tutorialCanvas.SetActive(true);
        interactCanvas.SetActive(false);

        ShowCurrentHint();
        PlayVoiceGuideForStep(currentStep);
    }

    // 버튼 클릭 시 호출
    public void CheckStep(int clickedStep)
    {
        if (clickedStep == currentStep)
        {
            NextStep();
        }
        else
        {
            Debug.Log("아직 이 단계가 아닙니다.");
        }
    }

    void NextStep()
    {
        currentStep++;

        if (currentStep >= tutorialHints.Length)
        {
            Debug.Log("튜토리얼 완료!");
            interactCanvas.SetActive(true);

            tutorialCanvas.SetActive(false);
            foreach (var hint in tutorialHints)
                hint.SetActive(false);
            return;
        }

        ShowCurrentHint();
        PlayVoiceGuideForStep(currentStep);
    }

    // 단계에 맞는 음성 재생
    void PlayVoiceGuideForStep(int step)
    {
        if (voiceGuide == null || voiceGuide.thisVoice.Length == 0)
            return;

        voiceGuide.SetVoice(voiceGuide.thisVoice);
        voiceGuide.PlayVoiceForStep(step);
    }
}
