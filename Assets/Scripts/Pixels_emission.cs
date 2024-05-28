using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;




public class Pixels_emission : MonoBehaviour
{
    public int width = 500;
    public int height = 500;
    private Texture2D emissionTexture;
    public Renderer rendererComponent;

    private EntityManager entityManager;
    private EntityQuery pixelQuery;
    private NativeArray<Color32> pixelColors;

    void Start()
    {
        emissionTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        emissionTexture.filterMode = FilterMode.Point;
        rendererComponent.material.SetTexture("_EmissionMap", emissionTexture);
        rendererComponent.material.EnableKeyword("_EMISSION"); // Enable emission

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        pixelQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<PixelColor>(), ComponentType.ReadOnly<PixelPosition>());

        pixelColors = new NativeArray<Color32>(width * height, Allocator.Persistent);
    }

    void Update()
    {
        UpdateEmissionTexture();
    }

    void OnDestroy()
    {
        pixelColors.Dispose();
    }

    void UpdateEmissionTexture()
    {
        var pixelEntities = pixelQuery.ToEntityArray(Allocator.TempJob);
        var pixelColorsFromQuery = pixelQuery.ToComponentDataArray<PixelColor>(Allocator.TempJob);
        var pixelPositionsFromQuery = pixelQuery.ToComponentDataArray<PixelPosition>(Allocator.TempJob);

        var job = new UpdateTextureJob
        {
            Width = width,
            Height = height,
            PixelColorsFromQuery = pixelColorsFromQuery,
            PixelPositionsFromQuery = pixelPositionsFromQuery,
            PixelColors = pixelColors
        };

        job.Schedule(pixelColors.Length, 64).Complete();

        emissionTexture.SetPixels32(pixelColors.ToArray());
        emissionTexture.Apply();

        pixelEntities.Dispose();
        pixelColorsFromQuery.Dispose();
        pixelPositionsFromQuery.Dispose();
    }

    [BurstCompile]
    private struct UpdateTextureJob : IJobParallelFor
    {
        public int Width;
        public int Height;
        [ReadOnly] public NativeArray<PixelColor> PixelColorsFromQuery;
        [ReadOnly] public NativeArray<PixelPosition> PixelPositionsFromQuery;
        public NativeArray<Color32> PixelColors;

        public void Execute(int index)
        {
            int x = PixelPositionsFromQuery[index].Position.x;
            int y = PixelPositionsFromQuery[index].Position.y;
            int pixelIndex = y * Width + x;
            PixelColors[pixelIndex] = new Color32(
                (byte)(PixelColorsFromQuery[index].Color.x * 255),
                (byte)(PixelColorsFromQuery[index].Color.y * 255),
                (byte)(PixelColorsFromQuery[index].Color.z * 255),
                (byte)(PixelColorsFromQuery[index].Color.w * 255)
            );
        }
    }
}


