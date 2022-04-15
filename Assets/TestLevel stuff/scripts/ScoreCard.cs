using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class ScoreCard : NetworkBehaviour, IComparable<ScoreCard>
{
    [SyncVar(hook = nameof(OnLivesChanged))] public int livesLeft;

    public bool Dead => (livesLeft <= 0);

    public string PlayerName => GetComponent<GameClient>()?.PlayerName ?? "<missing>";

    public int Index => GetComponent<GameClient>()?.ClientIndex ?? -1;

    public int CompareTo(ScoreCard other)
    {
        if (other.isLocalPlayer)
            return 1;
        if (this.isLocalPlayer)
        {
            return -1;
        }
        return this.Index.CompareTo(other.Index);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        FindObjectOfType<ScoreKeeper>()?.InitializeScoreCard(this);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        FindObjectOfType<Scoreboard>()?.SetLocalLives(livesLeft);
    }

    void OnLivesChanged(int previous, int current)
    {
        if(isLocalPlayer)
            FindObjectOfType<Scoreboard>()?.SetLocalLives(current);
    }
}
