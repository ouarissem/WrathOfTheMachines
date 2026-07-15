using System;
using System.Linq;
using CalamityMod;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WoTM.Content.NPCs.ExoMechs.Draedon.Dialogue;
using WoTM.Content.NPCs.ExoMechs.FightManagers;
using WoTM.Content.NPCs.ExoMechs.Projectiles;
using WoTM.Core.BehaviorOverrides;

namespace WoTM.Content.NPCs.ExoMechs.Draedon;

public sealed partial class DraedonBehavior : NPCBehaviorOverride
{
    /// <summary>
    /// The monologue that Draedon uses upon the Exo Mechs battle concluding the first time you defeat them.
    /// The order of Add() calls determines the sequence of spoken lines.
    /// Each key must exist in DraedonDialogueManager.Dialogue and in the localization file.
    /// The corresponding audio file is automatically loaded from Assets/Sounds/Custom/VoiceActing/Drae_{key}.wav
    /// </summary>
    public static readonly DraedonDialogueChain PostBattleInterjection = new DraedonDialogueChain().
        Add("EndOfBattle_FirstDefeat1"). // "Extraordinary!"
        Add("EndOfBattle_FirstDefeat2"). // "This outcome has fallen far outside the expectations..."
        Add("EndOfBattle_FirstDefeat3"). // "I shall soon depart..."
        Add("EndOfBattle_FirstDefeat4"). // "Take this, as an extension of gratitude..."
        Add("EndOfBattle_FirstDefeat5"). // "If you wish to do combat with my machines again..."
        Add("EndOfBattle_FirstDefeat6"); // "Farewell."

    /// <summary>
    /// The monologue that Draedon uses upon the Exo Mechs battle concluding in successive defeats.
    /// This chain includes dynamic selection based on player performance.
    /// </summary>
    public static readonly DraedonDialogueChain PostBattleAnalysisInterjection = new DraedonDialogueChain().
        Add("EndOfBattle_SuccessiveDefeat1"). // "Another completed experiment."
        Add("EndOfBattle_SuccessiveDefeat2"). // "Allow me a moment to analyze the results..."
        Add(() => DraedonDialogueManager.Dialogue[EndOfBattle_SuccessiveDefeat3Selection()]). // Dynamic performance assessment.
        Add(() => DraedonDialogueManager.Dialogue[EndOfBattle_SuccessiveDefeat4Selection()]). // Dynamic improvement suggestion.
        Add("EndOfBattle_SuccessiveDefeat5"); // "I await the next battle."

    /// <summary>
    /// How many style points the player must have in order for Draedon to recognize a battle as perfect.
    /// </summary>
    public static float StylePoints_Perfection => 0.99f;

    /// <summary>
    /// How many style points the player must have in order for Draedon to recognize a battle as excellent.
    /// </summary>
    public static float StylePoints_Excellent => 0.93f;

    /// <summary>
    /// How many style points the player must have in order for Draedon to recognize a battle as good.
    /// </summary>
    public static float StylePoints_Good => 0.77f;

    /// <summary>
    /// How many style points the player must have in order for Draedon to recognize a battle as acceptable.
    /// </summary>
    public static float StylePoints_Acceptable => 0.56f;

    /// <summary>
    /// The AI method that makes Draedon speak to the player after an Exo Mech has been defeated.
    /// Processes the dialogue chain and automatically plays the corresponding audio/subtitles.
    /// </summary>
    public void DoBehavior_PostBattleInterjection()
    {
        int speakTimer = (int)AITimer - 90;
        DraedonDialogueChain dialogue = DownedBossSystem.downedExoMechs ? PostBattleAnalysisInterjection : PostBattleInterjection;
        dialogue.Process(speakTimer, out DraedonDialogue? currentLine, out int relativeTime);

        Vector2 hoverDestination = PlayerToFollow.Center + new Vector2((PlayerToFollow.Center.X - NPC.Center.X).NonZeroSign() * -450f, -5f);
        NPC.SmoothFlyNear(hoverDestination, 0.05f, 0.94f);
        NPC.dontTakeDamage = WasKilled;

        // Give the player their loot crate.
        bool lootCrateLine = currentLine == DraedonDialogueManager.Dialogue["EndOfBattle_FirstDefeat5"] || currentLine == DraedonDialogueManager.Dialogue["EndOfBattle_SuccessiveDefeat5"];
        if (Main.netMode != NetmodeID.MultiplayerClient && relativeTime == 1 && lootCrateLine)
        {
            foreach (Player player in Main.ActivePlayers)
            {
                if (player.WithinRange(NPC.Center, 6700f))
                    LumUtils.NewProjectileBetter(NPC.GetSource_FromAI(), player.Center - Vector2.UnitY * 800f, Vector2.Zero, ModContent.ProjectileType<DraedonLootCrate>(), 0, 0f, player.whoAmI);
            }
        }

        if (dialogue.Finished(speakTimer))
        {
            HologramOverlayInterpolant = LumUtils.Saturate(HologramOverlayInterpolant + 0.02f);
            MaxSkyOpacity = 1f - HologramOverlayInterpolant;
            if (HologramOverlayInterpolant >= 1f)
                NPC.active = false;
        }
        else
            HologramOverlayInterpolant = 0f;

        PerformStandardFraming();
    }

    /// <summary>
    /// Selects interjection text based on the player's performance for the third post-battle dialogue line.
    /// The selected key must exist in the localization file and have a corresponding audio file.
    /// </summary>
    public static string EndOfBattle_SuccessiveDefeat3Selection()
    {
        Player closest = Main.player[Player.FindClosest(new Vector2(Main.maxTilesX * 0.5f, (float)Main.worldSurface) * 16f, 1, 1)];

        if (!closest.TryGetModPlayer(out ExoMechStylePlayer stylePlayer))
            return "Error";

        float style = stylePlayer.Style;
        if (stylePlayer.PlayerIsMeltingBoss)
            return "EndOfBattle_SuccessiveDefeat3_WhyDidYouMeltTheBoss";
        if (style >= StylePoints_Perfection)
            return "EndOfBattle_SuccessiveDefeat3_Perfect";
        if (style >= StylePoints_Excellent)
            return "EndOfBattle_SuccessiveDefeat3_Excellent";
        if (style >= StylePoints_Good)
            return "EndOfBattle_SuccessiveDefeat3_Good";
        if (style >= StylePoints_Acceptable)
            return "EndOfBattle_SuccessiveDefeat3_Acceptable";

        return "EndOfBattle_SuccessiveDefeat3_Bad";
    }

    /// <summary>
    /// Selects interjection text based on the player's performance for the fourth post-battle dialogue line.
    /// The selected key must exist in the localization file and have a corresponding audio file.
    /// </summary>
    public static string EndOfBattle_SuccessiveDefeat4Selection()
    {
        Player closest = Main.player[Player.FindClosest(new Vector2(Main.maxTilesX * 0.5f, (float)Main.worldSurface) * 16f, 1, 1)];

        if (!closest.TryGetModPlayer(out ExoMechStylePlayer stylePlayer))
            return "Error";

        float style = stylePlayer.Style;
        if (stylePlayer.PlayerIsMeltingBoss)
            return "EndOfBattle_SuccessiveDefeat4_WhyDidYouMeltTheBoss";
        if (style >= StylePoints_Perfection)
            return "EndOfBattle_SuccessiveDefeat4_Perfect";
        if (style >= StylePoints_Excellent)
            return "EndOfBattle_SuccessiveDefeat4_Excellent";
        if (style >= StylePoints_Acceptable)
        {
            float[] weights = [stylePlayer.HitsWeight, stylePlayer.BuffsWeight, stylePlayer.FightTimeWeight, stylePlayer.AggressivenessWeight];
            float weakestWeight = weights.Min();
            int weakestWeightIndex = Array.IndexOf(weights, weakestWeight);

            return weakestWeightIndex switch
            {
                0 => "EndOfBattle_SuccessiveDefeat4_ImproveHitCounter",
                1 => "EndOfBattle_SuccessiveDefeat4_ImproveBuffs",
                2 => "EndOfBattle_SuccessiveDefeat4_ImproveFightTime",
                3 => "EndOfBattle_SuccessiveDefeat4_ImproveAggression",
                _ => "Error",
            };
        }

        return "EndOfBattle_SuccessiveDefeat4_Bad";
    }
}