using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

[RequireComponent(typeof(NavMeshAgent), typeof(NetworkObject))]
public class EnemyAI : NetworkBehaviour
{
    private NavMeshAgent agent;
    private Transform target;

    // скорость и поворот агента можно тонко настраивать в инспекторе
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // чтобы агент сам контролировал Transform
        agent.updatePosition = true;
        agent.updateRotation = true;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) // логика »» на сервере
        {
            enabled = false;
            return;
        }

        // найдЄм всех игроков по тегу и выберем ближайшего
        var players = GameObject.FindGameObjectsWithTag("Player");
        float bestDist = float.MaxValue;
        foreach (var p in players)
        {
            float d = Vector3.Distance(transform.position, p.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                target = p.transform;
            }
        }
    }

    void Update()
    {
        if (!IsServer || target == null) return;

        // каждый кадр ставим цель Ч и агент идЄт к ней
        agent.SetDestination(target.position);
    }

    // когда враг умрЄт, его нужно Despawn() или Destroy()
    public void Die()
    {
        if (IsServer)
            GetComponent<NetworkObject>().Despawn();
        else
            Destroy(gameObject);
    }
}
