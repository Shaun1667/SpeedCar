using System;
using UnityEngine;

/// <summary>
/// 효과음(SE)을 관리하는 매니저입니다. 여러 사운드 클립을 배열로 등록해두고,
/// 이름(Key)으로 찾아서 재생합니다.
///
/// 사용법: 인게임(Main) 씬에 빈 GameObject를 하나 만들어 AudioSource와 이 스크립트를
/// 함께 붙이세요(AudioSource가 없으면 자동으로 추가합니다). Sound Effects 배열에
/// Name(예: "PlayerMove", "CarPush", "CarFlyOff")과 재생할 Clip을 등록하면, 코드에서
/// SEManager.Instance.PlaySound("PlayerMove") 처럼 이름으로 재생할 수 있습니다.
/// 씬에 하나만 있으면 됩니다.
/// </summary>
public class SEManager : MonoBehaviour
{
    [Serializable]
    public class SoundEffect
    {
        [Tooltip("코드에서 PlaySound(이 이름)으로 재생할 때 쓰는 이름")]
        public string name;

        [Tooltip("재생할 오디오 클립")]
        public AudioClip clip;

        [Range(0f, 1f)]
        [Tooltip("이 효과음만의 재생 볼륨(0~1)")]
        public float volume = 1f;
    }

    public static SEManager Instance { get; private set; }

    [Tooltip("등록된 효과음 목록. Name으로 구분해서 PlaySound(\"이름\")으로 재생합니다.")]
    public SoundEffect[] soundEffects;

    AudioSource audioSource;

    void Awake()
    {
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 여러 효과음이 겹쳐서 재생될 수 있으므로 PlayOnAwake는 꺼둡니다.
        audioSource.playOnAwake = false;
    }

    /// <summary>등록된 이름의 효과음을 한 번 재생합니다. PlayOneShot을 사용하므로 이미
    /// 재생 중인 다른 효과음과 겹쳐서 재생돼도 서로 끊기지 않습니다.</summary>
    public void PlaySound(string soundName)
    {
        SoundEffect se = FindSound(soundName);
        if (se == null || se.clip == null)
        {
            Debug.LogWarning($"[SEManager] \"{soundName}\" 이름의 효과음을 찾을 수 없습니다. " +
                              "Sound Effects 배열에 등록되어 있는지 확인해주세요.");
            return;
        }

        audioSource.PlayOneShot(se.clip, se.volume);
    }

    SoundEffect FindSound(string soundName)
    {
        if (soundEffects == null) return null;

        foreach (var se in soundEffects)
        {
            if (se != null && se.name == soundName)
                return se;
        }
        return null;
    }
}
