using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TConvert.Convert;
using TConvert.Util;
using System.Text.Json;

namespace TConvert {
	/**<summary>The types of input modes for convert or extract.</summary>*/
	public enum InputModes {
		Folder = 0,
		File = 1
	}
	/**<summary>The tabs to be switched to and from.</summary>*/
	public enum Tabs {
		Extract = 0,
		Convert = 1,
		Backup = 2,
		Script = 3
	}
	/**<summary>The config settings handler.</summary>*/
	public static class Config {
		//=========== MEMBERS ============
		#region Members

		/**<summary>The path to the JSON config file.</summary>*/
		private static readonly string ConfigPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"TConvert", "TConvert.json"
		);

		/**<summary>The specified Terraria Content folder.</summary>*/
		public static string TerrariaContentDirectory { get; set; }
		/**<summary>The current tab.</summary>*/
		public static Tabs CurrentTab { get; set; }
		/**<summary>True if normal progress windows are auto-closed.</summary>*/
		public static bool AutoCloseProgress { get; set; }
		/**<summary>True if file drop progress windows are auto-closed.</summary>*/
		public static bool AutoCloseDropProgress { get; set; }
		/**<summary>True if command line progress windows are auto-closed.</summary>*/
		public static bool AutoCloseCmdProgress { get; set; }
		/**<summary>True if images are compressed.</summary>*/
		public static bool CompressImages { get; set; }
		/**<summary>True if a sound is played on completion.</summary>*/
		public static bool CompletionSound { get; set; }
		/**<summary>True if alpha is premultiplied by default when converting to xnb.</summary>*/
		public static bool PremultiplyAlpha { get; set; }

		#endregion
		//=========== CLASSES ============
		#region Classes

		/**<summary>A container for extract settings.</summary>*/
		public static class Extract {
			/**<summary>File or Folder mode.</summary>*/
			public static InputModes Mode;
			/**<summary>The input for folder mode.</summary>*/
			public static string FolderInput { get; set; }
			/**<summary>The output for folder mode.</summary>*/
			public static string FolderOutput { get; set; }
			/**<summary>The input for file mode.</summary>*/
			public static string FileInput { get; set; }
			/**<summary>The output for file mode.</summary>*/
			public static string FileOutput { get; set; }
			/**<summary>True if images are extracted.</summary>*/
			public static bool AllowImages { get; set; }
			/**<summary>True if sounds are extracted.</summary>*/
			public static bool AllowSounds { get; set; }
			/**<summary>True if fonts are extracted.</summary>*/
			public static bool AllowFonts { get; set; }
			/**<summary>True if wave banks are extracted.</summary>*/
			public static bool AllowWaveBank { get; set; }
			/**<summary>True if the output path is the input path.</summary>*/
			public static bool UseInput { get; set; }
			/**<summary>Gets the current input path.</summary>*/
			public static string CurrentInput {
				get {
					switch (Mode) {
					case InputModes.Folder: return FolderInput;
					case InputModes.File: return FileInput;
					}
					return "";
				}
				set {
					switch (Mode) {
					case InputModes.Folder: FolderInput = value; break;
					case InputModes.File: FileInput = value; break;
					}
				}
			}
			/**<summary>Gets the current output path.</summary>*/
			public static string CurrentOutput {
				get {
					switch (Mode) {
					case InputModes.Folder: return FolderOutput;
					case InputModes.File: return FileOutput;
					}
					return "";
				}
				set {
					switch (Mode) {
					case InputModes.Folder: FolderOutput = value; break;
					case InputModes.File: FileOutput = value; break;
					}
				}
			}
		}
		/**<summary>A container for convert settings.</summary>*/
		public static class Convert {
			/**<summary>File or Folder mode.</summary>*/
			public static InputModes Mode { get; set; }
			/**<summary>The input for folder mode.</summary>*/
			public static string FolderInput { get; set; }
			/**<summary>The output for folder mode.</summary>*/
			public static string FolderOutput { get; set; }
			/**<summary>The input for file mode.</summary>*/
			public static string FileInput { get; set; }
			/**<summary>The output for file mode.</summary>*/
			public static string FileOutput { get; set; }
			/**<summary>True if images are converted.</summary>*/
			public static bool AllowImages { get; set; }
			/**<summary>True if sounds are converted.</summary>*/
			public static bool AllowSounds { get; set; }
			/**<summary>True if the output path is the input path.</summary>*/
			public static bool UseInput { get; set; }
			/**<summary>Gets the current input path.</summary>*/
			public static string CurrentInput {
				get {
					switch (Mode) {
					case InputModes.Folder: return FolderInput;
					case InputModes.File: return FileInput;
					}
					return "";
				}
				set {
					switch (Mode) {
					case InputModes.Folder: FolderInput = value; break;
					case InputModes.File: FileInput = value; break;
					}
				}
			}
			/**<summary>Gets the current output path.</summary>*/
			public static string CurrentOutput {
				get {
					switch (Mode) {
					case InputModes.Folder: return FolderOutput;
					case InputModes.File: return FileOutput;
					}
					return "";
				}
				set {
					switch (Mode) {
					case InputModes.Folder: FolderOutput = value; break;
					case InputModes.File: FileOutput = value; break;
					}
				}
			}
		}
		/**<summary>A container for backup settings.</summary>*/
		public static class Backup {
			/**<summary>The last selected content folder.</summary>*/
			public static string FolderContent { get; set; }
			/**<summary>The last selected backup folder.</summary>*/
			public static string FolderBackup { get; set; }
		}
		/**<summary>A container for script settings.</summary>*/
		public static class Script {
			/**<summary>The last selected script file.</summary>*/
			public static string File { get; set; }
		}

		#endregion
		//=========== LOADING ============
		#region Loading

		/**<summary>A serializable snapshot of all settings.</summary>*/
		private class ConfigData {
			public string TerrariaContentDirectory { get; set; } = "";
			public string CurrentTab { get; set; } = "Extract";
			public bool AutoCloseProgress { get; set; } = false;
			public bool AutoCloseDropProgress { get; set; } = true;
			public bool AutoCloseCmdProgress { get; set; } = true;
			public bool CompressImages { get; set; } = true;
			public bool CompletionSound { get; set; } = false;
			public bool PremultiplyAlpha { get; set; } = true;

			public string ExtractFolderInput { get; set; } = "";
			public string ExtractFolderOutput { get; set; } = "";
			public string ExtractFileInput { get; set; } = "";
			public string ExtractFileOutput { get; set; } = "";
			public string ConvertFolderInput { get; set; } = "";
			public string ConvertFolderOutput { get; set; } = "";
			public string ConvertFileInput { get; set; } = "";
			public string ConvertFileOutput { get; set; } = "";
			public string BackupFolderContent { get; set; } = "";
			public string BackupFolderBackup { get; set; } = "";
			public string ScriptFile { get; set; } = "";

			public string ExtractMode { get; set; } = "Folder";
			public string ConvertMode { get; set; } = "Folder";

			public bool ExtractAllowImages { get; set; } = true;
			public bool ExtractAllowSounds { get; set; } = true;
			public bool ExtractAllowFonts { get; set; } = true;
			public bool ExtractAllowWaveBank { get; set; } = true;
			public bool ExtractUseInput { get; set; } = false;
			public bool ConvertAllowImages { get; set; } = true;
			public bool ConvertAllowSounds { get; set; } = true;
			public bool ConvertUseInput { get; set; } = false;
		}

		/**<summary>Loads the settings.</summary>*/
		public static void Load() {
			ConfigData data = null;
			try {
				if (File.Exists(ConfigPath)) {
					string json = File.ReadAllText(ConfigPath);
					if (!string.IsNullOrWhiteSpace(json))
						data = JsonSerializer.Deserialize<ConfigData>(json);
				}
			}
			catch { }
			if (data == null)
				data = new ConfigData();

			TerrariaContentDirectory = data.TerrariaContentDirectory;
			if (TerrariaContentDirectory == "" && !string.IsNullOrEmpty(TerrariaLocator.TerrariaContentDirectory)) {
				TerrariaContentDirectory = TerrariaLocator.TerrariaContentDirectory;
			}
			Tabs tab;
			Enum.TryParse<Tabs>(data.CurrentTab, out tab);
			CurrentTab = tab;
			AutoCloseProgress = data.AutoCloseProgress;
			AutoCloseDropProgress = data.AutoCloseDropProgress;
			AutoCloseCmdProgress = data.AutoCloseCmdProgress;
			CompressImages = data.CompressImages;
			CompletionSound = data.CompletionSound;
			PremultiplyAlpha = data.PremultiplyAlpha;

			Extract.FolderInput = data.ExtractFolderInput;
			Extract.FolderOutput = data.ExtractFolderOutput;
			Extract.FileInput = data.ExtractFileInput;
			Extract.FileOutput = data.ExtractFileOutput;

			Convert.FolderInput = data.ConvertFolderInput;
			Convert.FolderOutput = data.ConvertFolderOutput;
			Convert.FileInput = data.ConvertFileInput;
			Convert.FileOutput = data.ConvertFileOutput;

			Backup.FolderContent = data.BackupFolderContent;
			Backup.FolderBackup = data.BackupFolderBackup;

			Script.File = data.ScriptFile;

			InputModes mode;
			Enum.TryParse<InputModes>(data.ExtractMode, out mode);
			Extract.Mode = mode;

			Enum.TryParse<InputModes>(data.ConvertMode, out mode);
			Convert.Mode = mode;

			Extract.AllowImages = data.ExtractAllowImages;
			Extract.AllowSounds = data.ExtractAllowSounds;
			Extract.AllowFonts = data.ExtractAllowFonts;
			Extract.AllowWaveBank = data.ExtractAllowWaveBank;
			Extract.UseInput = data.ExtractUseInput;

			Convert.AllowImages = data.ConvertAllowImages;
			Convert.AllowSounds = data.ConvertAllowSounds;
			Convert.UseInput = data.ConvertUseInput;
		}
		/**<summary>Saves the settings.</summary>*/
		public static void Save() {
			ConfigData data = new ConfigData {
				TerrariaContentDirectory = TerrariaContentDirectory,
				CurrentTab = CurrentTab.ToString(),
				AutoCloseProgress = AutoCloseProgress,
				AutoCloseDropProgress = AutoCloseDropProgress,
				AutoCloseCmdProgress = AutoCloseCmdProgress,
				CompressImages = CompressImages,
				CompletionSound = CompletionSound,
				PremultiplyAlpha = PremultiplyAlpha,

				ExtractFolderInput = Extract.FolderInput,
				ExtractFolderOutput = Extract.FolderOutput,
				ExtractFileInput = Extract.FileInput,
				ExtractFileOutput = Extract.FileOutput,
				ConvertFolderInput = Convert.FolderInput,
				ConvertFolderOutput = Convert.FolderOutput,
				ConvertFileInput = Convert.FileInput,
				ConvertFileOutput = Convert.FileOutput,
				BackupFolderContent = Backup.FolderContent,
				BackupFolderBackup = Backup.FolderBackup,
				ScriptFile = Script.File,

				ExtractMode = Extract.Mode.ToString(),
				ConvertMode = Convert.Mode.ToString(),

				ExtractAllowImages = Extract.AllowImages,
				ExtractAllowSounds = Extract.AllowSounds,
				ExtractAllowFonts = Extract.AllowFonts,
				ExtractAllowWaveBank = Extract.AllowWaveBank,
				ExtractUseInput = Extract.UseInput,
				ConvertAllowImages = Convert.AllowImages,
				ConvertAllowSounds = Convert.AllowSounds,
				ConvertUseInput = Convert.UseInput,
			};
			try {
				string dir = Path.GetDirectoryName(ConfigPath);
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
				File.WriteAllText(ConfigPath, JsonSerializer.Serialize(data,
					new JsonSerializerOptions { WriteIndented = true }));
			}
			catch { }
		}

		#endregion
	}
}
