using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;

namespace TConvert.Windows {
	/**<summary>A window to display credits for the program.</summary>*/
	public partial class CreditsWindow : Window {
		//========= CONSTRUCTORS =========
		#region Constructors

		/**<summary>Constructs the credits window.</summary>*/
		public CreditsWindow() {
			InitializeComponent();
		}

		#endregion
		//=========== SHOWING ============
		#region Showing

		/**<summary>Shows the credits window.</summary>*/
		public new static void Show(Window owner) {
			CreditsWindow window = new CreditsWindow();
			window.ShowDialog(owner);
		}

		#endregion
	}
}
