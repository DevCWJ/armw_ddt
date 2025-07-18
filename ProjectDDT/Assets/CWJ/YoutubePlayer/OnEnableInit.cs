using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnEnableInit : MonoBehaviour
{
	public UnityEvent onEnableEvent= new UnityEvent();
	public UnityEvent onDisableEvent= new UnityEvent();
	public GameObject[] disableObjsWhenEnable;
	private void OnEnable()
	{
		if(disableObjsWhenEnable != null)
			foreach (var o in disableObjsWhenEnable)
			{
				o.SetActive(false);
			}

		onEnableEvent?.Invoke();
	}

	private void OnDisable()
	{
		onDisableEvent?.Invoke();
	}
}
