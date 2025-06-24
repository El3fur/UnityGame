using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] private PlayerPunch _playerPunch;

    public void EndHit()
    {
        _playerPunch.EndHit();
    }

    public void CheckHit()
    {
        _playerPunch.CheckHit();
    }
}
