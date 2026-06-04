using UnityEngine;

public class PlayerCarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float acceleration = 15f;
    public float maxSpeed = 30f;
    public float reverseSpeed = 10f;
    public float steerSpeed = 100f;
    public float maxSteerAngle = 45f;
    public float brakeForce = 20f;
    [Header("Resources")]
    public float fuel = 100f;
    public float armor = 100f;
    public float maxFuel = 100f;
    public float maxArmor = 100f;
    public float fuelDepletionRate = 2f;
    [Header("Wall Damage")]
    public float wallDamageAmount = 10f;
    public float wallDamageCooldown = 0.5f;
    public float minSpeedForDamage = 2f;
    [Header("Shield")]
    public bool isShielded = false;
    public float shieldTimeRemaining = 0f;
    [Header("Game State")]
    public bool isAlive = true;
    private Rigidbody rb;
    private float currentSpeed = 0f;
    private float horizontalInput;
    private float verticalInput;
    private float lastDamageTime = -1f;
    private Renderer carRenderer;
    private Color originalColor;
    private GameObject shieldVisual;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        carRenderer = GetComponent<Renderer>();

        if (carRenderer != null)
            originalColor = carRenderer.material.color;
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 1000;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 5f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX
                           | RigidbodyConstraints.FreezeRotationZ;
        }
        CreateShieldVisual();
    }
    void CreateShieldVisual()
    {
        shieldVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shieldVisual.name = "ShieldVisual";
        shieldVisual.transform.SetParent(transform);
        shieldVisual.transform.localPosition = Vector3.zero;
        shieldVisual.transform.localScale = new Vector3(2.5f, 2f, 3.5f);
        Destroy(shieldVisual.GetComponent<Collider>());
        Renderer rend = shieldVisual.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0, 0.5f, 1f, 0.3f);
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.renderQueue = 3000;
            rend.material = mat;
        }
        shieldVisual.SetActive(false);
    }
    void Update()
    {
        if (!isAlive) return;
        horizontalInput = GetHorizontalInput();
        verticalInput = GetVerticalInput();
        fuel -= fuelDepletionRate * Time.deltaTime;
        fuel = Mathf.Clamp(fuel, 0, maxFuel);
        if (isShielded)
        {
            shieldTimeRemaining -= Time.deltaTime;
            if (shieldTimeRemaining <= 0)
            {
                DeactivateShield();
            }
        }
        if (fuel <= 0 || armor <= 0)
        {
            TriggerGameOver();
        }
    }
    void FixedUpdate()
    {
        if (!isAlive) return;

        if (fuel > 0)
        {
            HandleMovement();
            HandleSteering();
        }
        else
        {
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity, Vector3.zero,
                Time.fixedDeltaTime * 2f);
        }
    }
    void HandleMovement()
    {
        if (verticalInput > 0)
        {
            currentSpeed += acceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
        }
        else if (verticalInput < 0)
        {
            if (currentSpeed > 0)
            {
                currentSpeed -= brakeForce * Time.fixedDeltaTime;
                currentSpeed = Mathf.Max(currentSpeed, 0);
            }
            else
            {
                currentSpeed -= acceleration * Time.fixedDeltaTime;
                currentSpeed = Mathf.Clamp(currentSpeed, -reverseSpeed, 0);
            }
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.fixedDeltaTime * 2f);
        }
        rb.linearVelocity = transform.forward * currentSpeed;
    }
    void HandleSteering()
    {
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            float dir = currentSpeed > 0 ? 1 : -1;
            float steerAngle = horizontalInput * maxSteerAngle;
            transform.Rotate(0,
                steerAngle * steerSpeed * Time.fixedDeltaTime * dir, 0);
        }
    }
    public void ActivateShield(float duration)
    {
        isShielded = true;
        shieldTimeRemaining = duration;
        if (shieldVisual != null)
            shieldVisual.SetActive(true);
        if (carRenderer != null)
            carRenderer.material.color = Color.cyan;

        Debug.Log("Shield ON! Duration: " + duration);
    }
    void DeactivateShield()
    {
        isShielded = false;
        shieldTimeRemaining = 0;
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
        if (carRenderer != null)
            carRenderer.material.color = originalColor;

        Debug.Log("Shield OFF!");
    }
    public void TakeDamage(float damage)
    {
        if (!isAlive) return;
        if (isShielded)
        {
            Debug.Log("Shield blocked damage: " + damage);
            StartCoroutine(FlashCyan());
            return;
        }

        armor -= damage;
        armor = Mathf.Clamp(armor, 0, maxArmor);
        Debug.Log("Damage taken: " + damage + " Armor: " + armor);

        StartCoroutine(FlashRed());

        if (armor <= 0)
            TriggerGameOver();
    }
    void OnCollisionEnter(Collision collision)
    {
        if (!isAlive) return;
        if (Time.time - lastDamageTime < wallDamageCooldown) return;

        float speed = rb.linearVelocity.magnitude;
        if (speed < minSpeedForDamage) return;

        if (collision.gameObject.CompareTag("Wall"))
        {
            lastDamageTime = Time.time;
            TakeDamage(wallDamageAmount);
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            lastDamageTime = Time.time;
            TakeDamage(wallDamageAmount * 1.5f);
        }
        else if (collision.gameObject.CompareTag("Traffic"))
        {
            lastDamageTime = Time.time;
            TakeDamage(wallDamageAmount * 0.5f);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            lastDamageTime = Time.time;
            TakeDamage(wallDamageAmount);
        }
    }
    void TriggerGameOver()
    {
        if (!isAlive) return;

        isAlive = false;
        currentSpeed = 0;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        GameUI gameUI = FindObjectOfType<GameUI>();
        if (gameUI != null)
            gameUI.ShowGameOver();

        Debug.Log("GAME OVER!");
    }

    // Flash effects
    System.Collections.IEnumerator FlashRed()
    {
        if (carRenderer != null)
        {
            carRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            carRenderer.material.color = isShielded ? Color.cyan : originalColor;
        }
    }

    System.Collections.IEnumerator FlashCyan()
    {
        if (carRenderer != null)
        {
            carRenderer.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            carRenderer.material.color = Color.cyan;
        }
    }

    // Input
    float GetHorizontalInput()
    {
        float input = 0f;
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.aKey.isPressed ||
                UnityEngine.InputSystem.Keyboard.current.leftArrowKey.isPressed)
                input = -1f;
            if (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed ||
                UnityEngine.InputSystem.Keyboard.current.rightArrowKey.isPressed)
                input = 1f;
        }
        var gamepad = UnityEngine.InputSystem.Gamepad.current;
        if (gamepad != null)
            input = gamepad.leftStick.x.ReadValue();
#else
        input = Input.GetAxis("Horizontal");
#endif
        return input;
    }

    float GetVerticalInput()
    {
        float input = 0f;
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.wKey.isPressed ||
                UnityEngine.InputSystem.Keyboard.current.upArrowKey.isPressed)
                input = 1f;
            if (UnityEngine.InputSystem.Keyboard.current.sKey.isPressed ||
                UnityEngine.InputSystem.Keyboard.current.downArrowKey.isPressed)
                input = -1f;
        }
        var gamepad = UnityEngine.InputSystem.Gamepad.current;
        if (gamepad != null)
            input = gamepad.leftStick.y.ReadValue();
#else
        input = Input.GetAxis("Vertical");
#endif
        return input;
    }
}