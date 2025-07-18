using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CWJ.UI;
using System;

namespace CWJ
{
	public static class ExtensionMethodTemp
	{
		public static void EnqueueMainThread(Action action) => UIFeedbackInjector._EnqueueAction(action);

		public static void EnqueueMainThread<T>(Action<T> action, T data) =>
			UIFeedbackInjector._EnqueueAction(action == null ? null : () => action.Invoke(data));

		public static void EnqueueMainThread<T, T2>(Action<T, T2> action, T data, T2 data2) =>
			UIFeedbackInjector._EnqueueAction(action == null ? null : () => action.Invoke(data, data2));

		public static bool TryGetComponentInParent_New<T>(this GameObject gameObject, out T result, out GameObject resultTargetObj)
		{
			Transform transform = gameObject.transform;
			do
			{
				if (transform.TryGetComponent(out result))
				{
					resultTargetObj = transform.gameObject;
					return true;
				}

				transform = transform.parent;
			}
			while (transform);

			result = default(T);
			resultTargetObj = null;
			return false;
		}
	}
}
