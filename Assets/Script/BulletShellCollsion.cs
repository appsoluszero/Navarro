using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BulletShellCollsion : MonoBehaviour
{
    private ParticleSystem part;
    private List<ParticleCollisionEvent> collisionEvents;

    [Header("Sound")]
    [Tooltip("Minimum collsion velocity that still produce a sound")]
    
    public AudioSource audioSource;
    [Tooltip("Bullet with collision velocity lesser than this produce no sound")]
    public float minAudiableVelocity = 2f;
    [Tooltip("Bullet with collision velocity greater than this have volume = 1")]
    public float maxDistinguishableVelocity = 12f;
    [Tooltip("Curve that convert collision velocity to sound volume, x=0 is audiableVelocity and x=1 is maxDistinguishableVelocity")]
    public AnimationCurve velocityToSoundCurve;

    // Start is called before the first frame update
    void Start()
    {
        this.part = GetComponent<ParticleSystem>();
        this.collisionEvents = new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject other) {
        this.part.GetCollisionEvents(other, this.collisionEvents);
        foreach (var collsion in this.collisionEvents)
        {
            var velocity = collsion.velocity.magnitude;
            if(velocity > this.minAudiableVelocity) {
                var t = (velocity-this.minAudiableVelocity)/(this.maxDistinguishableVelocity-this.minAudiableVelocity);
                float volume;
                if(t > 1) {
                    volume = this.velocityToSoundCurve.Evaluate(1);
                }
                else {
                    volume = this.velocityToSoundCurve.Evaluate(t);
                }
                var audio = Instantiate(this.audioSource, collsion.intersection, Quaternion.identity);
                audio.volume = volume;
                Destroy(audio.gameObject, audio.clip.length + 1);
            }
        }
    }
}
