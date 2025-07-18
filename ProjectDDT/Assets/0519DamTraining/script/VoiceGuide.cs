using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceGuide : MonoBehaviour
{
    public AudioClip[] thisVoice;

    public AudioSource voicePlay;
    private AudioClip[] currentVoiceArray;
    [HideInInspector]
    public int currentIndex = 0;

    void Start()
    {
        voicePlay = GetComponent<AudioSource>();
        if (voicePlay == null)
        {
            voicePlay = gameObject.AddComponent<AudioSource>();
        }

        voicePlay.playOnAwake = false;
    }

    public void SetVoice(AudioClip[] voiceArray)
    {
        currentVoiceArray = voiceArray;
    }

    // 튜토리얼 단계에 맞춰서 특정 인덱스 음성만 재생하는 함수 추가
    public void PlayVoiceForStep(int step)
    {
        if (currentVoiceArray == null || step < 0 || step >= currentVoiceArray.Length)
        {
            Debug.LogWarning("재생할 음성이 없습니다. step: " + step);
            return;
        }

        currentIndex = step;
        voicePlay.clip = currentVoiceArray[currentIndex];
        voicePlay.Play();
    }

    // 기존 함수 유지 (필요 시)
    public void PlayNextAudio()
    {
        if (currentVoiceArray == null)
        {
            Debug.LogWarning("음성 배열이 설정되지 않았습니다.");
            return;
        }

        if (currentIndex < currentVoiceArray.Length)
        {
            voicePlay.clip = currentVoiceArray[currentIndex];
            voicePlay.Play();
        }
    }
}
