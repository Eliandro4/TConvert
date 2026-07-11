using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Timer = System.Timers.Timer;
using TConvert.Util;
using Avalonia.Interactivity;

namespace TConvert.Windows {
	/**<summary>The TConvert progress window.</summary>*/
	public partial class ProgressWindow : Window {
		//=========== MEMBERS ============
		#region Members

		/**<summary>The thread being run.</summary>*/
		private Thread thread;
		/**<summary>The start time of the operation.</summary>*/
		private DateTime startTime;
		/**<summary>The timer to update the time taken.</summary>*/
		private Timer timer;
		/**<summary>The action to call when canceled.</summary>*/
		private Action cancel;

		#endregion
		//========= CONSTRUCTORS =========
		#region Constructors

		/**<summary>Constructs the progress window.</summary>*/
		public ProgressWindow(Thread thread, Action cancel) {
			InitializeComponent();

			this.thread = thread;
			this.cancel = cancel;
			this.timer = new Timer();
			timer.Interval = 200;
			timer.AutoReset = true;
			timer.Elapsed += TimerEllapsed;
		}

		#endregion
		//=========== PROGRESS ===========
		#region Progress

		/**<summary>Updates the progress on the window.</summary>*/
		public void Update(string status, double progress) {
			labelStatus.Content = status;
			progressBar.Value = progress;
		}
		/**<summary>Tells the window the progress is finished.</summary>*/
		public void Finish(string status, bool error) {
			timer.Stop();
			labelStatus.Content = status;
			progressBar.Value = 1.0;
			labelTime.Content = "Total Time: " + (DateTime.Now - startTime).ToString(@"m\:ss");
			buttonFinish.IsEnabled = true;
			buttonCancel.IsVisible = false;
			buttonFinish.Margin = new Thickness(0, 0, 10, 10);
			buttonFinish.IsDefault = true;
			buttonFinish.Focus();
		}

		#endregion
		//============ EVENTS ============
		#region Events

		private void TimerEllapsed(object sender, ElapsedEventArgs e) {
			Dispatcher.UIThread.InvokeAsync(() => {
				labelTime.Content = "Time: " + (DateTime.Now - startTime).ToString(@"m\:ss");
			});
		}
		private void OnWindowLoaded(object sender, RoutedEventArgs e) {
			startTime = DateTime.Now;
			timer.Start();
			thread.Start();
		}
		private void OnClosing(object sender, WindowClosingEventArgs e) {
			if (thread != null && thread.ThreadState != ThreadState.Stopped) {
				try {
					thread.Abort();
				}
				catch { }
			}
		}
		private void OnCancel(object sender, RoutedEventArgs e) {
			if (thread != null && thread.ThreadState != ThreadState.Stopped) {
				try {
					thread.Abort();
				}
				catch { }
			}
			if (cancel != null)
				cancel();
			Close();
		}
		private void OnFinish(object sender, RoutedEventArgs e) {
			Close();
		}

		#endregion
	}
}
