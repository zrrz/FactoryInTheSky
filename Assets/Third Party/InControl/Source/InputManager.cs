namespace InControl
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using UnityEngine;

#if NETFX_CORE
	using System.Reflection;
#endif


	public class InputManager
	{
		public static readonly VersionInfo Version = VersionInfo.InControlVersion();

		public static event Action OnSetup;
		public static event Action<ulong, float> OnUpdate;
		public static event Action OnReset;

		public static event Action<InputDevice> OnDeviceAttached;
		public static event Action<InputDevice> OnDeviceDetached;
		public static event Action<InputDevice> OnActiveDeviceChanged;

		internal static event Action<ulong, float> OnUpdateDevices;
		internal static event Action<ulong, float> OnCommitDevices;

		static List<InputDeviceManager> deviceManagers = new List<InputDeviceManager>();
		static Dictionary<Type, InputDeviceManager> deviceManagerTable = new Dictionary<Type, InputDeviceManager>();

		static InputDevice activeDevice = InputDevice.Null;
		static List<InputDevice> devices = new List<InputDevice>();

		static List<PlayerActionSet> playerActionSets = new List<PlayerActionSet>();


		/// <summary>
		/// A readonly collection of devices.
		/// Not every device in this list is guaranteed to be attached or even a controller.
		/// This collection should be treated as a pool from which devices may be selected.
		/// The collection is in no particular order and the order may change at any time.
		/// Do not treat this collection as a list of players.
		/// </summary>
		public static ReadOnlyCollection<InputDevice> Devices;

		/// <summary>
		/// Query whether a command button was pressed on any device during the last frame of input.
		/// </summary>
		public static bool CommandWasPressed { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Y axis should be inverted for
		/// two-axis (directional) controls. When false (default), the Y axis will be positive up,
		/// the same as Unity.
		/// </summary>
		public static bool InvertYAxis { get; set; }

		/// <summary>
		/// Gets a value indicating whether the InputManager is currently setup and running.
		/// </summary>
		public static bool IsSetup { get; private set; }


		internal static string Platform { get; private set; }

		static bool applicationIsFocused;
		static float initialTime;
		static float currentTime;
		static float lastUpdateTime;
		static ulong currentTick;
		static VersionInfo? unityVersion;


		[Obsolete( "Use InputManager.CommandWasPressed instead." )]
		public static bool MenuWasPressed
		{
			get
			{
				return CommandWasPressed;
			}
		}


		/// <summary>
		/// DEPRECATED: Use the InControlManager component instead.
		/// </summary>
		/// @deprecated
		/// Calling this method directly is no longer supported. Use the InControlManager component to
		/// manage the lifecycle of the input manager instead.
		[Obsolete( "Calling InputManager.Setup() directly is no longer supported. Use the InControlManager component to manage the lifecycle of the input manager instead.", true )]
		public static void Setup()
		{
			SetupInternal();
		}


		internal static bool SetupInternal()
		{
			if (IsSetup)
			{
				return false;
			}

#if !NETFX_CORE && !UNITY_WEBPLAYER && !UNITY_EDITOR_OSX && (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
			Platform = Utility.GetWindowsVersion().ToUpper();
#else
			Platform = (SystemInfo.operatingSystem + " " + SystemInfo.deviceModel).ToUpper();
#endif

			enabled = true;

			initialTime = 0.0f;
			currentTime = 0.0f;
			lastUpdateTime = 0.0f;
			currentTick = 0;
			applicationIsFocused = true;

			deviceManagers.Clear();
			deviceManagerTable.Clear();
			devices.Clear();
			Devices = new ReadOnlyCollection<InputDevice>( devices );
			activeDevice = InputDevice.Null;

			playerActionSets.Clear();

			// TODO: Can this move further down along with the OnSetup callback?
			IsSetup = true;

			var enableUnityInput = true;

			var nativeInputIsEnabled = EnableNativeInput && NativeInputDeviceManager.Enable();
			if (nativeInputIsEnabled)
			{
				enableUnityInput = false;
			}

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
			if (EnableXInput && enableUnityInput)
			{
				XInputDeviceManager.Enable();
			}
#endif

#if UNITY_IOS
			if (EnableICade)
			{
				ICadeDeviceManager.Enable();
			}
#endif

#if UNITY_XBOXONE
			if (XboxOneInputDeviceManager.Enable())
			{
				enableUnityInput = false;
			}
#endif

#if UNITY_SWITCH
			if (NintendoSwitchInputDeviceManager.Enable())
			{
				enableUnityInput = false;
			}
#endif

			// TODO: Can this move further down after the UnityInputDeviceManager is added?
			// Currently, it allows use of InputManager.HideDevicesWithProfile()
			if (OnSetup != null)
			{
				OnSetup.Invoke();
				OnSetup = null;
			}

#if UNITY_ANDROID && INCONTROL_OUYA && !UNITY_EDITOR
			enableUnityInput = false;
#endif

			if (enableUnityInput)
			{
				AddDeviceManager<UnityInputDeviceManager>();
			}

			return true;
		}


		/// <summary>
		/// DEPRECATED: Use the InControlManager component instead.
		/// </summary>
		/// @deprecated
		/// Calling this method directly is no longer supported. Use the InControlManager component to
		/// manage the lifecycle of the input manager instead.
		[Obsolete( "Calling InputManager.Reset() method directly is no longer supported. Use the InControlManager component to manage the lifecycle of the input manager instead.", true )]
		public static void Reset()
		{
			ResetInternal();
		}

		internal static void ResetInternal()
		{
			if (OnReset != null)
			{
				OnReset.Invoke();
			}

			OnSetup = null;
			OnUpdate = null;
			OnReset = null;
			OnActiveDeviceChanged = null;
			OnDeviceAttached = null;
			OnDeviceDetached = null;
			OnUpdateDevices = null;
			OnCommitDevices = null;

			DestroyDeviceManagers();
			DestroyDevices();

			playerActionSets.Clear();

			IsSetup = false;
		}


		/// <summary>
		/// DEPRECATED: Use the InControlManager component instead.
		/// </summary>
		/// @deprecated
		/// Calling this method directly is no longer supported. Use the InControlManager component to
		/// manage the lifecycle of the input manager instead.
		[Obsolete( "Calling InputManager.Update() directly is no longer supported. Use the InControlManager component to manage the lifecycle of the input manager instead.", true )]
		public static void Update()
		{
			UpdateInternal();
		}

		internal static void UpdateInternal()
		{
			AssertIsSetup();
			if (OnSetup != null)
			{
				OnSetup.Invoke();
				OnSetup = null;
			}

			if (!enabled)
			{
				return;
			}

			if (SuspendInBackground && !applicationIsFocused)
			{
				return;
			}

			currentTick++;
			UpdateCurrentTime();
			var deltaTime = currentTime - lastUpdateTime;

			UpdateDeviceManagers( deltaTime );

			CommandWasPressed = false;
			UpdateDevices( deltaTime );
			CommitDevices( deltaTime );

			UpdateActiveDevice();

			UpdatePlayerActionSets( deltaTime );

			if (OnUpdate != null)
			{
				OnUpdate.Invoke( currentTick, deltaTime );
			}

			lastUpdateTime = currentTime;
		}


		/// <summary>
		/// Force the input manager to reset and setup.
		/// </summary>
		public static void Reload()
		{
			ResetInternal();
			SetupInternal();
		}


		static void AssertIsSetup()
		{
			if (!IsSetup)
			{
				throw new Exception( "InputManager is not initialized. Call InputManager.Setup() first." );
			}
		}


		static void SetZeroTickOnAllControls()
		{
			var deviceCount = devices.Count;
			for (var i = 0; i < deviceCount; i++)
			{
				var controls = devices[i].Controls;
				var controlCount = controls.Count;
				for (var j = 0; j < controlCount; j++)
				{
					var control = controls[j];
					if (control != null)
					{
						control.SetZeroTick();
					}
				}
			}
		}


		/// <summary>
		/// Clears the state of input on all controls.
		/// The net result here should be that the state on all controls will return
		/// zero/false for the remainder of the current tick, and during the next update
		/// tick WasPressed, WasReleased, WasRepeated and HasChanged will return false.
		/// </summary>
		public static void ClearInputState()
		{
			var deviceCount = devices.Count;
			for (var i = 0; i < deviceCount; i++)
			{
				devices[i].ClearInputState();
			}

			var playerActionSetCount = playerActionSets.Count;
			for (var i = 0; i < playerActionSetCount; i++)
			{
				playerActionSets[i].ClearInputState();
			}

			activeDevice = InputDevice.Null;
		}


		internal static void OnApplicationFocus( bool focusState )
		{
			if (!focusState)
			{
				if (SuspendInBackground)
				{
					ClearInputState();
				}

				SetZeroTickOnAllControls();
			}

			applicationIsFocused = focusState;
		}


		internal static void OnApplicationPause( bool pauseState )
		{
		}


		internal static void OnApplicationQuit()
		{
			ResetInternal();
		}


		internal static void OnLevelWasLoaded()
		{
			SetZeroTickOnAllControls();
			UpdateInternal();
		}


		/// <summary>
		/// Adds a device manager.
		/// Only one instance of a given type can be added. An error will be raised if
		/// you try to add more than one.
		/// </summary>
		/// <param name="deviceManager">The device manager to add.</param>
		public static void AddDeviceManager( InputDeviceManager deviceManager )
		{
			AssertIsSetup();

			var type = deviceManager.GetType();

			if (deviceManagerTable.ContainsKey( type ))
			{
				Logger.LogError( "A device manager of type '" + type.Name + "' already exists; cannot add another." );
				return;
			}

			deviceManagers.Add( deviceManager );
			deviceManagerTable.Add( type, deviceManager );

			deviceManager.Update( currentTick, currentTime - lastUpdateTime );
		}


		/// <summary>
		/// Adds a device manager by type.
		/// </summary>
		/// <typeparam name="T">A subclass of InputDeviceManager.</typeparam>
		public static void AddDeviceManager<T>() where T : InputDeviceManager, new()
		{
			AddDeviceManager( new T() );
		}


		/// <summary>
		/// Get a device manager from the input manager by type if it one is present.
		/// </summary>
		/// <typeparam name="T">A subclass of InputDeviceManager.</typeparam>
		public static T GetDeviceManager<T>() where T : InputDeviceManager
		{
			InputDeviceManager deviceManager;
			if (deviceManagerTable.TryGetValue( typeof( T ), out deviceManager ))
			{
				return deviceManager as T;
			}

			return null;
		}


		/// <summary>
		/// Query whether a device manager is present by type.
		/// </summary>
		/// <typeparam name="T">A subclass of InputDeviceManager.</typeparam>
		public static bool HasDeviceManager<T>() where T : InputDeviceManager
		{
			return deviceManagerTable.ContainsKey( typeof( T ) );
		}


		static void UpdateCurrentTime()
		{
			// Have to do this hack since Time.realtimeSinceStartup is not set until AFTER Awake().
			if (initialTime < float.Epsilon)
			{
				initialTime = Time.realtimeSinceStartup;
			}

			currentTime = Mathf.Max( 0.0f, Time.realtimeSinceStartup - initialTime );
		}


		static void UpdateDeviceManagers( float deltaTime )
		{
			var inputDeviceManagerCount = deviceManagers.Count;
			for (var i = 0; i < inputDeviceManagerCount; i++)
			{
				deviceManagers[i].Update( currentTick, deltaTime );
			}
		}


		static void DestroyDeviceManagers()
		{
			var deviceManagerCount = deviceManagers.Count;
			for (var i = 0; i < deviceManagerCount; i++)
			{
				deviceManagers[i].Destroy();
			}

			deviceManagers.Clear();
			deviceManagerTable.Clear();
		}


		static void DestroyDevices()
		{
			var deviceCount = devices.Count;
			for (var i = 0; i < deviceCount; i++)
			{
				var device = devices[i];
				device.OnDetached();
			}
			devices.Clear();
			activeDevice = InputDevice.Null;
		}


		static void UpdateDevices( float deltaTime )
		{
			var deviceCount = devices.Count;
			for (var i = 0; i < deviceCount; i++)
			{
				var device = devices[i];
				device.Update( currentTick, deltaTime );
			}

			if (OnUpdateDevices != null)
			{
				OnUpdateDevices.Invoke( currentTick, deltaTime );
			}
		}


		static void CommitDevices( float deltaTime )
		{
			var deviceCount = devices.Count;
			for (var i = 0; i < deviceCount; i++)
			{
				var device = devices[i];
				device.Commit( currentTick, deltaTime );

				if (device.CommandWasPressed)
				{
					CommandWasPressed = true;
				}
			}

			if (OnCommitDevices != null)
			{
				OnCommitDevices.Invoke( currentTick, deltaTime );
			}
		}


		static void UpdateActiveDevice()
		{
			var lastActiveDevice = ActiveDevice;

			var deviceCount = devices.Count;
			for (var i = 0; i < deviceCount; i++)
			{
				var inputDevice = devices[i];
				if (inputDevice.LastChangedAfter( ActiveDevice ) && !inputDevice.Passive)
				{
					ActiveDevice = inputDevice;
				}
			}

			if (lastActiveDevice != ActiveDevice)
			{
				if (OnActiveDeviceChanged != null)
				{
					OnActiveDeviceChanged( ActiveDevice );
				}
			}
		}


		/// <summary>
		/// Attach a device to the input manager.
		/// </summary>
		/// <param name="inputDevice">The input device to attach.</param>
		public static void AttachDevice( InputDevice inputDevice )
		{
			AssertIsSetup();

			if (!inputDevice.IsSupportedOnThisPlatform)
			{
				return;
			}

			if (inputDevice.IsAttached)
			{
				return;
			}

			if (!devices.Contains( inputDevice ))
			{
				devices.Add( inputDevice );
				devices.Sort( ( d1, d2 ) => d1.SortOrder.CompareTo( d2.SortOrder ) );
			}

			inputDevice.OnAttached();

			if (OnDeviceAttached != null)
			{
				OnDeviceAttached( inputDevice );
			}
		}


		/// <summary>
		/// Detach a device from the input manager.
		/// </summary>
		/// <param name="inputDevice">The input device to attach.</param>
		public static void DetachDevice( InputDevice inputDevice )
		{
			if (!IsSetup)
			{
				return;
			}

			if (!inputDevice.IsAttached)
			{
				return;
			}

			devices.Remove( inputDevice );

			if (ActiveDevice == inputDevice)
			{
				ActiveDevice = InputDevice.Null;
			}

			inputDevice.OnDetached();

			if (OnDeviceDetached != null)
			{
				OnDeviceDetached( inputDevice );
			}
		}


		/// <summary>
		/// Hides the devices with a given profile.
		/// This must be called before the input manager is initialized.
		/// </summary>
		/// <param name="type">Type.</param>
		public static void HideDevicesWithProfile( Type type )
		{
#if NETFX_CORE
			if (type.GetTypeInfo().IsAssignableFrom( typeof( UnityInputDeviceProfile ).GetTypeInfo() ))
#else
			if (type.IsSubclassOf( typeof( UnityInputDeviceProfile ) ))
#endif
			{
				UnityInputDeviceProfile.Hide( type );
			}
		}


		internal static void AttachPlayerActionSet( PlayerActionSet playerActionSet )
		{
			if (!playerActionSets.Contains( playerActionSet ))
			{
				playerActionSets.Add( playerActionSet );
			}
		}


		internal static void DetachPlayerActionSet( PlayerActionSet playerActionSet )
		{
			playerActionSets.Remove( playerActionSet );
		}


		internal static void UpdatePlayerActionSets( float deltaTime )
		{
			var playerActionSetCount = playerActionSets.Count;
			for (var i = 0; i < playerActionSetCount; i++)
			{
				playerActionSets[i].Update( currentTick, deltaTime );
			}
		}


		/// <summary>
		/// Detects whether any (keyboard) key is currently pressed.
		/// For more flexibility, see <see cref="KeyCombo.Detect()"/>
		/// </summary>
		public static bool AnyKeyIsPressed
		{
			get
			{
				return KeyCombo.Detect( true ).IncludeCount > 0;
			}
		}


		/// <summary>
		/// Gets the currently active device if present, otherwise returns a null device which does nothing.
		/// The currently active device is defined as the last device that provided input events. This is
		/// a good way to query for a device in single player applications.
		/// </summary>
		public static InputDevice ActiveDevice
		{
			get
			{
				return (activeDevice == null) ? InputDevice.Null : activeDevice;
			}

			private set
			{
				activeDevice = (value == null) ? InputDevice.Null : value;
			}
		}


		/// <summary>
		/// Toggle whether input is processed or not. While disabled, all controls will return zero state.
		/// </summary>
		public static bool Enabled
		{
			get
			{
				return enabled;
			}

			set
			{
				if (enabled != value)
				{
					if (value)
					{
						SetZeroTickOnAllControls();
						UpdateInternal();
					}
					else
					{
						ClearInputState();
						SetZeroTickOnAllControls();
					}

					enabled = value;
				}
			}
		}
		static bool enabled;


		/// <summary>
		/// Suspend input updates when the application loses focus.
		/// When enabled and the app loses focus, input will be cleared and no.
		/// input updates will be processed. Input updates will resume when the app 
		/// regains focus.
		/// </summary>
		public static bool SuspendInBackground { get; internal set; }


		/// <summary>
		/// Enable Native Input support.
		/// When enabled on initialization, the input manager will first check
		/// whether Native Input is supported on this platform and if so, it will add
		/// a NativeInputDeviceManager.
		/// </summary>
		public static bool EnableNativeInput { get; internal set; }


		/// <summary>
		/// Enable XInput support (Windows only).
		/// When enabled on initialization, the input manager will first check
		/// whether XInput is supported on this platform and if so, it will add
		/// an XInputDeviceManager.
		/// </summary>
		public static bool EnableXInput { get; internal set; }


		/// <summary>
		/// Set the XInput background thread polling rate.
		/// When set to zero (default) it will equal the projects fixed updated rate.
		/// </summary>
		public static uint XInputUpdateRate { get; internal set; }


		/// <summary>
		/// Set the XInput buffer size. (Experimental)
		/// Usually you want this to be zero (default). Setting it higher will introduce
		/// latency, but may smooth out input if querying input on FixedUpdate, which
		/// tends to cluster calls at the end of a frame.
		/// </summary>
		public static uint XInputBufferSize { get; internal set; }


		/// <summary>
		/// Set Native Input on Windows to use XInput.
		/// When set to true (default), XInput will be utilized which better supports
		/// compatible controllers (such as Xbox 360 and Xbox One gamepads) including
		/// vibration control and proper separated triggers, but limits the number of 
		/// these controllers to four. Additional XInput-compatible beyond four 
		/// controllers will be ignored.
		/// DirectInput will be used for all non-XInput-compatible controllers.
		/// </summary>
		public static bool NativeInputEnableXInput { get; internal set; }


		/// <summary>
		/// Set Native Input to prevent system sleep and screensaver.
		/// Controller input generally does not prevent the system idle timer and
		/// the screensaver may come on during extended gameplay. When set to
		/// true, this will be prevented.
		/// </summary>
		public static bool NativeInputPreventSleep { get; internal set; }


		/// <summary>
		/// Set the Native Input background thread polling rate.
		/// When set to zero (default) it will equal the project's fixed update rate.
		/// </summary>
		public static uint NativeInputUpdateRate { get; internal set; }


		/// <summary>
		/// Enable iCade support (iOS only).
		/// When enabled on initialization, the input manager will first check
		/// whether XInput is supported on this platform and if so, it will add
		/// an XInputDeviceManager.
		/// </summary>
		public static bool EnableICade { get; internal set; }


		internal static VersionInfo UnityVersion
		{
			get
			{
				if (!unityVersion.HasValue)
				{
					unityVersion = VersionInfo.UnityVersion();
				}

				return unityVersion.Value;
			}
		}


		internal static ulong CurrentTick
		{
			get
			{
				return currentTick;
			}
		}
	}
}
