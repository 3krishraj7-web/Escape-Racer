using UnityEngine;

public class FuelCan : MonoBehaviour
{
    [Header("Settings")]
    public float fuelAmount = 30f;
    public float rotateSpeed = 90f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;
    private Vector3 startPosition;
    private GameUI gameUI;
    void Start()
    {
        startPosition = transform.position;
        gameUI = FindObjectOfType<GameUI>();
    }
    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
        float newY = startPosition.y +
            Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerCarController player =
                other.GetComponent<PlayerCarController>();
            if (player != null)
            { 
                player.fuel += fuelAmount;
                player.fuel = Mathf.Clamp(
                    player.fuel, 0, player.maxFuel);
                // Add score
                if (gameUI != null)
                    gameUI.AddScore(25f);
                Debug.Log("Fuel collected! +" + fuelAmount);
                Destroy(gameObject);
            }
        }
    }
}