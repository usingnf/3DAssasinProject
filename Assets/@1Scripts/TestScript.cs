using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q");
            SoundManager.Instance.PlaySound(transform.position, "SwatFootStep", 1.0f, false);
        }
    }

    private void OnMouseDown()
    {
        Debug.Log("!");
    }
}
