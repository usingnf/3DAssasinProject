using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Singleton
public class SoundManager : MonoBehaviour
{
    //Singleton
    public static SoundManager Instance { get; private set; }
    public GameObject soundPrefab;
    private List<GameObject> sounds = new List<GameObject>();
    private int sequence = 0;
    void Awake()
    {
        Instance = this;
        for(int i = 0; i < 30; i++)
        {
            GameObject obj = Instantiate(soundPrefab);
            obj.SetActive(false);
            sounds.Add(obj);
        }
            
    }

    public void PlayMusic(string music, float volume)
    {
        float musicVolume = volume * PlayerPrefs.GetFloat("MusicVolume");
    }

    //소리 크기, 소리세기, 3D사운드 설정, Enemy 탐지 가능 소리 여부 설정
    //사운드 객체는 오브젝트 풀링 사용.
    public void PlaySound(Vector3 vec, string sound, float volume = 1.0f, bool isDetecting = false, float intensity = 1.0f, float attenuation = 0.1f, float rate3D = 1.0f)
    {
        //GameObject obj = Instantiate(soundPrefab, vec, Quaternion.identity);
        //오브젝트 풀링
        GameObject obj = sounds[sequence];
        obj.SetActive(true);
        sequence++;
        if (sequence >= sounds.Count)
            sequence = 0;
        obj.transform.position = vec;
        AudioSource audioSource = obj.GetComponent<AudioSource>();
        audioSource.clip = Resources.Load<AudioClip>("Sound/" + sound);
        audioSource.volume = volume * PlayerPrefs.GetFloat("SEVolume");
        audioSource.spatialBlend = rate3D;
        audioSource.Play();
        StartCoroutine(DisableSoundObject(obj, 1.0f));
        //Destroy(obj, 1.0f);
        if(isDetecting == true)
        {
            SoundEmitter soundEmitter = obj.GetComponent<SoundEmitter>();
            soundEmitter.soundIntensity = intensity;
            soundEmitter.soundAttenuation = attenuation;
            obj.GetComponent<SphereCollider>().radius = intensity;
            soundEmitter.EmitReady();
        }
    }

    private IEnumerator DisableSoundObject(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }
}
