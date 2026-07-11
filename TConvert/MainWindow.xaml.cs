using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.Interactivity;
using TConvert.Convert;
using TConvert.Extract;
using TConvert.Util;
using TConvert.Windows;
using Path = System.IO.Path;

namespace TConvert {
	/**<summary>The main TConvert window.</summary>*/
	public partial class MainWindow : Window {
		//=========== MEMBERS ============
		#region Members

		/**<summary>True if the window has been loaded.</summary>*/
		bool loaded;
		/**<summary>The last path visited with a folder browser.</summary>*/
		string lastFolderPath;
		/**<summary>The last path visited with a file browser.</summary>*/
		string lastFilePath;

		#endregion
		//========= CONSTRUCTORS =========
		#region Constructors

		/**<summary>Constructs the main window.</summary>*/
		public MainWindow() {
			loaded = false;
			InitializeComponent();
			lastFolderPath = "";
			lastFilePath = "";

			labelDrop.IsVisible = false;

			LoadConfig();
		}

		#endregion
		//============ CONFIG ============
		#region Config

		/**<summary>Loads the config and updates controls.</summary>*/
		void LoadConfig() {
			loaded = false;

			Config.Load();

			textBoxTerrariaContent.Text = Config.TerrariaContentDirectory;

			comboBoxExtractMode.SelectedIndex = (int)Config.Extract.Mode;
			textBoxExtractInput.Text = Config.Extract.CurrentInput;
			textBoxExtractOutput.Text = Config.Extract.CurrentOutput;
			checkBoxExtractImages.IsChecked = Config.Extract.AllowImages;
			checkBoxExtractSounds.IsChecked = Config.Extract.AllowSounds;
			checkBoxExtractFonts.IsChecked = Config.Extract.AllowFonts;
			checkBoxExtractWaveBank.IsChecked = Config.Extract.AllowWaveBank;
			checkBoxExtractUseInput.IsChecked = Config.Extract.UseInput;
			textBoxExtractOutput.IsEnabled = !Config.Extract.UseInput;
			buttonExtractOutput.IsEnabled = !Config.Extract.UseInput;
			buttonExtractUseTerraria.IsEnabled = !Config.Extract.UseInput && Config.Extract.Mode == InputModes.Folder;
			checkBoxExtractImages.IsEnabled = Config.Extract.Mode == InputModes.Folder;
			checkBoxExtractSounds.IsEnabled = Config.Extract.Mode == InputModes.Folder;
			checkBoxExtractFonts.IsEnabled = Config.Extract.Mode == InputModes.Folder;
			checkBoxExtractWaveBank.IsEnabled = Config.Extract.Mode == InputModes.Folder;
			switch (Config.Extract.Mode) {
			case InputModes.Folder:
				labelExtractInput.Content = "Input Folder";
				labelExtractOutput.Content = "Output Folder";
				break;
			case InputModes.File:
				labelExtractInput.Content = "Input File";
				labelExtractOutput.Content = "Output File";
				break;
			}

			comboBoxConvertMode.SelectedIndex = (int)Config.Convert.Mode;
			textBoxConvertInput.Text = Config.Convert.CurrentInput;
			textBoxConvertOutput.Text = Config.Convert.CurrentOutput;
			checkBoxConvertImages.IsChecked = Config.Convert.AllowImages;
			checkBoxConvertSounds.IsChecked = Config.Convert.AllowSounds;
			checkBoxConvertUseInput.IsChecked = Config.Convert.UseInput;
			textBoxConvertOutput.IsEnabled = !Config.Convert.UseInput;
			buttonConvertOutput.IsEnabled = !Config.Convert.UseInput;
			buttonConvertUseTerraria.IsEnabled = !Config.Convert.UseInput && Config.Convert.Mode == InputModes.Folder;
			checkBoxConvertImages.IsEnabled = Config.Convert.Mode == InputModes.Folder;
			checkBoxConvertSounds.IsEnabled = Config.Convert.Mode == InputModes.Folder;
			switch (Config.Convert.Mode) {
			case InputModes.Folder:
				labelConvertInput.Content = "Input Folder";
				labelConvertOutput.Content = "Output Folder";
				break;
			case InputModes.File:
				labelConvertInput.Content = "Input File";
				labelConvertOutput.Content = "Output File";
				break;
			}

			textBoxContent.Text = Config.Backup.FolderContent;
			textBoxBackup.Text = Config.Backup.FolderBackup;

			textBoxScript.Text = Config.Script.File;

			tabControl.SelectedIndex = (int)Config.CurrentTab;

			menuItemCompressImages.IsEnabled = true;
			menuItemCompressImages.IsChecked = Config.CompressImages;
			menuItemPremultiply.IsChecked = Config.PremultiplyAlpha;
			menuItemCompletionSound.IsChecked = Config.CompletionSound;
			menuItemAutoCloseProgress.IsChecked = Config.AutoCloseProgress;
			menuItemAutoCloseDropProgress.IsChecked = Config.AutoCloseDropProgress;
			menuItemAutoCloseCmdProgress.IsChecked = Config.AutoCloseCmdProgress;
		}

		#endregion
		//============ EVENTS ============
		#region Events
		//--------------------------------
		#region General

		void OnWindowLoaded(object sender, RoutedEventArgs e) {
			loaded = true;
		}
		void OnWindowClosing(object sender, WindowClosingEventArgs e) {
			try {
				Config.Save();
			}
			catch { }
		}
		void OnTabChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded)
				return;
			Config.CurrentTab = (Tabs)tabControl.SelectedIndex;
		}
		async void OnBrowseTerraria(object sender, RoutedEventArgs e) {
			string path = await GetFolderPath(Config.TerrariaContentDirectory, "Choose Terraria Content folder");
			if (path != null) {
				Config.TerrariaContentDirectory = path;
				textBoxTerrariaContent.Text = path;
			}
		}
		void OnTerrariaContentChanged(object sender, TextChangedEventArgs e) {
			if (!loaded)
				return;
			Config.TerrariaContentDirectory = textBoxTerrariaContent.Text;
		}

		#endregion
		//--------------------------------
		#region Extracting

		void OnExtract(object sender, RoutedEventArgs e) {
			string input = Config.Extract.CurrentInput;
			string output = (Config.Extract.UseInput ? Config.Extract.CurrentInput : Config.Extract.CurrentOutput);
			bool allowImages = Config.Extract.AllowImages;
			bool allowSounds = Config.Extract.AllowSounds;
			bool allowFonts = Config.Extract.AllowFonts;
			bool allowWaveBank = Config.Extract.AllowWaveBank;

			Thread thread;
			if (Config.Extract.Mode == InputModes.Folder) {
				if (!Helpers.DirectoryExistsSafe(input)) {
					TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find the input folder.", "Invalid Path");
					return;
				}
				if (!Helpers.IsPathValid(output)) {
					TriggerMessageBox.Show(this, MessageIcon.Warning, "Output folder path is invalid.", "Invalid Path");
					return;
				}
				input = Helpers.FixPathSafe(input);
				output = Helpers.FixPathSafe(output);
				thread = new Thread(() => {
					Processing.ExtractAll(input, output, allowImages, allowSounds, allowFonts, allowWaveBank);
				});
			}
			else {
				if (!Helpers.FileExistsSafe(input)) {
					TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find the input file.", "Invalid Path");
					return;
				}
				if (!Helpers.IsPathValid(output)) {
					TriggerMessageBox.Show(this, MessageIcon.Warning, "Output file path is invalid.", "Invalid Path");
					return;
				}
				input = Helpers.FixPathSafe(input);
				output = Helpers.FixPathSafe(output);
				thread = new Thread(() => {
					Processing.ExtractSingleFile(input, output);
				});
			}
			Processing.StartProgressThread(this, "Extracting...", Config.AutoCloseProgress, Config.CompressImages, Config.CompletionSound, Config.PremultiplyAlpha, thread);
		}
		void OnExtractModeChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded)
				return;
			Config.Extract.Mode = (InputModes)comboBoxExtractMode.SelectedIndex;

			switch (Config.Extract.Mode) {
			case InputModes.Folder:
				labelExtractInput.Content = "Input Folder";
				labelExtractOutput.Content = "Output Folder";
				break;
			case InputModes.File:
				labelExtractInput.Content = "Input File";
				labelExtractOutput.Content = "Output File";
				break;
			}
			textBoxExtractInput.Text = Config.Extract.CurrentInput;
			textBoxExtractOutput.Text = Config.Extract.CurrentOutput;
			buttonExtractUseTerraria.IsEnabled = !Config.Extract.UseInput && Config.Extract.Mode == InputModes.Folder;
			checkBoxExtractImages.IsEnabled = Config.Extract.Mode == InputModes.Folder;
			checkBoxExtractSounds.IsEnabled = Config.Extract.Mode == InputModes.Folder;
			checkBoxExtractFonts.IsEnabled = Config.Extract.Mode == InputModes.Folder;
			checkBoxExtractWaveBank.IsEnabled = Config.Extract.Mode == InputModes.Folder;
		}
		async void OnExtractChangeInput(object sender, RoutedEventArgs e) {
			string path = await GetPath(Config.Extract.CurrentInput, true, true);
			if (path != null) {
				Config.Extract.CurrentInput = path;
				textBoxExtractInput.Text = path;
			}
		}
		async void OnExtractChangeOutput(object sender, RoutedEventArgs e) {
			string path = await GetPath(Config.Extract.CurrentOutput, false, true);
			if (path != null) {
				Config.Extract.CurrentOutput = path;
				textBoxExtractOutput.Text = path;
			}
		}
		void OnExtractUseInputChecked(object sender, RoutedEventArgs e) {
			Config.Extract.UseInput = checkBoxExtractUseInput.IsChecked ?? false;
			textBoxExtractOutput.IsEnabled = !Config.Extract.UseInput;
			buttonExtractOutput.IsEnabled = !Config.Extract.UseInput;
			buttonExtractUseTerraria.IsEnabled = !Config.Extract.UseInput && Config.Extract.Mode == InputModes.Folder;
		}
		void OnExtractImagesChecked(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			Config.Extract.AllowImages = checkBoxExtractImages.IsChecked ?? false;
		}
		void OnExtractSoundsChecked(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			Config.Extract.AllowSounds = checkBoxExtractSounds.IsChecked ?? false;
		}
		void OnExtractFontsChecked(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			Config.Extract.AllowFonts = checkBoxExtractFonts.IsChecked ?? false;
		}
		void OnExtractWaveBankChecked(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			Config.Extract.AllowWaveBank = checkBoxExtractWaveBank.IsChecked ?? false;
		}
		void OnExtractUseTerraria(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			Config.Extract.FolderInput = Config.TerrariaContentDirectory;
			textBoxExtractInput.Text = Config.TerrariaContentDirectory;
		}
		void OnExtractInputChanged(object sender, TextChangedEventArgs e) {
			if (!loaded)
				return;
			Config.Extract.CurrentInput = textBoxExtractInput.Text;
		}
		void OnExtractOutputChanged(object sender, TextChangedEventArgs e) {
			if (!loaded)
				return;
			Config.Extract.CurrentOutput = textBoxExtractOutput.Text;
		}

		#endregion
		//--------------------------------
		#region Converting

		void OnConvert(object sender, RoutedEventArgs e) {
			string input = Config.Convert.CurrentInput;
			string output = (Config.Convert.UseInput ? Config.Convert.CurrentInput : Config.Convert.CurrentOutput);
			bool allowImages = Config.Convert.AllowImages;
			bool allowSounds = Config.Convert.AllowSounds;

			Thread thread;
			if (Config.Convert.Mode == InputModes.Folder) {
				if (!Helpers.DirectoryExistsSafe(input)) {
					TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find the input folder.", "Invalid Path");
					return;
				}
				if (!Helpers.IsPathValid(output)) {
					TriggerMessageBox.Show(this, MessageIcon.Warning, "Output folder path is invalid.", "Invalid Path");
					return;
				}
				input = Helpers.FixPathSafe(input);
				output = Helpers.FixPathSafe(output);
				thread = new Thread(() => {
					Processing.ConvertAll(input, output, allowImages, allowSounds);
				});
			}
			else {
				if (!Helpers.FileExistsSafe(input)) {
					TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find the input file", "Invalid Path");
					return;
				}
				if (!Helpers.IsPathValid(output)) {
					TriggerMessageBox.Show(this, MessageIcon.Warning, "Output file path is invalid.", "Invalid Path");
					return;
				}
				input = Helpers.FixPathSafe(input);
				output = Helpers.FixPathSafe(output);
				thread = new Thread(() => {
					Processing.ConvertSingleFile(input, output);
				});
			}
			Processing.StartProgressThread(this, "Converting...", Config.AutoCloseProgress, Config.CompressImages, Config.CompletionSound, Config.PremultiplyAlpha, thread);
		}
		void OnConvertModeChanged(object sender, SelectionChangedEventArgs e) {
			if (!loaded)
				return;

			Config.Convert.Mode = (InputModes)comboBoxConvertMode.SelectedIndex;

			switch (Config.Convert.Mode) {
			case InputModes.Folder:
				labelConvertInput.Content = "Input Folder";
				labelConvertOutput.Content = "Output Folder";
				break;
			case InputModes.File:
				labelConvertInput.Content = "Input File";
				labelConvertOutput.Content = "Output File";
				break;
			}
			textBoxConvertInput.Text = Config.Convert.CurrentInput;
			textBoxConvertOutput.Text = Config.Convert.CurrentOutput;
			buttonConvertUseTerraria.IsEnabled = !Config.Convert.UseInput && Config.Convert.Mode == InputModes.Folder;
			checkBoxConvertImages.IsEnabled = Config.Convert.Mode == InputModes.Folder;
			checkBoxConvertSounds.IsEnabled = Config.Convert.Mode == InputModes.Folder;
		}
		async void OnConvertChangeInput(object sender, RoutedEventArgs e) {
			string path = await GetPath(Config.Convert.CurrentInput, true, false);
			if (path != null) {
				Config.Convert.CurrentInput = path;
				textBoxConvertInput.Text = path;
			}
		}
		async void OnConvertChangeOutput(object sender, RoutedEventArgs e) {
			string path = await GetPath(Config.Convert.CurrentOutput, false, false);
			if (path != null) {
				Config.Convert.CurrentOutput = path;
				textBoxConvertOutput.Text = path;
			}
		}
		void OnConvertUseInputChecked(object sender, RoutedEventArgs e) {
			Config.Convert.UseInput = checkBoxConvertUseInput.IsChecked ?? false;
			textBoxConvertOutput.IsEnabled = !Config.Convert.UseInput;
			buttonConvertOutput.IsEnabled = !Config.Convert.UseInput;
			buttonConvertUseTerraria.IsEnabled = !Config.Convert.UseInput && Config.Convert.Mode == InputModes.Folder;
		}
		void OnConvertImagesChecked(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			Config.Convert.AllowImages = checkBoxConvertImages.IsChecked ?? false;
		}
		void OnConvertSoundsChecked(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			Config.Convert.AllowSounds = checkBoxConvertSounds.IsChecked ?? false;
		}
		void OnConvertUseTerraria(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			Config.Convert.FolderOutput = Config.TerrariaContentDirectory;
			textBoxConvertOutput.Text = Config.TerrariaContentDirectory;
		}
		void OnConvertInputChanged(object sender, TextChangedEventArgs e) {
			if (!loaded)
				return;
			Config.Convert.CurrentInput = textBoxConvertInput.Text;
		}
		void OnConvertOutputChanged(object sender, TextChangedEventArgs e) {
			if (!loaded)
				return;
			Config.Convert.CurrentOutput = textBoxConvertOutput.Text;
		}

		#endregion
		//--------------------------------
		#region Backup/Restore

		void OnBackup(object sender, RoutedEventArgs e) {
			string input = Config.Backup.FolderContent;
			string output = Config.Backup.FolderBackup;

			if (!Helpers.DirectoryExistsSafe(input)) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find the Content folder.", "Invalid Path");
				return;
			}
			if (!Helpers.IsPathValid(output)) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "The Backup folder path is invalid.", "Invalid Path");
				return;
			}
			if (string.Compare(Path.GetFullPath(input), Path.GetFullPath(output), true) == 0) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Backup paths cannot be the same folder.", "Invalid Path");
				return;
			}

			input = Helpers.FixPathSafe(input);
			output = Helpers.FixPathSafe(output);
			Thread thread = new Thread(() => {
				Processing.BackupFiles(input, output);
			});
			Processing.StartProgressThread(this, "Backing Up...", Config.AutoCloseProgress, Config.CompressImages, Config.CompletionSound, Config.PremultiplyAlpha, thread);
		}
		void OnRestore(object sender, RoutedEventArgs e) {
			string input = Config.Backup.FolderBackup;
			string output = Config.Backup.FolderContent;

			if (!Helpers.DirectoryExistsSafe(input)) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find the Backup folder.", "Invalid Path");
				return;
			}
			if (!Helpers.DirectoryExistsSafe(output)) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find the Content folder.", "Invalid Path");
				return;
			}
			if (string.Compare(Path.GetFullPath(input), Path.GetFullPath(output), true) == 0) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Restore paths cannot be the same folder.", "Invalid Path");
				return;
			}

			input = Helpers.FixPathSafe(input);
			output = Helpers.FixPathSafe(output);
			Thread thread = new Thread(() => {
				Processing.RestoreFiles(input, output);
			});
			Processing.StartProgressThread(this, "Restoring...", Config.AutoCloseProgress, Config.CompressImages, Config.CompletionSound, Config.PremultiplyAlpha, thread);
		}
		async void OnBackupChangeContent(object sender, RoutedEventArgs e) {
			string path = await GetFolderPath(Config.Backup.FolderContent, "Choose Content folder");
			if (path != null) {
				Config.Backup.FolderContent = path;
				textBoxContent.Text = path;
			}
		}
		async void OnBackupChangeBackup(object sender, RoutedEventArgs e) {
			string path = await GetFolderPath(Config.Backup.FolderBackup, "Choose Backup folder");
			if (path != null) {
				Config.Backup.FolderBackup = path;
				textBoxBackup.Text = path;
			}
		}
		void OnBackupUseTerraria(object sender, RoutedEventArgs e) {
			Config.Backup.FolderContent = Config.TerrariaContentDirectory;
			textBoxContent.Text = Config.TerrariaContentDirectory;
		}
		void OnContentChanged(object sender, TextChangedEventArgs e) {
			if (!loaded)
				return;
			Config.Backup.FolderContent = textBoxContent.Text;
		}
		void OnBackupChanged(object sender, TextChangedEventArgs e) {
			if (!loaded)
				return;
			Config.Backup.FolderBackup = textBoxBackup.Text;
		}

		#endregion
		//--------------------------------
		#region Scripting

		void OnRunScript(object sender, RoutedEventArgs e) {
			string input = Config.Script.File;

			Thread thread;
			if (!Helpers.FileExistsSafe(input)) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find the script file.", "Invalid Path");
				return;
			}
			input = Helpers.FixPathSafe(input);
			thread = new Thread(() => {
				Processing.RunScript(input);
			});
			Processing.StartProgressThread(this, "Running Script...", Config.AutoCloseProgress, Config.CompressImages, Config.CompletionSound, Config.PremultiplyAlpha, thread);
		}
		async void OnChangeScript(object sender, RoutedEventArgs e) {
			string input = Config.Script.File;

			if (!Helpers.FileExistsSafe(input)) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find the script file.", "Invalid Path");
				return;
			}
			IStorageFolder scriptStart = null;
			if (!string.IsNullOrEmpty(Config.Script.File)) {
				try {
					scriptStart = await StorageProvider.TryGetFolderFromPathAsync(Path.GetDirectoryName(Config.Script.File) ?? lastFilePath);
				}
				catch {
					scriptStart = null;
				}
			}
			var options = new FilePickerOpenOptions {
				Title = "Choose script file",
				AllowMultiple = false,
				SuggestedFileName = !string.IsNullOrEmpty(Config.Script.File) ? Path.GetFileName(Config.Script.File) : null,
				SuggestedStartLocation = scriptStart,
				FileTypeFilter = new List<FilePickerFileType> {
					new FilePickerFileType("Xml files") { Patterns = new List<string> { "xml" } },
					new FilePickerFileType("All files") { Patterns = new List<string> { "*" } }
				}
			};
			var result = await StorageProvider.OpenFilePickerAsync(options);
			if (result != null && result.Count > 0) {
				string file = result[0].Path.LocalPath;
				textBoxScript.Text = file;
				Config.Script.File = file;
				try {
					lastFilePath = Path.GetDirectoryName(file);
				}
				catch { }
			}
		}
		void OnScriptChanged(object sender, TextChangedEventArgs e) {
			if (!loaded)
				return;
			Config.Script.File = textBoxScript.Text;
		}

		#endregion
		//--------------------------------
		#region File Drop

		void OnFileDrop(object sender, DragEventArgs e) {
			labelDrop.IsVisible = false;
			if (this.OwnedWindows.Count == 0 && e.DataTransfer.Items.OfType<IStorageItem>().Any()) {
				List<string> files = new List<string>();
				List<string> extractFiles = new List<string>();
				List<string> convertFiles = new List<string>();
				List<string> scriptFiles = new List<string>();
				var initialFiles = e.DataTransfer.Items.OfType<IStorageItem>().Select(item => item.Path.LocalPath).ToArray();

				// Allow extracting/converting of directories too
				foreach (string file in initialFiles) {
					if (Directory.Exists(file))
						files.AddRange(Helpers.FindAllFiles(file));
					else
						files.Add(file);
				}

				foreach (string file in files) {
					string ext = Path.GetExtension(file).ToLower();
					switch (ext) {
					case ".xnb":
					case ".xwb":
						extractFiles.Add(file);
						break;
					case ".png":
					case ".bmp":
					case ".jpg":
						convertFiles.Add(file);
						break;
					case ".xml":
						scriptFiles.Add(file);
						break;
					default:
						if (Processing.IsAudioExtension(ext))
							convertFiles.Add(file);
						break;
					}
				}

				if (extractFiles.Count == 0 && convertFiles.Count == 0 && scriptFiles.Count == 0) {
					TriggerMessageBox.Show(this, MessageIcon.Warning, "No files to convert or extract, or scripts to run!", "File Drop");
				}
				else {
					Thread thread = new Thread(() => {
						Processing.ProcessDropFiles(extractFiles.ToArray(), convertFiles.ToArray(), scriptFiles.ToArray());
					});
					Processing.StartProgressThread(this, "Processing Drop Files...", Config.AutoCloseDropProgress, Config.CompressImages, Config.CompletionSound, Config.PremultiplyAlpha, thread);
				}
			}
		}
		void OnFileDropEnter(object sender, DragEventArgs e) {
			if (this.OwnedWindows.Count == 0 && e.DataTransfer.Items.OfType<IStorageItem>().Any()) {
				labelDrop.IsVisible = true;
				e.DragEffects = DragDropEffects.Copy;
			}
			else {
				e.DragEffects = DragDropEffects.None;
			}
			e.Handled = true;
		}
		void OnFileDropOver(object sender, DragEventArgs e) {
			if (this.OwnedWindows.Count == 0 && e.DataTransfer.Items.OfType<IStorageItem>().Any()) {
				e.DragEffects = DragDropEffects.Copy;
			}
			else {
				e.DragEffects = DragDropEffects.None;
			}
			e.Handled = true;
		}
		void OnFileDropLeave(object sender, DragEventArgs e) {
			labelDrop.IsVisible = false;
		}

		#endregion
		//--------------------------------
		#region Menu Items

		void OnLaunchTerraria(object sender, RoutedEventArgs e) {
			if (Config.TerrariaContentDirectory != string.Empty) {
				try {
					string dir = Path.GetDirectoryName(Config.TerrariaContentDirectory);
					string terraria = Path.Combine(dir, "Terraria.exe");
					if (File.Exists(terraria)) {
						ProcessStartInfo start = new ProcessStartInfo();
						start.FileName = terraria;
						start.Arguments = TerrariaLocator.FindTerraLauncherSaveDirectory(terraria);
						start.WorkingDirectory = dir;
						start.UseShellExecute = true;
						Process.Start(start);
						return;
					}
				}
				catch { }

				try {
					string dir = Config.TerrariaContentDirectory;
					string terraria = Path.Combine(dir, "Terraria.exe");
					if (File.Exists(terraria)) {
						ProcessStartInfo start = new ProcessStartInfo();
						start.FileName = terraria;
						start.Arguments = TerrariaLocator.FindTerraLauncherSaveDirectory(terraria);
						start.WorkingDirectory = dir;
						start.UseShellExecute = true;
						Process.Start(start);
						return;
					}
				}
				catch { }
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Failed to locate Terraria executable.", "Missing Exe");
			}
			else {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "No path to Terraria specified.", "No Path");
			}
		}
		void OnOpenTerrariaFolder(object sender, RoutedEventArgs e) {
			if (Config.TerrariaContentDirectory != string.Empty) {
				string dir = Config.TerrariaContentDirectory;
				try {
					dir = Path.GetDirectoryName(Config.TerrariaContentDirectory);
					OpenPath(dir);
				}
				catch {
					TriggerMessageBox.Show(this, MessageIcon.Warning, "Failed to locate Terraria folder.", "Missing Folder");
				}
			}
		}
		void OnExit(object sender, RoutedEventArgs e) {
			Close();
		}

		void OnPremultiplyAlphaChecked(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			Config.PremultiplyAlpha = menuItemPremultiply.IsChecked;
		}
		void OnCompressImagesChecked(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			Config.CompressImages = menuItemCompressImages.IsChecked;
		}
		void OnCompletionSoundChecked(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			Config.CompletionSound = menuItemCompletionSound.IsChecked;
		}
		void OnAutoCloseProgress(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			Config.AutoCloseProgress = menuItemAutoCloseProgress.IsChecked;
		}
		void OnAutoCloseDropProgress(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			Config.AutoCloseDropProgress = menuItemAutoCloseDropProgress.IsChecked;
		}
		void OnAutoCloseCmdProgress(object sender, RoutedEventArgs e) {
			if (!loaded)
				return;
			Config.AutoCloseCmdProgress = menuItemAutoCloseCmdProgress.IsChecked;
		}

		void OnAbout(object sender, RoutedEventArgs e) {
			AboutWindow.Show(this);
		}
		void OnHelp(object sender, RoutedEventArgs e) {
			OpenPath("https://github.com/trigger-death/TConvert/wiki");
		}
		void OnCredits(object sender, RoutedEventArgs e) {
			CreditsWindow.Show(this);
		}
		void OnViewOnGitHub(object sender, RoutedEventArgs e) {
			OpenPath("https://github.com/trigger-death/TConvert");
		}

		#endregion
		//--------------------------------
		#endregion
		//=========== HELPERS ============
		#region Helpers

		/**<summary>Opens a path using the OS shell.</summary>*/
		static void OpenPath(string path) {
			try {
				Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
			}
			catch { }
		}
		/**<summary>Shows a folder browser dialog and returns the selected path.</summary>*/
		async Task<string> GetFolderPath(string currentPath, string description) {
			var options = new FolderPickerOpenOptions {
				Title = description,
				AllowMultiple = false
			};
			if (!string.IsNullOrEmpty(currentPath)) {
				try {
					options.SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(currentPath);
				}
				catch { }
			}
			var result = await StorageProvider.OpenFolderPickerAsync(options);
			if (result != null && result.Count > 0) {
				string path = result[0].Path.LocalPath;
				lastFolderPath = path;
				return path;
			}
			return null;
		}
		/**<summary>Shows a file or folder browser dialog and returns the selected path.</summary>*/
		async Task<string> GetPath(string currentPath, bool input, bool extract) {
			switch (extract ? Config.Extract.Mode : Config.Convert.Mode) {
			case InputModes.Folder: {
					var options = new FolderPickerOpenOptions {
						Title = "Choose " + (input ? "input" : "output") + " folder",
						AllowMultiple = false
					};
					if (!string.IsNullOrEmpty(currentPath)) {
						try {
							options.SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(currentPath);
						}
						catch { }
					}
					var result = await StorageProvider.OpenFolderPickerAsync(options);
					if (result != null && result.Count > 0) {
						string path = result[0].Path.LocalPath;
						lastFolderPath = path;
						return path;
					}
					return null;
				}
			case InputModes.File: {
					IStorageFolder start = null;
					if (!string.IsNullOrEmpty(currentPath)) {
						try {
							start = await StorageProvider.TryGetFolderFromPathAsync(Path.GetDirectoryName(currentPath));
						}
						catch { }
					}
					FilePickerFileType[] filters = new FilePickerFileType[] {
						new FilePickerFileType(extract == input ? "Xna files" : "Image & Audio files") {
							Patterns = (extract == input ?
								new List<string> { "xnb", "xwb" } :
								new List<string> { "png", "bmp", "jpg", "wav", "mp3", "mp2", "mpga", "m4a", "aac", "flac", "ogg", "wma", "aif", "aiff", "aifc" })
						},
						new FilePickerFileType("All files") { Patterns = new List<string> { "*" } }
					};
					if (input) {
						var options = new FilePickerOpenOptions {
							Title = "Choose " + (input ? "input" : "output") + " file",
							AllowMultiple = false,
							SuggestedFileName = !string.IsNullOrEmpty(currentPath) ? Path.GetFileName(currentPath) : null,
							SuggestedStartLocation = start,
							FileTypeFilter = filters
						};
						var result = await StorageProvider.OpenFilePickerAsync(options);
						if (result != null && result.Count > 0) {
							string file = result[0].Path.LocalPath;
							try {
								lastFilePath = Path.GetDirectoryName(file);
							}
							catch { }
							return file;
						}
					}
					else {
						var options = new FilePickerSaveOptions {
							Title = "Choose " + (input ? "input" : "output") + " file",
							SuggestedFileName = !string.IsNullOrEmpty(currentPath) ? Path.GetFileName(currentPath) : null,
							SuggestedStartLocation = start,
							FileTypeChoices = filters
						};
						var result = await StorageProvider.SaveFilePickerAsync(options);
						if (result != null) {
							string file = result.Path.LocalPath;
							try {
								lastFilePath = Path.GetDirectoryName(file);
							}
							catch { }
							return file;
						}
					}
					break;
				}
			}
			return null;
		}

		#endregion
	}
}
