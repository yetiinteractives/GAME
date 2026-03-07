using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Editor tool that scans every material in the project for missing or
/// Built-in pipeline shaders and converts them to URP equivalents.
///
/// Menu: Tools → Fix Pink Materials → Scan Only          (dry run, all Assets)
///       Tools → Fix Pink Materials → Scan and Fix       (fix all Assets)
///       Tools → Fix Pink Materials → _FromStore Scan    (dry run, _FromStore only)
///       Tools → Fix Pink Materials → _FromStore Fix     (fix _FromStore only)
/// </summary>
public static class FixPinkMaterials
{
    // ──────────── Shader name mappings: Built-in / third-party → URP ────────────

    private static readonly Dictionary<string, string> ShaderMap = new Dictionary<string, string>
    {
        // Standard
        { "Standard",                                         "Universal Render Pipeline/Lit" },
        { "Standard (Specular setup)",                        "Universal Render Pipeline/Lit" },

        // Mobile
        { "Mobile/Diffuse",                                   "Universal Render Pipeline/Simple Lit" },
        { "Mobile/Bumped Diffuse",                            "Universal Render Pipeline/Simple Lit" },
        { "Mobile/Bumped Specular",                           "Universal Render Pipeline/Simple Lit" },
        { "Mobile/Unlit (Supports Lightmap)",                 "Universal Render Pipeline/Simple Lit" },
        { "Mobile/VertexLit",                                 "Universal Render Pipeline/Simple Lit" },
        { "Mobile/Particles/Additive",                        "Universal Render Pipeline/Particles/Unlit" },
        { "Mobile/Particles/Alpha Blended",                   "Universal Render Pipeline/Particles/Unlit" },
        { "Mobile/Particles/Multiply",                        "Universal Render Pipeline/Particles/Unlit" },
        { "Mobile/Particles/VertexLit Blended",               "Universal Render Pipeline/Particles/Lit" },

        // Legacy
        { "Legacy Shaders/Diffuse",                           "Universal Render Pipeline/Simple Lit" },
        { "Legacy Shaders/Specular",                          "Universal Render Pipeline/Simple Lit" },
        { "Legacy Shaders/Bumped Diffuse",                    "Universal Render Pipeline/Simple Lit" },
        { "Legacy Shaders/Bumped Specular",                   "Universal Render Pipeline/Simple Lit" },
        { "Legacy Shaders/Transparent/Diffuse",               "Universal Render Pipeline/Lit" },
        { "Legacy Shaders/Transparent/Specular",              "Universal Render Pipeline/Lit" },
        { "Legacy Shaders/Transparent/Bumped Diffuse",        "Universal Render Pipeline/Lit" },
        { "Legacy Shaders/Transparent/Cutout/Diffuse",        "Universal Render Pipeline/Lit" },
        { "Legacy Shaders/Transparent/Cutout/Bumped Diffuse", "Universal Render Pipeline/Lit" },
        { "Legacy Shaders/Self-Illumin/Diffuse",              "Universal Render Pipeline/Simple Lit" },
        { "Legacy Shaders/Self-Illumin/Specular",             "Universal Render Pipeline/Simple Lit" },
        { "Legacy Shaders/Reflective/Diffuse",                "Universal Render Pipeline/Lit" },
        { "Legacy Shaders/Reflective/Specular",               "Universal Render Pipeline/Lit" },
        { "Legacy Shaders/VertexLit",                         "Universal Render Pipeline/Simple Lit" },

        // Nature
        { "Nature/Tree Soft Occlusion Leaves",                "Universal Render Pipeline/Simple Lit" },
        { "Nature/Tree Soft Occlusion Bark",                  "Universal Render Pipeline/Simple Lit" },
        { "Nature/SpeedTree",                                 "Universal Render Pipeline/Simple Lit" },

        // Unlit
        { "Unlit/Color",                                      "Universal Render Pipeline/Unlit" },
        { "Unlit/Texture",                                    "Universal Render Pipeline/Unlit" },
        { "Unlit/Transparent",                                "Universal Render Pipeline/Unlit" },
        { "Unlit/Transparent Cutout",                         "Universal Render Pipeline/Unlit" },

        // Particles
        { "Particles/Standard Surface",                       "Universal Render Pipeline/Particles/Lit" },
        { "Particles/Standard Unlit",                         "Universal Render Pipeline/Particles/Unlit" },
        { "Particles/Additive",                               "Universal Render Pipeline/Particles/Unlit" },
        { "Particles/Alpha Blended",                          "Universal Render Pipeline/Particles/Unlit" },
        { "Particles/Multiply",                               "Universal Render Pipeline/Particles/Unlit" },
        { "Particles/Alpha Blended Premultiply",              "Universal Render Pipeline/Particles/Unlit" },
        { "Particles/Additive (Soft)",                        "Universal Render Pipeline/Particles/Unlit" },
        { "Particles/VertexLit Blended",                      "Universal Render Pipeline/Particles/Lit" },

        // Skybox (usually fine, but map the common one)
        { "Skybox/Procedural",                                "Skybox/Procedural" },

        // UI — normally fine under URP, but map the legacy variant
        { "UI/Default",                                       "UI/Default" },

        // WarFX (third-party particle/effect shaders — Built-in surface shaders, pink in URP)
        { "WFX/Additive Alpha8",                              "Universal Render Pipeline/Particles/Unlit" },
        { "WFX/Alpha Blended (No Soft Particles)",            "Universal Render Pipeline/Particles/Unlit" },
        { "WFX/Scroll/Smoke",                                 "Universal Render Pipeline/Particles/Unlit" },
        { "WFX/Scroll/Additive",                              "Universal Render Pipeline/Particles/Unlit" },
        { "WFX/Scroll/Multiply Soft Tint",                    "Universal Render Pipeline/Particles/Unlit" },
        { "WFX/Scroll/Alpha Blended",                         "Universal Render Pipeline/Particles/Unlit" },
        { "WFX/Transparent Diffuse",                          "Universal Render Pipeline/Simple Lit" },
        { "WFX/Transparent Specular",                         "Universal Render Pipeline/Simple Lit" },
        { "WFX/Multiply Soft Tint",                           "Universal Render Pipeline/Particles/Unlit" },
        { "WFX/Multiply Alpha8",                              "Universal Render Pipeline/Particles/Unlit" },
        { "WFX/Additive (Soft) Alpha8",                       "Universal Render Pipeline/Particles/Unlit" },

        // Error shader → pink material
        { "Hidden/InternalErrorShader",                       "Universal Render Pipeline/Lit" },
    };

    // Shaders that are valid under URP — skip these during the scan.
    private static readonly HashSet<string> URPSafe = new HashSet<string>
    {
        "Universal Render Pipeline/Lit",
        "Universal Render Pipeline/Simple Lit",
        "Universal Render Pipeline/Unlit",
        "Universal Render Pipeline/Baked Lit",
        "Universal Render Pipeline/Complex Lit",
        "Universal Render Pipeline/Particles/Lit",
        "Universal Render Pipeline/Particles/Simple Lit",
        "Universal Render Pipeline/Particles/Unlit",
        "Universal Render Pipeline/Terrain/Lit",
        "Universal Render Pipeline/Nature/SpeedTree7",
        "Universal Render Pipeline/Nature/SpeedTree8",
        "Universal Render Pipeline/2D/Sprite-Lit-Default",
        "Universal Render Pipeline/2D/Sprite-Unlit-Default",
        "Skybox/Procedural",
        "Skybox/6 Sided",
        "Skybox/Cubemap",
        "Skybox/Panoramic",
        "UI/Default",
        "UI/Default Font",
        "TextMeshPro/Mobile/Distance Field",
        "TextMeshPro/Distance Field",
        "TextMeshPro/Mobile/Distance Field SSD",
        "TextMeshPro/Distance Field SSD",
        "TextMeshPro/Mobile/Distance Field Overlay",
        "TextMeshPro/Mobile/Bitmap",
        "TextMeshPro/Bitmap",
        "Sprites/Default",
        "Sprites/Diffuse",
        "Hidden/Universal Render Pipeline/FallbackError",
    };

    // ──────────── Menu Items ────────────

    [MenuItem("Tools/Fix Pink Materials/Scan Only (Dry Run)")]
    private static void ScanOnly()
    {
        Run(dryRun: true, pathFilter: null);
    }

    [MenuItem("Tools/Fix Pink Materials/Scan and Fix")]
    private static void ScanAndFix()
    {
        Run(dryRun: false, pathFilter: null);
    }

    [MenuItem("Tools/Fix Pink Materials/_FromStore Scan (Dry Run)")]
    private static void ScanFromStoreOnly()
    {
        Run(dryRun: true, pathFilter: "Assets/_FromStore/");
    }

    [MenuItem("Tools/Fix Pink Materials/_FromStore Fix")]
    private static void FixFromStore()
    {
        Run(dryRun: false, pathFilter: "Assets/_FromStore/");
    }

    // ──────────── Core ────────────

    private static void Run(bool dryRun, string pathFilter)
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");

        int scanned = 0;
        int alreadyOk = 0;
        int fixed_ = 0;
        int customSkipped = 0;
        var report = new System.Text.StringBuilder();

        string scope = pathFilter != null ? pathFilter : "All Assets";
        report.AppendLine("═══════════════════════════════════════════════════");
        report.AppendLine(dryRun
            ? $"  FIX PINK MATERIALS — DRY RUN  [{scope}]"
            : $"  FIX PINK MATERIALS — APPLYING FIXES  [{scope}]");
        report.AppendLine("═══════════════════════════════════════════════════");
        report.AppendLine();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Skip packages — we only fix project materials
            if (path.StartsWith("Packages/"))
                continue;

            // If a path filter is set, only process materials under that path
            if (pathFilter != null && !path.StartsWith(pathFilter))
                continue;

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
                continue;

            scanned++;

            Shader shader = mat.shader;
            string shaderName = shader != null ? shader.name : "(null)";

            // 1. Already URP-compatible?
            if (shader != null && IsURPCompatible(shaderName))
            {
                alreadyOk++;
                continue;
            }

            // 2. Check if the shader compiles (pink = error shader or null)
            bool isBroken = shader == null
                || shaderName == "Hidden/InternalErrorShader"
                || shaderName == "Hidden/Universal Render Pipeline/FallbackError"
                || ShaderUtil.ShaderHasError(shader);

            // 3. Do we have a mapping?
            if (ShaderMap.TryGetValue(shaderName, out string replacement))
            {
                Shader newShader = Shader.Find(replacement);
                if (newShader == null)
                {
                    report.AppendLine($"  [WARN] {path}");
                    report.AppendLine($"         Shader \"{shaderName}\" → \"{replacement}\" NOT FOUND in project.");
                    report.AppendLine();
                    continue;
                }

                report.AppendLine($"  [FIX]  {path}");
                report.AppendLine($"         \"{shaderName}\" → \"{replacement}\"");
                report.AppendLine();

                if (!dryRun)
                {
                    Undo.RecordObject(mat, "Fix Pink Material");
                    mat.shader = newShader;
                    EditorUtility.SetDirty(mat);
                }

                fixed_++;
            }
            else if (isBroken)
            {
                // Broken + no mapping → fall back to URP/Lit
                string fallback = "Universal Render Pipeline/Lit";
                Shader fallbackShader = Shader.Find(fallback);

                report.AppendLine($"  [BROKEN] {path}");
                report.AppendLine($"           Shader \"{shaderName}\" is missing/broken → falling back to \"{fallback}\"");
                report.AppendLine();

                if (!dryRun && fallbackShader != null)
                {
                    Undo.RecordObject(mat, "Fix Pink Material");
                    mat.shader = fallbackShader;
                    EditorUtility.SetDirty(mat);
                }

                fixed_++;
            }
            else
            {
                // Unknown shader that isn't explicitly broken — likely a custom/third-party shader
                report.AppendLine($"  [SKIP] {path}");
                report.AppendLine($"         Shader \"{shaderName}\" is not in the mapping table. Verify manually.");
                report.AppendLine();
                customSkipped++;
            }
        }

        if (!dryRun)
            AssetDatabase.SaveAssets();

        report.AppendLine("───────────────────────────────────────────────────");
        report.AppendLine($"  Scanned:     {scanned} materials");
        report.AppendLine($"  Already OK:  {alreadyOk}");
        report.AppendLine($"  Fixed:       {fixed_}");
        report.AppendLine($"  Skipped:     {customSkipped} (custom/unknown shader — check manually)");
        report.AppendLine("═══════════════════════════════════════════════════");

        if (fixed_ > 0 && !dryRun)
            report.AppendLine("  All changes are undoable (Edit → Undo).");

        Debug.Log(report.ToString());

        EditorUtility.DisplayDialog(
            dryRun ? "Scan Complete" : "Fix Complete",
            $"Scanned {scanned} materials.\n" +
            $"{fixed_} {(dryRun ? "would be fixed" : "fixed")}.\n" +
            $"{customSkipped} skipped (custom shader).\n\n" +
            "See Console for full report.",
            "OK"
        );
    }

    private static bool IsURPCompatible(string shaderName)
    {
        if (URPSafe.Contains(shaderName))
            return true;

        // Any shader under the URP namespace is fine
        if (shaderName.StartsWith("Universal Render Pipeline/"))
            return true;

        // Custom project shaders (Assets/Shader/) — trust them
        if (shaderName.StartsWith("Custom/"))
            return true;

        // Shader Graph outputs
        if (shaderName.StartsWith("Shader Graphs/"))
            return true;

        return false;
    }
}
