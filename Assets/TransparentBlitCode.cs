using UnityEngine;
using Mirror;

public class TransparentBlitCode : NetworkBehaviour
{
    [SerializeField] Camera mainCamera;
    RenderTexture renderTexture;
    [SerializeField] Camera secondaryCamera;
    public Material blitMaterial;
    public string rendererTextureName = "_SecondaryCamera_";


    void Awake()
    {
        Debug.Assert(mainCamera);
        Debug.Assert(secondaryCamera);
        secondaryCamera.enabled = false;
    }

    public override void OnStartLocalPlayer()
    {
        SetActiveCamera();
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
        }
    }

    void CreateTexture()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
        }
        renderTexture = new RenderTexture(Screen.width, Screen.height, 1, RenderTextureFormat.Default);
        secondaryCamera.targetTexture = renderTexture;
        renderTexture.name = rendererTextureName;
    }

    private void Update()
    {
        if (mainCamera.enabled == false)
            return;
        
        if (Screen.width != secondaryCamera.targetTexture.width || Screen.height != secondaryCamera.targetTexture.height)
        {
            if(renderTexture != null)
                CreateTexture();
        }
    }

    public void SetActiveCamera()
    {
        if (renderTexture == null)
        {
            CreateTexture();
        }
        Shader.SetGlobalTexture(rendererTextureName, renderTexture);
        secondaryCamera.enabled = true;
    }

    public void DeactivateCamera()
    {
        secondaryCamera.enabled = false;
    }
}
