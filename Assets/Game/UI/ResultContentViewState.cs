using UnityEngine;

namespace Game.UI
{
    public sealed class ResultContentViewState
    {
        public string Id { get; }
        public string Name { get; }
        public string Kind { get; }
        public Sprite Icon { get; }
        public ResultContentViewState(string id, string name, string kind, Sprite icon = null)
        { Id = id; Name = name; Kind = kind; Icon = icon; }
    }
}
