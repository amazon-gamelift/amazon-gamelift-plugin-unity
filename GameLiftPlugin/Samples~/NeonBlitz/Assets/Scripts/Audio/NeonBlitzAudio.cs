// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if !UNITY_SERVER

using System.Collections;
using UnityEngine;

/// <summary>
/// Procedurally generates all Neon Blitz sound effects using Unity's
/// <see cref="AudioSource"/> and synthesised waveforms — no audio assets
/// required.
///
/// Sounds produced:
///   • Move tick   — short blip
///   • Power-up    — rising chime
///   • Eliminate   — descending zap
///   • Countdown   — periodic beep
///   • Match start — ascending fanfare
///   • Match end   — resolving chord
///
/// Attach to the same GameObject as <see cref="NeonBlitzManager"/>.
/// </summary>
public class NeonBlitzAudio : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Range(0f, 1f)] [SerializeField] private float _masterVolume = 0.6f;

    // ── AudioSources ──────────────────────────────────────────────────────────
    private AudioSource _sfxSource;
    private AudioSource _bgmSource;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop   = true;
        _bgmSource.volume = _masterVolume * 0.35f;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void PlayMove()         => PlayClip(SynthBlip(0.03f, 880f,  880f));
    public void PlayPowerUp()      => PlayClip(SynthBlip(0.25f, 440f,  1320f));
    public void PlayEliminate()    => PlayClip(SynthBlip(0.35f, 660f,  110f));
    public void PlayShieldHit()    => PlayClip(SynthBlip(0.18f, 1100f, 880f));
    public void PlayBombBlast()    => PlayClip(SynthNoise(0.3f));
    public void PlayCountdown()    => StartCoroutine(PlayCountdownBeeps());
    public void PlayMatchStart()   => PlayClip(SynthFanfare());
    public void PlayMatchEnd()     => PlayClip(SynthChord());

    // ── Synthesis ─────────────────────────────────────────────────────────────

    /// <summary>Sine-wave blip sweeping from <paramref name="startHz"/> to <paramref name="endHz"/>.</summary>
    private static AudioClip SynthBlip(float duration, float startHz, float endHz)
    {
        const int SampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(duration * SampleRate);
        float[] data    = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t   = (float)i / sampleCount;
            float hz  = Mathf.Lerp(startHz, endHz, t);
            float env = Mathf.Sin(Mathf.PI * t);          // fade in + out
            data[i]   = Mathf.Sin(2f * Mathf.PI * hz * i / SampleRate) * env * 0.6f;
        }

        return Build(data, SampleRate, duration);
    }

    /// <summary>White-noise burst (bomb).</summary>
    private static AudioClip SynthNoise(float duration)
    {
        const int SampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(duration * SampleRate);
        float[] data    = new float[sampleCount];
        var rng         = new System.Random();

        for (int i = 0; i < sampleCount; i++)
        {
            float t   = (float)i / sampleCount;
            float env = Mathf.Pow(1f - t, 2f);
            data[i]   = ((float)rng.NextDouble() * 2f - 1f) * env * 0.5f;
        }

        return Build(data, SampleRate, duration);
    }

    /// <summary>Short rising fanfare (three tones).</summary>
    private static AudioClip SynthFanfare()
    {
        const int SampleRate = 44100;
        float duration  = 0.6f;
        int sampleCount = Mathf.RoundToInt(duration * SampleRate);
        float[] data    = new float[sampleCount];

        float[] freqs   = { 440f, 554f, 659f, 880f };
        float segLen    = duration / freqs.Length;

        for (int i = 0; i < sampleCount; i++)
        {
            float t   = (float)i / SampleRate;
            int   seg = Mathf.Min((int)(t / segLen), freqs.Length - 1);
            float env = 1f - (t - seg * segLen) / segLen;
            data[i]   = Mathf.Sin(2f * Mathf.PI * freqs[seg] * t) * env * 0.5f;
        }

        return Build(data, SampleRate, duration);
    }

    /// <summary>Resolving major chord (game end).</summary>
    private static AudioClip SynthChord()
    {
        const int SampleRate = 44100;
        float duration  = 1.0f;
        int sampleCount = Mathf.RoundToInt(duration * SampleRate);
        float[] data    = new float[sampleCount];
        float[] freqs   = { 261.6f, 329.6f, 392f, 523.3f };

        for (int i = 0; i < sampleCount; i++)
        {
            float t   = (float)i / SampleRate;
            float env = Mathf.Pow(1f - t / duration, 1.5f);
            float s   = 0f;
            foreach (float hz in freqs)
                s += Mathf.Sin(2f * Mathf.PI * hz * t);
            data[i] = (s / freqs.Length) * env * 0.5f;
        }

        return Build(data, SampleRate, duration);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IEnumerator PlayCountdownBeeps()
    {
        for (int i = 0; i < 3; i++)
        {
            PlayClip(SynthBlip(0.12f, 660f, 660f));
            yield return new WaitForSeconds(1f);
        }
        PlayClip(SynthBlip(0.2f, 880f, 1320f));
    }

    private void PlayClip(AudioClip clip)
    {
        _sfxSource.PlayOneShot(clip, _masterVolume);
    }

    private static AudioClip Build(float[] data, int sampleRate, float duration)
    {
        var clip = AudioClip.Create("synth", data.Length, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}

#endif // !UNITY_SERVER
