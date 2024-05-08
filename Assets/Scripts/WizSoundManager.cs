using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizSoundManager : MonoBehaviour
{
  private AudioSource[] audioSources;
  private AudioSource walk;
  private AudioSource attack;
  private AudioSource spell_1;
  private AudioSource spell_2;
  
  void Start() {
    audioSources = GetComponents<AudioSource>();
    walk = audioSources[0];
    attack = audioSources[1];
    spell_1 = audioSources[2];
    spell_2 = audioSources[3];
  }

  public void PlayWalk() {
    walk.Play();
  }

  public void StopWalk() {
    walk.Stop();
  }

  public void PlayAttack() {
    attack.Play();
 }

  public void PlaySpell_1() {
    spell_1.Play();
 }

  public void PlaySpell_2() {
    spell_2.Play();
 }
}
