using UnityEngine;
public class ShieldPickup : MonoBehaviour
{
    [Header("Settings")]
    public float shieldDuration = 10f;
    public float rotateSpeed = 120f;
    private GameUI gameUI;
    void Start()
    {
        gameUI = FindObjectOfType<GameUI>();
    }
    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerCarController player =
                other.GetComponent<PlayerCarController>();
            if (player != null)
            {
                player.ActivateShield(shieldDuration);
                if (gameUI != null)
                    gameUI.AddScore(15f);
                Debug.Log("Shield activated! Duration: " + shieldDuration);
                Destroy(gameObject);
            }
        }
    }
}