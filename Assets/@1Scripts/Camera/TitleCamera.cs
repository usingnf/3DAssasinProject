using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//회전하는 (카메라의 상위 오브젝트)
public class TitleCamera : MonoBehaviour
{
    void LateUpdate()
    {
        this.transform.rotation *= Quaternion.Euler(0, 30.0f * Time.deltaTime, 0);
    }
}
