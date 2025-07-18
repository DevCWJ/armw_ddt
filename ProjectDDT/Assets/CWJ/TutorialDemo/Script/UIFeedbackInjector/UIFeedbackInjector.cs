#if UNITY_WITH_MULTITHREADING
#undef USE_THREAD_SAFETY
#else
#define USE_THREAD_SAFETY
#endif
using ActionQueue
#if USE_THREAD_SAFETY
	= System.Collections.Concurrent.ConcurrentQueue<System.Action>; //서버나 하드웨어 통신작업때문에 찐 multi thread 환경이 필요하다면 Thread-Safe한 ConcurrentQueue쓰기
#else
    = System.Collections.Generic.Queue<System.Action>;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine.Events;


namespace CWJ.UI
{
	using static ExtensionMethodTemp;

	/// <summary>
	/// <para/>아래와 같은 컴포넌트를 클릭할때
	/// <br/>클릭 사운드, 애니메이션을 실행해줌
	/// <br/>[UIFeedbackInjector 타겟 조건]
	/// <br/>1. Selectable를 상속받는 UI컴포넌트
	/// <br/>2. IPointerClickHandler 를 상속받는 컴포넌트
	/// <br/>3. 3D오브젝트는 아직 지원안함 (무분별하게 검출할까봐)
	/// <para/>[25.05.21]
	/// </summary>
	[AddComponentMenu("CWJ/UI/CWJ - UI Feedback Injector")]
	[DisallowMultipleComponent, RequireComponent(typeof(AudioSource))]
	public class UIFeedbackInjector : MonoBehaviour
	{
		private static UIFeedbackInjector _Instance = null;

		[Tooltip("UnityEngine.EventSystems.IPointerClickHandler 상속컴포넌트 도 인식 시킬건지?")]
		public bool isRecognizable_IPointerClickHandler = false;

		[Header("클릭 피드백 모션 사용여부")]
		public bool useTweenAnimation = true;

		public bool CanTweenAnim => useTweenAnimation;

		[Header("Inject Event")]
		public UnityEngine.Events.UnityEvent<Transform> pointerClickEvent;
		public UnityEngine.Events.UnityEvent<Transform> pointerDownEvent, pointerUpEvent,
		                                                                  notInteractableEvent;

		[Header("클릭 사운드 사용여부")]
		public bool useClickSound = true;

		[SerializeField] private AudioSource audioSource;

		[SerializeField] private AudioClip pointerDownSound, pointerUpSound, notInteractableSound, inputFieldEndEditSound;
		public bool CanClickSound => useClickSound && audioSource;

#if UNITY_EDITOR
		private void Reset()
		{
			UnityEditor.EditorApplication.delayCall += () =>
			{
				if (!(audioSource = gameObject.GetComponent<AudioSource>()))
					audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.playOnAwake = false;
				UnityEditorInternal.ComponentUtility.MoveComponentDown(audioSource);
			};
		}
#endif
		private void Awake()
		{
			if (!_Instance)
				_Instance = this;
#if UNITY_EDITOR
			else if (_Instance != this)
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
		}

		private void Update()
		{
			_ProcessQueue(_MainThreadActionsQueue);

			if (!useClickSound && !useTweenAnimation) //이럴거면 그냥 enabled = false 해줘
			{
				return;
			}

			if (Input.GetMouseButtonDown(0))
			{
				var curEventSys = EventSystem.current;
				if (curEventSys)
					CheckRegisteredAndInit(curEventSys);
			}
		}

		void OnPointerDownFeedback(InteractableUICache uiCache)
		{
			bool isInteractable = uiCache.GetIsInteractableWhenPointerDown(out bool isDisable);
			if (isInteractable)
			{
				PlaySoundFx(pointerDownSound);
				uiCache.DoTween_PointerDown();
				pointerDownEvent?.Invoke(uiCache.targetTrf);
			}
			else
			{
				PlaySoundFx(notInteractableSound);
				uiCache.DoTween_NotInteractable();
				notInteractableEvent?.Invoke(uiCache.targetTrf);
			}
		}

		private void OnPointerUpOrClickFeedback(InteractableUICache uiCache, bool isClicked)
		{
			bool isInteractable = uiCache.GetIsInteractableWhenPointerUp(out bool isInteractableWhenPointerDown);
			if (isInteractableWhenPointerDown)
			{
				var trf = uiCache.isDisposed ? null : uiCache.targetTrf;

				if (isInteractable)
				{
					if (isClicked)
					{
						uiCache.DoTween_PointerClick();
						pointerClickEvent?.Invoke(trf);
					}
					else
					{
						uiCache.DoTween_PointerUp();
						PlaySoundFx(pointerUpSound);
						pointerUpEvent?.Invoke(trf);
					}
				}
				else
				{
					uiCache.DoTween_NotInteractable();
					notInteractableEvent?.Invoke(trf);
				}
			}
		}

		private void ClickableUI_OnEndEdit(string text)
		{
			PlaySoundFx(inputFieldEndEditSound);
		}

		private static readonly HashSet<int> _RegisteredID = new ();

		//최적화 필요시 이거 풀링해서 쓰면됨
		private static readonly Queue<InteractableUICache> _ClickableUICachesTemp = new();

		private static readonly List<RaycastResult> raycastResults = new ();
		private void CheckRegisteredAndInit(EventSystem curEventSystem)
		{
			GameObject hoveredObj = curEventSystem.currentSelectedGameObject;

			Debug.LogError("1 > " + hoveredObj.name, hoveredObj);
			if (!hoveredObj)
			{
				PointerEventData pointerData = new(curEventSystem) { position = Input.mousePosition };
				EventSystem.current.RaycastAll(pointerData, raycastResults);
				if (raycastResults.Count == 0)
				{
					return;
				}

				hoveredObj = raycastResults[0].gameObject;
			}

			Debug.LogError("2 > " + hoveredObj.name, hoveredObj);


			int objId = hoveredObj.GetInstanceID();

			if (_RegisteredID.Add(objId)) // 최초 등록 오브젝트만 세팅
			{
				Selectable curSelectable;
				IPointerClickHandler clickHandlerInterface = null;

				if (hoveredObj.TryGetComponentInParent_New<Selectable>(out curSelectable, out GameObject curSelectGo)
				|| (isRecognizable_IPointerClickHandler &&
				    hoveredObj.TryGetComponentInParent_New<IPointerClickHandler>(out clickHandlerInterface, out curSelectGo)))
				{
					int targetId= curSelectGo.GetInstanceID();
					if (objId == targetId || _RegisteredID.Add(targetId))
					{
						objId = targetId;
						if (!curSelectGo.TryGetComponent(out UIEventTrigger evtTrigger))
							evtTrigger = curSelectGo.AddComponent<UIEventTrigger>();
						evtTrigger.InitForInjector(objId, _selectable: curSelectable, uiEventSystemHandler: clickHandlerInterface);
						if (evtTrigger.hasSelectableUI)
						{
							if (curSelectGo.TryGetComponent(out TMP_InputField ipf))
							{
								ipf.onEndEdit ??= new TMP_InputField.SubmitEvent();
								ipf.onEndEdit.AddListener(_Instance.ClickableUI_OnEndEdit);
							}
						}

						if (!_ClickableUICachesTemp.TryDequeue(out InteractableUICache clickableUICache))
							clickableUICache = new InteractableUICache();
						clickableUICache.Init(objId, evtTrigger);
						OnPointerDownFeedback(clickableUICache); //EventTrigger를 이제 세팅했으니까 최초 PointerDown은 직접 호출해줌
					}
				}
			}
		}


		private void PlaySoundFx(AudioClip soundClip)
		{
#if UNITY_EDITOR
			if (useClickSound && !audioSource)
			{
				Debug.LogError("useClickSound 가 활성화 되어있지만 audioSource가 없음", this);
			}
#endif
			if (!CanClickSound || !soundClip) return;
			audioSource.PlayOneShot(soundClip);
		}

		public class InteractableUICache
		{
			public GameObject targetObj { get; private set; }
			public Transform targetTrf { get; private set; }
			public UIEventTrigger uiEventTrigger { get; private set; }
			public bool HasSelectableUI => uiEventTrigger.hasSelectableUI;
			public bool HasUIInterface => uiEventTrigger.hasUIInterface;
			public Vector3 localScale;
			private Sequence sequence;
			public int objId { get; private set; }
			public bool isDisposed { get; private set; }
			public bool isInteractableWhenDestroy { get; private set; }

			public InteractableUICache() { }

			public void Init(int id, UIEventTrigger _uiEventTrigger)
			{
				KillTween();

				this.objId = id;
				this.targetObj = _uiEventTrigger.gameObject;
				this.targetTrf = _uiEventTrigger.transform;
				this.uiEventTrigger = _uiEventTrigger;
				isDisposed = false;
				isInteractableWhenDestroy = false;

				this.localScale = targetTrf.localScale;

				InitTween();

				_uiEventTrigger.onEnableCallback.AddListener(OnEnable);
				_uiEventTrigger.onDisableCallback.AddListener(OnDisable);
				_uiEventTrigger.onDestroyCallback.AddListener(OnDestroy);
				_uiEventTrigger.AddTrigger((EventTriggerType.PointerDown, OnPointerDown),
				                          (EventTriggerType.PointerUp, OnPointerUp),
				                          (EventTriggerType.PointerClick, OnPointerClick));
			}

			void OnDestroy()
			{
				isInteractableWhenDestroy = GetIsInteractable(out _);
				isDisposed = true;
				_RegisteredID.Remove(objId);
				KillTween();
				_ClickableUICachesTemp.Enqueue(this);
			}

			void OnDisable()
			{
				KillTween();
			}

			void OnEnable()
			{
				InitTween();
			}

			void OnPointerDown(BaseEventData eventData)
			{
				_Instance.OnPointerDownFeedback(this);
			}

			void OnPointerUp(BaseEventData eventData)
			{
				_Instance.OnPointerUpOrClickFeedback(this, false);
			}

			void OnPointerClick(BaseEventData eventData)
			{
				_Instance.OnPointerUpOrClickFeedback(this, true);
			}

			private bool _isInteractableWhenPointerDown;

			public bool GetIsInteractable(out bool isDisable)
			{
				isDisable = true;
				if (isDisposed)
					return false;
				if (HasUIInterface)
				{
					isDisable = false;
					return true;
				}

				if (!HasSelectableUI) //중간에 selectable컴포넌트만 destroy된 경우
				{
					return false;
				}
				if (HasSelectableUI && !uiEventTrigger.selectable)
				{
					uiEventTrigger.hasSelectableUI = false;
					//중간에 selectable컴포넌트만 destroy된 경우
					return false;
				}

				isDisable = !uiEventTrigger.selectable.enabled;
				return !isDisable && uiEventTrigger.selectable.IsInteractable();
			}

			/// <summary>
			/// Selectable컴포넌트가 아니거나 Selectable컴포넌트라면 interactable true인경우
			/// </summary>
			public bool GetIsInteractableWhenPointerDown(out bool isDisable)
			{
				isDisable = true;
				if (isDisposed)
					return _isInteractableWhenPointerDown = isInteractableWhenDestroy;
				_isInteractableWhenPointerDown = GetIsInteractable(out isDisable);
				return _isInteractableWhenPointerDown;
			}

			public bool GetIsInteractableWhenPointerUp(out bool isInteractableWhenPointerDown)
			{
				isInteractableWhenPointerDown = _isInteractableWhenPointerDown;
				if (isDisposed)
					return isInteractableWhenDestroy;
				return GetIsInteractable(out _);
			}

#region DoTween

			private bool isTweenInit;

			private Tweener scaleTweener;
			private Sequence seqPointerClick, seqNotInteractable;

			private void KillTween()
			{
				MuteOtherTween();
				isTweenInit = false;
				seqPointerClick?.Kill();
				seqNotInteractable?.Kill();
				scaleTweener?.Kill();
			}

			private bool IsTweenPlayable()
			{
				return !isDisposed && _Instance.CanTweenAnim && targetObj.activeSelf;
			}

			void PauseAndRewind(Tween tw)
			{
				if (TweenExtensions.IsActive(tw))
				{
					tw.Rewind();
					tw.Pause();
				}
			}

			void MuteOtherTween(Tween except = null)
			{
				if (!isTweenInit) return;
				if (except == null)
				{
					PauseAndRewind(scaleTweener);
					PauseAndRewind(seqPointerClick);
					PauseAndRewind(seqNotInteractable);
					return;
				}
				if (except != scaleTweener) PauseAndRewind(scaleTweener);
				if (except != seqPointerClick) PauseAndRewind(seqPointerClick);
				if (except != seqNotInteractable) PauseAndRewind(seqNotInteractable);
			}

			public void DoTween_PointerDown()
			{
				if (!IsTweenPlayable()) return;
				scaleTweener.ChangeEndValue(localScale * 0.95f, 0.1f, true)
				            .SetEase(Ease.OutQuad)
				            .Restart();
			}

			public void DoTween_PointerUp()
			{
				if (!IsTweenPlayable()) return;
				MuteOtherTween(scaleTweener);
				scaleTweener.ChangeEndValue(localScale, 0.1f, true)
				            .SetEase(Ease.OutQuad)
				            .Restart();
			}

			public void DoTween_PointerClick()
			{
				if (!IsTweenPlayable()) return;
				MuteOtherTween(seqPointerClick);
				seqPointerClick.Restart();
			}

			public void DoTween_NotInteractable()
			{
				if (!IsTweenPlayable()) return;
				MuteOtherTween(seqNotInteractable);
				seqNotInteractable.Restart();
			}



			private void InitTween()
			{
				if (isTweenInit) return;
				isTweenInit = true;

				scaleTweener = targetTrf.DOScale(localScale, 0)
				                        .SetAutoKill(false)
				                        .Pause()
				                        .SetRecyclable(true);

				seqPointerClick = DOTween.Sequence(targetObj)
				                         .Append(targetTrf.DOScale(localScale * 1.05f, 0.08f).SetEase(Ease.OutQuad))
				                         .Append(targetTrf.DOScale(localScale, 0.12f).SetEase(Ease.OutBack))
				                         .SetAutoKill(false)
				                         .Pause()
				                         .SetRecyclable(true);

				seqNotInteractable = DOTween.Sequence(targetObj)
				                            .Append(targetTrf.DOShakeScale(0.2f, strength: 0.1f, vibrato: 10, randomness: 90, fadeOut: true)
				                                             .SetEase(Ease.OutCubic))
				                            .SetAutoKill(false)
				                            .Pause()
				                            .SetRecyclable(true);
			}

#endregion
		}


		private static readonly ActionQueue _MainThreadActionsQueue = new();

		public static void _EnqueueAction(Action action)
		{
			if (action == null)
			{
#if UNITY_EDITOR
				Debug.LogError("_MainThreadActionsQueue action is null");
#endif
				return;
			}

			_MainThreadActionsQueue.Enqueue(action);
		}

		private void _ProcessQueue(ActionQueue queue)
		{
			while (queue.TryDequeue(out var action))
			{
#if UNITY_EDITOR
				try
				{
#endif
					action.Invoke();
#if UNITY_EDITOR
				}
				catch (Exception e)
				{
					Debug.LogError($"Exception in UpdateQueue action: {e}");
				}
#endif
			}
		}

	}


}

