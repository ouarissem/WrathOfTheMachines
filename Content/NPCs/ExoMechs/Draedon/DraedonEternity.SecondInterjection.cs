using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using WoTM.Content.NPCs.ExoMechs.Draedon.Dialogue;
using WoTM.Content.NPCs.ExoMechs.FightManagers;
using WoTM.Core.BehaviorOverrides;

namespace WoTM.Content.NPCs.ExoMechs.Draedon;

public sealed partial class DraedonBehavior : NPCBehaviorOverride
{
    /// <summary>
    /// The monologue that Draedon uses upon his second Exo Mech being defeated, prior to the final Exo Mech doing battle with the player.
    /// The order of Add() calls determines the sequence of spoken lines.
    /// Each key must exist in DraedonDialogueManager.Dialogue and in the localization file.
    /// The corresponding audio file is automatically loaded from Assets/Sounds/Custom/VoiceActing/Drae_{key}.wav
    /// </summary>
    public static readonly DraedonDialogueChain SecondInterjection = new DraedonDialogueChain().
        Add("Interjection7").
        Add("Interjection8").
        Add("Interjection9").
        Add(() =>
        {
            
            if (NPC.AnyNPCs(ExoMechNPCIDs.ApolloID))
                return DraedonDialogueManager.Dialogue["Interjection10_Plural"];
            return DraedonDialogueManager.Dialogue["Interjection10"];
        }).
        Add("Interjection11"); // "Good luck."

    /// <summary>
    /// The AI method that makes Draedon speak to the player after an Exo Mech has been defeated.
    /// Processes the dialogue chain and automatically plays the corresponding audio/subtitles.
    /// </summary>
    public void DoBehavior_SecondInterjection()
    {
        int speakTimer = (int)AITimer - 150;
        var monologue = SecondInterjection;
        SecondInterjection.Process(speakTimer, out DraedonDialogue? dialogue, out int relativeTime);

        HologramOverlayInterpolant = 0f;

        Vector2 hoverDestination = PlayerToFollow.Center + new Vector2((PlayerToFollow.Center.X - NPC.Center.X).NonZeroSign() * -420f, -20f);
        NPC.SmoothFlyNear(hoverDestination, 0.05f, 0.94f);

        if (dialogue == DraedonDialogueManager.Dialogue["Interjection9"] && relativeTime == dialogue.Duration - 60)
            ResetPlayerFightVariables();

        if (dialogue == DraedonDialogueManager.Dialogue["Interjection11"] && relativeTime == dialogue.Duration - 60 && !DraedonDialogueManager.UseSubtitles)
        {
            ScreenShakeSystem.StartShake(6f);
            SoundEngine.PlaySound(CalamityMod.NPCs.ExoMechs.Draedon.LaughSound);
        }

        if (SecondInterjection.Finished(speakTimer))
        {
            AIState = DraedonAIState.MoveAroundDuringBattle;
            AITimer = 0f;
            NPC.netUpdate = true;
        }

        PerformStandardFraming();
    }
}