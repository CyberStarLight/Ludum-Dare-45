using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundClip3D : MonoBehaviour
{
    public AudioClip Clip;
    public float Volume;
    public float MinDistance;
    public float MaxxDistance;

    //to avoid having to set a prefab inside the base project, we build the GameObject at runtime
    public static SoundClip3D InstantiateAt(Vector3 worldPos, AudioClip _Clip, float _Volume = 1f, float _MinDistance = 1f, float _MaxDistance = 100f)
    {
        //Create new object
        var newGameObject = new GameObject();
        newGameObject.name = "3D Sound - " + _Clip.name;
        
        //Place object at target position
        newGameObject.transform.position = worldPos;

        //Add AudioSource component
        var audioSource = newGameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = _MinDistance;
        audioSource.maxDistance = _MaxDistance;
        audioSource.PlayOneShot(_Clip, _Volume);

        //Add SoundClip3D component
        var soundClip3D = newGameObject.AddComponent<SoundClip3D>();
        soundClip3D.Clip = _Clip;
        soundClip3D.Volume = _Volume;
        soundClip3D.MinDistance = _MinDistance;
        soundClip3D.MaxxDistance = _MaxDistance;
        
        //Set to selfdestruct when clip finished playing
        soundClip3D.Invoke("SelfDestruct", _Clip.length + 0.1f);

        return soundClip3D;
    }

    private void SelfDestruct()
    {
        Destroy(gameObject);
    }
    
}
