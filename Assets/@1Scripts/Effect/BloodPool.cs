using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//피웅덩이의 크기를 실시간으로 증가. 시각효과는 Particle을 사용하므로 둘의 크기를 동시에 조절필요.
public class BloodPool : MonoBehaviour
{
    private BoxCollider boxCollider;
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        StartCoroutine(StartSpread(1.4f));
    }
    
    private IEnumerator StartSpread(float size)
    {
        while(true)
        {
            if (boxCollider.size.x > size)
                break;
            boxCollider.size += new Vector3(0.01f, 0, 0.01f);
            yield return new WaitForSeconds(0.04f);
        }
    }
}
