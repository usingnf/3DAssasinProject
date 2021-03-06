using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//충돌과 Raycast로 청각 기능 구현
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
    
    void Awake()
    {
        receiverDic = new Dictionary<int, SoundReceiver>();
        if (emitterObject == null)
            emitterObject = gameObject;
        this.GetComponent<SphereCollider>().radius = soundIntensity;
    }

    public void EmitReady()
    {
        StartCoroutine(Emit());
    }

    private IEnumerator Emit()
    {
        yield return new WaitForSeconds(0.02f);
        GameObject srObj;
        Vector3 srPos;
        float intensity;
        float distance;
        Vector3 emitterPos = emitterObject.transform.position;
        foreach (SoundReceiver sr in receiverDic.Values)
        {
            intensity = soundIntensity;
            //Raycast로 장애물에 의한 소리 감소
            Vector3 direction = sr.transform.position - emitterPos;
            float rayDistance = direction.magnitude;
            direction.Normalize();
            Ray ray = new Ray(emitterPos, direction);
            RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance, LayerMask.GetMask("Wall"));
            foreach (RaycastHit hit in hits)
            {
                intensity *= 0.8f;
            }
            // 거리 소리 감소 계산
            srObj = sr.gameObject;
            srPos = srObj.transform.position;
            distance = Vector3.Distance(srPos, emitterPos);
            
            intensity -= soundAttenuation * distance;
            if (intensity < sr.soundThreshold)
                continue;

            sr.Receive(soundIntensity, emitterPos);
        }
        yield break;
    }

    //충돌을 통해 범위내 오브젝트에 대해서만 계산
    private void OnTriggerEnter(Collider other)
    {
        SoundReceiver receiver = null;
        receiver = other.gameObject.GetComponent<SoundReceiver>();
        if (receiver == null)
            return;
        int objId = other.gameObject.GetInstanceID();
        if (receiverDic.ContainsKey(objId) == true)
            return;
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
