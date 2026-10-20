
using System;
using UnityEngine;

public class PoseGestureClassifier : MonoBehaviour {
    [Header("Lane detection")]
    [Tooltip("Half-width of the centre lane around x = 0.5.")]
    [Range(0.10f, 0.40f)]
    public float laneBoundary = 0.18f;

    [Tooltip("Extra margin to prevent lane jitter.")]
    [Range(0f, 0.10f)]
    public float laneHysteresis = 0.04f;

    public float laneDwellTime = 0.12f;

    [Header("Gesture thresholds")]
    [Range(0.05f, 0.5f)]
    public float jumpRiseThreshold = 0.12f;

    [Range(0.2f, 0.98f)]
    public float duckRatioThreshold = 0.78f;

    public float gestureCooldown = 0.65f;
    public float smoothing = 12f;

    [Header("Calibration")]
    public float calibrationTime = 1.5f;

    // Lane convention: -1 = left, 0 = centre, +1 = right.
    public event Action<int> LaneChanged;
    public event Action JumpDetected;
    public event Action<bool> DuckChanged;

    public int CurrentLane { get; private set; } = 0;
    public bool IsDucking { get; private set; }
    public bool IsCalibrated { get; private set; }

    float neutralTorso;
    float neutralHipY;

    float filteredX = 0.5f;
    float filteredTorso;
    float filteredHipY;

    float calibrationElapsed;
    float calibrationTorsoSum;
    float calibrationHipSum;
    int calibrationSamples;

    int candidateLane;
    float laneCandidateElapsed;

    float lastJumpTime = -100f;
    float lastDuckTime = -100f;

    bool wasAboveJumpThreshold;
    bool poseWasValid;

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

        float shoulderX =
            (leftShoulder.x + rightShoulder.x) * 0.5f;

        float shoulderY =
            (leftShoulder.y + rightShoulder.y) * 0.5f;

        float hipX =
            (leftHip.x + rightHip.x) * 0.5f;

        float hipY =
            (leftHip.y + rightHip.y) * 0.5f;

        float bodyX = (shoulderX + hipX) * 0.5f;
        float torso = Mathf.Abs(hipY - shoulderY);

        if (!poseWasValid) {
            filteredX = bodyX;
            filteredHipY = hipY;
            filteredTorso = torso;
            poseWasValid = true;
        }
        else {
            float a = 1f - Mathf.Exp(-smoothing * dt);

            filteredX = Mathf.Lerp(filteredX, bodyX, a);
            filteredHipY = Mathf.Lerp(filteredHipY, hipY, a);
            filteredTorso = Mathf.Lerp(filteredTorso, torso, a);
        }

        if (!IsCalibrated) {
            Calibrate(dt);
            return;
        }

        UpdateLane(dt);
        UpdateJump();
        UpdateDuck();
    }

    void Calibrate(float dt) {
        calibrationElapsed += dt;
        calibrationTorsoSum += filteredTorso;
        calibrationHipSum += filteredHipY;
        calibrationSamples++;

        if (calibrationElapsed < calibrationTime ||
            calibrationSamples == 0)
            return;

        neutralTorso =
            calibrationTorsoSum / calibrationSamples;

        neutralHipY =
            calibrationHipSum / calibrationSamples;

        IsCalibrated = neutralTorso > 0.01f;

        Debug.Log(
            $"Pose calibrated: torso={neutralTorso:F3}, " +
            $"hipY={neutralHipY:F3}");
    }

    int GetDesiredLane() {
        float leftThreshold = 0.5f - laneBoundary;
        float rightThreshold = 0.5f + laneBoundary;

        // Hysteresis makes it harder to leave the current lane
        // than to remain inside it.
        switch (CurrentLane) {
            case -1:
                if (filteredX > rightThreshold + laneHysteresis)
                    return 1;
                if (filteredX > leftThreshold + laneHysteresis)
                    return 0;
                return -1;

            case 1:
                if (filteredX < leftThreshold - laneHysteresis)
                    return -1;
                if (filteredX < rightThreshold - laneHysteresis)
                    return 0;
                return 1;

            default:
                if (filteredX < leftThreshold - laneHysteresis)
                    return -1;
                if (filteredX > rightThreshold + laneHysteresis)
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

        // Restart the dwell timer if the desired lane changes.
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
}
