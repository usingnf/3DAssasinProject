using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    public GameObject target;
    void Awake()
    {
        if(target != null)
            this.transform.position = target.transform.position + new Vector3(0, 20.0f,0);
    }

    void Update()
    {
        if (target != null)
            this.transform.position = target.transform.position + new Vector3(0, 20.0f, 0);
    }
}
