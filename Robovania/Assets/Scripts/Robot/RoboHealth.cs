using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ROBOHEALTH:
 * Handles pawn death. Basically just handles out of bounds death cases
 * could be extended to include a health value that modifies incoming
 * knockback values, a la SSB.
 * */
public class RoboHealth : MonoBehaviour
{
    public GameObject explosionPrefab;

    private AudioSource explosionAudio;
    private ParticleSystem explosionParticles;
    private bool dead;

    private void Awake()
    {
        explosionParticles = Instantiate(explosionPrefab).GetComponent<ParticleSystem>();
        explosionAudio = explosionParticles.GetComponent<AudioSource>();

        explosionParticles.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        dead = false;
    }

    private void Update()
    {
        if (transform.position.y < -20f || transform.position.x > 25f)
        {
            if(!dead)
            {
                OnDeath();
            }
        }
    }

    public void TakeDamage(float amount)
    {

    }

    private void SetHealthUI()
    {

    }

    private void OnDeath()
    {
        dead = true;
        explosionParticles.transform.position = transform.position;
        explosionParticles.gameObject.SetActive(true);

        explosionParticles.Play();
        explosionAudio.Play();

        gameObject.SetActive(false);
    }
}
