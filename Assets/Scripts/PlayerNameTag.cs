using Mirror;
using TMPro;

public class PlayerNameTag : NetworkBehaviour
{
    public TextMeshProUGUI NameCanvas;

    public override void OnStartClient()
    {
        base.OnStartClient();
        NameCanvas.text = GetComponent<GameClient>()?.PlayerName ?? "missing";
    }
}
