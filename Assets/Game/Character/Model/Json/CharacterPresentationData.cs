namespace Game.Character.Json
{
    public sealed class CharacterPresentationData
    {
        public string Role { get; set; }
        public string BaselineId { get; set; }
        public string CropId { get; set; }
        public string IconId { get; set; }
        public string[] Highlights { get; set; }
    }
}
