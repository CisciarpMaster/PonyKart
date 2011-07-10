using Mogre;

namespace Lymph.Handlers {
	/// <summary>
	/// This class just adds glow to things. I'm not entirely sure how it works but it's best not to touch it unless you know what you're doing
	/// </summary>
	public class GlowHandler {

		public GlowHandler(SceneManager sceneMgr)
		{
			sceneMgr.RenderQueueStarted += new RenderQueueListener.RenderQueueStartedHandler(RenderQueueStarted);
			sceneMgr.RenderQueueEnded += new RenderQueueListener.RenderQueueEndedHandler(RenderQueueEnded);
		}
		

		////////////////////////////////////////////////////////////////////////////////////
		//                                                                                //
		//  This is all stuff for the "glow" effect                                       //
		//  See http://www.ogre3d.org/addonforums/viewtopic.php?f=8&t=3987 for more info  //
		//                                                                                //
		////////////////////////////////////////////////////////////////////////////////////

		public const byte RENDER_QUEUE_OUTER_GLOW = (byte)RenderQueueGroupID.RENDER_QUEUE_MAIN + 4;
		public const byte RENDER_QUEUE_INNER_GLOW = (byte)RenderQueueGroupID.RENDER_QUEUE_MAIN + 3;
		public const byte LAST_STENCIL_OP_RENDER_QUEUE = RENDER_QUEUE_INNER_GLOW;
		//public const byte RENDER_QUEUE_GLOW_OBJECTS = (byte)RenderQueueGroupID.RENDER_QUEUE_MAIN + 1;
		public const int STENCIL_VALUE_FOR_FULL_GLOW = 2;
		public const int STENCIL_VALUE_FOR_OUTLINE_GLOW = 1;
		public const uint STENCIL_FULL_MASK = 0xFFFFFFFF;

		public void RenderQueueEnded(byte queueGroupId, string invocation, out bool skipThisInvocation)
		{
			skipThisInvocation = false;
			if (queueGroupId == LAST_STENCIL_OP_RENDER_QUEUE)
			{
				RenderSystem rendersys = Root.Singleton.RenderSystem;
				rendersys.SetStencilCheckEnabled(false);
				rendersys.SetStencilBufferParams();
			}
		}

		public void RenderQueueStarted(byte queueGroupId, string invocation, out bool skipThisInvocation)
		{
			skipThisInvocation = false;
			RenderSystem rendersys = Root.Singleton.RenderSystem;

			/*if (queueGroupId == RENDER_QUEUE_GLOW_OBJECTS) // outline glow object
			{
				Launch.Log("Render_Queue_Glow_Objects");
				rendersys.ClearFrameBuffer((uint)FrameBufferType.FBT_STENCIL);
				rendersys.SetStencilCheckEnabled(true);
				rendersys.SetStencilBufferParams(CompareFunction.CMPF_ALWAYS_PASS,
												 STENCIL_VALUE_FOR_OUTLINE_GLOW,
												 STENCIL_FULL_MASK,
												 StencilOperation.SOP_KEEP,
												 StencilOperation.SOP_KEEP,
												 StencilOperation.SOP_REPLACE,
												 false);
			}*/

			if (queueGroupId == RENDER_QUEUE_OUTER_GLOW)  // full glow - alpha glow
			{
				rendersys.SetStencilCheckEnabled(true);
				rendersys.SetStencilBufferParams(CompareFunction.CMPF_ALWAYS_PASS,
												 STENCIL_VALUE_FOR_FULL_GLOW,
												 STENCIL_FULL_MASK,
												 StencilOperation.SOP_KEEP,
												 StencilOperation.SOP_KEEP,
												 StencilOperation.SOP_REPLACE,
												 false);
			}

			if (queueGroupId == RENDER_QUEUE_INNER_GLOW)  // full glow - glow
			{
				rendersys.SetStencilCheckEnabled(true);
				rendersys.SetStencilBufferParams(CompareFunction.CMPF_EQUAL,
												 STENCIL_VALUE_FOR_FULL_GLOW,
												 STENCIL_FULL_MASK,
												 StencilOperation.SOP_KEEP,
												 StencilOperation.SOP_KEEP,
												 StencilOperation.SOP_ZERO,
												 false);
			}
		}
	}
}
