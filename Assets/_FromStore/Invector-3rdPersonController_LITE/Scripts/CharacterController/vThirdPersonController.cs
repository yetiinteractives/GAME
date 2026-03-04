using UnityEngine;

namespace Invector.vCharacterController
{
    public class vThirdPersonController : vThirdPersonAnimator
    {
        // Add this to track if we're holding strafe
        private bool isStrafeHeld = false;

        public virtual void ControlAnimatorRootMotion()
        {
            if (!this.enabled) return;

            if (inputSmooth == Vector3.zero)
            {
                transform.position = animator.rootPosition;
                transform.rotation = animator.rootRotation;
            }

            if (useRootMotion)
                MoveCharacter(moveDirection);
        }

        public virtual void ControlLocomotionType()
        {
            if (lockMovement) return;

            // Check if we should be strafing (either by hold or by toggle)
            bool shouldStrafe = isStrafeHeld ||
                               (locomotionType.Equals(LocomotionType.OnlyStrafe)) ||
                               (locomotionType.Equals(LocomotionType.FreeWithStrafe) && isStrafing);

            if ((locomotionType.Equals(LocomotionType.FreeWithStrafe) && !shouldStrafe) || locomotionType.Equals(LocomotionType.OnlyFree))
            {
                SetControllerMoveSpeed(freeSpeed);
                SetAnimatorMoveSpeed(freeSpeed);
                // Reset legacy strafe state when not strafing
                if (locomotionType.Equals(LocomotionType.FreeWithStrafe))
                    isStrafing = false;
            }
            else if (shouldStrafe)
            {
                // Set legacy strafe state for compatibility
                if (locomotionType.Equals(LocomotionType.FreeWithStrafe))
                    isStrafing = true;
                SetControllerMoveSpeed(strafeSpeed);
                SetAnimatorMoveSpeed(strafeSpeed);
            }

            if (!useRootMotion)
                MoveCharacter(moveDirection);
        }

        public virtual void ControlRotationType()
        {
            if (lockRotation) return;

            bool strafingForRotation = isStrafeHeld ||
                                      (locomotionType.Equals(LocomotionType.OnlyStrafe)) ||
                                      (locomotionType.Equals(LocomotionType.FreeWithStrafe) && isStrafing);

            bool validInput = input != Vector3.zero || (strafingForRotation ? strafeSpeed.rotateWithCamera : freeSpeed.rotateWithCamera);

            if (validInput)
            {
                inputSmooth = Vector3.Lerp(inputSmooth, input, (strafingForRotation ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);

                Vector3 dir = (strafingForRotation && (!isSprinting || sprintOnlyFree == false) || (freeSpeed.rotateWithCamera && input == Vector3.zero)) && rotateTarget ? rotateTarget.forward : moveDirection;
                RotateToDirection(dir);
            }
        }

        public virtual void UpdateMoveDirection(Transform referenceTransform = null)
        {
            if (input.magnitude <= 0.01)
            {
                moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, ((isStrafeHeld || isStrafing) ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
                return;
            }

            if (referenceTransform && !rotateByWorld)
            {
                var right = referenceTransform.right;
                right.y = 0;
                var forward = Quaternion.AngleAxis(-90, Vector3.up) * right;
                moveDirection = (inputSmooth.x * right) + (inputSmooth.z * forward);
            }
            else
            {
                moveDirection = new Vector3(inputSmooth.x, 0, inputSmooth.z);
            }
        }

        public virtual void Sprint(bool value)
        {
            var sprintConditions = (input.sqrMagnitude > 0.1f && isGrounded &&
                !((isStrafeHeld || isStrafing) && !strafeSpeed.walkByDefault && (horizontalSpeed >= 0.5 || horizontalSpeed <= -0.5 || verticalSpeed <= 0.1f)));

            if (value && sprintConditions)
            {
                if (input.sqrMagnitude > 0.1f)
                {
                    if (isGrounded && useContinuousSprint)
                    {
                        isSprinting = !isSprinting;
                    }
                    else if (!isSprinting)
                    {
                        isSprinting = true;
                    }
                }
                else if (!useContinuousSprint && isSprinting)
                {
                    isSprinting = false;
                }
            }
            else if (isSprinting)
            {
                isSprinting = false;
            }
        }

        // KEEP the old Strafe() method for backward compatibility
        // This is called by vThirdPersonInput.cs
        public virtual void Strafe()
        {
            // Don't toggle anymore - just set based on hold
            // The actual hold logic is handled by UpdateStrafeInput()
            // We need to keep this method signature for compatibility
        }

        // NEW method to update strafe hold state
        public virtual void UpdateStrafeInput(bool isHeld)
        {
            isStrafeHeld = isHeld;

            // For FreeWithStrafe locomotion type, update the isStrafing variable
            if (locomotionType.Equals(LocomotionType.FreeWithStrafe))
            {
                isStrafing = isHeld;
            }
        }

        public virtual void Jump()
        {
            jumpCounter = jumpTimer;
            isJumping = true;

            if (input.sqrMagnitude < 0.1f)
                animator.CrossFadeInFixedTime("Jump", 0.1f);
            else
                animator.CrossFadeInFixedTime("JumpMove", .2f);
        }
    }
}