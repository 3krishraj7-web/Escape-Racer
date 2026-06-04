using UnityEngine;
public class TrafficCar : MonoBehaviour
{
    [Header("Traffic Settings")]
    public float minSpeed = 8f;
    public float maxSpeed = 15f;
    public bool isOncoming = true;
    private Rigidbody rb;
    private float speed;
    private Transform player;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        speed = Random.Range(minSpeed, maxSpeed);
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (isOncoming)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
    void FixedUpdate()
    {
        rb.linearVelocity = transform.forward * speed;
        if (player != null)
        {
            float distance = Vector3.Distance(
                transform.position, player.position);

            if (distance > 150f)
            {
                Destroy(gameObject);
            }
        }
    }
}