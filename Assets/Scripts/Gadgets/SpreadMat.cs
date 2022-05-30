using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.VFX;

public class SpreadMat : NetworkBehaviour
{
    [SerializeField] GameObject marker;
    [SerializeField] GameObject ice;
    [SerializeField] private LayerMask IceAndFreezeLayermask;
    [SerializeField] private LayerMask onlyIceLayermask;
    [SerializeField] float precision;
    [SerializeField] float iceThickness;
    [SerializeField] float affectedHeight;
    [SerializeField] float overhangVariance;
    [SerializeField] float edgeSprinkling;
    [SerializeField] float radius;
    [SerializeField] bool addCollider;
    [SerializeField] float rayLength;

    [SerializeField] VisualEffect vfx;
    Vector3[] points;
    int[] newTriangles;

    public Transform parent;

    List<GameObject> frozenObjects;
    List<GameObject> icedObjects;
    public List<GameObject> iceMat;

    [SerializeField] SphereCollider Icecollider;
    [SerializeField] SphereCollider slipcollider;
    [SerializeField] MeshCollider trapCollider;
    int rays;
    string meshName = "";

    [SerializeField] GameSettings _settings;
    [SyncVar] NetworkTimer _endTime;
    [SyncVar] NetworkTimer _destroyTime;
    public override void OnStartServer()
    {
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }
    }

    private void Awake()
    {
        slipcollider.radius = radius - 0.5f;
        rays = (int)(radius / precision);
        iceMat = new List<GameObject>();
        frozenObjects = new List<GameObject>();
        icedObjects = new List<GameObject>();
    }
    public void Trigger()
    {
        Icecollider.enabled = true;
        slipcollider.enabled = true;
        _endTime = NetworkTimer.FromNow(_settings.IceGadget.Duration);
        _destroyTime = NetworkTimer.FromNow(_settings.IceGadget.Duration + 1);
        Icecollider.isTrigger = true;
        Icecollider.radius = radius - 0.5f;
        MakeIce(transform.position + new Vector3(0, affectedHeight - 0.05f, 0), rayLength, radius, overhangVariance, IceAndFreezeLayermask);
        Invoke("DisableCollider", 0.1f);
    }

    void DisableCollider()
    {
        Icecollider.enabled = false;
        trapCollider.enabled = false;
    }

    void destroyIce()
    {
        unFreezeObjects();
        foreach (var item in iceMat)
        {
            Destroy(item);
        }
        slipcollider.enabled = false;
    }

    [Client]
    void FixedUpdate()
    {
        if (_endTime.HasTicked && 0 < iceMat.Count)
        {
            destroyIce();
        }
        if (_destroyTime.HasTicked)
        {
            Destroy(this.gameObject);
        }
    }

    [Client]
    //Makes 4 Ice Meshes Iteratevly from origin outward
    void MakeIce(Vector3 origin, float rayLength, float radius, float overHangVariance, LayerMask layermask)
    {
        rays = (int)(radius / precision);

        int xDirection =1;
        int zDirection =1;
        Vector3 pos;

        for (int i = 0; i < 4; i++)
        {
            shots = 0;
            triangleIndex = 0;
            meshName = "FrozenMesh "+ i;
            points = new Vector3[rays * rays];

            if (i == 1)
                xDirection = -1;
            if (i == 2)
                zDirection = -1;
            if (i == 3)
                xDirection = 1;

            for (int x = 0; x < rays; x++)
            {
                for (int z = 0; z < rays; z++)
                {
                    if (i == 0 || i == 2)
                        pos = origin + new Vector3(z * zDirection, 0, x * xDirection) * precision;
                    else 
                        pos = origin + new Vector3(x * xDirection, 0, z * zDirection) * precision;
                     ShootRays(pos, rayLength, radius, overhangVariance, x + z * rays, layermask);
                }
            }   
        }
    }

    bool lastHit = true;
    int shots = 0;
    void ShootRays(Vector3 origin, float rayLength, float radius, float overHangVariance, int index, LayerMask layermask)
    {
        shots++;
        RaycastHit hit;
        Ray ray = new Ray(origin, Vector3.down);

        if (Physics.Raycast(ray, out hit, rayLength, layermask))
        {
            points[index] = hit.point;
            lastHit = true;
        }
        else//icetap
        {
            if (lastHit)
            {
                Vector3 p = origin;
                p.y -= (rayLength + Random.Range(-overHangVariance, overHangVariance));
                points[index] = p;
            }
            lastHit = false;
        }

        if (shots == rays * rays)
            makeMesh();
    }

    void makeMesh()
    {
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
        go.transform.SetParent(transform, true);
        go.name = meshName;
        go.GetComponent<MeshFilter>().mesh = mesh;
        go.transform.position += new Vector3(0, iceThickness, 0);
        if (snow)
        {
            VisualEffect ve = go.GetComponentInChildren<VisualEffect>();
            ve.enabled = true;
            ve.transform.position = snowPosition;
            snow = false;
        }

        //if (addCollider)
        //{
        //    MeshCollider col = go.AddComponent<MeshCollider>();
        //    col.enabled = false;
        //    col.sharedMesh = mesh;
        //}
        iceMat.Add(go);
    }

    void MakeIceOnClient(Mesh mesh)
    {
        GameObject go = Instantiate(ice, transform.position, Quaternion.identity);
        go.name = meshName;
        go.GetComponent<MeshFilter>().mesh = mesh;
    }

    [SerializeField] float pointMaxDistance;
    int triangleIndex = 0;
    void AddTriangle(Vector3Int tri)
    {
        Vector3 mid = transform.position;

        for (int i = 0; i < 3; i++)
        {
            Vector3 other = points[tri[i]];
            other.y = transform.position.y;
            if (Random.Range(radius-edgeSprinkling, radius) < (other - mid).magnitude)
                return;
        }
        if (points[tri.x] == Vector3.zero)
            return;
        if (points[tri.y] == Vector3.zero)
            return;
        if (points[tri.z] == Vector3.zero)
            return;

        if (pointMaxDistance < (points[tri.x] - points[tri.y]).magnitude)
            return;
        if (pointMaxDistance < (points[tri.x] - points[tri.z]).magnitude)
            return;
        if (pointMaxDistance < (points[tri.y] - points[tri.z]).magnitude)
            return;

        newTriangles[triangleIndex++] = tri.x;
        newTriangles[triangleIndex++] = tri.y;
        newTriangles[triangleIndex++] = tri.z;
    }

    void FreezeObject(GameObject go)
    {
        //print("Freezing " + go.name);
        if (go.GetComponent<Rigidbody>())
        {
            go.GetComponent<Rigidbody>().isKinematic = true;
        }
        if (go.GetComponentInParent<Rigidbody>())
        {
            go.GetComponentInParent<Rigidbody>().isKinematic = true;
        }
        if (go.GetComponent<Pullable>())
        {
            go.GetComponent<Pullable>().enabled = false;
        }
        frozenObjects.Add(go);
    }

    public void unFreezeObjects()
    {
        foreach (var go in frozenObjects)
        {
            if (go)
            {
                if (go.GetComponent<Rigidbody>())
                {
                    go.GetComponent<Rigidbody>().isKinematic = false;
                }
                if (go.GetComponentInParent<Rigidbody>())
                {
                    go.GetComponentInParent<Rigidbody>().isKinematic = false;
                }
                if (go.GetComponent<Pullable>())
                {
                    go.GetComponent<Pullable>().enabled = true;
                }
            }
        }
    }

    bool snow = false;
    Vector3 snowPosition = new Vector3();
    public void setCollision(Collider other)
    {

        GameObject go = other.gameObject;
        //this is to make ice on trees
        if (onlyIceLayermask == (onlyIceLayermask | (1 << go.layer)) && !icedObjects.Contains(go))
        {
            snow = true;
            snowPosition = go.transform.parent.position + Vector3.up * 6;
            MakeIce(go.transform.parent.position + new Vector3(0, 7, 0), 2, 3.4f, 0.9f, onlyIceLayermask);
            icedObjects.Add(go);
        }
        else
        if (IceAndFreezeLayermask == (IceAndFreezeLayermask | (1 << go.layer)) && !frozenObjects.Contains(go) && other.gameObject.tag != "DoNotFreeze")
        {
            FreezeObject(go);
        }
    }
}
