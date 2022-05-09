using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadMat : MonoBehaviour
{
    Vector3[] points;
    [SerializeField] GameObject marker;
    [SerializeField] GameObject newShape;
    [SerializeField] float precision;
    [SerializeField] float iceThickness;
    [SerializeField] float affectedHeight;
    [SerializeField] float overhang;
    [SerializeField] float overhangVariance;
    [SerializeField] float edgeSprinkling;
    [SerializeField] bool spawnMarkers =false;
    [SerializeField] float radius;
    [SerializeField] private LayerMask layermask;



    int rays;
    public GameObject[] iceMat;

    enum directions { left, right, up, down }


    private void Start()
    {
        rays = (int)(radius / precision);
        iceMat = new GameObject[4];

        raysLeft();
        raysRight();
        raysUp();
        raysDown();
    }

    string meshName = "";

    void raysLeft()
    {
        //points = new List<Vector3>();
        points = new Vector3[rays * rays];
        lastHit = true;
        lastY = 0;
        shots = 0;
        reportedPoints = 0;
        i = 0;

        meshName = "leftMesh";
        Vector3 pos = transform.position;
        Vector3 pos1 = transform.position;
        pos1.y = transform.position.y + affectedHeight;

        for (int x = 0; x < rays; x++)
        {
            for (int z = 0; z < rays; z++)
            {
                pos = pos1 + new Vector3(-x, 0, z) * precision;
                ShootRay(pos, x + z * rays);
            }
        }
    }
    void raysRight()
    {
        points = new Vector3[rays * rays];
        lastHit = true;
        lastY = 0;
        shots = 0;
        reportedPoints = 0;
        i = 0;

        meshName = "rightMesh";
        Vector3 pos = transform.position;
        Vector3 pos1 = transform.position;
        pos1.y = transform.position.y + affectedHeight;

        for (int x = 0; x < rays; x++)
        {
            for (int z = 0; z < rays; z++)
            {
                pos = pos1 + new Vector3(x, 0, -z) * precision;
                ShootRay(pos, x + z * rays);
            }
        }
    }

    void raysUp()
    {
        points = new Vector3[rays * rays];
        lastHit = true;
        lastY = 0;
        shots = 0;
        reportedPoints = 0;
        i = 0;

        meshName = "upMesh";
        Vector3 pos = transform.position;
        Vector3 pos1 = transform.position;
        pos1.y = transform.position.y + affectedHeight;

        for (int z = 0; z < rays; z++)
        {
            for (int x = 0; x < rays; x++)
            {
                pos = pos1 + new Vector3(x, 0, z) * precision;
                ShootRay(pos, z + x * rays);
            }
        }
    }

    void raysDown()
    {
        points = new Vector3[rays * rays];
        lastHit = true;
        lastY = 0;
        shots = 0;
        reportedPoints = 0;
        i = 0;

        meshName = "downMesh";
        Vector3 pos = transform.position;
        Vector3 pos1 = transform.position;
        pos1.y = transform.position.y + affectedHeight;

        for (int z = 0; z < rays; z++)
        {
            for (int x = 0; x < rays; x++)
            {
                pos = pos1 + new Vector3(-x, 0, -z) * precision;
                ShootRay(pos, z + x * rays);
            }
        }
    }


    bool lastHit = true;
    float lastY =0;
    int shots = 0;
    GameObject parent;
    void ShootRay(Vector3 origin, int index)
    {
        shots++;
        RaycastHit hit;
        Ray ray = new Ray(origin, Vector3.down);

        if (Physics.Raycast(ray, out hit, affectedHeight + overhang, layermask))
        {
            if (!parent)
            {
                parent = hit.transform.gameObject;
            }
            points[index] = hit.point;
            lastHit = true;
            lastY = hit.point.y;
        }

        else//icetap
        {
            if (lastHit)
            {
                Vector3 p = origin;
                //p.y = lastY + ( + overhang + Random.Range(-overhangVariance, overhangVariance));
                p.y -= (affectedHeight + overhang + Random.Range(-overhangVariance, overhangVariance));

                lastY = p.y;
                points[index] = p;

            }
            lastHit = false;
        }

        if (spawnMarkers)
        {
            SpawnMarker(points[index]);
        }

        if (shots == rays * rays)
        {
            makeMesh();
        }
    }

    int seekerNumber = 0;
    void SpawnMarker(Vector3 startPos)
    {
        GameObject go = Instantiate(marker, startPos, Quaternion.identity);
        go.transform.SetParent(this.transform);
        go.name = "marker number " + seekerNumber;
        go.transform.localScale = new Vector3(0.01f, 1, 0.01f);
        //go.SetActive(false);
    }

    int reportedPoints = 0;
    public void ReportPosition(Vector3 pos)
    {
        reportedPoints++;

        if (reportedPoints == rays * rays)
        {
            makeMesh();
        }
    }

    int[] newTriangles;

    int meshIndex =0;
    void makeMesh()
    {

        print("making mesh: " + meshName);

        Mesh mesh = new Mesh();

        Vector3[] vertices = points;

        newTriangles = new int[rays * rays * 2 * 3];

        int p0 = 0;
        int p1 = 0;
        int p2 = 0;
        int p3 = 0;
        for (int i = 0; i < rays * (rays - 1); i++)
        {
            if ((i + 1) % rays != 0)
            {
                p0 = i;
                p1 = i + 1;
                p2 = rays + i + 1;
                p3 = rays + i;
                AddTriangle(new Vector3Int(p0, p1, p2));
                AddTriangle(new Vector3Int(p0, p2, p3));
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = newTriangles;
        mesh.RecalculateNormals();

        GameObject go = Instantiate(newShape, Vector3.zero, Quaternion.identity);
        go.name = meshName;
        go.GetComponent<MeshFilter>().mesh = mesh;
        MeshCollider col = go.AddComponent<MeshCollider>();
        //col.enabled = false;
        col.sharedMesh = mesh;
        go.transform.position += new Vector3(0, iceThickness, 0);
        //iceMat.transform.SetParent(parent.transform);
        go.transform.localScale = new Vector3(1, 1, 1);
        iceMat[meshIndex++] = go;
    }

    int i = 0;
    void AddTriangle(Vector3Int tri)
    {

        Vector3 mid = transform.position;

        for (int i = 0; i < 3; i++)
        {
            Vector3 other = points[tri[i]];
            other.y = transform.position.y;
            if (Random.Range(radius-edgeSprinkling, radius) < (other - mid).magnitude)
            {
                return;
            }
        }
        newTriangles[i++] = tri.x;
        newTriangles[i++] = tri.y;
        newTriangles[i++] = tri.z;
    }
}
