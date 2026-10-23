
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
    [Tooltip("Smaller values make lane switching easier.")]
    [Range(0.05f, 0.40f)]
    public float laneBoundary = 0.12f;

    [Range(0f, 0.10f)]
    public float laneHysteresis = 0.015f;

    public float laneDwellTime = 0.10f;

    [Header("Jump detection")]
    [Tooltip("Upward hip movement needed to trigger a jump.")]
    [Range(0.03f, 0.50f)]
    public float jumpRiseThreshold = 0.12f;

    public float gestureCooldown = 0.65f;

    [Header("Duck / slide detection")]
    [Tooltip("Duck when torso height becomes less than this " +
             "fraction of its calibrated standing height.")]
    [Range(0.20f, 0.98f)]
    public float duckRatioThreshold = 0.78f;

    [Header("Smoothing")]
    public float smoothing = 12f;

    [Header("Calibration")]
    public float calibrationTime = 1.5f;

    public event Action<int> LaneChanged;
    public event Action JumpDetected;
    public event Action<bool> DuckChanged;

    public bool IsDucking { get; private set; }
    public bool IsCalibrated { get; private set; }

    float filteredX = 0.5f;
    float filteredY = 0.5f;
    float filteredHipY = 0.5f;
    float filteredTorso;

    float neutralHipY;
    float neutralTorso;

    float calibrationElapsed;
    float calibrationHipSum;
    float calibrationTorsoSum;
    int calibrationSamples;

    int candidateLane;
    float laneCandidateElapsed;

    float lastJumpTime = -100f;
    float lastDuckTime = -100f;

    bool wasAboveJumpThreshold;
    bool poseWasValid;

    void Awake() {
        if (bodyCentreUI != null && panelRect == null)
            panelRect = bodyCentreUI.parent as RectTransform;

        candidateLane = CurrentLane;
    }

    public void ProcessPose(
        Vector2 leftShoulder,
        Vector2 rightShoulder,
        Vector2 leftHip,
        Vector2 rightHip,
        bool valid) {
        if (!valid) {
            poseWasValid = false;
            laneCandidateElapsed = 0f;
            return;
        }

        float dt = Mathf.Max(Time.deltaTime, 0.001f);

        bool leftShoulderValid = IsValidLandmark(leftShoulder);
        bool rightShoulderValid = IsValidLandmark(rightShoulder);
        bool leftHipValid = IsValidLandmark(leftHip);
        bool rightHipValid = IsValidLandmark(rightHip);

        bool shouldersValid =
            leftShoulderValid && rightShoulderValid;

        bool hipsValid = leftHipValid && rightHipValid;

        if (!shouldersValid && !hipsValid) {
            poseWasValid = false;
            laneCandidateElapsed = 0f;
            return;
        }

        Vector2 shoulderCenter = Vector2.zero;
        Vector2 hipCenter = Vector2.zero;

        if (shouldersValid) {
            shoulderCenter =
                (leftShoulder + rightShoulder) * 0.5f;
        }

        if (hipsValid) {
            hipCenter = (leftHip + rightHip) * 0.5f;
        }

        // Preserve the old jump and duck mechanics whenever
        // the relevant landmarks are visible.
        float rawHipY;
        float rawTorso;

        if (hipsValid) {
            rawHipY = hipCenter.y;
        }
        else {
            // Fallback if hips disappear: approximate hip height
            // using the shoulders and the previous torso height.
            rawHipY = filteredHipY;
        }

        if (shouldersValid && hipsValid) {
            rawTorso = Mathf.Abs(
                hipCenter.y - shoulderCenter.y);
        }
        else {
            // Keep the last known torso measurement if shoulders
            // or hips temporarily disappear.
            rawTorso = filteredTorso;
        }

        // Body centre for the preview marker and lane detection.
        Vector2 bodyCenter;

        if (shouldersValid && hipsValid) {
            bodyCenter = (shoulderCenter + hipCenter) * 0.5f;
        }
        else if (hipsValid) {
            bodyCenter = new Vector2(
                hipCenter.x,
                hipCenter.y - 0.12f);
        }
        else {
            bodyCenter = shoulderCenter;
        }

        if (!poseWasValid) {
            filteredX = bodyCenter.x;
            filteredY = bodyCenter.y;
            filteredHipY = rawHipY;
            filteredTorso = rawTorso;
            poseWasValid = true;
        }
        else {
            float a = 1f - Mathf.Exp(-smoothing * dt);

            filteredX = Mathf.Lerp(filteredX, bodyCenter.x, a);
            filteredY = Mathf.Lerp(filteredY, bodyCenter.y, a);
            filteredHipY = Mathf.Lerp(filteredHipY, rawHipY, a);
            filteredTorso = Mathf.Lerp(
                filteredTorso, rawTorso, a);
        }

        float displayX = mirrorX ? 1f - filteredX : filteredX;

        BodyCenterNormalized = new Vector2(
            displayX, filteredY);

        BodyCenterPixels = new Vector2(
            displayX * panelWidth,
            filteredY * panelHeight);

        UpdateBodyCentreUI();

        if (!IsCalibrated) {
            Calibrate(dt);
            return;
        }

        UpdateLane(dt);
        UpdateJump();
        UpdateDuck();
        UpdateRow();
    }

    bool IsValidLandmark(Vector2 point) {
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
            Mathf.Lerp(
                rect.xMin, rect.xMax,
                BodyCenterNormalized.x),
            Mathf.Lerp(
                rect.yMax, rect.yMin,
                BodyCenterNormalized.y));

        if (bodyCentreUI.parent == panelRect) {
            bodyCentreUI.anchoredPosition = localPoint;
        }
        else {
            bodyCentreUI.position =
                panelRect.TransformPoint(localPoint);
        }
    }

    void Calibrate(float dt) {
        calibrationElapsed += dt;
        calibrationHipSum += filteredHipY;
        calibrationTorsoSum += filteredTorso;
        calibrationSamples++;

        if (calibrationElapsed < calibrationTime ||
            calibrationSamples == 0)
            return;

        neutralHipY =
            calibrationHipSum / calibrationSamples;

        neutralTorso =
            calibrationTorsoSum / calibrationSamples;

        IsCalibrated = neutralTorso > 0.01f;

        Debug.Log(
            $"Pose calibrated: hipY={neutralHipY:F3}, " +
            $"torso={neutralTorso:F3}");
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

    void UpdateLane(float dt) {
        int desiredLane = GetDesiredLane();

        if (desiredLane == CurrentLane) {
            candidateLane = CurrentLane;
            laneCandidateElapsed = 0f;
            return;
        }

        if (desiredLane != candidateLane) {
            candidateLane = desiredLane;
            laneCandidateElapsed = 0f;
        }

        laneCandidateElapsed += dt;

        if (laneCandidateElapsed >= laneDwellTime) {
            CurrentLane = candidateLane;
            laneCandidateElapsed = 0f;
            LaneChanged?.Invoke(CurrentLane);
        }
    }

    void UpdateJump() {
        // Original working mechanic: detect upward hip movement
        // relative to the neutral hip position.
        float rise = neutralHipY - filteredHipY;

        bool aboveThreshold = rise > jumpRiseThreshold;

        if (aboveThreshold &&
            !wasAboveJumpThreshold &&
            Time.time - lastJumpTime >= gestureCooldown) {
            lastJumpTime = Time.time;
            JumpDetected?.Invoke();
        }

        wasAboveJumpThreshold = aboveThreshold;
    }

    void UpdateDuck() {
        // Original working mechanic: detect a reduction in torso
        // height compared with the calibrated standing posture.
        if (neutralTorso <= 0.01f)
            return;

        float torsoRatio = filteredTorso / neutralTorso;
        bool duckNow = torsoRatio < duckRatioThreshold;

        if (duckNow == IsDucking)
            return;

        if (duckNow &&
            Time.time - lastDuckTime < gestureCooldown)
            return;

        if (duckNow)
            lastDuckTime = Time.time;

        IsDucking = duckNow;
        DuckChanged?.Invoke(IsDucking);
    }

    void UpdateRow() {
        // Rows are informational; jump and duck events use
        // the restored, independent gesture classifiers.
        if (IsDucking)
            CurrentRow = 2;
        else if (neutralHipY - filteredHipY > jumpRiseThreshold)
            CurrentRow = 0;
        else
            CurrentRow = 1;
    }
}
