using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using TConvert.Extract;
using TConvert.Windows;

namespace TConvert {
	/**<summary>The application class.</summary>*/
	public partial class App : Application {
		//=========== MEMBERS ============
		#region Members

		/**<summary>The last exception. Used to prevent multiple error windows for the same error.</summary>*/
		private static object lastException = null;

		#endregion
		//========= CONSTRUCTORS =========
		#region Constructors

		/**<summary>Constructs the app and sets up exception handling.</summary>*/
		public App() {
			// Catch exceptions not in a UI thread
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnAppDomainUnhandledException);
			TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;
			Dispatcher.UIThread.UnhandledException += OnAppUnhandledException;
		}

		#endregion
		//========== LIFECYCLE ===========
		#region Lifecycle

		/**<summary>Initializes the application (loads the XAML).</summary>*/
		public override void Initialize() {
			AvaloniaXamlLoader.Load(this);
		}
		/**<summary>Called once the application framework is initialized.</summary>*/
		public override void OnFrameworkInitializationCompleted() {
			string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();
			if (args.Length > 0) {
				// Only reach here from CommandLine starting up the app to use the progress window.
				CommandLine.ParseCommand(args);
			}
			else {
				MainWindow mainWindow = new MainWindow();
				Processing.MainWindow = mainWindow;
				mainWindow.Show();
			}
			base.OnFrameworkInitializationCompleted();
		}

		#endregion
		//============ EVENTS ============
		#region Events

		private async void OnAppUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
			if (e.Exception != lastException) {
				lastException = e.Exception;
				if (await ErrorMessageBox.Show(e.Exception))
					Environment.Exit(0);
				e.Handled = true;
			}
		}
		private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) {
			if (e.ExceptionObject != lastException) {
				lastException = e.ExceptionObject;
				Dispatcher.UIThread.InvokeAsync(async () => {
					if (await ErrorMessageBox.Show(e.ExceptionObject))
						Environment.Exit(0);
				});
			}
		}
		private void OnTaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) {
			if (e.Exception != lastException) {
				lastException = e.Exception;
				Dispatcher.UIThread.InvokeAsync(async () => {
					if (await ErrorMessageBox.Show(e.Exception))
						Environment.Exit(0);
				});
			}
		}

		#endregion
	}
}
