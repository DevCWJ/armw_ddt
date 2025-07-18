using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Main script to set up the video player
/// </summary>
public class AdvancedVideoManager : MonoBehaviour
{
    public VideoClip templateVideo; //Video clip that uses in video manager
    public string templateUrl; //Video url that uses in video manager

    public bool isLocalVideo = true; //Is use video clip or video from url
    [Header("Play Setting")]
    public bool isPlayOnAwake = false;
    public bool playAlwaysZeroPoint = true;
    public bool isPlayModeSyncEnabled = false;
    public bool isLoop = false; //Is video should be looping

    [Header("Interface")]
    public bool needToClickToShowInterface = false; //Is need to click on video to show an interface with the buttons
    public bool neverHideInterfaceWhileHover = false; //Is never hide interface while mouse is hovering the video
    public bool saveVolumeInPlayerPrefs = true; //Is need to save volume settings in PlayerPrefs
    public bool savePlaybackSpeedInPlayerPrefs = true; //Is need to save playback speed settings in PlayerPrefs

    public int masterVolume = 100; //Master volume (0% - 100%)

    public int volumeStepOffset = 5; //Volume step by pressing Up/Down arrows (0% - 100%)
    public int timeStepOffset = 10; //Time step by pressing Left/Right arrows or double pressing on video Left/Right sides (0 seconds - infinity seconds)

    public bool isDisablePlaybackBtn = false;
    public float[] allowedPlaybackSpeeds = new float[4] { 1, 1.5f, 2f, 0.5f }; //Allowed playback speed list

    VideoManager videoManager;
    VolumeScript volumeManager;
    PlaybackRate playbackSpeedManager;
    [SerializeField] private GameObject playbackSpeedBtn;
    private bool isInit = false;
    /// <summary>
    /// Start method
    /// </summary>
    private void Awake()
    {
        if (!isInit)
            InitAllSettings();
    }

    private void Start()
    {
        if (!isPlayModeSyncEnabled)
        {
            if (isPlayOnAwake)
                StartCoroutine(PlayAuto());
        }
    }

    private void OnEnable()
    {
        if (isPlayModeSyncEnabled)
        {
            StartCoroutine(PlayAuto());
        }
    }

    private void OnDisable()
    {
        if (isPlayModeSyncEnabled)
        {
            videoManager.Pause();
        }
    }

    private IEnumerator PlayAuto()
    {
        yield return null;

        videoManager.Play();
        // if (!isLocalVideo)
        //     yield return new WaitUntil(() => videoManager.notLocalInit);
        yield return new WaitUntil(() => videoManager.isStarted);
        yield return null;
        if (playAlwaysZeroPoint)
        {
            videoManager.isSliderDown = true;
            videoManager.targetSlider.value = 0;
            videoManager.SetSliderValue();
            videoManager.isSliderDown = false;
        }

        videoManager.Play();
    }

    /// <summary>
    /// Save settings and initialize all scripts
    /// </summary>
    public void InitAllSettings() {

        if (!isInit)
        {
            isInit = true;
            videoManager = transform.GetComponentInChildren<VideoManager>();
            volumeManager = transform.GetComponentInChildren<VolumeScript>();
            playbackSpeedManager = transform.GetComponentInChildren<PlaybackRate>();
            if (isDisablePlaybackBtn)
            {
                allowedPlaybackSpeeds = new float[1] { 1 };
                videoManager.SetPlaybackSpeed(1);
            }
        }

        if (!isLocalVideo && templateUrl!=null)
        {
            var normalized = templateUrl.Replace("\\", "/");

            // "StreamingAssets/" 로 시작하면
            const string streamingAssetPrefix = "StreamingAssets/";
            if (normalized.StartsWith(streamingAssetPrefix))
            {
                var relative = normalized.Substring(streamingAssetPrefix.Length);

                templateUrl = Path.Combine(Application.streamingAssetsPath, relative).Replace("\\", "/");
            }
        }

        volumeManager.isSaveToPrefs = saveVolumeInPlayerPrefs;
        playbackSpeedManager.isSaveToPrefs = savePlaybackSpeedInPlayerPrefs;
        playbackSpeedManager.allowedRates = allowedPlaybackSpeeds;
        videoManager.InitVideo();
        playbackSpeedManager.Init();
        volumeManager.Init();

        if (isDisablePlaybackBtn)
            playbackSpeedBtn.SetActive(false);
    }

    /// <summary>
    /// Load video clip
    /// </summary>
    /// <param name="targetClip">Target video clip</param>
    public void LoadVideoClip(VideoClip targetClip) {
        templateVideo = targetClip;
        isLocalVideo = true;
        InitAllSettings();
    }

    /// <summary>
    /// Load video from url
    /// </summary>
    /// <param name="targetUrl">Target url (local or http/https)</param>
    public void LoadVideoFromUrl(string targetUrl)
    {
        templateUrl = targetUrl;
        isLocalVideo = false;
        InitAllSettings();
    }
}
