using UnityEngine;

namespace BobbysMusicPlayer.Extensions;

public static class SmoothExtensions
{
    public static float SmoothTowards(this float current, float target, float deltaTime, float speed)
    {
        return Mathf.Lerp(current, target, deltaTime * speed);
    }
}