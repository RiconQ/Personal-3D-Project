using UnityEngine;

public class Sliding : MonoBehaviour
{
    private PlayerCharacter _pm;

    [Header("Sliding")]
    [SerializeField] private float _slideStartSpeed = 25f;
    [SerializeField] private float _slideEndSpeed = 15;
    [SerializeField] private float _slideFriction = 0.8f;
    [SerializeField] private float _slideSteerAcceleration = 5f;
    [SerializeField] private float _slideGravity = -90f;

    [Header("SlidingJump")]
    [SerializeField] private float _slideJumpNormal = 30;
    [SerializeField] private float _slideJumpUp = 40;
    [SerializeField] private float _slideJumpForward = 30;
    [SerializeField] private float _slideThreshold = 0.1f;

    public void Initialize()
    {
        _pm = GetComponent<PlayerCharacter>();
    }

    public void StartSliding(ref Vector3 currentVelocity, Vector3 groundedMovement)
    {
        var moving = groundedMovement.sqrMagnitude > 0f;
        var crouching = _pm.currentState.Stance is EStance.Crouch;
        var wasStanding = _pm.lastState.Stance is EStance.Stand;
        var wasInAir = !_pm.lastState.Grounded;
        if (moving && crouching && (wasStanding || wasInAir))
        {
            _pm.currentState.Stance = EStance.Slide;

            /*
             * When landing on stable ground the Character motor projects the velocity onto a flat ground plane
             * See : KinematicCharacterMotor.HandleVelocityProjection()
             * This is normally good, because under normal circumstances the player shouldn't slide when landing on the ground;
             * In this case, we want the player to slide
             * Reproject the last frame (falling) velocity onto the ground normal to slide
             */
            if (wasInAir)
            {
                currentVelocity = Vector3.ProjectOnPlane
                (
                    vector: _pm.lastState.Velocity,
                    planeNormal: _pm.Motor.GroundingStatus.GroundNormal
                );
                //currentVelocity = _lastState.Velocity;
            }

            var effectiveSlideStartSpeed = _slideStartSpeed;
            if (!_pm.lastState.Grounded && !_pm.requestedInput.CrouchInAir)
            {
                effectiveSlideStartSpeed = 0f;
                _pm.requestedInput.CrouchInAir = false;
            }

            var slideSpeed = Mathf.Max(effectiveSlideStartSpeed, currentVelocity.magnitude);
            currentVelocity = _pm.Motor.GetDirectionTangentToSurface
            (
                direction: currentVelocity,
                surfaceNormal: _pm.Motor.GroundingStatus.GroundNormal
            ) * slideSpeed;
        }
    }

    public void ContinueSliding(ref Vector3 currentVelocity, float deltaTime, Vector3 groundedMovement)
    {
        //Friction
        currentVelocity -= currentVelocity * (_slideFriction * deltaTime);

        // Slope
        {
            var force = Vector3.ProjectOnPlane
            (
                vector: -_pm.Motor.CharacterUp,
                planeNormal: _pm.Motor.GroundingStatus.GroundNormal
            ) * _slideGravity;
            // In video 

            var groundNormal = _pm.Motor.GroundingStatus.GroundNormal;

            Vector3 slopeDirection = Vector3.Cross(Vector3.Cross(groundNormal, Vector3.up), groundNormal).normalized;

            float dotProduct = Vector3.Dot(_pm.Motor.CharacterForward, slopeDirection);

            if (dotProduct > _slideThreshold)
            {
                //Upwards Slide
            }
            else if (dotProduct < -_slideThreshold)
            {
                //Downwards Slide
                currentVelocity -= force * deltaTime;
            }
        }

        // Steer 
        {
            //Target velocity is the player's movement direction at the current speed
            var currentSpeed = currentVelocity.magnitude;
            var targetVelocity = groundedMovement * currentSpeed;
            var steerVelocity = currentVelocity;
            var steerForce = (targetVelocity - steerVelocity) * _slideSteerAcceleration * deltaTime;

            //Add steer force, but clamp velocity so the slide speed doesnt increase due to direct movement input
            steerVelocity += steerForce;
            steerVelocity = Vector3.ClampMagnitude(steerVelocity, currentSpeed);

            _pm.currentState.Acceleration = (steerVelocity - currentVelocity) / deltaTime;

            currentVelocity = steerVelocity;
        }

        //Stop
        if (currentVelocity.magnitude < _slideEndSpeed)
            _pm.currentState.Stance = EStance.Crouch;
    }

    public void SlidingJump(ref Vector3 currentVelocity)
    {
        var groundNormal = _pm.Motor.GroundingStatus.GroundNormal;

        Vector3 slopeDirection = Vector3.Cross(Vector3.Cross(groundNormal, Vector3.up), groundNormal).normalized;

        float dotProduct = Vector3.Dot(_pm.Motor.CharacterForward, slopeDirection);

        if (dotProduct > _slideThreshold)
        {
            Debug.Log("Upwards Slide - 플레이어가 경사면을 위로 슬라이딩 중");
            // Set Minimum Vertical Speed to the Jump Speed
            var currentVerticalSpeed = Vector3.Dot(currentVelocity, _pm.Motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, _slideJumpUp);

            // Add the difference in current and target vertical speed to the character's velocity
            currentVelocity += _pm.Motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }
        else if (dotProduct < -_slideThreshold)
        {
            Debug.Log("Downwards Slide - 플레이어가 경사면을 아래로 슬라이딩 중");

            // Set Minimum Vertical Speed to the Jump Speed
            var currentVerticalSpeed = Vector3.Dot(currentVelocity, _pm.Motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, _slideJumpNormal);

            // Add the difference in current and target vertical speed to the character's velocity
            currentVelocity += 
                _pm.Motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed)
                + _pm.Motor.CharacterForward * _slideJumpForward;
        }
        else
        {
            Debug.Log("Upwards Slide - 플레이어가 평지 슬라이딩 중");
            // Set Minimum Vertical Speed to the Jump Speed
            var currentVerticalSpeed = Vector3.Dot(currentVelocity, _pm.Motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, _slideJumpNormal);

            // Add the difference in current and target vertical speed to the character's velocity
            currentVelocity += _pm.Motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }
    }
}
