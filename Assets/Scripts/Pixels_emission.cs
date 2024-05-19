using UnityEngine;
using Unity.Entities;
using Unity.Collections;

public class Pixels_emission: MonoBehaviour
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

        for (int i = 0; i < pixelEntities.Length; i++)
        {
            int x = pixelPositionsFromQuery[i].Position.x;
            int y = pixelPositionsFromQuery[i].Position.y;
            int index = y * width + x;
            pixelColors[index] = new Color(pixelColorsFromQuery[i].Color.x, pixelColorsFromQuery[i].Color.y, pixelColorsFromQuery[i].Color.z, pixelColorsFromQuery[i].Color.w);
        }

        emissionTexture.SetPixels32(pixelColors.ToArray());
        emissionTexture.Apply();

        pixelEntities.Dispose();
        pixelColorsFromQuery.Dispose();
        pixelPositionsFromQuery.Dispose();
    }
}
