using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace XIVThirdEye.Core
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        [NonSerialized]
        public IDalamudPluginInterface PluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
        }

        public void Save()
        {
            PluginInterface?.SavePluginConfig(this);
        }
    }
}
