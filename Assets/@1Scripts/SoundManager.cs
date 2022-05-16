using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public GameObject soundPrefab;
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySound(Vector3 vec, string sound, float volume = 1.0f, bool isDetecting = false, float intensity = 1.0f, float attenuation = 0.1f)
    {
        GameObject obj = Instantiate(soundPrefab, vec, Quaternion.identity);
        AudioSource audioSource = obj.GetComponent<AudioSource>();
        audioSource.clip = Resources.Load<AudioClip>("Sound/" + sound);
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(obj, 1.0f);
        if(isDetecting == true)
        {
            SoundEmitter soundEmitter = obj.GetComponent<SoundEmitter>();
            soundEmitter.soundIntensity = intensity;
            soundEmitter.soundAttenuation = attenuation;
            obj.GetComponent<SphereCollider>().radius = intensity;
            soundEmitter.EmitReady();
        }
    }
}
