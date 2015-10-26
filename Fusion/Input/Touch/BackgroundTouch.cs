using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using Fusion.Mathematics;

namespace Fusion.Input.Touch
{
    public class BackgroundTouch : BaseTouchHandler
    {
	    TouchForm touchForm;

	    Vector2 center;
	    Vector2 prevTranslation;

        public BackgroundTouch(TouchForm tForm) : base()
        {
	        touchForm = tForm;

            Win32TouchFunctions.INTERACTION_CONTEXT_CONFIGURATION[] cfg = new Win32TouchFunctions.INTERACTION_CONTEXT_CONFIGURATION[]
            {
                new Win32TouchFunctions.INTERACTION_CONTEXT_CONFIGURATION(Win32TouchFunctions.INTERACTION.TAP,
                    Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.TAP |
                    Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.TAP_DOUBLE),

                new Win32TouchFunctions.INTERACTION_CONTEXT_CONFIGURATION(Win32TouchFunctions.INTERACTION.SECONDARY_TAP,
                    Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.SECONDARY_TAP),

                new Win32TouchFunctions.INTERACTION_CONTEXT_CONFIGURATION(Win32TouchFunctions.INTERACTION.HOLD,
                    Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.HOLD),

					new Win32TouchFunctions.INTERACTION_CONTEXT_CONFIGURATION(Win32TouchFunctions.INTERACTION.MANIPULATION,
                    Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.MANIPULATION |
                    Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.MANIPULATION_SCALING |
					Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.MANIPULATION_TRANSLATION_X |
                    Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.MANIPULATION_TRANSLATION_Y)
            };

            Win32TouchFunctions.SetInteractionConfigurationInteractionContext(Context, cfg.Length, cfg);
        }

        internal override void ProcessEvent(InteractionOutput output)
        {
            if (output.Data.Interaction == Win32TouchFunctions.INTERACTION.TAP) {
	            
				var p = touchForm.PointToClient(new System.Drawing.Point((int)output.Data.X, (int)output.Data.Y));

	            if (output.Data.Tap.Count == 1) {
					touchForm.NotifyTap(new Vector2(p.X, p.Y));
	            } 
				else if (output.Data.Tap.Count == 2) {
					touchForm.NotifyDoubleTap(new Vector2(p.X, p.Y));
                }
            }
            else if (output.Data.Interaction == Win32TouchFunctions.INTERACTION.SECONDARY_TAP)
            {
				var p = touchForm.PointToClient(new System.Drawing.Point((int)output.Data.X, (int)output.Data.Y));

				touchForm.NotifyTouchSecondaryTap(new Vector2(p.X, p.Y));
            }
			else if (output.Data.Interaction == Win32TouchFunctions.INTERACTION.HOLD) {
				
			}
			else if (output.Data.Interaction == Win32TouchFunctions.INTERACTION.MANIPULATION) {
				if (output.IsBegin()) {
					var p	= touchForm.PointToClient(new System.Drawing.Point((int)output.Data.X, (int)output.Data.Y));
					center	= new Vector2(p.X, p.Y);
					prevTranslation = new Vector2();
					return;
				}

				var mt = output.Data.Manipulation.Cumulative;

				var translation = new Vector2(mt.TranslationX, mt.TranslationY);

				var delta = translation - prevTranslation;

				prevTranslation = translation;


				touchForm.NotifyTouchManipulation(center, delta, mt.Scale);

				//output.IsEnd()
			}
        }
    }
}
