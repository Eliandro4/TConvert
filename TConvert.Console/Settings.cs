using System;

namespace TConvert.Properties {
	/**<summary>
	 * A portable, in-memory replacement for the designer-generated
	 * TConvert.Properties.Settings used by the Windows build. The console
	 * build has no persisted user settings, so all values use their defaults.
	 *</summary>*/
	internal sealed class Settings {
		private static readonly Settings defaultInstance = new Settings();

		public static Settings Default {
			get { return defaultInstance; }
		}

		// No-op persistence for the console build.
		public void Save() { }

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
		public string ScriptFile { get; set; } = "";
		public string BackupFolderContent { get; set; } = "";
		public string BackupFolderBackup { get; set; } = "";

		public string ExtractMode { get; set; } = "";
		public string ConvertMode { get; set; } = "";

		public bool ExtractAllowImages { get; set; } = true;
		public bool ExtractAllowSounds { get; set; } = true;
		public bool ExtractAllowFonts { get; set; } = true;
		public bool ExtractAllowWaveBank { get; set; } = true;
		public bool ExtractUseInput { get; set; } = false;

		public bool ConvertAllowImages { get; set; } = true;
		public bool ConvertAllowSounds { get; set; } = true;
		public bool ConvertUseInput { get; set; } = false;
	}
}
