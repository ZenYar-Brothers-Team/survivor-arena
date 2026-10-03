namespace Game.Traps.Json
{
    /// <summary>
    /// A 3D model a turret may be drawn with in the 2D scene: the model lies on its XZ ground, is tilted towards the camera and
    /// pushed forward in depth so it covers the sprites behind it.
    /// </summary>
    public sealed class TrapModelData
    {
        public string Key { get; set; }
        /// <summary>Name of the child transform that turns about its local Y axis with the turret head.</summary>
        public string HeadNode { get; set; }
        /// <summary>Elevation of the viewing camera over the model ground, degrees.</summary>
        public float? TiltDegrees { get; set; }
        public float? Scale { get; set; }
        /// <summary>How far towards the camera the model is moved so it is never hidden by ground sprites.</summary>
        public float? DepthOffset { get; set; }
    }
}
