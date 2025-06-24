using UnityEngine;

[RequireComponent(typeof(HeldItemHandler))]
public class PlayerPunch : MonoBehaviour
{
    [SerializeField] private float sphereRadius = 0.5f;
    [SerializeField] private float sphereInFront = 1f;
    [SerializeField] private float sphereHeight = 0f;
    [SerializeField] private Animator animator;

    private bool _canHit = true;
    private HeldItemHandler heldItemHandler;

    private void Awake()
    {
        heldItemHandler = GetComponent<HeldItemHandler>();
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {

        if (UIManager.Instance != null && UIManager.Instance.IsUIOpen)
            return;

        
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (!_canHit)
            {
                Debug.Log("Can't hit yet");
                return;
            }

            
            if (heldItemHandler == null || heldItemHandler.currentHeldItem == null)
            {
                Debug.Log("Удар: в руке нет предмета.");
                return;
            }

            
            var it = heldItemHandler.currentHeldItem;
            if (it.type != InventoryItem.ItemType.Weapon && it.type != InventoryItem.ItemType.Tool)
            {
                Debug.Log($"Удар: предмет типа {it.type}, нельзя атаковать им.");
                return;
            }

           
            StartHit();
        }
    }

    private void StartHit()
    {
        _canHit = false;
        animator.SetTrigger("Hit");
        animator.SetTrigger("Attack");
    }

    public void EndHit()   
    {
        _canHit = true;
    }

    public void CheckHit() 
    {
        if (heldItemHandler == null || heldItemHandler.currentHeldItem == null)
            return;

        var it = heldItemHandler.currentHeldItem;
        int dmgToEnemies = it.damageToEnemies;
        int dmgToRes = it.damageToResources;

        Vector3 center = transform.position + transform.forward * sphereInFront + transform.up * sphereHeight;
        Collider[] colliders = Physics.OverlapSphere(center, sphereRadius);

        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent<EnemyHealth>(out var enemy))
            {
                if (dmgToEnemies > 0)
                    enemy.TakeDamage(dmgToEnemies);
            }
            else if (col.TryGetComponent<ResourceHealth>(out var res))
            {
                if (dmgToRes > 0)
                    res.TakeDamage(dmgToRes);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + transform.forward * sphereInFront + transform.up * sphereHeight;
        Gizmos.DrawWireSphere(center, sphereRadius);
    }
}
