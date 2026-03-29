using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource sfxSource;

    public AudioClip correctClip;
    public AudioClip incorrectClip;
    public AudioClip explodeClip;

    public void PlayCorrect()
    {
        if (sfxSource != null && correctClip != null)
            sfxSource.PlayOneShot(correctClip);
    }

    public void PlayIncorrect()
    {
        if (sfxSource != null && incorrectClip != null)
            sfxSource.PlayOneShot(incorrectClip);
    }

    public void PlayExplode()
    {
        if (sfxSource != null && explodeClip != null)
            sfxSource.PlayOneShot(explodeClip);
    }
}