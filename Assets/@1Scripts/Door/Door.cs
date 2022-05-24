using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IOpen
{
    private float startAngle = 0;
    public Transform door;
    private IEnumerator excuteCoroutine = null;

    // Start is called before the first frame update
    void Start()
    {
        startAngle = this.transform.rotation.eulerAngles.y;
    }

    public void Close(float intensity)
    {
        float spaceAngle = door.rotation.eulerAngles.y - startAngle;
        if (Mathf.Sin(spaceAngle * Mathf.Deg2Rad) <= Mathf.Sin(90 * Mathf.Deg2Rad) &&
            Mathf.Sin(spaceAngle * Mathf.Deg2Rad) >= Mathf.Sin(0 * Mathf.Deg2Rad))
        {
            door.Rotate(new Vector3(0, -intensity * Time.deltaTime, 0));
        }
    }

    public void Open(float intensity)
    {
        float spaceAngle = door.rotation.eulerAngles.y - startAngle;
        if(Mathf.Cos(spaceAngle * Mathf.Deg2Rad) >= Mathf.Cos(90 * Mathf.Deg2Rad))
        {
            door.Rotate(new Vector3(0, intensity * Time.deltaTime, 0));
        }
    }
    public float GetAngle()
    {
        return startAngle;
    }

    public void OpenPerfect(float intensity)
    {
        if (excuteCoroutine != null)
            StopCoroutine(excuteCoroutine);
        excuteCoroutine = OpenCoroutine(intensity);
        StartCoroutine(excuteCoroutine);
    }

    private IEnumerator OpenCoroutine(float intensity)
    {
        float spaceAngle = 0;
        while (true)
        {
            spaceAngle = door.rotation.eulerAngles.y - startAngle;
            if (Mathf.Cos(spaceAngle * Mathf.Deg2Rad) >= Mathf.Cos(90 * Mathf.Deg2Rad))
            {
                door.Rotate(new Vector3(0, intensity * Time.deltaTime, 0));
                yield return new WaitForEndOfFrame();
            }
            else
            {
                break;
            }
            
        }
        yield return null;
    }

    public void ClosePerfect(float intensity)
    {
        if (excuteCoroutine != null)
            StopCoroutine(excuteCoroutine);
        excuteCoroutine = CloseCoroutine(intensity);
        StartCoroutine(excuteCoroutine);
    }

    private IEnumerator CloseCoroutine(float intensity)
    {
        float spaceAngle = 0;
        while (true)
        {
            spaceAngle = door.rotation.eulerAngles.y - startAngle;
            if (Mathf.Sin(spaceAngle * Mathf.Deg2Rad) <= Mathf.Sin(90 * Mathf.Deg2Rad) &&
            Mathf.Sin(spaceAngle * Mathf.Deg2Rad) >= Mathf.Sin(0 * Mathf.Deg2Rad))
            {
                door.Rotate(new Vector3(0, intensity * Time.deltaTime, 0));
                yield return new WaitForEndOfFrame();
            }
            else
            {
                break;
            }

        }
        yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.isTrigger == true)
        {
            return;
        }
        if(other.gameObject.tag == "Enemy")
        {
            OpenPerfect(100);
        }
        
    }
}
