using System;
using UnityEngine;
using Mirror;

public class BlackHole : NetworkBehaviour
{
    [SerializeField] GameSettings _settings;    
    [SyncVar] NetworkTimer _endTime;

    [SerializeField] CameraShakeSource _cameraShake;

    private void Start()
    {
        GetComponent<SphereCollider>().radius = _settings.BlackHole.Range;
    }
    
    public override void OnStartServer()
    {
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }
        
        _endTime = NetworkTimer.FromNow(_settings.BlackHole.Duration);
    }

   
    void FixedUpdate()
    {
        if (_endTime.HasTicked)
        {
            if (NetworkServer.active)
            {
                NetworkServer.Destroy(this.gameObject);
            }
            return;
        }
        
        var collisions = Physics.OverlapSphere(this.transform.position, _settings.BlackHole.Range, ~0, QueryTriggerInteraction.Ignore);
        foreach (var collision in collisions)
        {
            var rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null && Util.HasPhysicsAuthority(collision.gameObject))
            {
                var distance = Mathf.Max(Vector3.Distance(rb.transform.position, transform.position), 1.0f);
                var force = (transform.position - rb.transform.position).normalized / distance * _settings.BlackHole.Intensity;
                rb.AddForce(force, ForceMode.Force);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == NetworkClient.localPlayer.gameObject)
        {
            _cameraShake.OneShotShake(NetworkTimer.Now);
        }
    }
}
