	using System;
	using System.Diagnostics;
	using System.Linq;

	namespace Flaky
	{
	public static class PlatformDependant
	{
		public static void SetProcessPriority()
		{
			if(IsWindows)
				Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
		}

		public static IAudioDevice GetAudioDevice()
		{
			if (IsWindows)
				return new NAudioDevice();
			else
				return new OpenTKAudioDevice();
		}

		private static bool IsWindows
		{
			get
			{
				return Environment.OSVersion.Platform != PlatformID.MacOSX &&
					Environment.OSVersion.Platform != PlatformID.Unix;
			}
		}
	}
	}
