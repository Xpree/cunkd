using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentBlitCode : MonoBehaviour
{
    Camera mainCamera;
    RenderTexture renderTexture;
    Camera secondaryCamera;
    public Material blitMaterial;
    public string rendererTextureName = "_SecondaryCamera_";

    void Start()//on update, set screen height and width on render texture, with event?
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Debug.Assert(mainCamera);
        renderTexture = new RenderTexture(Screen.width, Screen.height, 1, RenderTextureFormat.Default);
        secondaryCamera = GetComponent<Camera>();
        Debug.Assert(secondaryCamera);
        secondaryCamera.targetTexture = renderTexture;
        renderTexture.name = rendererTextureName;
        Shader.SetGlobalTexture(rendererTextureName, renderTexture);
    }
}
