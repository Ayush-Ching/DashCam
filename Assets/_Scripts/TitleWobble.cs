using UnityEngine;
using TMPro;

public class TitleWobble : MonoBehaviour {

    [Header("Running Motion")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float bounce = 4f;
    [SerializeField] private float sway = 2f;
    [SerializeField] private float rotation = 5f;
    [SerializeField] private float letterOffset = 0.7f;

    private TextMeshProUGUI titleText;
    private TMP_MeshInfo[] originalMeshes;

    private void Awake() {
        titleText = GetComponent<TextMeshProUGUI>();

        titleText.ForceMeshUpdate();
        originalMeshes = titleText.textInfo.CopyMeshInfoVertexData();
    }

    private void LateUpdate() {
        titleText.ForceMeshUpdate();

        TMP_TextInfo textInfo = titleText.textInfo;
        float time = Time.unscaledTime * speed;

        for (int i = 0; i < textInfo.characterCount; i++) {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            Vector3[] original = originalMeshes[materialIndex].vertices;

            Vector3 center = (original[vertexIndex] + original[vertexIndex + 2]) * 0.5f;

            float phase = time - i * letterOffset;

            Vector3 offset = new Vector3(Mathf.Sin(phase * 0.7f) * sway, Mathf.Sin(phase) * bounce, 0f);

            float angle = Mathf.Sin(phase * 0.8f) * rotation;
            Quaternion rot = Quaternion.Euler(0f, 0f, angle);

            for (int j = 0; j < 4; j++) {
                vertices[vertexIndex + j] = rot * (original[vertexIndex + j] - center) + center + offset;
            }
        }

        titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}
