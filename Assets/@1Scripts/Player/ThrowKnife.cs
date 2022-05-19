﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowKnife : MonoBehaviour
{
    private bool isThrow = true;
    private Vector3 pos;
    private Transform colTrans;
    public GameObject blood;
    void Start()
    {
        //transform.rotation = transform.rotation;
    }

    // Update is called once per frame
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

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.gameObject.name);
        if (isThrow == false)
        {
            return;
        }
        if (collision.collider.isTrigger == true)
        {
            return;
        }
        GameObject obj = collision.gameObject;
        if (obj.layer == LayerMask.NameToLayer("Unit"))
        {
            isThrow = false;
            colTrans = obj.transform;
            pos = this.transform.position - colTrans.position;
            this.transform.parent = obj.transform;
            GameObject bloodObj = Instantiate(blood, transform.position, Quaternion.LookRotation(transform.position - obj.transform.position));
            Destroy(bloodObj, 3.0f);
            obj.GetComponent<IDamagable>().Damaged(5);
            Destroy(this.gameObject, 0.1f);
        }
        else if (obj.layer == LayerMask.NameToLayer("Wall")
            || obj.layer == LayerMask.NameToLayer("Ground"))
        {
            isThrow = false;
            colTrans = obj.transform;
            pos = this.transform.position - colTrans.position;
            this.transform.parent = obj.transform;
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
            GameObject bloodObj = Instantiate(blood, transform.position, Quaternion.LookRotation(transform.position - obj.transform.position));
            Destroy(bloodObj, 3.0f);
            obj.GetComponent<IDamagable>().Damaged(5);
            Destroy(this.gameObject, 0.1f);
        }
        else if (obj.layer == LayerMask.NameToLayer("Wall")
            || obj.layer == LayerMask.NameToLayer("Ground"))
        {
            isThrow = false;
            colTrans = obj.transform;
            pos = this.transform.position - colTrans.position;
            this.transform.parent = obj.transform;
            Destroy(this.gameObject, 0.1f);
        }
    }

}