using System;
using SpaceP.Scoring;
using UnityEngine;

public class ShipVisual : MonoBehaviour
{
    [SerializeField] ParticleSystem middleThrusterParticleSystem;
    [SerializeField] ParticleSystem leftThrusterParticleSystem;
    [SerializeField] ParticleSystem rightThrusterParticleSystem;
    [SerializeField] GameObject landerVfx;


    private void Awake()
    {
        PlayerMovement.Instance.onUpForce += lander_OnUpForce ;
        PlayerMovement.Instance.onLeftForce += lander_OnLeftForce;
        PlayerMovement.Instance.onRightForce += lander_OnRightForce;
        PlayerMovement.Instance.onBeforeForce += lander_OnBeforeForce;

        SetEnabledThrusterPaticleSystem(middleThrusterParticleSystem, false);
        SetEnabledThrusterPaticleSystem(leftThrusterParticleSystem, false);
        SetEnabledThrusterPaticleSystem(rightThrusterParticleSystem, false);


    }

    private void Start()
    {
        PlayerMovement.Instance.onLanded += Lander_onLanded;
    }

    private void Lander_onLanded(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        switch(e.Result.Type)
        {
            case LandingType.TooFast:
            case LandingType.TooSteep:
            case LandingType.WrongLandingArea:
                Instantiate(landerVfx, transform.position, Quaternion.identity);
                gameObject.SetActive(false);
                CinemachineCameraShake.Instance.ShakeCamera(8f,  0.25f);
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
        if (particleSystem == null)
        {
            return;
        }

        ParticleSystem[] particleSystems =
            particleSystem.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem currentParticleSystem in particleSystems)
        {
            ParticleSystem.EmissionModule emissionModule = currentParticleSystem.emission;
            emissionModule.enabled = enalbed;
        }
    }
}
