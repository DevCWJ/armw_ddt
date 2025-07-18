using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

/// <summary>
/// Video manager that contains most main mechanics
/// </summary>
public class VideoManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public AdvancedVideoManager advancedVideoManager; // Reference to a main script

    VideoPlayer videoPlayer; // Reference to VideoPlayer script

    public Animator targetAnimator; // Reference to an animator for interface

    public GameObject playButton; // Reference to a play button
    public GameObject stopButton; // Reference to a stop button
    public GameObject replayButton; // Reference to a replay button


    public RawImage rawImage; // Reference to a RawImage

    public TextMeshProUGUI timeText; // Reference to a time text

    public bool isStarted { get; private set; } // Is initialised yet

    public Slider targetSlider; // Reference to a time slider

    public GameObject setFullscreenButton; // Reference to a fullscreen button
    public GameObject setSmallscreenButton; // Reference to a smallscreen button

    Vector2 targetRectSize; // Rect size of a RawImage

    GameObject fullscreenRect; // Spawned fullscreen object
    public GameObject fullscreenRectTemplate; // Fullscreen object prefab
    public RectTransform smallscreenRect; // Smallscreen object

    RectTransform videoParent; // Parent of a RawImage

    public bool isOpenedDarkWindow; // Is interface open

    float targetSpeed; // Target playback speed

    public ChangeTime addTimeScript; // Add time script
    public ChangeTime minusTimeScript; // Minus time script

    /// <summary>
    /// Add seconds to time
    /// </summary>
    public void AddSeconds() {
        targetSlider.value += advancedVideoManager.timeStepOffset;
        canSetSlider = false;
        if (canSetTime) {
            videoPlayer.time = targetSlider.value;
        }
    }

    /// <summary>
    /// Minus seconds from time
    /// </summary>
    public void MinusSeconds() {
        if (isEnd) {
            playButton.GetComponent<Button>().interactable = true;
            stopButton.GetComponent<Button>().interactable = false;
            replayButton.GetComponent<Button>().interactable = false;
            playButton.SetActive(true);
            stopButton.SetActive(false);
            replayButton.SetActive(false);
        }
        targetSlider.value -= advancedVideoManager.timeStepOffset;
        canSetSlider = false;
        if (canSetTime) {
            videoPlayer.time = targetSlider.value;
        }
    }

    /// <summary>
    /// Set playback speed
    /// </summary>
    /// <param name="targetSpeed">Target playback speed</param>
    public void SetPlaybackSpeed(float targetSpeed) {
        this.targetSpeed = targetSpeed;
    }

    int currentVolume; // Current volume

    /// <summary>
    /// Set volume
    /// </summary>
    /// <param name="targetVolume">Target volume</param>
    public void SetVolume(int targetVolume) {
        currentVolume = targetVolume;
        videoPlayer.SetDirectAudioVolume(0, ((float)targetVolume / 100) * ((float)advancedVideoManager.masterVolume / 100));
    }

    /// <summary>
    /// Start method
    /// </summary>
    private void Start() {
        videoParent = GetComponent<RectTransform>();
        videoPlayer.sendFrameReadyEvents = true;
        videoPlayer.frameReady += VideoPlayer_frameReady;
    }

    /// <summary>
    /// Fit RawImage to parent object
    /// </summary>
    public void FitToParent() {
        Vector2 rectParent = new Vector2(rawImage.transform.parent.GetComponent<RectTransform>().rect.width,
            rawImage.transform.parent.GetComponent<RectTransform>().rect.height);
        if (rectParent.x / targetRectSize.x < rectParent.y / targetRectSize.y) {
            rawImage.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(targetRectSize.x * (rectParent.x / targetRectSize.x) + 5,
                targetRectSize.y * (rectParent.x / targetRectSize.x) + 5);
        } else {
            rawImage.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(targetRectSize.x * (rectParent.y / targetRectSize.y) + 5,
                targetRectSize.y * (rectParent.y / targetRectSize.y) + 5);
        }
    }

    /// <summary>
    /// Set fullscreen mode
    /// </summary>
    public void SetFullscreenMode() {
        fullscreenRect = Instantiate(fullscreenRectTemplate);
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        setFullscreenButton.SetActive(false);
        setSmallscreenButton.SetActive(true);
        videoParent.SetParent(fullscreenRect.transform);
        videoParent.anchoredPosition = new Vector2(0, 0);
        videoParent.sizeDelta = new Vector2(0, 0);
        videoParent.localScale = new Vector3(1, 1, 1);
    }

    /// <summary>
    /// Set smallscreen mode
    /// </summary>
    public void SetSmallMode() {
        Screen.orientation = ScreenOrientation.Portrait;
        setFullscreenButton.SetActive(true);
        setSmallscreenButton.SetActive(false);
        videoParent.SetParent(smallscreenRect);
        videoParent.anchoredPosition = new Vector2(0, 0);
        videoParent.sizeDelta = new Vector2(0, 0);
        videoParent.localScale = new Vector3(1, 1, 1);
        Destroy(fullscreenRect);
    }

    /// <summary>
    /// Initialize video clip or video from url
    /// </summary>
    public void InitVideo() {
        canSetTime = true;
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.source = advancedVideoManager.isLocalVideo ? VideoSource.VideoClip : VideoSource.Url;
        rawImage.enabled = false;

        if (advancedVideoManager.isLocalVideo) {
            videoPlayer.clip = null;
            videoPlayer.clip = advancedVideoManager.templateVideo;
            videoPlayer.time = 0;
            RenderTexture newTexture = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 0);
            videoPlayer.targetTexture = newTexture;
            rawImage.texture = newTexture;
        } else {
            videoPlayer.url = advancedVideoManager.templateUrl;
            videoPlayer.time = 0;
            notLocalInit = false;
        }

        videoPlayer.Play();
        videoPlayer.Prepare();
        videoPlayer.SetDirectAudioMute(0, true);
        isStarted = false;
        isPlaying = false;

        setFullscreenButton.SetActive(true);
        setSmallscreenButton.SetActive(false);

        targetRectSize = rawImage.transform.GetComponent<RectTransform>().sizeDelta;

        canSetSlider = true;

        playButton.GetComponent<Button>().interactable = true;
        stopButton.GetComponent<Button>().interactable = false;
        replayButton.GetComponent<Button>().interactable = false;
        playButton.SetActive(true);
        stopButton.SetActive(false);
        replayButton.SetActive(false);
    }

    public bool notLocalInit { get; private set; }

    /// <summary>
    /// Callback when frame is ready
    /// </summary>
    /// <param name="source"></param>
    /// <param name="frameIdx"></param>
    private void VideoPlayer_frameReady(VideoPlayer source, long frameIdx) {
        if (!advancedVideoManager.isLocalVideo && !notLocalInit) {
            notLocalInit = true;
            videoPlayer.time = 0;
            RenderTexture newTexture = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 0);
            videoPlayer.targetTexture = newTexture;
            rawImage.texture = newTexture;
            return;
        }
        if (!isStarted) {
            isStarted = true;
            rawImage.enabled = true;
            videoPlayer.SetDirectAudioMute(0, false);
            videoPlayer.Pause();
            timeText.text = GetStringFromSeconds(0) + " / " + GetStringFromSeconds((int)videoPlayer.length);
            targetSlider.maxValue = (int)videoPlayer.length;
        }

        canSetTime = true;
        videoPlayer.playbackSpeed = targetSpeed;
    }

    bool canSetTime = true;

    /// <summary>
    /// Play video
    /// </summary>
    public void Play() {
        if (isEnd) {
            videoPlayer.time = 0;
        }

        SetTriggerOpenState();

        isPlaying = true;
        videoPlayer.Play();
        playButton.GetComponent<Button>().interactable = !isPlaying;
        stopButton.GetComponent<Button>().interactable = isPlaying;
        replayButton.GetComponent<Button>().interactable = false;
        playButton.SetActive(!isPlaying);
        stopButton.SetActive(isPlaying);
        replayButton.SetActive(false);

        if (!canSetSlider) {
            canSetSlider = true;
        }
    }

    /// <summary>
    /// Pause video
    /// </summary>
    public void Pause() {
        SetTriggerOpenState();

        isPlaying = false;
        videoPlayer.Pause();
        playButton.GetComponent<Button>().interactable = !isPlaying;
        stopButton.GetComponent<Button>().interactable = isPlaying;
        replayButton.GetComponent<Button>().interactable = false;
        playButton.SetActive(!isPlaying);
        stopButton.SetActive(isPlaying);
        replayButton.SetActive(false);
    }

    /// <summary>
    /// Play video when you unpress a time slider
    /// </summary>
    public void PlayForSlider() {
        if (isEnd) {
            videoPlayer.time = 0;
        }
        videoPlayer.Play();
        isPlaying = true;
    }

    /// <summary>
    /// Pause video when you touch a time slider
    /// </summary>
    public void PauseForSlider() {
        videoPlayer.Pause();
        isPlaying = false;
    }

    public bool isSliderDown; // Is pressing slider

    public VolumeScript volumeScript; // Volume script
    public ChangeVolumeAnim changeVolume; // Change volume script

    /// <summary>
    /// Change volume by Up/Down arrows
    /// </summary>
    /// <param name="modifier">Offset (-1 or 1)</param>
    public void ChangeVolumeOffset(int modifier) {
        currentVolume = Mathf.Clamp(currentVolume + (modifier * advancedVideoManager.volumeStepOffset), 0, 100);
        volumeScript.volumeSlider.value = currentVolume;
        changeVolume.Open(currentVolume);
    }

    public bool isPlaying; // Is video playing

    /// <summary>
    /// Update method
    /// </summary>
    private void Update() {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            ChangeVolumeOffset(1);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            ChangeVolumeOffset(-1);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            minusTimeScript.AddByButton();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            addTimeScript.AddByButton();
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (!isSliderDown) {
                if (isPlaying) {
                    Pause();
                } else {
                    Play();
                }
            }
        }
        if (isStarted) {
            timeText.text = GetStringFromSeconds((int)targetSlider.value) + " / " + GetStringFromSeconds((int)videoPlayer.length);
            if (!isSliderDown) {
                if (canSetSlider) {
                    targetSlider.value = (float)videoPlayer.time;
                }
                if (IsVideoEnded()) {
                    if (!isEnd) {
                        if (advancedVideoManager.isLoop) {
                            videoPlayer.Pause();
                            isEnd = true;
                            videoPlayer.time = 0;
                            videoPlayer.Play();
                        } else {
                            Pause();
                            replayButton.GetComponent<Button>().interactable = true;
                            playButton.GetComponent<Button>().interactable = false;
                            stopButton.GetComponent<Button>().interactable = false;
                            replayButton.SetActive(true);
                            playButton.SetActive(false);
                            stopButton.SetActive(false);
                            SetTriggerOpenState();

                            isEnd = true;
                        }
                    }
                } else {
                    isEnd = false;
                }
            }
        }
        FitToParent();
        if (isHovering) {
            if (advancedVideoManager.neverHideInterfaceWhileHover) {
                SetTriggerOpenState();

            }
            if (Input.GetMouseButtonDown(0)) {
                SetTriggerOpenState();

            }
            if (!advancedVideoManager.needToClickToShowInterface) {
                if (prevMousePosition != Input.mousePosition) {
                    prevMousePosition = Input.mousePosition;
                    SetTriggerOpenState();

                }
            }
        }
    }

    public bool isEnd; // Is video ended

    bool canSetSlider;

    public void WaitUntilLoading() {
        canSetSlider = true;
    }

    public void SetSliderValue() {
        if (isSliderDown) {
            videoPlayer.time = targetSlider.value;
        }
    }

    /// <summary>
    /// Returns a string formated like MM:SS from seconds
    /// </summary>
    /// <param name="totalSeconds">Seconds</param>
    /// <returns></returns>
    public string GetStringFromSeconds(int totalSeconds) {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds - (minutes * 60);
        string targetString = "";
        if(minutes < 10) {
            targetString += "0";
        }
        targetString += minutes + ":";
        if (seconds < 10) {
            targetString += "0";
        }
        targetString += seconds + "";
        return targetString;
    }

    /// <summary>
    /// Check is video ended
    /// </summary>
    /// <returns></returns>
    public bool IsVideoEnded() {
        return (int)videoPlayer.length == (int)videoPlayer.time;
    }

    public void InterfaceOpened() {
        isOpenedDarkWindow = true;
    }

    public void InterfaceClosed() {
        isOpenedDarkWindow = false;
    }

    bool isHovering; // Is pointer hovering the video rect
    Vector3 prevMousePosition;

    /// <summary>
    /// Is pointer enter in video rect
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData) {
        isHovering = true;
        if (advancedVideoManager.needToClickToShowInterface) {
            return;
        }
        prevMousePosition = Input.mousePosition;
        SetTriggerOpenState();
    }

    /// <summary>
    /// Is pointer exit the video rect
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData) {
        isHovering = false;
        if (advancedVideoManager.needToClickToShowInterface) {
            return;
        }
    }

    private void SetTriggerOpenState()
    {
        if (isOpenedDarkWindow)
        {
            targetAnimator.SetTrigger("StartHoldingOpenAgain");
        }
        else
        {
            targetAnimator.SetTrigger("SwitchOpenState");
        }
    }

    private void ResetTriggerOpenState()
    {
        if (isOpenedDarkWindow)
        {
            targetAnimator.ResetTrigger("StartHoldingOpenAgain");
        }
        else
        {
            targetAnimator.ResetTrigger("SwitchOpenState");
        }
    }

    private void OnDisable()
    {
        ResetTriggerOpenState();
    }
}
