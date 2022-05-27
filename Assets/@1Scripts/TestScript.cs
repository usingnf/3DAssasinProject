using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MeshFilter[] meshFilter = GetComponentsInChildren<MeshFilter>();
        foreach(MeshFilter filter in meshFilter)
        {
            Debug.Log(filter.gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            SoundManager.Instance.PlaySound(this.transform.position, "GunFire3", 1.0f, true, 13.0f, 0.1f);
        }
        
    }

}
