using UnityEngine;
using Unity.Netcode;

public class PlayerExperience : NetworkBehaviour
{
    private const int MinThreshold = 800;
    private const int MaxThreshold = 1100;

    public NetworkVariable<int> CurrentXP = new NetworkVariable<int>(0);
    public NetworkVariable<int> XPThreshold = new NetworkVariable<int>(0);
    public NetworkVariable<int> SkillPoints = new NetworkVariable<int>(0);
    public NetworkVariable<bool> HasDashSkill = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            SetNewRandomThreshold();

        CurrentXP.OnValueChanged += OnXPChanged;
    }

    public override void OnNetworkDespawn()
    {
        CurrentXP.OnValueChanged -= OnXPChanged;
    }

    private void SetNewRandomThreshold()
    {
        if (!IsServer) return;
        XPThreshold.Value = Random.Range(MinThreshold, MaxThreshold + 1);
    }

    public void AwardXP(int amount)
    {
        if (!IsServer) return;
        CurrentXP.Value += amount;
    }

    private void OnXPChanged(int oldVal, int newVal)
    {
        if (!IsServer) return;

        if (newVal >= XPThreshold.Value)
        {
            CurrentXP.Value = newVal - XPThreshold.Value;
            SkillPoints.Value += 1;
            SetNewRandomThreshold();
        }
    }

    
    [ServerRpc(RequireOwnership = false)]
    public void BuyDashSkillServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        if (SkillPoints.Value > 0 && !HasDashSkill.Value)
        {
            SkillPoints.Value -= 1;
            HasDashSkill.Value = true;
        }
    }
}
