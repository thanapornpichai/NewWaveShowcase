using UnityEngine;

public class TouchModelViewer_FreeOrbit : MonoBehaviour
{
    [Header("Rotation")]
    public float rotateSpeed = 0.2f;
    public float mouseRotateSpeed = 4f;
    public float smoothTime = 0.08f;

    public float minPitch = -80f;
    public float maxPitch = 80f;

    public bool enableRoll = false;
    public float rollSpeed = 0.15f;

    [Header("Zoom (FOV)")]
    public Camera targetCamera;
    public float pinchToFovSpeed = 0.08f;
    public float wheelToFovSpeed = 20f;
    public float minFOV = 25f;
    public float maxFOV = 60f;
    public float fovSmoothTime = 0.08f;

    private float targetYaw;
    private float targetPitch;
    private float targetRoll;

    private float yawVel;
    private float pitchVel;
    private float rollVel;

    private float targetFov;
    private float fovVel;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        Vector3 e = transform.eulerAngles;
        targetYaw = e.y;
        targetPitch = e.x;
        targetRoll = e.z;

        targetFov = Mathf.Clamp(targetCamera.fieldOfView, minFOV, maxFOV);
    }

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouse();
#else
        HandleTouch();
#endif
        ApplyTransform();
    }

    void HandleMouse()
    {
        if (Input.GetMouseButton(0))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            targetYaw += dx * mouseRotateSpeed;     
            targetPitch -= dy * mouseRotateSpeed;       
            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
        }

        if (enableRoll && Input.GetMouseButton(1))
        {
            float dx = Input.GetAxis("Mouse X");
            targetRoll += dx * mouseRotateSpeed;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            targetFov -= scroll * wheelToFovSpeed;
            targetFov = Mathf.Clamp(targetFov, minFOV, maxFOV);
        }
    }

    void HandleTouch()
    {
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved)
            {
                targetYaw += t.deltaPosition.x * rotateSpeed;
                targetPitch -= t.deltaPosition.y * rotateSpeed;
                targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 p0Prev = t0.position - t0.deltaPosition;
            Vector2 p1Prev = t1.position - t1.deltaPosition;

            float prevDist = Vector2.Distance(p0Prev, p1Prev);
            float curDist = Vector2.Distance(t0.position, t1.position);
            float delta = curDist - prevDist;

            targetFov -= delta * pinchToFovSpeed;
            targetFov = Mathf.Clamp(targetFov, minFOV, maxFOV);

            if (enableRoll)
            {
                float prevAngle = Mathf.Atan2(
                    p1Prev.y - p0Prev.y,
                    p1Prev.x - p0Prev.x
                ) * Mathf.Rad2Deg;

                float curAngle = Mathf.Atan2(
                    t1.position.y - t0.position.y,
                    t1.position.x - t0.position.x
                ) * Mathf.Rad2Deg;

                float angleDelta = Mathf.DeltaAngle(prevAngle, curAngle);
                targetRoll += angleDelta * rollSpeed;
            }
        }
    }

    void ApplyTransform()
    {
        float y = Mathf.SmoothDampAngle(
            transform.eulerAngles.y, targetYaw, ref yawVel, smoothTime);

        float x = Mathf.SmoothDampAngle(
            transform.eulerAngles.x, targetPitch, ref pitchVel, smoothTime);

        float z = Mathf.SmoothDampAngle(
            transform.eulerAngles.z, targetRoll, ref rollVel, smoothTime);

        transform.rotation = Quaternion.Euler(x, y, z);

        float fov = Mathf.SmoothDamp(
            targetCamera.fieldOfView, targetFov, ref fovVel, fovSmoothTime);

        targetCamera.fieldOfView = fov;
    }
}
