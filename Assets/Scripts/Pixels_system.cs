using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

public partial class Pixels_system : SystemBase
{
    private float _animationSpeed = 20f; // Renamed to avoid ambiguity
    private float _currentTime; // Renamed to avoid ambiguity

    protected override void OnUpdate()
    {
        float localAnimationSpeed = _animationSpeed;
        float deltaTime = SystemAPI.Time.DeltaTime;
        _currentTime += deltaTime * localAnimationSpeed;
        float localCurrentTime = _currentTime;
        float centerX = 250; // Assuming a width of 500
        float centerY = 250; // Assuming a height of 500
        float maxRadius = math.sqrt(centerX * centerX + centerY * centerY);

        Debug.Log("Updating Pixels_system: " + localCurrentTime);

        Entities
            .WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
            .ForEach((ref PixelColor pixelColor, in PixelPosition pixelPosition) =>
            {
                float2 position = pixelPosition.Position;
                float distance = math.distance(position, new float2(centerX, centerY));

                if (distance < maxRadius)
                {
                    float angle = math.radians(localCurrentTime * 10f) + distance * math.PI;
                    float sineValue = math.sin(angle);
                    float cosineValue = math.cos(angle);

                    float4 color = new float4(
                        (sineValue + 1) / 2, // Red channel oscillates between 0 and 1
                        (cosineValue + 1) / 2, // Green channel oscillates between 0 and 1
                        (sineValue * cosineValue + 1) / 2, // Blue channel combines sine and cosine
                        1 // Alpha channel remains 1
                    );

                    pixelColor.Color = color;

                    // Log the color changes for a specific pixel (optional)
                    if (pixelPosition.Position.x == 250 && pixelPosition.Position.y == 250) // Log for center pixel
                    {
                        Debug.Log($"Pixel at (250, 250): {color}");
                    }
                }

            }).ScheduleParallel();
    }
}

