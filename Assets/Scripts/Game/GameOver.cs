using UnityEngine;

public class GameOverLine : MonoBehaviour
{
    public GameOverManager gameOverManager;
    public float stayDuration = 2f; // How long a ball can stay before game over

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") || 
            other.CompareTag("Bomb"))
        {
            // Start counting time
            StartCoroutine(CheckBallStay(other));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        StopAllCoroutines(); // Reset timer when ball leaves
    }

    private System.Collections.IEnumerator CheckBallStay(Collider ball)
    {
        yield return new WaitForSeconds(stayDuration);

        // If the ball is still inside after 2 seconds, trigger Game Over
        if (ball != null && ball.bounds.Intersects(GetComponent<Collider>().bounds))
        {
            Debug.Log("Game Over: Ball stayed too long!");
            if (gameOverManager != null)
            {
                gameOverManager.ShowGameOverPanel();
            }
        }
    }
}
