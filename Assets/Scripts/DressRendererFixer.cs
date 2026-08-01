using UnityEngine;

/// <summary>
/// Fixes dress SkinnedMeshRenderers and Cloth components that become invisible on Android builds:
/// 1. Enables updateWhenOffscreen on all SkinnedMeshRenderers so they render even when AABB is small/zero.
/// 2. Expands localBounds to prevent camera frustum culling on Android.
/// 3. Resets Cloth component motion buffers and toggles enabled state to force mobile PhysX mesh re-binding.
/// 4. Destroys redundant MeshRenderer & MeshFilter components that conflict with SkinnedMeshRenderer on Android.
/// 5. Validates materials/shaders for Android build compatibility.
/// </summary>
public static class DressRendererFixer
{
    /// <summary>
    /// Fixes and initializes all dress GameObjects in the dresses array.
    /// </summary>
    public static void FixDressRenderers(GameObject[] dresses)
    {
        if (dresses == null || dresses.Length == 0)
        {
            Debug.LogWarning("[DressRendererFixer] No dresses to fix.");
            return;
        }

        for (int i = 0; i < dresses.Length; i++)
        {
            if (dresses[i] != null)
            {
                FixAndResetDress(dresses[i]);
            }
        }

        Debug.Log($"[DressRendererFixer] Finished processing {dresses.Length} dress(es).");
    }

    /// <summary>
    /// Prepares, resets, and fixes a single dress GameObject whenever it is activated/equipped.
    /// </summary>
    public static void FixAndResetDress(GameObject dress)
    {
        if (dress == null) return;

        // 1. Process all SkinnedMeshRenderers in hierarchy
        SkinnedMeshRenderer[] smrs = dress.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if (smrs != null && smrs.Length > 0)
        {
            foreach (SkinnedMeshRenderer smr in smrs)
            {
                if (smr == null) continue;

                smr.updateWhenOffscreen = true;
                smr.enabled = true;

                // Ensure local bounds are non-zero to prevent frustum culling on Android
                if (smr.localBounds.size.sqrMagnitude < 0.01f)
                {
                    smr.localBounds = new Bounds(Vector3.zero, Vector3.one * 10f);
                }

                // Check materials & fallback shader if needed
                Material[] mats = smr.sharedMaterials;
                if (mats != null)
                {
                    for (int m = 0; m < mats.Length; m++)
                    {
                        if (mats[m] != null && (mats[m].shader == null || mats[m].shader.name.Contains("InternalErrorShader")))
                        {
                            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
                            if (urpLit == null) urpLit = Shader.Find("Standard");
                            if (urpLit != null)
                            {
                                mats[m].shader = urpLit;
                                Debug.LogWarning($"[DressRendererFixer] Applied fallback shader '{urpLit.name}' on material '{mats[m].name}' for dress '{dress.name}'");
                            }
                        }
                    }
                }
            }
        }

        // 2. Remove redundant MeshRenderer & MeshFilter on the same GameObjects as SkinnedMeshRenderer
        MeshRenderer[] mrs = dress.GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer mr in mrs)
        {
            if (mr != null)
            {
                Object.Destroy(mr);
                Debug.Log($"[DressRendererFixer] '{dress.name}': Removed redundant MeshRenderer.");
            }
        }

        MeshFilter[] mfs = dress.GetComponentsInChildren<MeshFilter>(true);
        foreach (MeshFilter mf in mfs)
        {
            if (mf != null)
            {
                Object.Destroy(mf);
                Debug.Log($"[DressRendererFixer] '{dress.name}': Removed redundant MeshFilter.");
            }
        }

        // 3. Reset Cloth components for Android PhysX simulation
        Cloth[] cloths = dress.GetComponentsInChildren<Cloth>(true);
        if (cloths != null && cloths.Length > 0)
        {
            foreach (Cloth cloth in cloths)
            {
                if (cloth == null) continue;

                // Toggle enabled state and clear transform motion buffer to force Android vertex re-simulation
                cloth.enabled = false;
                cloth.enabled = true;
                cloth.ClearTransformMotion();
                Debug.Log($"[DressRendererFixer] '{dress.name}': Reset Cloth simulation motion buffer.");
            }
        }
    }
}
