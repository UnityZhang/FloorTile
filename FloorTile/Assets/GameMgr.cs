using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr : SingletonMonoBehaviour<GameMgr>
{
    public Brush brush;
    public Transform CreateMeshTran;
    public float LerpRange = 0.02f;
    float Width = 0.2f;
    public int Segments = 8;//分割数 
    public float Radius = 0.3f;    //半径  

    Vector3[] vertices;
    Mesh mesh;
    MeshFilter Filter;
    
    Vector3 lastpos = Vector3.zero;
    List<Vector3> point = new List<Vector3>();

    int m_State = 1;


    protected override void Awake()
    {
        base.Awake();
        Filter = CreateMeshTran.GetComponent<MeshFilter>();
    }
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (m_State == 1)
            {
                RaycastHit hitinfo;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hitinfo))
                {
                    Debug.DrawLine(ray.origin, hitinfo.point);
                    if (lastpos == Vector3.zero)
                    {
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
                            GenerateCircleMesh();
                        }
                    }

                }
            }
            else if (m_State == 2)
            {
              
            }

        }
        if (Input.GetMouseButtonUp(0))
        {
            if (m_State == 1)
                lastpos = Vector3.zero;
            else if (m_State == 2)
            {

            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            m_State++;
        }
        if (m_State == 2)
        {
            CheckMeshDeformation();
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
                    vertices[point.Count * j] = RotateAround(point0, pos, Vector3.up, -angle);
                    //vertices[point.Count * j] = pos;

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
                    vertices[i + (point.Count * j)] = RotateAround(pointi, pos, Vector3.up, -angle);
                    //vertices[i + (point.Count * j)] = pos;

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
                    vertices[i + (point.Count * j)] = RotateAround(pointi, pos, Vector3.up, -angle);
                    //vertices[i + (point.Count * j)] = pos;

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
        }
        return nor;
    }

   
    public void CheckMeshDeformation()
    {
        //pos = point;
        //return;
        //范围是x幂函数在z轴变宽 y = (3x)^2 * 0.6
        //根据x得到y值
        bool isChange = false;
        float x = brush.Center.position.x;
        float z = brush.Center.position.z;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 pos = vertices[i];
            //判断x范围
            if (pos.x <= x +(brush.xWidth * 0.5f) && pos.x >= x - (brush.xWidth * 0.5f))
            {
                //判断z范围
                if (pos.z >= z - (brush.zWidth * 0.5f) && pos.z <= z + (brush.zWidth * 0.5f))
                {
                    //如果y高度大于函数图像的y值，就向下移
                    float parmaX = pos.x - x;
                    float resultY = Mathf.Pow(brush.RatioX * parmaX, 2) * brush.RatioHeight + brush.Center.position.y;
                    if (pos.y > resultY)
                    {
                        isChange = true;
                        vertices[i].y = resultY - 0.001f;
                    }
                }
            }
        }
        if (isChange)
        {
            Filter.mesh.SetVertices(vertices);
            Filter.mesh.RecalculateNormals();
        }
    }

    Vector3 pos = Vector3.zero;


    private void OnDrawGizmos()
    {
        //return;
        Gizmos.color = Color.white;
        //float xUnit = 0.01f;
        //for (int i = -17; i <= 17; i++)
        //{
        //    Gizmos.DrawSphere(brush.Center.position + new Vector3(xUnit * i, Mathf.Pow((xUnit * i * brush.RatioX), 2) * brush.RatioHeight, 0), 0.01f);
        //}
        Vector3 vector = new Vector3(1, 0, 1);
        Gizmos.DrawSphere(vector, 0.05f);
        Gizmos.DrawSphere(RotateAround(Vector3.zero, vector, Vector3.up, -10), 0.05f);
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
}
