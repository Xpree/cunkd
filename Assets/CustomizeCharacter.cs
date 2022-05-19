using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizeCharacter : MonoBehaviour
{
    [SerializeField] Material[] detailsMaterials;
    [SerializeField] Material[] furMaterials;

    [SerializeField] SkinnedMeshRenderer detailsRenderer;
    [SerializeField] SkinnedMeshRenderer furRenderer;
    // Start is called before the first frame update
    void Start()
    {
        int index = Random.Range(0, furMaterials.Length);
        furRenderer.material = furMaterials[index];
        detailsRenderer.material = detailsMaterials[index];
    }
}
