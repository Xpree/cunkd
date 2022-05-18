using UnityEngine;

public class PlaneCuller : MonoBehaviour
{
    [SerializeField] float renderDistance = 100;

    Plane[] edgePlanes;
    MeshRenderer[] meshRenderers;

    private void Start()
    {
        meshRenderers = FindObjectOfType<ScoreKeeper>().gameObject.transform.GetChild(0).GetComponentsInChildren<MeshRenderer>();
        edgePlanes = new Plane[meshRenderers.Length];
        for (int i = 0; i < edgePlanes.Length; i++)
        {
            edgePlanes[i].SetNormalAndPosition(meshRenderers[i].transform.up, meshRenderers[i].transform.position);
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < edgePlanes.Length; i++)
        {
            if (edgePlanes[i].GetDistanceToPoint(transform.position) < renderDistance)
                meshRenderers[i].enabled = true;
            else
                meshRenderers[i].enabled = false;
        }
    }
}
