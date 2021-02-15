using UnityEngine;

public class RotatingUtilities : MonoBehaviour
{
    public static Vector3 Rotate(ref float currentAngle, float increment)
    {
        currentAngle += increment;
        var newDirection = CalculatePosition(currentAngle * Mathf.Deg2Rad);

        return newDirection;
        
    }

    public static Vector2 CalculatePosition(float angle)
    {
        float x = Mathf.Sin(angle);
        float y = Mathf.Cos(angle);

        return new Vector2(x, y);
    }
}
