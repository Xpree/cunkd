using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CustomizeHammer : NetworkBehaviour
{
    [SerializeField] Material[] hammerMaterials;
    //[SerializeField] Material[] furMaterials;

    [SerializeField] MeshRenderer hammerRenderer;
    //[SerializeField] SkinnedMeshRenderer furRenderer;
    // Start is called before the first frame update

    [SyncVar(hook = nameof(setColorOnClients))] int colorIndex;

    [Server]
    void Start()
    {
        //int index = Random.Range(0, furMaterials.Length);
        //furRenderer.material = furMaterials[index];
        //detailsRenderer.material = detailsMaterials[index];
        colorIndex = Random.Range(0, hammerMaterials.Length);
        //furRenderer.material = furMaterials[colorIndex];
        hammerRenderer.material = hammerMaterials[colorIndex];
    }

    [Server]
    public void randomColor()
    {
        int colorIndex = Random.Range(0, hammerMaterials.Length);
        //furRenderer.material = furMaterials[colorIndex];
        hammerRenderer.material = hammerMaterials[colorIndex];
    }

    void setColorOnClients(int old, int current)
    {
        //furRenderer.material = furMaterials[current];
        hammerRenderer.material = hammerMaterials[current];
    }
}
