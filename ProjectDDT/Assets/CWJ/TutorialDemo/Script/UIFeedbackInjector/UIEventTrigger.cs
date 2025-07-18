using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif
namespace CWJ.UI
{
	public sealed class UIEventTrigger :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler
        // ,IInitializePotentialDragHandler
        // ,IBeginDragHandler
        // ,IDragHandler
        // ,IEndDragHandler
        // ,IDropHandler
        // ,IScrollHandler
        // ,IUpdateSelectedHandler
        // ,ISelectHandler
        // ,IDeselectHandler
        // ,IMoveHandler
        // ,ISubmitHandler
        // ,ICancelHandler
	{
        private UIEventTrigger(Selectable selectable, IPointerClickHandler uiHandlerInterface)
        {
            this.selectable = selectable;
            this.uiHandlerInterface = uiHandlerInterface;
        }

        [SerializeField] private UnityEvent _onEnableCallback, _onDisableCallback, _onDestroyCallback;

        private static readonly Dictionary<int, UIEventTrigger> _UITriggerCacheDic = new();

        public bool isInitByInjector { get; private set; }

        public Selectable selectable { get; private set; }
        public bool hasSelectableUI;
        public IEventSystemHandler uiHandlerInterface { get; private set; }
        public bool hasUIInterface;

        public static bool TryGetCache(int objID, out UIEventTrigger evtTrigger)
        {
            return _UITriggerCacheDic.TryGetValue(objID, out evtTrigger);
        }

        public void InitForInjector(int objID, Selectable _selectable, IEventSystemHandler uiEventSystemHandler)
        {
            if (!_UITriggerCacheDic.TryAdd(objID, this))
            {
                Debug.Assert(isInitByInjector, $"별도 코드 추가한건가? 그러지말자 {nameof(UIFeedbackInjector)}용 함수임");
                return;
            }
            isInitByInjector = true;
            selectable = _selectable;
            hasSelectableUI = _selectable != null;
            uiHandlerInterface = uiEventSystemHandler;
            hasUIInterface = uiHandlerInterface != null;
        }

        public bool AddTrigger(params (EventTriggerType evtID, UnityAction<BaseEventData> callback)[] callbacks)
        {
            foreach ((EventTriggerType eid, UnityAction<BaseEventData> act) in callbacks)
                AddTrigger(eid, act);
            return true;
        }

        public void AddTrigger(EventTriggerType evtID, UnityAction<BaseEventData> callback)
        {
            if (_triggers.Count > 0 && _triggers.TryGetValue(evtID, out var trigger))
            {
                trigger ??= new EventTrigger.TriggerEvent();
                AddPersistentListener(trigger, callback);
            }
            else
            {
                var t = new EventTrigger.TriggerEvent();
                AddPersistentListener(t, callback);
                _triggers.Add(evtID, t);
            }
        }

        private void AddPersistentListener(EventTrigger.TriggerEvent evt, UnityAction<BaseEventData> callback)
        {
#if UNITY_EDITOR
            Debug.Assert(evt != null);
            try
            {
                UnityEventTools.AddPersistentListener(evt, callback);
            }
            catch /* (Exception e)*/
            {
                evt.AddListener(callback);
                Debug.LogWarning("경고, 테스트 필수" +
                                 "(무명메소드 혹은 Invoke()를 담을 경우 UnityEventTools.AddPersistentListener가 적용되지 않거나 Remove가 작동하지 않을 수 있습니다)/n이 로그가 보였다면 이벤트가 인스펙터에 표시 안될수도있음. 그래도 이벤트 기능은 정상작동함.\n" /*+ e.ToString()*/);
            }
#else
            evt.AddListener(callback);
#endif
        }

        public UnityEvent onEnableCallback => _onEnableCallback ??= new UnityEvent();
        public UnityEvent onDisableCallback => _onDisableCallback ??= new UnityEvent();
        public UnityEvent onDestroyCallback => _onDestroyCallback ??= new UnityEvent();

        private void OnEnable() { _onEnableCallback?.Invoke(); }

        private void OnDisable() { _onDisableCallback?.Invoke(); }

        private void OnDestroy() { _onDestroyCallback?.Invoke(); }

        public Dictionary<EventTriggerType, EventTrigger.TriggerEvent> triggers => _triggers;

        [SerializeField] private Dictionary<EventTriggerType, EventTrigger.TriggerEvent> _triggers = new();

        private void Execute(EventTriggerType id, BaseEventData eventData)
        {
            if (_triggers.TryGetValue(id, out var trigger) && trigger != null)
                trigger.Invoke(eventData);
        }

        /// <summary>
        /// Called by the EventSystem when the pointer enters the object associated with this EventTrigger.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerEnter, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when the pointer exits the object associated with this EventTrigger.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerExit, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a PointerDown event occurs.
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerDown, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a PointerUp event occurs.
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerUp, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a Click event occurs.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerClick, eventData);
        }

        // /// <summary>
        // /// Called by the EventSystem every time the pointer is moved during dragging.
        // /// </summary>
        // public virtual void OnDrag(PointerEventData eventData)
        // {
        //     Execute(EventTriggerType.Drag, eventData);
        // }
        //
        // /// <summary>
        // /// Called by the EventSystem when an object accepts a drop.
        // /// </summary>
        // public virtual void OnDrop(PointerEventData eventData)
        // {
        //     Execute(EventTriggerType.Drop, eventData);
        // }

        // /// <summary>
        // /// Called by the EventSystem when a Select event occurs.
        // /// </summary>
        // public virtual void OnSelect(BaseEventData eventData)
        // {
        //     Execute(EventTriggerType.Select, eventData);
        // }
        //
        // /// <summary>
        // /// Called by the EventSystem when a new object is being selected.
        // /// </summary>
        // public virtual void OnDeselect(BaseEventData eventData)
        // {
        //     Execute(EventTriggerType.Deselect, eventData);
        // }
        //
        // /// <summary>
        // /// Called by the EventSystem when a new Scroll event occurs.
        // /// </summary>
        // public virtual void OnScroll(PointerEventData eventData)
        // {
        //     Execute(EventTriggerType.Scroll, eventData);
        // }
        //
        // /// <summary>
        // /// Called by the EventSystem when a Move event occurs.
        // /// </summary>
        // public virtual void OnMove(AxisEventData eventData)
        // {
        //     Execute(EventTriggerType.Move, eventData);
        // }
        //
        // /// <summary>
        // /// Called by the EventSystem when the object associated with this EventTrigger is updated.
        // /// </summary>
        // public virtual void OnUpdateSelected(BaseEventData eventData)
        // {
        //     Execute(EventTriggerType.UpdateSelected, eventData);
        // }
        //
        // /// <summary>
        // /// Called by the EventSystem when a drag has been found, but before it is valid to begin the drag.
        // /// </summary>
        // public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        // {
        //     Execute(EventTriggerType.InitializePotentialDrag, eventData);
        // }
        //
        // /// <summary>
        // /// Called before a drag is started.
        // /// </summary>
        // public virtual void OnBeginDrag(PointerEventData eventData)
        // {
        //     Execute(EventTriggerType.BeginDrag, eventData);
        // }
        //
        // /// <summary>
        // /// Called by the EventSystem once dragging ends.
        // /// </summary>
        // public virtual void OnEndDrag(PointerEventData eventData)
        // {
        //     Execute(EventTriggerType.EndDrag, eventData);
        // }
        //
        // /// <summary>
        // /// Called by the EventSystem when a Submit event occurs.
        // /// </summary>
        // public virtual void OnSubmit(BaseEventData eventData)
        // {
        //     Execute(EventTriggerType.Submit, eventData);
        // }
        //
        // /// <summary>
        // /// Called by the EventSystem when a Cancel event occurs.
        // /// </summary>
        // public virtual void OnCancel(BaseEventData eventData)
        // {
        //     Execute(EventTriggerType.Cancel, eventData);
        // }
	}
}
