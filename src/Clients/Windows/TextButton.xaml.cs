using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Gablarski.Clients.Windows
{
	/// <summary>
	/// Interaction logic for TextButton.xaml
	/// </summary>
	public partial class TextButton : TextBlock
	{
		public TextButton()
		{
			InitializeComponent();
		}

		public event EventHandler Clicked;	

		protected override void OnMouseEnter (MouseEventArgs e)
		{
			VisualStateManager.GoToElementState (this, "Hover", true);
			base.OnMouseEnter (e);
		}

		protected override void OnMouseLeave (MouseEventArgs e)
		{
			VisualStateManager.GoToElementState (this, "Normal", true);
			base.OnMouseLeave (e);
		}

		protected override void OnMouseDown (MouseButtonEventArgs e)
		{
			VisualStateManager.GoToElementState (this, "Pressed", true);
			base.OnMouseDown (e);
		}

		protected override void OnMouseUp (MouseButtonEventArgs e)
		{
			if (VisualTreeHelper.HitTest (this, e.GetPosition (this)) == null) {
				VisualStateManager.GoToElementState (this, "Normal", true);
			} else {
				e.Handled = true;
				VisualStateManager.GoToElementState (this, "Hover", true);
				OnClicked();
			}

			base.OnMouseUp (e);
		}

		protected virtual void OnClicked ()
		{
			EventHandler handler = this.Clicked;
			if (handler != null)
				handler (this, EventArgs.Empty);
		}
	}
}
