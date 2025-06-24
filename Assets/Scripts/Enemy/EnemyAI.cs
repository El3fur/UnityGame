using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

[RequireComponent(typeof(NavMeshAgent), typeof(NetworkObject))]
public class EnemyAI : NetworkBehaviour
{
    private NavMeshAgent agent;
    private Transform target;

    // �������� � ������� ������ ����� ����� ����������� � ����������
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // ����� ����� ��� ������������� Transform
        agent.updatePosition = true;
        agent.updateRotation = true;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) // ������ �� �� �������
        {
            enabled = false;
            return;
        }

        // ����� ���� ������� �� ���� � ������� ����������
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

        // ������ ���� ������ ���� � � ����� ��� � ���
        agent.SetDestination(target.position);
    }

    // ����� ���� ����, ��� ����� Despawn() ��� Destroy()
    public void Die()
    {
        if (IsServer)
            GetComponent<NetworkObject>().Despawn();
        else
            Destroy(gameObject);
    }
}
