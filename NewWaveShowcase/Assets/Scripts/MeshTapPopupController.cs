using UnityEngine;
using UnityEngine.EventSystems;

public class MeshTapPopupController : MonoBehaviour
{
    [Header("Raycast")]
    public Camera raycastCamera;
    public LayerMask interactableMask = ~0;
    public float maxDistance = 50f;

    [Header("Popup UI")]
    public PartInfoPopupUI popup;

    [Header("Tap vs Drag (avoid conflict with rotate/zoom)")]
    public float tapMaxMovePixels = 12f;
    public float tapMaxTime = 0.28f;

    [Header("Behavior")]
    public bool disablePickWhenPopupOpen = false;

    private Vector2 startPos;
    private float startTime;
    private bool trackingTap;

    void Awake()
    {
        if (raycastCamera == null) raycastCamera = Camera.main;
    }

    void Update()
    {
        if (popup == null) return;
        if (disablePickWhenPopupOpen && popup.IsOpen) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    void HandleMouse()
    {
        if (IsPointerOverUI_Mouse()) return;

        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            startTime = Time.time;
            trackingTap = true;
        }

        if (Input.GetMouseButton(0))
        {
            float moved = ((Vector2)Input.mousePosition - startPos).magnitude;
            float dt = Time.time - startTime;

            if (moved > tapMaxMovePixels || dt > tapMaxTime)
                trackingTap = false;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!trackingTap) return;

            float moved = ((Vector2)Input.mousePosition - startPos).magnitude;
            float dt = Time.time - startTime;

            if (dt <= tapMaxTime && moved <= tapMaxMovePixels)
                TryPickAndShow(Input.mousePosition);

            trackingTap = false;
        }
    }

    bool IsPointerOverUI_Mouse()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0) return;

        if (Input.touchCount >= 2)
        {
            trackingTap = false;
            return;
        }

        Touch t = Input.GetTouch(0);

        if (IsPointerOverUI_Touch(t)) return;

        if (t.phase == TouchPhase.Began)
        {
            startPos = t.position;
            startTime = Time.time;
            trackingTap = true;
        }
        else if (t.phase == TouchPhase.Moved)
        {
            float moved = (t.position - startPos).magnitude;
            float dt = Time.time - startTime;

            if (moved > tapMaxMovePixels || dt > tapMaxTime)
                trackingTap = false;
        }
        else if (t.phase == TouchPhase.Ended)
        {
            if (!trackingTap) return;

            float moved = (t.position - startPos).magnitude;
            float dt = Time.time - startTime;

            if (dt <= tapMaxTime && moved <= tapMaxMovePixels)
                TryPickAndShow(t.position);

            trackingTap = false;
        }
        else if (t.phase == TouchPhase.Canceled)
        {
            trackingTap = false;
        }
    }

    bool IsPointerOverUI_Touch(Touch t)
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject(t.fingerId);
    }

    void TryPickAndShow(Vector2 screenPos)
    {
        if (raycastCamera == null) return;

        Ray ray = raycastCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactableMask, QueryTriggerInteraction.Collide))
        {
            PartInfo info = hit.collider.GetComponentInParent<PartInfo>();
            if (info != null)
            {
                popup.Show(info, screenPos);
                return;
            }
        }

    }
}
