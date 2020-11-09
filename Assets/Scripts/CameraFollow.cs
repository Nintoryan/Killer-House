using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public PlayerController _Player;

    private Vector3 previous;
    private Vector3 current;
    private Vector3 delta;
    private void Start()
    {
        previous = _Player.transform.position;
    }

    private void FixedUpdate()
    {
        current = _Player.transform.position;
        delta = current - previous;
        transform.position += delta;
        previous = current;
    }
}
