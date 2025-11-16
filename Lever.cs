using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    [Header("Lever Settings")]
    [SerializeField] private float activatedAngle = -45f;
    [SerializeField] private float deactivatedAngle = 45f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Vector3 rotationAxis = Vector3.right;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    
    [Header("Events")]
    public UnityEvent OnLeverActivated;
    public UnityEvent OnLeverDeactivated;
    public UnityEvent OnLeverToggled;
    
    private bool isActivated = false;
    private Quaternion targetRotation;
    private Quaternion initialRotation;
    private Transform playerTransform;
    private bool playerInRange = false;

    private void Start()
    {
        initialRotation = transform.localRotation;
        targetRotation = initialRotation * Quaternion.Euler(rotationAxis * deactivatedAngle);
        transform.localRotation = targetRotation;
        
        FindPlayer();
    }

    private void Update()
    {
        CheckPlayerDistance();
        HandleInput();
        UpdateRotation();
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
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            ToggleLever();
        }
    }

    public void ToggleLever()
    {
        isActivated = !isActivated;
        
        float angle = isActivated ? activatedAngle : deactivatedAngle;
        targetRotation = initialRotation * Quaternion.Euler(rotationAxis * angle);
        
        OnLeverToggled?.Invoke();
        
        if (isActivated)
        {
            OnLeverActivated?.Invoke();
        }
        else
        {
            OnLeverDeactivated?.Invoke();
        }
    }

    public void SetLeverState(bool activated)
    {
        if (isActivated != activated)
        {
            ToggleLever();
        }
    }

    private void UpdateRotation()
    {
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation, 
            targetRotation, 
            Time.deltaTime * rotationSpeed
        );
    }

    public bool IsActivated()
    {
        return isActivated;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
