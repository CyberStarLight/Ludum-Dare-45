using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FMOD.Studio;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class BaseFMODManager<TManager, TSoundsEnum, TMusicEnum> : SmartBeheviourSingleton<TManager> where TManager : BaseFMODManager<TManager, TSoundsEnum, TMusicEnum>
{
    private EventInstance MainAudioSource;
    private EventInstance SecondaryAudioSource;

    [Header("Clips List")]
    public FMODClip[] SoundsList;
    public FMODClip[] MusicList;

    public static bool IsMusicPlaying { get { return Instance.isMusicPLaying(); } }
    public static float CurrentMusicTime { get { return Instance.getMusicTime(); } }
    public static TMusicEnum CurrentMusic { get { return Instance.currentMusic; } }

    protected Dictionary<TSoundsEnum, FMODClip> SoundsDictionary = new Dictionary<TSoundsEnum, FMODClip>();
    protected Dictionary<TMusicEnum, FMODClip> MusicDictionary = new Dictionary<TMusicEnum, FMODClip>();

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

    public static void StopMusic(FMOD.Studio.STOP_MODE mode = FMOD.Studio.STOP_MODE.IMMEDIATE)
    {
        Instance.MainAudioSource.stop(mode);
    }

    public static FMODClip Play(TSoundsEnum sound, Action SoundEndedCallback = null)
    {
        return Instance.playSound(sound, SoundEndedCallback);
    }

    public static FMODClip Play(TMusicEnum music, TMusicEnum intro, float time = 0f, Action MusicEndedCallback = null)
    {
        return Instance.playMusic(intro, time, () => {
            Play(music, 0f, MusicEndedCallback);
        });
    }

    public static FMODClip Play(TMusicEnum music, float time = 0f, Action MusicEndedCallback = null, bool cancelCallbackOnInterrupt = true)
    {
        return Instance.playMusic(music, time, MusicEndedCallback, cancelCallbackOnInterrupt);
    }
    
    //public static SoundClip3D PlaySound3D(TSoundsEnum sound, Vector3 worldPos, float minDistance = 1f, float maxDistance = 100f, Action SoundEndedCallback = null)
    //{
    //    return Instance.playSound3D(sound, worldPos, minDistance, maxDistance, SoundEndedCallback);
    //}

    private FMODClip playSound(TSoundsEnum sound, Action SoundEndedCallback = null)
    {
        FMODClip namedClip = null;
        if (SoundsDictionary.TryGetValue(sound, out namedClip) && namedClip != null && namedClip.EventPath != null)
        {
            FMODUnity.RuntimeManager.PlayOneShot(namedClip.EventPath);
            //MainAudioSource.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            var oneShotInstance = FMODUnity.RuntimeManager.CreateInstance(namedClip.EventPath);
            //oneShotInstance.setVolume(namedClip.volume);
            float clipLength = getEventLength(oneShotInstance);
            //oneShotInstance.start();

            //MainAudioSource.PlayOneShot(namedClip.EventPath, namedClip.volume);

            if (SoundEndedCallback != null)
                StartCoroutine(ClipCallcabkCorutine(clipLength, SoundEndedCallback));
        }
        
        return namedClip;
    }

    //public SoundClip3D playSound3D(TSoundsEnum sound, Vector3 worldPos, float minDistance = 1f, float maxDistance = 100f, Action SoundEndedCallback = null)
    //{
    //    FMODClip namedClip = null;
    //    if (SoundsDictionary.TryGetValue(sound, out namedClip) && namedClip != null && namedClip.EventPath != null)
    //    {
    //        var newSoundClip3D = SoundClip3D.InstantiateAt(worldPos, namedClip.EventPath, namedClip.volume, minDistance, maxDistance);

    //        if (SoundEndedCallback != null)
    //            StartCoroutine(ClipCallcabkCorutine(namedClip.EventPath.length, SoundEndedCallback));

    //        return newSoundClip3D;
    //    }

    //    return null;
    //}

    public FMODClip playMusic(TMusicEnum music, float time = 0f, Action MusicEndedCallback = null, bool cancelCallbackOnInterrupt = true)
    {
        FMODClip namedClip = null;
        if (MusicDictionary.TryGetValue(music, out namedClip) && namedClip != null && namedClip.EventPath != null)
        {
            MainAudioSource.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            MainAudioSource = FMODUnity.RuntimeManager.CreateInstance(namedClip.EventPath);
            MainAudioSource.setVolume(namedClip.volume);
            MainAudioSource.start();
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
            List<FMODClip> newSoundsList = new List<FMODClip>();
            foreach (var soundName in soundNames)
            {
                var newFMODClip = new FMODClip();
                newFMODClip.Name = soundName;

                if(SoundsList != null)
                {
                    //check if the clip already been set before, and move it to the new list
                    var oldFMODClip = SoundsList.FirstOrDefault(x => x.Name == soundName);
                    if (oldFMODClip != null)
                    {
                        newFMODClip.EventPath = oldFMODClip.EventPath;
                        newFMODClip.volume = oldFMODClip.volume;
                    }
                }

                //add to dictionary
                TSoundsEnum value = (TSoundsEnum)Enum.Parse(typeof(TSoundsEnum), newFMODClip.Name);
                SoundsDictionary[value] = newFMODClip;

                newSoundsList.Add(newFMODClip);
            }
            SoundsList = newSoundsList.ToArray();
        }
        
        //Update music list
        var musicNames = Enum.GetNames(typeof(TMusicEnum));
        if (lastEnumCount_Music != musicNames.Length || lastListCount_Music != musicNames.Length)
        {
            MusicDictionary.Clear();
            List<FMODClip> newMusicList = new List<FMODClip>();
            foreach (var musicName in musicNames)
            {
                var newFMODClip = new FMODClip();
                newFMODClip.Name = musicName;

                if(MusicList != null)
                {
                    //check if the clip already been set before, and move it to the new list
                    var oldFMODClip = MusicList.FirstOrDefault(x => x.Name == musicName);
                    if (oldFMODClip != null)
                    {
                        newFMODClip.EventPath = oldFMODClip.EventPath;
                        newFMODClip.volume = oldFMODClip.volume;
                    }
                }

                //add to dictionary
                TMusicEnum value = (TMusicEnum)Enum.Parse(typeof(TMusicEnum), newFMODClip.Name);
                MusicDictionary[value] = newFMODClip;

                newMusicList.Add(newFMODClip);
            }
            MusicList = newMusicList.ToArray();
        }

        //Set delay variables
        lastInspectorUpdate = DateTime.Now;
    }

    //Helpers
    private bool isMusicPLaying()
    {
        MainAudioSource.getDescription(out FMOD.Studio.EventDescription description);
        MainAudioSource.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE state);

        return state == PLAYBACK_STATE.PLAYING;
    }

    private float getMusicTime()
    {
        MainAudioSource.getTimelinePosition(out int intMilisecondPosition);
        return intMilisecondPosition / 1000f;
    }

    private float getEventLength(EventInstance audioEvent)
    {
        audioEvent.getDescription(out FMOD.Studio.EventDescription description);
        description.getLength(out int milisecondLength);
        return milisecondLength / 1000f;
    }

    //Courutines
    IEnumerator ClipCallcabkCorutine(float timeDelay, Action action)
    {
        yield return new WaitForSeconds(timeDelay);
        action.Invoke();
    }

    IEnumerator _MusicCallcabkCorutine;
    IEnumerator MusicCallcabkCorutine(Action action, bool cancelOnInterrupt = true)
    {
        MainAudioSource.getDescription(out FMOD.Studio.EventDescription description);
        description.getLength(out int milisecondLength);
        MainAudioSource.getTimelinePosition(out int intMilisecondPosition);
        MainAudioSource.getPlaybackState( out FMOD.Studio.PLAYBACK_STATE state);
        float clipTimePosition = intMilisecondPosition / 1000f;
        float clipLength = milisecondLength / 1000f;
        float endTime = Time.time + (clipLength - clipTimePosition);
        description.getPath(out string startingClipPath);

        bool wasInterrupted = false;
        while (Time.time < endTime)
        {
            MainAudioSource.getDescription(out FMOD.Studio.EventDescription currentDescription);
            description.getPath(out string currentClipPath);

            if (state != FMOD.Studio.PLAYBACK_STATE.PLAYING || currentClipPath != startingClipPath)
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
public class FMODClip
{
    public string Name;
    public string EventPath;
    public float volume = 1f;
}