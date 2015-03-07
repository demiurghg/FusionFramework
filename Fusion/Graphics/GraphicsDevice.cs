using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;
using NvApiWrapper;
using Device = SharpDX.Direct3D11.Device;
using System.IO;
using Fusion.Graphics;
using Fusion.Graphics.Display;
using Fusion.Mathematics;


namespace Fusion.Graphics {

	public partial class GraphicsDevice : DisposableBase {

		public GraphicsProfile		GraphicsProfile		{ get; private set; }

		public readonly Game		Game;
		public Rectangle			DisplayBounds	{ get { return new Rectangle(0,0, display.Bounds.Width, display.Bounds.Height); } }

		public bool					FullScreen  { get { return display.Fullscreen; } set { display.Fullscreen = value; } }

		public event EventHandler	DisplayBoundsChanged;

		internal Device			Device			{ get { return device; } }			
		internal DeviceContext	DeviceContext	{ get { return deviceContext; } }	
		internal BaseDisplay	Display			{ get { return display; } }
		

		BaseDisplay				display			=	null;
		Device					device			=	null;
		DeviceContext			deviceContext	=	null;

		public RenderTarget2D	BackbufferColor	{ get { return display.BackbufferColor; } }
		public DepthStencil2D	BackbufferDepth	{ get { return display.BackbufferDepth; } }


		//LayoutManager			layoutManager	=	null;

		HashSet<IDisposable>	toDispose = new HashSet<IDisposable>();

		public SamplerStateCollection	PixelShaderSamplers		{ get; private set; }	
		public SamplerStateCollection	VertexShaderSamplers	{ get; private set; }	
		public SamplerStateCollection	GeometryShaderSamplers	{ get; private set; }	
		public SamplerStateCollection	ComputeShaderSamplers	{ get; private set; }	
		public SamplerStateCollection	DomainShaderSamplers	{ get; private set; }	
		public SamplerStateCollection	HullShaderSamplers		{ get; private set; }	

		public ShaderResourceCollection	PixelShaderResources	{ get; private set; }	
		public ShaderResourceCollection	VertexShaderResources	{ get; private set; }	
		public ShaderResourceCollection	GeometryShaderResources	{ get; private set; }	
		public ShaderResourceCollection	ComputeShaderResources	{ get; private set; }	
		public ShaderResourceCollection	DomainShaderResources	{ get; private set; }	
		public ShaderResourceCollection	HullShaderResources		{ get; private set; }	

		public ConstantBufferCollection	PixelShaderConstants	{ get; private set; }	
		public ConstantBufferCollection	VertexShaderConstants	{ get; private set; }	
		public ConstantBufferCollection	GeometryShaderConstants	{ get; private set; }	
		public ConstantBufferCollection	ComputeShaderConstants	{ get; private set; }	
		public ConstantBufferCollection	DomainShaderConstants	{ get; private set; }	
		public ConstantBufferCollection	HullShaderConstants		{ get; private set; }	

		internal ShaderFactory ShaderFactory { get { return shaderFactory; } }
		ShaderFactory shaderFactory;

		/// <summary>
		/// 
		/// </summary>
		public GraphicsDevice ( Game game )
		{
			this.Game	=	game;
		}




		/// <summary>
		/// Initializes graphics device
		/// </summary>
		internal void Initialize ( GameParameters parameters )
		{
			this.GraphicsProfile	=	parameters.GraphicsProfile;

			try {
				if (parameters.StereoMode==StereoMode.Disabled) 	display	=	new GenericDisplay( Game, this, parameters ); else
				if (parameters.StereoMode==StereoMode.NV3DVision)	display	=	new NV3DVisionDisplay( Game, this, parameters ); else 
				if (parameters.StereoMode==StereoMode.DualHead)		display	=	new StereoDualHeadDisplay( Game, this, parameters ); else 
				if (parameters.StereoMode==StereoMode.Interlaced)	display	=	new StereoInterlacedDisplay( Game, this, parameters ); else 
				if (parameters.StereoMode==StereoMode.OculusRift)	display	=	new OculusRiftDisplay( Game, this, parameters ); else 
				//if (parameters.StereoMode==StereoMode.OculusRift)	display	=	new NV3DVisionDisplay( Game, this, parameters ); else 
					throw new ArgumentException("parameters.StereoMode");
			} catch ( Exception e ) {
				Log.Warning("Failed to intialize graphics device.");
				Log.Warning("{0}", e.Message );
				Log.Warning("Attempt to use default parameters...");

				parameters.FullScreen	=	false;
				parameters.StereoMode	=	StereoMode.Disabled;

				display	=	new GenericDisplay( Game, this, parameters ); 
			}
			

			device			=	display.d3dDevice;
			deviceContext	=	device.ImmediateContext;

			shaderFactory		=	new ShaderFactory( this );

			display.CreateDisplayResources();


			//
			//	create color buffer :
			//	
			PixelShaderResources	=	new ShaderResourceCollection( this, DeviceContext.PixelShader		);
			VertexShaderResources 	=	new ShaderResourceCollection( this, DeviceContext.VertexShader		);
			GeometryShaderResources 	=	new ShaderResourceCollection( this, DeviceContext.GeometryShader	);
			ComputeShaderResources 	=	new ShaderResourceCollection( this, DeviceContext.ComputeShader		);
			DomainShaderResources 	=	new ShaderResourceCollection( this, DeviceContext.DomainShader		);
			HullShaderResources 	=	new ShaderResourceCollection( this, DeviceContext.HullShader		);

			PixelShaderSamplers		=	new SamplerStateCollection	( this, DeviceContext.PixelShader		);
			VertexShaderSamplers		=	new SamplerStateCollection	( this, DeviceContext.VertexShader		);
			GeometryShaderSamplers		=	new SamplerStateCollection	( this, DeviceContext.GeometryShader	);
			ComputeShaderSamplers		=	new SamplerStateCollection	( this, DeviceContext.ComputeShader		);
			DomainShaderSamplers		=	new SamplerStateCollection	( this, DeviceContext.DomainShader		);
			HullShaderSamplers		=	new SamplerStateCollection	( this, DeviceContext.HullShader		);

			PixelShaderConstants	=	new ConstantBufferCollection( this, DeviceContext.PixelShader		);
			VertexShaderConstants	=	new ConstantBufferCollection( this, DeviceContext.VertexShader		);
			GeometryShaderConstants	=	new ConstantBufferCollection( this, DeviceContext.GeometryShader	);
			ComputeShaderConstants	=	new ConstantBufferCollection( this, DeviceContext.ComputeShader		);
			DomainShaderConstants	=	new ConstantBufferCollection( this, DeviceContext.DomainShader		);
			HullShaderConstants	=	new ConstantBufferCollection( this, DeviceContext.HullShader		);
		}



		/// <summary>
		/// 
		/// </summary>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {

				SafeDispose( ref shaderFactory );

				deviceContext.Flush();
				SafeDispose( ref deviceContext );
				SafeDispose( ref display );

				SamplerState.DisposeStates();
				RasterizerState.DisposeStates();
				DepthStencilState.DisposeStates();
				BlendState.DisposeStates();
			}

			base.Dispose(disposing);
		}



		internal string requestScreenShotPath = null;



		/// <summary>
		/// Makes screenshot and saves image at specified path.
		/// If path is null image will be stored at MyPicures\AppName\Shot-CurrentDate.bmp
		/// </summary>
		/// <param name="path"></param>
		public void Screenshot ( string path = null )
		{
			if (path==null) {
				string userImgs = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
				string appName	= Path.GetFileNameWithoutExtension( AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "") );
				string fileName = userImgs + @"\" + appName + @"\Screenshots\Shot-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff") + ".bmp";
				Directory.CreateDirectory( Path.GetDirectoryName( fileName ) );

				path	=	fileName;
			}

			requestScreenShotPath	=	path;
		}



		/// <summary>
		/// 
		/// </summary>
		internal void NotifyViewportChanges ()
		{
			if (DisplayBoundsChanged!=null) {
				DisplayBoundsChanged( this, EventArgs.Empty );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		internal void Present ()
		{
			if (requestScreenShotPath != null ) {

				var path = requestScreenShotPath;
				requestScreenShotPath = null;

				BackbufferColor.SaveToFile( path );
			}

			display.SwapBuffers( Game.Parameters.VSyncInterval );

			display.Update();
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 * 	Input assembler stuff :
		 *	
		-----------------------------------------------------------------------------------------*/

		internal bool	vertexInputDirty		= true;
		internal bool	vertexOutputDirty		= true;
		internal string vertexShaderSignature	= null;

		D3D.Buffer[]		inputVertexBuffers	= null;
		D3D.Buffer			inputIndexBuffer	= null;
		int[]				inputVertexOffsets	= null;
		int[]				inputVertexStrides	= null;
		VertexInputLayout 	inputVertexLayout	= null;

		StreamOutputBufferBinding[] outputBinding		= null;
		StreamOutputBufferBinding[] outputBindingAppend = null;
		VertexOutputLayout	outputVertexLayout	= null;

		int[] zeroIntBuffer = new[]{ 0,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,0 };

		StreamOutputBufferBinding[] nullSO = 
			new StreamOutputBufferBinding[] {
				new StreamOutputBufferBinding( null, 0 ),
				new StreamOutputBufferBinding( null, 0 ),
				new StreamOutputBufferBinding( null, 0 ),
				new StreamOutputBufferBinding( null, 0 ),
			};


		/// <summary>
		/// 
		/// </summary>
		void ApplyVertexStage()
		{
			if (vertexOutputDirty) {
				if (outputBinding!=null && outputVertexLayout!=null ) {
					
					if (gs==null) {
						deviceContext.StreamOutput.SetTargets( nullSO );
						throw new InvalidOperationException("Geometry shader must be set to use vertex output.");
					} else {
						deviceContext.StreamOutput.SetTargets( outputBinding );
						outputBinding = outputBindingAppend;
						DeviceContext.GeometryShader.Set( outputVertexLayout.GetStreamOutputGeometryShader(gs) );
					}

				} else {
					var oldGS = GeometryShader;
					GeometryShader = null;
					GeometryShader = oldGS;
					deviceContext.StreamOutput.SetTargets( nullSO );
				}// */

				vertexOutputDirty	=	false;
			}


			if (vertexInputDirty) {

				if (inputVertexBuffers!=null && inputVertexLayout!=null) {
					deviceContext.InputAssembler.InputLayout	=	inputVertexLayout.GetInputLayout( vertexShaderSignature );
					deviceContext.InputAssembler.SetVertexBuffers( 0, inputVertexBuffers, inputVertexStrides, inputVertexOffsets );
				} else {
					deviceContext.InputAssembler.InputLayout	=	null;
					deviceContext.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( null, 0, 0 ) );
				}

				if (inputIndexBuffer!=null) {
					deviceContext.InputAssembler.SetIndexBuffer( inputIndexBuffer, DXGI.Format.R32_UInt, 0 );
				} else {	
					deviceContext.InputAssembler.SetIndexBuffer( null, Format.Unknown, 0 );
				}

				vertexInputDirty	=	false;
			}
		}



		/// <summary>
		/// Setups vertex input 
		/// </summary>
		/// <param name="vb">Vertex buffer</param>
		/// <param name="ib">Index buffer, can be null</param>
		/// <param name="primitive">Primitive to draw</param>
		/// <param name="signature">Input signature</param>
		public void SetupVertexInput ( VertexInputLayout layout, VertexBuffer vb, IndexBuffer ib )
		{
			vertexInputDirty	=	true;

			inputVertexLayout	=	layout;
			inputVertexBuffers	=	vb == null ? null : new[]{ vb.Buffer };
			inputVertexOffsets	=	new[]{ 0 };
			inputVertexStrides	=	new[]{ vb == null ? 0 : vb.Stride };
			inputIndexBuffer	=	ib == null ? null : ib.Buffer;
		}



		/// <summary>
		/// Setups vertex input.
		/// </summary>
		/// <param name="vertexBuffers">Array of vertex buffers to bind. Null means no VBs to bind.</param>
		/// <param name="offsets">Array of vertex offsets. Null means that all offsets are zero.</param>
		/// <param name="instancingRate">Array of instance rates. Zero instance rates means that data is per-vertex.
		/// Null means that all instance rates are zero.</param>
		/// <param name="indexBuffer">Index buffer to bind. Null means no IB to bind.</param>
		/// <param name="vertexType">Vertex type that describes input layout.</param>
		public void SetupVertexInput ( VertexInputLayout layout, VertexBuffer[] vertexBuffers, int[] offsets, IndexBuffer indexBuffer )
		{
			vertexInputDirty	=	true;

			inputVertexLayout	=	layout;

			if (vertexBuffers!=null) {
				inputVertexBuffers	=	vertexBuffers.Select( vb => vb.Buffer ).ToArray();
				inputVertexStrides	=	vertexBuffers.Select( vb => vb.Stride ).ToArray();
			} else {
				inputVertexBuffers	=	null;
				inputVertexStrides	=	zeroIntBuffer;
			}

			if (offsets==null) {
				inputVertexOffsets	= zeroIntBuffer;
			} else {
				inputVertexOffsets	= offsets;
			}

			inputIndexBuffer	=	indexBuffer == null ? null : indexBuffer.Buffer;
		} 


		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="layout"></param>
		/// <param name="vertexBuffer"></param>
		/// <param name="offset"></param>
		public void SetupVertexOutput ( VertexOutputLayout layout, VertexBuffer vertexBuffer, int offset )
		{
			SetupVertexOutput( layout, new[]{ vertexBuffer }, new[]{ offset } );
		}



		/// <summary>
		/// Setups vertex (stream) output.
		/// </summary>
		/// <param name="vertexBuffers">Vertex buffer to write data.</param>
		/// <param name="offset">Offset where to start writing data. -1 means append.</param>
		public void SetupVertexOutput ( VertexOutputLayout layout, VertexBuffer[] vertexBuffers, int[] offsets )
		{
			vertexOutputDirty	=	true;

			if (layout==null || vertexBuffers==null || offsets==null ) {
				outputBinding		=	null;
				return;
			}

			if (vertexBuffers.Length>4) {
				throw new ArgumentException("Length of 'vertexBuffers' must be less than 4");
			}

			if (vertexBuffers.Length!=offsets.Length) {
				throw new ArgumentException("Lengths of 'vertexBuffers' and 'offsets' must be the same.");
			}

			if ( !offsets.All( e => (e<0) || (e%4==0) ) ) {
				throw new ArgumentException("SetupVertexOutput: Offsets must be multiple of 4.");
			}

			if ( !vertexBuffers.All( vb => vb.IsVertexOutputEnabled) ) {
				throw new GraphicsException("SetupVertexOutput: Vertex buffer must be created with enabled vertex output.");
			}

			outputVertexLayout	=	layout;
			outputBinding		=	vertexBuffers.Zip( offsets, (vb,offset) => new StreamOutputBufferBinding( vb.Buffer, offset ) ).ToArray();
			outputBindingAppend	=	vertexBuffers.Zip( offsets, (vb,offset) => new StreamOutputBufferBinding( vb.Buffer, -1 ) ).ToArray();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="threadGroupCountX"></param>
		/// <param name="threadGroupCountY"></param>
		/// <param name="threadGroupCountZ"></param>
		public void Dispatch( int threadGroupCountX, int threadGroupCountY = 1, int threadGroupCountZ = 1 )
		{
			deviceContext.Dispatch( threadGroupCountX, threadGroupCountY, threadGroupCountZ ); 
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertexCount"></param>
		/// <param name="vertexFirstIndex"></param>
		public void DrawAuto ( Primitive primitive )
		{									 
			ApplyVertexStage();
			deviceContext.InputAssembler.PrimitiveTopology	=	Converter.Convert( primitive );
			deviceContext.DrawAuto();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertexCount"></param>
		/// <param name="vertexFirstIndex"></param>
		public void Draw ( Primitive primitive, int vertexCount, int firstIndex )
		{									 
			ApplyVertexStage();
			deviceContext.InputAssembler.PrimitiveTopology	=	Converter.Convert( primitive );
			deviceContext.Draw( vertexCount, firstIndex );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="primitive"></param>
		/// <param name="vertexCountPerInstance"></param>
		/// <param name="instanceCount"></param>
		/// <param name="startVertexLocation"></param>
		/// <param name="startInstanceLocation"></param>
		public void DrawInstanced ( Primitive primitive, int vertexCountPerInstance, int instanceCount, int startVertexLocation, int startInstanceLocation )
		{
			ApplyVertexStage();
			deviceContext.InputAssembler.PrimitiveTopology	=	Converter.Convert( primitive );
			deviceContext.DrawInstanced( vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="indexCount"></param>
		/// <param name="firstIndex"></param>
		/// <param name="baseVertexOffset"></param>
		public void DrawIndexed ( Primitive primitive, int indexCount, int firstIndex, int baseVertexOffset )
		{
			ApplyVertexStage();
			deviceContext.InputAssembler.PrimitiveTopology	=	Converter.Convert( primitive );
			deviceContext.DrawIndexed( indexCount, firstIndex,	baseVertexOffset );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="indexCount"></param>
		/// <param name="firstIndex"></param>
		/// <param name="baseVertexOffset"></param>
		public void DrawInstancedIndexed ( Primitive primitive, int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation )
		{
			ApplyVertexStage();
			deviceContext.InputAssembler.PrimitiveTopology	=	Converter.Convert( primitive );
			deviceContext.DrawIndexedInstanced( indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation );
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 * 	State control
		 *	
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// Resets all? including RTs and DS
		/// </summary>
		public void ResetStates ()
		{
			DeviceContext.ClearState();
			
			//	Kill cached value :
			vertexInputDirty	=	false;
			vertexOutputDirty	=	false;

			blendState	=	null;
			rasterState	=	null;
			depthState	=	null;

			ps	=	null;
			vs	=	null;
			gs	=	null;
			cs	=	null;
			ds	=	null;
			hs	=	null;

			vertexInputDirty		= true;
			vertexOutputDirty		= true;
			vertexShaderSignature	= null;

			inputVertexBuffers	= null;
			inputIndexBuffer	= null;
			inputVertexOffsets	= null;
			inputVertexStrides	= null;
			inputVertexLayout	= null;

			outputBinding		= null;
		}



		BlendState			blendState	=	null;
		RasterizerState		rasterState	=	null;
		DepthStencilState	depthState	=	null;



		/// <summary>
		/// Sets and gets blend state.
		/// </summary>
		public BlendState BlendState {
			set {
				if (value==null) {
					throw new ArgumentNullException();
				}
				if (blendState!=value) {
					blendState = value;
					blendState.Apply( this );
				}
			} 
			get {
				return blendState;
			}
		}



		/// <summary>
		/// Sets and gets rasterizer state.
		/// </summary>
		public RasterizerState RasterizerState {
			set {
				if (value==null) {
					throw new ArgumentNullException();
				}
				if (rasterState!=value) {
					rasterState = value;
					rasterState.Apply( this );
				}
			}
			get {
				return rasterState;
			}
		} 



		/// <summary>
		/// Sets and gets depth stencil state.
		/// </summary>
		public DepthStencilState DepthStencilState {
			set {
				if (value==null) {
					throw new ArgumentNullException();
				}
				if (depthState!=value) {
					depthState = value;
					depthState.Apply(this);
				}
			}
			get {
				return depthState;
			}
		}



		PixelShader		ps;
		VertexShader	vs;
		GeometryShader	gs;
		ComputeShader	cs;
		DomainShader	ds;
		HullShader		hs;

		public PixelShader PixelShader {
			set { 
				if (ps!=value) { 
					ps=value; 
					deviceContext.PixelShader.Set( value==null? null:value.Shader ); 
				} 
			}
			get { 
				return ps; 
			}
		}


		public VertexShader VertexShader {
			set { 
				if (vs!=value) { 
					vs = value;
					vertexInputDirty = true;
					if (value==null) {
						vertexShaderSignature = null;
						deviceContext.VertexShader.Set( null );
					} else {
						vertexShaderSignature = value.Bytecode;
						deviceContext.VertexShader.Set( value.Shader ); 
					}
				} 
			}
			get { 
				return vs; 
			}
		}


		public GeometryShader GeometryShader {
			set { 
				if (gs!=value) { 
					gs=value; 
					vertexOutputDirty = true;
					deviceContext.GeometryShader.Set( value==null? null:value.Shader ); 
				} 
			}
			get { 
				return gs; 
			}
		}


		public ComputeShader ComputeShader {
			set { 
				if (cs!=value) { 
					cs=value; 
					deviceContext.ComputeShader.Set( value==null? null:value.Shader ); 
				} 
			}
			get { 
				return cs; 
			}
		}


		public DomainShader DomainShader {
			set { 
				if (ds!=value) { 
					ds=value; 
					deviceContext.DomainShader.Set( value==null? null:value.Shader ); 
				} 
			}
			get { 
				return ds; 
			}
		}


		public HullShader HullShader {
			set { 
				if (hs!=value) { 
					hs=value; 
					deviceContext.HullShader.Set( value==null? null:value.Shader ); 
				} 
			}
			get { 
				return hs; 
			}
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Targets :
		 * 
		-----------------------------------------------------------------------------------------*/

		RenderTargetSurface[]	renderTargetSurfaces	=	new RenderTargetSurface[8];
		DepthStencilSurface		depthStencilSurface		=	null;


		/// <summary>
		/// 
		/// </summary>
		public void RestoreBackbuffer ()
		{
			SetTargets( BackbufferDepth, BackbufferColor );

			deviceContext.Rasterizer.SetViewport( SharpDXHelper.Convert( new ViewportF( 0,0, BackbufferColor.Width, BackbufferColor.Height ) ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="color"></param>
		public void ClearBackbuffer ( Color4 color, float depth = 1, byte stencil = 0 )
		{
			Clear( BackbufferColor.Surface, color );
			Clear( BackbufferDepth.Surface, depth, stencil );
		}



		/// <summary>
		/// Analogue of SetTargets( depthStencil.Surface, renderTarget.Surface )
		/// </summary>
		/// <param name="depthStencil"></param>
		/// <param name="renderTarget"></param>
		public void SetTargets ( DepthStencil2D depthStencil, RenderTarget2D renderTarget )
		{
			SetTargets( depthStencil==null?null:depthStencil.Surface, renderTarget==null?null:renderTarget.Surface );
		}



		/// <summary>
		/// Sets targets
		/// </summary>
		/// <param name="renderTargets"></param>
		public void SetTargets ( DepthStencilSurface depthStencil, params RenderTargetSurface[] renderTargets )
		{
			int w = -1;
			int h = -1;

			if (renderTargets.Length>8) {
				throw new ArgumentException("Could not bind more than 8 render targets");
			}


			this.depthStencilSurface	=	depthStencil;
			renderTargets.CopyTo( renderTargetSurfaces, 0 );


			if (depthStencil!=null) {
				w	=	depthStencil.Width;
				h	=	depthStencil.Height;
			}

			if (renderTargets.Any()) {
				
				if (w==-1 || h==-1) {
					w	=	renderTargets.First().Width;
					h	=	renderTargets.First().Height;
				}
				
				if ( !renderTargets.All( surf => surf.Width == w && surf.Height == h ) ) {
					throw new ArgumentException("All surfaces must be the same size", "renderTargets");
				}
			}

			DepthStencilView	dsv		=	depthStencil == null ? null : depthStencil.DSV;
			RenderTargetView[] 	rtvs	=	renderTargets.Select( rt => rt.RTV ).ToArray();

			if (!rtvs.Any()) {
				deviceContext.OutputMerger.SetTargets( dsv, (RenderTargetView)null );
			} else {
				deviceContext.OutputMerger.SetTargets( dsv, rtvs );
			}

			SetViewport( 0, 0, w, h );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="depthStencil"></param>
		/// <param name="renderTargets"></param>
		public void GetTargets ( out DepthStencilSurface depthStencil, out RenderTargetSurface[] renderTargets )
		{
			depthStencil	=	depthStencilSurface;
			renderTargets	=	renderTargetSurfaces.ToArray();
		}



		/// <summary>
		/// Clear depth stencil surface
		/// </summary>
		/// <param name="surface"></param>
		/// <param name="depth"></param>
		/// <param name="stencil"></param>
		public void Clear ( DepthStencilSurface surface, float depth = 1, byte stencil = 0 )
		{
			deviceContext.ClearDepthStencilView( surface.DSV, DepthStencilClearFlags.Depth|DepthStencilClearFlags.Stencil, depth, stencil );
		}



		/// <summary>
		/// Clears render target using given color
		/// </summary>
		/// <param name="surface"></param>
		/// <param name="color"></param>
		public void Clear ( RenderTargetSurface surface, Color4 color )
		{
			deviceContext.ClearRenderTargetView( surface.RTV, SharpDXHelper.Convert( color ) );
		}



		/// <summary>
		/// Fills structured buffer with given values
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="values"></param>
		public void Clear ( StructuredBuffer buffer, Int4 values )
		{
			deviceContext.ClearUnorderedAccessView( buffer.UAV, SharpDXHelper.Convert( values ) );
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		public void Resolve ( RenderTarget2D source, RenderTarget2D destination )
		{
			Resolve( source.Surface, destination.Surface );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		public void Resolve ( RenderTargetSurface source, RenderTargetSurface destination )
		{
			if ( source.Width != destination.Width || source.Height != destination.Height ) {
				throw new GraphicsException( "Could not resolve: source and destination are not the same size");
			}

			if ( source.SampleCount <= 1 ) {
				throw new GraphicsException( "Could not resolve: source surface is not multisampled");
			}

			if ( destination.SampleCount > 1 ) {
				throw new GraphicsException( "Could not resolve: destination surface is multisampled");
			}

			deviceContext.ResolveSubresource( source.Resource, source.Subresource, destination.Resource, destination.Subresource, Converter.Convert( source.Format ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="w"></param>
		/// <param name="h"></param>
		public void SetViewport ( int x, int y, int w, int h )
		{
			deviceContext.Rasterizer.SetViewport( SharpDXHelper.Convert( new ViewportF(x,y,w,h) ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="w"></param>
		/// <param name="h"></param>
		public void SetViewport ( Viewport viewport )
		{
			deviceContext.Rasterizer.SetViewport( SharpDXHelper.Convert( new ViewportF( viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinDepth, viewport.MaxDepth ) ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="w"></param>
		/// <param name="h"></param>
		public void SetViewport ( ViewportF viewport )
		{
			deviceContext.Rasterizer.SetViewport( SharpDXHelper.Convert( viewport ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="register"></param>
		/// <param name="buffer"></param>
		/// <param name="initialCount">An array of append and consume buffer offsets. 
		/// A value of -1 indicates to keep the current offset. 
		/// Any other values set the hidden counter for that appendable and consumable UAV. </param>
		public void SetCSRWBuffer ( int register, StructuredBuffer buffer, int initialCount = -1 ) 
		{ 
			if (register>8) {
				throw new GraphicsException("Could not bind RW buffer at register " + register.ToString() + " (max 8)");
			}

			DeviceContext.ComputeShader.SetUnorderedAccessView( register, buffer==null?null:buffer.UAV, initialCount ); 
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="register"></param>
		/// <param name="buffer"></param>
		/// <param name="initialCount">An array of append and consume buffer offsets. 
		/// A value of -1 indicates to keep the current offset. 
		/// Any other values set the hidden counter for that appendable and consumable UAV. </param>
		public void SetPSRWBuffer ( int register, StructuredBuffer buffer, int initialCount = -1 ) 
		{ 
			if (register>8) {
				throw new GraphicsException("Could not bind RW buffer at register " + register.ToString() + " (max 8)");
			}

			DeviceContext.OutputMerger.SetUnorderedAccessView ( register, buffer==null?null:buffer.UAV, initialCount ); 
		}



		/// <summary>
		/// SetCSRWBuffer & SetCSRWTexture share same registers.
		/// </summary>
		/// <param name="register"></param>
		/// <param name="buffer"></param>
		/// <param name="initialCount">An array of append and consume buffer offsets. 
		/// A value of -1 indicates to keep the current offset. 
		/// Any other values set the hidden counter for that appendable and consumable UAV. </param>
		public void SetCSRWTexture ( int register, RenderTargetSurface surface ) 
		{ 
			if (register>8) {
				throw new GraphicsException("Could not bind RW texture at register " + register.ToString() + " (max 8)");
			}

			DeviceContext.ComputeShader.SetUnorderedAccessView( register, surface==null?null:surface.UAV, -1 ); 
		}



		/// <summary>
		/// SetPSRWBuffer & SetPSRWTexture share same registers.
		/// </summary>
		/// <param name="register"></param>
		/// <param name="buffer"></param>
		/// <param name="initialCount">An array of append and consume buffer offsets. 
		/// A value of -1 indicates to keep the current offset. 
		/// Any other values set the hidden counter for that appendable and consumable UAV. </param>
		public void SetPSRWTexture ( int register, RenderTargetSurface surface ) 
		{ 
			if (register>8) {
				throw new GraphicsException("Could not bind RW texture at register " + register.ToString() + " (max 8)");
			}

			DeviceContext.OutputMerger.SetUnorderedAccessView ( register, surface==null?null:surface.UAV, -1 ); 
		}
	}
}
