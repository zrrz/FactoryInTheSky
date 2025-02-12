﻿namespace InControl
{
	// @cond nodoc
	[AutoDiscover]
	public class AppleTVWirelessControllerProfile : UnityInputDeviceProfile
	{
		// Naming of this file/class is important. It needs to come after 
		// the remote profile alphabetically.
		//
		// Also take note of these docs:
		// https://docs.unity3d.com/Manual/tvOS.html
		// https://docs.unity3d.com/ScriptReference/Apple.TV.Remote.html
		// Specifically, the UnityEngine.Apple.TV.Remote.allowExitToHome flag.
		//
		public AppleTVWirelessControllerProfile()
		{
			Name = "Apple TV Controller";
			Meta = "Apple TV Controller on tvOS";

			DeviceClass = InputDeviceClass.Controller;
			DeviceStyle = InputDeviceStyle.AppleMFi;

			IncludePlatforms = new[] {
				"AppleTV"
			};

			JoystickRegex = new[] {
				"controller",
				"basic",
				"extended"
			};

			LowerDeadZone = 0.05f;
			UpperDeadZone = 0.95f;

			ButtonMappings = new[] {
				new InputControlMapping {
					Handle = "A",
					Target = InputControlType.Action1,
					Source = Button14
				},
				new InputControlMapping {
					Handle = "B",
					Target = InputControlType.Action2,
					Source = Button13
				},
				new InputControlMapping {
					Handle = "X",
					Target = InputControlType.Action3,
					Source = Button15
				},
				new InputControlMapping {
					Handle = "Y",
					Target = InputControlType.Action4,
					Source = Button12
				},
				new InputControlMapping {
					Handle = "DPad Up",
					Target = InputControlType.DPadUp,
					Source = Button4
				},
				new InputControlMapping {
					Handle = "DPad Down",
					Target = InputControlType.DPadDown,
					Source = Button6
				},
				new InputControlMapping {
					Handle = "DPad Left",
					Target = InputControlType.DPadLeft,
					Source = Button7
				},
				new InputControlMapping {
					Handle = "DPad Right",
					Target = InputControlType.DPadRight,
					Source = Button5
				},
				new InputControlMapping {
					Handle = "Left Bumper",
					Target = InputControlType.LeftBumper,
					Source = Button8
				},
				new InputControlMapping {
					Handle = "Right Bumper",
					Target = InputControlType.RightBumper,
					Source = Button9
				},
				new InputControlMapping {
					Handle = "Menu",
					Target = InputControlType.Menu,
					Source = Button0
				},
			};

			AnalogMappings = new[] {
				LeftStickLeftMapping( Analog0 ),
				LeftStickRightMapping( Analog0 ),
				LeftStickUpMapping( Analog1 ),
				LeftStickDownMapping( Analog1 ),

				RightStickLeftMapping( Analog2 ),
				RightStickRightMapping( Analog2 ),
				RightStickUpMapping( Analog3 ),
				RightStickDownMapping( Analog3 ),

				new InputControlMapping {
					Handle = "Left Trigger",
					Target = InputControlType.LeftTrigger,
					Source = Analog10
				},
				new InputControlMapping {
					Handle = "Right Trigger",
					Target = InputControlType.RightTrigger,
					Source = Analog11
				}
			};
		}
	}
	// @endcond
}

