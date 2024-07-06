using System;
using UnityEngine;

namespace Pancake.Sound
{
    public static class AudioStatic
    {
        #region Play

        /// <summary>
        /// Play an audio
        /// </summary>
        /// <param name="id"></param>
        public static IAudioPlayer Play(this SoundId id) => SoundManager.Instance?.Play(id);

        /// <summary>
        /// Play an audio at the given position
        /// </summary>
        /// <param name="id"></param>
        /// <param name="position"></param>
        public static IAudioPlayer Play(this SoundId id, Vector3 position) => SoundManager.Instance?.Play(id, position);

        /// <summary>
        /// Play an audio and let it keep following the target
        /// </summary>
        /// <param name="id"></param>
        /// <param name="followTarget"></param>
        public static IAudioPlayer Play(this SoundId id, Transform followTarget) => SoundManager.Instance?.Play(id, followTarget);

        #endregion

        #region Stop

        /// <summary>
        /// Stop playing all audio that match the given audio type
        /// </summary>
        /// <param name="audioType"></param>
        public static void Stop(this EAudioType audioType) => SoundManager.Instance.Stop(audioType);

        /// <summary>
        /// Stop playing all audio that match the given audio type
        /// </summary>
        /// <param name="audioType"></param>
        /// <param name="fadeOut">Set this value to override the LibraryManager's setting</param>
        public static void Stop(this EAudioType audioType, float fadeOut) => SoundManager.Instance?.Stop(audioType, fadeOut);

        /// <summary>
        /// Stop playing an audio
        /// </summary>
        /// <param name="id"></param>
        public static void Stop(this SoundId id) => SoundManager.Instance?.Stop(id);

        /// <summary>
        /// Stop playing an audio
        /// </summary>
        /// <param name="id"></param>
        /// /// <param name="fadeOut">Set this value to override the LibraryManager's setting</param>
        public static void Stop(this SoundId id, float fadeOut) => SoundManager.Instance?.Stop(id, fadeOut);

        #endregion

        #region Pause

        /// <summary>
        /// Pause an audio
        /// </summary>
        /// <param name="id"></param>
        public static void Pause(this SoundId id) => SoundManager.Instance?.Pause(id);

        /// <summary>
        /// Pause an audio
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fadeOut">Set this value to override the LibraryManager's setting</param>
        public static void Pause(this SoundId id, float fadeOut) => SoundManager.Instance?.Pause(id, fadeOut);

        #endregion

        #region Volume

        /// <summary>
        /// Set the master volume
        /// </summary>
        /// <param name="volume">Accepts values from 0 to 10, default is 1</param>
        public static void SetVolume(float volume) => SetVolume(EAudioType.All, volume);

        /// <summary>
        /// Set the volume of the given audio type
        /// </summary>
        /// <param name="volume">Accepts values from 0 to 10, default is 1</param>
        /// <param name="audioType"></param>
        public static void SetVolume(this EAudioType audioType, float volume) => SetVolume(audioType, volume, AudioConstant.FADE_TIME_IMMEDIATE);

        /// <summary>
        /// Set the volume of the given audio type
        /// </summary>
        /// <param name="volume">Accepts values from 0 to 10, default is 1</param>
        /// <param name="audioType"></param>
        /// <param name="fadeTime">Set this value to override the LibraryManager's setting</param>
        public static void SetVolume(this EAudioType audioType, float volume, float fadeTime) => SoundManager.Instance?.SetVolume(volume, audioType, fadeTime);

        /// <summary>
        /// Set the volume of an audio
        /// </summary>
        /// <param name="volume">Accepts values from 0 to 10, default is 1</param>
        /// <param name="id"></param>
        public static void SetVolume(this SoundId id, float volume) => SetVolume(id, volume, AudioConstant.FADE_TIME_IMMEDIATE);

        /// <summary>
        /// Set the volume of an audio
        /// </summary>
        /// <param name="volume">Accepts values from 0 to 10, default is 1</param>
        /// <param name="id"></param>
        /// <param name="fadeTime">Set this value to override the LibraryManager's setting</param>
        public static void SetVolume(this SoundId id, float volume, float fadeTime) => SoundManager.Instance?.SetVolume(id, volume, fadeTime);

        #endregion

        /// <summary>
        /// Set all audio's pitch immediately
        /// </summary>
        /// <param name="pitch">values between -3 to 3, default is 1</param>
        public static void SetPitch(float pitch) => SoundManager.Instance?.SetPitch(pitch, EAudioType.All, AudioConstant.FADE_TIME_IMMEDIATE);

        /// <summary>
        /// Set the pitch of the given audio type immediately
        /// </summary>
        /// <param name="pitch">values between -3 to 3, default is 1</param>
        /// <param name="audioType"></param>
        public static void SetPitch(float pitch, EAudioType audioType) => SoundManager.Instance?.SetPitch(pitch, audioType, AudioConstant.FADE_TIME_IMMEDIATE);

        /// <summary>
        /// Set all audio's pitch
        /// </summary>
        /// <param name="pitch">values between -3 to 3, default is 1</param>
        /// <param name="fadeTime"></param>
        public static void SetPitch(float pitch, float fadeTime) => SoundManager.Instance?.SetPitch(pitch, EAudioType.All, fadeTime);

        /// <summary>
        /// Set the pitch of the given audio type
        /// </summary>
        /// <param name="pitch">values between -3 to 3, default is 1</param>
        /// <param name="audioType"></param>
        /// <param name="fadeTime"></param>
        public static void SetPitch(float pitch, EAudioType audioType, float fadeTime) => SoundManager.Instance?.SetPitch(pitch, audioType, fadeTime);

#if !UNITY_WEBGL

        #region Effect

        /// <summary>
        /// Set effect for all audio
        /// </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public static IAutoResetWaitable SetEffect(Effect effect) => SoundManager.Instance?.SetEffect(effect);

        /// <summary>
        /// Set effect for all audio that mathch the given audio type
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="audioType"></param>
        /// <returns></returns>
        public static IAutoResetWaitable SetEffect(Effect effect, EAudioType audioType) => SoundManager.Instance?.SetEffect(audioType, effect);

        #endregion

#endif

        #region Chain Method

        #region Stop

        public static void Stop(this IAudioStoppable player) => player?.Stop();
        public static void Stop(this IAudioStoppable player, Action onFinished) => player?.Stop(onFinished);

        public static void Stop(this IAudioStoppable player, float fadeOut) => player?.Stop(fadeOut);

        public static void Stop(this IAudioStoppable player, float fadeOut, Action onFinished) => player?.Stop(fadeOut, onFinished);

        public static void Pause(this IAudioStoppable player) => player?.Pause();

        public static void Pause(this IAudioStoppable player, float fadeOut) => player?.Pause(fadeOut);

        #endregion

        #region Set Pitch

        /// <summary>
        /// Set the player's pitch (-3 ~ 3).
        /// </summary>
        /// <param name="player"></param>
        /// <param name="pitch">The target pitch</param>
        /// <param name="fadeTime">Time to reach the target volume from the current volume.</param>
        public static IAudioPlayer SetPitch(this IAudioPlayer player, float pitch, float fadeTime = AudioConstant.FADE_TIME_IMMEDIATE) =>
            player?.SetPitch(pitch, fadeTime);

        /// <inheritdoc cref="SetPitch(IAudioPlayer,float,float)"/>
        public static IAudioPlayer SetPitch(this IMusicPlayer player, float pitch, float fadeTime = AudioConstant.FADE_TIME_IMMEDIATE) =>
            player?.SetPitch(pitch, fadeTime);

        /// <inheritdoc cref="SetPitch(IAudioPlayer,float,float)"/>
        public static IAudioPlayer SetPitch(this IPlayerEffect player, float pitch, float fadeTime = AudioConstant.FADE_TIME_IMMEDIATE) =>
            player?.SetPitch(pitch, fadeTime);

        #endregion

        #region Set Volume

        /// <summary>
        /// Set the player's volume (0~1)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="volume">The target volume</param>
        /// <param name="fadeTime">Time to reach the target volume from the current volume.</param>
        public static IAudioPlayer SetVolume(this IAudioPlayer player, float volume, float fadeTime = AudioConstant.FADE_TIME_IMMEDIATE) =>
            player?.SetVolume(volume, fadeTime);

        /// <inheritdoc cref="SetVolume(IAudioPlayer,float,float)"/>
        public static IAudioPlayer SetVolume(this IMusicPlayer player, float volume, float fadeTime = AudioConstant.FADE_TIME_IMMEDIATE) =>
            player?.SetVolume(volume, fadeTime);

        /// <inheritdoc cref="SetVolume(IAudioPlayer,float,float)"/>
        public static IAudioPlayer SetVolume(this IPlayerEffect player, float volume, float fadeTime = AudioConstant.FADE_TIME_IMMEDIATE) =>
            player?.SetVolume(volume, fadeTime);

        #endregion

        #region As Background Music

        /// <summary>
        /// As a background music, which will transition automatically if another BGM is played after it.
        /// </summary>
        public static IMusicPlayer AsBGM(this IAudioPlayer player) => player?.AsBGM();

        /// <inheritdoc cref="AsBGM(IAudioPlayer)"/>
        public static IMusicPlayer AsBGM(this IPlayerEffect player) => player?.AsBGM();

        #endregion

        #region Set Transition

        /// <summary>
        /// Set the transition type for BGM
        /// </summary>
        /// <param name="player"></param>
        /// <param name="transition">Transition type</param>
        /// <returns></returns>
        public static IMusicPlayer SetTransition(this IMusicPlayer player, EAudioTransition transition) =>
            player?.SetTransition(transition, AudioPlayer.USE_ENTITY_SETTING);

        /// <param name="transition"></param>
        /// <param name="overrideFade">Override value of the fading time</param>
        /// <param name="player"></param>
        /// <inheritdoc cref="SetTransition(IMusicPlayer, EAudioTransition)"/>
        public static IMusicPlayer SetTransition(this IMusicPlayer player, EAudioTransition transition, float overrideFade) =>
            player?.SetTransition(transition, default, overrideFade);

        /// <param name="transition"></param>
        /// <param name="stopMode">The stop mode of the previous player</param>
        /// <param name="player"></param>
        /// <inheritdoc cref="SetTransition(IMusicPlayer, EAudioTransition)"/>
        public static IMusicPlayer SetTransition(this IMusicPlayer player, EAudioTransition transition, EAudioStopMode stopMode) =>
            player?.SetTransition(transition, stopMode, AudioPlayer.USE_ENTITY_SETTING);

        /// <param name="stopMode"></param>
        /// <param name="overrideFade">Override value of the fading time</param>
        /// <param name="player"></param>
        /// <param name="transition"></param>
        /// <inheritdoc cref="SetTransition(IMusicPlayer, EAudioTransition,EAudioStopMode)"/>
        public static IMusicPlayer SetTransition(this IMusicPlayer player, EAudioTransition transition, EAudioStopMode stopMode, float overrideFade) =>
            player?.SetTransition(transition, stopMode, overrideFade);

        #endregion

#if !UNITY_WEBGL

        #region As Dominator

        /// <summary>
        /// To be a dominator, which will affect or change the behavior of other audio players.
        /// </summary>
        /// <param name="player"></param>
        public static IPlayerEffect AsDominator(this IAudioPlayer player) => player?.AsDominator();

        /// <inheritdoc>
        ///     <cref>AsDominator(IAudioPlayer, EAudioType)</cref>
        /// </inheritdoc>
        public static IPlayerEffect AsDominator(this IMusicPlayer player) => player?.AsDominator();

        /// <summary>
        /// While this audio player is playing, the volume of other audio players will be lowered to the given ratio.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="othersVol"></param>
        /// <param name="fadeTime">The time duration of the FadeIn and FadeOut</param>
        public static IPlayerEffect QuietOthers(this IPlayerEffect player, float othersVol, float fadeTime = AudioConstant.FADE_TIME_QUICK) =>
            player?.QuietOthers(othersVol, fadeTime);

        /// <inheritdoc cref = "QuietOthers(IPlayerEffect, float, float)" />
        /// <param name="othersVol"></param>
        /// <param name="fading">The fading setting of this action</param>
        /// <param name="player"></param>
        public static IPlayerEffect QuietOthers(this IPlayerEffect player, float othersVol, Fading fading) => player?.QuietOthers(othersVol, fading);

        /// <summary>
        /// While this audio player is playing, a lowpass filter will be added to other audio players (i.e. their higher frequencies will be cutted off)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="frequency">10 Hz ~ 22000Hz</param>
        /// <param name="fadeTime">The time duration of the FadeIn and FadeOut</param>
        public static IPlayerEffect LowPassOthers(
            this IPlayerEffect player,
            float frequency = AudioConstant.LOW_PASS_FREQUENCY,
            float fadeTime = AudioConstant.FADE_TIME_QUICK) =>
            player?.LowPassOthers(frequency, fadeTime);

        /// <inheritdoc cref = "LowPassOthers(IPlayerEffect, float, float)" />
        /// <param name="frequency"></param>
        /// <param name="fading">The fading setting of this action</param>
        /// <param name="player"></param>
        public static IPlayerEffect LowPassOthers(this IPlayerEffect player, float frequency, Fading fading) => player?.LowPassOthers(frequency, fading);

        /// <summary>
        /// While this audio player is playing, a highpass filter will be added to other audio players (i.e. their lower frequencies will be cutted off)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="frequency">10 Hz ~ 22000Hz</param>
        /// <param name="fadeTime">The time duration of the FadeIn and FadeOut</param>
        public static IPlayerEffect HighPassOthers(
            this IPlayerEffect player,
            float frequency = AudioConstant.HIGH_PASS_FREQUENCY,
            float fadeTime = AudioConstant.FADE_TIME_QUICK) =>
            player?.HighPassOthers(frequency, fadeTime);

        /// <inheritdoc cref = "HighPassOthers(IPlayerEffect, float, float)" />
        /// <param name="frequency"></param>
        /// <param name="fading">The fading setting of this action</param>
        /// <param name="player"></param>
        public static IPlayerEffect HighPassOthers(this IPlayerEffect player, float frequency, Fading fading) => player?.HighPassOthers(frequency, fading);

        // Note: LowPass == HighCut , HighPass == LowCut

        #endregion

#endif

        #endregion
    }
}