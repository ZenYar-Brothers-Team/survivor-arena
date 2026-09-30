using System;
using Game.Content.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace Game.Settings
{
    public static class SettingsCodec
    {
        private static readonly JsonSerializerSettings Json = new JsonSerializerSettings
        { MissingMemberHandling=MissingMemberHandling.Error, ContractResolver=new DefaultContractResolver { NamingStrategy=new CamelCaseNamingStrategy() } };
        public static SettingsSnapshot Decode(string text)
        {
            var d=JsonConvert.DeserializeObject<SettingsData>(text,Json);
            if(d==null || (d.SchemaVersion!=1 && d.SchemaVersion!=2 && d.SchemaVersion!=3) || !d.Master.HasValue || !d.Music.HasValue || !d.Sfx.HasValue || !d.Shake.HasValue || !d.Width.HasValue || !d.Height.HasValue || !d.Borderless.HasValue || (d.SchemaVersion>=2 && !d.MouseMovement.HasValue) || (d.SchemaVersion==3 && !d.UiScale.HasValue))
                throw new ArgumentException("Invalid or unsupported settings document.");
            return new SettingsSnapshot(d.Master.Value,d.Music.Value,d.Sfx.Value,d.Shake.Value,new VideoMode(d.Width.Value,d.Height.Value,d.Borderless.Value),d.MouseMovement??false,d.UiScale??0f);
        }
        public static string Encode(SettingsSnapshot s) => JsonConvert.SerializeObject(new SettingsData
        { SchemaVersion=3,Master=s.Master,Music=s.Music,Sfx=s.Sfx,Shake=s.Shake,MouseMovement=s.MouseMovement,Width=s.Video.Width,Height=s.Video.Height,Borderless=s.Video.Borderless,UiScale=s.UiScale },Json);
    }
}
