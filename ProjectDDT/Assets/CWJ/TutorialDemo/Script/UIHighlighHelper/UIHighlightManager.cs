using System;
using System.Collections;
using System.Collections.Generic;
using Abu;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

namespace CWJ.UI
{
	[AddComponentMenu("CWJ/UI/CWJ - UI Highlight Manager")]
	[DisallowMultipleComponent]
	public class UIHighlightManager : MonoBehaviour
	{
		private static UIHighlightManager _Instance;

		public static UIHighlightManager Instance
		{
			get
			{
				if (!_Instance)
					_Instance = FindObjectOfType<UIHighlightManager>(true);
				return _Instance;
			}
		}

		public UIHoleFadeImage uiHoleImg;

		public RectTransform cursorRectTrf;
		[Tooltip("페이드 외곽 smoothness")]
		[SerializeField] private float _fadeSmoothness = 0.005f;

		public float FadeSmoothness
		{
			get => _fadeSmoothness;
			set
			{
				if (uiHoleImg)
					uiHoleImg.Smoothness = value;
				_fadeSmoothness = value;
			}
		}

		[Tooltip("페이드 색상")]
		[SerializeField] private Color _fadeColor = new Color(0f, 0f, 0f, 0.7f);

		public Color FadeColor
		{
			get => _fadeColor;
			set
			{
				if (uiHoleImg && uiHoleImg.color != value)
					uiHoleImg.color = value;
				_fadeColor = value;
			}
		}
		private Sequence cursorSeq, fadeSeq, clickLoopSeq;
		[Tooltip("커서 효과 tween동작 시간")]
		[SerializeField] private float cursorDuration = 1.5f;

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (!Application.isPlaying)
			{
				if (uiHoleImg)
				{
					bool changed = false;

					if (uiHoleImg.enabled != this.enabled)
					{
						uiHoleImg.enabled = this.enabled;
						changed = true;
					}

					if (Mathf.Abs(uiHoleImg.Smoothness - _fadeSmoothness) > 0.0001f)
					{
						FadeSmoothness = _fadeSmoothness;
						changed = true;
					}

					if (uiHoleImg.color != _fadeColor)
					{
						FadeColor = _fadeColor;
						changed = true;
					}

#if UNITY_EDITOR
					if (changed)
					{
						UnityEditor.EditorUtility.SetDirty(uiHoleImg);
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(uiHoleImg.gameObject.scene);
					}
#endif
				}
			}
		}

		private void Reset()
		{
			if (uiHoleImg == null)
			{
				uiHoleImg = FindObjectOfType<UIHoleFadeImage>(true);
				if (uiHoleImg == null)
				{
					var canvas = FindObjectOfType<Canvas>();
					var uiHoleGo = new GameObject(nameof(UIHoleFadeImage));
					uiHoleGo.transform.SetParent(canvas.transform);
					uiHoleGo.transform.SetAsLastSibling();
					if (!uiHoleGo.TryGetComponent(out RectTransform fadeRectTrf))
						fadeRectTrf = uiHoleGo.AddComponent<RectTransform>();
					fadeRectTrf.anchorMin = Vector2.zero;
					fadeRectTrf.anchorMax = Vector2.one;
					fadeRectTrf.sizeDelta = Vector2.zero;
					fadeRectTrf.anchoredPosition = Vector2.zero;
					fadeRectTrf.pivot = Vector2.one * 0.5f;
					uiHoleImg = uiHoleGo.AddComponent<UIHoleFadeImage>();
					uiHoleImg.Smoothness = FadeSmoothness;
					uiHoleImg.color = FadeColor;
				}
			}

			if (cursorRectTrf)
			{
				if (uiHoleImg.transform.childCount > 0)
				{
					var existsCursor = uiHoleImg.transform.Find(cursorRectTrf.gameObject.name);
					if (existsCursor != null)
						cursorRectTrf = existsCursor.GetComponent<RectTransform>();
				}
				else
				{
					var instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(cursorRectTrf.gameObject, uiHoleImg.transform);
					cursorRectTrf = instance.GetComponent<RectTransform>();
					cursorRectTrf.anchorMin = new Vector2(0.5f, 0.5f);
					cursorRectTrf.anchorMax = new Vector2(0.5f, 0.5f);
					cursorRectTrf.pivot = new Vector2(0.5f, 0.5f);
					cursorRectTrf.anchoredPosition = Vector2.zero;
					cursorRectTrf.gameObject.SetActive(false);
				}
			}
		}
#endif

		private void Awake()
		{
			if (_Instance == null)
				_Instance = this;
#if UNITY_EDITOR
			else if(_Instance != this)
			{
				Debug.Break();
				Debug.LogError($"{this.GetType().Name}는 씬에 하나만 존재해야함\n{_Instance.gameObject.name}가 이미 instance화 됨", _Instance);
				Debug.LogError($"{this.GetType().Name}가 하나더 있네\n{gameObject.name}는 머임", this);
				UnityEditor.EditorApplication.delayCall += () =>
				{
					UnityEditor.EditorGUIUtility.PingObject(this);
					UnityEditor.Selection.objects = new UnityEngine.Object[] { _Instance.gameObject, gameObject };
				};
			}
#endif
			cursorRectTrf.gameObject.SetActive(false);
		}

		private void OnEnable()
		{
			if (uiHoleImg)
				uiHoleImg.enabled = true;
		}

		private void OnDisable()
		{
			if (uiHoleImg)
				uiHoleImg.enabled = false;
		}

		private void OnDestroy()
		{
			_Instance = null;
			fadeSeq?.Kill();
			cursorSeq?.Kill();
			clickLoopSeq?.Kill();
		}

		private const float _MaxFadeOutSmoothness = 0.5f;

		public void TweenFadeSmoothness(float fadeInSmoothness, float smoothnessDuration)
		{
			fadeSeq?.Kill();
			uiHoleImg.Smoothness = _MaxFadeOutSmoothness;
			fadeSeq = DOTween.Sequence(uiHoleImg);
			fadeSeq
				.Append(DOTween.To(
					        () => uiHoleImg.Smoothness,
					        x => uiHoleImg.Smoothness = x,
					        fadeInSmoothness,
					        smoothnessDuration
				        )).Play();
		}


		public void HideCursor()
		{
			cursorSeq?.Kill();
			clickLoopSeq?.Kill();
			cursorRectTrf.gameObject.SetActive(false);
		}

		public void ShowCursorImg(RectTransform targetRectTrf)
		{
			HideCursor();

			RectTransform cursorParent = cursorRectTrf.parent as RectTransform;
			Debug.Assert(cursorParent, "cursor오브젝트는 Canvas아래에 있어야함.\nUIHoleImage 자식에 있는걸 추천");

			// Canvas 모드에 따라 카메라 결정 (Overlay면 null, Camera 모드면 worldCamera 또는 에디터에서 지정한 uiCamera)
			Canvas canvas = cursorParent.GetComponentInParent<Canvas>();
			Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : (canvas.worldCamera != null ? canvas.worldCamera : Camera.main);

			Vector2 startPos;
			if (!cursorRectTrf.gameObject.activeSelf)
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(
					cursorParent,
					Input.mousePosition,
					cam,
					out startPos
				);
			}
			else
			{
				startPos = cursorRectTrf.anchoredPosition;
			}

			Vector2 screenPt = RectTransformUtility.WorldToScreenPoint(cam, targetRectTrf.position);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(cursorParent, screenPt, cam
			                                                      , out Vector2 targetPos);
			targetPos = new Vector2(targetPos.x + targetRectTrf.rect.width * 0.5f, targetPos.y - targetRectTrf.rect.height * 0.5f);

			cursorRectTrf.anchoredPosition = startPos;
			cursorRectTrf.gameObject.SetActive(true);

			// tween 실행
			cursorSeq = DOTween.Sequence(cursorRectTrf)
			                   .Append(cursorRectTrf.DOAnchorPos(targetPos, cursorDuration).SetEase(Ease.OutCubic))
			                   .OnComplete(() =>
			                   {
				                   clickLoopSeq = DOTween.Sequence(cursorRectTrf)
				                                         .Append(cursorRectTrf.DOPunchScale(new Vector3(-0.1f, -0.1f, 0), 0.2f, 1, 0)
				                                                              .SetEase(Ease.InOutSine))
				                                         .AppendInterval(1f)
				                                         .SetLoops(-1, LoopType.Restart);
			                   });
		}
	}

}
