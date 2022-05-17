using UnityEngine;

public class TransparentBlitCode : MonoBehaviour
{
    Camera mainCamera;
    RenderTexture renderTexture;
    Camera secondaryCamera;
    public Material blitMaterial;
    public string rendererTextureName = "_SecondaryCamera_";

    private Vector2 resolution;

    void Start()
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

    private void Update()
    {
        if(Screen.width != secondaryCamera.targetTexture.width || Screen.height != secondaryCamera.targetTexture.height)
        {
            renderTexture = new RenderTexture(Screen.width, Screen.height, 1, RenderTextureFormat.Default);
            secondaryCamera.targetTexture = renderTexture;
            renderTexture.name = rendererTextureName;
            Shader.SetGlobalTexture(rendererTextureName, renderTexture);
        }
    }
}
