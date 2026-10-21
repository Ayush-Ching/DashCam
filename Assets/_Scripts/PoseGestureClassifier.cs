
using System;
using UnityEngine;

public class PoseGestureClassifier : MonoBehaviour {
    [Header("Grid / UI")]
    public float panelWidth = 640f;
    public float panelHeight = 360f;

    [Tooltip("The RectTransform of the camera preview panel.")]
    [SerializeField] private RectTransform panelRect;

    [Tooltip("UI marker that follows the detected body centre.")]
    [SerializeField] private RectTransform bodyCentreUI;

    [Tooltip("Mirror the displayed X coordinate if the preview is mirrored.")]
    [SerializeField] private bool mirrorX = false;

    public Vector2 BodyCenterNormalized { get; private set; }
        = new Vector2(0.5f, 0.5f);

    public Vector2 BodyCenterPixels { get; private set; }
        = new Vector2(320f, 180f);

    // -1 = left, 0 = centre, +1 = right.
    public int CurrentLane { get; private set; } = 0;

    // 0 = top/jump, 1 = middle/normal, 2 = bottom/duck.
    public int CurrentRow { get; private set; } = 1;

    [Header("Lane boundaries")]
    [Tooltip("Half-width of the centre lane.")]
    [Range(0.05f, 0.40f)]
    public float laneBoundary = 0.18f;

    [Tooltip("Extra margin for lane stability.")]
    [Range(0f, 0.10f)]
    public float laneHysteresis = 0.02f;

    [Header("Jump and duck boundaries")]
    [Tooltip("Jump triggers when the body centre rises above this Y. " +
             "Smaller values require a higher upward movement.")]
    [Range(0.05f, 0.5f)]
    public float jumpYBoundary = 0.30f;

    [Tooltip("Duck triggers when the body centre falls below this Y. " +
             "Larger values require a deeper downward movement.")]
    [Range(0.55f, 0.95f)]
    public float duckYBoundary = 0.70f;

    [Tooltip("Extra margin for stable row transitions.")]
    [Range(0f, 0.10f)]
    public float rowHysteresis = 0.025f;

    [Tooltip("Time the body must remain in a new cell.")]
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

    float calibrationElapsed;
    int calibrationSamples;

    int candidateLane;
    int candidateRow;
    float gridCandidateElapsed;

    bool poseWasValid;

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
        if (!valid) {
            poseWasValid = false;
            gridCandidateElapsed = 0f;
            return;
        }

        float dt = Mathf.Max(Time.deltaTime, 0.001f);

        Vector2 shoulderCenter =
            (leftShoulder + rightShoulder) * 0.5f;

        Vector2 hipCenter =
            (leftHip + rightHip) * 0.5f;

        Vector2 bodyCenter =
            (shoulderCenter + hipCenter) * 0.5f;

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
        calibrationSamples++;

        if (calibrationElapsed < calibrationTime ||
            calibrationSamples == 0)
            return;

        IsCalibrated = true;

        Debug.Log(
            $"Grid calibrated. Body centre = " +
            $"{BodyCenterPixels.x:F0}, {BodyCenterPixels.y:F0}");
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
        // MediaPipe Y increases downwards.
        // Jump and duck have independent boundaries, leaving
        // a wider middle region between them.

        if (CurrentRow == 0) {
            if (filteredY > jumpYBoundary + rowHysteresis) {
                if (filteredY > duckYBoundary + rowHysteresis)
                    return 2;

                return 1;
            }

            return 0;
        }

        if (CurrentRow == 2) {
            if (filteredY < duckYBoundary - rowHysteresis) {
                if (filteredY < jumpYBoundary - rowHysteresis)
                    return 0;

                return 1;
            }

            return 2;
        }

        if (filteredY < jumpYBoundary - rowHysteresis)
            return 0;

        if (filteredY > duckYBoundary + rowHysteresis)
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

        // Trigger once when entering the jump region.
        if (CurrentRow == 0 && previousRow != 0)
            JumpDetected?.Invoke();

        // Duck remains active while in the lower region.
        bool shouldDuck = CurrentRow == 2;

        if (shouldDuck != IsDucking) {
            IsDucking = shouldDuck;
            DuckChanged?.Invoke(IsDucking);
        }
    }
}
