using UnityEngine;

public static class RaycastUtility
{
    public static bool Raycast(Vector2 origin, Vector2 direction, float distance, LayerMask layerMask, Color debugColor)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, layerMask);
        Debug.DrawRay(origin, direction * distance, debugColor, 0.2f);
        return hit.collider != null;
    }

    public static bool Raycast(Vector2 origin, Vector2 direction, float distance, LayerMask layerMask)
    {
        return Raycast(origin, direction, distance, layerMask, Color.red);
    }

    public static RaycastHit2D GetRaycastHit(Vector2 origin, Vector2 direction, float distance, LayerMask layerMask)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, layerMask);
        Debug.DrawRay(origin, direction * distance, hit.collider != null ? Color.green : Color.red, 0.1f);
        return hit;
    }
}
