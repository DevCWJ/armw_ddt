using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	public GameObject[] awakeDisableObjs;
	public string videoUrl;
	public AdvancedVideoManager videoManager;

	private void Awake()
	{
		foreach (var o in awakeDisableObjs)
		{
			o.SetActive(false);
		}
	}

	private void Start()
	{
		videoManager.LoadVideoFromUrl(videoUrl);
	}
}
