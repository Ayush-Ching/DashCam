
using UnityEngine;
using System.IO;
using System.Collections;

public class Screenshot : MonoBehaviour {
    [ContextMenu("Capture Screenshot")]
    public void Capture() {
        StartCoroutine(CaptureCoroutine());
    }

    private IEnumerator CaptureCoroutine() {
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();

        byte[] png = screenshot.EncodeToPNG();

        string path = Path.Combine(
            Application.persistentDataPath,
            "Screenshot.png"
        );

        File.WriteAllBytes(path, png);
        Debug.Log("Screenshot saved to: " + path);

        Destroy(screenshot);
    }
}
