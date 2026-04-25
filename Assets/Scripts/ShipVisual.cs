using System;
using UnityEngine;

public class ShipVisual : MonoBehaviour
{
    [SerializeField] ParticleSystem middleThrusterParticleSystem;
    [SerializeField] ParticleSystem leftThrusterParticleSystem;
    [SerializeField] ParticleSystem rightThrusterParticleSystem;


    private PlayerMovement lander;

    private void Awake()
    {
        lander = GetComponent<PlayerMovement>();
        lander.onUpForce += lander_OnUpForce ;
        lander.onLeftForce += lander_OnLeftForce;
        lander.onRightForce += lander_OnRightForce;

        lander.onBeforeForce += lander_OnBeforeForce;

        SetEnabledThrusterPaticleSystem(middleThrusterParticleSystem, false);
        SetEnabledThrusterPaticleSystem(leftThrusterParticleSystem, false);
        SetEnabledThrusterPaticleSystem(rightThrusterParticleSystem, false);


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
