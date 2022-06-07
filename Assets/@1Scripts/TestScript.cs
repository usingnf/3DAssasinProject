using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MinimapCamera minimapCamera = GetComponentInChildren<MinimapCamera>();
        Debug.Log(minimapCamera.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void message()
    {
       
    }

}
