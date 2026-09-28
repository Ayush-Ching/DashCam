using UnityEngine;

public class Truck : MonoBehaviour {
    [SerializeField] private int relativeLocalSpeed = 1;

    public int GetRelativeLocalSpeed() {
        return relativeLocalSpeed;
    }
}
