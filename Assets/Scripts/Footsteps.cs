using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string footstepReference;
    [FMODUnity.EventRef]
    public string jumpReference;
    [Range (0.0f, 1.0f)]
    public float maxWalkingTickTime = 0f;
    [Range(0.5f, 2.0f)]
    public float footstepTimeMultiplier = 0.5f;
    public float minTriggerSpeed = 0.1f;
    public KinematicCharacterController.KinematicCharacterMotor playerMotor;

    private float _footstepSpeed = 0f;
    private FMOD.Studio.EventInstance _footstepEvent;
    private FMOD.Studio.EventInstance _jumpEvent;
    private float _timer = 0f;
    private Vector3 _playerMoveSpeedVector;

    private float _lastKnownFallSpeed;

    private const int TAKEOFF = 0;
    private const int LANDING = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        KinematicCharacterController.KinematicCharacterMotorState playerState = playerMotor.GetState();
        if (playerState.MustUnground)
        {
            CallJumpSound(TAKEOFF);
        } else if (playerState.LastMovementIterationFoundAnyGround) {
            CallJumpSound(LANDING);
        }
        _footstepSpeed = Mathf.Min (maxWalkingTickTime, footstepTimeMultiplier / GetPlayerMoveSpeed());
        _timer += Time.deltaTime;
        if (_timer > _footstepSpeed && GetPlayerMoveSpeed() > minTriggerSpeed && playerMotor.GroundingStatus.IsStableOnGround)
        {
            CallFootstepSound();
            _timer = 0f;
        }

        _lastKnownFallSpeed = GetPlayerFallSpeed();
    }

    void CallFootstepSound ()
    {
        _footstepEvent = FMODUnity.RuntimeManager.CreateInstance(footstepReference);
        _footstepEvent.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        _footstepEvent.setParameterByName("Speed", GetPlayerMoveSpeed());
        _footstepEvent.start();
        _footstepEvent.release();
    }

    void CallJumpSound (int state)
    {
        _jumpEvent = FMODUnity.RuntimeManager.CreateInstance(jumpReference);
        _jumpEvent.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        _jumpEvent.setParameterByName("Jump State", state);
        _jumpEvent.setParameterByName("LandingSpeed", _lastKnownFallSpeed);
        _jumpEvent.start();
        _jumpEvent.release();
    }

    float GetPlayerMoveSpeed ()
    {
        _playerMoveSpeedVector = playerMotor.Velocity;
        _playerMoveSpeedVector.y *= 0;
        return _playerMoveSpeedVector.magnitude;
    }

    float GetPlayerFallSpeed ()
    {
        _playerMoveSpeedVector = playerMotor.Velocity;
        _playerMoveSpeedVector.x *= 0;
        _playerMoveSpeedVector.z *= 0;
        if (_playerMoveSpeedVector.y < -4) Debug.Log(_playerMoveSpeedVector.magnitude);
        return _playerMoveSpeedVector.y < -4 ? _playerMoveSpeedVector.magnitude: 0;
    }

}
