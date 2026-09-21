namespace Game.Field.Json
{
    public sealed class FieldCatalogData
    {
        public string DefaultFieldId { get; set; }
        public string[] AvailableFieldIds { get; set; }
        public FieldData[] Fields { get; set; }
        public FieldEnvironmentData[] Environments { get; set; }
    }
}
