using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundReceiver : MonoBehaviour
{
    public float soundThreshold = 0.0f;
    private Receivable receive = null;
    private void Awake()
    {
        receive = GetComponent<Receivable>();
    }
    public void Receive(float intensity, Vector3 position)
    {
        receive?.ReceiveAction(position);
    }
}
