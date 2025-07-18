using UnityEngine;
using UnityEngine.Events;


public static class MonoBehaviourCallback_Unity_Utility
    {
        public static MonoBehaviourCallback GetMonoBehaviourEvent(this GameObject go)
        {
            return go.TryGetComponent<MonoBehaviourCallback>(out  var callback) ? callback : go.AddComponent<MonoBehaviourCallback>();
        }
        public static MonoBehaviourCallback GetMonoBehaviourEvent(this Transform transform)
        {
            return GetMonoBehaviourEvent(transform.gameObject);
        }
        public static MonoBehaviourCallback GetMonoBehaviourEvent(this MonoBehaviour m)
        {
            return GetMonoBehaviourEvent(m.gameObject);
        }
    }

public class UnityEvent_Transform : UnityEvent<Transform> { }

    public class MonoBehaviourCallback : MonoBehaviour
    {

        public UnityEvent_Transform awakeEvent = new UnityEvent_Transform();
        public UnityEvent_Transform startEvent = new UnityEvent_Transform();

        public UnityEvent_Transform onEnabledEvent = new UnityEvent_Transform();
        public UnityEvent_Transform onDisabledEvent = new UnityEvent_Transform();

        public UnityEvent_Transform onDestroyEvent = new UnityEvent_Transform();

        private void Awake() => awakeEvent.Invoke(transform);
        private void Start() => startEvent.Invoke(transform);
        private void OnEnable() => onEnabledEvent.Invoke(transform);
        private void OnDisable() => onDisabledEvent.Invoke(transform);
        private void OnDestroy() => onDestroyEvent.Invoke(transform);
    }
