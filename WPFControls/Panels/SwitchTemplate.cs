















using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Core.Utilities;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012.Forms.ToolKit.Panels
{
	public static class SwitchTemplate
	{
		#region ID Attached Property

		public static readonly DependencyProperty IDProperty =
		  DependencyProperty.RegisterAttached("ID", typeof(string), typeof(SwitchTemplate),
			new FrameworkPropertyMetadata(null,
			  new PropertyChangedCallback(SwitchTemplate.OnIDChanged)));

		public static string GetID(DependencyObject d)
		{
			return (string)d.GetValue(SwitchTemplate.IDProperty);
		}

		public static void SetID(DependencyObject d, string value)
		{
			d.SetValue(SwitchTemplate.IDProperty, value);
		}

		private static void OnIDChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if ((e.NewValue == null) || !(d is UIElement))
				return;

			SwitchPresenter parentPresenter = VisualTreeHelperEx.FindAncestorByType<SwitchPresenter>(d);
			if (parentPresenter != null)
			{
				parentPresenter.RegisterID(e.NewValue as string, d as FrameworkElement);
			}
			else
			{
				d.Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
					(ThreadStart)delegate()
				{
					parentPresenter = VisualTreeHelperEx.FindAncestorByType<SwitchPresenter>(d);
					if (parentPresenter != null)
					{
						parentPresenter.RegisterID(e.NewValue as string, d as FrameworkElement);
					}
				});
			}
		}

		#endregion
	}
}
