# VR Controller and Hand Input (OpenXR + XR Interaction Toolkit)

## Table of Contents
1. Action-Based Controller Setup
2. Reading Controller Input
3. XR Interaction Toolkit Interactors
4. Hand Tracking
5. Haptic Feedback

---

## 1. Action-Based Controller Setup

XR Interaction Toolkit uses the **Action-based** controller model. Input Actions are defined in an Input Actions Asset (`.inputactions` file) and referenced via `InputActionReference`.

The XR Interaction Toolkit Starter Assets sample includes a pre-configured `XRI Default Input Actions` asset. Import it:
- Package Manager > XR Interaction Toolkit > Samples > Starter Assets > Import.

This provides standard bindings for move, turn, grab, activate, UI interact, etc.

## 2. Reading Controller Input

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyGame.VR
{
    public class VRInputReader : MonoBehaviour
    {
        [Header("Input Action References")]
        [SerializeField] private InputActionReference _triggerAction;
        [SerializeField] private InputActionReference _gripAction;
        [SerializeField] private InputActionReference _primaryButtonAction;
        [SerializeField] private InputActionReference _thumbstickAction;

        private void OnEnable()
        {
            _triggerAction.action.Enable();
            _gripAction.action.Enable();
            _primaryButtonAction.action.Enable();
            _thumbstickAction.action.Enable();

            _primaryButtonAction.action.performed += OnPrimaryButton;
        }

        private void OnDisable()
        {
            _primaryButtonAction.action.performed -= OnPrimaryButton;

            _triggerAction.action.Disable();
            _gripAction.action.Disable();
            _primaryButtonAction.action.Disable();
            _thumbstickAction.action.Disable();
        }

        private void Update()
        {
            float trigger = _triggerAction.action.ReadValue<float>();
            float grip = _gripAction.action.ReadValue<float>();
            Vector2 thumbstick = _thumbstickAction.action.ReadValue<Vector2>();
        }

        private void OnPrimaryButton(InputAction.CallbackContext ctx)
        {
            // A/X button pressed
        }
    }
}
```

### Oculus Touch Controller Mapping

| Physical Button | OpenXR Path |
|----------------|-------------|
| Trigger (index) | `{LeftHand}/trigger` or `{RightHand}/trigger` |
| Grip (middle) | `{LeftHand}/squeeze` or `{RightHand}/squeeze` |
| Thumbstick | `{LeftHand}/thumbstick` or `{RightHand}/thumbstick` |
| A / X button | `{RightHand}/primaryButton` / `{LeftHand}/primaryButton` |
| B / Y button | `{RightHand}/secondaryButton` / `{LeftHand}/secondaryButton` |
| Menu button | `{LeftHand}/menu` |

## 3. XR Interaction Toolkit Interactors

### Direct Interactor
For grabbing objects within arm's reach. Attach to controller GameObjects.

```csharp
// On the interactable object:
[RequireComponent(typeof(XRGrabInteractable))]
public class GrabbableItem : MonoBehaviour
{
    // XRGrabInteractable handles grab/release via XR Interaction Toolkit
}
```

### Ray Interactor
For pointing at distant objects or UI. Attach to controller GameObjects.

### Poke Interactor
For pressing UI buttons with finger tips. Useful for spatial UI panels.

## 4. Hand Tracking

Requirements:
- `com.unity.xr.hands` package installed.
- Meta Quest Hand Tracking feature enabled in OpenXR feature group.

```csharp
using UnityEngine;
using UnityEngine.XR.Hands;

namespace MyGame.VR
{
    public class HandTrackingReader : MonoBehaviour
    {
        private XRHandSubsystem _handSubsystem;

        private void Start()
        {
            var subsystems = new System.Collections.Generic.List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            if (subsystems.Count > 0)
                _handSubsystem = subsystems[0];
        }

        private void Update()
        {
            if (_handSubsystem == null) return;

            XRHand leftHand = _handSubsystem.leftHand;
            if (leftHand.isTracked)
            {
                XRHandJoint indexTip = leftHand.GetJoint(XRHandJointID.IndexTip);
                if (indexTip.TryGetPose(out Pose pose))
                {
                    // Use pose.position, pose.rotation
                }
            }
        }
    }
}
```

## 5. Haptic Feedback

```csharp
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MyGame.VR
{
    public class HapticHelper : MonoBehaviour
    {
        /// <summary>
        /// Send a haptic pulse to a controller.
        /// </summary>
        /// <param name="controller">The XR controller to vibrate.</param>
        /// <param name="amplitude">0-1 intensity.</param>
        /// <param name="duration">Duration in seconds.</param>
        public static void SendPulse(
            ActionBasedController controller, float amplitude, float duration)
        {
            controller.SendHapticImpulse(amplitude, duration);
        }
    }
}
```

For rhythm games, use short, precise pulses (0.05-0.1s) on note hits to give tactile feedback.
