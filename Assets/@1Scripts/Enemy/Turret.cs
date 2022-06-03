using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Turret은 상태 패턴을 통해 ai가 구현됨.
//Turret의 상태
public enum TurretState
{
    None,
    Guard,
    Scout,
    Attack,
    Death,
    Return,
}

public class Turret : MonoBehaviour, IDamagable, IViewMinimap
{
    [Header("Status")]
    public TurretStatus turretData;
    private float viewAngle = 60.0f;
    private float viewDistance = 15.0f;
    private bool isDead = false;
    private Vector3 startPos;
    private Vector3 lastDetectPos;
    private Quaternion startAngle;
    public TurretState startState;
    public TurretState turretState = TurretState.None;
    private float lastFindTime = 0.0f;
    private float lostDelayTime = 1.0f;
    [SerializeField]
    private float hp = 1.0f;
    private float lastAttackTime = 0.0f;
    private float attackDelayTime = 1.0f;
    private float lastTurnTime = 0.0f;
    private float turnDelayTime = 3.0f;
    private float damage = 50.0f;
    public List<GameObject> location = new List<GameObject>();
    public GameObject curLocation = null;

    [Header("Internal Object")]
    [SerializeField]
    private NavMeshAgent agent;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Transform gunTrans;
    [SerializeField]
    private Transform bodyTrans;
    [SerializeField]
    private Transform eyeTrans;
    [SerializeField]
    private Transform headTrans;
    [SerializeField]
    private GameObject minimapPos;
    [SerializeField]
    private SearchingRegion searchingRegion;
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
    private GameObject lightning;
    
    void Start()
    {
        //Scriptable Object를 통해 스텟 기본 값 설정.
        viewAngle = turretData.viewAngle;
        viewDistance = turretData.viewDistance;
        lostDelayTime = turretData.lostDelayTime;
        hp = turretData.hp;
        damage = turretData.damage;
        attackDelayTime = turretData.attackDelayTime;
        turnDelayTime = turretData.turnDelayTime;

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        startPos = transform.position;
        startAngle = transform.rotation;
        searchingRegion.viewAngle = viewAngle * 2;
        searchingRegion.viewRadius = viewDistance;
    }

    void Update()
    {
        State();
        CheckEnemy();
        TestMode();
    }

    //Animation의 각도 설정값을 무시하기 위해 LateUpdate에서 각도 강제 설정.
    void LateUpdate()
    {
        if (turretState == TurretState.Attack)
        {
            Vector3 vec = (target.transform.position - bodyTrans.position);
            vec.y = vec.y - 1.0f;
            Quaternion q = Quaternion.LookRotation(vec);
            q *= Quaternion.Euler(0, 90, 0);
            bodyTrans.rotation = Quaternion.Slerp(bodyTrans.rotation, q, 1.0f);
        }
        else if(turretState == TurretState.Scout)
        {
            Vector3 vec = (curLocation.transform.position - headTrans.position).normalized;
            vec.y = 0;
            Quaternion q = Quaternion.LookRotation(vec);
            //headTrans.rotation = Quaternion.Lerp(headTrans.rotation, q, 1.0f);
            q *= Quaternion.Euler(0, 90, 0);
            headTrans.rotation = Quaternion.Slerp(headTrans.rotation, q, 2 * Time.deltaTime);
        }
    }

    //상태 패턴
    private void State()
    {
        if(turretState == TurretState.None)
        {

        }
        else if(turretState == TurretState.Attack)
        {
            if(curTarget != null)
            {
                
                
                if (lastAttackTime + attackDelayTime < Time.time)
                {
                    lastAttackTime = Time.time;
                    animator.SetTrigger("Attack");
                }
            }
            else
            {
                if(lastFindTime + lostDelayTime < Time.time)
                {
                    searchingRegion.ChangeColor(new Color(0, 1, 0));
                    turretState = TurretState.Return;
                }
            }
            
        }
        else if (turretState == TurretState.Scout)
        {
            if(lastTurnTime + turnDelayTime < Time.time)
            {
                lastTurnTime = Time.time;
                curLocation = location[0];
                location.Add(location[0]);
                location.RemoveAt(0);
            }
            else
            {
                
            }
        }
        else if (turretState == TurretState.Death)
        {
            //x
        }
        else if(turretState == TurretState.Return)
        {
            bodyTrans.rotation = Quaternion.Slerp(bodyTrans.rotation, startAngle, 1.0f);
            if(Quaternion.Angle(bodyTrans.rotation, startAngle) < 0.1f)
            {
                turretState = startState;
            }
        }
    }

    //Animation Event에서 사용됨.
    private void Shoot()
    {
        GameObject muzzleObj = Instantiate(muzzle, gunTrans);
        muzzle.GetComponent<ParticleSystem>().Play();
        Destroy(muzzleObj, 2.0f);
        SoundManager.Instance.PlaySound(this.transform.position, "GunFire3", 1.0f, true, 13.0f, 0.1f);
        if (Physics.Linecast(gunTrans.position, target.transform.position, out RaycastHit hit, LayerMask.GetMask("Player", "Wall", "Ground")))
        {
            IDamagable d = hit.transform.GetComponent<IDamagable>();
            if (d != null)
            {
                if (d?.Damaged(damage) == true)
                {
                    GameObject obj = Instantiate(blood, hit.point, Quaternion.LookRotation(hit.point - gunTrans.position));
                    Destroy(obj, 3.0f);
                }
            }
        }
    }

    //Raycast와 시야각을 통한 적 탐지.
    private void CheckEnemy()
    {
        if (target == null)
            return;
        if (turretState == TurretState.Death)
            return;

        //일정 시간 이상 적을 놓칠 경우 탐지 실패.
        if (Time.time >= lastFindTime + lostDelayTime)
        {
            curTarget = null;
        }

        //Raycast와 시야각도를 계산.
        RaycastHit hit;
        Vector3 rayVec = eyeTrans.position + ((target.transform.position - eyeTrans.position).normalized * viewDistance);
        if (Physics.Linecast(eyeTrans.position, rayVec, out hit, LayerMask.GetMask("Player", "Wall", "Ground", "Door", "ClimbWall")))
        {
            if (hit.collider != null)
            {
                Transform objTrans = hit.transform;
                if (objTrans.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    Vector3 objPos = objTrans.position;
                    objPos.y = 0;
                    Vector3 pos = eyeTrans.position;
                    pos.y = 0;
                    Vector3 normal = (objPos - pos).normalized;
                    float tempAngle = (Mathf.Atan2(normal.z, normal.x) * Mathf.Rad2Deg) - 90;
                    tempAngle += eyeTrans.rotation.eulerAngles.y - 90;
                    if (Mathf.Cos(tempAngle * Mathf.Deg2Rad) >= Mathf.Cos(viewAngle * Mathf.Deg2Rad))
                    {
                        lastFindTime = Time.time;
                        if(turretState == TurretState.Attack)
                        {
                            if(curTarget == null)
                            {
                                searchingRegion.ChangeColor(new Color(1, 0, 0));
                                MessageManager.Instance.CreateMessage($"{gameObject.name}에게 들킴");
                            }
                            curTarget = target;
                            lastDetectPos = curTarget.transform.position;
                        }
                        else
                        {
                            Player player = hit.collider.GetComponent<Player>();
                            if (player != null)
                            {
                                if (player.isInvisible == true)
                                {
                                    //은신일 경우 탐지 실패. 발견된 상태일 경우 은신 무시.
                                }
                                else
                                {
                                    if (curTarget == null)
                                    {
                                        searchingRegion.ChangeColor(new Color(1, 0, 0));
                                        MessageManager.Instance.CreateMessage($"{gameObject.name}에게 들킴");
                                    }
                                    curTarget = target;
                                    lastDetectPos = curTarget.transform.position;
                                    turretState = TurretState.Attack;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    //시야각 표기
    private void TestMode()
    {
        if(turretState == TurretState.Death)
            return;
        if (Input.GetKeyDown(key.T))
        {
            searchingRegion.enabled = !searchingRegion.enabled;
            if (searchingRegion.enabled == false)
            {
                searchingRegion.ClearMesh();
            }
        }
    }

    //특정 상황에서 일정 시간동안 미니맵에 표기
    public IEnumerator ViewMinimapLoop(float time)
    {
        minimapPos.SetActive(true);
        yield return new WaitForSeconds(time);
        minimapPos.SetActive(false);
        yield return null;
    }

    public void ViewMinimap(float time)
    {
        StartCoroutine(ViewMinimapLoop(time));
    }

    //데미지 및 사망여부 판단
    public bool Damaged(float damage)
    {
        this.hp += -damage;
        if (this.isDead == false)
        {
            SoundManager.Instance.PlaySound(transform.position, "TurretDamaged", 1.0f);
            if (this.hp <= 0)
            {
                animator.SetTrigger("Death");
                this.isDead = true;
                searchingRegion.enabled = false;
                searchingRegion.ClearMesh();
                MessageManager.Instance.CreateMessage($"{gameObject.name}가 파괴됨");
                this.GetComponent<Collider>().enabled = false;
                this.turretState = TurretState.Death;
                GameObject obj = Instantiate(lightning, bodyTrans.position, transform.rotation * Quaternion.Euler(0,0,0));
                Destroy(obj, 10.0f);
                SoundManager.Instance.PlaySound(transform.position, "TurretDie", 1.0f, true, 1.0f, 0.1f);
            }
            return true;
        }
        return false;
    }
}
