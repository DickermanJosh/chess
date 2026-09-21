using UnityEngine;

namespace Opera
{
    public enum OperaSound { Lift, Move, Capture, Castle, Button, Return }

    /// <summary>Original, deterministic wooden impacts. No downloaded recordings or DSP plugin.</summary>
    public sealed class OperaAudio : MonoBehaviour
    {
        private static OperaAudio instance;
        private AudioSource source;
        private AudioClip[][] clips;
        private int variation;
        public static bool Muted
        {
            get => PlayerPrefs.GetInt("Opera.SoundMuted", 0) == 1;
            set => PlayerPrefs.SetInt("Opera.SoundMuted", value ? 1 : 0);
        }
        public static void Play(OperaSound sound)
        {
            if (Muted || !Application.isPlaying) return;
            if (instance == null) instance = new GameObject("Wooden piece sounds").AddComponent<OperaAudio>();
            var variants = instance.clips[(int)sound];
            instance.source.PlayOneShot(variants[instance.variation++ % variants.Length], sound == OperaSound.Button ? .28f : .65f);
        }
        private void Awake()
        {
            instance = this; DontDestroyOnLoad(gameObject);
            source = gameObject.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend = 0;
            clips = new AudioClip[6][];
            for (int sound = 0; sound < clips.Length; sound++)
            {
                clips[sound] = new AudioClip[3];
                for (int variant = 0; variant < 3; variant++) clips[sound][variant] = Make((OperaSound)sound, variant);
            }
        }
        private static AudioClip Make(OperaSound sound, int variant)
        {
            const int rate = 44100;
            bool small = sound == OperaSound.Lift || sound == OperaSound.Button || sound == OperaSound.Return;
            float duration = small ? .15f : .36f;
            var samples = new float[(int)(rate * duration)];
            var random = new System.Random(913 + (int)sound * 101 + variant);
            float filtered = 0, previous = 0, baseHz = (small ? 185 : sound == OperaSound.Capture ? 93 : 118) + variant * 7;
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)rate;
                float attack = 1 - Mathf.Exp(-t * 650);
                float noise = (float)random.NextDouble() * 2 - 1;
                filtered += .10f * (noise - filtered); // Damp the sharp high frequencies.
                float body = Mathf.Sin(2 * Mathf.PI * baseHz * t) * Mathf.Exp(-t * 27) * .40f +
                    Mathf.Sin(2 * Mathf.PI * baseHz * 1.71f * t + .8f) * Mathf.Exp(-t * 38) * .21f +
                    Mathf.Sin(2 * Mathf.PI * baseHz * 2.43f * t) * Mathf.Exp(-t * 58) * .10f;
                float friction = filtered * Mathf.Exp(-t * (small ? 38 : 32)) * .90f;
                float sample = (body + friction) * attack;
                if (sound == OperaSound.Castle && t > .095f)
                    sample += .24f * Mathf.Sin(2 * Mathf.PI * 137 * (t - .095f)) * Mathf.Exp(-(t - .095f) * 34) * (1 - Mathf.Exp(-(t - .095f) * 550));
                previous += .22f * (sample - previous);
                samples[i] = Mathf.Clamp(previous * (small ? .52f : 1), -.9f, .9f) * Mathf.Clamp01((duration - t) / .035f);
            }
            var clip = AudioClip.Create("Walnut " + sound + " " + variant, samples.Length, 1, rate, false);
            clip.SetData(samples, 0); return clip;
        }
        private void OnDestroy()
        {
            if (instance == this) instance = null;
            if (clips == null) return;
            foreach (var group in clips) foreach (var clip in group) if (clip != null) Destroy(clip);
        }
    }
}
