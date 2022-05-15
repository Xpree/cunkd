using UnityEngine;
using Mirror;

public class ScoreKeeperChildTrigger : MonoBehaviour
{
    ScoreKeeper scoreKeeper;

    [ServerCallback]
    void Start()
    {
        scoreKeeper = GetComponentInParent<ScoreKeeper>();
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        scoreKeeper.RemoteTrigger(other);
    }
}
