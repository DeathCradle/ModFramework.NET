//using dnlib.DotNet;
//using dnSpy.AsmEditor.Compiler;
//using dnSpy.Contracts.AsmEditor.Compiler;
//using dnSpy.Contracts.Decompiler;
//using dnSpy.Contracts.MVVM;
//using dnSpy.Contracts.Text.Editor;
//using Microsoft.VisualStudio.Text;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows;

//namespace Mod.Framework.dnSpyExtension
//{
//	public class MethodDecompilerVM : ViewModelBase, IDisposable
//	{
//		//protected MethodDef method;
//		//protected Mono.Cecil.AssemblyDefinition assembly;
//		//protected Mono.Cecil.MethodDefinition method;
//		protected MethodDef dnMethod;

//		protected IDecompiler decompiler;
//		protected EditMethodCodeVM editVM;

//		public string DecompiledCode { get; private set; }

//		public MethodDecompilerVM(MethodDef method, IDecompiler decompiler, EditMethodCodeVM editVM)
//		{
//			this.dnMethod = method;
//			this.decompiler = decompiler;
//			this.editVM = editVM;

//			StartDecompileAsync().ContinueWith((result) =>
//			{
//				var ex = result.Exception;
//				Debug.Assert(ex == null);
//			}, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
//		}

//		public void Dispose()
//		{
//			//this.assembly = null;
//			//this.method = null;
//			this.dnMethod = null;
//		}


//		public sealed class CodeDocument
//		{
//			public string Name => codeDocument.Name;
//			public string NameNoExtension => codeDocument.NameNoExtension;
//			public IDsWpfTextView TextView => codeDocument.TextView;
//			public IDsWpfTextViewHost TextViewHost => codeDocument.TextViewHost;

//			readonly ICodeDocument codeDocument;
//			SnapshotPoint initialPosition;

//			public CodeDocument(ICodeDocument codeDocument)
//			{
//				this.codeDocument = codeDocument;
//				codeDocument.TextView.VisualElement.SizeChanged += VisualElement_SizeChanged;
//			}

//			void VisualElement_SizeChanged(object sender, SizeChangedEventArgs e)
//			{
//				if (e.NewSize.Height == 0)
//					return;
//				codeDocument.TextView.VisualElement.SizeChanged -= VisualElement_SizeChanged;

//				Debug.Assert(initialPosition.Snapshot != null);
//				if (initialPosition.Snapshot == null)
//					return;
//				codeDocument.TextView.Caret.MoveTo(initialPosition.TranslateTo(codeDocument.TextView.TextSnapshot, PointTrackingMode.Negative));
//				codeDocument.TextView.EnsureCaretVisible(true);
//			}

//			public void Initialize(SnapshotPoint initialPosition)
//			{
//				this.initialPosition = initialPosition;
//				codeDocument.TextView.Selection.Clear();
//			}

//			public void Dispose() => codeDocument.TextView.VisualElement.SizeChanged -= VisualElement_SizeChanged;
//		}

//		public ObservableCollection<CodeDocument> Documents { get; } = new ObservableCollection<CodeDocument>();
//		public CodeDocument SelectedDocument
//		{
//			get => selectedDocument;
//			set
//			{
//				if (selectedDocument != value)
//				{
//					selectedDocument = value;
//					OnPropertyChanged(nameof(SelectedDocument));
//				}
//			}
//		}
//		CodeDocument selectedDocument;

//		protected struct SimpleDocument
//		{
//			public string NameNoExtension { get; }
//			public string Text { get; }
//			public Span? CaretSpan { get; }
//			public SimpleDocument(string nameNoExtension, string text, Span? caretSpan)
//			{
//				NameNoExtension = nameNoExtension;
//				Text = text;
//				CaretSpan = caretSpan;
//			}
//		}

//		protected sealed class DecompileAsyncResult
//		{
//			public List<SimpleDocument> Documents { get; } = new List<SimpleDocument>();

//			public void AddDocument(string nameNoExtension, string text, Span? caretSpan) =>
//				Documents.Add(new SimpleDocument(nameNoExtension, text, caretSpan));
//		}


//		protected const string MAIN_CODE_NAME = "main";
//		protected const string MAIN_G_CODE_NAME = "main.g";

//		public Task StartDecompileAsync()
//		{
//			return Task.Run(() =>
//			{
				
//				//var location = this.dnMethod.Module.Location;

//				//this.assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(location);
//				//this.method = this.assembly.Modules
//				//	.SelectMany(m => m.Types)
//				//	.SelectMany(t => t.Methods)
//				//	.Single(m => m.DeclaringType.FullName == this.dnMethod.DeclaringType.FullName
//				//		&& m.FullName == this.dnMethod.FullName
//				//	);

//				//var output = new DecompilerOutput();
//				//var ctx = new DecompilerContext()
//				//{

//				//};
//				//this.decompiler.Decompile(this.dnMethod, output, ctx);


//				//var result = new DecompileAsyncResult();
//				//result.AddDocument(MAIN_CODE_NAME, state.MainOutput.ToString(), state.MainOutput.Span);
//				//result.AddDocument(MAIN_G_CODE_NAME, state.HiddenOutput.ToString(), null);
//				//return Task.FromResult(result);

//				//DecompiledCode = output.GetText();
//				//OnPropertyChanged(nameof(DecompiledCode));
//			});
//		}
//	}

//	//sealed class EditMethodDecompileCodeState : DecompileCodeState
//	//{
//	//	public ReferenceDecompilerOutput MainOutput { get; }
//	//	public StringBuilderDecompilerOutput HiddenOutput { get; }

//	//	public EditMethodDecompileCodeState(object referenceToEdit, MethodSourceStatement? methodSourceStatement)
//	//	{
//	//		MainOutput = new ReferenceDecompilerOutput(referenceToEdit, methodSourceStatement);
//	//		HiddenOutput = new StringBuilderDecompilerOutput();
//	//	}
//	//}

//	//class DecompilerOutput : StringBuilderDecompilerOutput, IDecompilerOutput
//	//{
//	//	readonly object reference;
//	//	public Span? Span => statementSpan ?? referenceSpan;
//	//	Span? referenceSpan;
//	//	Span? statementSpan;
//	//	MethodSourceStatement? methodSourceStatement;

//	//	bool IDecompilerOutput.UsesCustomData => true;

//	//	public DecompilerOutput(object reference, MethodSourceStatement? methodSourceStatement)
//	//	{
//	//		this.reference = reference;
//	//		this.methodSourceStatement = methodSourceStatement;
//	//	}

//	//	public override void Write(string text, object reference, DecompilerReferenceFlags flags, object color) =>
//	//		Write(text, 0, text.Length, reference, flags, color);

//	//	public override void Write(string text, int index, int length, object reference, DecompilerReferenceFlags flags, object color)
//	//	{
//	//		if (reference == this.reference && (flags & DecompilerReferenceFlags.Definition) != 0 && referenceSpan == null)
//	//		{
//	//			int start = NextPosition;
//	//			base.Write(text, index, length, reference, flags, color);
//	//			referenceSpan = new Span(start, Length - start);
//	//		}
//	//		else
//	//			base.Write(text, index, length, reference, flags, color);
//	//	}

//	//	void IDecompilerOutput.AddCustomData<TData>(string id, TData data)
//	//	{
//	//		if (id == PredefinedCustomDataIds.DebugInfo)
//	//			AddDebugInfo(data as MethodDebugInfo);
//	//	}

//	//	void AddDebugInfo(MethodDebugInfo info)
//	//	{
//	//		if (info == null)
//	//			return;
//	//		if (methodSourceStatement == null)
//	//			return;
//	//		if (methodSourceStatement.Value.Method != info.Method)
//	//			return;
//	//		var stmt = info.GetSourceStatementByCodeOffset(methodSourceStatement.Value.Statement.BinSpan.Start);
//	//		if (stmt == null)
//	//			return;
//	//		statementSpan = new Span(stmt.Value.TextSpan.Start, stmt.Value.TextSpan.Length);
//	//	}
//	//}

//	//public class DecompilerContext : DecompilationContext
//	//{

//	//}
//}
