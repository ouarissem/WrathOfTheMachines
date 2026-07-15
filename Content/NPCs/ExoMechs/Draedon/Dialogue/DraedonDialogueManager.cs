using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using WoTM.Core.Configuration;

namespace WoTM.Content.NPCs.ExoMechs.Draedon.Dialogue;

public class DraedonDialogueManager : ModSystem
{
    /// <summary>
    /// The dictionary that stores all dialogue entries, keyed by their unique string identifier.
    /// </summary>
    public static readonly Dictionary<string, DraedonDialogue> Dialogue = [];

    /// <summary>
    /// Determines whether subtitles (audio + text) should be used instead of plain chat messages.
    /// When true, the audio files are played with on-screen subtitles.
    /// </summary>
    public static bool UseSubtitles => WoTM.Config.EnableVoiceActing;

    /// <summary>
    /// The target volume for the game music when Draedon is speaking.
    /// This creates the "ducking" effect where music lowers during dialogue.
    /// </summary>
    public static float MusicVolumeWhileSpeaking => 0.2f;

    /// <summary>
    /// How quickly the music volume transitions (0-1). Higher = faster transition.
    /// </summary>
    public static float MusicVolumeTransitionSpeed => 0.05f;

    /// <summary>
    /// Tracks whether Draedon is currently speaking.
    /// Used to control music ducking.
    /// </summary>
    public static bool IsSpeaking { get; internal set; }

    private static float _originalMusicVolume = 1f;
    private static bool _isDucking = false;

    public override void PostSetupContent()
    {
        Color edgyTextColor = CalamityMod.NPCs.ExoMechs.Draedon.TextColorEdgy;
		
        GenerateNew("IntroductionMonologue1");
        GenerateNew("IntroductionMonologue2");
        GenerateNew("IntroductionMonologue3");
        GenerateNew("IntroductionMonologue4");
        GenerateNew("IntroductionMonologue5", edgyTextColor, 1);
        GenerateNew("IntroductionMonologueBrief", edgyTextColor, 1);

        GenerateNew("ExoMechChoiceResponse1");
        GenerateNew("ExoMechChoiceResponse2", edgyTextColor, 1);

        GenerateNew("Interjection1");
        GenerateNew("Interjection2_BluntForceTrauma_Minor");
        GenerateNew("Interjection2_BluntForceTrauma_Major");
        GenerateNew("Interjection2_BluntForceTrauma_NearLethal");
        GenerateNew("Interjection2_Electricity_Minor");
        GenerateNew("Interjection2_Electricity_Major");
        GenerateNew("Interjection2_Electricity_NearLethal");
        GenerateNew("Interjection2_Internal_Minor");
        GenerateNew("Interjection2_Internal_Major");
        GenerateNew("Interjection2_Internal_NearLethal");
        GenerateNew("Interjection2_Plasma_Minor");
        GenerateNew("Interjection2_Plasma_Major");
        GenerateNew("Interjection2_Plasma_NearLethal");
        GenerateNew("Interjection2_Thermal_Minor");
        GenerateNew("Interjection2_Thermal_Major");
        GenerateNew("Interjection2_Thermal_NearLethal");
        GenerateNew("Interjection2_Undamaged");
        GenerateNew("Interjection3");
        GenerateNew("Interjection4");
        GenerateNew("Interjection5");
        GenerateNew("Interjection6");

        GenerateNew("Interjection7");
        GenerateNew("Interjection8");
        GenerateNew("Interjection9");
        GenerateNew("Interjection10");
        GenerateNew("Interjection10_Plural");
        GenerateNew("Interjection11", edgyTextColor, 1);

        GenerateNew("EndOfBattle_FirstDefeat1");
        GenerateNew("EndOfBattle_FirstDefeat2");
        GenerateNew("EndOfBattle_FirstDefeat3");
        GenerateNew("EndOfBattle_FirstDefeat4");
        GenerateNew("EndOfBattle_FirstDefeat5");
        GenerateNew("EndOfBattle_FirstDefeat6");

        GenerateNew("EndOfBattle_SuccessiveDefeat1");
        GenerateNew("EndOfBattle_SuccessiveDefeat2");
        GenerateNew("EndOfBattle_SuccessiveDefeat3_Perfect");
        GenerateNew("EndOfBattle_SuccessiveDefeat3_Excellent");
        GenerateNew("EndOfBattle_SuccessiveDefeat3_Good");
        GenerateNew("EndOfBattle_SuccessiveDefeat3_Acceptable");
        GenerateNew("EndOfBattle_SuccessiveDefeat3_Bad");
        GenerateNew("EndOfBattle_SuccessiveDefeat3_WhyDidYouMeltTheBoss");
        GenerateNew("EndOfBattle_SuccessiveDefeat4_Perfect");
        GenerateNew("EndOfBattle_SuccessiveDefeat4_Excellent");
        GenerateNew("EndOfBattle_SuccessiveDefeat4_ImproveFightTime");
        GenerateNew("EndOfBattle_SuccessiveDefeat4_ImproveAggression");
        GenerateNew("EndOfBattle_SuccessiveDefeat4_ImproveBuffs");
        GenerateNew("EndOfBattle_SuccessiveDefeat4_ImproveHitCounter");
        GenerateNew("EndOfBattle_SuccessiveDefeat4_Bad");
        GenerateNew("EndOfBattle_SuccessiveDefeat4_WhyDidYouMeltTheBoss");
        GenerateNew("EndOfBattle_SuccessiveDefeat5");

        GenerateNew("EndOfBattle_FirstDefeatReconBodyKill1");
        GenerateNew("EndOfBattle_FirstDefeatReconBodyKill2");
        GenerateNew("EndOfBattle_FirstDefeatReconBodyKill3");

        GenerateNew("Death", null, LumUtils.SecondsToFrames(1.85f));
        GenerateNew("PlayerDeathAtAmusingTime", null, LumUtils.SecondsToFrames(1.2f));

        GenerateNew("Error");
    }

    internal static DraedonDialogue GenerateNew(string key, Color? chatTextColorOverride = null, int? chatTextSpeakTimeOverride = null)
    {
        int chatTextSpeakTime = chatTextSpeakTimeOverride ?? DraedonBehavior.StandardSpeakTime;
        Color chatTextColor = chatTextColorOverride ?? CalamityMod.NPCs.ExoMechs.Draedon.TextColor;

        string localizationKey = $"Mods.WoTM.NPCs.Draedon.{key}";
        DraedonDialogue dialogue = new(
            localizationKey,
            new DraedonSubtitle(SoundPath(key)),
            new DraedonChatTextData(chatTextColor, chatTextSpeakTime)
        );
        Dialogue.Add(key, dialogue);
        return dialogue;
    }

    internal static string SoundPath(string relativePath) =>
        $"Assets/Sounds/Custom/VoiceActing/Drae_{relativePath}.wav";

    /// <summary>
    /// Updates the music volume based on whether Draedon is speaking.
    /// Creates a smooth ducking effect while respecting the user's original volume setting.
    /// </summary>
    public override void UpdateUI(GameTime gameTime)
    {
        bool isSpeaking = DraedonSubtitleManager.IsSpeaking;

        if (isSpeaking && !_isDucking)
        {
            _originalMusicVolume = Main.musicVolume;
            _isDucking = true;
        }

        if (!isSpeaking && _isDucking)
        {
            _isDucking = false;
            Main.musicVolume = _originalMusicVolume;
            return;
        }

        if (isSpeaking)
        {
            float targetVolume = Math.Min(MusicVolumeWhileSpeaking, _originalMusicVolume);
            float currentVolume = Main.musicVolume;

            if (Math.Abs(currentVolume - targetVolume) > 0.001f)
            {
                Main.musicVolume = MathHelper.Lerp(currentVolume, targetVolume, MusicVolumeTransitionSpeed);

                if (Math.Abs(Main.musicVolume - targetVolume) < 0.001f)
                    Main.musicVolume = targetVolume;
            }
        }
    }
}