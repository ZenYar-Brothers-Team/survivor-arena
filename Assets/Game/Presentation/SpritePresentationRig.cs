using System;
using UnityEngine;

namespace Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class SpritePresentationRig : MonoBehaviour
    {
        [SerializeField]
        private Transform bodyRoot;

        [SerializeField]
        private SpriteRenderer bodyRenderer;

        [SerializeField]
        private SpriteRenderer shadowRenderer;

        public Transform BodyRoot => bodyRoot;
        public SpriteRenderer BodyRenderer => bodyRenderer;
        public SpriteRenderer ShadowRenderer => shadowRenderer;

        public void Configure(
            Transform animatedBodyRoot,
            SpriteRenderer animatedBodyRenderer,
            SpriteRenderer groundShadowRenderer = null)
        {
            bodyRoot = animatedBodyRoot;
            bodyRenderer = animatedBodyRenderer;
            shadowRenderer = groundShadowRenderer;
            Validate();
        }

        public void Validate()
        {
            if (bodyRoot == null)
                throw new InvalidOperationException("Sprite presentation rig requires a body root.");
            if (bodyRenderer == null)
                throw new InvalidOperationException("Sprite presentation rig requires a body renderer.");
            if (bodyRoot == transform || !bodyRoot.IsChildOf(transform))
                throw new InvalidOperationException("Body root must be a child of the visual root.");
            if (!bodyRenderer.transform.IsChildOf(bodyRoot) && bodyRenderer.transform != bodyRoot)
                throw new InvalidOperationException("Body renderer must belong to the body root subtree.");
            if (shadowRenderer == null)
                return;
            if (!shadowRenderer.transform.IsChildOf(transform))
                throw new InvalidOperationException("Shadow renderer must belong to the visual root subtree.");
            if (shadowRenderer.transform.IsChildOf(bodyRoot) || shadowRenderer.transform == bodyRoot)
                throw new InvalidOperationException("Shadow renderer cannot inherit the animated body pose.");
        }
    }
}
