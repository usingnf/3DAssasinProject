using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

//Enemy는 상태패턴을 통해 ai가 구현됨.
//Enemy의 상태 종류
public enum EnemyState
{
    None, //일반
    Stun, //스턴
    Attack, //공격준비
    Death, //사망
    Guard, //경계
    Chase, //추적
    Scout, //정찰
    Shoot, //사격중
    Return, //귀환중
    Noise, //소리감지
}
public class Enemy : SoundReceiver, IDamagable, Receivable, IViewMinimap
{
    [Header("Status")]
    public SwatStatus swatData;
    private float viewAngle = 60.0f;
    private float viewDistance = 15.0f;
    private float attackDistance = 7.0f;
    private float audioDistance = 13.0f;
    private bool isAim = false;
    private bool isShoot = false;
    private bool isDead = false;
    private Vector3 startPos;
    private Vector3 lastDetectPos;
    private Quaternion startAngle;
    [SerializeField]
    private EnemyState startState;
    [SerializeField]
    private EnemyState enemyState = EnemyState.None;
    private float lastFindTime = 0.0f;
    private float lostDelayTime = 1.0f;
    private float lastReturnTime = 0.0f;
    private float returnDelayTime = 3.0f;
    private float lastScoutTime = 0.0f;
    private float scoutDelayTime = 5.0f;
    [SerializeField]
    private float hp = 1.0f;
    private float speed = 1.2f;
    private float damage = 50.0f;
    private bool isDetected = false;
    public List<GameObject> location = new List<GameObject>();
    [SerializeField]
    private GameObject curLocation = null;
    private List<GameObject> tracePoint = new List<GameObject>();

    [Header("Internal Object")]
    [SerializeField]
    private NavMeshAgent agent;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Transform eyeTrans;
    [SerializeField]
    private Transform gunTrans;
    [SerializeField]
    private Transform bodyTrans;
    [SerializeField]
    private GameObject minimapPos;
    [SerializeField]
    private SearchingRegion searchingRegion;
    [SerializeField]
    private Transform overHead;
    private GameObject emotion;
    [SerializeField]
    private Key key;

    [Header("Extern Object")]
    public GameObject target;
    public GameObject curTarget;
    [SerializeField]
    private GameObject blood;
    [SerializeField]
    private GameObject muzzle;
    [SerializeField]
    private GameObject bloodPool;

    void Start()
    {
        //Scriptable Object를 통해 스텟 기본 값 설정.
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
        ViewField();
    }

    //Animation의 통한 각도 설정값을 무시하기 위해 LateUpdate에서 상체의 각도를 강제 설정.
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

    
    //바닥에 Enemy의 시야각을 표기.
    private void ViewField()
    {
        if (enemyState == EnemyState.Death)
            return;
        if (Input.GetKeyDown(key.T))
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
            //행동 불능, 구현 하지 않음.
            agent.speed = speed;
            if (isAim == true)
            {
                isAim = false;
            }
        }
        else if (enemyState == EnemyState.Attack)
        {
            //목표 방향으로 공격(RayCast을 통해 즉시 공격)
            minimapPos.SetActive(true);
            if (isAim == false)
            {
                isAim = true;
            }
            if (curTarget == null)
            {
                //목표를 놓쳤을 경우 마지막 목격 지점까지 이동
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
                //공격 거리내에 있을 경우 공격. 아닐 경우 추격.
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
            //처음 위치(상태)로 귀환
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
            //전방 경계
            if(isDetected == false)
                minimapPos.SetActive(false);
            agent.speed = speed;
            if (lastReturnTime < Time.time - returnDelayTime - 3.0f)
            {
                lastReturnTime = Time.time;
            }
            if (Time.time > lastReturnTime + returnDelayTime)
            {
                if (emotion != null)
                    Destroy(emotion);
                searchingRegion.ChangeColor(new Color(0, 1, 0));
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
            if (curTarget == null) // 적을 놓친 상태
            {
                if (Vector3.Distance(transform.position, agent.destination) < 1.0f) // 목표 지점 도착
                {
                    agent.SetDestination(transform.position);
                    enemyState = EnemyState.Guard; // 경계 태세
                }
            }
            else
            {
                //공격 사정거리내 있을 경우 추격 중지후 공격
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
            //Animation Event, LateUpdate에서 처리
        }
        else if (enemyState == EnemyState.Death)
        {
            if (isAim == true)
            {
                isAim = false;
            }
            agent.speed = 0.01f;
            agent.SetDestination(this.transform.position);
            this.enabled = false;
        }
    }

    //경로가 없을 경우 귀환
    private IEnumerator isPath(EnemyState state)
    {
        EnemyState temp = state;
        yield return new WaitForSeconds(0.1f);
        if (temp == enemyState)
        {
            if (agent.hasPath == false)
            {
                if (emotion != null)
                    Destroy(emotion);
                searchingRegion.ChangeColor(new Color(0, 1, 0));
                enemyState = EnemyState.Return;
            }
        }
        yield return null;
    }

    //애니메이션에 속도값과 조준 여부를 전달
    private void AnimationSet()
    {
        if (isShoot == true)
            return;
        Vector3 speedVec = agent.velocity;
        speedVec.y = 0;
        animator.SetFloat("Speed", speedVec.magnitude);
        animator.SetBool("isAim", isAim);
    }

    //실시간으로 적의 위치를 탐색(Raycast와 시야각도로 판정)
    private void CheckEnemy()
    {
        if (target == null)
            return;
        if (enemyState == EnemyState.Death)
            return;

        //일정시간 이상 적을 놓칠 경우 추격 실패
        if(Time.time >= lastFindTime + lostDelayTime)
        {
            curTarget = null;
        }

        //플레이어 방향으로 Raycast를 사용 및 장애물 여부 판단
        RaycastHit hit;        
        Vector3 rayVec = eyeTrans.position + ((target.transform.position - eyeTrans.position).normalized * viewDistance);
        if (Physics.Linecast(eyeTrans.position, rayVec, out hit, 
            LayerMask.GetMask("Player", "Wall", "Ground", "Door", "ClimbWall")))
        {
            if(hit.collider != null)
            {
                Transform objTrans = hit.transform;
                if (objTrans.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    //좌우 시야각만 판단
                    Vector3 objPos = objTrans.position;
                    objPos.y = 0;
                    Vector3 pos = transform.position;
                    pos.y = 0;
                    Vector3 normal = (objPos - pos).normalized;
                    float tempAngle = (Mathf.Atan2(normal.z, normal.x) * Mathf.Rad2Deg) - 90;
                    tempAngle += transform.rotation.eulerAngles.y; // 일반적으로 빼야하지만, 각도계를 반대로써서 더해줌.
                    if (Mathf.Cos(tempAngle * Mathf.Deg2Rad) >= Mathf.Cos(viewAngle * Mathf.Deg2Rad))
                    {
                        //은신 상태여도 발견된 상태면 숨지 못함.
                        lastFindTime = Time.time;
                        if(enemyState == EnemyState.Noise || enemyState == EnemyState.Attack ||
                            enemyState == EnemyState.Chase || enemyState == EnemyState.Shoot)
                        {
                            if(curTarget == null)
                                MessageManager.Instance.CreateMessage($"{gameObject.name}에게 발견됨");
                            curTarget = target;
                            lastDetectPos = curTarget.transform.position;
                            if (enemyState == EnemyState.None || enemyState == EnemyState.Guard ||
                                enemyState == EnemyState.Scout || enemyState == EnemyState.Return ||
                                enemyState == EnemyState.Noise)
                            {
                                if (enemyState != EnemyState.Chase)
                                {
                                    searchingRegion.ChangeColor(new Color(1, 0, 0));
                                    CreateEmotion("!", 2.0f, new Color(1, 0, 0));
                                }
                                enemyState = EnemyState.Chase;
                            }
                        }
                        else
                        {
                            Player player = hit.collider.GetComponent<Player>();
                            if (player != null)
                            {
                                if (player.isInvisible == true)
                                {
                                    //은신일 경우 발견 실패. 은신상태여도 발견된 상태면 숨지 못함.
                                }
                                else
                                {
                                    if (curTarget == null)
                                        MessageManager.Instance.CreateMessage($"{gameObject.name}에게 발견됨");
                                    curTarget = target;
                                    lastDetectPos = curTarget.transform.position;
                                    if (enemyState == EnemyState.None || enemyState == EnemyState.Guard ||
                                        enemyState == EnemyState.Scout || enemyState == EnemyState.Return ||
                                        enemyState == EnemyState.Noise)
                                    {
                                        if(enemyState != EnemyState.Chase)
                                        {
                                            searchingRegion.ChangeColor(new Color(1, 0, 0));
                                            CreateEmotion("!", 2.0f, new Color(1, 0, 0));
                                        }
                                        enemyState = EnemyState.Chase;
                                    }
                                        
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    //Animation Event에서 사용중.
    public void ShootStart()
    {
        if (enemyState == EnemyState.Death)
            return;
        if (emotion != null)
            Destroy(emotion);
        agent.SetDestination(transform.position);
        enemyState = EnemyState.Shoot;
    }
    //Animation Event에서 사용중.
    public void ShootEnd()
    {
        if (enemyState == EnemyState.Death)
            return;
        enemyState = EnemyState.Attack;
    }
    //Animation Event에서 사용중.
    public void Shoot()
    {
        if (enemyState == EnemyState.Death)
            return;
        // 총 타격 이펙트
        GameObject muzzleObj = Instantiate(muzzle, gunTrans);
        muzzle.GetComponent<ParticleSystem>().Play();
        Destroy(muzzleObj, 2.0f);
        SoundManager.Instance.PlaySound(this.transform.position, "GunFire3", 1.0f, true, 13.0f, 0.1f, 0.5f);
        if (Physics.Linecast(eyeTrans.position, target.transform.position, out RaycastHit hit, LayerMask.GetMask("Player", "Wall", "Ground")))
        {
            // 공격 가능한 대상인지 판독
            IDamagable d = hit.transform.GetComponent<IDamagable>();
            if(d != null)
            {
                if(d?.Damaged(damage) == true) // 데미지를 실제로 받았는지 판단
                {
                    // 출혈 이펙트
                    GameObject obj = Instantiate(blood, hit.point, Quaternion.LookRotation(hit.point - gunTrans.position));
                    Destroy(obj, 3.0f);
                }
            }
        }
    }

    //데미지 및 사망여부 판단
    public bool Damaged(float damage)
    {
        this.hp += -damage;
        if(this.isDead == false)
        {
            if (emotion != null)
                Destroy(emotion);
            MessageManager.Instance.CreateMessage($"{gameObject.name}가 {damage} 만큼 데미지를 받음");
            if (this.hp <= 0)
            {
                animator.SetTrigger("Death");
                animator.Play("Death");
                this.isDead = true;
                isAim = false;
                isShoot = false;
                searchingRegion.enabled = false;
                searchingRegion.ClearMesh();
                minimapPos.SetActive(false);
                this.GetComponent<Collider>().enabled = false;
                this.enemyState = EnemyState.Death;
                SoundManager.Instance.PlaySound(transform.position, "EnemyDeath", 1.0f, true, 1.0f, 0.1f);
            }
            return true;
        }

        return false;
    }

    //일반 상태에서 주변에서 소음을 들었을 경우 해당 위치 탐색.
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
        if(enemyState != EnemyState.Noise && enemyState != EnemyState.Chase)
        {
            if (curTarget == null)
            {
                searchingRegion.ChangeColor(new Color(1, 0.7f, 0));
                MessageManager.Instance.CreateMessage($"{gameObject.name}가 소리를 들음");
                CreateEmotion("?", 3.0f, new Color(1, 1, 0));
            }
        }
        enemyState = EnemyState.Noise;
    }
    //Animation Event에서 사용중.
    public void FootStep(float intensity)
    {
        SoundManager.Instance.PlaySound(transform.position, "SwatFootStep", 1.0f, false);
    }

    //적의 오브젝트(나이프, 피, 발자국)를 탐지했을 경우 추적.
    public void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;
        if(obj.layer == LayerMask.NameToLayer("Knife"))
        {
            StartCoroutine(KnifeDetect(obj));
        }
        if (obj.CompareTag("Blood") || obj.CompareTag("FootPrint"))
        {
            if(tracePoint.Contains(obj) == false)
            {
                //발자국 및 피자국은 객체당 한번만 추적
                tracePoint.Add(obj);
                StartCoroutine(CheckTracePoint(obj));
            }
        }
    }

    //발자국 및 피자국은 객체당 한번만 추적
    public void OnTriggerExit(Collider other)
    {
        GameObject obj = other.gameObject;
        if (obj.CompareTag("Blood") || obj.CompareTag("FootPrint"))
        {
            if (tracePoint.Contains(obj) == true)
            {
                tracePoint.Remove(obj);
            }
        }
    }

    //Raycast와 충돌을 활용한 발자국 추적
    private IEnumerator CheckTracePoint(GameObject obj)
    {
        while(obj != null)
        {
            if (target == null)
                yield break;
            if (enemyState == EnemyState.Attack)
                yield break;
            if (enemyState == EnemyState.Shoot)
                yield break;
            if (enemyState == EnemyState.Death)
                yield break;

            RaycastHit hit;
            Vector3 rayVec = eyeTrans.position + ((obj.transform.position - eyeTrans.position).normalized * viewDistance);
            if (Physics.Linecast(eyeTrans.position, rayVec, out hit, LayerMask.GetMask("Environment", "Wall", "Ground", "Door", "ClimbWall")))
            {
                if (hit.collider != null)
                {
                    Transform objTrans = hit.transform;
                    if (objTrans.CompareTag("Blood") || objTrans.CompareTag("FootPrint"))
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
                            lastDetectPos = objTrans.position;
                            agent.SetDestination(lastDetectPos);
                            if (enemyState == EnemyState.None || enemyState == EnemyState.Guard ||
                                enemyState == EnemyState.Scout || enemyState == EnemyState.Return ||
                                enemyState == EnemyState.Noise)
                            {
                                enemyState = EnemyState.Chase;
                                CreateEmotion("!", 2.0f, new Color(1, 0, 0));
                                break;
                            }
                                
                        }
                    }
                }
            }
            yield return new WaitForSeconds(0.04f);
        }

        yield return null;
    }

    //Raycast와 충돌을 통한 나이프 추적
    private IEnumerator KnifeDetect(GameObject obj)
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
                        if (Mathf.Cos(tempAngle * Mathf.Deg2Rad) >= Mathf.Cos(75.0f * Mathf.Deg2Rad))
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

    //일정 시간동안 미니맵에 자신의 위치를 표기
    private IEnumerator ViewMinimapLoop(float time)
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

    //사망시 피웅덩이 생성
    private void CreateBloodPool(float time)
    {
        Vector3 vec = bodyTrans.position;
        vec.y = transform.position.y;
        GameObject obj = Instantiate(bloodPool, vec, Quaternion.identity);
        Destroy(obj, time);
    }

    //감정 표현
    private void CreateEmotion(string text, float time, Color color)
    {
        if(emotion != null)
            Destroy(emotion);
        emotion = Instantiate(Resources.Load<GameObject>("Prefab/3DText"), overHead.position, overHead.rotation, overHead);
        emotion.GetComponent<TextMesh>().text = text;
        emotion.GetComponent<Renderer>().material.color = color;
        Destroy(emotion, time);
    }
}

