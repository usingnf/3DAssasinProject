using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//플레이어 위에서 Minimap Layer만 확인하여 미니맵 텍스쳐 생성.
public class MinimapCamera : MonoBehaviour
{
    public GameObject target;
    void Awake()
    {
        if(target != null)
            this.transform.position = target.transform.position + new Vector3(0, 20.0f,0);
    }

    //일관된 카메라 이동을 위해 LateUpdate에서 이동.
    void LateUpdate()
    {
        if (target != null)
            this.transform.position = target.transform.position + new Vector3(0, 20.0f, 0);
    }
}
