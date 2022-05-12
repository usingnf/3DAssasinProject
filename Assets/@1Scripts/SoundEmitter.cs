using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    // 소리 세기
    public float soundIntensity;
    // 소리 감쇠
    public float soundAttenuation;
    // 소리 주체
    public GameObject emitterObject;
    // 소리를 들을 수 있는 거리에 있는 오브젝트 목록
    private Dictionary<int, SoundReceiver> receiverDic;
    // Start is called before the first frame update
    void Start()
    {
        receiverDic = new Dictionary<int, SoundReceiver>();
        if (emitterObject == null)
            emitterObject = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Emit()
    {
        // 소리 내기
        GameObject srObj;
        Vector3 srPos;
        float intensity;
        float distance;
        Vector3 emitterPos = emitterObject.transform.position;

        foreach (SoundReceiver sr in receiverDic.Values)
        {
            // 소리 들었음! 장애물 효과는 미적용.
            srObj = sr.gameObject;
            srPos = srObj.transform.position;
            distance = Vector3.Distance(srPos, emitterPos);
            intensity = soundIntensity;
            intensity -= soundAttenuation * distance;
            //레이 케이스트로 장애물 효과 추가
            if (intensity < sr.soundThreshold)
                continue;
            sr.Receive(intensity, emitterPos);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SoundReceiver receiver = null;
        receiver = other.gameObject.GetComponent<SoundReceiver>();
        if (receiver == null)
            return;
        int objId = other.gameObject.GetInstanceID();
        receiverDic.Add(objId, receiver);
    }

    private void OnTriggerExit(Collider other)
    {
        SoundReceiver receiver = null;
        receiver = other.gameObject.GetComponent<SoundReceiver>();
        if (receiver == null)
            return;
        int objId = other.gameObject.GetInstanceID();
        receiverDic.Remove(objId);
    }
}
