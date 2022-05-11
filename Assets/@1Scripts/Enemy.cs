using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    None, //일반
    Stun, //스턴
    Attack,
    Death,
    Guard, //경계
    Chase, //추적
    Scout, //정찰
}

public class Enemy : MonoBehaviour, IDamagable
{
    public NavMeshAgent agent;
    public Animator animator;
    public EnemyState enemyState = EnemyState.None;
    public GameObject target;
    public GameObject curTarget;
    public Transform eyeTrans;
    //public Location curLocation;
    //public Location[] scoutLocation;
    private float viewAngle = 30.0f;
    private float viewDistance = 10.0f;
    private float audioDistance = 10.0f;
    private bool isAim = false;
    private bool isAttack = false;
    private bool isDead = false;
    public float hp = 1.0f;
    public GameObject test;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if(isAim == true)
            {
                isAim = false;
            }
            else
            {
                isAim = true;
            }
            animator.SetBool("isAim", isAim);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (isAim == true)
            {
                isAttack = !isAttack;
                animator.SetBool("isAttack", isAttack);
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            agent.destination = transform.position + new Vector3(0, 0, 100);
        }
        State();
        AnimationSet();
        CheckEnemy();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 vec = eyeTrans.position + ((target.transform.position - eyeTrans.position).normalized * viewDistance);
        Gizmos.DrawLine(eyeTrans.position, vec);
    }

    private void CheckEnemy()
    {
        if (target == null)
            return;
        RaycastHit hit;

        //if (Physics.Linecast(transform.position, target.transform.position, out hit))
        //{
        //   Debug.Log(hit.collider.gameObject.name);
        //}
        Vector3 vec = eyeTrans.position + ((target.transform.position - eyeTrans.position).normalized * viewDistance);
        //Vector3 vec = target.transform.position;
        if (Physics.Linecast(eyeTrans.position, vec, out hit))//, LayerMask.GetMask("Unit", "Wall", "Ground")))
        {
            if(hit.collider != null)
            {
                Debug.Log(hit.collider.gameObject.name);
                curTarget = hit.collider.gameObject;
                if (hit.collider.gameObject == target)
                {
                    Debug.Log("찾았다");
                }
            }
        }
    }

    private void State()
    {
        if(enemyState == EnemyState.None)
        {
            animator.SetInteger("State", (int)EnemyState.None);
            //가만히 경계
        }
        else if(enemyState == EnemyState.Stun)
        {
            animator.SetInteger("State", (int)EnemyState.None);
            //행동 불능
        }
        else if(enemyState == EnemyState.Attack)
        {
            animator.SetInteger("State", (int)EnemyState.None);
            //목표 방향으로 공격(RayCast)
        }
        else if(enemyState == EnemyState.Guard)
        {
            animator.SetInteger("State", (int)EnemyState.None);
            //주변 경계
        }
        else if (enemyState == EnemyState.Chase)
        {
            animator.SetInteger("State", (int)EnemyState.None);
            //목표의 위치로 공격 태세 이동
        }
        else if (enemyState == EnemyState.Scout)
        {
            animator.SetInteger("State", (int)EnemyState.None);
            //순찰 위치를 반복 이동
        }
        else if (enemyState == EnemyState.Death)
        {
            animator.SetInteger("State", (int)EnemyState.None);
        }
    }

    private void AnimationSet()
    {
        Vector3 vec = agent.velocity;
        vec.y = 0;
        animator.SetFloat("Speed", vec.magnitude);
    }

    public void Shoot()
    {

    }

    public void Damaged(float damage)
    {
        this.hp += -damage;
        if(this.isDead == false)
        {
            if (this.hp <= 0)
            {
                animator.SetTrigger("Death");
                this.isDead = true;
                this.enemyState = EnemyState.Death;
            }
        }
        
        
    }
}

