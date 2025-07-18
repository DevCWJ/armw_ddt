// using System;
//
// // #if (UNITY_WEBGL && !UNITY_EDITOR)
// // using System.Runtime.InteropServices;
// // #endif
// using UnityEngine;
//
// ///현재 안됨
// public class YouTubePanel : MonoBehaviour
// {
// 	public RectTransform targetPanel; // 영상 띄울 UI 패널
// 	public string videoId;
//
// 	private string lastVideoId;
//
// 	private bool isPlaying = false;
// 	//유튜브 정책때문에 막혀서 막음
// // #if (UNITY_WEBGL && !UNITY_EDITOR)
// //     [DllImport("__Internal")] private static extern void ShowYouTube(string vid);
// //     [DllImport("__Internal")] private static extern void HideYouTube();
// //     [DllImport("__Internal")] private static extern void SetYouTubeRect(float x, float y, float w, float h);
// // #else
// 	void ShowYouTube(string vid) => Debug.Log("play " + vid);
// 	void HideYouTube() => Debug.Log("stop");
// 	void SetYouTubeRect(float x, float y, float w, float h) =>
// 		Debug.Log($"Rect {x},{y} {w}x{h}");
// // #endif
//
// 	public void Play()
// 	{
// 		if (isPlaying && lastVideoId == videoId)
// 			return;
// 		isPlaying = true;
// 		lastVideoId = videoId;
// 		UpdateRect();
// 		ShowYouTube(videoId);
// 	}
//
// 	public void Stop()
// 	{
// 		if (!isPlaying)
// 			return;
// 		isPlaying = false;
// 		HideYouTube();
// 	}
//
// 	void UpdateRect()
// 	{
// 		Vector3[] corners = new Vector3[4];
// 		targetPanel.GetWorldCorners(corners);
// 		Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
// 		Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
//
// 		float x = min.x;
// 		float y = Screen.height - max.y; // HTML 좌표계 맞춤
// 		float w = max.x - min.x;
// 		float h = max.y - min.y;
//
// 		SetYouTubeRect(x, y, w, h);
// 	}
//
// 	private void Start()
// 	{
// 		if (targetPanel)
// 		{
// 			targetPanel.GetMonoBehaviourEvent().onEnabledEvent.AddListener((_) => Play());
// 			targetPanel.GetMonoBehaviourEvent().onDisabledEvent.AddListener((_) => Stop());
// 		}
// 	}
//
// 	// void LateUpdate()
// 	// {
// 	// 	UpdateRect(); // 패널 이동/크기변경 시 갱신
// 	// }
// }
