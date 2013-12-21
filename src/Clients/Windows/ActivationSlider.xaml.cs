using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gablarski.Clients.Windows
{
	/// <summary>
	/// Interaction logic for ActivationSlider.xaml
	/// </summary>
	public partial class ActivationSlider : UserControl
	{
		public ActivationSlider()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register (
			"Value", typeof (int), typeof (ActivationSlider), new FrameworkPropertyMetadata (default(int)) { DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, BindsTwoWayByDefault = true });

		public int Value
		{
			get { return (int) GetValue (ValueProperty); }
			set { SetValue (ValueProperty, value); }
		}

		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register (
			"Maximum", typeof (int), typeof (ActivationSlider), new PropertyMetadata (default(int)));

		public int Maximum
		{
			get { return (int) GetValue (MaximumProperty); }
			set { SetValue (MaximumProperty, value); }
		}

		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register (
			"Minimum", typeof (int), typeof (ActivationSlider), new PropertyMetadata (default(int)));

		public int Minimum
		{
			get { return (int) GetValue (MinimumProperty); }
			set { SetValue (MinimumProperty, value); }
		}

		public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register (
			"LargeChange", typeof (int), typeof (ActivationSlider), new PropertyMetadata (default(int)));

		public int LargeChange
		{
			get { return (int) GetValue (LargeChangeProperty); }
			set { SetValue (LargeChangeProperty, value); }
		}

		public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register (
			"SmallChange", typeof (int), typeof (ActivationSlider), new PropertyMetadata (default(int)));

		public int SmallChange
		{
			get { return (int) GetValue (SmallChangeProperty); }
			set { SetValue (SmallChangeProperty, value); }
		}

		public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register (
			"TickFrequency", typeof (int), typeof (ActivationSlider), new PropertyMetadata (default(int)));

		public int TickFrequency
		{
			get { return (int) GetValue (TickFrequencyProperty); }
			set { SetValue (TickFrequencyProperty, value); }
		}

		public static readonly DependencyProperty ActivationLevelProperty = DependencyProperty.Register (
			"ActivationLevel", typeof (int), typeof (ActivationSlider), new PropertyMetadata (default(int)));

		public int ActivationLevel
		{
			get { return (int) GetValue (ActivationLevelProperty); }
			set { SetValue (ActivationLevelProperty, value); }
		}

		protected override void OnPropertyChanged (DependencyPropertyChangedEventArgs e)
		{
			if (e.Property == ActualWidthProperty)
				this.activation.Width = (double)e.NewValue - 20;

			base.OnPropertyChanged (e);
		}
	}
}
