using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    None, //일반
    Stun, //스턴
    Attack, //공격준비
    Death,
    Guard, //경계
    Chase, //추적
    Scout, //정찰
    Shoot, //사격중
    Return, //귀환중
    Noise, //소리감지
}

public class Enemy : SoundReceiver, IDamagable, Receiveable
{
    public NavMeshAgent agent;
    public Animator animator;
    public EnemyState enemyState = EnemyState.None;
    public GameObject target;
    public GameObject curTarget;
    public Transform eyeTrans;
    public Transform gunTrans;
    public List<GameObject> location = new List<GameObject>();
    public GameObject curLocation = null;
    //public Location curLocation;
    //public Location[] scoutLocation;
    private float viewAngle = 60.0f;
    private float viewDistance = 10.0f;
    private float attackDistance = 5.0f;
    private float audioDistance = 10.0f;
    private bool isAim = false;
    private bool isAttack = false;
    private bool isShoot = false;
    private bool isDead = false;
    private Vector3 startPos;
    private Quaternion startAngle;
    public EnemyState startState;
    private float lastFindTime = 0.0f;
    private float lostDelayTime = 1.0f;
    private float lastReturnTime = 0.0f;
    private float returnDelayTime = 3.0f;
    private float lastScoutTime = 0.0f;
    private float scoutDelayTime = 5.0f;
    public float hp = 1.0f;
    private float speed = 3.5f;
    public GameObject blood;
    public GameObject muzzle;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        startPos = transform.position;
        startAngle = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            agent.destination = target.transform.position;
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
        //Gizmos.color = Color.red;
        //Vector3 vec = eyeTrans.position + ((target.transform.position - eyeTrans.position).normalized * viewDistance);
        //Gizmos.DrawLine(eyeTrans.position, vec);
    }

    private void CheckEnemy()
    {
        if (target == null)
            return;
        if (enemyState == EnemyState.Death)
            return;

        if(Time.time >= lastFindTime + lostDelayTime)
        {
            curTarget = null;
        }

        RaycastHit hit;        
        Vector3 vec = eyeTrans.position + ((target.transform.position - eyeTrans.position).normalized * viewDistance);
        if (Physics.Linecast(eyeTrans.position, vec, out hit, LayerMask.GetMask("Player", "Wall", "Ground")))
        {
            if(hit.collider != null)
            {
                Transform objTrans = hit.transform;
                if (objTrans.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    Vector3 objPos = objTrans.position;
                    objPos.y = 0;
                    Vector3 pos = transform.position;
                    pos.y = 0;
                    Vector3 normal = (objPos - pos).normalized;
                    float tempAngle = (Mathf.Atan2(normal.z, normal.x) * Mathf.Rad2Deg) - 90;
                    tempAngle += transform.rotation.eulerAngles.y;
                    Debug.Log("탐색가능");
                    Debug.Log(tempAngle);
                    if (Mathf.Cos(tempAngle * Mathf.Deg2Rad) >= Mathf.Cos(viewAngle * Mathf.Deg2Rad))
                    {
                        lastFindTime = Time.time;
                        curTarget = objTrans.gameObject;
                        if (enemyState == EnemyState.None || enemyState == EnemyState.Guard || 
                            enemyState == EnemyState.Scout || enemyState == EnemyState.Return || 
                            enemyState == EnemyState.Noise)
                            enemyState = EnemyState.Chase;
                    }
                }
            }
        }
    }

    private void State()
    {
        if(enemyState == EnemyState.None)
        {
            animator.SetInteger("State", (int)EnemyState.None);
            agent.speed = 1.2f;
            //가만히 경계
            if (isAim == true)
            {
                isAim = false;
            }
        }
        else if(enemyState == EnemyState.Stun)
        {
            animator.SetInteger("State", (int)EnemyState.None);
            agent.speed = 0.1f;
            //행동 불능
        }
        else if(enemyState == EnemyState.Attack)
        {
            animator.SetInteger("State", (int)EnemyState.Attack);
            //목표 방향으로 공격(RayCast)
            if (isAim == false)
            {
                isAim = true;
            }
            if (curTarget == null)
            {
                animator.SetBool("isAttack", false);
                enemyState = EnemyState.Guard;
            }
            else
            {
                if (Vector3.Distance(transform.position, target.transform.position) < attackDistance + 0.5f)
                {
                    animator.SetBool("isAttack", true);
                }
                else
                {
                    animator.SetBool("isAttack", false);
                    enemyState = EnemyState.Chase;
                }
            }
        }
        else if (enemyState == EnemyState.Return)
        {
            animator.SetInteger("State", (int)EnemyState.Return);
            agent.speed = 1.2f;
            if (isAim == true)
            {
                isAim = false;
            }
            if (Vector3.Distance(transform.position, agent.destination) < 1.0f)
            {
                enemyState = startState;
                agent.SetDestination(transform.position);
                transform.rotation = startAngle;
            }
        }
        else if(enemyState == EnemyState.Guard)
        {
            agent.speed = 1.2f;
            animator.SetInteger("State", (int)EnemyState.None);
            //주변 경계
            if(lastReturnTime < Time.time - returnDelayTime - 3.0f)
            {
                lastReturnTime = Time.time;
            }
            if(Time.time > lastReturnTime + returnDelayTime)
            {
                enemyState = EnemyState.Return;
                agent.SetDestination(startPos);
            }
        }
        else if (enemyState == EnemyState.Chase)
        {
            animator.SetInteger("State", (int)EnemyState.None);
            agent.speed = 4.0f;
            //목표의 위치로 공격 태세 이동
            if (isAim == true)
            {
                isAim = false;
            }
            if(curTarget == null)
            {
                if(Vector3.Distance(transform.position, agent.destination) < 1.0f)
                {
                    agent.SetDestination(transform.position);
                    enemyState = EnemyState.Guard;
                }
            }
            else
            {
                if(Vector3.Distance(transform.position, target.transform.position) < attackDistance)
                {
                    agent.SetDestination(transform.position);
                    enemyState = EnemyState.Attack;
                }
                else
                {
                    agent.SetDestination(curTarget.transform.position);
                }
            }
            
        }
        else if (enemyState == EnemyState.Scout)
        {
            agent.speed = 1.2f;
            animator.SetInteger("State", (int)EnemyState.None);
            if (isAim == true)
            {
                isAim = false;
            }
            //순찰 위치를 반복 이동
            if (Time.time > lastScoutTime + scoutDelayTime)
            {
                if (Vector3.Distance(transform.position, location[0].transform.position) < 1.0f)
                {
                    curLocation = location[0];
                    location.Add(location[0]);
                    location.RemoveAt(0);
                    lastScoutTime = Time.time;
                }
                else
                {
                    agent.SetDestination(location[0].transform.position);
                }
            }
        }
        else if(enemyState == EnemyState.Noise)
        {
            //소리감지
            agent.speed = 2.5f;
            if (Vector3.Distance(transform.position, agent.destination) < 0.5f)
            {
                agent.SetDestination(transform.position);
                enemyState = EnemyState.Guard;
            }
        }
        else if(enemyState == EnemyState.Shoot)
        {
            animator.SetInteger("State", (int)EnemyState.Shoot);
            //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0.0f, 0, 0.0f), 1.0f);
            Vector3 vec = target.transform.position;
            vec.y = transform.position.y;
            transform.LookAt(vec);
            //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler((target.transform.position - transform.position).normalized), 0.1f);
        }
        else if (enemyState == EnemyState.Death)
        {
            animator.SetInteger("State", (int)EnemyState.None);
            if (isAim == true)
            {
                isAim = false;
            }
            agent.speed = 0.01f;
            agent.SetDestination(this.transform.position);
        }
    }

    private void AnimationSet()
    {
        if (isShoot == true)
            return;
        Vector3 vec = agent.velocity;
        vec.y = 0;
        animator.SetFloat("Speed", vec.magnitude);
        animator.SetBool("isAim", isAim);
    }
    public void ShootStart()
    {
        if (enemyState == EnemyState.Death)
            return;
        agent.SetDestination(transform.position);
        enemyState = EnemyState.Shoot;
    }

    public void ShootEnd()
    {
        if (enemyState == EnemyState.Death)
            return;
        enemyState = EnemyState.Attack;
    }

    public void Shoot()
    {
        if (enemyState == EnemyState.Death)
            return;
        //GameObject muzzleObj = Instantiate(muzzle, gunTrans.position, gunTrans.rotation);
        GameObject muzzleObj = Instantiate(muzzle, gunTrans);
        muzzle.GetComponent<ParticleSystem>().Play();
        Destroy(muzzleObj, 2.0f);
        SoundManager.Instance.PlaySound(this.transform.position, "GunFire3");
        RaycastHit hit;        
        if (Physics.Linecast(eyeTrans.position, target.transform.position, out hit, LayerMask.GetMask("Player", "Unit", "Wall", "Ground")))
        {
            IDamagable d = hit.transform.GetComponent<IDamagable>();
            if(d != null)
            {
                if(d?.Damaged(5) == true)
                {
                    GameObject obj = Instantiate(blood, hit.point, Quaternion.LookRotation(hit.point - gunTrans.position));
                    Destroy(obj, 3.0f);
                }
                
            }
        }
    }

    public bool Damaged(float damage)
    {
        this.hp += -damage;
        if(this.isDead == false)
        {
            if (this.hp <= 0)
            {
                animator.SetTrigger("Death");
                animator.Play("Death");
                this.isDead = true;
                isAim = false;
                isAttack = false;
                isShoot = false;
                this.GetComponent<Collider>().enabled = false;
                this.enemyState = EnemyState.Death;
            }
            return true;
        }

        return false;
    }

    public void ReceiveAction(Vector3 position)
    {
        //소리 들었음.
        if (curTarget != null)
            return;
        if (enemyState == EnemyState.Death)
            return;
        if (enemyState == EnemyState.Stun)
            return;
        if (enemyState == EnemyState.Attack)
            return;
        if (enemyState == EnemyState.Shoot)
            return;
        agent.SetDestination(position);
        enemyState = EnemyState.Noise;
    }

    public void FootStep(float intensity)
    {
        SoundManager.Instance.PlaySound(transform.position, "SwatFootStep", 1.0f, false);
    }
}

