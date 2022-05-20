using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCamera : MonoBehaviour
{
    void Update()
    {
        this.transform.rotation *= Quaternion.Euler(0, 30.0f * Time.deltaTime, 0);
    }
}
