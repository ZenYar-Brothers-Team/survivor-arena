using Game.Presentation;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEngine;

namespace Game.MLAgents
{
    /// <summary>
    /// Builds the MicroArena hierarchy (player/threat/XP visuals + the agent's ML-Agents components) entirely in
    /// code, so the scene file itself only needs a camera and one GameObject with this component. Place in an
    /// otherwise empty scene (see Assets/Scenes/MLMicroArena.unity) and press Play.
    /// </summary>
    public sealed class MicroArenaBootstrap : MonoBehaviour
    {
        private const string BehaviorName = "MicroArena";

        private void Awake()
        {
            var threat = CreateDisc("Threat", new Color(1f, 0.3f, 0.3f, 1f), MicroArenaConfig.Default(0).ThreatRadius);
            var xp = CreateDisc("Xp", new Color(1f, 0.85f, 0.2f, 1f), MicroArenaConfig.Default(0).XpRadius);
            var player = CreateDisc("Player", new Color(0.3f, 0.85f, 1f, 1f), MicroArenaConfig.Default(0).PlayerRadius);

            var behaviorParameters = player.AddComponent<BehaviorParameters>();
            behaviorParameters.BehaviorName = BehaviorName;
            behaviorParameters.BrainParameters.VectorObservationSize = MicroArenaObservationEncoder.ObservationCount;
            behaviorParameters.BrainParameters.NumStackedVectorObservations = 1;
            behaviorParameters.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(2);
            behaviorParameters.BehaviorType = BehaviorType.Default;

            var agent = player.AddComponent<MicroArenaAgent>();
            agent.Configure(threat.transform, xp.transform);
        }

        private static GameObject CreateDisc(string name, Color color, float radius)
        {
            var go = new GameObject(name);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = ProceduralShapeSprites.Disc;
            renderer.color = color;
            go.transform.localScale = Vector3.one * (radius * 2f);
            return go;
        }
    }
}
