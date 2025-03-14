using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Enum for mesh flip direction.
    /// </summary>
    public enum MeshFlipperMeshDirection
    {
        /// <summary>
        /// Create a flipped mesh.
        /// </summary>
        Flip,

        /// <summary>
        /// Create a double-sided mesh.
        /// </summary>
        BothSides,
    }

    /// <summary>
    /// Enum for mask mode to flip the mesh.
    /// </summary>
    public enum FlipFlipperMaskMode
    {
        /// <summary>
        /// Flip triangles when their all vertices are on white pixels.
        /// </summary>
        FlipWhite,

        /// <summary>
        /// Flip triangles when their all vertices are on black pixels.
        /// </summary>
        FlipBlack,
    }

    /// <summary>
    /// Component to flip the mesh of its game object's renderer.
    /// </summary>
    [AddComponentMenu("VRCQuestTools/VQT Mesh Flipper")]
    [HelpURL("https://kurotu.github.io/VRCQuestTools/docs/references/components/mesh-flipper?lang=auto")]
    [DisallowMultipleComponent]
    public class MeshFlipper : VRCQuestToolsEditorOnly, INdmfComponent, IPlatformDependentComponent, IExperimentalComponent
    {
        /// <summary>
        /// Direction to flip the mesh.
        /// </summary>
        public MeshFlipperMeshDirection direction = MeshFlipperMeshDirection.BothSides;

        /// <summary>
        /// Enable on PC target.
        /// </summary>
        public bool enabledOnPC = false;

        /// <summary>
        /// Enable on Android target.
        /// </summary>
        public bool enabledOnAndroid = true;

        /// <summary>
        /// Use mask to flip the mesh.
        /// </summary>
        public bool useMask = false;

        /// <summary>
        /// Mask texture to flip the mesh.
        /// </summary>
        public Texture2D maskTexture = null;

        /// <summary>
        /// Mask texture mode to flip the mesh.
        /// </summary>
        public FlipFlipperMaskMode maskMode = FlipFlipperMaskMode.FlipWhite;

        /// <summary>
        /// Get shared mesh of the component.
        /// </summary>
        /// <returns>Shared mesh object.</returns>
        public Mesh GetSharedMesh()
        {
            var smr = GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
            {
                return smr.sharedMesh;
            }
            var mf = GetComponent<MeshFilter>();
            if (mf != null)
            {
                return mf.sharedMesh;
            }
            return null;
        }

        /// <summary>
        /// Set shared mesh to the component.
        /// </summary>
        /// <param name="mesh">Mesh to set.</param>
        public void SetSharedMesh(Mesh mesh)
        {
            var smr = GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
            {
                smr.sharedMesh = mesh;
            }
            var mf = GetComponent<MeshFilter>();
            if (mf != null)
            {
                mf.sharedMesh = mesh;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Create a new mesh based on the direction.
        /// </summary>
        /// <returns>New mesh.</returns>
        public Mesh CreateMesh()
        {
            var sharedMesh = GetSharedMesh();
            switch (direction)
            {
                case MeshFlipperMeshDirection.Flip:
                    return CreateFlippedMesh(sharedMesh, useMask, maskTexture, maskMode);
                case MeshFlipperMeshDirection.BothSides:
                    return CreateBothSidesMesh(sharedMesh, useMask, maskTexture, maskMode);
                default:
                    throw new System.InvalidProgramException("Mask texture must be readable.");
            }
        }

        private static Mesh CreateFlippedMesh(Mesh mesh, bool useMask, Texture2D mask, FlipFlipperMaskMode maskMode)
        {
            Mesh newMesh = Instantiate(mesh);
            newMesh.name = mesh.name + "_flipped";
            for (int i = 0; i < newMesh.subMeshCount; i++)
            {
                if (useMask)
                {
                    if (mask == null)
                    {
                        throw new MeshFlipperMaskMissingException("Mask texture is missing.");
                    }
                    if (!mask.isReadable)
                    {
                        throw new MeshFlipperMaskNotReadableException("Mask texture must be readable.", mask);
                    }
                    var triangles = newMesh.GetTriangles(i);
                    for (int j = 0; j < triangles.Length; j += 3)
                    {
                        var uvs = new Vector2[] {
                            mesh.uv[triangles[j]],
                            mesh.uv[triangles[j + 1]],
                            mesh.uv[triangles[j + 2]],
                        };
                        var colors = uvs.Select(uv => mask.GetPixelBilinear(uv.x, uv.y));
                        var shouldFlip = colors.All(color => maskMode == FlipFlipperMaskMode.FlipWhite ? color.r > 0.5f : color.r < 0.5f);
                        if (shouldFlip)
                        {
                            var tmp = triangles[j];
                            triangles[j] = triangles[j + 2];
                            triangles[j + 2] = tmp;
                        }
                    }
                    newMesh.SetTriangles(triangles, i);
                }
                else
                {
                    newMesh.SetTriangles(newMesh.GetTriangles(i).Reverse().ToArray(), i);
                }
            }
            return newMesh;
        }

        private static Mesh CreateBothSidesMesh(Mesh mesh, bool useMask, Texture2D mask, FlipFlipperMaskMode maskMode)
        {
            Mesh newMesh = Instantiate(mesh);
            newMesh.name = mesh.name + "_bothSides";
            for (int i = 0; i < newMesh.subMeshCount; i++)
            {
                var triangles = newMesh.GetTriangles(i);
                if (useMask) {
                    if (mask == null)
                    {
                        throw new MeshFlipperMaskMissingException("Mask texture is missing.");
                    }
                    if (!mask.isReadable)
                    {
                        throw new MeshFlipperMaskNotReadableException("Mask texture must be readable.", mask);
                    }
                    var trianglesList = new List<int>();
                    for (int j = 0; j < triangles.Length; j += 3)
                    {
                        var uvs = new Vector2[]{
                            mesh.uv[triangles[j]],
                            mesh.uv[triangles[j + 1]],
                            mesh.uv[triangles[j + 2]],
                        };
                        var colors = uvs.Select(uv => mask.GetPixelBilinear(uv.x, uv.y));
                        var shouldFlip = colors.All(color => maskMode == FlipFlipperMaskMode.FlipWhite ? color.r > 0.5f : color.r < 0.5f);
                        if (shouldFlip)
                        {
                            trianglesList.Add(triangles[j + 2]);
                            trianglesList.Add(triangles[j + 1]);
                            trianglesList.Add(triangles[j]);
                        }
                    }
                    newMesh.SetTriangles(triangles.Concat(trianglesList).ToArray(), i);
                }
                else {
                    newMesh.SetTriangles(triangles.Concat(triangles.Reverse()).ToArray(), i);
                }
            }
            return newMesh;
        }
#endif
    }
}
