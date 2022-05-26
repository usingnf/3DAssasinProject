using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Enemy2 : SoundReceiver, IDamagable, Receiveable, IViewMinimap
{
    [Header("Status")]
    public SwatStatus swatData;
    private float viewAngle = 60.0f;
    private float viewDistance = 15.0f;
    private float attackDistance = 7.0f;
    private float audioDistance = 13.0f;
    private bool isAim = false;
    private bool isAttack = false;
    private bool isShoot = false;
    private bool isDead = false;
    private Vector3 startPos;
    private Vector3 lastDetectPos;
    private Quaternion startAngle;
    public EnemyState startState;
    public EnemyState enemyState = EnemyState.None;
    private float lastFindTime = 0.0f;
    private float lostDelayTime = 1.0f;
    private float lastReturnTime = 0.0f;
    private float returnDelayTime = 3.0f;
    private float lastScoutTime = 0.0f;
    private float scoutDelayTime = 5.0f;
    public float hp = 1.0f;
    private float speed = 1.2f;
    private float damage = 50.0f;
    private bool isDetected = false;
    public List<GameObject> location = new List<GameObject>();
    public GameObject curLocation = null;

    [Header("Internal Object")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform eyeTrans;
    public Transform gunTrans;
    public Transform bodyTrans;
    public GameObject minimapPos;
    public SearchingRegion searchingRegion;

    [Header("Extern Object")]
    public GameObject target;
    public GameObject curTarget;
    public GameObject blood;
    public GameObject muzzle;

    void Start()
    {
        viewAngle = swatData.viewAngle;
        viewDistance = swatData.viewDistance;
        attackDistance = swatData.audioDistance;
        audioDistance = swatData.audioDistance;
        lostDelayTime = swatData.lostDelayTime;
        returnDelayTime = swatData.returnDelayTime;
        scoutDelayTime = swatData.scoutDelayTime;
        hp = swatData.hp;
        speed = swatData.speed;
        damage = swatData.damage;

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        startPos = transform.position;
        startAngle = transform.rotation;
        this.GetComponent<SphereCollider>().radius = audioDistance;
        searchingRegion.viewAngle = viewAngle * 2;
        searchingRegion.viewRadius = viewDistance;
    }

    void Update()
    {
        State();
        AnimationSet();
        CheckEnemy();
        TestMode();
    }
    void LateUpdate()
    {
        //Set character view angle
        if(enemyState == EnemyState.Shoot || enemyState == EnemyState.Attack)
        {
            Vector3 vec = (target.transform.position - bodyTrans.position);
            float tempy = vec.y;
            vec.y = transform.position.y;
            Quaternion q2 = Quaternion.LookRotation(vec);
            vec.y = tempy - 0.5f;
            Quaternion q = Quaternion.LookRotation(vec);            
            transform.rotation = Quaternion.Slerp(transform.rotation, q2, 1.0f);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
            bodyTrans.rotation = Quaternion.Slerp(transform.rotation, q, 1.0f);
        }
    }

    private void TestMode()
    {
        if (enemyState == EnemyState.Death)
            return;
        if (Input.GetKeyDown(KeyCode.T))
        {
            searchingRegion.enabled = !searchingRegion.enabled;
            if(searchingRegion.enabled == false)
            {
                searchingRegion.ClearMesh();
            }
        }
    }

    private void State()
    {
        animator.SetInteger("State", (int)enemyState);
        if (enemyState == EnemyState.None)
        {
            //가만히 경계
            agent.speed = speed;
            if (isAim == true)
            {
                isAim = false;
            }
        }
        else if (enemyState == EnemyState.Stun)
        {
            //행동 불능
            agent.speed = speed;
            if (isAim == true)
            {
                isAim = false;
            }
        }
        else if (enemyState == EnemyState.Attack)
        {
            //목표 방향으로 공격(RayCast)
            minimapPos.SetActive(true);
            if (isAim == false)
            {
                isAim = true;
            }
            if (curTarget == null)
            {
                animator.SetBool("isAttack", false);
                if (Vector3.Distance(transform.position, lastDetectPos) > 1.0f)
                {
                    agent.SetDestination(lastDetectPos);
                    StartCoroutine(isPath(enemyState));
                }
                else
                {
                    agent.SetDestination(transform.position);
                    enemyState = EnemyState.Guard;
                }
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
            //처음 위치로 귀환
            agent.speed = speed;
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
        else if (enemyState == EnemyState.Guard)
        {
            //주변 경계
            if(isDetected == false)
                minimapPos.SetActive(false);
            agent.speed = speed;
            if (lastReturnTime < Time.time - returnDelayTime - 3.0f)
            {
                lastReturnTime = Time.time;
            }
            if (Time.time > lastReturnTime + returnDelayTime)
            {
                enemyState = EnemyState.Return;
                agent.SetDestination(startPos);
            }
        }
        else if (enemyState == EnemyState.Chase)
        {
            //목표의 위치로 공격 태세 이동
            agent.speed = speed * 3 + 0.4f;
            if (isAim == true)
            {
                isAim = false;
            }
            if (curTarget == null)
            {
                if (Vector3.Distance(transform.position, agent.destination) < 1.0f)
                {
                    agent.SetDestination(transform.position);
                    enemyState = EnemyState.Guard;
                }
            }
            else
            {
                if (Vector3.Distance(transform.position, target.transform.position) < attackDistance)
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
            //지정 장소 순회
            agent.speed = speed;
            
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
        else if (enemyState == EnemyState.Noise)
        {
            //소리감지
            agent.speed = speed * 2;
            if (Vector3.Distance(transform.position, agent.destination) < 0.5f)
            {
                agent.SetDestination(transform.position);
                enemyState = EnemyState.Guard;
            }
        }
        else if (enemyState == EnemyState.Shoot)
        {
            //Animation, LateUpdate에서 처리
        }
        else if (enemyState == EnemyState.Death)
        {
            if (isAim == true)
            {
                isAim = false;
            }
            agent.speed = 0.01f;
            agent.SetDestination(this.transform.position);
        }
    }
    private IEnumerator isPath(EnemyState state)
    {
        EnemyState temp = state;
        yield return new WaitForSeconds(0.1f);
        if (temp == enemyState)
        {
            if (agent.hasPath == false)
            {
                enemyState = EnemyState.Return;
            }
        }
        yield return null;
    }
    private void AnimationSet()
    {
        if (isShoot == true)
            return;
        Vector3 speedVec = agent.velocity;
        speedVec.y = 0;
        animator.SetFloat("Speed", speedVec.magnitude);
        animator.SetBool("isAim", isAim);
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
        Vector3 rayVec = eyeTrans.position + ((target.transform.position - eyeTrans.position).normalized * viewDistance);
        if (Physics.Linecast(eyeTrans.position, rayVec, out hit, LayerMask.GetMask("Player", "Wall", "Ground", "Door", "ClimbWall")))
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
                    if (Mathf.Cos(tempAngle * Mathf.Deg2Rad) >= Mathf.Cos(viewAngle * Mathf.Deg2Rad))
                    {
                        lastFindTime = Time.time;
                        //curTarget = objTrans.gameObject;
                        if(enemyState == EnemyState.Noise || enemyState == EnemyState.Attack ||
                            enemyState == EnemyState.Chase || enemyState == EnemyState.Shoot)
                        {
                            curTarget = target;
                            lastDetectPos = curTarget.transform.position;
                            if (enemyState == EnemyState.None || enemyState == EnemyState.Guard ||
                                enemyState == EnemyState.Scout || enemyState == EnemyState.Return ||
                                enemyState == EnemyState.Noise)
                                enemyState = EnemyState.Chase;
                        }
                        else
                        {
                            Player player = hit.collider.GetComponent<Player>();
                            if (player != null)
                            {
                                if (player.isInvisible == true)
                                {

                                }
                                else
                                {
                                    curTarget = target;
                                    lastDetectPos = curTarget.transform.position;
                                    if (enemyState == EnemyState.None || enemyState == EnemyState.Guard ||
                                        enemyState == EnemyState.Scout || enemyState == EnemyState.Return ||
                                        enemyState == EnemyState.Noise)
                                        enemyState = EnemyState.Chase;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    //Used at animation event
    public void ShootStart()
    {
        if (enemyState == EnemyState.Death)
            return;
        agent.SetDestination(transform.position);
        enemyState = EnemyState.Shoot;
    }
    //Used at animation event
    public void ShootEnd()
    {
        if (enemyState == EnemyState.Death)
            return;
        enemyState = EnemyState.Attack;
    }
    //Used at animation event
    public void Shoot()
    {
        if (enemyState == EnemyState.Death)
            return;

        GameObject muzzleObj = Instantiate(muzzle, gunTrans);
        muzzle.GetComponent<ParticleSystem>().Play();
        Destroy(muzzleObj, 2.0f);
        SoundManager.Instance.PlaySound(this.transform.position, "GunFire3", 1.0f, true, 13.0f, 0.1f);
        if (Physics.Linecast(eyeTrans.position, target.transform.position, out RaycastHit hit, LayerMask.GetMask("Player", "Wall", "Ground")))
        {
            IDamagable d = hit.transform.GetComponent<IDamagable>();
            if(d != null)
            {
                if(d?.Damaged(damage) == true)
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
                searchingRegion.enabled = false;
                this.GetComponent<Collider>().enabled = false;
                this.enemyState = EnemyState.Death;
                SoundManager.Instance.PlaySound(transform.position, "EnemyDeath", 1.0f, true, 1.0f, 0.1f);
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
    //Used at animation event
    public void FootStep(float intensity)
    {
        SoundManager.Instance.PlaySound(transform.position, "SwatFootStep", 1.0f, false);
    }

    public void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;
        if(obj.layer == LayerMask.NameToLayer("Knife"))
        {
            StartCoroutine(KnifeDetect(obj));
        }
    }

    public IEnumerator KnifeDetect(GameObject obj)
    {
        while(obj != null)
        {
            if (target == null)
                yield break;
            if (enemyState == EnemyState.Death)
                yield break;

            RaycastHit hit;
            Vector3 rayVec = eyeTrans.position + ((obj.transform.position - eyeTrans.position).normalized * viewDistance);
            if (Physics.Linecast(eyeTrans.position, rayVec, out hit, LayerMask.GetMask("Knife", "Wall", "Ground", "Door", "ClimbWall")))
            {
                if (hit.collider != null)
                {
                    Transform objTrans = hit.transform;
                    if (objTrans.gameObject.layer == LayerMask.NameToLayer("Knife"))
                    {
                        Vector3 objPos = objTrans.position;
                        objPos.y = 0;
                        Vector3 pos = transform.position;
                        pos.y = 0;
                        Vector3 normal = (objPos - pos).normalized;
                        float tempAngle = (Mathf.Atan2(normal.z, normal.x) * Mathf.Rad2Deg) - 90;
                        tempAngle += transform.rotation.eulerAngles.y;
                        if (Mathf.Cos(tempAngle * Mathf.Deg2Rad) >= Mathf.Cos(225.0f * Mathf.Deg2Rad))
                        {
                            lastFindTime = Time.time;
                            //curTarget = objTrans.gameObject;
                            lastDetectPos = objTrans.position;
                            agent.SetDestination(lastDetectPos);
                            if (enemyState == EnemyState.None || enemyState == EnemyState.Guard ||
                                enemyState == EnemyState.Scout || enemyState == EnemyState.Return ||
                                enemyState == EnemyState.Noise)
                                enemyState = EnemyState.Chase;
                        }
                    }
                }
            }
            yield return new WaitForSeconds(0.04f);
        }
        yield return null;
    }

    public IEnumerator ViewMinimapLoop(float time)
    {
        isDetected = true;
        minimapPos.SetActive(true);
        yield return new WaitForSeconds(time);
        minimapPos.SetActive(false);
        isDetected = false;
        yield return null;
    }

    public void ViewMinimap(float time)
    {
        StartCoroutine(ViewMinimapLoop(time));
    }
}

