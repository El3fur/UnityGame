using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField]
    private int maxHealth = 10;

    private int currentHealth;
    private Animator _animator;

    [SerializeField]
    private ParticleSystem deathParticles;

    [Header("XP Settings")]
    [Tooltip("Сколько XP даёт убийство этого врага")]
    [SerializeField]
    private int xpReward = 100;


    private Dictionary<ulong, int> damageByClient = new Dictionary<ulong, int>();

    private void Awake()
    {

        _animator = GetComponent<Animator>();

        currentHealth = maxHealth;
    }

   
    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        _animator.SetTrigger("Hit");

        if (!IsServer)
        {
          
            ApplyDamageServerRpc(damage, NetworkManager.Singleton.LocalClientId);
            return;
        }

        ApplyDamageOnServer(damage, NetworkManager.Singleton.LocalClientId);
    }

    private void ApplyDamageOnServer(int damage, ulong ownerClientId)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        
        if (!damageByClient.ContainsKey(ownerClientId))
            damageByClient[ownerClientId] = 0;
        damageByClient[ownerClientId] += damage;

        
        PlayHitAnimationClientRpc();

        if (currentHealth <= 0)
        {
            DistributeXPAndDie();
        }
    }

    
    [ServerRpc(RequireOwnership = false)]
    private void ApplyDamageServerRpc(int damage, ulong ownerClientId, ServerRpcParams rpcParams = default)
    {
        
        if (!IsServer) return;
        ApplyDamageOnServer(damage, ownerClientId);
    }

    
    [ClientRpc]
    private void PlayHitAnimationClientRpc(ClientRpcParams rpcParams = default)
    {
        if (_animator != null)
            _animator.SetTrigger("Hit");
    }


    private void DistributeXPAndDie()
    {
        if (!IsServer) return;

        
        int totalDamage = damageByClient.Values.Sum();
        if (totalDamage <= 0)
        {
            
            DeathCleanup();
            return;
        }

        
        foreach (var kvp in damageByClient)
        {
            ulong clientId = kvp.Key;
            int dmg = kvp.Value;
            
            float share = (float)dmg / totalDamage;
            int xpForPlayer = Mathf.RoundToInt(share * xpReward);

            
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                var playerObj = client.PlayerObject;
                if (playerObj != null)
                {
                    var expComp = playerObj.GetComponent<PlayerExperience>();
                    if (expComp != null)
                        expComp.AwardXP(xpForPlayer);
                }
            }
        }

        
        DeathCleanup();
    }


    [ClientRpc]
    private void PlayDeathEffectsAndDespawnClientRpc(ClientRpcParams rpcParams = default)
    {
        
        if (deathParticles != null)
        {
            
            Instantiate(deathParticles, transform.position, Quaternion.identity);
        }

        var netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    private void DeathCleanup()
    {
        
        PlayDeathEffectsAndDespawnClientRpc();
    }
}
