using System.Collections;
using UnityEngine;

public class GrenadeThrow : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Grenade Settings")]
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform throwPoint;

    [Header("Throw Settings")]
    [SerializeField] private float minThrowForce = 3f;
    [SerializeField] private float maxThrowForce = 10f;
    [SerializeField] private float chargeRate = 20f;
    [SerializeField] private float throwAngle = 30f;

    [Header("Trajectory Settings")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryResolution = 50;
    [SerializeField] private float trajectoryTimeStep = 0.05f;
    [SerializeField] private LayerMask obstacleLayer = -1; // Hit everything by default

    [Header("Line Style Settings")]
    [SerializeField] private float lineWidth = 0.05f; // Much thinner now!
    [SerializeField] private Color lineStartColor = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField] private Color lineEndColor = new Color(1f, 1f, 1f, 0.3f);



    private float currentChargeForce = 0f;
    private bool isCharging = false;

    int grenadeLayerIndex;

    [SerializeField]FreeLookADS  freeLookAds;

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
        grenadeLayerIndex = animator.GetLayerIndex("Grenade Layer");
    }

    void Start()
    {
        

        // Setup trajectory line renderer
        if (trajectoryLine == null)
        {
            GameObject lineObj = new GameObject("TrajectoryLine");
            lineObj.transform.SetParent(transform); // Child of current GameObject
            lineObj.transform.localPosition = Vector3.zero;
            trajectoryLine = lineObj.AddComponent<LineRenderer>();

            //  renderer settings
            trajectoryLine.startWidth = lineWidth;
            trajectoryLine.endWidth = lineWidth * 0.5f; // Even thinner at the end
            trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
            trajectoryLine.startColor = lineStartColor;
            trajectoryLine.endColor = lineEndColor;
            trajectoryLine.numCornerVertices = 3;
            trajectoryLine.numCapVertices = 3;
            trajectoryLine.alignment = LineAlignment.View; // Always faces camera
            trajectoryLine.textureMode = LineTextureMode.Stretch;

            // Clean rendering
            trajectoryLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trajectoryLine.receiveShadows = false;
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
        // Start charging when RIGHT MOUSE is pressed
        if (Input.GetMouseButtonDown(1))
        {
            isCharging = true;
            currentChargeForce = minThrowForce;
            trajectoryLine.enabled = true;
        }

        // Continue charging while RIGHT MOUSE is held
        if (Input.GetMouseButton(1) && isCharging)
        {
            currentChargeForce += chargeRate * Time.deltaTime;
            currentChargeForce = Mathf.Clamp(currentChargeForce, minThrowForce, maxThrowForce);


            // Update trajectory preview
            DrawTrajectory();

            // Throw grenade when LEFT MOUSE is clicked while charging
            if (Input.GetMouseButtonDown(0))
            {
                ThrowGrenade();
                isCharging = false;
                trajectoryLine.enabled = false;

                StartCoroutine(PlayThrowAnimation());
            }

            if (freeLookAds)
            {
            freeLookAds.SetADSState();

            }

        }

        // Cancel throw if RIGHT MOUSE is released without throwing
        if (Input.GetMouseButtonUp(1) && isCharging)
        {
            CancelThrow();

            if (freeLookAds != null)
            {
                freeLookAds.SetNormalState();

            }
        }
    }

    IEnumerator PlayThrowAnimation()
    {
        animator.SetLayerWeight(grenadeLayerIndex, 1f);
        yield return new WaitForSeconds(0.5f);
        //animator.SetTrigger("GrenadeThrow");
        yield return new WaitForSeconds(1f);
        animator.SetLayerWeight(grenadeLayerIndex, 0f);
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

    void CancelThrow()
    {
        // Hide trajectory line
        trajectoryLine.enabled = false;

        // Reset charging state
        isCharging = false;
        currentChargeForce = 0f;
    }

    Vector3 CalculateThrowVelocity(float force)
    {
        // Convert angle to radians
        float angleRad = throwAngle * Mathf.Deg2Rad;

        // Get throw direction (forward from player/camera)
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

        Vector3 previousPoint = startPos;
        int actualPointCount = 0;
        bool hitObstacle = false;

        // Calculate trajectory points using projectile motion equations
        for (int i = 0; i < trajectoryResolution; i++)
        {
            float time = i * trajectoryTimeStep;

            // Projectile motion equations
            Vector3 point = startPos + velocity * time;
            point.y += 0.5f * Physics.gravity.y * time * time;

            // Check if trajectory hits an obstacle
            if (i > 0)
            {
                Vector3 direction = point - previousPoint;
                float distance = direction.magnitude;

                RaycastHit hit;
                if (Physics.Raycast(previousPoint, direction.normalized, out hit, distance, obstacleLayer))
                {
                    // Hit an obstacle! Stop the trajectory here
                    points[i] = hit.point;
                    actualPointCount = i + 1;
                    hitObstacle = true;
                    break;
                }
            }

            points[i] = point;
            previousPoint = point;
            actualPointCount++;

            // Stop drawing if trajectory goes below ground
            if (point.y < 0.1f)
            {
                break;
            }
        }

        // Only set the points we actually need
        trajectoryLine.positionCount = actualPointCount;
        trajectoryLine.SetPositions(points);
    }
}