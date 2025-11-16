using UnityEngine;
using UnityEngine.Events;

public class InteractiveLever : MonoBehaviour
{
    [Header("Lever Rotation Settings")]
    [SerializeField] private float minAngle = -60f;
    [SerializeField] private float maxAngle = 60f;
    [SerializeField] private Vector3 rotationAxis = Vector3.right;
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private bool smoothReturn = true;
    [SerializeField] private float returnSpeed = 3f;
    [SerializeField] private float centerAngle = 0f;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private KeyCode grabKey = KeyCode.E;
    [SerializeField] private bool requirePlayerNearby = true;
    
    [Header("Activation Zones")]
    [SerializeField] private bool useActivationZones = true;
    [SerializeField] private float activationThresholdMin = 40f;
    [SerializeField] private float activationThresholdMax = 60f;
    
    [Header("Events")]
    public UnityEvent<float> OnLeverMoved;
    public UnityEvent OnLeverActivated;
    public UnityEvent OnLeverDeactivated;
    public UnityEvent OnLeverReleased;
    public UnityEvent OnLeverGrabbed;
    
    private bool isGrabbed = false;
    private float currentAngle = 0f;
    private Quaternion initialRotation;
    private Transform playerTransform;
    private bool playerInRange = false;
    private Camera mainCamera;
    private bool wasActivated = false;

    private void Start()
    {
        initialRotation = transform.localRotation;
        mainCamera = Camera.main;
        currentAngle = centerAngle;
        
        if (requirePlayerNearby)
        {
            FindPlayer();
        }
        
        UpdateLeverRotation();
    }

    private void Update()
    {
        if (requirePlayerNearby)
        {
            CheckPlayerDistance();
        }
        
        HandleInput();
        
        if (isGrabbed)
        {
            HandleMouseDrag();
        }
        else if (smoothReturn)
        {
            ReturnToCenter();
        }
        
        CheckActivationZones();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void CheckPlayerDistance()
    {
        if (playerTransform == null) return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        playerInRange = distance <= interactionDistance;
    }

    private void HandleInput()
    {
        bool canInteract = !requirePlayerNearby || playerInRange;
        
        if (Input.GetKeyDown(grabKey) && canInteract)
        {
            isGrabbed = true;
            OnLeverGrabbed?.Invoke();
        }
        
        if (Input.GetKeyUp(grabKey) && isGrabbed)
        {
            isGrabbed = false;
            OnLeverReleased?.Invoke();
        }
    }

    private void HandleMouseDrag()
    {
        float mouseDelta = Input.GetAxis("Mouse Y");
        currentAngle += mouseDelta * sensitivity;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        
        UpdateLeverRotation();
        OnLeverMoved?.Invoke(currentAngle);
    }

    private void ReturnToCenter()
    {
        if (Mathf.Abs(currentAngle - centerAngle) > 0.1f)
        {
            currentAngle = Mathf.Lerp(currentAngle, centerAngle, Time.deltaTime * returnSpeed);
            UpdateLeverRotation();
            OnLeverMoved?.Invoke(currentAngle);
        }
    }

    private void UpdateLeverRotation()
    {
        Quaternion targetRotation = initialRotation * Quaternion.Euler(rotationAxis * currentAngle);
        transform.localRotation = targetRotation;
    }

    private void CheckActivationZones()
    {
        if (!useActivationZones) return;
        
        bool isActivated = currentAngle >= activationThresholdMin && currentAngle <= activationThresholdMax;
        
        if (isActivated && !wasActivated)
        {
            OnLeverActivated?.Invoke();
            wasActivated = true;
        }
        else if (!isActivated && wasActivated)
        {
            OnLeverDeactivated?.Invoke();
            wasActivated = false;
        }
    }

    public void SetAngle(float angle)
    {
        currentAngle = Mathf.Clamp(angle, minAngle, maxAngle);
        UpdateLeverRotation();
        OnLeverMoved?.Invoke(currentAngle);
    }

    public float GetAngle()
    {
        return currentAngle;
    }

    public float GetNormalizedAngle()
    {
        return Mathf.InverseLerp(minAngle, maxAngle, currentAngle);
    }

    public bool IsGrabbed()
    {
        return isGrabbed;
    }

    public bool IsActivated()
    {
        return wasActivated;
    }

    private void OnDrawGizmosSelected()
    {
        if (requirePlayerNearby)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
        
        Gizmos.color = Color.green;
        Vector3 direction = transform.TransformDirection(rotationAxis);
        Gizmos.DrawRay(transform.position, direction * 0.5f);
    }
}
