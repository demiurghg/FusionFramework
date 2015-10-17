using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using SharpDX.X3DAudio;
using Fusion.Core.Content;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Audio {
	[ContentLoader(typeof(SoundEffect))]
	public class SoundEffectLoader : ContentLoader {

		public override object Load ( GameEngine game, Stream stream, Type requestedType, string assetPath )
		{
		#if false
			return new SoundEffect( stream );
		#else

			BinaryReader reader = new BinaryReader(stream);

			int chunkID			= reader.ReadInt32();
			int fileSize		= reader.ReadInt32();
			int riffType		= reader.ReadInt32();
			int fmtID			= reader.ReadInt32();
			int fmtSize			= reader.ReadInt32();
			int fmtCode			= reader.ReadInt16();
			int channels		= reader.ReadInt16();
			int sampleRate		= reader.ReadInt32();
			int fmtAvgBPS		= reader.ReadInt32();
			int fmtBlockAlign	= reader.ReadInt16();
			int bitDepth		= reader.ReadInt16();

			//Log.Message("chunkID        = {0}", chunkID		 );
			//Log.Message("fileSize       = {0}", fileSize		 );
			//Log.Message("riffType       = {0}", riffType		 );
			//Log.Message("fmtID          = {0}", fmtID			 );
			//Log.Message("fmtSize        = {0}", fmtSize		 );
			//Log.Message("fmtCode        = {0}", fmtCode		 );
			//Log.Message("channels       = {0}", channels		 );
			//Log.Message("sampleRate     = {0}", sampleRate	 );
			//Log.Message("fmtAvgBPS      = {0}", fmtAvgBPS		 );
			//Log.Message("fmtBlockAlign  = {0}", fmtBlockAlign	 );
			//Log.Message("bitDepth       = {0}", bitDepth		 );

			if (fmtSize == 18) {
				// Read any extra values
				int fmtExtraSize = reader.ReadInt16();
				reader.ReadBytes(fmtExtraSize);
			}

			int dataID = reader.ReadInt32();
			int dataSize = reader.ReadInt32();

			//Log.Message("dataID         = {0}", dataID );
			//Log.Message("dataSize       = {0}", dataSize );

			var byteArray = reader.ReadBytes(dataSize);

			var sampleCount	=	byteArray.Length * 8 / bitDepth / channels;

		//	return new SoundEffect( byteArray, 0, byteArray.Length, sampleRate, (AudioChannels)channels, 0, sampleCount );
			return new SoundEffect( game.AudioDevice, byteArray, 0, byteArray.Length, sampleRate, (AudioChannels)channels, 0, sampleCount );

		#endif
		}
	}
		
}
