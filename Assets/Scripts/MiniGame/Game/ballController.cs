using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class BallController : MonoBehaviour
{
    public float moveSpeed = 10f;
    [HideInInspector] public BallSpawner spawner;

    [Header("Particle Prefabs")]
    public GameObject bombParticlePrefab;
    public GameObject powerParticlePrefab;
    public GameObject normalPopParticlePrefab;

    private Rigidbody rb;
    private bool isPlaced = false;
    private bool hasTriggeredPower = false;
    private bool isBomb = false;

    public bool IsPlaced => isPlaced;

    private void OnEnable()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        isPlaced = false;
        hasTriggeredPower = false;

        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotation;

        isBomb = CompareTag("Bomb");
    }

    private void Update()
    {
        if (isPlaced) return;

        if (Input.GetMouseButton(0))
            MoveWithMouse();

        if (Input.GetMouseButtonUp(0))
            ReleaseBall();
    }

    private void MoveWithMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        Vector3 targetPos = transform.position;
        targetPos.x = worldPos.x;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
    }

    private void ReleaseBall()
    {
        isPlaced = true;
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        if (isBomb)
        {
            StartCoroutine(BombCountdown());
        }

        if (spawner != null)
            spawner.SpawnBallDelayed(1f);
    }

    private IEnumerator BombCountdown()
    {
        yield return new WaitForSeconds(2f);
        ExplodeBomb();
    }

    private void ExplodeBomb()
    {
        float explosionRadius = 2f;

        transform.DOShakeScale(0.3f, 0.5f, 8, 90).OnComplete(() =>
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
            int poppedCount = 0;

            foreach (Collider hit in hits)
            {
                // ✅ Use BallData instead of "Ball" tag
                BallData bd = hit.GetComponent<BallData>();
                if (bd != null && !hit.CompareTag("Bomb") && !hit.CompareTag("PowerBall"))
                {
                    poppedCount++;
                    hit.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f, 5, 0.7f)
                        .OnComplete(() =>
                        {
                            if (bombParticlePrefab != null)
                                Instantiate(bombParticlePrefab, hit.transform.position, Quaternion.identity);
                            hit.gameObject.SetActive(false);
                        });
                }
            }

            if (Audio.Instance != null)
                Audio.Instance.PlaySFX(Audio.Instance.bomb);

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(poppedCount * 5);

            if (bombParticlePrefab != null)
                Instantiate(bombParticlePrefab, transform.position, Quaternion.identity);

            gameObject.SetActive(false);
        });
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isPlaced = true;
            return;
        }

        // ✅ Check by BallData component instead of tag
        BallData otherBallData = collision.gameObject.GetComponent<BallData>();
        if (otherBallData != null)
        {
            if (CompareTag("PowerBall"))
            {
                TriggerPowerEffect(collision.gameObject);
            }
            else
            {
                CheckClusterAndPop();
            }
        }
    }

    private void TriggerPowerEffect(GameObject firstHitBall)
    {
        if (hasTriggeredPower) return;
        hasTriggeredPower = true;

        // ✅ Play power sound
        if (Audio.Instance != null)
            Audio.Instance.PlaySFX(Audio.Instance.power);

        BallData hitBallData = firstHitBall.GetComponent<BallData>();
        if (hitBallData == null) return;

        string targetColor = hitBallData.colorID;
        int poppedCount = 0;

        GameObject[] allBalls = GameObject.FindGameObjectsWithTag("Ball");
        foreach (GameObject b in allBalls)
        {
            BallData bd = b.GetComponent<BallData>();
            if (bd != null && bd.colorID == targetColor)
            {
                poppedCount++;

                b.transform.DOScale(b.transform.localScale * 1.3f, 0.15f)
                    .SetLoops(2, LoopType.Yoyo)
                    .OnComplete(() =>
                    {
                        if (powerParticlePrefab != null)
                            Instantiate(powerParticlePrefab, b.transform.position, Quaternion.identity);

                        b.SetActive(false);
                    });
            }
        }

        transform.DOScale(transform.localScale * 1.4f, 0.2f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                if (powerParticlePrefab != null)
                    Instantiate(powerParticlePrefab, transform.position, Quaternion.identity);

                gameObject.SetActive(false);
            });

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(poppedCount * 5);
    }

    private void CheckClusterAndPop()
    {
        BallData ballData = GetComponent<BallData>();
        if (ballData == null) return;

        List<GameObject> connectedBalls = FindConnectedBalls(ballData.colorID);

        if (connectedBalls.Count >= 3)
        {
            // ✅ Play pop sound
            if (Audio.Instance != null)
                Audio.Instance.PlaySFX(Audio.Instance.pop);

            foreach (GameObject b in connectedBalls)
            {
                b.transform.DOMove(b.transform.position, 0.2f).SetEase(Ease.InSine);
                b.transform.DOScale(b.transform.localScale * 0.0f, 0.2f)

                
                    .OnComplete(() =>
                    {
                        if (normalPopParticlePrefab != null)
                            Instantiate(normalPopParticlePrefab, b.transform.position, Quaternion.identity);

                        b.SetActive(false);
                    });
            }

            

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(connectedBalls.Count * 5);
        }
    }

    private List<GameObject> FindConnectedBalls(string colorID)
    {
        List<GameObject> result = new List<GameObject>();
        Queue<GameObject> queue = new Queue<GameObject>();

        queue.Enqueue(gameObject);
        result.Add(gameObject);

        while (queue.Count > 0)
        {
            GameObject current = queue.Dequeue();

            Collider[] hits = Physics.OverlapSphere(current.transform.position, 1f);
            foreach (Collider hit in hits)
            {
                // ✅ Use BallData component instead of "Ball" tag
                BallData otherData = hit.GetComponent<BallData>();
                if (otherData != null && !result.Contains(hit.gameObject))
                {
                    if (otherData.colorID == colorID)
                    {
                        result.Add(hit.gameObject);
                        queue.Enqueue(hit.gameObject);
                    }
                }
            }
        }
        return result;
    }
}
