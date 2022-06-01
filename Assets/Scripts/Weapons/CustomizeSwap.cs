using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CustomizeSwap : NetworkBehaviour
{
    [SerializeField] Material[] swapMaterials;
    //[SerializeField] Material[] furMaterials;

    [SerializeField] MeshRenderer swapRenderer;
    //[SerializeField] SkinnedMeshRenderer furRenderer;
    // Start is called before the first frame update

    [SyncVar(hook = nameof(setColorOnClients))] int colorIndex;

    [Server]
    void Start()
    {
        //int index = Random.Range(0, furMaterials.Length);
        //furRenderer.material = furMaterials[index];
        //detailsRenderer.material = detailsMaterials[index];
        colorIndex = Random.Range(0, swapMaterials.Length);
        //furRenderer.material = furMaterials[colorIndex];
        swapRenderer.material = swapMaterials[colorIndex];
    }

    [Server]
    public void randomColor()
    {
        int colorIndex = Random.Range(0, swapMaterials.Length);
        //furRenderer.material = furMaterials[colorIndex];
        swapRenderer.material = swapMaterials[colorIndex];
    }

    void setColorOnClients(int old, int current)
    {
        //furRenderer.material = furMaterials[current];
        swapRenderer.material = swapMaterials[current];
    }
}
