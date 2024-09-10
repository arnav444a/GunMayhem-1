using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using System;
using System.Reflection;
using System.ComponentModel;

public static partial class Core
{
    public static float ComputePathLength(this NavMeshPath path)
    {
        Vector3[] corners = path.corners;
        if (corners.Length < 2)
            return 0;
        float distance = 0;
        Vector3 a = corners[0];
        foreach (Vector3 b in corners)
        {
            distance += Vector3.Distance(a, b);
            a = b;
        }
        return distance;
    }
    public static Vector2 Absolute(this Vector2 v)
    {
        return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }
    public static Vector3 Absolute(this Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }
    public static bool Contains(this Vector2[] vertices, Vector2 point)
    {
        bool collision = false;

        // go through each of the vertices, plus
        // the next vertex in the list
        int next = 0;
        int nbVertices = vertices.Length;
        float px = point.x;
        float py = point.y;
        for (int current = 0; current < nbVertices; current++)
        {
            // get next vertex in list
            // if we've hit the end, wrap around to 0
            next = current + 1;
            if (next == nbVertices) next = 0;

            // get the PVectors at our current position
            // this makes our if statement a little cleaner
            Vector2 vc = vertices[current];    // c for "current"
            Vector2 vn = vertices[next];       // n for "next"

            // compare position, flip 'collision' variable
            // back and forth
            if (((vc.y >= py && vn.y < py)
                || (vc.y < py && vn.y >= py))
                && (px < (vn.x - vc.x) * (py - vc.y) / (vn.y - vc.y) + vc.x))
            {
                collision = !collision;
            }
        }
        return collision;
    }
    public static Vector3 ClosestPointByDistance(this NavMeshPath path, float fromPointB)
    {
        Vector3[] corners = path.corners;
        if (corners.Length < 2)
            return Vector3.zero;//invalid path

        corners = corners.Reverse().ToArray();
        float distance = 0;
        Vector3 a = corners[0];
        foreach (Vector3 b in corners)
        {
            float segment = Vector3.Distance(a, b);
            distance += segment;
            if (distance > fromPointB)
            {
                float mid = distance - fromPointB;
                float t = mid / segment;
                Vector3 closest = Vector3.Lerp(a, b, t);
                return closest;
            }
            a = b;
        }
        return Vector3.zero;
    }

    public static Vector3 MapLayoutToWorld(this Vector2Int mapCoordinate, int blockSize)
    {
        Vector3 worldPosition;

        worldPosition = new Vector3(mapCoordinate.x * blockSize, 0, mapCoordinate.y * blockSize);

        return worldPosition;
    }

    public static Vector2Int WorldPositionToMapLayout(this Vector3 worldPosition, int blockSize)
    {
        Vector2Int mapCoordinate;

        mapCoordinate = new Vector2Int(Mathf.FloorToInt(worldPosition.x / blockSize), Mathf.FloorToInt(worldPosition.z / blockSize));

        return mapCoordinate;
    }


    public static float DistanceFromPoint(this NavMeshPath path, Vector3 point)
    {
        Vector3[] corners = path.corners;

        if (corners.Length < 2)
            return 100;

        List<float> distanceToAll = new List<float>();

        for (int i = 0; i < corners.Length - 1; i++)
        {
            Vector3 pointA = corners[i];
            Vector3 pointB = corners[i + 1];

            float angle = Vector3.Angle(pointB - pointA, point - pointA);
            float anotherAngle = Vector3.Angle(pointA - pointB, point - pointB);
            float distance;
            if (angle < 90 && anotherAngle < 90)
                distance = Mathf.Sin(angle * Mathf.Deg2Rad) * (point - pointA).magnitude;
            else
                distance = (point - pointA).magnitude;
            distanceToAll.Add(distance);
        }


        return distanceToAll.Min();

    }

    public static Texture2D ConvertToTexture2D(this Sprite sprite)
    {
        Texture2D tex = sprite.texture;
        Rect rect = sprite.rect;
        if (tex.isReadable && sprite.rect.width != sprite.texture.width)
        {
            Rect textureRect = sprite.textureRect;
            Texture2D newText = new Texture2D((int)textureRect.width, (int)textureRect.height);
            Color[] newColors = tex.GetPixels((int)textureRect.x,
                                                         (int)textureRect.y,
                                                         (int)textureRect.width,
                                                         (int)textureRect.height, 0);
            newText.SetPixels(newColors);
            newText.Apply();
            return newText;
        }
        else
            return tex;
    }
    public static bool RayCastPoint(this Plane plane, Ray ray, out Vector3 position)
    {
        position = Vector3.zero;
        if (plane.Raycast(ray, out float enter))
        {
            position = ray.GetPoint(enter);
            return true;
        }
        else return false;
    }
    public static bool CompareApproximate(this Vector3 q1, Vector3 q2, float range)
    {
        return Mathf.Abs(Vector3.Dot(q1, q2)) > 1f - range;
    }
    public static bool CompareApproximate(this Quaternion q1, Quaternion q2, float range)
    {
        return Mathf.Abs(Quaternion.Dot(q1, q2)) > 1f - range;
    }

    public static string GetDescription(this System.Enum value)
    {
        FieldInfo fi = value.GetType().GetField(value.ToString());
        if (fi != null)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
                return attributes[0].Description;
        }
        return value.ToString();
    }

    public static void ChangeRenderMode(this Material standardShaderMaterial, BlendMode blendMode)
    {
        switch (blendMode)
        {
            case BlendMode.Opaque:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                standardShaderMaterial.SetInt("_ZWrite", 1);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = -1;
                break;
            case BlendMode.Cutout:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                standardShaderMaterial.SetInt("_ZWrite", 1);
                standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 2450;
                break;
            case BlendMode.Fade:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                standardShaderMaterial.SetInt("_ZWrite", 0);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 3000;
                break;
            case BlendMode.Transparent:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                standardShaderMaterial.SetInt("_ZWrite", 0);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 3000;
                break;
        }

    }

    public static bool IsValid(this object[] args, int minimumSize = 1)
    {
        if (args == null || args.Length < 1) return false;
        else return true;
    }

    public static T GetValue<T>(this object[] args, T value = default)
    {
        if (args.IsValid())
        {
            value = (T)args.FirstOrDefault(x => x != default && x is T);
        }
        return value;
    }

    public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
    {
        foreach (var i in ie)
        {
            action(i);
        }
    }
}

public enum BlendMode
{
    Opaque,
    Cutout,
    Fade,
    Transparent
}
