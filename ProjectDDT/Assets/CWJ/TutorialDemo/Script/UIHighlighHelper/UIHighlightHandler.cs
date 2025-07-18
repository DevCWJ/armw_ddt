using System;
using Abu;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace CWJ.UI
{
	[ExecuteInEditMode, DisallowMultipleComponent]
	[AddComponentMenu("CWJ/UI/CWJ - UI Highlight Handler")]
	public class UIHighlightHandler : MonoBehaviour
	{
		[SerializeField] private UIHoleFadeImage _uiHoleImg;

		/// <summary>
		/// 런타임중에 생성되면 수동으로 set 해줘야함
		/// </summary>
		public UIHoleFadeImage UIHoleImg
		{
			get => _uiHoleImg;
			set
			{
				if (_uiHoleImg == value)
					return;

				if (_uiHoleImg != null)
					_uiHoleImg.RemoveHole(hole);

				_uiHoleImg = value;

				if (_uiHoleImg != null)
					_uiHoleImg.AddHole(hole);
			}
		}

		public Selectable selectable;

		[Header("Effects")]
		[Tooltip("object 활성화 시 페이드인 tween효과가 필요한지")]
		public bool isFadingSmoothnessOnEnable = false;

		[Tooltip("object 활성화 시 tween효과 포함할것인지")]
		public bool isTweenShowOnEnable = true;

		[Tooltip("object 비활성화 시 tween효과 포함할것인지")]
		public bool isTweenHideWhenBtnClick = false;

		[Tooltip("커서 오브젝트가 가리키는 효과 필요한지")]
		public bool isCursorIndicate = false;

		[Tooltip("isFadingSmoothnessOnEnable 활성화 시 smoothness 도달 지점")]
		[SerializeField] float fadeInSmoothness = 0.01f;

		[Header("Tween Durations")]
		[Tooltip("isTweenShowOnEnable 활성화 시 효과 동작시간")]
		[SerializeField] float scaleDuration = 1f;

		[Tooltip("isFadingSmoothnessOnEnable 활성화 시 효과 동작시간")]
		[SerializeField] float fadeSmoothnessDuration = 1f;


		private Sequence scaleSeq, hideSeq;

		private HoleCore _hole;

		protected HoleCore hole
		{
			get
			{
				if (_hole == null)
				{
					if (TryGetComponent(out RectTransform rectTransform))
						_hole = new RectTransformRenderHole(rectTransform);
					else if (TryGetComponent(out Renderer rendererComponent) && UIHoleImg != null)
						_hole = new RendererHole(rendererComponent, UIHoleImg);
				}

				return _hole;
			}
		}

		private Vector3 lastLocalScale;


		private RectTransform rectTrf;

		private void Awake()
		{
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
				return;
#endif
			lastLocalScale = transform.localScale;
			rectTrf = transform as RectTransform;

			if (isTweenShowOnEnable)
				transform.localScale = Vector3.zero;

			if (selectable || isCursorIndicate)
			{
				if (!gameObject.TryGetComponent(out UIEventTrigger trigger))
					trigger = gameObject.AddComponent<UIEventTrigger>();

				trigger.AddTrigger(EventTriggerType.PointerClick, OnPointerClick);
			}
		}

		void OnPointerClick(BaseEventData _)
		{
			if (isCursorIndicate)
				UIHighlightManager.Instance.HideCursor();

			if (isTweenHideWhenBtnClick)
				HideWithTweenScale();
		}

		void OnEnable()
		{
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
			{
				if (UIHoleImg != null)
					UIHoleImg.AddHole(hole);
				return;
			}
#endif

			if (isTweenShowOnEnable)
				OnShowWithTweenScale();
			else
			{
				if (isCursorIndicate)
					UIHighlightManager.Instance.ShowCursorImg(rectTrf);
			}

			if (UIHoleImg != null)
				UIHoleImg.AddHole(hole);

			if (isFadingSmoothnessOnEnable)
			{
				ExtensionMethodTemp.EnqueueMainThread(() =>
				{
					if (isActiveAndEnabled)
					{
						UIHighlightManager.Instance.TweenFadeSmoothness(fadeInSmoothness, fadeSmoothnessDuration);
					}
				});
			}
		}

		void OnShowWithTweenScale()
        {
            scaleSeq?.Kill();
            if (lastLocalScale == Vector3.zero)
                lastLocalScale = Vector3.one;

            transform.localScale = Vector3.zero;
            scaleSeq = DOTween.Sequence(transform);
            scaleSeq
                .Append(transform.DOScale(lastLocalScale, scaleDuration).SetEase(Ease.OutBack))
                .OnComplete(() =>
	            {
		            if (isCursorIndicate)
			            UIHighlightManager.Instance.ShowCursorImg(rectTrf);
	            }).Play();
        }

       public void HideWithTweenScale()
        {
            hideSeq?.Kill();
            Vector3 curScale = transform.localScale;
            hideSeq = DOTween.Sequence(transform);
            hideSeq.Append(transform.DOScale(Vector3.zero, scaleDuration).SetEase(Ease.InBack)).OnComplete(() =>
	            { gameObject.SetActive(false); }).Play();
        }

        void OnDisable()
        {
	        if (UIHoleImg != null)
                UIHoleImg.RemoveHole(hole);

#if UNITY_EDITOR
	        if (!UnityEditor.EditorApplication.isPlaying)
		        return;
#endif
	        // 모든 시퀀스 kill
	        scaleSeq?.Kill();
	        hideSeq?.Kill();

        }

        void OnDestroy()
        {
	        if (UIHoleImg != null)
                UIHoleImg.RemoveHole(hole);
        }

    #if UNITY_EDITOR
		private void Reset()
		{
			if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}
			isCursorIndicate = TryGetComponent<Selectable>(out selectable);
		}

		void OnValidate()
		{
			if (Application.isPlaying)
				return;

			if (!UIHoleImg)
			{
				if (UIHighlightManager.Instance)
					UIHoleImg = UIHighlightManager.Instance.uiHoleImg;
				else
					Debug.LogError("UIHighlightManager 부터 씬에 배치해야함.\n(UIHighlightManager를 생성하면 UIHoleImage는 자동생성 됩니다)");
				if (!_uiHoleImg)
					UIHoleImg = FindObjectOfType<UIHoleFadeImage>(false);
			}

			if (_uiHoleImg)
			{
				if (isActiveAndEnabled)
					UIHoleImg.AddHole(hole);
			}
        }
    #endif
	}
}
