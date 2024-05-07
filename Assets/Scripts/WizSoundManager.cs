using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizSoundManager : MonoBehaviour
{
  private AudioSource[] audioSources;
  private AudioSource walk;
  private AudioSource spell_1;
  private AudioSource spell_2;
  private AudioSource deathsfx;
  void Start() {
    audioSources = GetComponents<AudioSource>();
    walk = audioSources[0];
    spell_1 = audioSources[1];
    spell_2 = audioSources[2];
    deathsfx = audioSources[3];
  }

  public void PlayWalk() {
    walk.Play();
  }

  public void StopWalk() {
    walk.Stop();
  }

  public void PlaySpell_1() {
    spell_1.Play();
 }

  public void PlaySpell_2() {
    spell_2.Play();
 }

 public void PlayDeathSFX() {
    deathsfx.Play();
 }
}
