using System;
using UnityEngine;

public class ShipVisual : MonoBehaviour
{
    [SerializeField] ParticleSystem middleThrusterParticleSystem;
    [SerializeField] ParticleSystem leftThrusterParticleSystem;
    [SerializeField] ParticleSystem rightThrusterParticleSystem;
    [SerializeField] GameObject landerVfx;


    private void Awake()
    {
        PlayerMovement.instance.onUpForce += lander_OnUpForce ;
        PlayerMovement.instance.onLeftForce += lander_OnLeftForce;
        PlayerMovement.instance.onRightForce += lander_OnRightForce;
        PlayerMovement.instance.onBeforeForce += lander_OnBeforeForce;

        SetEnabledThrusterPaticleSystem(middleThrusterParticleSystem, false);
        SetEnabledThrusterPaticleSystem(leftThrusterParticleSystem, false);
        SetEnabledThrusterPaticleSystem(rightThrusterParticleSystem, false);


    }

    private void Start()
    {
        PlayerMovement.instance.onLanded += Lander_onLanded;
    }

    private void Lander_onLanded(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        switch(e.landingType)
        {
            case PlayerMovement.LandingType.TooSpeedLanding:
            case PlayerMovement.LandingType.TooSteepAngle:
            case PlayerMovement.LandingType.WrongLandingArea:
                Instantiate(landerVfx, transform.position, Quaternion.identity);
                gameObject.SetActive(false);
                break;

        }
    }

    private void lander_OnBeforeForce(object sender, EventArgs e)
    {
        SetEnabledThrusterPaticleSystem(middleThrusterParticleSystem, false);
        SetEnabledThrusterPaticleSystem(leftThrusterParticleSystem, false);
        SetEnabledThrusterPaticleSystem(rightThrusterParticleSystem, false);
    }

    private void lander_OnRightForce(object sender, EventArgs e)
    {
        SetEnabledThrusterPaticleSystem(leftThrusterParticleSystem, true);
    }

    private void lander_OnLeftForce(object sender, EventArgs e)
    {
        SetEnabledThrusterPaticleSystem(rightThrusterParticleSystem, true);
    }

    private void lander_OnUpForce(object sender, EventArgs e)
    {
        SetEnabledThrusterPaticleSystem(middleThrusterParticleSystem, true);
        SetEnabledThrusterPaticleSystem(leftThrusterParticleSystem, true);
        SetEnabledThrusterPaticleSystem(rightThrusterParticleSystem, true);
    }

    private void SetEnabledThrusterPaticleSystem(ParticleSystem particleSystem, bool enalbed)
    {
        ParticleSystem.EmissionModule emissionModule = particleSystem.emission;
        emissionModule.enabled = enalbed;
    }
}
