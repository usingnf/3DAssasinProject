using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum etest
{
    a,
    b,
    c,
    d,
}

public class TitleCamera : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        this.transform.rotation *= Quaternion.Euler(0, 30.0f * Time.deltaTime, 0);
    }
}
