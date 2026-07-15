using Luminance.Common.Utilities;
using Terraria;
using WoTM.Content.NPCs.ExoMechs.Draedon.Dialogue;
using WoTM.Core.BehaviorOverrides;

namespace WoTM.Content.NPCs.ExoMechs.Draedon;

public sealed partial class DraedonBehavior : NPCBehaviorOverride
{
    /// <summary>
    /// The monologue that Draedon uses if the player dies during the battle.
    /// The order of Add() calls determines the sequence of spoken lines.
    /// Each key must exist in DraedonDialogueManager.Dialogue and in the localization file.
    /// The corresponding audio file is automatically loaded from Assets/Sounds/Custom/VoiceActing/Drae_{key}.wav
    /// </summary>
    public static readonly DraedonDialogueChain StandardPlayerDeathMonologue = new DraedonDialogueChain().
        Add("Death");

    /// <summary>
    /// The monologue that Draedon uses if the player dies in the middle of him speaking.
    /// </summary>
    public static readonly DraedonDialogueChain FunnyPlayerDeathMonologue = new DraedonDialogueChain().
        Add("PlayerDeathAtAmusingTime");

    /// <summary>
    /// The AI method that makes Draedon speak after the player dies.
    /// Processes the dialogue chain and automatically plays the corresponding audio/subtitles.
    /// </summary>
    public void DoBehavior_PlayerDeathMonologue()
    {
        int speakTimer = (int)AITimer;
        var dialogue = AIState == DraedonAIState.FunnyPlayerDeathMonologue ? FunnyPlayerDeathMonologue : StandardPlayerDeathMonologue;
        dialogue.Process(speakTimer);

        NPC.velocity *= 0.9f;

        if (dialogue.Finished(speakTimer))
        {
            HologramOverlayInterpolant = LumUtils.Saturate(HologramOverlayInterpolant + 0.04f);
            MaxSkyOpacity = 1f - HologramOverlayInterpolant;
            if (HologramOverlayInterpolant >= 1f)
                NPC.active = false;
        }
        else
            HologramOverlayInterpolant = 0f;

        PerformStandardFraming();
    }
}