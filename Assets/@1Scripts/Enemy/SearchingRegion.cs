using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Enemy의 시야각 표기
/*
 * 원리
 * 1. 시야각도만큼 다수의 Raycast 사용.
 * 2. Raycast가 장애물 충돌시 해당 위치를 저장
 * 3. Raycast를 통해 얻은 좌표를 연결하여 Mesh 생성
 */
public class SearchingRegion : MonoBehaviour
{
    [Header("Status")]
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;
    public float offRadius;
    [Range(0.1f, 1)]
    public float meshResolution;
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [Header("Internal Object")]
    public Transform eyeTrans;
    public MeshFilter filter;
    Mesh mesh;

    [Header("Extern Object")]
    public Transform Player;
    public List<Transform> visibleTargets = new List<Transform>();
    public Material greenMaterial;
    public Material orangeMaterial;
    public Material redMaterial;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "View Mesh";
        filter.mesh = mesh; 
        //StartCoroutine(CreateMesh());
    }

    void LateUpdate()
    {
        CreateMesh2();
    }

    //연산량 감소를 위한 Coroutine code
    private IEnumerator CreateMesh()
    {
        while(true)
        {
            int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
            float stepAngleSize = viewAngle / stepCount;
            List<Vector3> viewPoint = new List<Vector3>();
            for (int i = 0; i < stepCount; i++)
            {
                float angle = eyeTrans.eulerAngles.y + offRadius - viewAngle / 2 + stepAngleSize * i;
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
                vertices[i + 1] = transform.InverseTransformPoint(viewPoint[i]);//viewPoint[i];
                if(i < vertexCount-2)
                {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            //mesh.RecalculateBounds();
            yield return new WaitForSeconds(0.02f);
        }
    }

    public void ClearMesh()
    {
        if (mesh == null)
            return;
        mesh.Clear();
    }

    //LateUpdate에서 처리.
    private void CreateMesh2()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoint = new List<Vector3>();
        for (int i = 0; i < stepCount; i++)
        {
            float angle = eyeTrans.eulerAngles.y + offRadius - viewAngle / 2 + stepAngleSize * i;
            //Debug.DrawLine(transform.position, transform.position + DirFromAngle(angle,true) * viewRadius, Color.red);
            //정점좌표 구하기
            ViewCastInfo newViewCast = ViewCast(angle);
            viewPoint.Add(newViewCast.position);
        }

        int vertexCount = viewPoint.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];
        vertices[0] = new Vector3(0, 0, 0);
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoint[i]);//viewPoint[i];
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        //mesh.RecalculateBounds();
    }

    //해당 각도에 Raycast를 발사하여 최종 좌표 반환
    public ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;
        if (Physics.Raycast(eyeTrans.position, dir, out hit, viewRadius, LayerMask.GetMask("Wall")))
        {
            Vector3 vec = hit.point;
            vec.y = transform.position.y + 0.1f;
            return new ViewCastInfo(true, vec, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius + new Vector3(0,0.1f,0), viewRadius, globalAngle);
        }
    }

    //각도를 벡터로 변환
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    //좌표 데이터
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


    // Raycast 발사 도중 적을 발견했을 경우 적 발견하는 방법. 사용하지 않고 Enemy에서 구현됨.
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

    //표시 색상 변경
    public void ChangeColor(Color color)
    {
        filter.gameObject.GetComponent<Renderer>().material.color = color;
    }
}