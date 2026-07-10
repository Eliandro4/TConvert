/*******************************************************************************
 *	Copyright (C) 2017  sullerandras
 *	
 *	This program is free software: you can redistribute it and/or modify
 *	it under the terms of the GNU General Public License as published by
 *	the Free Software Foundation, either version 3 of the License, or
 *	(at your option) any later version.
 *	
 *	This program is distributed in the hope that it will be useful,
 *	but WITHOUT ANY WARRANTY; without even the implied warranty of
 *	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *	GNU General Public License for more details.
 *	
 *	You should have received a copy of the GNU General Public License
 *	along with this program.  If not, see <http://www.gnu.org/licenses/>.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TConvert.Util;

using ImageMagick;

namespace TConvert.Convert {
	/**<summary>A Png to Xnb Converter.</summary>*/
	public class PngConverter {
		//========== CONSTANTS ===========
		#region Constants

		private const string Texture2DType =
			"Microsoft.Xna.Framework.Content.Texture2DReader, " + 
			"Microsoft.Xna.Framework.Graphics, Version=4.0.0.0, " + 
			"Culture=neutral, PublicKeyToken=842cf8be1de50553";

		private const int HeaderSize = 3 + 1 + 1 + 1;
		private const int CompressedFileSize = 4;
		private const int TypeReaderCountSize = 1;
		private static readonly int TypeSize = 2 + Texture2DType.Length + 4;
		private const int SharedResourceCountSize = 1;
		private const int ObjectHeaderSize = 21;
		
		private static readonly int MetadataSize =
			HeaderSize + CompressedFileSize + TypeReaderCountSize +
			TypeSize + SharedResourceCountSize + ObjectHeaderSize;

		#endregion
		//========== CONVERTING ==========
		#region Converting

		/**<summary>Converts the specified input file and writes it to the output file.</summary>*/
		public static bool Convert(string inputFile, string outputFile, bool changeExtension, bool reach, bool premultiply) {
			if (changeExtension) {
				outputFile = Path.ChangeExtension(outputFile, ".xnb");
			}

			if (!Directory.Exists(Path.GetDirectoryName(inputFile)))
				throw new DirectoryNotFoundException("Could not find a part of the path '" + inputFile + "'.");
			else if (!File.Exists(inputFile))
				throw new FileNotFoundException("Could not find file '" + inputFile + "'.");

			int width;
			int height;
			byte[] rgba;

		using (MagickImage image = new MagickImage(inputFile)) {
				width = (int)image.Width;
				height = (int)image.Height;
				image.Format = MagickFormat.Rgba;
				rgba = image.ToByteArray(MagickFormat.Rgba);
			}
			// Magick returns RGBA; convert to the BGRA layout the GDI+ path uses
			// so WriteData's channel swap yields canonical RGBA XNB pixels.
			for (int i = 0; i < rgba.Length; i += 4) {
				byte b = rgba[i];
				rgba[i] = rgba[i + 2];
				rgba[i + 2] = b;
			}

			using (FileStream stream = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.Write)) {
				using (BinaryWriter writer = new BinaryWriter(stream)) {
					stream.SetLength(0);
				writer.Write(Encoding.UTF8.GetBytes("XNB"));    // format-identifier
				writer.Write(Encoding.UTF8.GetBytes("w"));      // target-platform
				writer.Write((byte)5);                          // xnb-format-version
				byte flagBits = 0;
				if (!reach) {
					flagBits |= 0x01;
				}
				writer.Write(flagBits);
				writer.Write(MetadataSize + width * height * 4);
				WriteData(writer, rgba, width, height, premultiply);
				}
			}
			return true;
		}

		#endregion
		//=========== WRITING ============
		#region Writing

		/**<summary>Write uncompressed image data.</summary>*/
		private static void WriteData(BinaryWriter writer, byte[] rgba, int width, int height, bool premultiply) {
			writer.Write7BitEncodedInt(1);                 // type-reader-count
			writer.Write7BitEncodedString(Texture2DType);  // type-reader-name
			writer.Write((int)0);                          // reader version number
			writer.Write7BitEncodedInt(0);                 // shared-resource-count

			// writing the image pixel data
			writer.Write((byte)1);
			writer.Write((int)0);
			writer.Write(width);
			writer.Write(height);
			writer.Write((int)1);
			writer.Write(width * height * 4);

			// rgba is in RGBA order; swap/premultiply channels in place.
			for (int i = 0; i < rgba.Length; i += 4) {
				// Always swap red and blue channels; premultiply alpha if requested
				int a = rgba[i + 3];
				if (!premultiply || a == 255) {
					// No premultiply necessary
					byte b = rgba[i];
					rgba[i] = rgba[i + 2];
					rgba[i + 2] = b;
				}
				else if (a != 0) {
					byte b = rgba[i];
					rgba[i] = (byte) (rgba[i + 2] * a / 255);
					rgba[i + 1] = (byte) (rgba[i + 1] * a / 255);
					rgba[i + 2] = (byte) (b * a / 255);
				}
				else {
					// alpha is zero, so just zero everything
					rgba[i] = 0;
					rgba[i + 1] = 0;
					rgba[i + 2] = 0;
				}
			}
			writer.Write(rgba);
		}

		#endregion
	}
}
