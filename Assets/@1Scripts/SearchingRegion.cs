using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchingRegion : MonoBehaviour
{
    //private Patrol patrol;

    public Transform Player;
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [Range (0,1)]
    public float meshResolution;
    public MeshFilter filter;
    Mesh mesh;

    public List<Transform> visibleTargets = new List<Transform>();
    void Awake()
    {
        //patrol = GetComponent<Patrol>();
    }

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "View Mesh";
        //filter.mesh = mesh; 
        //StartCoroutine("FindTargetsWithDelay", .2f);
        StartCoroutine(CreateMesh());
    }

    void Update()
    {
        
        
    }

    private IEnumerator CreateMesh()
    {
        while(true)
        {
            int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
            float stepAngleSize = viewAngle / stepCount;
            List<Vector3> viewPoint = new List<Vector3>();
            for (int i = 0; i < stepCount; i++)
            {
                float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
                //Debug.DrawLine(transform.position, transform.position + DirFromAngle(angle,true) * viewRadius, Color.red);
                //정점좌표 구하기
                ViewCastInfo newViewCast = ViewCast(angle);
                viewPoint.Add(newViewCast.position);
            }

            int vertexCount = viewPoint.Count + 1;
            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[(vertexCount-2) * 3];
            vertices[0] = new Vector3(0, 0, 0);
            for(int i = 0; i < vertexCount-1; i++)
            {
                vertices[i+1] = viewPoint[i];
                if(i < vertexCount-2)
                {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                }
            }
            yield return new WaitForSeconds(0.02f);
        }
        yield return null;
    }

    public ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, LayerMask.GetMask("Wall")))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 position;
        public float dst;
        public float angle;

        public ViewCastInfo(bool hit, Vector3 position, float dst, float angle)
        {
            this.hit = hit;
            this.position = position;
            this.dst = dst;
            this.angle = angle;
        }

    }


    // not use
    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }
    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }
}

