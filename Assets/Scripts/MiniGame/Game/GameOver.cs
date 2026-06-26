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

        if (ball != null && ball.bounds.Intersects(GetComponent<Collider>().bounds))
        {
            Debug.Log("Game Over: Ball stayed too long!");
        
            Debug.Log("gameOverManager is null: " + (gameOverManager == null)); // ✅
        
            if (gameOverManager != null)
            {
                Debug.Log("Calling ShowGameOverPanel..."); // ✅
                gameOverManager.ShowGameOverPanel();
                Debug.Log("ShowGameOverPanel called!"); // ✅
            }
            else
            {
                Debug.LogError("❌ gameOverManager is NULL - assign it in Inspector!"); // ✅
            }
        }
        else
        {
            Debug.Log("Ball left the trigger zone - no game over"); // ✅
        }
    }
}
