using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

// FIP Variant 1 padel court — 20m × 10m
// Back walls:  3m solid glass + 1m weld mesh = 4m total
// Side walls:  3m glass (first 4m from each end) + 2m fence (middle 12m)
// Net:         0.88m at centre, 0.92m at posts

public static class PadelCourtBuilder
{
    [MenuItem("Tools/Build Padel Court")]
    static void Build()
    {
        var existing = GameObject.Find("PadelCourt");
        if (existing != null)
            Undo.DestroyObjectImmediate(existing);

        var root = new GameObject("PadelCourt");
        Undo.RegisterCreatedObjectUndo(root, "Build Padel Court");

        BuildFloor(root);
        BuildBackWalls(root);
        BuildSideWalls(root);
        BuildNet(root);
        BuildCourtLines(root);

        Selection.activeGameObject = root;
    }

    // ── Floor ─────────────────────────────────────────────────────────────────

    static void BuildFloor(GameObject root)
    {
        Opaque(Box(root, "Floor", Vector3.zero, new Vector3(10f, 0.02f, 20f)),
               new Color(0.10f, 0.36f, 0.20f));
    }

    // ── Back walls ────────────────────────────────────────────────────────────

    static void BuildBackWalls(GameObject root)
    {
        foreach (float z in new[] { 10f, -10f })
        {
            string s   = z > 0 ? "A" : "B";
            float face = z + Mathf.Sign(z) * 0.025f;

            // 3m solid glass panel
            Glass(Box(root, $"BackWall_{s}_Glass",
                      new Vector3(0f, 1.5f, face),
                      new Vector3(10f, 3f, 0.05f)),
                  new Color(0.72f, 0.91f, 1f, 0.22f));

            // 1m weld mesh above glass (y 3.0 → 4.0)
            FencePanel(root, $"BackWall_{s}_Fence",
                       new Vector3(0f, 3.5f, face),
                       spanLength: 10f, spanHeight: 1f, alongX: true);

            // Corner pillars at full back-wall height (4m)
            Pillar(root, $"Pillar_{s}_L", new Vector3(-5f, 2f, z), 4f);
            Pillar(root, $"Pillar_{s}_R", new Vector3( 5f, 2f, z), 4f);
        }
    }

    // ── Side walls ────────────────────────────────────────────────────────────
    // Stepped profile (Variant 1 UK):
    //   Step 1 — first 2m from each end:  3m glass + 1m fence = 4m total
    //   Step 2 — next 2m inward:          2m glass + 1m fence = 3m total
    //   Middle — remaining 12m (net zone): 3m metallic mesh fence

    static void BuildSideWalls(GameObject root)
    {
        foreach (float x in new[] { -5f, 5f })
        {
            string side = x < 0 ? "L" : "R";
            float face  = x + Mathf.Sign(x) * 0.025f;

            foreach (float zSign in new[] { 1f, -1f })
            {
                string ab = zSign > 0 ? "A" : "B";

                // Step 1: z ±8 to ±10 (2m wide) — 3m glass + 1m fence = 4m
                Glass(Box(root, $"SideWall_{side}_{ab}_S1_Glass",
                          new Vector3(face, 1.5f, zSign * 9f),
                          new Vector3(0.05f, 3f, 2f)),
                      new Color(0.72f, 0.91f, 1f, 0.18f));
                FencePanel(root, $"SideWall_{side}_{ab}_S1_Fence",
                           new Vector3(face, 3.5f, zSign * 9f),
                           spanLength: 2f, spanHeight: 1f, alongX: false);

                // Step 2: z ±6 to ±8 (2m wide) — 2m glass + 1m fence = 3m
                Glass(Box(root, $"SideWall_{side}_{ab}_S2_Glass",
                          new Vector3(face, 1f, zSign * 7f),
                          new Vector3(0.05f, 2f, 2f)),
                      new Color(0.72f, 0.91f, 1f, 0.18f));
                FencePanel(root, $"SideWall_{side}_{ab}_S2_Fence",
                           new Vector3(face, 2.5f, zSign * 7f),
                           spanLength: 2f, spanHeight: 1f, alongX: false);

                // Pillars at step boundaries
                Pillar(root, $"Pillar_{side}_{ab}_s12", new Vector3(x, 2f,   zSign * 8f), 4f); // step 1→2
                Pillar(root, $"Pillar_{side}_{ab}_s2f", new Vector3(x, 1.5f, zSign * 6f), 3f); // step 2→fence
            }

            // Middle: 3m metallic mesh fence — z −6 to +6 (12m)
            FencePanel(root, $"SideWall_{side}_Fence_Center",
                       new Vector3(face, 1.5f, 0f),
                       spanLength: 12f, spanHeight: 3f, alongX: false);
        }
    }

    // ── Net ───────────────────────────────────────────────────────────────────

    static void BuildNet(GameObject root)
    {
        // Net body — 0.88m at centre
        Opaque(Box(root, "Net",
                   new Vector3(0f, 0.44f, 0f),
                   new Vector3(10f, 0.88f, 0.015f)),
               new Color(0.10f, 0.10f, 0.10f));

        // White top band
        Opaque(Box(root, "NetBand",
                   new Vector3(0f, 0.895f, 0f),
                   new Vector3(10f, 0.03f, 0.02f)),
               Color.white);

        // Posts at side walls — 0.92m height (spec)
        foreach (float x in new[] { -5f, 5f })
        {
            Opaque(Box(root, $"NetPost_{(x < 0 ? "L" : "R")}",
                       new Vector3(x, 0.46f, 0f),
                       new Vector3(0.06f, 0.92f, 0.06f)),
                   new Color(0.75f, 0.75f, 0.75f));
        }
    }

    // ── Court lines ───────────────────────────────────────────────────────────

    static void BuildCourtLines(GameObject root)
    {
        const float h = 0.011f; // just above floor surface
        const float w = 0.05f;  // line width

        Line(root, "Baseline_A",      new Vector3( 0f, h,    10f), new Vector3(10f, 0.005f,    w));
        Line(root, "Baseline_B",      new Vector3( 0f, h,   -10f), new Vector3(10f, 0.005f,    w));
        Line(root, "Sideline_L",      new Vector3(-5f, h,     0f), new Vector3(  w, 0.005f,  20f));
        Line(root, "Sideline_R",      new Vector3( 5f, h,     0f), new Vector3(  w, 0.005f,  20f));
        Line(root, "ServiceLine_A",   new Vector3( 0f, h,  6.95f), new Vector3(10f, 0.005f,    w));
        Line(root, "ServiceLine_B",   new Vector3( 0f, h, -6.95f), new Vector3(10f, 0.005f,    w));
        Line(root, "CenterLine_A",    new Vector3( 0f, h, 3.475f), new Vector3(  w, 0.005f, 6.95f));
        Line(root, "CenterLine_B",    new Vector3( 0f, h,-3.475f), new Vector3(  w, 0.005f, 6.95f));
    }

    // ── Fence panel ───────────────────────────────────────────────────────────
    // Thin mesh fill + top/bottom rails + evenly spaced posts

    static void FencePanel(GameObject root, string name, Vector3 center,
                           float spanLength, float spanHeight, bool alongX)
    {
        var metalColor = new Color(0.55f, 0.55f, 0.55f);

        // Mesh fill
        Vector3 fillScale = alongX
            ? new Vector3(spanLength, spanHeight, 0.01f)
            : new Vector3(0.01f, spanHeight, spanLength);
        FenceMesh(Box(root, $"{name}_Fill", center, fillScale));

        // Top and bottom rails
        float halfH = spanHeight * 0.5f - 0.025f;
        Vector3 railScale = alongX
            ? new Vector3(spanLength, 0.05f, 0.04f)
            : new Vector3(0.04f, 0.05f, spanLength);
        Opaque(Box(root, $"{name}_RailTop", center + Vector3.up *  halfH, railScale), metalColor);
        Opaque(Box(root, $"{name}_RailBot", center + Vector3.up * -halfH, railScale), metalColor);

        // Vertical posts every ~2m
        int posts = Mathf.Max(2, Mathf.RoundToInt(spanLength / 2f) + 1);
        for (int i = 0; i < posts; i++)
        {
            float t = (float)i / (posts - 1) - 0.5f;
            Vector3 offset = alongX
                ? new Vector3(t * spanLength, 0f, 0f)
                : new Vector3(0f, 0f, t * spanLength);
            Opaque(Box(root, $"{name}_Post_{i}",
                       center + offset,
                       new Vector3(0.04f, spanHeight, 0.04f)),
                   metalColor);
        }
    }

    static void Pillar(GameObject root, string name, Vector3 center, float height)
    {
        Opaque(Box(root, name, center, new Vector3(0.08f, height, 0.08f)),
               new Color(0.55f, 0.55f, 0.55f));
    }

    // ── Primitives & materials ────────────────────────────────────────────────

    static GameObject Box(GameObject parent, string name, Vector3 pos, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        return go;
    }

    static void Line(GameObject root, string name, Vector3 pos, Vector3 scale)
        => Opaque(Box(root, name, pos, scale), Color.white);

    static void Opaque(GameObject go, Color color)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", color);
        go.GetComponent<Renderer>().material = mat;
    }

    static void Glass(GameObject go, Color color)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = (int)RenderQueue.Transparent;
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        go.GetComponent<Renderer>().material = mat;
    }

    static void FenceMesh(GameObject go)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", new Color(0.22f, 0.22f, 0.22f, 0.30f));
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = (int)RenderQueue.Transparent;
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        go.GetComponent<Renderer>().material = mat;
    }
}
