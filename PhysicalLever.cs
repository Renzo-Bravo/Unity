using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(HingeJoint))]
public class PhysicalLever : MonoBehaviour
{
    [Header("Lever Settings")]
    [SerializeField] private float minAngle = -60f;
    [SerializeField] private float maxAngle = 60f;
    [SerializeField] private float springForce = 50f;
    [SerializeField] private float damping = 10f;
    [SerializeField] private bool autoReturn = true;
    [SerializeField] private float targetReturnAngle = 0f;
    
    [Header("Activation Zones")]
    [SerializeField] private bool useActivationZones = true;
    [SerializeField] private float activationThresholdMin = 40f;
    [SerializeField] private float activationThresholdMax = 60f;
    
    [Header("Events")]
    public UnityEvent<float> OnLeverMoved;
    public UnityEvent OnLeverActivated;
    public UnityEvent OnLeverDeactivated;
    
    private HingeJoint hingeJoint;
    private bool wasActivated = false;
    private float previousAngle = 0f;

    private void Awake()
    {
        SetupHingeJoint();
    }

    private void SetupHingeJoint()
    {
        hingeJoint = GetComponent<HingeJoint>();
        
        if (hingeJoint == null)
        {
            hingeJoint = gameObject.AddComponent<HingeJoint>();
        }
        
        JointLimits limits = new JointLimits
        {
            min = minAngle,
            max = maxAngle
        };
        hingeJoint.limits = limits;
        hingeJoint.useLimits = true;
        
        if (autoReturn)
        {
            JointSpring spring = new JointSpring
            {
                spring = springForce,
                damper = damping,
                targetPosition = targetReturnAngle
            };
            hingeJoint.useSpring = true;
            hingeJoint.spring = spring;
        }
        else
        {
            hingeJoint.useSpring = false;
        }
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.mass = 1f;
        rb.angularDamping = 0.5f;
    }

    private void Update()
    {
        float currentAngle = hingeJoint.angle;
        
        if (Mathf.Abs(currentAngle - previousAngle) > 0.01f)
        {
            OnLeverMoved?.Invoke(currentAngle);
            previousAngle = currentAngle;
        }
        
        CheckActivationZones(currentAngle);
    }

    private void CheckActivationZones(float angle)
    {
        if (!useActivationZones) return;
        
        bool isActivated = angle >= activationThresholdMin && angle <= activationThresholdMax;
        
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

    public void SetSpringEnabled(bool enabled)
    {
        autoReturn = enabled;
        
        if (hingeJoint != null)
        {
            hingeJoint.useSpring = enabled;
            
            if (enabled)
            {
                JointSpring spring = hingeJoint.spring;
                spring.spring = springForce;
                spring.damper = damping;
                spring.targetPosition = targetReturnAngle;
                hingeJoint.spring = spring;
            }
        }
    }

    public void SetSpringForce(float force)
    {
        springForce = force;
        
        if (hingeJoint != null && autoReturn)
        {
            JointSpring spring = hingeJoint.spring;
            spring.spring = springForce;
            hingeJoint.spring = spring;
        }
    }

    public void ApplyForce(float force)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddTorque(transform.right * force, ForceMode.Impulse);
        }
    }

    public float GetCurrentAngle()
    {
        return hingeJoint != null ? hingeJoint.angle : 0f;
    }

    public float GetNormalizedAngle()
    {
        float angle = GetCurrentAngle();
        return Mathf.InverseLerp(minAngle, maxAngle, angle);
    }

    public bool IsActivated()
    {
        return wasActivated;
    }

    private void OnValidate()
    {
        if (Application.isPlaying && hingeJoint != null)
        {
            SetupHingeJoint();
        }
    }
}
