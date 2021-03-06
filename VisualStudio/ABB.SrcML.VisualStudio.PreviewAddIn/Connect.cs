/******************************************************************************
 * Copyright (c) 2011 ABB Group
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors:
 *    Vinay Augustine (ABB Group) - initial API, implementation, & documentation
 *****************************************************************************/

using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace ABB.SrcML.VisualStudio.PreviewAddIn
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
        }

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;
            _toolWindows = (Windows2)_applicationObject.Windows;
            _buildEvents = _applicationObject.DTE.Events.BuildEvents;
            _solutionEvents = _applicationObject.DTE.Events.SolutionEvents;

            switch (connectMode)
            {
                case ext_ConnectMode.ext_cm_UISetup:
                    break;
                case ext_ConnectMode.ext_cm_Startup:
                    break;
                case ext_ConnectMode.ext_cm_AfterStartup:
                    DisplayToolWindow();
                    if (_applicationObject.Solution.IsOpen)
                    {
                        _buildEvents_OnBuildDone(vsBuildScope.vsBuildScopeSolution, vsBuildAction.vsBuildActionBuild);
                        _solutionEvents_Opened();
                    }
                    break;
            }
        }

        void RegisterForEvents()
        {
            _buildEvents = _applicationObject.DTE.Events.BuildEvents;
            _solutionEvents = _applicationObject.DTE.Events.SolutionEvents;

            _buildEvents.OnBuildDone += _buildEvents_OnBuildDone;
            _solutionEvents.Opened += _solutionEvents_Opened;

            _previewControl.SrcMLFileCreatedEvent += HandleSrcMLFileCreatedEvent;
            _previewControl.OpenFileEvent += HandleOpenFileEvent;
        }

        void UnRegisterForEvents()
        {
            _previewControl.SrcMLFileCreatedEvent -= HandleSrcMLFileCreatedEvent;
            _previewControl.OpenFileEvent -= HandleOpenFileEvent;

            _buildEvents.OnBuildDone -= _buildEvents_OnBuildDone;
            _solutionEvents.Opened -= _solutionEvents_Opened;

            _buildEvents = null;
            _solutionEvents = null;
        }

        void _buildEvents_OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
        {
            switch (Action)
            {
                case vsBuildAction.vsBuildActionClean:
                case vsBuildAction.vsBuildActionDeploy:
                    break;
                case vsBuildAction.vsBuildActionBuild:
                case vsBuildAction.vsBuildActionRebuildAll:
                    var outputDir = getOutputPath();
                    var dlls = getBuiltDlls(outputDir);
                    _previewControl.processBuiltDlls(dlls);
                    break;
            }
        }

        void _solutionEvents_Opened()
        {
            DirectoryInfo srcmlFolder;

            var firstProject = _applicationObject.Solution.Projects.Item(1);
            if (null != firstProject)
            {
                _srcmldir = firstProject.ProjectItems.Item("SrcML Files");
                srcmlFolder = new DirectoryInfo(_srcmldir.get_FileNames(0));
                _previewControl.LoadSrcMLFiles(srcmlFolder);
            }
        }

        void HandleSrcMLFileCreatedEvent(Object sender, SrcMLFileCreatedEventArgs e)
        {
            if (_srcmldir != null)
            {
                _srcmldir.ProjectItems.AddFromFile(e.Path);
            }
        }

        void HandleOpenFileEvent(Object sender, OpenFileEventArgs e)
        {
            if (File.Exists(e.Path))
            {
                Window window = _applicationObject.ItemOperations.OpenFile(e.Path);
                TextWindow textWindow = window.Document.Object("TextWindow") as TextWindow;
                textWindow.Selection.GotoLine(e.LineNumber);
            }
            else
            {
                MessageBox.Show(String.Format("{0} does not exist (but if it did, it might have line {1}!", e.Path, e.LineNumber));
            }
        }
        string getOutputPath()
        {
            var project = _applicationObject.Solution.Projects.Item(1);
            var config = project.ConfigurationManager.ActiveConfiguration;

            var props = config.Properties;
            var output = props.Item("OutputPath");

            var outputPath = Path.Combine(Path.GetDirectoryName(project.FullName), output.Value.ToString());

            return outputPath;
        }
        private List<string> getBuiltDlls(string outputPath)
        {
            List<string> results = new List<string>();
            DirectoryInfo folder = new DirectoryInfo(outputPath);
            FileInfo[] dlls = folder.GetFiles("*.dll");

            foreach (var dll in dlls)
                results.Add(dll.FullName);
            
            return results;
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
            DisplayToolWindow();
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
            UnRegisterForEvents();
            _toolWindow.Close(vsSaveChanges.vsSaveChangesNo);
        }

        private void DisplayToolWindow()
        {
            object objTemp = null;
            Assembly assembly = Assembly.GetCallingAssembly();
            
            try
            {
                _toolWindow = _toolWindows.CreateToolWindow2(this._addInInstance, assembly.Location,
                    "ABB.SrcML.VisualStudio.PreviewAddIn.TransformPreviewControl", "SrcML Preview Pane", TransformPreviewControl.GUID, ref objTemp);
                if (_toolWindow != null)
                {
                    _previewControl = objTemp as TransformPreviewControl;
                    _toolWindow.Visible = true;

                    RegisterForEvents();
                }
            }
            catch (Exception e)
            {
                String msg = assembly.Location + "\n";
                msg += e.Source + ": " + e.Message + "\n" + e.StackTrace;
                if (e.InnerException != null)
                    msg += "\n\n" + e.InnerException.Source + ": " + e.InnerException.Message + "\n" + e.InnerException.StackTrace;
                MessageBox.Show(msg);
            }
        }
        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
        {
            if(neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (commandName == "ABB.SrcML.VisualStudio.PreviewAddIn.Connect.SrcMLPreviewAddin")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported|vsCommandStatus.vsCommandStatusEnabled;
                    return;
                }
            }
        }

        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if(executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (commandName == "ABB.SrcML.VisualStudio.PreviewAddIn.Connect.SrcMLPreviewAddin")
                {
                    handled = true;
                    return;
                }
            }
        }
        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private ProjectItem _srcmldir;

        private Window _toolWindow;
        private Windows2 _toolWindows;
        private TransformPreviewControl _previewControl;
        
        private BuildEvents _buildEvents;
        private SolutionEvents _solutionEvents;
    }
}