using UnityEngine;

public class GameInput : MonoBehaviour {
    
    public static GameInput Instance { get; private set; }


    private Controls controls;

    private void Awake() {
        Instance = this;

        controls = new Controls();

        controls.Player.Enable();
    }

    private void OnDestroy() {
        controls.Player.Disable();
        controls.Dispose();
    }

    public bool WasLeftPressed() {
        return controls.Player.Left.WasPressedThisFrame();
    }

    public bool WasRightPressed() {
        return controls.Player.Right.WasPressedThisFrame();
    }

    public bool WasJumpPressed() {
        return controls.Player.Jump.WasPressedThisFrame();
    }

    public bool WasSlidePressed() {
        return controls.Player.Slide.WasPressedThisFrame();
    }

    public bool WasPausePressed() {
        return controls.Player.Pause.WasPressedThisFrame();
    }

}
