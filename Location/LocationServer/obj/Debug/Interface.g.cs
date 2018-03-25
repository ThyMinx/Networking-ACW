﻿#pragma checksum "..\..\Interface.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "98BF79B42CC20BBB4783B10028FF57A7"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace LocationServer {
    
    
    /// <summary>
    /// Interface
    /// </summary>
    public partial class Interface : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 22 "..\..\Interface.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label l_database;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\Interface.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox t_database;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\Interface.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label l_logfile;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\Interface.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox t_logfile;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\Interface.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label l_debug;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\Interface.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox cb_debug;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\Interface.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label l_timeout;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\Interface.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox t_timeout;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\Interface.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label l_output;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\Interface.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox t_output;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\Interface.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button B_Run;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/LocationServer;component/interface.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\Interface.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.l_database = ((System.Windows.Controls.Label)(target));
            return;
            case 2:
            this.t_database = ((System.Windows.Controls.TextBox)(target));
            
            #line 23 "..\..\Interface.xaml"
            this.t_database.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.t_database_TextChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.l_logfile = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.t_logfile = ((System.Windows.Controls.TextBox)(target));
            
            #line 26 "..\..\Interface.xaml"
            this.t_logfile.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.t_logfile_TextChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.l_debug = ((System.Windows.Controls.Label)(target));
            return;
            case 6:
            this.cb_debug = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 7:
            this.l_timeout = ((System.Windows.Controls.Label)(target));
            return;
            case 8:
            this.t_timeout = ((System.Windows.Controls.TextBox)(target));
            
            #line 32 "..\..\Interface.xaml"
            this.t_timeout.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.t_timeout_TextChanged);
            
            #line default
            #line hidden
            return;
            case 9:
            this.l_output = ((System.Windows.Controls.Label)(target));
            return;
            case 10:
            this.t_output = ((System.Windows.Controls.TextBox)(target));
            return;
            case 11:
            this.B_Run = ((System.Windows.Controls.Button)(target));
            
            #line 37 "..\..\Interface.xaml"
            this.B_Run.Click += new System.Windows.RoutedEventHandler(this.B_Run_Click_1);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
