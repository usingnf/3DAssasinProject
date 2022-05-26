using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    public float hp = 1.0f;
    private float lastAttackTime = 0.0f;
    private float attackDelayTime = 1.0f;
    private float lastTurnTime = 0.0f;
    private float turnDelayTime = 3.0f;
    private float damage = 50.0f;
    private bool isDetected = false;
    public List<GameObject> location = new List<GameObject>();
    public GameObject curLocation = null;

    [Header("Internal Object")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform gunTrans;
    public Transform bodyTrans;
    public Transform eyeTrans;
    public Transform headTrans;
    public GameObject minimapPos;
    public SearchingRegion searchingRegion;

    [Header("Extern Object")]
    public GameObject target;
    public GameObject curTarget;
    public GameObject blood;
    public GameObject muzzle;
    public GameObject lightning;
    
    void Start()
    {
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



    private void CheckEnemy()
    {
        if (target == null)
            return;
        if (turretState == TurretState.Death)
            return;

        if (Time.time >= lastFindTime + lostDelayTime)
        {
            curTarget = null;
        }

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
                    //Debug.Log(eyeTrans.rotation.eulerAngles.y);
                    tempAngle += eyeTrans.rotation.eulerAngles.y - 90;
                    if (Mathf.Cos(tempAngle * Mathf.Deg2Rad) >= Mathf.Cos(viewAngle * Mathf.Deg2Rad))
                    {
                        Debug.Log("find");
                        lastFindTime = Time.time;
                        if(turretState == TurretState.Attack)
                        {
                            curTarget = target;
                            lastDetectPos = curTarget.transform.position;
                            turretState = TurretState.Attack;
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
                                    turretState = TurretState.Attack;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void TestMode()
    {
        if(turretState == TurretState.Death)
            return;
        if (Input.GetKeyDown(KeyCode.T))
        {
            searchingRegion.enabled = !searchingRegion.enabled;
            if (searchingRegion.enabled == false)
            {
                searchingRegion.ClearMesh();
            }
        }
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

    public bool Damaged(float damage)
    {
        this.hp += -damage;
        if (this.isDead == false)
        {
            SoundManager.Instance.PlaySound(transform.position, "TurretDamaged", 1.0f);
            if (this.hp <= 0)
            {
                animator.SetTrigger("Death");
                //animator.Play("Death");
                this.isDead = true;
                searchingRegion.enabled = false;
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
