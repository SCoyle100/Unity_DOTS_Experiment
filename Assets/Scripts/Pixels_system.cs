using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

public partial class Pixels_system : SystemBase
{
    private float animationSpeed = 20f;
    private float time;

    protected override void OnUpdate()
    {
        // Capture values into local variables to avoid capturing 'this'
        float localTime = time += SystemAPI.Time.DeltaTime * animationSpeed;
        float centerX = 250; // Assuming a width of 50
        float centerY = 250; // Assuming a height of 40
        float maxRadius = math.sqrt(centerX * centerX + centerY * centerY);

        Entities
            .WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
            .ForEach((ref PixelColor pixelColor, in PixelPosition pixelPosition) =>
            {
                float2 position = pixelPosition.Position;
                float distance = math.distance(position, new float2(centerX, centerY));

                if (distance < maxRadius)
                {
                    float angle = math.radians(localTime * (maxRadius / 2 - (distance % (maxRadius / 2))));
                    float4 color = math.lerp(new float4(1, 0, 0, 1), new float4(0, 0, 1, 1), (distance % (maxRadius / 2)) / (maxRadius / 2));
                    color = math.lerp(color, new float4(math.sin(distance % (maxRadius / 2) / (maxRadius / 2)), 1, 1, 1), 0.5f);
                    pixelColor.Color = color;
                }

            }).ScheduleParallel();
    }
}

