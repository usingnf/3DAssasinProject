using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamagable
{
    private Transform trans;
    private Animator animator;
    public CharacterController characterController;

    //0 == stand, 1 == Sit
    public int stand = 0;
    public int walk = 0;
    public int angle = 0;
    private float gravity = -9.0f;
    private IEnumerator jumpReady = null;
    private IEnumerator standUpReady = null;
    private IEnumerator standDownReady = null;
    public bool isGravity = true;
    public float jumpPower = 0.0f;
    public bool isAttack = false;
    public float hp = 1;
    public bool isDead = false;
    public bool isGround = true;
    public float fallingIntensity = 0.0f;
    public Transform knifeTrans;
    public Transform attackPoint;
    public GameObject blood;
    public GameObject detectLine;
    public float speed = 2.0f;
    private List<Collider> detectCollider = new List<Collider>();

    void Start()
    {
        
        trans = transform;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Detecting(10.0f);
            //SoundManager.Instance.PlaySound(transform.position, "FootStep1", 1.0f, true, 1, 0.1f);
        }
        AnimationPlay();
        Fall();
        Move();
        Jump();
        Attack();
    }


    private void Attack()
    {
        if (isAttack == true)
            return;
        if (isDead == true)
            return;
        if (animator.GetBool("isGround") == false)
            return;

        if(Input.GetMouseButtonDown(0))
        {
            isAttack = true;
            animator.SetBool("isAttack", true);
        }
    }

    private void Jump()
    {
        if (isAttack == true)
            return;
        if (isDead == true)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(characterController.isGrounded == true)
            {
                if (jumpReady != null)
                    StopCoroutine(jumpReady);
                jumpReady = JumpReady();
                StartCoroutine(jumpReady);
            }
        }
        
    }

    private IEnumerator JumpReady()
    {
        animator.SetBool("isJump", true);
        yield return new WaitForSeconds(0.2f);
        animator.SetBool("isJump", false);

        yield break;
    }

    private void Fall()
    {
        if (characterController.isGrounded == false)
        {
            if(isGround == true)
            {
                fallingIntensity = 0.0f;
                isGround = false;
            }
            else
            {
                fallingIntensity += Time.deltaTime;
            }
            
        }
        else
        {
            if(isGround == true)
            {

            }
            else
            {
                if(fallingIntensity > 0.1f)
                {
                    SoundManager.Instance.PlaySound(transform.position, "Fall", 1.0f, true, 1.0f, 0.1f);
                }
                fallingIntensity = 0.0f;
                isGround = true;
            }
            
        }
    }

    private void Move()
    {
        Vector3 vec = Vector3.zero;

        if (isAttack == true)
        {
            jumpPower += gravity * Time.deltaTime;
            if (jumpPower < gravity)
                jumpPower = gravity;
            vec.y = jumpPower;
            characterController.Move(vec * Time.deltaTime);
            return;
        }
        if (isDead == true)
            return;

        if (Input.GetKey(KeyCode.W))
        {
            vec += trans.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            vec += Quaternion.Euler(0, -90, 0) * trans.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            vec += Quaternion.Euler(0, 180, 0) * trans.forward;
        }
        if(Input.GetKey(KeyCode.D))
        {
            vec += Quaternion.Euler(0, 90, 0) * trans.forward;
        }

        float temp = Mathf.Atan2(trans.forward.x, trans.forward.z) * Mathf.Rad2Deg;
        float temp2 = Mathf.Atan2(characterController.velocity.x, characterController.velocity.z) * Mathf.Rad2Deg;
        float temp3 = Mathf.Cos((temp2 - temp) * Mathf.Deg2Rad);
        float temp4 = Mathf.Sin((temp2 - temp) * Mathf.Deg2Rad);
        if(temp3 > Mathf.Cos(60.0f * Mathf.Deg2Rad))
        {
            angle = 1;
        }
        else if (temp3 < Mathf.Cos(120.0f * Mathf.Deg2Rad))
        {
            angle = 4;
        }
        else if(temp4 > Mathf.Sin(45.0f * Mathf.Deg2Rad))
        {
            angle = 3;
        }
        else if(temp4 < Mathf.Sin(225.0f * Mathf.Deg2Rad))
        {
            angle = 2;
        }
        vec.Normalize();
        vec *= speed;
        if (stand == 1)
            vec *= 0.5f;
        if(Input.GetKey(KeyCode.LeftShift))
        {
            vec *= 2;
        }
        jumpPower += gravity * Time.deltaTime;
        if (jumpPower < gravity)
            jumpPower = gravity;
        vec.y = jumpPower;
        characterController.Move(vec * Time.deltaTime);
    }

    private IEnumerator StandUpReady()
    {
        animator.SetBool("isStandUp", true);
        yield return new WaitForSeconds(0.2f);
        animator.SetBool("isStandUp", false);

        yield break;
    }

    private IEnumerator StandDownReady()
    {
        animator.SetBool("isStandDown", true);
        yield return new WaitForSeconds(0.2f);
        animator.SetBool("isStandDown", false);

        yield break;
    }

    private void AnimationPlay()
    {
        if (isDead == true)
            return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (stand == 0)
            {
                if (standDownReady != null)
                    StopCoroutine(standDownReady);
                standDownReady = StandDownReady();
                StartCoroutine(standDownReady);
            }

            else if (stand == 1)
            {
                if (standUpReady != null)
                    StopCoroutine(standUpReady);
                standUpReady = StandUpReady();
                StartCoroutine(standUpReady);
            }
        }
        if(characterController.isGrounded == true)
        {
            animator.SetBool("isGround", true);
        }
        else
        {
            animator.SetBool("isGround", false);
        }
        animator.SetFloat("Stand", stand);
        animator.SetFloat("Angle", angle);
        
        Vector3 vec = characterController.velocity;
        vec.y = 0;
        animator.SetFloat("Speed", vec.magnitude);
    }

    public void KnifeAttackRange()
    {
        Collider[] hit;
        hit = Physics.OverlapSphere(knifeTrans.position, 0.6f, LayerMask.GetMask("Unit"));
        bool attackSuccess = false;
        foreach (Collider c in hit)
        {
            if(c.isTrigger == true)
            {
                continue;
            }
            attackSuccess = true;
            GameObject obj = Instantiate(blood, attackPoint.position, Quaternion.LookRotation(knifeTrans.position-attackPoint.position));
            Destroy(obj, 3.0f);
            c.GetComponent<IDamagable>().Damaged(5);
        }
        if (attackSuccess == true)
        {
            SoundManager.Instance.PlaySound(this.transform.position, "KillBlood", 1.0f, true, 1.0f, 0.1f);
        }
    }

    public void KnifeSound()
    {
        SoundManager.Instance.PlaySound(transform.position, "KnifeAttack", 1.0f, true, 1.0f, 0.1f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(knifeTrans.position, 0.6f);
    }

    public bool Damaged(float damage)
    {
        this.hp += -damage;
        hp = 100;
        if (this.isDead == false)
        {
            if (this.hp <= 0)
            {
                animator.SetTrigger("Death");
                this.isDead = true;
                
            }
            return true;
        }
        return false;
    }

    public void FootStep(float intensity)
    {
        SoundManager.Instance.PlaySound(transform.position + new Vector3(0,0.1f, 0), "FootStep1", 1.0f, true, intensity, 0.1f);
    }
    public void FootStep2(float intensity)
    {
        SoundManager.Instance.PlaySound(transform.position + new Vector3(0, 0.1f, 0), "FootStep1", 0.5f, true, intensity, 0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Finish")
        {
            GameManager.Instance.Finish();
        }
    }

    private void Detecting(float range)
    {
        detectCollider.Clear();
        GameObject obj = Instantiate(detectLine, trans.position, Quaternion.identity);
        StartCoroutine(DetectingLoop(range, obj));
    }

    private IEnumerator DetectingLoop(float range, GameObject obj)
    {
        float nrange = 1.0f;
        float lineSpeed = 0.2f;
        int count = (int)((range - nrange) / lineSpeed);
        Transform tempTrans = obj.transform;
        for(int i = 0; i < count; i++)
        {
            nrange += lineSpeed;
            tempTrans.localScale = new Vector3((nrange-1)*2, 0.01f, (nrange - 1) * 2);

            if (i % 10 == 0)
            {
                Collider[] col = Physics.OverlapSphere(transform.position, nrange, LayerMask.GetMask("Unit"));

                foreach (Collider collider in col)
                {
                    if (collider.isTrigger == true)
                        continue;
                    if (detectCollider.Contains(collider))
                        continue;
                    detectCollider.Add(collider);
                    Debug.Log(collider.gameObject.name);
                    Outline outline = collider.GetComponent<Outline>();
                    if (outline != null)
                    {
                        StartCoroutine(outline.Detect());
                    }
                }
            }
            
            yield return new WaitForSeconds(0.02f);
        }
        Destroy(obj);
        yield return null;
    }
}
