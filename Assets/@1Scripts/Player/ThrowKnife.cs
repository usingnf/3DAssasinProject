using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowKnife : MonoBehaviour
{
    public Vector3 angle;
    void Start()
    {
        //transform.rotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += (transform.forward * 20 * Time.deltaTime);
    }
}
