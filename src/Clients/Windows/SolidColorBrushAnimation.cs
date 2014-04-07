using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Gablarski.Clients.Windows
{
	class SolidColorBrushAnimation
		: AnimationTimeline
	{
		public static readonly DependencyProperty ToProperty = DependencyProperty.Register (
			"To", typeof (SolidColorBrush), typeof (SolidColorBrushAnimation), new PropertyMetadata (default(SolidColorBrush)));

		public SolidColorBrush To
		{
			get { return (SolidColorBrush) GetValue (ToProperty); }
			set { SetValue (ToProperty, value); }
		}

		public SolidColorBrush GetCurrentBrush (SolidColorBrush defaultOriginBrush, SolidColorBrush defaultDestinationBrush, AnimationClock animationClock)
		{
			return new SolidColorBrush (this.animation.GetCurrentValue (defaultOriginBrush.Color, defaultDestinationBrush.Color, animationClock));
		}

		public override object GetCurrentValue (object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
		{
			return GetCurrentBrush ((SolidColorBrush) defaultOriginValue, (SolidColorBrush) defaultDestinationValue, animationClock);
		}

		protected override Freezable CreateInstanceCore()
		{
			return new SolidColorBrushAnimation();
		}

		public override Type TargetPropertyType
		{
			get { return typeof (SolidColorBrush); }
		}

		private readonly ColorAnimation animation = new ColorAnimation();

		protected override void OnPropertyChanged (DependencyPropertyChangedEventArgs e)
		{
			if (e.Property == DurationProperty)
				this.animation.Duration = Duration;
			else if (e.Property == ToProperty)
				this.animation.To = To.Color;

			base.OnPropertyChanged (e);
		}
	}
}