using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowKnife : MonoBehaviour
{
    [Header("Internal Object")]
    private bool isThrow = true;
    private Vector3 pos;
    private Transform colTrans;
    [Header("Extern Object")]
    private Player player;
    public GameObject blood;

    //바라보는 방향으로 날아감. 충돌시 위치 고정.
    void Update()
    {
        if(isThrow)
        {
            transform.position += (transform.forward * 20 * Time.deltaTime);
        }
        else
        {
            this.transform.position = colTrans.position + pos;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isThrow == false)
        {
            return;
        }
        if (other.isTrigger == true)
        {
            return;
        }
        GameObject obj = other.gameObject;
        if (obj.layer == LayerMask.NameToLayer("Unit"))
        {
            isThrow = false;
            colTrans = obj.transform;
            pos = this.transform.position - colTrans.position;
            this.transform.parent = obj.transform;
            if (obj.CompareTag("Enemy"))
            { 
                GameObject bloodObj = Instantiate(blood, transform.position, Quaternion.LookRotation(transform.position - obj.transform.position));
                Destroy(bloodObj, 3.0f);
            }
            obj.GetComponent<IDamagable>().Damaged(5);
            Destroy(this.gameObject, 0.2f);
        }
        else if (obj.layer == LayerMask.NameToLayer("Wall") ||
            obj.layer == LayerMask.NameToLayer("ClimbWall") ||
            obj.layer == LayerMask.NameToLayer("Door") ||
            obj.layer == LayerMask.NameToLayer("Ground"))
        {
            //장애물에 충돌시 정지
            isThrow = false;
            colTrans = obj.transform;
            pos = this.transform.position - colTrans.position;
            this.transform.parent = obj.transform;
            Destroy(this.gameObject, 0.2f);
        }
    }

}
