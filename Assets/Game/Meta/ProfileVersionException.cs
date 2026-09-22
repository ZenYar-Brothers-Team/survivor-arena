using System;
namespace Game.Meta
{
    public sealed class ProfileVersionException : Exception
    {
        public ProfileVersionException(string message) : base(message) { }
    }
}
