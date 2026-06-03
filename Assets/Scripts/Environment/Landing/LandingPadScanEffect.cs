using UnityEngine;

[DisallowMultipleComponent]
public class LandingPadScanEffect : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sortingSource;
    [SerializeField] private Vector2 outlineSize = new Vector2(3.35f, 0.34f);
    [SerializeField] private Vector2 localOffset = new Vector2(0f, 0.64f);
    [SerializeField] private float lineWidth = 0.035f;
    [SerializeField] private float segmentLength = 0.55f;
    [SerializeField] private float scanSpeed = 1.35f;
    [SerializeField] private Color scanColor = new Color(0.25f, 1f, 1f, 0.95f);
    [SerializeField] private int sortingOrderOffset = 2;

    private const string TopLineName = "LandingPad Scan Top";
    private const string BottomLineName = "LandingPad Scan Bottom";

    private LineRenderer topLine;
    private LineRenderer bottomLine;
    private Material lineMaterial;

    private void Awake()
    {
        CreateLines();
    }

    private void OnEnable()
    {
        CreateLines();
        SetLinesActive(true);
    }

    private void Update()
    {
        ApplyLiveSettings();
        UpdateScan();
    }

    private void OnDisable()
    {
        SetLinesActive(false);
    }

    private void OnDestroy()
    {
        if (lineMaterial == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(lineMaterial);
        }
        else
        {
            DestroyImmediate(lineMaterial);
        }
    }

    private void OnValidate()
    {
        outlineSize.x = Mathf.Max(0.01f, outlineSize.x);
        outlineSize.y = Mathf.Max(0f, outlineSize.y);
        lineWidth = Mathf.Max(0.001f, lineWidth);
        segmentLength = Mathf.Max(0.01f, segmentLength);
        scanSpeed = Mathf.Max(0f, scanSpeed);

        if (!Application.isPlaying)
        {
            return;
        }

        ApplyLiveSettings();
        UpdateScan();
    }

    private void CreateLines()
    {
        if (topLine != null && bottomLine != null)
        {
            return;
        }

        if (sortingSource == null)
        {
            sortingSource = GetComponentInChildren<SpriteRenderer>();
        }

        lineMaterial = new Material(GetLineShader());
        topLine = GetOrCreateLine(TopLineName);
        bottomLine = GetOrCreateLine(BottomLineName);
        UpdateSorting();
    }

    private LineRenderer GetOrCreateLine(string lineName)
    {
        Transform existing = transform.Find(lineName);
        GameObject lineObject = existing != null ? existing.gameObject : new GameObject(lineName);
        lineObject.transform.SetParent(transform, false);
        lineObject.transform.localPosition = Vector3.zero;
        lineObject.transform.localRotation = Quaternion.identity;
        lineObject.transform.localScale = Vector3.one;

        LineRenderer line = lineObject.GetComponent<LineRenderer>();
        if (line == null)
        {
            line = lineObject.AddComponent<LineRenderer>();
        }

        line.useWorldSpace = false;
        line.positionCount = 3;
        line.widthMultiplier = lineWidth;
        line.numCapVertices = 4;
        line.numCornerVertices = 4;
        line.textureMode = LineTextureMode.Stretch;
        line.material = lineMaterial;
        line.colorGradient = CreateScanGradient();

        return line;
    }

    private void ApplyLiveSettings()
    {
        if (topLine == null || bottomLine == null)
        {
            return;
        }

        ApplyLineSettings(topLine);
        ApplyLineSettings(bottomLine);
        UpdateSorting();
    }

    private void ApplyLineSettings(LineRenderer line)
    {
        line.widthMultiplier = lineWidth;
        line.colorGradient = CreateScanGradient();
    }

    private void UpdateScan()
    {
        if (topLine == null || bottomLine == null)
        {
            return;
        }

        float left = localOffset.x - outlineSize.x * 0.5f;
        float right = localOffset.x + outlineSize.x * 0.5f;
        float travelDistance = outlineSize.x + segmentLength;
        float startX = left - segmentLength + Mathf.Repeat(Time.time * scanSpeed, travelDistance);
        float endX = startX + segmentLength;
        float topY = localOffset.y + outlineSize.y * 0.5f;
        float bottomY = localOffset.y - outlineSize.y * 0.5f;

        SetSegment(topLine, startX, endX, left, right, topY);
        SetSegment(bottomLine, startX, endX, left, right, bottomY);
    }

    private void SetSegment(LineRenderer line, float startX, float endX, float left, float right, float y)
    {
        bool visible = endX > left && startX < right;
        line.enabled = visible;

        if (!visible)
        {
            return;
        }

        float clippedStart = Mathf.Max(startX, left);
        float clippedEnd = Mathf.Min(endX, right);
        float mid = (clippedStart + clippedEnd) * 0.5f;
        float z = -0.02f;

        line.widthMultiplier = lineWidth;
        line.SetPosition(0, new Vector3(clippedStart, y, z));
        line.SetPosition(1, new Vector3(mid, y, z));
        line.SetPosition(2, new Vector3(clippedEnd, y, z));
    }

    private void UpdateSorting()
    {
        if (sortingSource == null)
        {
            return;
        }

        ApplySorting(topLine);
        ApplySorting(bottomLine);
    }

    private void ApplySorting(LineRenderer line)
    {
        if (line == null)
        {
            return;
        }

        line.sortingLayerID = sortingSource.sortingLayerID;
        line.sortingOrder = sortingSource.sortingOrder + sortingOrderOffset;
    }

    private void SetLinesActive(bool active)
    {
        if (topLine != null)
        {
            topLine.gameObject.SetActive(active);
        }

        if (bottomLine != null)
        {
            bottomLine.gameObject.SetActive(active);
        }
    }

    private Gradient CreateScanGradient()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(scanColor, 0f),
                new GradientColorKey(Color.white, 0.5f),
                new GradientColorKey(scanColor, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(scanColor.a, 0.5f),
                new GradientAlphaKey(0f, 1f)
            });
        return gradient;
    }

    private Shader GetLineShader()
    {
        return Shader.Find("Sprites/Default")
               ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
               ?? Shader.Find("Unlit/Color");
    }
}
