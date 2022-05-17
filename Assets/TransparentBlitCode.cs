using UnityEngine;
using Mirror;

public class TransparentBlitCode : NetworkBehaviour
{
    [SerializeField] Camera mainCamera;
    RenderTexture renderTexture;
    [SerializeField]Camera secondaryCamera;
    public Material blitMaterial;
    public string rendererTextureName = "_SecondaryCamera_";

    public bool triggered = false;
    private Vector2 resolution;


    [Client]
    void Start()
    {
        base.OnStartServer();
        if (!isLocalPlayer)
        {
            secondaryCamera.enabled = false;
            return;
        }
        triggered = true;
        Debug.Assert(mainCamera);
        renderTexture = new RenderTexture(Screen.width, Screen.height, 1, RenderTextureFormat.Default);
        Debug.Assert(secondaryCamera);
        secondaryCamera.targetTexture = renderTexture;
        renderTexture.name = rendererTextureName;
        Shader.SetGlobalTexture(rendererTextureName, renderTexture);
    }
    [Client]
    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (Screen.width != secondaryCamera.targetTexture.width || Screen.height != secondaryCamera.targetTexture.height)
        {
            renderTexture = new RenderTexture(Screen.width, Screen.height, 1, RenderTextureFormat.Default);
            secondaryCamera.targetTexture = renderTexture;
            renderTexture.name = rendererTextureName;
            Shader.SetGlobalTexture(rendererTextureName, renderTexture);
        }
    }
}
