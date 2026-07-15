using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace WoTM.Core.Configuration;

public class WoTMConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(true)]
    [Label("Enable Draedon Voice Acting")]
    [Tooltip("If enabled, Draedon will speak with voice acting during dialogues. If disabled, only text will appear.")]
    public bool EnableVoiceActing;

    public WoTMConfig()
    {
        EnableVoiceActing = true;
    }
}