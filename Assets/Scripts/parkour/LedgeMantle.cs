
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class LedgeMantle : MonoBehaviour
{
    [Header("Ledge Detection")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("detection distance")]
    [SerializeField] private float ledgeDetectionDistance = 2f;
    [Tooltip("maximum ledge height from player (max height player can mantle")]
    [SerializeField] private float maxLedgeHeight = 10f;
    [Tooltip("minimum ledge height (height difference between player and ledge must be at least this value)")]
    [SerializeField] private float minLedgeHeight = 0f;
    [SerializeField]
    private float maximumMantleFallSpeed = 3f;

    [Header("Mantling")]
    [SerializeField, Range(0, 20)]
    private float climbSpeed = 5.0f;
    private Vector3 approachVelocity;
    [SerializeField, Range(0f, 1f)]
    private float momentumRetention = 0.8f;

    [SerializeField] private float minimumMantleSpeed = 1.0f;
    [SerializeField] private float approachVelocityInfluence = 0.6f;
    [SerializeField] private float mantleYOffset = 0.3f;
    [SerializeField] private float mantleForwardOffset = 0.5f;
    [SerializeField] private float mantleArcHeight = 1f;


    [SerializeField] private float mantleCooldown = 0.5f;

    // Component references
    private PlayerMovement movement;
    private Rigidbody rBody;
    private CapsuleCollider collider;
    private bool hasLandingPosition;
    
    private bool climbing;
    public bool IsMantling => climbing;
    private RaycastHit hitInfo;
    private float waitTime;
    
    private Vector3 startPos;
    private Vector3 controlPoint;
    private Vector3 landingPosition;
    
    private float mantleDuration;
    private float mantleTimer;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        rBody = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
    }

    private void FixedUpdate()
    {
        if (climbing)
        {
            mantleTimer += Time.fixedDeltaTime;
            
            float t = Mathf.Clamp01(mantleTimer / mantleDuration);
            float u = 1 - t;
            
            Vector3 nextPos = u * u * startPos + 
                              2 * u * t * controlPoint +
                              t * t * landingPosition;
            
            rBody.useGravity = false;

            rBody.MovePosition(nextPos);

            if (t >= 1)
            {
                Vector3 mantleDirection = landingPosition - startPos;
                mantleDirection.y = 0f;
                mantleDirection.Normalize();

                float exitSpeed = approachVelocity.magnitude * momentumRetention;

                Vector3 exitVelocity = mantleDirection * exitSpeed;

                rBody.linearVelocity = new Vector3(
                    exitVelocity.x,
                    0f,
                    exitVelocity.z
                );
            
                waitTime = mantleCooldown;
                climbing = false;
                rBody.useGravity = true;
                Debug.Log("Mantling finished");
            }

            return;
        }
        
        if (waitTime > 0f)
        {
            waitTime -= Time.deltaTime;
            return;
        }
        
        if (!movement.Airborne)
        {
            return;
        }
        
        if (rBody.linearVelocity.y < -maximumMantleFallSpeed)
        {
            return;
        }
        
        bool hit = Physics.Raycast(
            transform.position,
            transform.forward,
            out hitInfo,
            ledgeDetectionDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (!hit)
        {
            return;
        }
        if (!hitInfo.collider.CompareTag("Mantle") && !hitInfo.collider.CompareTag("WallrunMantle"))
        {
            return;
        }

        float bottom = collider.bounds.min.y;
        float platformTop = hitInfo.collider.bounds.max.y;
            
        float heightDifference = platformTop - bottom;
        float landingY = transform.position.y + heightDifference + mantleYOffset;

        Vector3 landingXZ = hitInfo.point + transform.forward * (collider.radius + mantleForwardOffset);
        
        if (heightDifference <= maxLedgeHeight && heightDifference > minLedgeHeight)
        {
            startPos = transform.position;
            landingPosition = new Vector3(landingXZ.x, landingY, landingXZ.z);
            hasLandingPosition = true;
            
                    
            controlPoint = new Vector3(
                transform.position.x,
                landingPosition.y + mantleArcHeight,
                transform.position.z
            );
            
            Debug.Log("Climbing");
            climbing = true;
            rBody.useGravity = false;
            Vector3 velocity = rBody.linearVelocity;
            approachVelocity = new  Vector3(velocity.x, 0, velocity.z);

            float mantleSpeed = climbSpeed + (approachVelocity.magnitude * approachVelocityInfluence);
            mantleSpeed = Mathf.Max(minimumMantleSpeed, mantleSpeed);
            float mantleDistance = Vector3.Distance(transform.position, landingPosition);
            
            mantleDuration = mantleDistance / mantleSpeed;
            rBody.linearVelocity = Vector3.zero;
            mantleTimer = 0f;
            Debug.Log("Mantling started");
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the forward ledge-detection ray.
        Gizmos.color = Color.blue;

        Gizmos.DrawRay(
            transform.position,
            transform.forward * ledgeDetectionDistance
        );

        // Only draw a landing point after one has been calculated.
        if (!Application.isPlaying || !hasLandingPosition)
        {
            return;
        }

        // Draw the calculated landing position.
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(landingPosition, 0.2f);

        // Draw a line from the player to the landing position.
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, landingPosition);
    }
}
