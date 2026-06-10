using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageScrollSnap : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("Scroll")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;

    [Header("Button")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("Snap Setting")]
    [SerializeField] private float snapSpeed = 10f;

    [Header("Start Setting")]
    [SerializeField] private bool startFromLastClearedStage = true;

    private int currentIndex = 0;
    private int childCount = 0;

    private bool isDragging = false;
    private bool isSnapping = false;

    private Coroutine snapCoroutine;

    private void Awake()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        if (scrollRect != null)
        {
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.inertia = false;
        }
    }

    private IEnumerator Start()
    {
        // Layout Group / Content Size Fitter가 자리 잡을 시간을 한 프레임 기다림
        yield return null;

        childCount = content.childCount;

        if (leftButton != null)
        {
            leftButton.onClick.RemoveListener(MoveLeft);
            leftButton.onClick.AddListener(MoveLeft);
        }

        if (rightButton != null)
        {
            rightButton.onClick.RemoveListener(MoveRight);
            rightButton.onClick.AddListener(MoveRight);
        }

        int startIndex = 0;

        if (startFromLastClearedStage)
        {
            int lastClearedIndex = StageProgress.LoadLastClearedStageIndex();

            // 마지막으로 깬 스테이지의 다음 스테이지를 보여줌
            startIndex = lastClearedIndex + 1;
        }
        else
        {
            startIndex = StageProgress.LoadLastSelectedStageIndex();
        }

        startIndex = Mathf.Clamp(startIndex, 0, childCount - 1);

        JumpToIndex(startIndex);
        UpdateButtonState();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        isSnapping = false;

        if (snapCoroutine != null)
        {
            StopCoroutine(snapCoroutine);
            snapCoroutine = null;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        int nearestIndex = GetNearestChildIndex();
        SnapToIndex(nearestIndex);
    }

    private void MoveLeft()
    {
        int nextIndex = currentIndex - 1;
        SnapToIndex(nextIndex);
    }

    private void MoveRight()
    {
        int nextIndex = currentIndex + 1;
        SnapToIndex(nextIndex);
    }

    private int GetNearestChildIndex()
    {
        if (childCount <= 0)
        {
            return 0;
        }

        float viewportCenterX = GetViewportCenterWorldX();

        int nearestIndex = 0;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < childCount; i++)
        {
            RectTransform child = content.GetChild(i) as RectTransform;

            if (child == null)
            {
                continue;
            }

            float distance = Mathf.Abs(child.position.x - viewportCenterX);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    private float GetViewportCenterWorldX()
    {
        RectTransform viewport = scrollRect.viewport;

        if (viewport == null)
        {
            viewport = scrollRect.GetComponent<RectTransform>();
        }

        Vector3[] corners = new Vector3[4];
        viewport.GetWorldCorners(corners);

        float left = corners[0].x;
        float right = corners[3].x;

        return (left + right) * 0.5f;
    }

    private void SnapToIndex(int index)
    {
        if (childCount <= 0)
        {
            return;
        }

        index = Mathf.Clamp(index, 0, childCount - 1);
        currentIndex = index;

        if (snapCoroutine != null)
        {
            StopCoroutine(snapCoroutine);
        }

        snapCoroutine = StartCoroutine(SnapRoutine(index));

        UpdateButtonState();
    }

    private void JumpToIndex(int index)
    {
        if (childCount <= 0)
        {
            return;
        }

        index = Mathf.Clamp(index, 0, childCount - 1);
        currentIndex = index;

        Vector2 targetPosition = GetTargetContentPosition(index);
        content.anchoredPosition = targetPosition;

        UpdateButtonState();
    }

    private IEnumerator SnapRoutine(int index)
    {
        isSnapping = true;

        Vector2 targetPosition = GetTargetContentPosition(index);

        while (Vector2.Distance(content.anchoredPosition, targetPosition) > 0.5f)
        {
            content.anchoredPosition = Vector2.Lerp(
                content.anchoredPosition,
                targetPosition,
                Time.deltaTime * snapSpeed
            );

            yield return null;
        }

        content.anchoredPosition = targetPosition;

        isSnapping = false;
        snapCoroutine = null;
    }

    private Vector2 GetTargetContentPosition(int index)
    {
        RectTransform child = content.GetChild(index) as RectTransform;

        if (child == null)
        {
            return content.anchoredPosition;
        }

        RectTransform viewport = scrollRect.viewport;

        if (viewport == null)
        {
            viewport = scrollRect.GetComponent<RectTransform>();
        }

        float viewportWidth = viewport.rect.width;

        // 선택한 카드가 Viewport 중앙에 오도록 Content 위치 계산
        float targetX = -child.anchoredPosition.x + (viewportWidth * 0.5f) - (child.rect.width * 0.5f);

        return new Vector2(targetX, content.anchoredPosition.y);
    }

    private void UpdateButtonState()
    {
        SoundManager_1.Instance.PlaySfx(SfxType.ButtonClick);

        if (leftButton != null)
        {
            leftButton.interactable = currentIndex > 0;
        }

        if (rightButton != null)
        {
            rightButton.interactable = currentIndex < childCount - 1;
        }
    }
}