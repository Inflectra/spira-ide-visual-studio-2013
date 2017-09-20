















using System;
using System.ComponentModel;
using System.Windows;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Core;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Media.Animation;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Panels
{
	[TypeConverter(typeof(AnimatorConverter))]
	public abstract class IterativeAnimator
	{
		#region Default Static Property

		public static IterativeAnimator Default
		{
			get
			{
				return _default;
			}
		}

		private static readonly IterativeAnimator _default = new DefaultAnimator();

		#endregion

		public abstract Rect GetInitialChildPlacement(
		  UIElement child,
		  Rect currentPlacement,
		  Rect targetPlacement,
		  AnimationPanel activeLayout,
		  ref AnimationRate animationRate,
		  out object placementArgs,
		  out bool isDone);

		public abstract Rect GetNextChildPlacement(
		  UIElement child,
		  TimeSpan currentTime,
		  Rect currentPlacement,
		  Rect targetPlacement,
		  AnimationPanel activeLayout,
		  AnimationRate animationRate,
		  ref object placementArgs,
		  out bool isDone);

		#region DefaultAnimator Nested Type

		private sealed class DefaultAnimator : IterativeAnimator
		{
			public override Rect GetInitialChildPlacement(UIElement child, Rect currentPlacement, Rect targetPlacement, AnimationPanel activeLayout, ref AnimationRate animationRate, out object placementArgs, out bool isDone)
			{
				throw new InvalidOperationException(ErrorMessages.GetMessage(ErrorMessages.DefaultAnimatorCantAnimate));
			}

			public override Rect GetNextChildPlacement(UIElement child, TimeSpan currentTime, Rect currentPlacement, Rect targetPlacement, AnimationPanel activeLayout, AnimationRate animationRate, ref object placementArgs, out bool isDone)
			{
				throw new InvalidOperationException(ErrorMessages.GetMessage(ErrorMessages.DefaultAnimatorCantAnimate));
			}
		}

		#endregion
	}
}
