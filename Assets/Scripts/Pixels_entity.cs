using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public struct PixelColor : IComponentData
{
    public float4 Color;
}

public struct PixelPosition : IComponentData
{
    public int2 Position;
}

public partial class Pixel_system_entity : SystemBase
{
    protected override void OnCreate()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityArchetype pixelArchetype = entityManager.CreateArchetype(
            typeof(PixelColor),
            typeof(PixelPosition)
        );

        NativeArray<Entity> pixelEntities = new NativeArray<Entity>(500 * 500, Allocator.Temp);
        entityManager.CreateEntity(pixelArchetype, pixelEntities);

        for (int y = 0; y < 500; y++)
        {
            for (int x = 0; x < 500; x++)
            {
                int index = y * 500 + x;
                Entity pixelEntity = pixelEntities[index];
                entityManager.SetComponentData(pixelEntity, new PixelPosition { Position = new int2(x, y) });
                entityManager.SetComponentData(pixelEntity, new PixelColor { Color = new float4(1, 0, 0, 1) });
            }
        }

        pixelEntities.Dispose();
    }

    protected override void OnUpdate() { }
}

