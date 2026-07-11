using System;
using System.Diagnostics;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using TConvert.Util;

namespace TConvert.Windows {
	/**<summary>Shows an error that occured in the program.</summary>*/
	public partial class ErrorMessageBox : Window {
		//=========== MEMBERS ============
		#region Members

		/**<summary>The exception that was raised.</summary>*/
		private Exception exception = null;
		/**<summary>The non-exception object that was raised.</summary>*/
		private object exceptionObject = null;
		/**<summary>True if viewing the full exception.</summary>*/
		private bool viewingFull = false;
		/**<summary>The timer for changing the copy button back to its original text.</summary>*/
		private Timer copyTimer = new Timer(1000);
		/**<summary>The text of the copy to clipboard button.</summary>*/
		private readonly string copyText;

		#endregion
		//========= CONSTRUCTORS =========
		#region Constructors

		/**<summary>Constructs the error message box with an exception.</summary>*/
		public ErrorMessageBox(Exception exception, bool alwaysContinue) {
			InitializeComponent();

			this.textBlockMessage.Text = "Exception:\n" + exception.Message;
			this.exception = exception;
			this.copyTimer.Elapsed += OnCopyTimer;
			this.copyTimer.AutoReset = false;
			this.copyText = buttonCopy.Content as string;
			if (alwaysContinue) {
				this.buttonExit.IsVisible = false;
				this.buttonContinue.IsDefault = true;
			}
		}
		/**<summary>Constructs the error message box with an exception object.</summary>*/
		public ErrorMessageBox(object exceptionObject, bool alwaysContinue) {
			InitializeComponent();

			this.textBlockMessage.Text = "Exception:\n" + (exceptionObject is Exception ? (exceptionObject as Exception).Message : exceptionObject.ToString());
			this.exception = (exceptionObject is Exception ? exceptionObject as Exception : null);
			this.exceptionObject = (exceptionObject is Exception ? null : exceptionObject);
			this.copyTimer.Elapsed += OnCopyTimer;
			this.copyTimer.AutoReset = false;
			this.copyText = buttonCopy.Content as string;
			if (!(exceptionObject is Exception)) {
				this.buttonException.IsEnabled = false;
			}
			if (alwaysContinue) {
				this.buttonExit.IsVisible = false;
				this.buttonContinue.IsDefault = true;
			}
		}


		#endregion
		//============ EVENTS ============
		#region Events

		private void OnWindowClosing(object sender, WindowClosingEventArgs e) {
			copyTimer.Stop();
		}
		private void OnExit(object sender, RoutedEventArgs e) {
			Close(true);
		}
		private void OnCopyTimer(object sender, ElapsedEventArgs e) {
			Dispatcher.UIThread.InvokeAsync(() => {
				buttonCopy.Content = copyText;
			});
		}
		private async void OnCopyToClipboard(object sender, RoutedEventArgs e) {
			await Clipboard.SetTextAsync(exception != null ? exception.ToString() : exceptionObject.ToString());
			buttonCopy.Content = "Exception Copied!";
			copyTimer.Stop();
			copyTimer.Start();
		}
		private void OnSeeFullException(object sender, RoutedEventArgs e) {
			viewingFull = !viewingFull;
			if (!viewingFull) {
				buttonException.Content = "See Full Exception";
				textBlockMessage.Text = "Exception:\n" + exception.Message;
				clientArea.Height = 230;
				scrollViewer.ScrollToHome();
			}
			else {
				buttonException.Content = "Hide Full Exception";
				textBlockMessage.Text = "Exception:\n" + exception.ToString();
				clientArea.Height = Math.Min(480, Math.Max(230, textBlockMessage.Bounds.Height + 102));
				scrollViewer.ScrollToHome();
			}
		}
		private void OnMessageSizeChanged(object sender, SizeChangedEventArgs e) {
			if (viewingFull) {
				clientArea.Height = Math.Min(480, Math.Max(230, textBlockMessage.Bounds.Height + 102));
				scrollViewer.ScrollToHome();
			}
		}
		private void OnPreviewKeyDown(object sender, KeyEventArgs e) {
			var focused = GetFocusedButton();
			switch (e.Key) {
			case Key.Right:
				if (focused == buttonContinue && buttonExit.IsVisible)
					buttonExit.Focus();
				else if (focused == buttonCopy)
					buttonContinue.Focus();
				else if (focused == buttonException)
					buttonCopy.Focus();
				e.Handled = true;
				break;
			case Key.Left:
				if (focused == null) {
					if (buttonExit.IsVisible)
						buttonContinue.Focus();
					else
						buttonCopy.Focus();
				}
				else if (focused == buttonExit)
					buttonContinue.Focus();
				else if (focused == buttonContinue)
					buttonCopy.Focus();
				else if (focused == buttonCopy && buttonException.IsEnabled)
					buttonException.Focus();
				e.Handled = true;
				break;
			}
		}

		#endregion
		//=========== SHOWING ============
		#region Showing

		/**<summary>Shows an error message box with an exception.</summary>*/
		public static async Task<bool> Show(Exception exception, bool alwaysContinue = false) {
			ErrorMessageBox messageBox = new ErrorMessageBox(exception, alwaysContinue);
			return await messageBox.ShowDialog<bool>(null);
		}
		/**<summary>Shows an error message box with an exception object.</summary>*/
		public static async Task<bool> Show(object exceptionObject, bool alwaysContinue = false) {
			ErrorMessageBox messageBox = new ErrorMessageBox(exceptionObject, alwaysContinue);
			return await messageBox.ShowDialog<bool>(null);
		}

		#endregion

		/**<summary>Gets the currently focused navigation button, or null.</summary>*/
		private Button GetFocusedButton() {
			if (buttonContinue.IsFocused) return buttonContinue;
			if (buttonExit.IsFocused) return buttonExit;
			if (buttonCopy.IsFocused) return buttonCopy;
			if (buttonException.IsFocused) return buttonException;
			return null;
		}
	}
}
