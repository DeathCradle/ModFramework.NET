using dnlib.DotNet;
using dnSpy.AsmEditor.Commands;
using dnSpy.AsmEditor.Compiler;
using dnSpy.AsmEditor.UndoRedo;
using dnSpy.Contracts.App;
using dnSpy.Contracts.AsmEditor.Compiler;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Documents.Tabs.DocViewer;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.Extension;
using dnSpy.Contracts.Images;
using dnSpy.Contracts.Menus;
using dnSpy.Contracts.MVVM;
using dnSpy.Contracts.TreeView;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Mod.Framework.dnSpyExtension
{
	[ExportAutoLoaded]
	sealed class Startup : IAutoLoaded
	{
		[ImportingConstructor]
		Startup(IAppWindow appWindow)
		{
			appWindow.AddTitleInfo("ModFramework.NET designer");
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
		}

		private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			//if (args.RequestingAssembly == typeof(Entry).Assembly)
			{
				var an = new AssemblyNameInfo(args.Name);
				var mf_name = $"{an.Name}.mf.dll";

				if (System.IO.File.Exists(mf_name))
				{
					var data = System.IO.File.ReadAllBytes(mf_name);
					return System.Reflection.Assembly.Load(data);
				}
			}

			return null;
		}
	}

	//[ExportExtension]
	public class dnSpyExtension : dnSpy.Contracts.Extension.IExtension
	{
		public ExtensionInfo ExtensionInfo => new ExtensionInfo()
		{
			Copyright = "https://github.com/DeathCradle/",
			ShortDescription = "Adds helper tools for OTAPI v3"
		};

		public IEnumerable<string> MergedResourceDictionaries
		{
			get
			{
				yield break;
			}
		}

		public void OnEvent(ExtensionEvent @event, object obj)
		{
		}
	}

	public static class Constants
	{
		//public const string GROUP_CODE = "20000,dfb24b57-e702-40bd-beab-6de6555d721a";
		public const string MENU_GROUP = "dfb24b57-e702-40bd-beab-6de6555d721a";
		public const string MENU_GROUP_SAVE_PATCH = "10,b59ae2fa-7be9-47d1-b9fa-2c1dbdd83a4d";

		//public const string APP_MENU_GUID_CHANGE_TO_INTERFACE = "20000,e3e1b0d5-7500-45b8-8f69-789a575728ae";

		public static class dnSpy
		{
			public static readonly Guid GUIDOBJ_DOCUMENTS_TREEVIEW_GUID = new Guid(MenuConstants.GUIDOBJ_DOCUMENTS_TREEVIEW_GUID);
			public static readonly Guid APP_MENU_EDIT_GUID = new Guid(MenuConstants.APP_MENU_EDIT_GUID);
			//public static readonly Guid GUIDOBJ_DOCUMENTS_TREEVIEW_GUID = new Guid(MenuConstants.GUIDOBJ_DOCUMENTS_TREEVIEW_GUID);
		}
	}

	[ExportMenu(OwnerGuid = MenuConstants.APP_MENU_EDIT_GUID, Guid = Constants.MENU_GROUP,
		Order = MenuConstants.ORDER_APP_MENU_DEBUG + 0.1,
		Header = "_ModFramework.NET")]
	sealed class DebugMenu : IMenu
	{

	}

	//sealed class ChangeToInterfaceCommand
	[ExportMenuItem(
		OwnerGuid = MenuConstants.APP_MENU_EDIT_GUID,
		Order = 50,
		Header = "Change to interface",
		Group = MenuConstants.GROUP_APP_MENU_EDIT_ASMED_NEW
	)]
	sealed class ChangeToInterfaceMenuItem : MenuItemBase
	{
		public override void Execute(IMenuItemContext context) => MsgBox.Instance.Show("Change to");

		public override bool IsVisible(IMenuItemContext context)
		{
			return (context.CreatorObject.Guid == Constants.dnSpy.APP_MENU_EDIT_GUID
				|| context.CreatorObject.Guid == Constants.dnSpy.GUIDOBJ_DOCUMENTS_TREEVIEW_GUID
				)
				&& context.Find<TreeNodeData[]>()?.Length == 1
				&& (context.Find<TreeNodeData[]>().Single() as FieldNode).FieldDef.FieldType.IsArray == true;
		}
	}
	[Export, ExportMenuItem(
		//OwnerGuid = MenuConstants.GUIDOBJ_DOCUMENTS_TREEVIEW_GUID,
		Order = 50,
		Header = "Change to interface",
		Group = MenuConstants.GROUP_CTX_DOCVIEWER_ASMED_SETTINGS
	)]
	sealed class ChangeToInterfaceMenuItem1 : dnSpy.AsmEditor.Commands.CodeContextMenuHandler
	{
		public override void Execute(CodeContext context) => MsgBox.Instance.Show("Change to");

		public override bool IsEnabled(CodeContext context)
		{
			return true;
			//return (context.CreatorObject.Guid == Constants.dnSpy.APP_MENU_EDIT_GUID
			//	|| context.CreatorObject.Guid == Constants.dnSpy.GUIDOBJ_DOCUMENTS_TREEVIEW_GUID
			//	)
			//	&& context.Find<TreeNodeData[]>()?.Length == 1
			//	&& (context.Find<TreeNodeData[]>().Single() as FieldNode).FieldDef.FieldType.IsArray == true;
		}
	}


	[ExportMenuItem(
		OwnerGuid = Constants.MENU_GROUP,
		Header = "Save changes as patch...",
		Order = 0,
		Group = Constants.MENU_GROUP_SAVE_PATCH)]
	sealed class SaveAsPatchMenuItem : MenuItemBase
	{
		public override void Execute(IMenuItemContext context) => MsgBox.Instance.Show("Command #1");
	}

	[ExportMenuItem(Header = "Swap to Interface", Group = Constants.MENU_GROUP, Order = 0)]
	sealed class SwapToInterfaceMenuItem : MenuItemBase
	{
		//readonly IDecompilerService decompilerService;
		//readonly IEnumerable<ILanguageCompilerProvider> languageCompilerProviders;
		//readonly IDocumentTreeView documentTreeView;
		//readonly IAppWindow appWindow;
		//readonly EditCodeVMCreator editCodeVMCreator;

		[ImportingConstructor]
		SwapToInterfaceMenuItem(
			IDocumentTreeView documentTreeView,
			IDecompilerService decompilerService,
			[ImportMany] IEnumerable<ILanguageCompilerProvider> languageCompilerProviders,
			IAppWindow appWindow,
			EditCodeVMCreator editCodeVMCreator,
			IUndoCommandService undoCommandService
		)
		{

		}
		public override void Execute(IMenuItemContext context)
		{
			if (context.CreatorObject.Guid != Constants.dnSpy.GUIDOBJ_DOCUMENTS_TREEVIEW_GUID)
				return;
		}

		//public override ImageReference? GetIcon(IMenuItemContext context)
		//{
		//	return this.CurrentLanguageProvider?.Icon;
		//}

		public override bool IsVisible(IMenuItemContext context)
		{
			return context.CreatorObject.Guid == Constants.dnSpy.GUIDOBJ_DOCUMENTS_TREEVIEW_GUID
				&& context.Find<TreeNodeData[]>().Length == 1;
		}
	}

	//[ExportMenuItem(Header = "OTAPI Modification", Group = Constants.GROUP_CODE, Order = 0)]
	//sealed class OTAPIModificationMenuItem : MenuItemBase
	//{
	//	readonly IDecompilerService decompilerService;
	//	readonly IEnumerable<ILanguageCompilerProvider> languageCompilerProviders;
	//	readonly IDocumentTreeView documentTreeView;
	//	readonly IAppWindow appWindow;
	//	readonly EditCodeVMCreator editCodeVMCreator;

	//	[ImportingConstructor]
	//	OTAPIModificationMenuItem(
	//		IDocumentTreeView documentTreeView,
	//		IDecompilerService decompilerService,
	//		[ImportMany] IEnumerable<ILanguageCompilerProvider> languageCompilerProviders,
	//		IAppWindow appWindow,
	//		EditCodeVMCreator editCodeVMCreator,
	//		IUndoCommandService undoCommandService
	//	)
	//	{
	//		this.documentTreeView = documentTreeView;
	//		this.decompilerService = decompilerService;
	//		this.languageCompilerProviders = languageCompilerProviders;
	//		this.appWindow = appWindow;
	//		this.editCodeVMCreator = editCodeVMCreator;
	//	}

	//	private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
	//	{
	//		//if (args.RequestingAssembly == typeof(Entry).Assembly)
	//		{
	//			var an = new AssemblyNameInfo(args.Name);
	//			var mf_name = $"{an.Name}.mf.dll";

	//			if (System.IO.File.Exists(mf_name))
	//			{
	//				var data = System.IO.File.ReadAllBytes(mf_name);
	//				return System.Reflection.Assembly.Load(data);
	//			}
	//		}

	//		return null;
	//	}

	//	public override void Execute(IMenuItemContext context)
	//	{
	//		if (context.CreatorObject.Guid != Constants.dnSpy.GUIDOBJ_DOCUMENTS_TREEVIEW_GUID)
	//			return;
	//		var nodeData = context.Find<TreeNodeData[]>();
	//		if (nodeData == null)
	//			return;

	//		if (nodeData.Length != 1)
	//		{
	//			return;
	//		}

	//		var node = nodeData.Single() as MethodNode;

	//		var dialog = new ModificationDialog();
	//		//dialog.DataContext = new MethodDecompilerVM(node.MethodDef, this.decompilerService.Decompiler);
	//		var vm = editCodeVMCreator.CreateEditMethodCode(node.MethodDef, Array.Empty<MethodSourceStatement>());
	//		dialog.DataContext = vm;
	//		dialog.Owner = appWindow.MainWindow;
	//		dialog.Title = string.Format("{0} - {1}", dialog.Title, node.ToString());
	//		dialog.ShowDialog();
	//	}

	//	private ILanguageCompilerProvider CurrentLanguageProvider =>
	//		this.languageCompilerProviders
	//		.OrderBy(lp => lp.Order)
	//		.FirstOrDefault(lp => lp.Language == this.decompilerService.Decompiler.GenericGuid);

	//	public override ImageReference? GetIcon(IMenuItemContext context)
	//	{
	//		return this.CurrentLanguageProvider?.Icon;
	//	}

	//	public override bool IsVisible(IMenuItemContext context)
	//	{
	//		return context.CreatorObject.Guid == Constants.dnSpy.GUIDOBJ_DOCUMENTS_TREEVIEW_GUID
	//			&& this.CurrentLanguageProvider != null
	//			&& decompilerService != null
	//			&& decompilerService.Decompiler != null
	//			&& !String.IsNullOrWhiteSpace(decompilerService.Decompiler.FileExtension)
	//			&& decompilerService.Decompiler.FileExtension.Equals(".cs", StringComparison.CurrentCultureIgnoreCase)

	//			&& context.Find<TreeNodeData[]>().Length == 1;
	//	}
	//}
}
