namespace TConvert.Properties {
	/**<summary>
	 * A portable replacement for the designer-generated TConvert.Properties.Resources
	 * used by the Windows build. The console build uses system tools (ffmpeg) and a
	 * managed LZX implementation instead of the embedded Windows binaries, so the
	 * resource byte arrays are no longer required.
	 *</summary>*/
	internal static class Resources {
		internal static byte[] ffmpeg {
			get { return new byte[0]; }
		}
		internal static byte[] xcompress32 {
			get { return new byte[0]; }
		}
	}
}
