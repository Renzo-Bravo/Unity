using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LeverInputSystem : MonoBehaviour
{
    [Header("Lever Settings")]
    [SerializeField] private float activatedAngle = -45f;
    [SerializeField] private float deactivatedAngle = 45f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Vector3 rotationAxis = Vector3.right;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private string interactActionName = "Interact";
    
    [Header("Events")]
    public UnityEvent OnLeverActivated;
    public UnityEvent OnLeverDeactivated;
    public UnityEvent OnLeverToggled;
    
    private bool isActivated = false;
    private Quaternion targetRotation;
    private Quaternion initialRotation;
    private Transform playerTransform;
    private bool playerInRange = false;
    private InputAction interactAction;

    private void Start()
    {
        initialRotation = transform.localRotation;
        targetRotation = initialRotation * Quaternion.Euler(rotationAxis * deactivatedAngle);
        transform.localRotation = targetRotation;
        
        FindPlayer();
        SetupInput();
    }

    private void SetupInput()
    {
        var playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
        {
            interactAction = playerInput.actions[interactActionName];
            if (interactAction != null)
            {
                interactAction.performed += OnInteractPerformed;
            }
        }
    }

    private void OnDestroy()
    {
        if (interactAction != null)
        {
            interactAction.performed -= OnInteractPerformed;
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (playerInRange)
        {
            ToggleLever();
        }
    }

    private void Update()
    {
        CheckPlayerDistance();
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
