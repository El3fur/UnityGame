using UnityEngine;

public class ResourceHealth : MonoBehaviour
{
   
    [SerializeField]
    private int maxHealth = 5;

    private int currentHealth;

 
    public InventoryItem dropItem;

   
    public int minDrops = 2;

   
    public int maxDrops = 4;

  
    public float dropRadius = 0.5f;

    [SerializeField]
    private ParticleSystem breakParticles;

    private void Awake()
    {
        currentHealth = maxHealth;
    }


    public void TakeDamage(int damage)
    {
        if (damage <= 0)
            return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            BreakResource();
        }
    }

    private void BreakResource()
    {
        
        if (breakParticles != null)
        {
            Instantiate(breakParticles, transform.position, Quaternion.identity);
        }

        
        int dropsCount = Random.Range(minDrops, maxDrops + 1);

        for (int i = 0; i < dropsCount; i++)
        {
            
            Vector2 randomCircle = Random.insideUnitCircle * dropRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            
            if (dropItem != null && dropItem.worldPrefab != null)
            {
                GameObject go = Instantiate(dropItem.worldPrefab, spawnPos, Quaternion.identity);

                
                ItemPickup ip = go.GetComponent<ItemPickup>();
                if (ip == null)
                {
                    ip = go.AddComponent<ItemPickup>();
                }

                ip.itemData = dropItem;
                ip.amount = 1;

                
                Collider c = go.GetComponent<Collider>();
                if (c != null)
                    c.isTrigger = true;

                
                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.isKinematic = true;
            }
            else
            {
                Debug.LogWarning($"[ResourceHealth] dropItem или dropItem.worldPrefab не назначены на {gameObject.name}");
            }
        }

       
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dropRadius);
    }
}
