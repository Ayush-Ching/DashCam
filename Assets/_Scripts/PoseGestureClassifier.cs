
using System;
using UnityEngine;

public class PoseGestureClassifier : MonoBehaviour {
    [Header("Grid / UI")]
    public float panelWidth = 640f;
    public float panelHeight = 360f;

    [SerializeField] private RectTransform panelRect;
    [SerializeField] private RectTransform bodyCentreUI;
    [SerializeField] private bool mirrorX = false;

    public Vector2 BodyCenterNormalized { get; private set; }
        = new Vector2(0.5f, 0.5f);

    public Vector2 BodyCenterPixels { get; private set; }
        = new Vector2(320f, 180f);

    // -1 = left, 0 = centre, +1 = right.
    public int CurrentLane { get; private set; } = 0;

    // 0 = top/jump, 1 = middle, 2 = bottom/duck.
    public int CurrentRow { get; private set; } = 1;

    [Header("Lane boundaries")]
    [Range(0.05f, 0.40f)]
    public float laneBoundary = 0.18f;

    [Range(0f, 0.10f)]
    public float laneHysteresis = 0.02f;

    [Header("Jump detection")]
    [Tooltip("How far upward the tracked centre must move " +
             "from its calibrated position to trigger a jump.")]
    [Range(0.03f, 0.40f)]
    public float jumpRiseThreshold = 0.10f;

    [Tooltip("Minimum time between jump events.")]
    public float jumpCooldown = 0.45f;

    [Header("Duck detection")]
    [Tooltip("Duck when the tracked centre is this far below " +
             "its calibrated position.")]
    [Range(0.03f, 0.40f)]
    public float duckDropThreshold = 0.16f;

    [Header("Grid boundaries")]
    [Range(0.0f, 0.10f)]
    public float rowHysteresis = 0.02f;

    public float gridDwellTime = 0.08f;

    [Header("Smoothing")]
    public float smoothing = 18f;

    [Header("Calibration")]
    public float calibrationTime = 1.5f;

    public event Action<int> LaneChanged;
    public event Action JumpDetected;
    public event Action<bool> DuckChanged;

    public bool IsDucking { get; private set; }
    public bool IsCalibrated { get; private set; }

    float filteredX = 0.5f;
    float filteredY = 0.5f;

    float neutralY = 0.5f;
    float calibrationElapsed;
    float calibrationYSum;
    int calibrationSamples;

    int candidateLane;
    int candidateRow;
    float gridCandidateElapsed;

    float lastJumpTime = -100f;

    bool poseWasValid;
    bool wasJumping;

    void Awake() {
        if (bodyCentreUI != null && panelRect == null)
            panelRect = bodyCentreUI.parent as RectTransform;

        candidateLane = CurrentLane;
        candidateRow = CurrentRow;
    }

    public void ProcessPose(
        Vector2 leftShoulder,
        Vector2 rightShoulder,
        Vector2 leftHip,
        Vector2 rightHip,
        bool valid) {
        // The MediaPipe adapter should set valid = true when
        // the hips are available, even if the shoulders are not.
        if (!valid) {
            poseWasValid = false;
            gridCandidateElapsed = 0f;
            return;
        }

        float dt = Mathf.Max(Time.deltaTime, 0.001f);

        bool shouldersValid =
            IsValidLandmark(leftShoulder) &&
            IsValidLandmark(rightShoulder);

        bool hipsValid =
            IsValidLandmark(leftHip) &&
            IsValidLandmark(rightHip);

        if (!hipsValid && !shouldersValid) {
            poseWasValid = false;
            gridCandidateElapsed = 0f;
            return;
        }

        Vector2 bodyCenter;

        if (shouldersValid && hipsValid) {
            Vector2 shoulderCenter =
                (leftShoulder + rightShoulder) * 0.5f;

            Vector2 hipCenter =
                (leftHip + rightHip) * 0.5f;

            bodyCenter = (shoulderCenter + hipCenter) * 0.5f;
        }
        else if (hipsValid) {
            // Fallback when the upper body leaves the frame.
            // Estimate the centre above the hips.
            Vector2 hipCenter =
                (leftHip + rightHip) * 0.5f;

            bodyCenter = new Vector2(
                hipCenter.x,
                hipCenter.y - 0.12f
            );
        }
        else {
            // If hips are missing but shoulders remain visible,
            // use the shoulder midpoint.
            bodyCenter =
                (leftShoulder + rightShoulder) * 0.5f;
        }

        if (!poseWasValid) {
            filteredX = bodyCenter.x;
            filteredY = bodyCenter.y;
            poseWasValid = true;
        }
        else {
            float a = 1f - Mathf.Exp(-smoothing * dt);

            filteredX = Mathf.Lerp(filteredX, bodyCenter.x, a);
            filteredY = Mathf.Lerp(filteredY, bodyCenter.y, a);
        }

        float displayX = mirrorX ? 1f - filteredX : filteredX;

        BodyCenterNormalized = new Vector2(displayX, filteredY);

        BodyCenterPixels = new Vector2(
            displayX * panelWidth,
            filteredY * panelHeight
        );

        UpdateBodyCentreUI();

        if (!IsCalibrated) {
            Calibrate(dt);
            return;
        }

        UpdateGrid(dt);
        UpdateJump();
    }

    bool IsValidLandmark(Vector2 point) {
        // Normalised coordinates can sometimes be outside the
        // frame. Allow a small margin for landmarks near edges.
        return !float.IsNaN(point.x) &&
               !float.IsNaN(point.y) &&
               !float.IsInfinity(point.x) &&
               !float.IsInfinity(point.y) &&
               point.x >= -0.2f && point.x <= 1.2f &&
               point.y >= -0.2f && point.y <= 1.2f;
    }

    void UpdateBodyCentreUI() {
        if (bodyCentreUI == null || panelRect == null)
            return;

        Rect rect = panelRect.rect;

        Vector2 localPoint = new Vector2(
            Mathf.Lerp(rect.xMin, rect.xMax, BodyCenterNormalized.x),
            Mathf.Lerp(rect.yMax, rect.yMin, BodyCenterNormalized.y)
        );

        if (bodyCentreUI.parent == panelRect) {
            bodyCentreUI.anchoredPosition = localPoint;
        }
        else {
            Vector3 worldPoint = panelRect.TransformPoint(localPoint);
            bodyCentreUI.position = worldPoint;
        }
    }

    void Calibrate(float dt) {
        calibrationElapsed += dt;
        calibrationYSum += filteredY;
        calibrationSamples++;

        if (calibrationElapsed < calibrationTime ||
            calibrationSamples == 0)
            return;

        neutralY = calibrationYSum / calibrationSamples;
        IsCalibrated = true;

        Debug.Log(
            $"Pose calibrated. Neutral Y={neutralY:F3}");
    }

    int GetDesiredLane() {
        float leftBoundary = 0.5f - laneBoundary;
        float rightBoundary = 0.5f + laneBoundary;

        switch (CurrentLane) {
            case -1:
                if (filteredX > rightBoundary + laneHysteresis)
                    return 1;

                if (filteredX > leftBoundary + laneHysteresis)
                    return 0;

                return -1;

            case 1:
                if (filteredX < leftBoundary - laneHysteresis)
                    return -1;

                if (filteredX < rightBoundary - laneHysteresis)
                    return 0;

                return 1;

            default:
                if (filteredX < leftBoundary - laneHysteresis)
                    return -1;

                if (filteredX > rightBoundary + laneHysteresis)
                    return 1;

                return 0;
        }
    }

    int GetDesiredRow() {
        float rise = neutralY - filteredY;
        float drop = filteredY - neutralY;

        if (rise > jumpRiseThreshold)
            return 0;

        if (drop > duckDropThreshold)
            return 2;

        return 1;
    }

    void UpdateGrid(float dt) {
        int desiredLane = GetDesiredLane();
        int desiredRow = GetDesiredRow();

        if (desiredLane == CurrentLane &&
            desiredRow == CurrentRow) {
            candidateLane = CurrentLane;
            candidateRow = CurrentRow;
            gridCandidateElapsed = 0f;
            return;
        }

        if (desiredLane != candidateLane ||
            desiredRow != candidateRow) {
            candidateLane = desiredLane;
            candidateRow = desiredRow;
            gridCandidateElapsed = 0f;
        }

        gridCandidateElapsed += dt;

        if (gridCandidateElapsed < gridDwellTime)
            return;

        int previousLane = CurrentLane;
        int previousRow = CurrentRow;

        CurrentLane = candidateLane;
        CurrentRow = candidateRow;

        gridCandidateElapsed = 0f;

        if (CurrentLane != previousLane)
            LaneChanged?.Invoke(CurrentLane);

        bool shouldDuck = CurrentRow == 2;

        if (shouldDuck != IsDucking) {
            IsDucking = shouldDuck;
            DuckChanged?.Invoke(IsDucking);
        }
    }

    void UpdateJump() {
        bool jumping = CurrentRow == 0;

        if (jumping &&
            !wasJumping &&
            Time.time - lastJumpTime >= jumpCooldown) {
            lastJumpTime = Time.time;
            JumpDetected?.Invoke();
        }

        wasJumping = jumping;
    }
}
