using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerExperience))]
public class PlayerDash : NetworkBehaviour
{
    [Header("Параметры рывка")]
    [Tooltip("Сила импульса рывка")]
    public float dashForce = 8f;

    [Tooltip("Время ожидания между рывками (сек)")]
    public float cooldown = 4f;

    private Rigidbody rb;
    private PlayerExperience exp;
    private bool canDash = true;
    private Animator animator;
    private float[] lastTapTime = new float[4];
        
    public bool IsCurrentlyDashing { get; private set; }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        exp = GetComponent<PlayerExperience>();

        for (int i = 0; i < lastTapTime.Length; i++)
            lastTapTime[i] = -1f;

        IsCurrentlyDashing = false;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (exp == null || !exp.HasDashSkill.Value) return;
        if (!canDash) return;

        CheckDoubleTap(KeyCode.W, transform.forward, 0);
        CheckDoubleTap(KeyCode.S, -transform.forward, 1);
        CheckDoubleTap(KeyCode.A, -transform.right, 2);
        CheckDoubleTap(KeyCode.D, transform.right, 3);
    }

    private void CheckDoubleTap(KeyCode key, Vector3 dir, int idx)
    {
        if (Input.GetKeyDown(key))
        {
            float t = Time.time;
            if (t - lastTapTime[idx] < 0.3f)
            {
                lastTapTime[idx] = -1f;
                DoDash(dir);
            }
            else
            {
                lastTapTime[idx] = t;
            }
        }
    }

    private void DoDash(Vector3 direction)
    {
        animator.SetBool("IsDashing", true);
        canDash = false;
        IsCurrentlyDashing = true;

        Vector3 flatVel = rb.linearVelocity;
        flatVel.y = 0f;
        rb.linearVelocity = flatVel;

        rb.AddForce(direction.normalized * dashForce, ForceMode.VelocityChange);

        StartCoroutine(EndDash());
 
        StartCoroutine(ResetCooldown());
    }

    private IEnumerator EndDash()
    {
       
        yield return new WaitForSeconds(0.15f);
        IsCurrentlyDashing = false;

        // Vector3 vel = rb.velocity; vel.x = 0; vel.z = 0; rb.velocity = vel;
    }

    private IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(cooldown);
        animator.SetBool("IsDashing", false);
        canDash = true;
    }
}
