using UnityEngine;

public class GrenadeThrow : MonoBehaviour
{
    [Header("Grenade Settings")]
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform throwPoint;

    [Header("Throw Settings")]
    [SerializeField] private float minThrowForce = 5f;
    [SerializeField] private float maxThrowForce = 25f;
    [SerializeField] private float chargeRate = 10f;
    [SerializeField] private float throwAngle = 45f;

    [Header("Trajectory Settings")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryResolution = 30;
    [SerializeField] private float trajectoryTimeStep = 0.1f;

    private float currentChargeForce = 0f;
    private bool isCharging = false;

    void Start()
    {
        // Setup trajectory line renderer
        if (trajectoryLine == null)
        {
            GameObject lineObj = new GameObject("TrajectoryLine");
            lineObj.transform.SetParent(transform);
            trajectoryLine = lineObj.AddComponent<LineRenderer>();
            trajectoryLine.startWidth = 0.05f;
            trajectoryLine.endWidth = 0.05f;
            trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
            trajectoryLine.startColor = Color.white;
            trajectoryLine.endColor = Color.red;
        }

        trajectoryLine.enabled = false;

        // Setup throw point if not assigned
        if (throwPoint == null)
        {
            GameObject throwPointObj = new GameObject("ThrowPoint");
            throwPointObj.transform.SetParent(transform);
            throwPointObj.transform.localPosition = Vector3.forward + Vector3.up;
            throwPoint = throwPointObj.transform;
        }
    }

    void Update()
    {
        // Start charging when G is pressed
        if (Input.GetKeyDown(KeyCode.G))
        {
            isCharging = true;
            currentChargeForce = minThrowForce;
            trajectoryLine.enabled = true;
        }

        // Continue charging while G is held
        if (Input.GetKey(KeyCode.G) && isCharging)
        {
            currentChargeForce += chargeRate * Time.deltaTime;
            currentChargeForce = Mathf.Clamp(currentChargeForce, minThrowForce, maxThrowForce);

            // Update trajectory preview
            DrawTrajectory();
        }

        // Throw grenade when G is released
        if (Input.GetKeyUp(KeyCode.G) && isCharging)
        {
            ThrowGrenade();
            isCharging = false;
            trajectoryLine.enabled = false;
        }
    }

    void ThrowGrenade()
    {
        // Instantiate grenade
        GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, Quaternion.identity);

        // Get or add Rigidbody
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = grenade.AddComponent<Rigidbody>();
        }

        // Calculate throw velocity
        Vector3 velocity = CalculateThrowVelocity(currentChargeForce);

        // Apply velocity to grenade
        rb.linearVelocity = velocity;

        // Reset charge
        currentChargeForce = 0f;
    }

    Vector3 CalculateThrowVelocity(float force)
    {
        // Convert angle to radians
        float angleRad = throwAngle * Mathf.Deg2Rad;

        // Get throw direction (forward from player)
        Vector3 forward = transform.forward;

        // Calculate velocity components
        float vx = force * Mathf.Cos(angleRad);
        float vy = force * Mathf.Sin(angleRad);

        // Combine into velocity vector
        Vector3 velocity = forward * vx + Vector3.up * vy;

        return velocity;
    }

    void DrawTrajectory()
    {
        Vector3[] points = new Vector3[trajectoryResolution];

        // Starting position and velocity
        Vector3 startPos = throwPoint.position;
        Vector3 velocity = CalculateThrowVelocity(currentChargeForce);

        // Calculate trajectory points using projectile motion equations
        for (int i = 0; i < trajectoryResolution; i++)
        {
            float time = i * trajectoryTimeStep;

            // Projectile motion equations
            // x(t) = x0 + vx * t
            // y(t) = y0 + vy * t - 0.5 * g * t^2
            Vector3 point = startPos + velocity * time;
            point.y += 0.5f * Physics.gravity.y * time * time;

            points[i] = point;

            // Stop drawing if trajectory goes below ground (adjust as needed)
            if (point.y < 0f)
            {
                // Fill remaining points with last valid point
                for (int j = i; j < trajectoryResolution; j++)
                {
                    points[j] = point;
                }
                break;
            }
        }

        trajectoryLine.positionCount = trajectoryResolution;
        trajectoryLine.SetPositions(points);
    }
}