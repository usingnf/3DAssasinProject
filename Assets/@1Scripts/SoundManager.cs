using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Singleton
public class SoundManager : MonoBehaviour
{
    //Singleton
    public static SoundManager Instance { get; private set; }
    public GameObject soundPrefab;
    void Awake()
    {
        Instance = this;
    }

    public void PlayMusic(string music, float volume)
    {
        float musicVolume = volume * PlayerPrefs.GetFloat("MusicVolume");
    }

    //소리 크기, 소리세기, 3D사운드 설정, Enemy 탐지 가능 소리 여부 설정
    public void PlaySound(Vector3 vec, string sound, float volume = 1.0f, bool isDetecting = false, float intensity = 1.0f, float attenuation = 0.1f, float rate3D = 1.0f)
    {
        GameObject obj = Instantiate(soundPrefab, vec, Quaternion.identity);
        AudioSource audioSource = obj.GetComponent<AudioSource>();
        audioSource.clip = Resources.Load<AudioClip>("Sound/" + sound);
        audioSource.volume = volume * PlayerPrefs.GetFloat("SEVolume");
        audioSource.spatialBlend = rate3D;
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
