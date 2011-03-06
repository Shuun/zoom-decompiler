﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;

using Debugger;
using Debugger.Interop.CorPublish;
using ICSharpCode.Decompiler;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Visitors;
using ILSpy.Debugger.Bookmarks;
using ILSpy.Debugger.Models.TreeModel;
using ILSpy.Debugger.Services.Debugger;
using ILSpy.Debugger.Tooltips;
using Mono.Cecil;
using CorDbg = Debugger;
using Process = Debugger.Process;
using StackFrame = Debugger.StackFrame;

namespace ILSpy.Debugger.Services
{
	public class WindowsDebugger : IDebugger
	{
		enum StopAttachedProcessDialogResult {
			Detach = 0,
			Terminate = 1,
			Cancel = 2
		}
		
		bool useRemotingForThreadInterop = false;
		bool attached;
		
		NDebugger debugger;
		
		ICorPublish corPublish;
		
		Process debuggedProcess;
		
		//DynamicTreeDebuggerRow currentTooltipRow;
		//Expression             currentTooltipExpression;
		
		private ConcurrentDictionary<string, List<MethodMapping>> CodeMappingsStorage {
			get {
				return CodeMappings.GetStorage(DebugData.Language);
			}
		}
		
		public event EventHandler<ProcessEventArgs> ProcessSelected;
		
		public NDebugger DebuggerCore {
			get {
				return debugger;
			}
		}
		
		public Process DebuggedProcess {
			get {
				return debuggedProcess;
			}
		}
		
		public static Process CurrentProcess {
			get {
				WindowsDebugger dbgr = DebuggerService.CurrentDebugger as WindowsDebugger;
				if (dbgr != null && dbgr.DebuggedProcess != null) {
					return dbgr.DebuggedProcess;
				} else {
					return null;
				}
			}
		}
		
		/// <inheritdoc/>
		public bool BreakAtBeginning {
			get;
			set;
		}
		
		protected virtual void OnProcessSelected(ProcessEventArgs e)
		{
			if (ProcessSelected != null) {
				ProcessSelected(this, e);
			}
		}
		
		public bool ServiceInitialized {
			get {
				return debugger != null;
			}
		}
		
		public WindowsDebugger()
		{
			
		}
		
		#region IDebugger Members
		
		string errorDebugging      = "Error.Debugging";
		string errorNotDebugging   = "Error.NotDebugging";
		string errorProcessRunning = "Error.ProcessRunning";
		string errorProcessPaused  = "Error.ProcessPaused";
		string errorCannotStepNoActiveFunction = "Threads.CannotStepNoActiveFunction";
		
		public bool IsDebugging {
			get {
				return ServiceInitialized && debuggedProcess != null;
			}
		}
		
		public bool IsAttached {
			get {
				return ServiceInitialized && attached;
			}
		}
		
		public bool IsProcessRunning {
			get {
				return IsDebugging && debuggedProcess.IsRunning;
			}
		}
		
		public void Start(ProcessStartInfo processStartInfo)
		{
			if (IsDebugging) {
				MessageBox.Show(errorDebugging);
				return;
			}
			if (!ServiceInitialized) {
				InitializeService();
			}

			string version = debugger.GetProgramVersion(processStartInfo.FileName);
			
			if (version.StartsWith("v1.0")) {
				MessageBox.Show("Net10NotSupported");
			} else if (version.StartsWith("v1.1")) {
				MessageBox.Show("Net1.1NotSupported");
//					} else if (string.IsNullOrEmpty(version)) {
//					// Not a managed assembly
//					MessageBox.Show(".Error.BadAssembly}");
			} else if (debugger.IsKernelDebuggerEnabled) {
				MessageBox.Show("KernelDebuggerEnabled");
			} else {
				attached = false;
				if (DebugStarting != null)
					DebugStarting(this, EventArgs.Empty);
				
				try {
					// set the JIT flag for evaluating optimized code
					Process.DebugMode = DebugModeFlag.Debug;
					
					Process process = debugger.Start(processStartInfo.FileName,
					                                 processStartInfo.WorkingDirectory,
					                                 processStartInfo.Arguments);
					SelectProcess(process);
				} catch (System.Exception e) {
					// COMException: The request is not supported. (Exception from HRESULT: 0x80070032)
					// COMException: The application has failed to start because its side-by-side configuration is incorrect. Please see the application event log for more detail. (Exception from HRESULT: 0x800736B1)
					// COMException: The requested operation requires elevation. (Exception from HRESULT: 0x800702E4)
					// COMException: The directory name is invalid. (Exception from HRESULT: 0x8007010B)
					// BadImageFormatException:  is not a valid Win32 application. (Exception from HRESULT: 0x800700C1)
					// UnauthorizedAccessException: Отказано в доступе. (Исключение из HRESULT: 0x80070005 (E_ACCESSDENIED))
					if (e is COMException || e is BadImageFormatException || e is UnauthorizedAccessException) {
						string msg = "CannotStartProcess";
						msg += " " + e.Message;
						// TODO: Remove
						if (e is COMException && ((uint)((COMException)e).ErrorCode == 0x80070032)) {
							msg += Environment.NewLine + Environment.NewLine;
							msg += "64-bit debugging is not supported.  Please set Project -> Project Options... -> Compiling -> Target CPU to 32bit.";
						}
						MessageBox.Show(msg);
						
						if (DebugStopped != null)
							DebugStopped(this, EventArgs.Empty);
					} else {
						throw;
					}
				}
			}
		}
		
		public void Attach(System.Diagnostics.Process existingProcess)
		{
			if (existingProcess == null)
				return;
			
			if (IsDebugging) {
				MessageBox.Show(errorDebugging);
				return;
			}
			if (!ServiceInitialized) {
				InitializeService();
			}
			
			string version = debugger.GetProgramVersion(existingProcess.MainModule.FileName);
			if (version.StartsWith("v1.0")) {
				MessageBox.Show("Net10NotSupported");
			} else {
				if (DebugStarting != null)
					DebugStarting(this, EventArgs.Empty);
				
				try {
					// set the JIT flag for evaluating optimized code
					Process.DebugMode = DebugModeFlag.Debug;
					
					Process process = debugger.Attach(existingProcess);
					attached = true;
					SelectProcess(process);
				} catch (System.Exception e) {
					// CORDBG_E_DEBUGGER_ALREADY_ATTACHED
					if (e is COMException || e is UnauthorizedAccessException) {
						string msg = "CannotAttachToProcess";
						MessageBox.Show(msg + " " + e.Message);
						
						if (DebugStopped != null)
							DebugStopped(this, EventArgs.Empty);
					} else {
						throw;
					}
				}
			}
		}

		public void Detach()
		{
			if (debuggedProcess == null)
				return;
			
			debugger.Detach();
		}
		
		public void StartWithoutDebugging(ProcessStartInfo processStartInfo)
		{
			System.Diagnostics.Process.Start(processStartInfo);
		}
		
		public void Stop()
		{
			if (!IsDebugging) {
				MessageBox.Show(errorNotDebugging, "Stop");
				return;
			}
			if (IsAttached) {
				Detach();
			} else {
				debuggedProcess.Terminate();
			}
		}
		
		// ExecutionControl:
		
		public void Break()
		{
			if (!IsDebugging) {
				MessageBox.Show(errorNotDebugging, "Break");
				return;
			}
			if (!IsProcessRunning) {
				MessageBox.Show(errorProcessPaused, "Break");
				return;
			}
			debuggedProcess.Break();
		}
		
		public void Continue()
		{
			if (!IsDebugging) {
				MessageBox.Show(errorNotDebugging, "Continue");
				return;
			}
			if (IsProcessRunning) {
				MessageBox.Show(errorProcessRunning, "Continue");
				return;
			}
			debuggedProcess.AsyncContinue();
		}
		
		// Stepping:
		
		SourceCodeMapping GetNextCodeMapping()
		{
			if (CurrentLineBookmark.Instance == null)
				return null;
			
			// get the mapped instruction from the current line marker or the next one
			uint token;
			var instruction = CodeMappingsStorage.GetInstructionByTypeAndLine(
				CurrentLineBookmark.Instance.Type.FullName,
				CurrentLineBookmark.Instance.LineNumber, out token);
			
			var val = CodeMappingsStorage[CurrentLineBookmark.Instance.Type.FullName];
			
			var mapping = val.Find(m => m.MetadataToken == token);
			
			return mapping.MethodCodeMappings.FirstOrDefault(s => s.ILInstructionOffset.From == instruction.ILInstructionOffset.From);
		}
		
		StackFrame GetStackFrame()
		{
			var map = GetNextCodeMapping();
			if (map == null) {
				CurrentLineBookmark.Remove();
				Continue();
				return null;
			} else {
				var frame = debuggedProcess.SelectedThread.MostRecentStackFrame;
				frame.SourceCodeLine = map.SourceCodeLine;
				frame.ILRanges = map.ToArray();
				return frame;
			}
		}
		
		public void StepInto()
		{
			if (!IsDebugging) {
				MessageBox.Show(errorNotDebugging, "StepInto");
				return;
			}
			
			// use most recent stack frame because we don't have the symbols
			if (debuggedProcess.SelectedThread == null ||
			    debuggedProcess.SelectedThread.MostRecentStackFrame == null ||
			    debuggedProcess.IsRunning) {
				MessageBox.Show(errorCannotStepNoActiveFunction, "StepInto");
			} else {
				var frame = GetStackFrame();
				if (frame != null)
					frame.AsyncStepInto();
			}
		}
		
		public void StepOver()
		{
			if (!IsDebugging) {
				MessageBox.Show(errorNotDebugging, "StepOver");
				return;
				
			}
			// use most recent stack frame because we don't have the symbols
			if (debuggedProcess.SelectedThread == null ||
			    debuggedProcess.SelectedThread.MostRecentStackFrame == null ||
			    debuggedProcess.IsRunning) {
				MessageBox.Show(errorCannotStepNoActiveFunction, "StepOver");
			} else {
				var frame = GetStackFrame();
				if (frame != null)
					frame.AsyncStepOver();
			}
		}
		
		public void StepOut()
		{
			if (!IsDebugging) {
				MessageBox.Show(errorNotDebugging, "StepOut");
				return;
			}
			
			// use most recent stack frame because we don't have the symbols
			if (debuggedProcess.SelectedThread == null ||
			    debuggedProcess.SelectedThread.MostRecentStackFrame == null ||
			    debuggedProcess.IsRunning) {
				MessageBox.Show(errorCannotStepNoActiveFunction, "StepOut");
			} else {
				var frame = GetStackFrame();
				if (frame != null)
					frame.AsyncStepOut();
			}
		}
		
		public event EventHandler DebugStarting;
		public event EventHandler DebugStarted;
		public event EventHandler DebugStopped;
		public event EventHandler IsProcessRunningChanged;
		
		protected virtual void OnIsProcessRunningChanged(EventArgs e)
		{
			if (IsProcessRunningChanged != null) {
				IsProcessRunningChanged(this, e);
			}
		}
		
		/// <summary>
		/// Gets variable of given name.
		/// Returns null if unsuccessful. Can throw GetValueException.
		/// <exception cref="GetValueException">Thrown when evaluation fails. Exception message explains reason.</exception>
		/// </summary>
		public Value GetValueFromName(string variableName)
		{
			if (!CanEvaluate) {
				return null;
			}
			return ExpressionEvaluator.Evaluate(variableName, SupportedLanguage.CSharp, debuggedProcess.SelectedThread.MostRecentStackFrame);
		}
		
		/// <summary>
		/// Gets Expression for given variable. Can throw GetValueException.
		/// <exception cref="GetValueException">Thrown when getting expression fails. Exception message explains reason.</exception>
		/// </summary>
		public ICSharpCode.NRefactory.CSharp.Expression GetExpression(string variableName)
		{
			if (!CanEvaluate) {
				throw new GetValueException("Cannot evaluate now - debugged process is either null or running or has no selected stack frame");
			}
			return ExpressionEvaluator.ParseExpression(variableName, SupportedLanguage.CSharp);
		}
		
		public bool IsManaged(int processId)
		{
			corPublish = new CorpubPublishClass();
			CorDbg.Interop.TrackedComObjects.Track(corPublish);
			
			ICorPublishProcess process = corPublish.GetProcess((uint)processId);
			if (process != null) {
				return process.IsManaged() != 0;
			}
			return false;
		}
		
		/// <summary>
		/// Gets the current value of the variable as string that can be displayed in tooltips.
		/// Returns null if unsuccessful.
		/// </summary>
		public string GetValueAsString(string variableName)
		{
			try {
				Value val = GetValueFromName(variableName);
				if (val == null) return null;
				return val.AsString();
			} catch (GetValueException) {
				return null;
			}
		}
		
		/// <inheritdoc/>
		public bool CanEvaluate
		{
			get {
				return debuggedProcess != null &&
					!debuggedProcess.IsRunning &&
					debuggedProcess.SelectedThread != null &&
					debuggedProcess.SelectedThread.MostRecentStackFrame != null;
			}
		}
		
		/// <summary>
		/// Gets the tooltip control that shows the value of given variable.
		/// Return null if no tooltip is available.
		/// </summary>
		public object GetTooltipControl(AstLocation logicalPosition, string variableName)
		{
			try {
				var tooltipExpression = GetExpression(variableName);
				if (tooltipExpression == null) return null;
				
				string imageName;
				var image = ExpressionNode.GetImageForLocalVariable(out imageName);
				ExpressionNode expressionNode = new ExpressionNode(image, variableName, tooltipExpression);
				expressionNode.ImageName = imageName;
				
				return new DebuggerTooltipControl(logicalPosition, expressionNode);
			} catch (GetValueException) {
				return null;
			}
		}
		
		public ITreeNode GetNode(string variable, string currentImageName = null)
		{
			try {
				var expression = GetExpression(variable);
				string imageName;
				ImageSource image;
				if (string.IsNullOrEmpty(currentImageName)) {
					image = ExpressionNode.GetImageForLocalVariable(out imageName);
				}
				else {
					image = ImageService.GetImage(currentImageName);
					imageName = currentImageName;
				}
				ExpressionNode expressionNode = new ExpressionNode(image, variable, expression);
				expressionNode.ImageName = imageName;
				return expressionNode;
			} catch (GetValueException) {
				return null;
			}
		}
		
		public bool CanSetInstructionPointer(string filename, int line, int column)
		{
			if (debuggedProcess != null && debuggedProcess.IsPaused &&
			    debuggedProcess.SelectedThread != null && debuggedProcess.SelectedThread.MostRecentStackFrame != null) {
				SourcecodeSegment seg = debuggedProcess.SelectedThread.MostRecentStackFrame.CanSetIP(filename, line, column);
				return seg != null;
			} else {
				return false;
			}
		}
		
		public bool SetInstructionPointer(string filename, int line, int column)
		{
			if (CanSetInstructionPointer(filename, line, column)) {
				SourcecodeSegment seg = debuggedProcess.SelectedThread.MostRecentStackFrame.SetIP(filename, line, column);
				return seg != null;
			} else {
				return false;
			}
		}
		
		public void Dispose()
		{
			Stop();
		}
		
		#endregion
		
		public event EventHandler Initialize;
		
		public void InitializeService()
		{
			if (useRemotingForThreadInterop) {
				// This needs to be called before instance of NDebugger is created
				string path = RemotingConfigurationHelpper.GetLoadedAssemblyPath("Debugger.Core.dll");
				new RemotingConfigurationHelpper(path).Configure();
			}
			
			debugger = new NDebugger();
			
			//debugger.Options = DebuggingOptions.Instance;
			
			debugger.DebuggerTraceMessage    += debugger_TraceMessage;
			debugger.Processes.Added         += debugger_ProcessStarted;
			debugger.Processes.Removed       += debugger_ProcessExited;
			
			DebuggerService.BreakPointAdded  += delegate (object sender, BreakpointBookmarkEventArgs e) {
				AddBreakpoint(e.BreakpointBookmark);
			};
			
			foreach (BreakpointBookmark b in DebuggerService.Breakpoints) {
				AddBreakpoint(b);
			}
			
			if (Initialize != null) {
				Initialize(this, null);
			}
		}
		
		bool Compare(byte[] a, byte[] b)
		{
			if (a.Length != b.Length) return false;
			for(int i = 0; i < a.Length; i++) {
				if (a[i] != b[i]) return false;
			}
			return true;
		}
		
		void AddBreakpoint(BreakpointBookmark bookmark)
		{
			Breakpoint breakpoint = null;
			
			uint token;
			SourceCodeMapping map = CodeMappings
										.GetStorage(bookmark.Language)
										.GetInstructionByTypeAndLine(bookmark.Type.FullName, bookmark.LineNumber, out token);
			
			if (map != null) {
				breakpoint = new ILBreakpoint(
					debugger,
					bookmark.Type.FullName,
					bookmark.LineNumber,
					token,
					map.ILInstructionOffset.From,
					bookmark.IsEnabled);
			}
			
			if (breakpoint == null)
				return;
			
			debugger.Breakpoints.Add(breakpoint);
//			Action setBookmarkColor = delegate {
//				if (debugger.Processes.Count == 0) {
//					bookmark.IsHealthy = true;
//					bookmark.Tooltip = null;
//				} else if (!breakpoint.IsSet) {
//					bookmark.IsHealthy = false;
//					bookmark.Tooltip = "Breakpoint was not found in any loaded modules";
//				} else if (breakpoint.OriginalLocation.CheckSum == null) {
//					bookmark.IsHealthy = true;
//					bookmark.Tooltip = null;
//				} else {
//					byte[] fileMD5;
//					IEditable file = FileService.GetOpenFile(bookmark.FileName) as IEditable;
//					if (file != null) {
//						byte[] fileContent = Encoding.UTF8.GetBytesWithPreamble(file.Text);
//						fileMD5 = new MD5CryptoServiceProvider().ComputeHash(fileContent);
//					} else {
//						fileMD5 = new MD5CryptoServiceProvider().ComputeHash(File.ReadAllBytes(bookmark.FileName));
//					}
//					if (Compare(fileMD5, breakpoint.OriginalLocation.CheckSum)) {
//						bookmark.IsHealthy = true;
//						bookmark.Tooltip = null;
//					} else {
//						bookmark.IsHealthy = false;
//						bookmark.Tooltip = "Check sum or file does not match to the original";
//					}
//				}
//			};
			
			// event handlers on bookmark and breakpoint don't need deregistration
			bookmark.IsEnabledChanged += delegate {
				breakpoint.Enabled = bookmark.IsEnabled;
			};
			breakpoint.Set += delegate {
				//setBookmarkColor();
			};
			
			//setBookmarkColor();
			
			EventHandler<CollectionItemEventArgs<Process>> bp_debugger_ProcessStarted = (sender, e) => {
				//setBookmarkColor();
				// User can change line number by inserting or deleting lines
				breakpoint.Line = bookmark.LineNumber;
			};
			EventHandler<CollectionItemEventArgs<Process>> bp_debugger_ProcessExited = (sender, e) => {
				//setBookmarkColor();
			};
			
			EventHandler<BreakpointEventArgs> bp_debugger_BreakpointHit =
				new EventHandler<BreakpointEventArgs>(
					delegate(object sender, BreakpointEventArgs e)
					{
						//LoggingService.Debug(bookmark.Action + " " + bookmark.ScriptLanguage + " " + bookmark.Condition);
						
						switch (bookmark.Action) {
							case BreakpointAction.Break:
								break;
							case BreakpointAction.Condition:
//								if (Evaluate(bookmark.Condition, bookmark.ScriptLanguage))
//									DebuggerService.PrintDebugMessage(string.Format(StringParser.Parse("${res:MainWindow.Windows.Debug.Conditional.Breakpoints.BreakpointHitAtBecause}") + "\n", bookmark.LineNumber, bookmark.FileName, bookmark.Condition));
//								else
//									this.debuggedProcess.AsyncContinue();
								break;
							case BreakpointAction.Trace:
								//DebuggerService.PrintDebugMessage(string.Format(StringParser.Parse("${res:MainWindow.Windows.Debug.Conditional.Breakpoints.BreakpointHitAt}") + "\n", bookmark.LineNumber, bookmark.FileName));
								break;
						}
					});
			
			BookmarkEventHandler bp_bookmarkManager_Removed = null;
			bp_bookmarkManager_Removed = (sender, e) => {
				if (bookmark == e.Bookmark) {
					debugger.Breakpoints.Remove(breakpoint);
					
					// unregister the events
					debugger.Processes.Added -= bp_debugger_ProcessStarted;
					debugger.Processes.Removed -= bp_debugger_ProcessExited;
					breakpoint.Hit -= bp_debugger_BreakpointHit;
					BookmarkManager.Removed -= bp_bookmarkManager_Removed;
				}
			};
			// register the events
			debugger.Processes.Added += bp_debugger_ProcessStarted;
			debugger.Processes.Removed += bp_debugger_ProcessExited;
			breakpoint.Hit += bp_debugger_BreakpointHit;
			BookmarkManager.Removed += bp_bookmarkManager_Removed;
		}
		
		bool Evaluate(string code, string language)
		{
			try {
				SupportedLanguage supportedLanguage = (SupportedLanguage)Enum.Parse(typeof(SupportedLanguage), language, true);
				Value val = ExpressionEvaluator.Evaluate(code, supportedLanguage, debuggedProcess.SelectedThread.MostRecentStackFrame);
				
				if (val != null && val.Type.IsPrimitive && val.PrimitiveValue is bool)
					return (bool)val.PrimitiveValue;
				else
					return false;
			} catch (GetValueException e) {
				string errorMessage = "Error while evaluating breakpoint condition " + code + ":\n" + e.Message + "\n";
				//DebuggerService.PrintDebugMessage(errorMessage);
				//WorkbenchSingleton.SafeThreadAsyncCall(MessageService.ShowWarning, errorMessage);
				return true;
			}
		}
		
		void LogMessage(object sender, MessageEventArgs e)
		{
			//DebuggerService.PrintDebugMessage(e.Message);
		}
		
		void debugger_TraceMessage(object sender, MessageEventArgs e)
		{
			//LoggingService.Debug("Debugger: " + e.Message);
		}
		
		void debugger_ProcessStarted(object sender, CollectionItemEventArgs<Process> e)
		{
			if (debugger.Processes.Count == 1) {
				if (DebugStarted != null) {
					DebugStarted(this, EventArgs.Empty);
				}
			}
			e.Item.LogMessage += LogMessage;
		}
		
		void debugger_ProcessExited(object sender, CollectionItemEventArgs<Process> e)
		{
			if (debugger.Processes.Count == 0) {
				if (DebugStopped != null) {
					DebugStopped(this, e);
				}
				SelectProcess(null);
			} else {
				SelectProcess(debugger.Processes[0]);
			}
		}
		
		public void SelectProcess(Process process)
		{
			if (debuggedProcess != null) {
				debuggedProcess.Paused          -= debuggedProcess_DebuggingPaused;
				debuggedProcess.ExceptionThrown -= debuggedProcess_ExceptionThrown;
				debuggedProcess.Resumed         -= debuggedProcess_DebuggingResumed;
				debuggedProcess.ModulesAdded 	-= debuggedProcess_ModulesAdded;
			}
			debuggedProcess = process;
			if (debuggedProcess != null) {
				debuggedProcess.Paused          += debuggedProcess_DebuggingPaused;
				debuggedProcess.ExceptionThrown += debuggedProcess_ExceptionThrown;
				debuggedProcess.Resumed         += debuggedProcess_DebuggingResumed;
				debuggedProcess.ModulesAdded 	+= debuggedProcess_ModulesAdded;
				
				debuggedProcess.BreakAtBeginning = BreakAtBeginning;
			}
			// reset
			BreakAtBeginning = false;
			
			JumpToCurrentLine();
			OnProcessSelected(new ProcessEventArgs(process));
		}

		void debuggedProcess_ModulesAdded(object sender, ModuleEventArgs e)
		{
			foreach (var bookmark in DebuggerService.Breakpoints) {
				var breakpoint =
					debugger.Breakpoints.FirstOrDefault(
						b => b.Line == bookmark.LineNumber && b.TypeName == bookmark.Type.FullName);
				if (breakpoint == null)
					continue;
				
				breakpoint.SetBreakpoint(e.Module);
			}
		}
		
		void debuggedProcess_DebuggingPaused(object sender, ProcessEventArgs e)
		{
			JumpToCurrentLine();
			OnIsProcessRunningChanged(EventArgs.Empty);
		}
		
		void debuggedProcess_DebuggingResumed(object sender, CorDbg.ProcessEventArgs e)
		{
			OnIsProcessRunningChanged(EventArgs.Empty);
			DebuggerService.RemoveCurrentLineMarker();
		}
		
		void debuggedProcess_ExceptionThrown(object sender, CorDbg.ExceptionEventArgs e)
		{
			if (!e.IsUnhandled) {
				// Ignore the exception
				e.Process.AsyncContinue();
				return;
			}
			
			JumpToCurrentLine();
			
			StringBuilder stacktraceBuilder = new StringBuilder();
			
			// Need to intercept now so that we can evaluate properties
			if (e.Process.SelectedThread.InterceptCurrentException()) {
				stacktraceBuilder.AppendLine(e.Exception.ToString());
				string stackTrace;
				try {
					stackTrace = e.Exception.GetStackTrace("--- End of inner exception stack trace ---");
				} catch (GetValueException) {
					stackTrace = e.Process.SelectedThread.GetStackTrace("at {0} in {1}:line {2}", "at {0}");
				}
				stacktraceBuilder.Append(stackTrace);
			} else {
				// For example, happens on stack overflow
				stacktraceBuilder.AppendLine("CannotInterceptException");
				stacktraceBuilder.AppendLine(e.Exception.ToString());
				stacktraceBuilder.Append(e.Process.SelectedThread.GetStackTrace("at {0} in {1}:line {2}", "at {0}"));
			}
			
			string title = e.IsUnhandled ? "Unhandled" : "Handled";
			string message = string.Format("Message {0} {1}", e.Exception.Type, e.Exception.Message);
			
			MessageBox.Show(message + stacktraceBuilder.ToString(), title);
		}
		
		public void JumpToCurrentLine()
		{
			DebuggerService.RemoveCurrentLineMarker();
			
			if (debuggedProcess != null &&  debuggedProcess.SelectedThread != null) {
				
				// use most recent stack frame because we don't have the symbols
				var frame = debuggedProcess.SelectedThread.MostRecentStackFrame;
				
				if (frame == null)
					return;
				
				uint token = (uint)frame.MethodInfo.MetadataToken;
				int ilOffset = frame.IP;
				int line;
				TypeDefinition type;
				if (CodeMappingsStorage.GetSourceCodeFromMetadataTokenAndOffset(token, ilOffset, out type, out line)) {
					DebuggerService.JumpToCurrentLine(type, line, 0, line, 0);
				}
			}
		}
		
		public void ShowAttachDialog()
		{
			throw new NotImplementedException();
		}
	}
}
