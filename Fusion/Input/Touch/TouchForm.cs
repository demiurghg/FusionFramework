using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;
using Fusion.Mathematics;
using Point = System.Drawing.Point;

namespace Fusion.Input.Touch
{
    public class TouchForm : Form
    {
        #region ** fields

        BackgroundTouch background;

	    public event Action<Vector2> TouchTap;
		public event Action<Vector2> TouchDoubleTap;
		public event Action<Vector2> TouchSecondaryTap;
	    public event Action<Vector2, Vector2, float> TouchManipulation;

        #endregion

        #region ** initialization

        public TouchForm() : base()
        {
			Win32TouchFunctions.EnableMouseInPointer(false);

			background = new BackgroundTouch(this);
        }

        #endregion

        #region ** finalization

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                background.Dispose();
                background = null;
            }
            base.Dispose(disposing);
        }

        #endregion


        [SecurityPermission(SecurityAction.LinkDemand)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Win32TouchFunctions.WM_POINTERDOWN:
                case Win32TouchFunctions.WM_POINTERUP:
                case Win32TouchFunctions.WM_POINTERUPDATE:
                case Win32TouchFunctions.WM_POINTERCAPTURECHANGED:
                    break;

                default:
                    base.WndProc(ref m);
                    return;
            }
            int pointerID = Win32TouchFunctions.GET_POINTER_ID(m.WParam);
            Win32TouchFunctions.POINTER_INFO pi = new Win32TouchFunctions.POINTER_INFO();
            if (!Win32TouchFunctions.GetPointerInfo(pointerID, ref pi))
            {
                Win32TouchFunctions.CheckLastError();
            }
            switch (m.Msg)
            {
                case Win32TouchFunctions.WM_POINTERDOWN:
                    {
                        if ((pi.PointerFlags & Win32TouchFunctions.POINTER_FLAGS.PRIMARY) != 0)
                        {
                            this.Capture = true;
                        }
                        Point pt = PointToClient(pi.PtPixelLocation.ToPoint());

                        background.AddPointer(pointerID);
                        background.ProcessPointerFrames(pointerID, pi.FrameID);
                        
                    }
                    break;

                case Win32TouchFunctions.WM_POINTERUP:

                    if ((pi.PointerFlags & Win32TouchFunctions.POINTER_FLAGS.PRIMARY) != 0)
                    {
                        this.Capture = false;
                    }
                    
                    if ( background.ActivePointers.Contains(pointerID))
                    {
                        background.ProcessPointerFrames(pointerID, pi.FrameID);
                        background.RemovePointer(pointerID);
                    }
                    break;

                case Win32TouchFunctions.WM_POINTERUPDATE:

                    if (background.ActivePointers.Contains(pointerID))
                    {
                        background.ProcessPointerFrames(pointerID, pi.FrameID);
                    }
                    break;

                case Win32TouchFunctions.WM_POINTERCAPTURECHANGED:

                    this.Capture = false;

                    if (background.ActivePointers.Contains(pointerID))
                    {
                        background.StopProcessing();
                    }
                    break;
            }
            m.Result = IntPtr.Zero;
        }


	    public void NotifyTap(Vector2 pos)
	    {
		    if (TouchTap != null) {
			    TouchTap(pos);
		    }
	    }

	    public void NotifyDoubleTap(Vector2 pos)
	    {
		    if (TouchDoubleTap != null) {
			    TouchDoubleTap(pos);
		    }
	    }

	    internal void NotifyTouchManipulation(Vector2 center, Vector2 delta, float scale)
	    {
		    if (TouchManipulation != null) {
			    TouchManipulation(center, delta, scale);
		    }
	    }

		internal void NotifyTouchSecondaryTap(Vector2 pos)
		{
			if (TouchSecondaryTap != null) {
				TouchSecondaryTap(pos);
			}
		}

    }
}
