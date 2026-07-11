using System;
using System.Runtime.InteropServices;

namespace TConvert.Util {
	/**<summary>A cross-platform replacement for System.Media.SystemSounds, which is
	 * not available on .NET (Core). Plays a best-effort audible cue.</summary>*/
	public static class Sound {
		//=========== SOUNDS ============
		#region Sounds

		/**<summary>Plays an asterisk (info) sound.</summary>*/
		public static void Asterisk() {
			Beep();
		}
		/**<summary>Plays an exclamation (warning) sound.</summary>*/
		public static void Exclamation() {
			Beep();
		}
		/**<summary>Plays a hand (error) sound.</summary>*/
		public static void Hand() {
			Beep();
		}
		/**<summary>Plays a question sound.</summary>*/
		public static void Question() {
			Beep();
		}

		#endregion
		//============ HELPER ============
		#region Helper

		/**<summary>Plays a simple audible cue. On Windows this uses the console
		 * beep; on other platforms it is currently a no-op.</summary>*/
		private static void Beep() {
			try {
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					Console.Beep();
			}
			catch { }
		}

		#endregion
	}
}
