using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotSoundManager : MonoBehaviour
{
  private AudioSource[] audioSources;
  private AudioSource roboMove;
  private AudioSource laser_atk;
  void Start() {
    audioSources = GetComponents<AudioSource>();
    roboMove = audioSources[0];
    laser_atk = audioSources[1];
  }

  public void PlayRoboAtk() {
    laser_atk.Play();
  }

  public void PlayRoboMove() {
    roboMove.Play();
 }

  public void StopRoboMove() {
    roboMove.Stop();
  }
} 