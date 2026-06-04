using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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

    private Vector2 dragStartWorldPos;
    private Vector2 dragCurrentWorldPos;

    private Coroutine stopCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        // 2D 슬링샷 게임이므로 중력은 사용하지 않음
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
        // 게임이 클리어/실패 상태라면 입력 막기
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
        {
            return;
        }

        // TurnManager가 있으면, 플레이어가 쏠 수 있는 턴인지 확인
        if (TurnManager.Instance != null && !TurnManager.Instance.CanPlayerShoot())
        {
            return;
        }

        // 혹시 TurnManager가 없어도 이동 중에는 재발사 방지
        if (isMoving)
        {
            return;
        }

        // 마우스를 처음 누른 순간
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // UI 위에서 누른 경우 슬링샷 입력 시작 안 함
            if (IsPointerOverUI())
            {
                return;
            }

            StartDrag();
        }

        // 드래그 중
        if (Mouse.current.leftButton.isPressed && isDragging)
        {
            UpdateDrag();
        }

        // 마우스를 뗀 순간
        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            Release();
        }
    }

    private void StartDrag()
    {
        // 한 번 더 안전 체크
        if (IsPointerOverUI())
        {
            return;
        }

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

        // 거의 안 당겼으면 발사하지 않음
        if (dragVector.sqrMagnitude <= 0.001f)
        {
            isDragging = false;
            HideArrow();
            return;
        }

        // 발사 직전 기존 속도 초기화
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 슬링샷 발사
        rb.AddForce(dragVector * powerMultiplier, ForceMode2D.Impulse);

        isDragging = false;
        isMoving = true;

        HideArrow();

        // 턴 매니저에게 이동 시작 알림
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.StartPlayerMove();
        }

        // 기존 감속 코루틴이 있으면 정리
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

            // 처음에는 약하게, 시간이 지날수록 강하게 감속
            float currentSlowPower = Mathf.Lerp(0.2f, stopSmoothPower, t);

            rb.linearVelocity = Vector2.Lerp(
                rb.linearVelocity,
                Vector2.zero,
                Time.deltaTime * currentSlowPower
            );

            rb.angularVelocity = Mathf.Lerp(
                rb.angularVelocity,
                0f,
                Time.deltaTime * currentSlowPower
            );

            // 충분히 느려졌으면 3초를 다 기다리지 않고 종료
            if (rb.linearVelocity.magnitude <= stopVelocityThreshold)
            {
                break;
            }

            yield return null;
        }

        // 마지막에는 완전히 정지
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        isMoving = false;
        stopCoroutine = null;

        // 턴 매니저에게 이동 종료 알림
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.EndPlayerMove();
        }
        else
        {
            Debug.Log("멈춤 완료! 다음 턴 시작 가능");
        }
    }

    private Vector2 GetClampedDragVector()
    {
        // 마우스를 뒤로 당긴 만큼 반대 방향으로 발사
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

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        return EventSystem.current.IsPointerOverGameObject();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isMoving)
        {
            return;
        }

        PlayerHeart playerHeart = GetComponent<PlayerHeart>();

        if (playerHeart == null)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Stone"))
        {
            playerHeart.TakeDamage(1);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySfx(SfxType.StoneBreak);
            }

            if (EffectManager.Instance != null)
            {
                Vector3 hitPosition = collision.GetContact(0).point;
                EffectManager.Instance.PlayEffect(EffectType.StoneBreak, hitPosition);
            }

            return;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {            

            Enemy enemy = collision.gameObject.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(1);
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySfx(SfxType.HitEnemy);
            }

            if (EffectManager.Instance != null && collision.contactCount > 0)
            {
                Vector3 hitPosition = collision.GetContact(0).point;
                EffectManager.Instance.PlayEffect(EffectType.HitEnemy, hitPosition);
            }

            return;
        }
    }
}