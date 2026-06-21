#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class CustomParticlePrefabCreator
{
    private const string VfxFolder = "Assets/Textures/VFX";
    private const string MaterialFolder = VfxFolder + "/Materials";
    private const string PrefabFolder = "Assets/Prefabs/VFX";
    private const string TexturePath = VfxFolder + "/SoftParticle_Custom.png";
    private const string AdditiveMaterialPath = MaterialFolder + "/M_CustomParticle_Additive.mat";
    private const string AlphaMaterialPath = MaterialFolder + "/M_CustomParticle_Alpha.mat";
    private const string ThrusterPrefabPath = PrefabFolder + "/CustomThrusterParticleSystem.prefab";
    private const string ExplosionPrefabPath = PrefabFolder + "/CustomExplosionParticleSystem.prefab";

    [InitializeOnLoadMethod]
    private static void CreateMissingPrefabsOnImport()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(ThrusterPrefabPath) != null &&
            AssetDatabase.LoadAssetAtPath<GameObject>(ExplosionPrefabPath) != null)
        {
            return;
        }

        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                CreateCustomParticlePrefabs();
            }
        };
    }

    [MenuItem("Tools/VFX/Create Custom Thruster And Explosion")]
    public static void CreateCustomParticlePrefabs()
    {
        EnsureFolder("Assets/Textures", "VFX");
        EnsureFolder(VfxFolder, "Materials");
        EnsureFolder("Assets/Prefabs", "VFX");

        Texture2D particleTexture = CreateSoftParticleTexture();
        Material additiveMaterial = CreateParticleMaterial(
            AdditiveMaterialPath,
            particleTexture,
            true
        );
        Material alphaMaterial = CreateParticleMaterial(
            AlphaMaterialPath,
            particleTexture,
            false
        );

        CreateThrusterPrefab(additiveMaterial);
        CreateExplosionPrefab(additiveMaterial, alphaMaterial);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(ThrusterPrefabPath);
        EditorGUIUtility.PingObject(Selection.activeObject);

        Debug.Log(
            "Created custom particle prefabs:\n" +
            ThrusterPrefabPath + "\n" +
            ExplosionPrefabPath
        );
    }

    private static Texture2D CreateSoftParticleTexture()
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "SoftParticle_Custom";

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 uv = new Vector2(
                    (x + 0.5f) / size * 2f - 1f,
                    (y + 0.5f) / size * 2f - 1f
                );
                float alpha = Mathf.Clamp01(1f - uv.magnitude);
                alpha = alpha * alpha * (3f - 2f * alpha);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        File.WriteAllBytes(TexturePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
    }

    private static Material CreateParticleMaterial(
        string path,
        Texture2D texture,
        bool additive
    )
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Unlit");
        }

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material.shader = shader;
        }

        material.name = Path.GetFileNameWithoutExtension(path);
        material.mainTexture = texture;
        material.SetTexture("_BaseMap", texture);
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", additive ? 2f : 0f);
        material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
        material.SetFloat("_ZWrite", 0f);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)RenderQueue.Transparent;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void CreateThrusterPrefab(Material additiveMaterial)
    {
        GameObject root = new GameObject("CustomThrusterParticleSystem", typeof(ParticleSystem));

        ParticleSystem core = root.GetComponent<ParticleSystem>();
        ConfigureRenderer(core, additiveMaterial, 12);
        ConfigureDirectionalLayer(
            core,
            68f,
            new Vector2(0.16f, 0.28f),
            new Vector2(3.4f, 5.5f),
            new Vector2(0.17f, 0.29f),
            CreateGradient(
                new Color(1f, 1f, 1f, 1f),
                new Color(0.05f, 0.95f, 1f, 0.78f),
                new Color(0.02f, 0.25f, 1f, 0f)
            ),
            0.035f
        );
        ConfigureStretchedRenderer(core, 1.85f, 0.15f);

        ParticleSystem glow = CreateParticleLayer(root.transform, "OuterGlow", additiveMaterial, 11);
        ConfigureDirectionalLayer(
            glow,
            34f,
            new Vector2(0.24f, 0.42f),
            new Vector2(1.7f, 3.2f),
            new Vector2(0.32f, 0.58f),
            CreateGradient(
                new Color(0.05f, 0.9f, 1f, 0.47f),
                new Color(0.05f, 0.25f, 1f, 0.3f),
                new Color(0f, 0.05f, 0.35f, 0f)
            ),
            0.055f
        );

        ParticleSystem sparks = CreateParticleLayer(root.transform, "Sparks", additiveMaterial, 13);
        ConfigureDirectionalLayer(
            sparks,
            14f,
            new Vector2(0.16f, 0.34f),
            new Vector2(3.8f, 6.4f),
            new Vector2(0.034f, 0.072f),
            CreateGradient(
                new Color(0.8f, 1f, 1f, 1f),
                new Color(0.05f, 0.8f, 1f, 0.9f),
                new Color(0f, 0.25f, 1f, 0f)
            ),
            0.018f
        );
        ConfigureStretchedRenderer(sparks, 1.28f, 0.1f);

        PrefabUtility.SaveAsPrefabAsset(root, ThrusterPrefabPath);
        Object.DestroyImmediate(root);
    }

    private static void ConfigureDirectionalLayer(
        ParticleSystem particleSystem,
        float emissionRate,
        Vector2 lifetime,
        Vector2 speed,
        Vector2 size,
        Gradient gradient,
        float spawnRadius
    )
    {
        ParticleSystem.MainModule main = particleSystem.main;
        main.loop = true;
        main.playOnAwake = true;
        // World space leaves emitted particles behind while the ship rotates,
        // so new particles visibly show the current thrust direction.
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
        main.maxParticles = Mathf.CeilToInt(emissionRate * lifetime.y * 2f) + 8;

        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.enabled = true;
        emission.rateOverTime = emissionRate;

        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = spawnRadius;

        ParticleSystem.VelocityOverLifetimeModule velocity = particleSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
        velocity.y = new ParticleSystem.MinMaxCurve(-speed.y, -speed.x);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        ParticleSystem.ColorOverLifetimeModule color = particleSystem.colorOverLifetime;
        color.enabled = true;
        color.color = gradient;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
            1f,
            new AnimationCurve(
                new Keyframe(0f, 0.35f),
                new Keyframe(0.15f, 1f),
                new Keyframe(1f, 0.08f)
            )
        );
    }

    private static void CreateExplosionPrefab(Material additiveMaterial, Material alphaMaterial)
    {
        GameObject root = new GameObject("CustomExplosionParticleSystem");
        root.AddComponent<ParticleEffectAutoDestroy>().SetLifetime(2.8f);

        ParticleSystem flash = CreateParticleLayer(root.transform, "Flash", additiveMaterial, 24);
        ConfigureBurstLayer(
            flash,
            3,
            4,
            new Vector2(0.1f, 0.18f),
            Vector2.zero,
            new Vector2(2.4f, 3.4f),
            CreateGradient(Color.white, new Color(0.1f, 0.9f, 1f, 0.75f), Color.clear),
            ParticleSystemShapeType.Circle,
            0.02f
        );

        ParticleSystem fire = CreateParticleLayer(root.transform, "FireBurst", additiveMaterial, 22);
        ConfigureBurstLayer(
            fire,
            42,
            56,
            new Vector2(0.45f, 0.9f),
            new Vector2(3.5f, 8f),
            new Vector2(0.28f, 0.68f),
            CreateGradient(
                new Color(1f, 0.95f, 0.55f, 1f),
                new Color(1f, 0.22f, 0.02f, 0.9f),
                new Color(0.25f, 0.01f, 0f, 0f)
            ),
            ParticleSystemShapeType.Circle,
            0.2f
        );

        ParticleSystem sparks = CreateParticleLayer(root.transform, "Sparks", additiveMaterial, 25);
        ConfigureBurstLayer(
            sparks,
            32,
            44,
            new Vector2(0.5f, 1.15f),
            new Vector2(6f, 12f),
            new Vector2(0.045f, 0.12f),
            CreateGradient(
                new Color(1f, 1f, 0.75f, 1f),
                new Color(1f, 0.35f, 0.02f, 1f),
                new Color(0.5f, 0.02f, 0f, 0f)
            ),
            ParticleSystemShapeType.Circle,
            0.14f
        );
        ConfigureStretchedRenderer(sparks, 1.8f, 0.16f);

        ParticleSystem smoke = CreateParticleLayer(root.transform, "Smoke", alphaMaterial, 20);
        ConfigureBurstLayer(
            smoke,
            14,
            20,
            new Vector2(1f, 1.9f),
            new Vector2(0.8f, 2.4f),
            new Vector2(0.7f, 1.35f),
            CreateGradient(
                new Color(0.18f, 0.28f, 0.42f, 0.5f),
                new Color(0.04f, 0.06f, 0.1f, 0.35f),
                new Color(0.01f, 0.01f, 0.02f, 0f)
            ),
            ParticleSystemShapeType.Circle,
            0.28f
        );

        ParticleSystem.SizeOverLifetimeModule smokeSize = smoke.sizeOverLifetime;
        smokeSize.enabled = true;
        smokeSize.size = new ParticleSystem.MinMaxCurve(
            1f,
            new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 1.65f))
        );

        PrefabUtility.SaveAsPrefabAsset(root, ExplosionPrefabPath);
        Object.DestroyImmediate(root);
    }

    private static void ConfigureBurstLayer(
        ParticleSystem particleSystem,
        short minCount,
        short maxCount,
        Vector2 lifetime,
        Vector2 speed,
        Vector2 size,
        Gradient gradient,
        ParticleSystemShapeType shapeType,
        float radius
    )
    {
        ParticleSystem.MainModule main = particleSystem.main;
        main.loop = false;
        main.playOnAwake = true;
        main.duration = 1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed.x, speed.y);
        main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
        main.maxParticles = maxCount + 5;

        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, minCount, maxCount) });

        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = shapeType;
        shape.radius = radius;
        shape.radiusThickness = 1f;

        ParticleSystem.ColorOverLifetimeModule color = particleSystem.colorOverLifetime;
        color.enabled = true;
        color.color = gradient;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
            1f,
            new AnimationCurve(
                new Keyframe(0f, 0.2f),
                new Keyframe(0.12f, 1f),
                new Keyframe(1f, 0f)
            )
        );
    }

    private static ParticleSystem CreateParticleLayer(
        Transform parent,
        string name,
        Material material,
        int sortingOrder
    )
    {
        GameObject layer = new GameObject(name, typeof(ParticleSystem));
        layer.transform.SetParent(parent, false);

        ParticleSystem particleSystem = layer.GetComponent<ParticleSystem>();
        ConfigureRenderer(particleSystem, material, sortingOrder);
        return particleSystem;
    }

    private static void ConfigureRenderer(
        ParticleSystem particleSystem,
        Material material,
        int sortingOrder
    )
    {
        ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.material = material;
        renderer.sortingOrder = sortingOrder;
    }

    private static void ConfigureStretchedRenderer(
        ParticleSystem particleSystem,
        float lengthScale,
        float velocityScale
    )
    {
        ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = lengthScale;
        renderer.velocityScale = velocityScale;
    }

    private static Gradient CreateGradient(Color start, Color middle, Color end)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(start, 0f),
                new GradientColorKey(middle, 0.35f),
                new GradientColorKey(end, 1f)
            },
            new[]
            {
                new GradientAlphaKey(start.a, 0f),
                new GradientAlphaKey(middle.a, 0.35f),
                new GradientAlphaKey(end.a, 1f)
            }
        );
        return gradient;
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
