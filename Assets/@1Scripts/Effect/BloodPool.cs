using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
