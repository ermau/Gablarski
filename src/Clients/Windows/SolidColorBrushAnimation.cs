// Copyright (c) 2014, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

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