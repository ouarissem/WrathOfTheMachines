using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace WoTM.Core.Configuration;

[BackgroundColor(20, 25, 35, 230)]
public class WoTMConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("$Mods.WoTM.Config.AudioSettings")]

    [BackgroundColor(30, 38, 55, 245)]
    [DefaultValue(true)]
    [Label("Enable Draedon Voice Acting")]
    [Tooltip("If enabled, Draedon will speak with voice acting during dialogues. If disabled, only text will appear.")]
    public bool EnableVoiceActing;

    public WoTMConfig()
    {
        EnableVoiceActing = true;
    }
}
