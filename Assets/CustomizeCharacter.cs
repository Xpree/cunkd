using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CustomizeCharacter : NetworkBehaviour
{
    [SerializeField] Material[] detailsMaterials;
    [SerializeField] Material[] furMaterials;

    [SerializeField] SkinnedMeshRenderer detailsRenderer;
    [SerializeField] SkinnedMeshRenderer furRenderer;
    // Start is called before the first frame update


    public override void OnStartClient()
    {
        base.OnStartClient();
        int index = GetComponent<GameClient>().ClientIndex % furMaterials.Length;

        furRenderer.material = furMaterials[index];
        detailsRenderer.material = detailsMaterials[index];
    }

    /*

    [SyncVar(hook = nameof(setColorOnClients))] int colorIndex;

    [Server]
    void Start()
    {
        //int index = Random.Range(0, furMaterials.Length);
        //furRenderer.material = furMaterials[index];
        //detailsRenderer.material = detailsMaterials[index];
        colorIndex = Random.Range(0, furMaterials.Length);
        furRenderer.material = furMaterials[colorIndex];
        detailsRenderer.material = detailsMaterials[colorIndex];
    }
    [Server]
    public void randomColor()
    {
        int colorIndex = Random.Range(0, furMaterials.Length);
        furRenderer.material = furMaterials[colorIndex];
        detailsRenderer.material = detailsMaterials[colorIndex];
    }
    
    void setColorOnClients(int old, int current)
    {
        furRenderer.material = furMaterials[current];
        detailsRenderer.material = detailsMaterials[current];
    }
    */
}
