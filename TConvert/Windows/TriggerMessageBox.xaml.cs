using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Interactivity;
using TConvert.Util;

namespace TConvert.Windows {
	/**<summary>The different types of icons available for message boxes.</summary>*/
	public enum MessageIcon {
		/**<summary>A blue (i) icon.</summary>*/
		Info,
		/**<summary>A blue (?) icon.</summary>*/
		Question,
		/**<summary>A yellow /!\ icon.</summary>*/
		Warning,
		/**<summary>A red (!) icon.</summary>*/
		Error
	}
	/**<summary>The different button configurations for message boxes.</summary>*/
	public enum MessageBoxButton {
		OK,
		OKCancel,
		YesNo,
		YesNoCancel
	}
	/**<summary>The result of pressing a message box button.</summary>*/
	public enum MessageBoxResult {
		None,
		OK,
		Cancel,
		Yes,
		No
	}

	/**<summary>A custom message box that doesn't look like shite.</summary>*/
	public partial class TriggerMessageBox : Window {
		//=========== MEMBERS ============
		#region Members

		/**<summary>The result of from pressing one of the message box buttons.</summary>*/
		private MessageBoxResult result;
		/**<summary>The minimum width of the message box.</summary>*/
		private int minWidth;
		/**<summary>The message box buttons setup.</summary>*/
		private MessageBoxButton buttons;

		#endregion
		//========= CONSTRUCTORS =========
		#region Constructors

		/**<summary>Constructs and sets up the message box.</summary>*/
		private TriggerMessageBox(MessageIcon icon, string title, string message, MessageBoxButton buttons, string buttonName1 = null, string buttonName2 = null, string buttonName3 = null) {
			InitializeComponent();
			this.buttons = buttons;
			this.minWidth = 280;

			// Setup the buttons
			switch (buttons) {
			#region MessageBoxButton.OK
			case MessageBoxButton.OK:
				button1.IsDefault = true;
				button1.Content = "OK";
				button1.Tag = MessageBoxResult.OK;
				button2.IsVisible = false;
				button3.IsVisible = false;
				minWidth -= 85 * 2;
				result = MessageBoxResult.OK;
				if (buttonName1 != null)
					button1.Content = buttonName1;
				break;
			#endregion
			#region MessageBoxButton.OKCancel
			case MessageBoxButton.OKCancel:
				button1.IsDefault = true;
				button1.Content = "OK";
				button1.Tag = MessageBoxResult.OK;
				button2.IsCancel = true;
				button2.Content = "Cancel";
				button2.Tag = MessageBoxResult.Cancel;
				button3.IsVisible = false;
				minWidth -= 85;
				result = MessageBoxResult.Cancel;
				if (buttonName1 != null)
					button1.Content = buttonName1;
				if (buttonName2 != null)
					button2.Content = buttonName2;
				break;
			#endregion
			#region MessageBoxButton.YesNo
			case MessageBoxButton.YesNo:
				button1.IsDefault = true;
				button1.Content = "Yes";
				button1.Tag = MessageBoxResult.Yes;
				button2.IsCancel = true;
				button2.Content = "No";
				button2.Tag = MessageBoxResult.No;
				button3.IsVisible = false;
				minWidth -= 85;
				result = MessageBoxResult.No;
				if (buttonName1 != null)
					button1.Content = buttonName1;
				if (buttonName2 != null)
					button2.Content = buttonName2;
				break;
			#endregion
			#region MessageBoxButton.YesNoCancel
			case MessageBoxButton.YesNoCancel:
				button1.IsDefault = true;
				button1.Content = "Yes";
				button1.Tag = MessageBoxResult.Yes;
				button2.Content = "No";
				button2.Tag = MessageBoxResult.No;
				button3.IsCancel = true;
				button3.Content = "Cancel";
				button3.Tag = MessageBoxResult.Cancel;
				result = MessageBoxResult.Cancel;
				if (buttonName1 != null)
					button1.Content = buttonName1;
				if (buttonName2 != null)
					button2.Content = buttonName2;
				if (buttonName3 != null)
					button3.Content = buttonName3;
				break;
				#endregion
			}

			this.Title = title;
			this.textBlockMessage.Text = message;
			this.iconTemp = icon;
		}

		#endregion
		//=========== HELPERS ============
		#region Helpers

		/**<summary>Gets the number of message box buttons.</summary>*/
		private int ButtonCount {
			get {
				switch (buttons) {
				case MessageBoxButton.OK:
					return 1;
				case MessageBoxButton.OKCancel:
				case MessageBoxButton.YesNo:
					return 2;
				case MessageBoxButton.YesNoCancel:
					return 3;
				}
				return 3;
			}
		}
		/**<summary>Gets the button at the specified index.</summary>*/
		private Button GetButtonAt(int index) {
			switch (index) {
			case 0: return button1;
			case 1: return button2;
			case 2: return button3;
			}
			return null;
		}

		#endregion
		//============ EVENTS ============
		#region Events

		private void OnWindowLoaded(object sender, RoutedEventArgs e) {
			#region Load Message Sounds
			switch (iconTemp) {
			case MessageIcon.Info: Sound.Asterisk(); break;
			case MessageIcon.Question: Sound.Asterisk(); break;
			case MessageIcon.Warning: Sound.Exclamation(); break;
			case MessageIcon.Error: Sound.Hand(); break;
			}
			#endregion
		}
		private void OnButtonClicked(object sender, RoutedEventArgs e) {
			result = (MessageBoxResult)((Button)sender).Tag;
			Close(result);
		}
		private void OnPreviewKeyDown(object sender, KeyEventArgs e) {
			int focused = CurrentFocusedIndex();
			switch (e.Key) {
			case Key.Right:
				if (focused < 0 && ButtonCount > 1)
					GetButtonAt(1).Focus();
				else if (focused >= 0 && focused < ButtonCount - 1)
					GetButtonAt(focused + 1).Focus();
				e.Handled = true;
				break;
			case Key.Left:
				if (focused > 0)
					GetButtonAt(focused - 1).Focus();
				e.Handled = true;
				break;
			}
		}
		/**<summary>Gets the index of the currently focused button, or -1.</summary>*/
		private int CurrentFocusedIndex() {
			if (button1.IsVisible && button1.IsFocused) return 0;
			if (button2.IsVisible && button2.IsFocused) return 1;
			if (button3.IsVisible && button3.IsFocused) return 2;
			return -1;
		}

		#endregion
		//=========== SHOWING ============
		#region Showing

		/**<summary>Shows the message box.</summary>*/
		public static Task<MessageBoxResult> Show(Window window, MessageIcon icon, string message) {
			return Show(window, icon, message, "", MessageBoxButton.OK);
		}
		/**<summary>Shows the message box.</summary>*/
		public static Task<MessageBoxResult> Show(Window window, MessageIcon icon, string message, string title) {
			return Show(window, icon, message, title, MessageBoxButton.OK);
		}
		/**<summary>Shows the message box.</summary>*/
		public static Task<MessageBoxResult> Show(Window window, MessageIcon icon, string message, MessageBoxButton buttons) {
			return Show(window, icon, message, "", buttons);
		}
		/**<summary>Shows the message box.</summary>*/
		public static async Task<MessageBoxResult> Show(Window window, MessageIcon icon, string message, string title, MessageBoxButton buttons, string buttonName1 = null, string buttonName2 = null, string buttonName3 = null) {
			TriggerMessageBox messageBox = new TriggerMessageBox(icon, title, message, buttons, buttonName1, buttonName2, buttonName3);
			if (window == null || !window.IsVisible)
				messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			return await messageBox.ShowDialog<MessageBoxResult>(window);
		}

		#endregion

		/**<summary>Stores the icon for the loaded sound handler.</summary>*/
		private MessageIcon iconTemp;
		/**<summary>Gets the index of the button.</summary>*/
		private int IndexOfButton(Button button) {
			if (button == button1)
				return 0;
			else if (button == button2)
				return 1;
			else if (button == button3)
				return 2;
			return -1;
		}
	}
}
