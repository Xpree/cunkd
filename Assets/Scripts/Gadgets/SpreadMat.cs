using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class SpreadMat : MonoBehaviour
{
    Vector3[] points;
    [SerializeField] GameObject marker;
    [SerializeField] GameObject ice;
    [SerializeField] float precision;
    [SerializeField] float iceThickness;
    [SerializeField] float affectedHeight;
    [SerializeField] float overhang;
    [SerializeField] float overhangVariance;
    [SerializeField] float edgeSprinkling;
    [SerializeField] bool spawnMarkers =false;
    [SerializeField] float radius;
    [SerializeField] private LayerMask layermask;
    [SerializeField] bool addCollider;

    [SerializeField] float rayLength;


    List<GameObject> frozenObjects;



    int rays;
    public GameObject[] iceMat;

    enum directions { left, right, up, down }


    private void Start()
    {
        rays = (int)(radius / precision);
        iceMat = new GameObject[4];
        frozenObjects = new List<GameObject>();

        raysLeft();
        raysRight();
        raysUp();
        raysDown();


        //raysLeft();
        //raysRight();
        //raysUp();
        //raysDown();
    }

    string meshName = "";

    void raysLeft()
    {
        first = true;
        //points = new List<Vector3>();
        points = new Vector3[rays * rays];
        lastHit = true;
        lastY = transform.position.y + affectedHeight;
        shots = 0;
        reportedPoints = 0;
        i = 0;
        meshName = "leftMesh";


        Vector3 pos = transform.position;
        Vector3 pos1 = transform.position;
        pos1.y = transform.position.y + affectedHeight ;

        //Vector3 pos = currentGo.transform.position;
        //Vector3 pos1 = currentGo.transform.position;
        //Vector3 pos1 = new Vector3(Mathf.Round(currentGo.transform.position.x), Mathf.Round(currentGo.transform.position.y), Mathf.Round(currentGo.transform.position.z));
        //pos1.y = pos1.y + affectedHeight;

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
        first = true;
        points = new Vector3[rays * rays];
        lastHit = true;
        lastY = transform.position.y + affectedHeight;
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
        first = true;
        points = new Vector3[rays * rays];
        lastHit = true;
        lastY = transform.position.y + affectedHeight;
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
        first = true;
        points = new Vector3[rays * rays];
        lastHit = true;
        lastY = transform.position.y + affectedHeight;
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

    bool first = true;
    void ShootRay(Vector3 origin, int index)
    {
        shots++;
        RaycastHit hit;
        Ray ray = new Ray(origin, Vector3.down);

        if (Physics.Raycast(ray, out hit, rayLength, layermask))
        {
            bool volcano = hit.transform.gameObject.name == "Volcano Original";

            if (first)
            {
                lastY = hit.point.y;
                first = false;
            }
            if (Mathf.Abs(lastY - hit.point.y) < overhang || volcano)
            {
                points[index] = hit.point;
                lastHit = true;
                lastY = hit.point.y;
                if (volcano)
                {
                    first = true;
                }
            }
            else
            if (lastHit)
            {
                Vector3 p = origin;
                p.y = lastY - (overhang + Random.Range(-overhangVariance, overhangVariance));
                //p.y -= (overhang + Random.Range(-overhangVariance, overhangVariance));

                //lastY = p.y;
                points[index] = p;
                lastHit = false;

            }


        }

        else//icetap
        {
            if (lastHit)
            {
                Vector3 p = origin;
                p.y = lastY - (overhang + Random.Range(-overhangVariance, overhangVariance));
                //p.y -= (overhang + Random.Range(-overhangVariance, overhangVariance));

                //lastY = p.y;
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

        GameObject go = Instantiate(ice, Vector3.zero, Quaternion.identity);
        go.name = meshName;
        go.GetComponent<MeshFilter>().mesh = mesh;

        if (addCollider)
        {
            MeshCollider col = go.AddComponent<MeshCollider>();
            col.sharedMesh = mesh;
        }
        //col.enabled = false;
        go.transform.position += new Vector3(0, iceThickness, 0);
        //iceMat.transform.SetParent(parent.transform);
        go.transform.localScale = new Vector3(1, 1, 1);
        //iceMat[meshIndex++] = go;
    }

    [SerializeField] float pointMaxDistance;
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

        //if (pointMaxDistance < (points[tri.x] - points[tri.y]).magnitude)
        //{
        //    return;
        //}
        //if (pointMaxDistance < (points[tri.x] - points[tri.z]).magnitude)
        //{
        //    return;
        //}
        //if (pointMaxDistance < (points[tri.y] - points[tri.z]).magnitude)
        //{
        //    return;
        //}

        newTriangles[i++] = tri.x;
        newTriangles[i++] = tri.y;
        newTriangles[i++] = tri.z;
    }

    void CopyMesh(GameObject go)
    {
        MeshFilter[] originalMeshes = go.GetComponentsInChildren<MeshFilter>();
        ProBuilderMesh[] originalPBMeshes = go.GetComponentsInChildren<ProBuilderMesh>();
        MeshRenderer ba;

        foreach (var meshF in originalMeshes)
        {

            GameObject frozenMesh = Instantiate(ice, meshF.transform.position, Quaternion.identity);
            frozenMesh.name = meshF.transform.name + " frozen mesh";
            frozenMesh.GetComponent<MeshFilter>().mesh = meshF.mesh;


            frozenMesh.transform.position = meshF.transform.position;
            frozenMesh.transform.rotation = meshF.transform.rotation;
            frozenMesh.transform.localScale = meshF.transform.localScale;
            //frozenMesh.transform.localScale = go.transform.localScale;
        }

        //foreach (var item in originalPBMeshes)
        //{
        //    Mesh mesh = new Mesh();
        //    mesh.Clear();
        //    Vertex
        //    mesh.vertices = item.GetVertices().GetValue(0);
        //}

        //if (originalMesh)
        //{
        //    GameObject frozenMesh = Instantiate(ice, go.transform.position, Quaternion.identity);
        //    frozenMesh.name = go.name + " frozen mesh";
        //    frozenMesh.GetComponent<MeshFilter>().mesh = originalMesh;

        //    //MeshCollider col = frozenMesh.AddComponent<MeshCollider>();
        //    //col.enabled = false;
        //    //col.sharedMesh = originalMesh;
        //    //iceMat.transform.SetParent(parent.transform);
        //    frozenMesh.transform.localScale = go.transform.localScale;
        //    //frozenMesh.transform.SetParent(go.transform);
        //}
    }

    GameObject currentGo;

    //void FreezeObject(GameObject go)
    //{
    //    if (go)
    //    {
    //        print("Freezing " + go.name);
    //        currentGo = go;
    //        raysLeft();
    //        //raysRight();
    //        //raysUp();
    //        //raysDown();
    //    }
    //}

    //bool onlyOnce = true;
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (onlyOnce)
    //    {
    //        FreezeObject(other.gameObject);
    //        onlyOnce = false;
    //    }
    //    //frozenObjects.Add(other.gameObject);
    //}
}
