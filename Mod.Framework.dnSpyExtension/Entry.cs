using dnSpy.Contracts.AsmEditor.Compiler;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Documents.Tabs.DocViewer;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.Extension;
using dnSpy.Contracts.Images;
using dnSpy.Contracts.Menus;
using dnSpy.Contracts.TreeView;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;

namespace Mod.Framework.dnSpyExtension
{
	public class Entry : dnSpy.Contracts.Extension.IExtension
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
				// We don't have any extra resource dictionaries
				yield break;
			}
		}

		public void OnEvent(ExtensionEvent @event, object obj)
		{

		}
	}

	public static class Constants
	{
		public const string GROUP_CODE = "20000,dfb24b57-e702-40bd-beab-6de6555d721a";

		public static class dnSpy
		{
			public static readonly Guid GUIDOBJ_DOCUMENTS_TREEVIEW_GUID = new Guid(MenuConstants.GUIDOBJ_DOCUMENTS_TREEVIEW_GUID);
		}
	}

	[ExportMenuItem(Header = "OTAPI Modification", Group = Constants.GROUP_CODE, Order = 0)]
	sealed class OTAPIModificationMenuItem : MenuItemBase
	{
		readonly IDecompilerService decompilerService;
		readonly ILanguageCompilerProvider languageCompilerProvider;
		readonly IDocumentTreeView documentTreeView;

		[ImportingConstructor]
		OTAPIModificationMenuItem(IDocumentTreeView documentTreeView, IDecompilerService decompilerService, [ImportMany] IEnumerable<ILanguageCompilerProvider> languageCompilerProviders)
		{
			this.documentTreeView = documentTreeView;
			this.decompilerService = decompilerService;
			this.languageCompilerProvider = languageCompilerProviders.OrderBy(lp => lp.Order).FirstOrDefault(lp => lp.Language == this.decompilerService.Decompiler.GenericGuid);
		}


		public override void Execute(IMenuItemContext context)
		{
			if (context.CreatorObject.Guid != Constants.dnSpy.GUIDOBJ_DOCUMENTS_TREEVIEW_GUID)
				return;
			var nodeData = context.Find<TreeNodeData[]>();
			if (nodeData == null)
				return;

			if(nodeData.Length != 1)
			{
				return;
			}

			var node = nodeData.Single() as DocumentTreeNodeData;
			//var node = documentTreeView.FindNode(nodeDataItem.);
			var nodes = node == null ? Array.Empty<DocumentTreeNodeData>() : new DocumentTreeNodeData[] { node };

			
		}

		public override ImageReference? GetIcon(IMenuItemContext context)
		{
			return this.languageCompilerProvider?.Icon;
		}

		public override bool IsVisible(IMenuItemContext context)
		{
			return context.CreatorObject.Guid == Constants.dnSpy.GUIDOBJ_DOCUMENTS_TREEVIEW_GUID
				&& this.languageCompilerProvider != null
				&& decompilerService != null
				&& decompilerService.Decompiler != null
				&& !String.IsNullOrWhiteSpace(decompilerService.Decompiler.FileExtension)
				&& decompilerService.Decompiler.FileExtension.Equals(".cs", StringComparison.CurrentCultureIgnoreCase)

				&& context.Find<TreeNodeData[]>().Length == 1;
		}
	}
}
