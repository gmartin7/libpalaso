#if MONO
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Palaso.Email;
using Palaso.Reporting;
using System.IO;

namespace Palaso.Reporting
{
	 public class ExceptionReportingDialog : IDisposable
	{
		private bool _isLethal;
		private static bool s_doIgnoreReport = false;
		private string _details;


		protected ExceptionReportingDialog(bool isLethal)
		{
			_isLethal = isLethal;
		}

		#region IDisposable override

		/// <summary>
		/// Check to see if the object has been disposed.
		/// All public Properties and Methods should call this
		/// before doing anything else.
		/// </summary>
// Dispose() calls Dispose(true)
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"></param>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the bug.
		///
		/// If subclasses override this method, they should call the base implementation.
		/// </remarks>
		protected void Dispose(bool disposing)
		{

			if (disposing)
			{
				// Dispose managed resources here.
			}

			// Dispose unmanaged resources here, whether disposing is true or false.

		}

		#endregion IDisposable override

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// show a dialog or output to the error log, as appropriate.
		/// </summary>
		/// <param name="error">the exception you want to report</param>
		/// ------------------------------------------------------------------------------------
		internal static void ReportException(Exception error)
		{
			ReportException(error, null);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="error"></param>
		/// <param name="parent"></param>
		internal static void ReportException(Exception error, Form parent)
		{
			ReportException(error, null, true);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// show a dialog or output to the error log, as appropriate.
		/// </summary>
		/// <param name="error">the exception you want to report</param>
		/// <param name="parent">the parent form that this error belongs to (i.e. the form
		/// show modally on)</param>
		/// ------------------------------------------------------------------------------------
		/// <param name="isLethal"></param>
		internal static void ReportException(Exception error, Form parent, bool isLethal)
		{
			if (s_doIgnoreReport)
			{
				return;            // ignore message if we are showing from a previous error
			}

			using (ExceptionReportingDialog dlg = new ExceptionReportingDialog(isLethal))
			{
				dlg.Report(error, parent);
			}
		}

		internal static void ReportMessage(string message, StackTrace stack, bool isLethal)
		{
			if (s_doIgnoreReport)
			{
				return;            // ignore message if we are showing from a previous error
			}

			using (ExceptionReportingDialog dlg = new ExceptionReportingDialog(isLethal))
			{
				dlg.Report(message, string.Empty, stack, null);
			}
		}
		internal static void ReportMessage(string message, Exception error, bool isLethal)
		{
			if (s_doIgnoreReport)
			{
				return;            // ignore message if we are showing from a previous error
			}

			using (ExceptionReportingDialog dlg = new ExceptionReportingDialog(isLethal))
			{
				dlg.Report(message, null, error,null);
			}
		}

		 public void Report(Exception error, Form owningForm)
		{
			Report(null,null, error, owningForm);
		}


		 public void Report(string message, string messageBeforeStack, Exception error, Form owningForm)
		 {
			 try
			 {
				 if(!string.IsNullOrEmpty(message))
					UsageReporter.ReportExceptionString(message);
				 else if(error!=null)
					 UsageReporter.ReportException(error);
			 }
			 catch
			 {
				 //swallow
			 }

			 if (!string.IsNullOrEmpty(message))
			 {
				Console.WriteLine ("Message (not an exception): " + message);
				_details += "Message (not an exception): " + message + Environment.NewLine;
				_details += Environment.NewLine;
			 }
			if (!string.IsNullOrEmpty(messageBeforeStack))
			 {
				Console.WriteLine (messageBeforeStack);
				 _details += messageBeforeStack;
				 _details += Environment.NewLine;
			 }

			 Exception innerMostException = null;
			 _details += ErrorReport.GetHiearchicalExceptionInfo(error, ref innerMostException);
			Console.WriteLine(ErrorReport.GetHiearchicalExceptionInfo(error, ref innerMostException));

			 //if the exception had inner exceptions, show the inner-most exception first, since that is usually the one
			 //we want the developer to read.
			 if (innerMostException != null)
			 {
				 _details += "Inner-most exception:\r\n" + ErrorReport.GetExceptionText(innerMostException) +
								  "\r\n\r\nFull, hierarchical exception contents:\r\n" + _details;
				Console.WriteLine ("Inner-most exception:\r\n" + ErrorReport.GetExceptionText(innerMostException) +
								  "\r\n\r\nFull, hierarchical exception contents:\r\n");
			 }

			 AddErrorReportingPropertiesToDetails();


			 Debug.WriteLine(_details);
			 if (innerMostException != null)
			 {
				 error = innerMostException;
			 }


			 try
			 {
				 Logger.WriteEvent("Got exception " + error.GetType().Name);
			 }
			 catch (Exception err)
			 {
				 //We have more than one report of dieing while logging an exception.
				Console.WriteLine ("****Could not write to log (" + err.Message + ")");
				 _details += "****Could not write to log (" + err.Message + ")" + Environment.NewLine;
				Console.WriteLine ("****Could not write to log (" + err.Message + ")");
				 _details += "Was trying to log the exception: " + error.Message + Environment.NewLine;
				Console.WriteLine ("Recent events:");
				 _details += "Recent events:" + Environment.NewLine;
				Console.WriteLine (Logger.MinorEventsLog);
				 _details += Logger.MinorEventsLog;
			 }

			 ShowReportDialogIfAppropriate(owningForm);
		 }

		 public void Report(string message, string messageBeforeStack, StackTrace stackTrace, Form owningForm)
		 {

			_details += "Message (not an exception): " + message + Environment.NewLine;
			Console.WriteLine ("Message (not an exception): " + message);
			 _details += Environment.NewLine;
			 if(!string.IsNullOrEmpty(messageBeforeStack))
			 {
				_details += messageBeforeStack;
				_details += Environment.NewLine;
				Console.WriteLine(messageBeforeStack);
			 }
			_details += "--Stack--"+ Environment.NewLine;;
			Console.WriteLine ("--Stack--");
			 _details += stackTrace.ToString() + Environment.NewLine; ;
			Console.WriteLine (stackTrace.ToString());

			 AddErrorReportingPropertiesToDetails();

			 Debug.WriteLine(_details);


			 try
			 {
				 Logger.WriteEvent("Got error message " + message);
			 }
			 catch (Exception err)
			 {
				 //We have more than one report of dieing while logging an exception.
				Console.WriteLine ("****Could not write to log (" + err.Message + ")");
				 _details += "****Could not write to log (" + err.Message + ")" + Environment.NewLine;
			 }

			 ShowReportDialogIfAppropriate(owningForm);
		 }

		 private void AddErrorReportingPropertiesToDetails()
		 {

			 _details += Environment.NewLine+"--Error Reporting Properties--"+Environment.NewLine;
			Console.WriteLine("--Error Reporting Properties--");
			 foreach (string label in ErrorReport.Properties.Keys)
			 {
				 _details += label + ": " + ErrorReport.Properties[label] + Environment.NewLine;
				Console.WriteLine(label + ": " + ErrorReport.Properties[label]);
			 }

			 _details += Environment.NewLine+"--Log--"+Environment.NewLine;
			Console.WriteLine ("--Log--");
			 try
			 {
				 _details += Logger.LogText;
				Console.WriteLine (Logger.LogText);
			 }
			 catch (Exception err)
			 {
				 //We have more than one report of dieing while logging an exception.
				Console.WriteLine ("****Could not read from log: " + err.Message);
				 _details += "****Could not read from log: " + err.Message + Environment.NewLine;
			 }
		 }


		 private void ShowReportDialogIfAppropriate(Form owningForm)
		 {

			 if (ErrorReport.IsOkToInteractWithUser)
			 {
				 s_doIgnoreReport = true;
				// DG: May 2012 Starting a new process with the reporting dialog
				// in a separate program so that problems with mono SWF don't
				// stop you getting the error log

				string tempFileName = Path.GetTempFileName();

				File.WriteAllText(tempFileName, _details);

				string commandLineArgs;
				if (_isLethal)
					commandLineArgs = "1";
				else
					commandLineArgs = "0";

				commandLineArgs += " '" + tempFileName + "'";
				commandLineArgs += " '"+ ErrorReport.EmailAddress + "'";
				commandLineArgs += " '"+ ErrorReport.EmailSubject + "'";

				string dir = Directory.GetParent(Application.ExecutablePath).FullName;

				var p = new Process
				{
					StartInfo =
					{
						FileName = Path.Combine(dir, "PalasoError.exe"),
						Arguments = commandLineArgs,
						UseShellExecute = true,
						ErrorDialog = true
					}
				};

				p.Start();
				s_doIgnoreReport = false;

				if (_isLethal)
				{
					Process.GetCurrentProcess().Kill();
				}
			 }
			 else //the test environment already prohibits dialogs but will save the contents of assertions in some log.
			 {
				 Debug.Fail(_details);
			 }
		 }
	}
}
#else
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Palaso.Email;
using Palaso.Reporting;

namespace Palaso.Reporting
{
	 public class ExceptionReportingDialog : Form, IDisposable
	{
		#region Member variables

		private Label label3;
		private TextBox _details;
		private TextBox _pleaseHelpText;
		private TextBox m_reproduce;

		private bool _isLethal;

		private Button _sendAndCloseButton;
		 private TextBox _notificationText;
		 private TextBox textBox1;
		 private ComboBox _methodCombo;
		 private Button _privacyNoticeButton;
		 private Label _emailAddress;
		private static bool s_doIgnoreReport = false;

		#endregion

		protected ExceptionReportingDialog(bool isLethal)
		{
			_isLethal = isLethal;
		}

		#region IDisposable override

		/// <summary>
		/// Check to see if the object has been disposed.
		/// All public Properties and Methods should call this
		/// before doing anything else.
		/// </summary>
		public void CheckDisposed()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(String.Format("'{0}' in use after being disposed.", GetType().Name));
			}
		}

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"></param>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the bug.
		///
		/// If subclasses override this method, they should call the base implementation.
		/// </remarks>
		protected override void Dispose(bool disposing)
		{
			//Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Must not be run more than once.
			if (IsDisposed)
			{
				return;
			}

			if (disposing)
			{
				// Dispose managed resources here.
			}

			// Dispose unmanaged resources here, whether disposing is true or false.

			base.Dispose(disposing);
		}

		#endregion IDisposable override

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExceptionReportingDialog));
			this.m_reproduce = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this._details = new System.Windows.Forms.TextBox();
			this._sendAndCloseButton = new System.Windows.Forms.Button();
			this._pleaseHelpText = new System.Windows.Forms.TextBox();
			this._notificationText = new System.Windows.Forms.TextBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this._methodCombo = new System.Windows.Forms.ComboBox();
			this._privacyNoticeButton = new System.Windows.Forms.Button();
			this._emailAddress = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// m_reproduce
			//
			this.m_reproduce.AcceptsReturn = true;
			this.m_reproduce.AcceptsTab = true;
			resources.ApplyResources(this.m_reproduce, "m_reproduce");
			this.m_reproduce.Name = "m_reproduce";
			//
			// label3
			//
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			//
			// _details
			//
			resources.ApplyResources(this._details, "_details");
			this._details.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this._details.Name = "_details";
			this._details.ReadOnly = true;
			//
			// _sendAndCloseButton
			//
			resources.ApplyResources(this._sendAndCloseButton, "_sendAndCloseButton");
			this._sendAndCloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._sendAndCloseButton.Name = "_sendAndCloseButton";
			this._sendAndCloseButton.Click += new System.EventHandler(this.btnClose_Click);
			//
			// _pleaseHelpText
			//
			resources.ApplyResources(this._pleaseHelpText, "_pleaseHelpText");
			this._pleaseHelpText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this._pleaseHelpText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._pleaseHelpText.ForeColor = System.Drawing.Color.Black;
			this._pleaseHelpText.Name = "_pleaseHelpText";
			this._pleaseHelpText.ReadOnly = true;
			//
			// _notificationText
			//
			resources.ApplyResources(this._notificationText, "_notificationText");
			this._notificationText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this._notificationText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._notificationText.ForeColor = System.Drawing.Color.Black;
			this._notificationText.Name = "_notificationText";
			this._notificationText.ReadOnly = true;
			//
			// textBox1
			//
			resources.ApplyResources(this.textBox1, "textBox1");
			this.textBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox1.ForeColor = System.Drawing.Color.Black;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			//
			// _methodCombo
			//
			resources.ApplyResources(this._methodCombo, "_methodCombo");
			this._methodCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._methodCombo.FormattingEnabled = true;
			this._methodCombo.Name = "_methodCombo";
			this._methodCombo.SelectedIndexChanged += new System.EventHandler(this._methodCombo_SelectedIndexChanged);
			//
			// _privacyNoticeButton
			//
			resources.ApplyResources(this._privacyNoticeButton, "_privacyNoticeButton");
			this._privacyNoticeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._privacyNoticeButton.Image = global::Palaso.Properties.Resources.spy16x16;
			this._privacyNoticeButton.Name = "_privacyNoticeButton";
			this._privacyNoticeButton.UseVisualStyleBackColor = true;
			this._privacyNoticeButton.Click += new System.EventHandler(this._privacyNoticeButton_Click);
			//
			// _emailAddress
			//
			resources.ApplyResources(this._emailAddress, "_emailAddress");
			this._emailAddress.ForeColor = System.Drawing.Color.DimGray;
			this._emailAddress.Name = "_emailAddress";
			//
			// ExceptionReportingDialog
			//
			this.AcceptButton = this._sendAndCloseButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.ControlBox = false;
			this.Controls.Add(this._emailAddress);
			this.Controls.Add(this._privacyNoticeButton);
			this.Controls.Add(this._methodCombo);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.m_reproduce);
			this.Controls.Add(this._notificationText);
			this.Controls.Add(this._pleaseHelpText);
			this.Controls.Add(this._details);
			this.Controls.Add(this.label3);
			this.Controls.Add(this._sendAndCloseButton);
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExceptionReportingDialog";
			this.TopMost = true;
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ExceptionReportingDialog_KeyPress);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		 private void SetupMethodCombo()
		 {
			 _methodCombo.Items.Clear();
			 _methodCombo.Items.Add(new ReportingMethod("Send using my email program", "&Email", "mapiWithPopup", SendViaEmail));
			 _methodCombo.Items.Add(new ReportingMethod("Copy to clipboard", "&Copy", "clipboard", PutOnClipboard));
		 }

		 class ReportingMethod
		 {
			 private readonly string _label;
			 public readonly string CloseButtonLabel;
			 public readonly string Id;
			 public readonly Func<bool> Method;

			 public ReportingMethod(string label, string closeButtonLabel, string id, Func<bool> method)
			 {
				 _label = label;
				 CloseButtonLabel = closeButtonLabel;
				 Id = id;
				 Method = method;
			 }
			 public override string ToString()
			 {
				 return _label;
			 }
		 }
		#endregion



		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// show a dialog or output to the error log, as appropriate.
		/// </summary>
		/// <param name="error">the exception you want to report</param>
		/// ------------------------------------------------------------------------------------
		internal static void ReportException(Exception error)
		{
			ReportException(error, null);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="error"></param>
		/// <param name="parent"></param>
		internal static void ReportException(Exception error, Form parent)
		{
			ReportException(error, null, true);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// show a dialog or output to the error log, as appropriate.
		/// </summary>
		/// <param name="error">the exception you want to report</param>
		/// <param name="parent">the parent form that this error belongs to (i.e. the form
		/// show modally on)</param>
		/// ------------------------------------------------------------------------------------
		/// <param name="isLethal"></param>
		internal static void ReportException(Exception error, Form parent, bool isLethal)
		{
			if (s_doIgnoreReport)
			{
				return;            // ignore message if we are showing from a previous error
			}

			using (ExceptionReportingDialog dlg = new ExceptionReportingDialog(isLethal))
			{
				dlg.Report(error, parent);
			}
		}

		internal static void ReportMessage(string message, StackTrace stack, bool isLethal)
		{
			if (s_doIgnoreReport)
			{
				return;            // ignore message if we are showing from a previous error
			}

			using (ExceptionReportingDialog dlg = new ExceptionReportingDialog(isLethal))
			{
				dlg.Report(message, string.Empty, stack, null);
			}
		}
		internal static void ReportMessage(string message, Exception error, bool isLethal)
		{
			if (s_doIgnoreReport)
			{
				return;            // ignore message if we are showing from a previous error
			}

			using (ExceptionReportingDialog dlg = new ExceptionReportingDialog(isLethal))
			{
				dlg.Report(message, null, error,null);
			}
		}
		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected void GatherData()
		{
			_details.Text += "\r\nTo Reproduce: " + m_reproduce.Text + "\r\n";
		}

		 public void Report(Exception error, Form owningForm)
		{
			Report(null,null, error, owningForm);
		}


		 public void Report(string message, string messageBeforeStack, Exception error, Form owningForm)
		 {
			 try
			 {
				 if(!string.IsNullOrEmpty(message))
					UsageReporter.ReportExceptionString(message);
				 else if(error!=null)
					 UsageReporter.ReportException(error);
			 }
			 catch
			 {
				 //swallow
			 }

			 PrepareDialog();
			 if(!string.IsNullOrEmpty(message))
				 _notificationText.Text = message;

			 if (!string.IsNullOrEmpty(message))
			 {
				Console.WriteLine ("Message (not an exception): " + message);
				_details.Text += "Message (not an exception): " + message + Environment.NewLine;
				_details.Text += Environment.NewLine;
			 }
			if (!string.IsNullOrEmpty(messageBeforeStack))
			 {
				Console.WriteLine (messageBeforeStack);
				 _details.Text += messageBeforeStack;
				 _details.Text += Environment.NewLine;
			 }

			 Exception innerMostException = null;
			 _details.Text += ErrorReport.GetHiearchicalExceptionInfo(error, ref innerMostException);
			Console.WriteLine(ErrorReport.GetHiearchicalExceptionInfo(error, ref innerMostException));

			 //if the exception had inner exceptions, show the inner-most exception first, since that is usually the one
			 //we want the developer to read.
			 if (innerMostException != null)
			 {
				 _details.Text += "Inner-most exception:\r\n" + ErrorReport.GetExceptionText(innerMostException) +
								  "\r\n\r\nFull, hierarchical exception contents:\r\n" + _details.Text;
				Console.WriteLine ("Inner-most exception:\r\n" + ErrorReport.GetExceptionText(innerMostException) +
								  "\r\n\r\nFull, hierarchical exception contents:\r\n");
			 }

			 AddErrorReportingPropertiesToDetails();


			 Debug.WriteLine(_details.Text);
			 if (innerMostException != null)
			 {
				 error = innerMostException;
			 }


			 try
			 {
				 Logger.WriteEvent("Got exception " + error.GetType().Name);
			 }
			 catch (Exception err)
			 {
				 //We have more than one report of dieing while logging an exception.
				Console.WriteLine ("****Could not write to log (" + err.Message + ")");
				 _details.Text += "****Could not write to log (" + err.Message + ")" + Environment.NewLine;
				Console.WriteLine ("****Could not write to log (" + err.Message + ")");
				 _details.Text += "Was trying to log the exception: " + error.Message + Environment.NewLine;
				Console.WriteLine ("Recent events:");
				 _details.Text += "Recent events:" + Environment.NewLine;
				Console.WriteLine (Logger.MinorEventsLog);
				 _details.Text += Logger.MinorEventsLog;
			 }

			 ShowReportDialogIfAppropriate(owningForm);
		 }

		 public void Report(string message, string messageBeforeStack, StackTrace stackTrace, Form owningForm)
		 {
			 PrepareDialog();
			 _notificationText.Text = message;

			_details.Text += "Message (not an exception): " + message + Environment.NewLine;
			Console.WriteLine ("Message (not an exception): " + message);
			 _details.Text += Environment.NewLine;
			 if(!string.IsNullOrEmpty(messageBeforeStack))
			 {
				_details.Text += messageBeforeStack;
				_details.Text += Environment.NewLine;
				Console.WriteLine(messageBeforeStack);
			 }
			_details.Text += "--Stack--"+ Environment.NewLine;;
			Console.WriteLine ("--Stack--");
			 _details.Text += stackTrace.ToString() + Environment.NewLine; ;
			Console.WriteLine (stackTrace.ToString());

			 AddErrorReportingPropertiesToDetails();

			 Debug.WriteLine(_details.Text);


			 try
			 {
				 Logger.WriteEvent("Got error message " + message);
			 }
			 catch (Exception err)
			 {
				 //We have more than one report of dieing while logging an exception.
				Console.WriteLine ("****Could not write to log (" + err.Message + ")");
				 _details.Text += "****Could not write to log (" + err.Message + ")" + Environment.NewLine;
			 }

			 ShowReportDialogIfAppropriate(owningForm);
		 }

		 private void AddErrorReportingPropertiesToDetails()
		 {

			 _details.Text += Environment.NewLine+"--Error Reporting Properties--"+Environment.NewLine;
			Console.WriteLine("--Error Reporting Properties--");
			 foreach (string label in ErrorReport.Properties.Keys)
			 {
				 _details.Text += label + ": " + ErrorReport.Properties[label] + Environment.NewLine;
				Console.WriteLine(label + ": " + ErrorReport.Properties[label]);
			 }

			 _details.Text += Environment.NewLine+"--Log--"+Environment.NewLine;
			Console.WriteLine ("--Log--");
			 try
			 {
				 _details.Text += Logger.LogText;
				Console.WriteLine (Logger.LogText);
			 }
			 catch (Exception err)
			 {
				 //We have more than one report of dieing while logging an exception.
				Console.WriteLine ("****Could not read from log: " + err.Message);
				 _details.Text += "****Could not read from log: " + err.Message + Environment.NewLine;
			 }
		 }

		 private void PrepareDialog()
		 {
			 CheckDisposed();
			 Font = SystemFonts.MessageBoxFont;

			 //
			 // Required for Windows Form Designer support
			 //
			 InitializeComponent();
			 _emailAddress.Text = ErrorReport.EmailAddress;
			 SetupMethodCombo();

			 foreach (ReportingMethod  method in _methodCombo.Items)
			 {
				 if (ErrorReportSettings.Default.ReportingMethod == method.Id)
				 {
					SelectedMethod = method;
					break;
				 }
			 }

			 if (!_isLethal)
			 {
				 BackColor = Color.FromArgb(255, 255, 192); //yellow
				_notificationText.Text = "Take Courage. It'll work out.";
				 _notificationText.BackColor = BackColor;
				 _pleaseHelpText.BackColor = BackColor;
				 textBox1.BackColor = BackColor;
			 }

			 SetupCloseButtonText();
		 }

		 private void ShowReportDialogIfAppropriate(Form owningForm)
		 {


			 if (ErrorReport.IsOkToInteractWithUser)
			 {
				 s_doIgnoreReport = true;
				 ShowDialog(owningForm);
				 s_doIgnoreReport = false;
			 }
			 else //the test environment already prohibits dialogs but will save the contents of assertions in some log.
			 {
				 Debug.Fail(_details.Text);
			 }


		 }


		 /// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		private void btnClose_Click(object sender, EventArgs e)
		{
			 ErrorReportSettings.Default.ReportingMethod = ((ReportingMethod) (_methodCombo.SelectedItem)).Id;
			 ErrorReportSettings.Default.Save();

			if (ModifierKeys.Equals(Keys.Shift))
			{
				return;
			}
			GatherData();

		   // Clipboard.SetDataObject(_details.Text, true);

			if (SelectedMethod.Method())
			{
				CloseUp();
			}
			else
			{
				PutOnClipboard();
				CloseUp();
			}
		}

		 private bool PutOnClipboard()
		 {
			if (ErrorReport.EmailAddress != null)
			{
				_details.Text = String.Format(ReportingStrings.ksPleaseEMailThisToUs, ErrorReport.EmailAddress, _details.Text);
			}
			Clipboard.SetDataObject(_details.Text, true);
			 return true;
		 }

		 private bool SendViaEmail()
		 {
			 try
			 {
				 var emailProvider = EmailProviderFactory.PreferredEmailProvider();
				 var emailMessage = emailProvider.CreateMessage();
				 emailMessage.To.Add(ErrorReport.EmailAddress);
				 emailMessage.Subject = ErrorReport.EmailSubject;
				 emailMessage.Body = _details.Text;
				 if (emailMessage.Send(emailProvider))
				 {
					 CloseUp();
					 return true;
				 }
			 }
			 catch (Exception)
			 {
				 //swallow it and go to the alternate method
			 }

			 try
			 {
				 //EmailMessage msg = new EmailMessage();
				 // This currently does not work. The main issue seems to be the length of the error report. mailto
				 // apparently has some limit on the length of the message, and we are exceeding that.
				 var emailProvider = EmailProviderFactory.PreferredEmailProvider();
				 var emailMessage = emailProvider.CreateMessage();
				 emailMessage.To.Add(ErrorReport.EmailAddress);
				 emailMessage.Subject = ErrorReport.EmailSubject;
				 if (Environment.OSVersion.Platform == PlatformID.Unix)
				 {
					 emailMessage.Body = _details.Text;
				 }
				 else
				 {
					 PutOnClipboard();
					 emailMessage.Body = "<Details of the crash have been copied to the clipboard. Please paste them here>";
				 }
				 if (emailMessage.Send(emailProvider))
				 {
					 CloseUp();
					 return true;
				 }
			 }
			 catch (Exception error)
			 {
				 PutOnClipboard();
				 ErrorReport.NotifyUserOfProblem(error,
					 "This program wasn't able to get your email program, if you have one, to send the error message.  The contents of the error message has been placed on your Clipboard.");
				 return false;
			 }
			 return false;
		 }

		 private void CloseUp()
		{
			if (!_isLethal || ModifierKeys.Equals(Keys.Shift))
			{
				try
				{
					Logger.WriteEvent("Error Dialog: Continuing...");
				}
				catch (Exception)
				{
					//really can't handle an embedded error related to logging
				}
				Close();
				return;
			}
			try
			{
				Logger.WriteEvent("Error Dialog: Exiting...");
			}
			catch (Exception)
			{
				//really can't handle an embedded error related to logging
			}

			Process.GetCurrentProcess().Kill();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Shows the attempt to continue label if the shift key is pressed
		/// </summary>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ShiftKey && Visible)
			{
				_sendAndCloseButton.Text = "Continue";
			}
			base.OnKeyDown(e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Hides the attempt to continue label if the shift key is pressed
		/// </summary>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ShiftKey && Visible)
			{
				SetupCloseButtonText();
			}
			base.OnKeyUp(e);
		}

		 private void OnJustExit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		 {
			 CloseUp();
		 }

		 private void _methodCombo_SelectedIndexChanged(object sender, EventArgs e)
		 {
			 SetupCloseButtonText();
		 }

		 private void SetupCloseButtonText()
		 {
			_sendAndCloseButton.Text = SelectedMethod.CloseButtonLabel;
			 if (!_isLethal)
			 {
				// _dontSendEmailLink.Text = "Don't Send Email";
			 }
			 else
			 {
				 _sendAndCloseButton.Text += " and Exit";
			 }
		 }

		 private ReportingMethod SelectedMethod
		 {
			 get { return ((ReportingMethod) _methodCombo.SelectedItem); }
			 set { _methodCombo.SelectedItem = value; }
		 }

		 private void _privacyNoticeButton_Click(object sender, EventArgs e)
		 {
			MessageBox.Show(
				@"If you don't care who reads your bug report, you can skip this notice.

When you submit a crash report or other issue, the contents of your email go in our issue tracking system, 'jira', which is available via the web at http://jira.palso.org/issues. This is the normal way to handle issues in an open-source project.

Our issue-tracking system is not searchable by those without an account. Therefore, someone searching via Google will not find your bug reports.

However, anyone can make an account and then read what you sent us. So if you have something private to say, please send it to one of the developers privately with a note that you don't want the issue in our issue tracking system. If need be, we'll make some kind of sanitized place-holder for your issue so that we don't lose it.
");
		 }

		 private void ExceptionReportingDialog_KeyPress(object sender, KeyPressEventArgs e)
		 {
			 if(e.KeyChar== 27)//ESCAPE
			 {
				CloseUp();
			 }
		 }
	}
}
#endif
