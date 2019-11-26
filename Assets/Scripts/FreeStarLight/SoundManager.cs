using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class SoundManager<TManager, TSoundsEnum, TMusicEnum> : SmartBeheviourSingleton<TManager> where TManager : SoundManager<TManager, TSoundsEnum, TMusicEnum>
{
    [Header("Sound References")]
    public AudioSource MainAudioSource;
    public AudioSource SecondaryAudioSource;

    [Header("Clips List")]
    public NamedClip[] SoundsList;
    public NamedClip[] MusicList;

    public static bool IsMusicPlaying { get { return Instance.MainAudioSource.isPlaying; } }
    public static float CurrentMusicTime { get { return Instance.MainAudioSource.time; } }
    public static TMusicEnum CurrentMusic { get { return Instance.currentMusic; } }

    protected Dictionary<TSoundsEnum, NamedClip> SoundsDictionary = new Dictionary<TSoundsEnum, NamedClip>();
    protected Dictionary<TMusicEnum, NamedClip> MusicDictionary = new Dictionary<TMusicEnum, NamedClip>();

    private int lastEnumCount_Sounds = -1;
    private int lastListCount_Sounds = -1;
    private int lastEnumCount_Music = -1;
    private int lastListCount_Music = -1;
    
    private float inspectorUpdateCooldown = 1f;
    private DateTime lastInspectorUpdate = DateTime.Now.AddMinutes(-5);
    private DateTime? pendingInspectorUpdate = null;
    private TMusicEnum currentMusic;

    public override void Awake()
    {
        base.Awake();
        UpdateInspectorAndDictionaries();
    }

#if UNITY_EDITOR
    private void EditorUpdate()
    {
        if (pendingInspectorUpdate.HasValue && DateTime.Now >= pendingInspectorUpdate.Value)
        {
            pendingInspectorUpdate = null;
            UpdateInspectorAndDictionaries();
        }

        if (!pendingInspectorUpdate.HasValue)
        {
            EditorApplication.update -= EditorUpdate;
        }
    }

    public virtual void OnValidate()
    {
        if (pendingInspectorUpdate.HasValue)
            return;

        var secondsSinceLastUpdate = (float)((DateTime.Now - lastInspectorUpdate).TotalSeconds);
        if (secondsSinceLastUpdate >= inspectorUpdateCooldown)
        {
            UpdateInspectorAndDictionaries();
        }
        else
        {
            float secondsTillNextUpdate = (inspectorUpdateCooldown - secondsSinceLastUpdate);
            pendingInspectorUpdate = DateTime.Now.AddSeconds(secondsTillNextUpdate);
            EditorApplication.update += EditorUpdate;
        }
    }
#endif

    public static void StopMusic()
    {
        Instance.MainAudioSource.Stop();
    }

    public static NamedClip Play(TSoundsEnum sound, Action SoundEndedCallback = null)
    {
        return Instance.playSound(sound, SoundEndedCallback);
    }

    public static NamedClip Play(TMusicEnum music, TMusicEnum intro, float time = 0f, Action MusicEndedCallback = null)
    {
        return Instance.playMusic(intro, time, () => {
            Play(music, 0f, MusicEndedCallback);
        });
    }

    public static NamedClip Play(TMusicEnum music, float time = 0f, Action MusicEndedCallback = null, bool cancelCallbackOnInterrupt = true)
    {
        return Instance.playMusic(music, time, MusicEndedCallback, cancelCallbackOnInterrupt);
    }
    
    public static SoundClip3D PlaySound3D(TSoundsEnum sound, Vector3 worldPos, float minDistance = 1f, float maxDistance = 100f, Action SoundEndedCallback = null)
    {
        return Instance.playSound3D(sound, worldPos, minDistance, maxDistance, SoundEndedCallback);
    }

    private NamedClip playSound(TSoundsEnum sound, Action SoundEndedCallback = null)
    {
        NamedClip namedClip = null;
        if (SoundsDictionary.TryGetValue(sound, out namedClip) && namedClip != null && namedClip.Clip != null)
        {
            MainAudioSource.PlayOneShot(namedClip.Clip, namedClip.volume);

            if(SoundEndedCallback != null)
                StartCoroutine(ClipCallcabkCorutine(namedClip.Clip.length, SoundEndedCallback));
        }
        
        return namedClip;
    }

    public SoundClip3D playSound3D(TSoundsEnum sound, Vector3 worldPos, float minDistance = 1f, float maxDistance = 100f, Action SoundEndedCallback = null)
    {
        NamedClip namedClip = null;
        if (SoundsDictionary.TryGetValue(sound, out namedClip) && namedClip != null && namedClip.Clip != null)
        {
            var newSoundClip3D = SoundClip3D.InstantiateAt(worldPos, namedClip.Clip, namedClip.volume, minDistance, maxDistance);

            if (SoundEndedCallback != null)
                StartCoroutine(ClipCallcabkCorutine(namedClip.Clip.length, SoundEndedCallback));

            return newSoundClip3D;
        }

        return null;
    }

    public NamedClip playMusic(TMusicEnum music, float time = 0f, Action MusicEndedCallback = null, bool cancelCallbackOnInterrupt = true)
    {
        NamedClip namedClip = null;
        if (MusicDictionary.TryGetValue(music, out namedClip) && namedClip != null && namedClip.Clip != null)
        {
            MainAudioSource.Stop();
            MainAudioSource.clip = namedClip.Clip;
            MainAudioSource.time = time;
            MainAudioSource.volume = namedClip.volume;
            MainAudioSource.Play();
            currentMusic = music;

            if (MusicEndedCallback != null)
            {
                if (_MusicCallcabkCorutine != null)
                    StopCoroutine(_MusicCallcabkCorutine);

                _MusicCallcabkCorutine = MusicCallcabkCorutine(MusicEndedCallback, cancelCallbackOnInterrupt);
                StartCoroutine(_MusicCallcabkCorutine);
            }
        }

        return namedClip;
    }
    
    public void UpdateInspectorAndDictionaries()
    {
        //Update sounds list
        var soundNames = Enum.GetNames(typeof(TSoundsEnum));
        if (lastEnumCount_Sounds != soundNames.Length || lastListCount_Sounds != soundNames.Length)
        {
            SoundsDictionary.Clear();
            List<NamedClip> newSoundsList = new List<NamedClip>();
            foreach (var soundName in soundNames)
            {
                var newNamedClip = new NamedClip();
                newNamedClip.Name = soundName;

                if(SoundsList != null)
                {
                    //check if the clip already been set before, and move it to the new list
                    var oldNamedClip = SoundsList.FirstOrDefault(x => x.Name == soundName);
                    if (oldNamedClip != null)
                    {
                        newNamedClip.Clip = oldNamedClip.Clip;
                        newNamedClip.volume = oldNamedClip.volume;
                    }
                }

                //add to dictionary
                TSoundsEnum value = (TSoundsEnum)Enum.Parse(typeof(TSoundsEnum), newNamedClip.Name);
                SoundsDictionary[value] = newNamedClip;

                newSoundsList.Add(newNamedClip);
            }
            SoundsList = newSoundsList.ToArray();
        }
        
        //Update music list
        var musicNames = Enum.GetNames(typeof(TMusicEnum));
        if (lastEnumCount_Music != musicNames.Length || lastListCount_Music != musicNames.Length)
        {
            MusicDictionary.Clear();
            List<NamedClip> newMusicList = new List<NamedClip>();
            foreach (var musicName in musicNames)
            {
                var newNamedClip = new NamedClip();
                newNamedClip.Name = musicName;

                if(MusicList != null)
                {
                    //check if the clip already been set before, and move it to the new list
                    var oldNamedClip = MusicList.FirstOrDefault(x => x.Name == musicName);
                    if (oldNamedClip != null)
                    {
                        newNamedClip.Clip = oldNamedClip.Clip;
                        newNamedClip.volume = oldNamedClip.volume;
                    }
                }

                //add to dictionary
                TMusicEnum value = (TMusicEnum)Enum.Parse(typeof(TMusicEnum), newNamedClip.Name);
                MusicDictionary[value] = newNamedClip;

                newMusicList.Add(newNamedClip);
            }
            MusicList = newMusicList.ToArray();
        }

        //Set delay variables
        lastInspectorUpdate = DateTime.Now;
    }

    IEnumerator ClipCallcabkCorutine(float timeDelay, Action action)
    {
        yield return new WaitForSeconds(timeDelay);
        action.Invoke();
    }

    IEnumerator _MusicCallcabkCorutine;
    IEnumerator MusicCallcabkCorutine(Action action, bool cancelOnInterrupt = true)
    {
        var currentClip = MainAudioSource.clip;
        float endTime = Time.time + (currentClip.length - MainAudioSource.time);

        bool wasInterrupted = false;
        while (Time.time < endTime)
        {
            if(!MainAudioSource.isPlaying || MainAudioSource.clip != currentClip)
            {
                wasInterrupted = true;
                break;
            }

            yield return null;
        }
        
        if(!wasInterrupted || (wasInterrupted && !cancelOnInterrupt))
            action.Invoke();

        _MusicCallcabkCorutine = null;
    }
}

[System.Serializable]
public class NamedClip
{
    public string Name;
    public AudioClip Clip;
    public float volume = 1f;
}