using UnityEngine;
using UnityEngine.InputSystem;

public class LeverController : MonoBehaviour
{
    [Header("Lever Settings")]
    [SerializeField] private float maxRotationAngle = 45f;
    [SerializeField] private float minRotationAngle = -45f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float returnSpeed = 5f;
    [SerializeField] private bool autoReturn = false;

    [Header("Mouse Control")]
    [SerializeField] private float mouseSensitivity = 0.5f;

    private bool isDragging = false;
    private float currentRotation = 0f;
    private Camera mainCamera;
    private Collider leverCollider;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No se encontró una cámara principal en la escena.");
        }

        leverCollider = GetComponentInChildren<Collider>();
        if (leverCollider == null)
        {
            Debug.LogError("No se encontró un Collider en la palanca o sus hijos.");
        }
    }

    private void Update()
    {
        HandleMouseInput();
        UpdateLeverRotation();
    }

    private void HandleMouseInput()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (IsMouseOverLever())
            {
                isDragging = true;
            }
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            float rotationChange = -mouseDelta.y * mouseSensitivity * rotationSpeed;
            currentRotation = Mathf.Clamp(currentRotation + rotationChange, minRotationAngle, maxRotationAngle);
        }
        else if (autoReturn)
        {
            currentRotation = Mathf.Lerp(currentRotation, 0f, Time.deltaTime * returnSpeed);
        }
    }

    private bool IsMouseOverLever()
    {
        if (mainCamera == null || leverCollider == null) return false;

        Mouse mouse = Mouse.current;
        if (mouse == null) return false;

        Vector2 mousePosition = mouse.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider == leverCollider;
        }

        return false;
    }

    private void UpdateLeverRotation()
    {
        transform.localRotation = Quaternion.Euler(currentRotation, 0f, 0f);
    }

    public float GetNormalizedPosition()
    {
        float range = maxRotationAngle - minRotationAngle;
        return (currentRotation - minRotationAngle) / range;
    }

    public float GetCurrentRotation()
    {
        return currentRotation;
    }
}
