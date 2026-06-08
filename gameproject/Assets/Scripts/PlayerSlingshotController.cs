using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSlingshotController : MonoBehaviour
{
    [Header("Power")]
    [SerializeField] private float powerMultiplier = 8f;
    [SerializeField] private float maxDragDistance = 3f;

    [Header("Move Stop")]
    [SerializeField] private float maxMoveTime = 3f;
    [SerializeField] private float stopSmoothPower = 2.5f;
    [SerializeField] private float stopVelocityThreshold = 0.05f;

    [Header("Arrow Guide")]
    [SerializeField] private LineRenderer arrowLine;
    [SerializeField] private Transform arrowHead;
    [SerializeField] private float arrowLengthMultiplier = 1.2f;
    [SerializeField] private float arrowHeadDistance = 0.25f;

    [Header("State")]
    [SerializeField] private bool isDragging = false;
    [SerializeField] private bool isMoving = false;

    private Rigidbody2D rb;
    private Camera mainCamera;
    private PlayerHealth playerHealth;

    private Vector2 dragStartWorldPos;
    private Vector2 dragCurrentWorldPos;

    private Coroutine stopCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        playerHealth = GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            playerHealth = gameObject.AddComponent<PlayerHealth>();
        }

        rb.gravityScale = 0f;

        HideArrow();
    }

    private void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        HandleInput();
    }

    private void HandleInput()
    {
        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
        {
            return;
        }

        if (TurnManager.Instance != null && !TurnManager.Instance.CanPlayerShoot())
        {
            return;
        }

        if (isMoving)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartDrag();
        }

        if (Mouse.current.leftButton.isPressed && isDragging)
        {
            UpdateDrag();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            Release();
        }
    }

    private void StartDrag()
    {
        dragStartWorldPos = GetMouseWorldPosition();
        dragCurrentWorldPos = dragStartWorldPos;
        isDragging = true;
        ShowArrow();
    }

    private void UpdateDrag()
    {
        dragCurrentWorldPos = GetMouseWorldPosition();
        UpdateArrow();
    }

    private void Release()
    {
        Vector2 dragVector = GetClampedDragVector();

        if (dragVector.sqrMagnitude <= 0.001f)
        {
            isDragging = false;
            HideArrow();
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.AddForce(dragVector * powerMultiplier, ForceMode2D.Impulse);

        isDragging = false;
        isMoving = true;

        HideArrow();

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.StartPlayerMove();
        }

        if (stopCoroutine != null)
        {
            StopCoroutine(stopCoroutine);
        }

        stopCoroutine = StartCoroutine(SlowStopRoutine());
    }

    private IEnumerator SlowStopRoutine()
    {
        float timer = 0f;

        while (timer < maxMoveTime)
        {
            timer += Time.deltaTime;
            float t = timer / maxMoveTime;
            float currentSlowPower = Mathf.Lerp(0.2f, stopSmoothPower, t);

            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.deltaTime * currentSlowPower);
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, 0f, Time.deltaTime * currentSlowPower);

            if (rb.linearVelocity.magnitude <= stopVelocityThreshold)
            {
                break;
            }

            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        isMoving = false;
        stopCoroutine = null;

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.EndPlayerMove();
        }
        else
        {
            Debug.Log("Move stopped. Ready for next shot.");
        }
    }

    private Vector2 GetClampedDragVector()
    {
        Vector2 dragVector = dragStartWorldPos - dragCurrentWorldPos;

        if (dragVector.magnitude > maxDragDistance)
        {
            dragVector = dragVector.normalized * maxDragDistance;
        }

        return dragVector;
    }

    private void UpdateArrow()
    {
        Vector2 dragVector = GetClampedDragVector();

        if (dragVector.sqrMagnitude <= 0.001f)
        {
            HideArrow();
            return;
        }

        ShowArrow();

        Vector3 startPos = transform.position;
        Vector3 direction = dragVector.normalized;
        float arrowLength = dragVector.magnitude * arrowLengthMultiplier;
        Vector3 endPos = startPos + direction * arrowLength;

        if (arrowLine != null)
        {
            arrowLine.positionCount = 2;
            arrowLine.SetPosition(0, startPos);
            arrowLine.SetPosition(1, endPos);
        }

        if (arrowHead != null)
        {
            arrowHead.position = endPos - direction * arrowHeadDistance;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrowHead.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void ShowArrow()
    {
        if (arrowLine != null)
        {
            arrowLine.enabled = true;
        }

        if (arrowHead != null)
        {
            arrowHead.gameObject.SetActive(true);
        }
    }

    private void HideArrow()
    {
        if (arrowLine != null)
        {
            arrowLine.enabled = false;
        }

        if (arrowHead != null)
        {
            arrowHead.gameObject.SetActive(false);
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        return new Vector2(worldPos.x, worldPos.y);
    }
}
