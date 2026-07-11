using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if !(CONSOLE)
using Avalonia;
#endif

namespace TConvert {
	/**<summary>The class defining the entry point to the program.</summary>*/
	static class Startup {
		//============= MAIN =============
		#region Main

		/**<summary>The entry point to the program.</summary>*/
		[STAThread]
		static void Main(string[] args) {
			#if !(CONSOLE)
			BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
			#else
			CommandLine.ParseCommand(args);
			#endif
		}

		/**<summary>Builds the Avalonia application.</summary>*/
		#if !(CONSOLE)
		static AppBuilder BuildAvaloniaApp() {
			return AppBuilder.Configure<App>()
				.UsePlatformDetect()
				.LogToTrace();
		}
		#endif

		#endregion
	}
}
