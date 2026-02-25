using System;
using System.Collections.Generic;
using UnityEngine;

public class ModelColorCustomizer : MonoBehaviour
{
    [Serializable]
    public class PartTarget
    {
        [Header("ID")]
        public string partId = "Body";

        [Header("Targets")]
        public List<Renderer> renderers = new List<Renderer>();

        public List<int> materialIndices = new List<int>();

        public string colorProperty = "";

        [Header("Optional Presets (Swap Materials)")]
        public List<MaterialPreset> materialPresets = new List<MaterialPreset>();
    }

    [Serializable]
    public class MaterialPreset
    {
        public string presetId = "Silver";

        public List<Material> materials = new List<Material>();
    }

    public enum Theme3
    {
        Silver,
        Orange,
        Dark
    }

    [Header("Parts")]
    public List<PartTarget> parts = new List<PartTarget>();

    [Header("Theme 3 (Preset IDs must match Inspector)")]
    public Theme3 startTheme = Theme3.Silver;

    [SerializeField] private Theme3 currentTheme = Theme3.Silver;

    private readonly Dictionary<Renderer, MaterialPropertyBlock> _mpbCache = new Dictionary<Renderer, MaterialPropertyBlock>();
    private static readonly string[] AUTO_COLOR_PROPS = { "_BaseColor", "_Color", "_TintColor" };

    private void Awake()
    {
        currentTheme = startTheme;
    }

    public void SetPartColor(string partId, Color color)
    {
        var part = FindPart(partId);
        if (part == null) return;

        foreach (var r in part.renderers)
        {
            if (r == null) continue;

            int slotCount = r.sharedMaterials != null ? r.sharedMaterials.Length : 0;
            if (slotCount <= 0) continue;

            string prop = string.IsNullOrEmpty(part.colorProperty)
                ? DetectColorProperty(r)
                : part.colorProperty;

            if (string.IsNullOrEmpty(prop)) continue;

            if (part.materialIndices == null || part.materialIndices.Count == 0)
            {
                for (int i = 0; i < slotCount; i++)
                    ApplyColorToRendererSlot(r, i, prop, color);
            }
            else
            {
                foreach (int i in part.materialIndices)
                {
                    if (i < 0 || i >= slotCount) continue;
                    ApplyColorToRendererSlot(r, i, prop, color);
                }
            }
        }
    }

    public void SetMultiplePartColors(Dictionary<string, Color> partColors)
    {
        foreach (var kv in partColors)
            SetPartColor(kv.Key, kv.Value);
    }

    public void ClearPartColorOverride(string partId)
    {
        var part = FindPart(partId);
        if (part == null) return;

        foreach (var r in part.renderers)
        {
            if (r == null) continue;

            int slotCount = r.sharedMaterials != null ? r.sharedMaterials.Length : 0;
            if (slotCount <= 0) continue;

            if (part.materialIndices == null || part.materialIndices.Count == 0)
            {
                for (int i = 0; i < slotCount; i++)
                    r.SetPropertyBlock(null, i);
            }
            else
            {
                foreach (int i in part.materialIndices)
                {
                    if (i < 0 || i >= slotCount) continue;
                    r.SetPropertyBlock(null, i);
                }
            }
        }
    }

    public void ApplyMaterialPreset(string partId, string presetId)
    {
        var part = FindPart(partId);
        if (part == null) return;

        var preset = part.materialPresets.Find(p => p.presetId == presetId);
        if (preset == null) return;

        foreach (var r in part.renderers)
        {
            if (r == null) continue;

            var mats = r.materials; 
            if (mats == null || mats.Length == 0) continue;

            if (preset.materials == null || preset.materials.Count == 0) continue;

            if (preset.materials.Count == 1)
            {
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = preset.materials[0];
            }
            else
            {
                for (int i = 0; i < mats.Length && i < preset.materials.Count; i++)
                {
                    if (preset.materials[i] != null)
                        mats[i] = preset.materials[i];
                }
            }

            r.materials = mats;

            ClearPropertyBlockAllSlots(r);
        }
    }

    public void CycleTheme()
    {
        switch (currentTheme)
        {
            case Theme3.Silver: ApplyTheme(Theme3.Orange); break;
            case Theme3.Orange: ApplyTheme(Theme3.Dark); break;
            default: ApplyTheme(Theme3.Silver); break;
        }
    }

    public void ApplyThemeSilver() => ApplyTheme(Theme3.Silver);
    public void ApplyThemeOrange() => ApplyTheme(Theme3.Orange);
    public void ApplyThemeDark() => ApplyTheme(Theme3.Dark);

    public void ApplyTheme(Theme3 theme)
    {
        currentTheme = theme;
        string presetId = theme.ToString(); 

        for (int i = 0; i < parts.Count; i++)
        {
            var part = parts[i];
            if (part == null) continue;

            ApplyMaterialPreset(part.partId, presetId);
        }
    }

    public void ApplyThemeByPresetId(string presetId)
    {
        if (string.IsNullOrEmpty(presetId)) return;

        for (int i = 0; i < parts.Count; i++)
        {
            var part = parts[i];
            if (part == null) continue;

            ApplyMaterialPreset(part.partId, presetId);
        }
    }

    public void ApplyTintToAllParts(Color color)
    {
        for (int i = 0; i < parts.Count; i++)
        {
            var part = parts[i];
            if (part == null) continue;
            SetPartColor(part.partId, color);
        }
    }

    private PartTarget FindPart(string partId)
    {
        return parts.Find(p => p.partId == partId);
    }

    private MaterialPropertyBlock GetBlock(Renderer r)
    {
        if (!_mpbCache.TryGetValue(r, out var block) || block == null)
        {
            block = new MaterialPropertyBlock();
            _mpbCache[r] = block;
        }
        return block;
    }

    private void ApplyColorToRendererSlot(Renderer r, int slotIndex, string prop, Color color)
    {
        var block = GetBlock(r);
        r.GetPropertyBlock(block, slotIndex);
        block.SetColor(prop, color);
        r.SetPropertyBlock(block, slotIndex);
    }

    private void ClearPropertyBlockAllSlots(Renderer r)
    {
        int slotCount = r.sharedMaterials != null ? r.sharedMaterials.Length : 0;
        for (int i = 0; i < slotCount; i++)
            r.SetPropertyBlock(null, i);
    }

    private string DetectColorProperty(Renderer r)
    {
        var mats = r.sharedMaterials;
        if (mats == null || mats.Length == 0) return "";

        for (int mi = 0; mi < mats.Length; mi++)
        {
            var m = mats[mi];
            if (m == null) continue;

            foreach (var p in AUTO_COLOR_PROPS)
            {
                if (m.HasProperty(p))
                    return p;
            }
        }
        return "";
    }
}