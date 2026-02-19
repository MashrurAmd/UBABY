using System.Collections;
using UnityEngine;

public class CatDamageController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public PlayerManager playerManager;

    [Header("Health")]
    public int maxHealth = 10;
    public float slowTapTime = 0.6f;

    private int currentHealth;
    private float lastTapTime;
    private bool isDead;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

#if UNITY_EDITOR || UNITY_WEBGL
        if (Input.GetMouseButtonDown(0))
            HandleInput(Input.mousePosition);
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            HandleInput(Input.GetTouch(0).position);
#endif
    }

    void HandleInput(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 100f)) return;

        float tapGap = Time.time - lastTapTime;
        lastTapTime = Time.time;
        bool slowTap = tapGap > slowTapTime;

        if (slowTap)
        {
            Heal();
            return;
        }

        if (hit.collider.CompareTag("Head"))
            PlayHit("HeadHit");
        else if (hit.collider.CompareTag("BodyHit"))
            PlayHit("BodyHit");
        else if (hit.collider.CompareTag("Leg"))
            PlayHit("LegHit");
    }

    void PlayHit(string trigger)
    {
        if (isDead) return;

        animator.ResetTrigger("HeadHit");
        animator.ResetTrigger("BodyHit");
        animator.ResetTrigger("LegHit");

        animator.SetTrigger(trigger);
        currentHealth--;

        if (currentHealth <= 0)
            StartCoroutine(DieRoutine());
    }

    void Heal()
    {
        if (currentHealth < maxHealth)
            currentHealth++;

        animator.SetTrigger("BodyHit");
    }

    IEnumerator DieRoutine()
    {
        isDead = true;
        animator.SetTrigger("Die");

        yield return new WaitForSeconds(1f);

        currentHealth = maxHealth;
        animator.Play("Idle", 0, 0f);
        isDead = false;
    }
}
