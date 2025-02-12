namespace InControl
{
	// @cond nodoc
	[AutoDiscover]
	public class PlayStation4MacUSBProfile : UnityInputDeviceProfile
	{
		public PlayStation4MacUSBProfile()
		{
			Name = "PlayStation 4 Controller";
			Meta = "PlayStation 4 Controller on Mac";

			DeviceClass = InputDeviceClass.Controller;
			DeviceStyle = InputDeviceStyle.PlayStation4;

			IncludePlatforms = new[] {
				"OS X"
			};

			JoystickNames = new[] {
				"Sony Computer Entertainment Wireless Controller",
				"Sony Interactive Entertainment Wireless Controller"
			};

			ButtonMappings = new[] {
				new InputControlMapping {
					Handle = "Cross",
					Target = InputControlType.Action1,
					Source = Button1
				},
				new InputControlMapping {
					Handle = "Circle",
					Target = InputControlType.Action2,
					Source = Button2
				},
				new InputControlMapping {
					Handle = "Square",
					Target = InputControlType.Action3,
					Source = Button0
				},
				new InputControlMapping {
					Handle = "Triangle",
					Target = InputControlType.Action4,
					Source = Button3
				},
				new InputControlMapping {
					Handle = "Left Bumper",
					Target = InputControlType.LeftBumper,
					Source = Button4
				},
				new InputControlMapping {
					Handle = "Right Bumper",
					Target = InputControlType.RightBumper,
					Source = Button5
				},
				new InputControlMapping {
					Handle = "Share",
					Target = InputControlType.Share,
					Source = Button8
				},
				new InputControlMapping {
					Handle = "Options",
					Target = InputControlType.Options,
					Source = Button9
				},
				new InputControlMapping {
					Handle = "Left Stick Button",
					Target = InputControlType.LeftStickButton,
					Source = Button10
				},
				new InputControlMapping {
					Handle = "Right Stick Button",
					Target = InputControlType.RightStickButton,
					Source = Button11
				},
				new InputControlMapping {
					Handle = "System",
					Target = InputControlType.System,
					Source = Button12
				},
				new InputControlMapping {
					Handle = "TouchPad Button",
					Target = InputControlType.TouchPadButton,
					Source = Button13
				}
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

				LeftTriggerMapping( Analog4 ),
				RightTriggerMapping( Analog5 ),

				DPadLeftMapping( Analog6 ),
				DPadRightMapping( Analog6 ),
				DPadUpMapping( Analog7 ),
				DPadDownMapping( Analog7 ),
			};
		}
	}
	// @endcond
}

