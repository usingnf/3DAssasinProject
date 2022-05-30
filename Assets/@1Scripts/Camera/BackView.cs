using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//플레이어 전용 백뷰 시점 카메라
public class BackView : MonoBehaviour
{
    [Header("Status")]
    public float followSpeed = 10f;
    public float sensitivity = 100f;
    public float clampAngleUp = 30f;
    public float clampAngleDown = 30f;
    private float rotX;
    private float rotY;
    public Vector3 dir;
    public Vector3 finalDir;
    public float minDistance;
    public float maxDistance;
    public float finalDistance;
    public float smoothness = 10f;

    [Header("Internal Object")]
    public Transform target;
    public Transform trans;
    public Transform cameraPos;

    [Header("Extern Object")]
    private Player player;    
    
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
        //휠을 통한 확대 축소
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

        //좌우 시점 이동
        rotX += -(Input.GetAxis("Mouse Y")) * sensitivity * Time.deltaTime;
        rotX = Mathf.Clamp(rotX, -clampAngleUp, clampAngleDown);
        //특정 동작중에 시야 이동 불가(좌우)
        if (player.isAttack == false && player.isDead == false && player.isClimb == false && player.isDoor == false)
        {
            rotY += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        }
        Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
        cameraPos.localRotation = Quaternion.Euler(rotX, 0, 0);

        //특정 동작중에 시야 이동 불가(상하)
        if(player.isClimb == true)
        {
            rotY = target.localRotation.eulerAngles.y;
        }
        trans.rotation = Quaternion.Euler(0, rotY, 0);

        /*
        //카메라 페이드인, 페이드 아웃.
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

    //이미지를 활용한 페이드인, 페이드아웃
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
        //카메라와 목표사이에 장애물이 있을 경우 카메라를 근접시킴.
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
