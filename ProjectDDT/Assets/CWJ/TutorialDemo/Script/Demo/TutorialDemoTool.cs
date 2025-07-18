using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CWJ.UI;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TutorialDemoTool : MonoBehaviour
{
	[SerializeField] private AudioSource audioSrc;
	[Serializable]
	public class TutorialLayer
	{
		[Tooltip("안내문에 적힐 문자열")]
		[TextArea]
		public string description;
		[Tooltip("내레이션 오디오클립")]
        public AudioClip audioClip;
		[Tooltip("레이어 켜졌을때 활성화시킬 오브젝트\n highlightHandlers오브젝트는 자동으로 포함됨")]
		public GameObject[] enableObjsWhenSetLayer;
		[Tooltip("최초 실행시 (Awake에서) 비활성화해놓을 오브젝트")]
		public GameObject[] disableObjsWhenInit;
		[Tooltip("현재 레이어에서 하이라이트 시킬 오브젝트")]
		public CWJ.UI.UIHighlightHandler[] highlightHandlers;
		[Tooltip("다음으로 넘어갈 버튼을 넣으면됨")]
		public Button[] nextButtons;
		[Tooltip("다음으로 넘어갈 버튼 강조 지연시간")]
		public float nextBtnHighlightDelay = 3;

		[Tooltip("하이라이트 종료 레이어인지")]
		public bool isAllHighlightDisable = false;

		// public bool autoNextLayer = false;
		// public float autoNextDelay = 3f;

		private int _myIndex;
		private TutorialDemoTool _tutorialDemoTool;

		private bool isInit = false;
		public void Init(TutorialDemoTool tutorialDemoTool, int myIndex)
		{
			this._tutorialDemoTool = tutorialDemoTool;
			this._myIndex = myIndex;

			SetActiveHighlight(false);

			foreach (var go in disableObjsWhenInit)
			{
				if (go.activeSelf)
					go.SetActive(false);
			}

			if (!isInit)
			{
				isInit = true;
				if (nextButtons != null && nextButtons.Length > 0)
				{
					foreach (var nextButton in nextButtons)
					{
						nextButton.gameObject.SetActive(false);

						nextButton.onClick.AddListener(() =>
						{
							_tutorialDemoTool.SetLayer(_myIndex + 1);
						});

						if (!nextButton.TryGetComponent<CWJ.UI.UIHighlightHandler>(out var hHandler))
						{
							hHandler = nextButton.gameObject.AddComponent<CWJ.UI.UIHighlightHandler>();
						}

						hHandler.enabled = false;

						if (!hHandler.selectable)
						{
							hHandler.UIHoleImg = CWJ.UI.UIHighlightManager.Instance.uiHoleImg;
							hHandler.selectable = nextButton;
							hHandler.isCursorIndicate = true;
						}

						nextButton.gameObject.SetActive(true);
					}
				}

				List<GameObject> activeObjList = new List<GameObject>(enableObjsWhenSetLayer);
				activeObjList.AddRange(highlightHandlers.Select(h => h.gameObject));
				enableObjsWhenSetLayer = activeObjList.Distinct().ToArray();
			}
		}

		public void EnableGameObjs(bool active)
		{
			foreach (var go in enableObjsWhenSetLayer)
			{
				if (go.activeSelf != active)
					go.SetActive(active);
			}
		}

		public void SetActiveHighlight(bool active)
		{
			if (nextButtons != null && nextButtons.Length > 0)
			{
				if (active)
				{
					_tutorialDemoTool.SetDelayAction(() =>
					{
						foreach (var nextButton in nextButtons)
							nextButton.GetComponent<CWJ.UI.UIHighlightHandler>().enabled = true;
					}, nextBtnHighlightDelay);
				}
				else
				{
					foreach (var nextButton in nextButtons)
						nextButton.GetComponent<CWJ.UI.UIHighlightHandler>().enabled = false;
				}
			}

			foreach (var handler in highlightHandlers)
			{
				handler.enabled = active;
			}

		}
	}

	[SerializeField] private TextMeshProUGUI descTxt;

	[Header("튜토리얼 순서 설정")]

	[FormerlySerializedAs("highlightLayers")]
	[SerializeField] private TutorialLayer[] tutorialLayers;

	private TutorialLayer curLayer;

	/// <summary>
	/// 첫시작 또는 재시작시
	/// </summary>
	public void RestartTutorial()
	{
		for (int i = 0; i < tutorialLayers.Length; i++)
		{
			tutorialLayers[i].Init(this, i);
		}

		SetLayer(0);
	}

	/// <summary>
	/// Button의 경우 TutorialLayer의 NextButton배열에 등록시 자동으로 SetLayer가 설정되기에
	/// button에는 굳이 쓸일 없음.
	/// </summary>
	/// <param name="index"></param>
	public void SetLayer(int index)
	{
		if (!tutorialLayers[index].isAllHighlightDisable && !UIHighlightManager.Instance.uiHoleImg.enabled)
		{
			UIHighlightManager.Instance.uiHoleImg.enabled = true;
		}

		for (int i = 0; i < tutorialLayers.Length; i++)
		{
			if (i == index) continue;
			tutorialLayers[i].SetActiveHighlight(false);
		}

		curLayer = tutorialLayers[index];
		descTxt.SetText(curLayer.description);
		curLayer.SetActiveHighlight(true);
		curLayer.EnableGameObjs(true);
		if (curLayer.audioClip != null)
		{
			audioSrc.Stop();
			audioSrc.PlayOneShot(curLayer.audioClip);
		}

		if (curLayer.isAllHighlightDisable)
		{
			UIHighlightManager.Instance.uiHoleImg.enabled = false;
		}

		// if (curLayer.autoNextLayer)
		// {
		// 	SetLayerDelay(index+1, curLayer.autoNextDelay);
		// }
	}

	IEnumerator DO_DelayAction(Action callback, float delayTime)
	{
		yield return new WaitForSeconds(delayTime);
		callback();
	}

	 void SetDelayAction(Action callback, float delayTime)
	{
		StartCoroutine(DO_DelayAction(callback, delayTime));
	}


	private void Start()
	{
		RestartTutorial();
	}
}
