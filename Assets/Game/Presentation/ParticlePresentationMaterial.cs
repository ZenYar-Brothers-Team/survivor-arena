using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Gives code-created particle systems a real material. <c>AddComponent&lt;ParticleSystem&gt;()</c> leaves the
    /// renderer without one, which renders as magenta squares (playtest 2026-09-24_e1e04fc4 OBS-01/02).
    /// Particles reuse the sprite material of their owner (same pipeline shader and 2D lighting as the
    /// sprites) with a soft round texture, so the configured particle colors show as intended.
    /// </summary>
    public static class ParticlePresentationMaterial
    {
        private static Material _shared;

        /// <summary>The shared particle material, created from <paramref name="spriteMaterialSource"/>'s shader.</summary>
        public static Material For(Renderer spriteMaterialSource)
        {
            var source = spriteMaterialSource != null ? spriteMaterialSource.sharedMaterial : null;
            if (_shared != null && (source == null || _shared.shader == source.shader)) return _shared;
            var material = source != null ? new Material(source) : new Material(Shader.Find("Sprites/Default"));
            material.name = "Procedural particle";
            material.mainTexture = ProceduralShapeSprites.Disc.texture;
            material.hideFlags = HideFlags.HideAndDontSave;
            return _shared = material;
        }

        /// <summary>Assigns the material and draws the particles just above the owner's sprite.</summary>
        public static void Apply(ParticleSystem particles, Renderer spriteMaterialSource)
        {
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = For(spriteMaterialSource);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            if (spriteMaterialSource == null) return;
            renderer.sortingLayerID = spriteMaterialSource.sortingLayerID;
            renderer.sortingOrder = spriteMaterialSource.sortingOrder + 1;
        }
    }
}
