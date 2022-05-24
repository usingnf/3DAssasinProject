using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BackView : MonoBehaviour
{
    public Transform target;
    private Player player;
    public float followSpeed = 10f;
    public float sensitivity = 100f;
    public float clampAngleUp = 30f;
    public float clampAngleDown = 30f;
    private float rotX;
    private float rotY;

    public Transform trans;
    public Transform cameraPos;
    public Vector3 dir;
    public Vector3 finalDir;
    public float minDistance;
    public float maxDistance;
    public float finalDistance;
    public float smoothness = 10f;
    void Start()
    {
        rotX = transform.localRotation.eulerAngles.x;
        rotY = transform.localRotation.eulerAngles.y;

        dir = trans.localPosition.normalized;
        finalDistance = trans.localPosition.magnitude;

        Cursor.lockState = CursorLockMode.Locked;
        player = target.GetComponent<Player>();
        //Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            minDistance += -Time.deltaTime * 10;
            maxDistance += -Time.deltaTime * 10;
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            minDistance += Time.deltaTime * 10;
            maxDistance += Time.deltaTime * 10;
        }
        rotX += -(Input.GetAxis("Mouse Y")) * sensitivity * Time.deltaTime;
        
        rotX = Mathf.Clamp(rotX, -clampAngleUp, clampAngleDown);
        if (player.isAttack == false && player.isDead == false && player.isClimb == false && player.isDoor == false)
        {
            rotY += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        }
        Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
        
        cameraPos.localRotation = Quaternion.Euler(rotX, 0, 0);
        if(player.isClimb == true)
        {
            rotY = target.localRotation.eulerAngles.y;
        }
        trans.rotation = Quaternion.Euler(0, rotY, 0);

        /*
        if(Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(FadeIn(1.0f));
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(FadeOut(1.0f));
        }
        if(Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(FadeInOut(1.0f));
        }
        */
    }

    public IEnumerator FadeIn(float time)
    {
        GameObject image = Instantiate(Resources.Load<GameObject>("Image"), GameObject.Find("Canvas").transform);
        image.GetComponent<Image>().color = new Color(0, 0, 0, 1);
        for (int i = 0; i < 100; i++)
        {
            image.GetComponent<Image>().color += new Color(0, 0, 0, -0.01f);
            yield return new WaitForSeconds(time * 0.01f);
        }
        Destroy(image);
        yield break;
    }
    public IEnumerator FadeOut(float time)
    {
        GameObject image = Instantiate(Resources.Load<GameObject>("Image"), GameObject.Find("Canvas").transform);
        image.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        for (int i = 0; i < 100; i++)
        {
            image.GetComponent<Image>().color += new Color(0, 0, 0, 0.01f);
            yield return new WaitForSeconds(time * 0.01f);
        }
        Destroy(image);
        yield break;
    }

    public IEnumerator FadeInOut(float time)
    {
        GameObject image = Instantiate(Resources.Load<GameObject>("Image"), GameObject.Find("Canvas").transform);
        image.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        for (int i = 0; i < 50; i++)
        {
            image.GetComponent<Image>().color += new Color(0, 0, 0, 0.02f);
            yield return new WaitForSeconds(time * 0.02f);
        }
        for (int i = 0; i < 50; i++)
        {
            image.GetComponent<Image>().color += new Color(0, 0, 0, -0.02f);
            yield return new WaitForSeconds(time * 0.02f);
        }
        Destroy(image);
        yield break;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, cameraPos.position);
    }

    private void LateUpdate()
    {        
        if (Physics.Linecast(cameraPos.position - (cameraPos.forward * maxDistance), cameraPos.position, out RaycastHit hit))
        {
            if(hit.collider != null)
            {
                if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall") ||
                    hit.collider.gameObject.layer == LayerMask.NameToLayer("ClimbWall") ||
                    hit.collider.gameObject.layer == LayerMask.NameToLayer("Door") ||
                    hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    if (finalDistance > minDistance)
                    {
                        finalDistance += -followSpeed * Time.deltaTime;
                    }                        
                    else
                        finalDistance = minDistance;
                }
                else
                {
                    if (finalDistance < maxDistance)
                    {
                        finalDistance += followSpeed * Time.deltaTime;
                    }
                    else
                        finalDistance = maxDistance;
                }
            }
            finalDistance = Mathf.Clamp(finalDistance, minDistance, maxDistance);
        }
        if (finalDistance < minDistance)
            finalDistance = minDistance;
        else if (finalDistance > maxDistance)
            finalDistance = maxDistance;
        transform.position = cameraPos.position - (cameraPos.forward * finalDistance);
        transform.LookAt(cameraPos.position);
    }
}
