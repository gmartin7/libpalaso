﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17626
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Palaso.Reporting {


	[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
	internal sealed partial class ErrorReportSettings : global::System.Configuration.ApplicationSettingsBase {

		private static ErrorReportSettings defaultInstance = ((ErrorReportSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new ErrorReportSettings())));

		public static ErrorReportSettings Default {
			get {
				return defaultInstance;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("mapiWithPopup")]
		public string ReportingMethod {
			get {
				return ((string)(this["ReportingMethod"]));
			}
			set {
				this["ReportingMethod"] = value;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("")]
		public string AnalyticsCookie {
			get {
				return ((string)(this["AnalyticsCookie"]));
			}
			set {
				this["AnalyticsCookie"] = value;
			}
		}
	}
}
