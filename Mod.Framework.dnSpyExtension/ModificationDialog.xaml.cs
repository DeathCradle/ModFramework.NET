using dnSpy.AsmEditor.Compiler;
using dnSpy.AsmEditor.Properties;
using dnSpy.Contracts.Controls;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Mod.Framework.dnSpyExtension
{
	/// <summary>
	/// Interaction logic for Modification.xaml
	/// </summary>
	public partial class ModificationDialog : WindowBase
	{
		//protected MethodDecompilerVM decompilerVM;

		public ModificationDialog()
		{
			InitializeComponent();

			decompilingControl.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.5)), FillBehavior.Stop));

			DataContextChanged += (s, e) =>
			{
				//if(e.NewValue is MethodDecompilerVM vm)
				//{
				//	vm.PropertyChanged += Vm_PropertyChanged;
				//	decompilerVM = vm;
				//}
				if (e.NewValue is EditCodeVM vm)
				{
					vm.PropertyChanged += Vm_PropertyChanged;
					//vm.OwnerWindow = this;
					vm.CodeCompiled += Vm_CodeCompiled;
					if (vm.HasDecompiled)
						RemoveProgressBar();
					else
					{
						vm.PropertyChanged += (s2, e2) =>
						{
							if (e2.PropertyName == nameof(vm.HasDecompiled) && vm.HasDecompiled)
								RemoveProgressBar();
						};
					}
					InputBindings.Add(new KeyBinding(vm.AddGacReferenceCommand, Key.O, ModifierKeys.Control | ModifierKeys.Shift));
					InputBindings.Add(new KeyBinding(vm.AddAssemblyReferenceCommand, Key.O, ModifierKeys.Control));
					InputBindings.Add(new KeyBinding(vm.GoToNextDiagnosticCommand, Key.F4, ModifierKeys.None));
					InputBindings.Add(new KeyBinding(vm.GoToNextDiagnosticCommand, Key.F8, ModifierKeys.None));
					InputBindings.Add(new KeyBinding(vm.GoToPreviousDiagnosticCommand, Key.F4, ModifierKeys.Shift));
					InputBindings.Add(new KeyBinding(vm.GoToPreviousDiagnosticCommand, Key.F8, ModifierKeys.Shift));
				}
			};
			diagnosticsListView.SelectionChanged += DiagnosticsListView_SelectionChanged;
		}

		private void Vm_CodeCompiled(object sender, EventArgs e)
		{

		}

		private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (EditCodeVM)sender;
			if (e.PropertyName == nameof(vm.SelectedDocument))
				UIUtilities.Focus(vm.SelectedDocument?.TextView.VisualElement);
		}

		//private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
		//{
		//	if(e.PropertyName == nameof(MethodDecompilerVM.DecompiledCode))
		//	{
		//		this.Dispatcher.Invoke(() =>
		//		{
		//			//decompilingControl_textBlock.Text = decompilerVM.DecompiledCode;
		//			//SelectedDocument
		//			RemoveProgressBar();
		//		});
		//	}
		//	}

		protected override void OnClosed(EventArgs e)
		{
			progressBar.IsIndeterminate = false;
			//if (this.DataContext is MethodDecompilerVM vm)
			//{
			//	vm.PropertyChanged -= Vm_PropertyChanged;
			//}
			base.OnClosed(e);
		}

		void RemoveProgressBar()
		{
			// An indeterminate progress bar that is collapsed still animates so make sure
			// it's not in the tree at all.
			decompilingControl.Child = null;
			decompilingControl.Visibility = Visibility.Collapsed;
		}

		void diagnosticsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{

		}
		void DiagnosticsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var item = diagnosticsListView.SelectedItem;
			if (item == null)
				return;
			diagnosticsListView.ScrollIntoView(item);
		}

		static readonly string[] viewHeaders = new string[] {
			dnSpy_AsmEditor_Resources.CompileDiagnostics_Header_Severity,
			dnSpy_AsmEditor_Resources.CompileDiagnostics_Header_Code,
			dnSpy_AsmEditor_Resources.CompileDiagnostics_Header_Description,
			dnSpy_AsmEditor_Resources.CompileDiagnostics_Header_File,
			dnSpy_AsmEditor_Resources.CompileDiagnostics_Header_Line,
		};
	}
}