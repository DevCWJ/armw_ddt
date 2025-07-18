using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SearchBtn : MonoBehaviour
{
	[SerializeField] private Button button;
	[SerializeField] private Michsky.MUIP.CustomDropdown dropdown;
	public string targetItemName;
	public UnityEvent selectTargetItem = new UnityEvent();
	public UnityEvent selectNotTargetItem = new UnityEvent();

	private void Reset()
	{
		if (button == null)
			button = GetComponent<Button>();
	}

	private void Start()
	{
		button.onClick.AddListener(() =>
		{
			if (dropdown.selectedItemIndex >= 0 && dropdown.items[dropdown.selectedItemIndex].itemName == targetItemName)
				selectTargetItem?.Invoke();
			else
				selectNotTargetItem?.Invoke();
		});
	}
}
