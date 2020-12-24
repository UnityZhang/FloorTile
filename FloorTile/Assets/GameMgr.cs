using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr : MonoBehaviour
{
    public GameObject Brush;
    public float LerpRange = 0.02f;

    List<GameObject> m_Brushs = new List<GameObject>();


    List<Vector3> point = new List<Vector3>();

    float Width = 0.2f;
    public int Segments = 8;//分割数 
    public float Radius = 0.3f;    //半径  

    Vector3[] vertices;
    Mesh mesh;
    MeshFilter Filter;
    MeshCollider meshCollider;
    public Transform CreateMeshTran;


    private void Awake()
    {
        Filter = CreateMeshTran.GetComponent<MeshFilter>();
        meshCollider = CreateMeshTran.GetComponent<MeshCollider>();
    }
    void Start()
    {
        
    }

    Vector3 lastpos = Vector3.zero;
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hitinfo;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hitinfo))
            {
                Debug.DrawLine(ray.origin, hitinfo.point);
                if (lastpos == Vector3.zero)
                {
                    //CreateBrush(hitinfo.point);
                    point.Add(hitinfo.point);
                    lastpos = hitinfo.point;
                    GenerateCircleMesh();
                }
                else
                {
                    //Debug.Log(Vector3.Distance(hitinfo.point, lastpos));
                    if (lastpos.z - hitinfo.point.z > LerpRange)
                    {
                        point.Add(hitinfo.point);
                        lastpos = hitinfo.point;
                        //Vector3 unitdir = (hitinfo.point - lastpos).normalized * LerpRange;
                        //bool OutRange = false;
                        //int i = 0;
                        //while (!OutRange)
                        //{
                        //    Vector3 pos = unitdir * i + lastpos;
                        //    CreateBrush(pos);
                        //    if (Vector3.Distance(pos, lastpos) > Vector3.Distance(hitinfo.point, lastpos))
                        //        OutRange = true;
                        //    i++;
                        //}
                        //CreateBrush(hitinfo.point);
                        GenerateCircleMesh();
                    }
                }
               
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            lastpos = Vector3.zero;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Filter.mesh = CreateMesh(new Vector3(0.1f,0.2f,0.5f), Radius, Segments);
            //Debug.Log(Vector3.Angle(Vector3.right, new Vector3(1, 0, 0.5f)));
        }

    }

    void GenerateMesh()
    {
        if (point.Count <= 1)
            return;
        Filter.mesh = mesh = new Mesh();
        mesh.name = "Grid";
        vertices = new Vector3[point.Count * (Segments + 1)];
        //添加顶点
        for (int i = 1; i < point.Count; i++)
        {
            //法线
            Vector3 nor1 = GetNormalRightSide(point[i - 1] - point[i]);
            //第一个点
            if (i - 1 == 0)
            {
                vertices[0] = point[0];
                //vertices[point.Count] = point[0] + (nor1 * Width);
                //平均八等份
                float unit = Width / Segments;
                for (int j = 1; j <= Segments; j++)
                    vertices[point.Count * j] = point[0] + (nor1 * unit * j);
            }
            if (i + 1 < point.Count)
            {
                //第二个点 根据第三个点计算平均法线
                Vector3 nor3 = GetNormalRightSide(point[i + 1] - point[i]);
                Vector3 nor2 = (nor1 + nor3) * 0.5f;
                //vertices[i + point.Count] = point[i] + (nor2 * Width);
                //平均八等份
                float unit = Width / Segments;
                for (int j = 1; j <= Segments; j++)
                    vertices[i + (point.Count * j)] = point[i] + (nor2 * unit * j);
            }
            if (i == point.Count - 1)//最后一个点
            {
                //vertices[i + point.Count] = point[i] + (nor1 * Width);
                //平均八等份
                float unit = Width / Segments;
                for (int j = 1; j <= Segments; j++)
                    vertices[i + (point.Count * j)] = point[i] + (nor1 * unit * j);
            }
            vertices[i] = point[i];
        }
        mesh.vertices = vertices;

        //添加三角面
        int[] triangles = new int[(point.Count - 1) * Segments * 6];
        for (int ti = 0, vi = 0, y = 0; y < Segments; y++, vi++)
        {
            for (int x = 0; x < point.Count - 1; x++, ti+= 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + point.Count;
                triangles[ti + 5] = vi + point.Count + 1;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void GenerateCircleMesh()
    {
        if (point.Count <= 1)
            return;
        Filter.mesh = mesh = new Mesh();
        mesh.name = "Grid";
        vertices = new Vector3[point.Count * (Segments + 1)];
        //添加顶点
        for (int i = 1; i < point.Count; i++)
        {
            //法线
            Vector3 nor1 = GetNormalRightSide(point[i - 1] - point[i]);

            float angledegree = 360f;
            float angleRad = Mathf.Deg2Rad * angledegree;
            float angleCur;
            float angledelta = angleRad / Segments;

            //第一个点
            if (i - 1 == 0)
            {
                Vector3 point0 = point[0];
                
                angleCur = angleRad;
                float angle = Vector3.Angle(Vector3.right, GetNormal(point[i] - point[i - 1]));
                for (int j = 1; j <= Segments; j++)
                {
                    float cosA = Mathf.Cos(angleCur);
                    float sinA = Mathf.Sin(angleCur);

                    Vector3 pos = new Vector3(Radius * cosA + point0.x, Radius * sinA + point0.y, point0.z);
                    //vertices[point.Count * j] = RotateAround(point0, pos, Vector3.up, -angle);
                    vertices[point.Count * j] = pos;

                    if (j == Segments)//第一个也是最后一个
                        vertices[0] = vertices[point.Count * j];

                    angleCur -= angledelta;
                }

            }
            Vector3 pointi = point[i];
            if (i + 1 < point.Count)
            {
                //第二个点 根据第三个点计算平均法线
                Vector3 nor3 = GetNormalRightSide(point[i + 1] - pointi);
                Vector3 nor2 = (nor1 + nor3) * 0.5f;

                angleCur = angleRad;
                float angle = Vector3.Angle(Vector3.right, GetNormal(point[i + 1] - pointi));
                for (int j = 1; j <= Segments; j++)
                {
                    float cosA = Mathf.Cos(angleCur);
                    float sinA = Mathf.Sin(angleCur);

                    Vector3 pos = new Vector3(Radius * cosA + pointi.x, Radius * sinA + pointi.y, pointi.z);
                    //vertices[i + (point.Count * j)] = RotateAround(pointi, pos, Vector3.up, -angle);
                    vertices[i + (point.Count * j)] = pos;

                    if (j == Segments)//第一个也是最后一个
                        vertices[i] = vertices[i + (point.Count * j)];

                    angleCur -= angledelta;
                }
            }
            if (i == point.Count - 1)//最后一个点
            {
                angleCur = angleRad;
                float angle = Vector3.Angle(Vector3.right, GetNormal(point[i] - point[i - 1]));
                for (int j = 1; j <= Segments; j++)
                {
                    float cosA = Mathf.Cos(angleCur);
                    float sinA = Mathf.Sin(angleCur);

                    Vector3 pos = new Vector3(Radius * cosA + pointi.x, Radius * sinA + pointi.y, pointi.z);
                    //vertices[i + (point.Count * j)] = RotateAround(pointi, pos, Vector3.up, -angle);
                    vertices[i + (point.Count * j)] = pos;

                    if (j == Segments)//第一个也是最后一个
                        vertices[i] = vertices[i + (point.Count * j)];

                    angleCur -= angledelta;
                }
            }
        }
        mesh.vertices = vertices;

        //添加三角面
        int[] triangles = new int[(point.Count - 1) * Segments * 6];
        for (int ti = 0, vi = 0, y = 0; y < Segments; y++, vi++)
        {
            for (int x = 0; x < point.Count - 1; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + point.Count;
                triangles[ti + 5] = vi + point.Count + 1;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = mesh;
    }
    Vector3 GetNormalRightSide(Vector3 dir)
    {
        Vector3 nor;
        if (dir.x == 0)
        {
            nor = new Vector3(1, dir.y, 0);
        }
        else
        {
            nor = new Vector3(-dir.z / dir.x, dir.y, 1).normalized;
            if (nor.x < 0)
                nor = -nor;
        }
        return nor;
    }
    Vector3 GetNormal(Vector3 dir)
    {
        Vector3 nor;
        if (dir.x == 0)
        {
            nor = new Vector3(1, dir.y, 0);
        }
        else
        {
            nor = new Vector3(-dir.z / dir.x, dir.y, 1).normalized;
            //if (nor.x < 0)
            //    nor = -nor;
        }
        return nor;
    }

    GameObject CreateBrush(Vector3 pos)
    {
        GameObject obj = Instantiate(Brush, transform);
        obj.transform.position = pos;
        m_Brushs.Add(obj);
        return obj;
    }

    
    Mesh CreateMesh(Vector3 center, float radius, int segments)
    {
        //vertices:
        int vertices_count = Segments + 1;
        Vector3[] vertices = new Vector3[vertices_count];
        vertices[0] = center;
        float angledegree = 360.0f;
        float angleRad = Mathf.Deg2Rad * angledegree;
        float angleCur = angleRad;
        float angledelta = angleRad / Segments;
        for (int i = 1; i < vertices_count; i++)
        {
            float cosA = Mathf.Cos(angleCur);
            float sinA = Mathf.Sin(angleCur);

            Vector3 pos = new Vector3(Radius * cosA + center.x, Radius * sinA + center.y, center.z);
            vertices[i] = Quaternion.AngleAxis(30, center) * pos;
            
            angleCur -= angledelta;
        }

        //triangles
        int triangle_count = segments * 3;
        int[] triangles = new int[triangle_count];
        for (int i = 0, vi = 1; i <= triangle_count - 1; i += 3, vi++)     //因为该案例分割了60个三角形，故最后一个索引顺序应该是：0 60 1；所以需要单独处理
        {
            triangles[i] = 0;
            triangles[i + 1] = vi;
            triangles[i + 2] = vi + 1;
        }
        triangles[triangle_count - 3] = 0;
        triangles[triangle_count - 2] = vertices_count - 1;
        triangles[triangle_count - 1] = 1;                  //为了完成闭环，将最后一个三角形单独拎出来

        //uv:
        Vector2[] uvs = new Vector2[vertices_count];
        for (int i = 0; i < vertices_count; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / radius / 2 + 0.5f, vertices[i].z / radius / 2 + 0.5f);
        }

        //负载属性与mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        return mesh;
    }

    private void OnDrawGizmos()
    {
        return;
        Gizmos.color = Color.white;
        if (point.Count >= 2)
        {
            //for (int i = 1; i < point.Count; i++)
            //{
            //    Gizmos.DrawLine(point[i - 1], point[i]);
            //    //中点
            //    //Vector3 center = (point[i - 1] + point[i]) * 0.5f;
            //    //Gizmos.DrawSphere(center, 0.05f);
            //    //法线
            //    Vector3 nor1 = GetNormal(point[i - 1] - point[i]);
            //    //第一个点
            //    if (i - 1 == 0)
            //        Gizmos.DrawLine(point[i - 1], point[i - 1] + (nor1 * Width));
            //    if (i + 1 < point.Count)
            //    {
            //        //第三个点
            //        Vector3 nor3 = GetNormal(point[i + 1] - point[i]);
            //        Vector3 nor2 = (nor1 + nor3) * 0.5f;
            //        Gizmos.DrawLine(point[i], point[i] + (nor2 * Width));
            //    }

            //}

            for (int i = 1; i < point.Count; i++)
            {
                ////法线
                //Vector3 nor1 = GetNormal(point[i - 1] - point[i]);
                ////第一个点
                //if (i - 1 == 0)
                //{
                //    Gizmos.DrawSphere(point[0], 0.05f);
                //    //Gizmos.DrawSphere(point[i - 1] + (nor1 * Width), 0.05f);

                //    Vector3 point0 = point[0];

                //    float angledegree = 360f;
                //    float angleRad = Mathf.Deg2Rad * angledegree;
                //    float angleCur = angleRad;
                //    float angledelta = angleRad / Segments;
                //    float angle = Vector3.Angle(Vector3.right, nor1);
                //    for (int j = 1; j <= Segments; j++)
                //    {
                //        float cosA = Mathf.Cos(angleCur);
                //        float sinA = Mathf.Sin(angleCur);

                //        Vector3 pos = new Vector3(Radius * cosA + point0.x, Radius * sinA + point0.y, point0.z);
                //        //vertices[point.Count * j] = Quaternion.AngleAxis(angle, Vector3.up) * pos;
                //        angleCur -= angledelta;
                //        Gizmos.DrawSphere(Quaternion.AngleAxis(angle, Vector3.up) * pos, 0.05f);
                //    }
                //}
                //if (i + 1 < point.Count)
                //{
                //    //第三个点
                //    Vector3 nor3 = GetNormal(point[i + 1] - point[i]);
                //    Vector3 nor2 = (nor1 + nor3) * 0.5f;
                //    Gizmos.DrawSphere(point[i] + (nor2 * Width), 0.05f);
                //}
                //if (i == point.Count - 1)//最后一个点
                //{
                //    Gizmos.DrawSphere(point[i] + (nor1 * Width), 0.05f);
                //}
                Gizmos.DrawSphere(point[i], 0.05f);

                //法线
                Vector3 nor1 = GetNormalRightSide(point[i - 1] - point[i]);

                float angledegree = 360f;
                float angleRad = Mathf.Deg2Rad * angledegree;
                float angleCur;
                float angledelta = angleRad / Segments;

                //第一个点
                if (i - 1 == 0)
                {
                    Vector3 point0 = point[0];

                    angleCur = angleRad;
                    float angle = Vector3.Angle(Vector3.right, GetNormal(point[i] - point[i - 1]));
                    for (int j = 1; j <= Segments; j++)
                    {
                        float cosA = Mathf.Cos(angleCur);
                        float sinA = Mathf.Sin(angleCur);

                        Vector3 pos = new Vector3(Radius * cosA + point0.x, Radius * sinA + point0.y, point0.z);
                        Gizmos.DrawSphere(pos, 0.03f);
                        Vector3 p = RotateAround(point0, pos, Vector3.up, -angle);
                        Gizmos.DrawSphere(p, 0.05f);

                        angleCur -= angledelta;
                    }

                }
                Vector3 pointi = point[i];
                if (i + 1 < point.Count)
                {
                    //第二个点 根据第三个点计算平均法线
                    Vector3 nor3 = GetNormalRightSide(point[i + 1] - pointi);
                    Vector3 nor2 = (nor1 + nor3) * 0.5f;

                    angleCur = angleRad;
                    float angle = Vector3.Angle(Vector3.right, nor2);
                    for (int j = 1; j <= Segments; j++)
                    {
                        float cosA = Mathf.Cos(angleCur);
                        float sinA = Mathf.Sin(angleCur);

                        Vector3 pos = new Vector3(Radius * cosA + pointi.x, Radius * sinA + pointi.y, pointi.z);
                        //Gizmos.DrawSphere(Quaternion.AngleAxis(angle, pointi) * pos, 0.05f);
                        //Gizmos.DrawSphere(pos, 0.05f);

                        angleCur -= angledelta;
                    }
                }
                if (i == point.Count - 1)//最后一个点
                {
                    angleCur = angleRad;
                    float angle = Vector3.Angle(Vector3.right, nor1);
                    for (int j = 1; j <= Segments; j++)
                    {
                        float cosA = Mathf.Cos(angleCur);
                        float sinA = Mathf.Sin(angleCur);

                        Vector3 pos = new Vector3(Radius * cosA + pointi.x, Radius * sinA + pointi.y, pointi.z);
                        //Gizmos.DrawSphere(Quaternion.AngleAxis(angle, pointi) * pos, 0.05f);
                        //Gizmos.DrawSphere(pos, 0.05f);

                        angleCur -= angledelta;
                    }
                }
            }
        }
    }

    Vector3 RotateAround(Vector3 center, Vector3 Pos, Vector3 axis, float angle)
    {
        //绕axis轴旋转angle角度
        Quaternion rotation = Quaternion.AngleAxis(angle, axis);
        //旋转之前,以center为起点,transform.position当前物体位置为终点的向量.
        //四元数 * 向量(不能调换位置, 否则发生编译错误)
        Vector3 afterVector = rotation * (Pos - center);//旋转后的向量
        //向量的终点 = 向量的起点 + 向量
        return afterVector + center;
    }



    //[ContextMenu("合并Mesh")]
    //void combineMesh()
    //{
    //    MeshFilter[] meshfilter = GetComponentsInChildren<MeshFilter>();//获取所有子物体的mesh fileter
    //    CombineInstance[] combineinstance = new CombineInstance[meshfilter.Length];//创建一个combineInstance 用于存储合并的

    //    MeshRenderer[] meshrender = GetComponentsInChildren<MeshRenderer>();//获取所有子辈的mesh render
    //    Material[] mats = new Material[meshrender.Length]; //并创建材质球


    //    for (int i = 0; i < meshfilter.Length; i++)
    //    {
    //        mats[i] = meshrender[i].material;

    //        combineinstance[i].mesh = meshfilter[i].sharedMesh;//将网格信息复制给conbineinstance
    //        combineinstance[i].transform = meshfilter[i].transform.localToWorldMatrix;//位置信息也是一样赋值给combineinstance

    //        if (meshfilter[i].gameObject.name != gameObject.name)
    //        {
    //            Destroy(meshfilter[i].gameObject);
    //        }
    //        // meshfilter[i].gameObject.SetActive(false);
    //    }

    //    //  transform.GetComponent<MeshFilter>().mesh = new Mesh();//获取自身的
    //    transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combineinstance, true);//这里第二个参数要设置为true 否则如果材质不同，物体都会使用所挂脚本物体的材质
    //    transform.GetComponent<MeshRenderer>().sharedMaterials = mats;//把合并后的材质球赋值给meshrender
    //    //transform.gameObject.SetActive(true);
    //}

}
