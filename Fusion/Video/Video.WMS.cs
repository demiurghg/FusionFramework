using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Graphics;
using Fusion.Mathematics;
using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;

namespace Fusion.Video
{
	public sealed partial class Video : IDisposable
	{
		private Topology _topology;
		internal Topology Topology { get { return _topology; } }

		internal VideoSampleGrabber SampleGrabber { get; private set; }

		MediaType _mediaType;

		public Texture2D VideoFrame { get; private set; }


		private void PlatformInitialize()
		{
			if (Topology != null)
				return;

			//MediaManagerState.CheckStartup();

			MediaFactory.CreateTopology(out _topology);

			SharpDX.MediaFoundation.MediaSource mediaSource;
			{
				SourceResolver resolver = new SourceResolver();
				
				ObjectType otype;
				ComObject source = resolver.CreateObjectFromURL(FileName, SourceResolverFlags.MediaSource, null, out otype);
				mediaSource = source.QueryInterface<SharpDX.MediaFoundation.MediaSource>();

				
				resolver.Dispose();
				source.Dispose();
			}


			PresentationDescriptor presDesc;
			mediaSource.CreatePresentationDescriptor(out presDesc);
			
			for (var i = 0; i < presDesc.StreamDescriptorCount; i++)
			{
				RawBool selected = false;
				StreamDescriptor desc;
				presDesc.GetStreamDescriptorByIndex(i, out selected, out desc);
				
				if (selected)
				{
					TopologyNode sourceNode;
					MediaFactory.CreateTopologyNode(TopologyType.SourceStreamNode, out sourceNode);

					sourceNode.Set(TopologyNodeAttributeKeys.Source, mediaSource);
					sourceNode.Set(TopologyNodeAttributeKeys.PresentationDescriptor, presDesc);
					sourceNode.Set(TopologyNodeAttributeKeys.StreamDescriptor, desc);
					

					TopologyNode outputNode;
					MediaFactory.CreateTopologyNode(TopologyType.OutputNode, out outputNode);

					var majorType = desc.MediaTypeHandler.MajorType;
					
					if (majorType == MediaTypeGuids.Video)
					{
						Activate activate;
						
						SampleGrabber = new VideoSampleGrabber();

						_mediaType = new MediaType();
						
						_mediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);

						// Specify that we want the data to come in as RGB32.
						_mediaType.Set(MediaTypeAttributeKeys.Subtype, new Guid("00000016-0000-0010-8000-00AA00389B71"));

						MediaFactory.CreateSampleGrabberSinkActivate(_mediaType, SampleGrabber, out activate);
						outputNode.Object = activate;


						long frameSize = desc.MediaTypeHandler.CurrentMediaType.Get<long>(MediaTypeAttributeKeys.FrameSize);

						Width	= (int)(frameSize >> 32);
						Height	= (int) (frameSize & 0x0000FFFF);
					}

					if (majorType == MediaTypeGuids.Audio)
					{
						Activate activate;
						MediaFactory.CreateAudioRendererActivate(out activate);

						outputNode.Object = activate;
					}

					_topology.AddNode(sourceNode);
					_topology.AddNode(outputNode);
					sourceNode.ConnectOutput(0, outputNode, 0);
					

					Duration = new TimeSpan(presDesc.Get<long>(PresentationDescriptionAttributeKeys.Duration));
					

					sourceNode.Dispose();
					outputNode.Dispose();
				}

				desc.Dispose();
			}

			presDesc.Dispose();
			mediaSource.Dispose();


			VideoFrame = new Texture2D(Game.Instance.GraphicsDevice, Width, Height, ColorFormat.Bgra8, false);
		}

		private void PlatformDispose(bool disposing)
		{
			if (_topology != null)
			{
				_topology.Dispose();
				_topology = null;
			}

			if (SampleGrabber != null)
			{
				SampleGrabber.Dispose();
				SampleGrabber = null;
			}
		}
	}
}
