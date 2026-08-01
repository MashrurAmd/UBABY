using UnityEngine;

/// <summary>
/// Fixes dress SkinnedMeshRenderers that have empty bone arrays and
/// UpdateWhenOffscreen disabled — which causes them to be invisible on
/// Android builds while still appearing in the Unity Editor.
///
/// Call FixDressRenderers() from WardrobeManager.Start().
/// </summary>
public static class DressRendererFixer
{
    /// <summary>
    /// Iterates over every dress GameObject and:
    ///   1. Enables UpdateWhenOffscreen on the SkinnedMeshRenderer so it
    ///      renders even when its (broken) AABB is outside the camera frustum.
    ///   2. Destroys the redundant MeshRenderer + MeshFilter that conflict
    ///      with the SkinnedMeshRenderer on Android.
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
            GameObject dress = dresses[i];
            if (dress == null)
            {
                Debug.LogWarning($"[DressRendererFixer] Dress [{i}] is null, skipping.");
                continue;
            }

            // --- Fix 1: SkinnedMeshRenderer.updateWhenOffscreen ---
            SkinnedMeshRenderer smr = dress.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (smr != null)
            {
                smr.updateWhenOffscreen = true;
                Debug.Log($"[DressRendererFixer] Dress [{i}] '{dress.name}': " +
                          $"Set updateWhenOffscreen = true on SkinnedMeshRenderer.");
            }
            else
            {
                Debug.LogWarning($"[DressRendererFixer] Dress [{i}] '{dress.name}': " +
                                 $"No SkinnedMeshRenderer found!");
            }

            // --- Fix 2: Remove redundant MeshRenderer + MeshFilter ---
            // The dress GameObjects have BOTH a SkinnedMeshRenderer (for Cloth)
            // and a MeshRenderer + MeshFilter using the FBX-embedded material.
            // The MeshRenderer conflicts with the SkinnedMeshRenderer on Android.
            MeshRenderer mr = dress.GetComponentInChildren<MeshRenderer>(true);
            if (mr != null)
            {
                Object.Destroy(mr);
                Debug.Log($"[DressRendererFixer] Dress [{i}] '{dress.name}': " +
                          $"Removed redundant MeshRenderer.");
            }

            MeshFilter mf = dress.GetComponentInChildren<MeshFilter>(true);
            if (mf != null)
            {
                Object.Destroy(mf);
                Debug.Log($"[DressRendererFixer] Dress [{i}] '{dress.name}': " +
                          $"Removed redundant MeshFilter.");
            }
        }

        Debug.Log($"[DressRendererFixer] Finished fixing {dresses.Length} dress(es).");
    }
}
