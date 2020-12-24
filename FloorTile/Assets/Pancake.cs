using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pancake : MonoBehaviour
{
    public MeshFilter meshFilter;

    private Vector3[] originPos;
    private Vector3[] deformationPos;
    /// <summary>
    /// 顶点最大高度   世界空间下
    /// </summary>
    public float m_VertexMaxHeightWorldSpace = .5f;
    /// <summary>
    /// 顶点最大高度，   模型空间下   由m_VertexMaxHeightWorldSpace转化而来
    /// </summary>
    private float m_VertexMaxHeightObjectSpacePos = 0;
    /// <summary>
    /// 顶点与圆心的最大变形距离
    /// </summary>
    public float m_VertexAndCenterPosDeformationDis = 1.0f;
    /// <summary>
    /// 点击位置变形的最大半径
    /// </summary>
    public float m_DeformationMaxRadiu = .5f;
    /// <summary>
    /// 每帧移动的距离
    /// </summary>
    public float m_PerFrameMoveDis = .1f;

    private MeshCollider meshCollider;
    /// <summary>
    /// 初始顶点距离中心点的最大距离
    /// </summary>
    private float m_VertexMaxDis = 0f;
    
    /// <summary>
    /// 模型空间下  中心点
    /// </summary>
    private Vector3 m_MeshObjectSpaceCenterPos = Vector3.zero;

    private void Start()
    {
        deformationPos = meshFilter.mesh.vertices;

        originPos = meshFilter.mesh.vertices;

        m_MeshObjectSpaceCenterPos = meshFilter.transform.InverseTransformPoint(meshFilter.transform.position);

        m_VertexMaxHeightObjectSpacePos = meshFilter.transform.InverseTransformPoint(new Vector3(0, m_VertexMaxHeightWorldSpace, 0)).y;

        m_MeshObjectSpaceCenterPos.y = m_VertexMaxHeightObjectSpacePos;

        for (int i = 0; i < originPos.Length; i++)
        {
            Vector3 pos = originPos[i];

            if (pos.y >= m_VertexMaxHeightObjectSpacePos)
            {
                pos.y = m_MeshObjectSpaceCenterPos.y;

                originPos[i] = pos;

                float dis = Vector3.Distance(pos, m_MeshObjectSpaceCenterPos);

                if (m_VertexMaxDis < dis)
                {
                    m_VertexMaxDis = dis;
                }
            }
        }

        meshCollider = meshFilter.GetComponent<MeshCollider>();

    }

    public List<CheckCircle> m_CheckCircleList = new List<CheckCircle>();
    private bool CheckIsCircle()
    {
        for (int i = 0; i < m_CheckCircleList.Count; i++)
        {
            if (m_CheckCircleList[i].isCollision == false) {
                return false;
            }
        }
        return true;
    }

    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                Deformation(hitInfo.point);
            }
        }

        if (Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                //DrawTex(hitInfo);
            }
        }

        if (Input.GetMouseButtonUp(1)) {
            bool isCircle = CheckIsCircle();

            if (isCircle) {
                Debug.LogError("完成");
            }
        }
    }
    private void Deformation(Vector3 clickPos)
    {
        clickPos = meshFilter.transform.InverseTransformPoint(clickPos);

        //每单位变形的距离
        float deformationDisPerUnit = m_VertexAndCenterPosDeformationDis / m_VertexMaxDis;

        for (int i = 0; i < deformationPos.Length; i++)
        {
            Vector3 vertexPos = deformationPos[i];

            clickPos.y = vertexPos.y;

            //顶点与点击位置的距离
            float vertexAndClickPosDis = Vector3.Distance(vertexPos, clickPos);

            if (vertexAndClickPosDis <= m_DeformationMaxRadiu)
            {
                if (vertexPos.y >= m_VertexMaxHeightObjectSpacePos)
                {
                    vertexPos.y = m_VertexMaxHeightObjectSpacePos;

                    //顶点与模型中心点的距离
                    float vertexAndCenterPosDis = Vector3.Distance(vertexPos, m_MeshObjectSpaceCenterPos);

                    float originDis = Vector3.Distance(originPos[i], m_MeshObjectSpaceCenterPos);

                    float maxDeformationDis = originDis * deformationDisPerUnit;

                    if (vertexAndCenterPosDis < maxDeformationDis)
                    {
                        Vector3 dir = (vertexPos - m_MeshObjectSpaceCenterPos).normalized;

                        dir.y = 0;

                        vertexPos += dir * m_PerFrameMoveDis;
                    }
                }

                deformationPos[i] = vertexPos;
            }
        }

        meshFilter.mesh.SetVertices(deformationPos);
        meshFilter.mesh.RecalculateNormals();

        meshCollider.sharedMesh = meshFilter.mesh;
    }

    private Texture2D tex2D;
    private void DrawTex(RaycastHit hitInfo)
    {
        Vector2 position = hitInfo.textureCoord;

        float r = 0.05f;

        for (int i = 0; i < tex2D.width; i++)
        {
            for (int j = 0; j < tex2D.height; j++)
            {
                float dis = Vector2.Distance(position, new Vector2(i * 1.0f / tex2D.width, j * 1.0f / tex2D.height));

                if (dis <= r)
                {
                    tex2D.SetPixel(i, j, Color.red);
                }
            }
        }
        tex2D.Apply();
    }
}
