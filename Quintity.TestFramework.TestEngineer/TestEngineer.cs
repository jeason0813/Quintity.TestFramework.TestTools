/*
 * Copyright 2002 - 2007 Quintity, LLC.  All Rights Reserved.
 * Use is subject to license terms.
 * 
*/
using System;
using System.Runtime.Serialization;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using Quintity.TestFramework.Core;

namespace Quintity.TestFramework.TestEngineer
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public partial class TestEngineer : System.Windows.Forms.Form
    {
        #region Class event definition
        #endregion

        #region Class data members

        // Delegate for updating UI from worker thread.
        delegate void UpdateExecutionUICallBack(bool executing);

        // Command line strings.
        static private string m_testPropertiesFile;
        static private Uri m_testPropertiesUri;

        static private string m_testSuiteFile;
        static private Uri m_testSuiteUri;

        static private string m_testListenerFile;

        static private TestEngineer m_testEngineer;
        private System.Windows.Forms.ContextMenu m_contextStatusBarMenu;
        private System.Windows.Forms.MenuItem m_miEditConfig;
        private System.Windows.Forms.MenuItem m_miStatusSeparator;
        private System.Windows.Forms.MenuItem m_miRefreshConfig;
        private System.Windows.Forms.MainMenu m_mainMenu;
        private System.Windows.Forms.MenuItem m_miFile;
        private System.Windows.Forms.MenuItem m_miFileExit;
        private System.Windows.Forms.MenuItem m_miSeparator1;
        private System.Windows.Forms.ToolBar m_toolBar;
        private System.Windows.Forms.StatusBar m_statusBar;
        private System.Windows.Forms.Panel m_panelClient;
        private System.Windows.Forms.Panel m_panelOutput;
        private System.Windows.Forms.Panel m_panelProperties;
        private System.Windows.Forms.Panel m_panelView;
        private System.Windows.Forms.Splitter m_splitterHorz;
        private System.Windows.Forms.Splitter m_splitterVert;
        private Quintity.TestFramework.TestEngineer.TestResultViewer m_TestResultViewer;
        private System.Windows.Forms.MenuItem m_miFileNew;
        private System.Windows.Forms.MenuItem m_miFileOpen;
        private System.Windows.Forms.MenuItem m_miFileSave;
        private System.Windows.Forms.MenuItem m_miFileSaveAs;
        private System.Windows.Forms.MenuItem m_miEdit;
        private System.Windows.Forms.MenuItem m_miEditReset;
        private System.Windows.Forms.MenuItem m_miQueue;
        internal System.Windows.Forms.MenuItem m_miSuiteExecute;
        private System.Windows.Forms.MenuItem m_miSeparator3;
        private System.Windows.Forms.MenuItem m_miHelp;
        private System.Windows.Forms.MenuItem m_miHelpAbout;
        private System.Windows.Forms.TabControl m_tabCtrlProperties;
        private System.Windows.Forms.ImageList m_ilToolbar;
        private System.Windows.Forms.ToolBarButton m_tbFileNew;
        private System.Windows.Forms.ToolBarButton m_tbFileOpen;
        private System.Windows.Forms.ToolBarButton m_tbFileSave;
        private System.Windows.Forms.ToolBarButton m_tbSeparator1;
        private System.Windows.Forms.ToolBarButton m_tbEditReset;
        private System.Windows.Forms.ToolBarButton m_tbSeparator2;
        private System.Windows.Forms.OpenFileDialog m_openFileDialog;
        private System.Windows.Forms.SaveFileDialog m_saveFileDialog;
        private System.Windows.Forms.TabControl m_tabCtrlResults;
        private System.Windows.Forms.TabPage m_tpTestResults;
        private System.Windows.Forms.TabPage m_tpOutput;
        private Quintity.TestFramework.TestEngineer.TestOutputViewer m_testOutputViewer;
        private System.Windows.Forms.ImageList m_ilTabCtrls;
        private System.Windows.Forms.TabPage m_tpProperties;
        private System.Windows.Forms.PropertyGrid m_propertyGrid;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.PrintDialog m_printDialog;
        private System.Drawing.Printing.PrintDocument m_printDocument;
        private System.Windows.Forms.PrintPreviewDialog m_printPreviewDialog;
        private System.Windows.Forms.Timer m_executionTimer;
        private MenuItem m_miFileReload;
        private MenuItem menuItem4;
        private Button m_level1Btn;
        private Button m_level3Btn;
        private Button m_level2Btn;
        private Button m_levelAllBtn;
        private Button m_testBtn;
        private System.Windows.Forms.StatusBarPanel m_sbConfig;
        private System.Windows.Forms.StatusBarPanel m_sbPass;
        private System.Windows.Forms.StatusBarPanel m_sbFail;
        private System.Windows.Forms.StatusBarPanel m_sbTime;
        private StatusBarPanel m_sbTotal;
        private StatusBarPanel m_sbProgress;
        private MenuItem m_miEditUndo;
        private MenuItem m_miEditRedo;
        private MenuItem menuItem2;
        internal ComboBox m_tagsSelectionComboBox;
        internal Label m_tagsSelectionComboBoxLabel;
        private MenuItem m_miOpenNewConfig;
        private MenuItem menuItem5;
        internal ToolBarButton m_tbQueueExec;
        private Label m_prioritySelectionComboBoxLabel;
        internal ComboBox m_prioritySelectionComboBox;
        internal NumericUpDown m_samplingRateSpinner;
        private Label label1;
        private TestTreeView m_treeView;
        private System.ComponentModel.IContainer components;
        #endregion

        private delegate void ExceptionDelegate(Exception x);
        private delegate void updateStatusBarCallBack(TestCaseResult result);

        [STAThread]
        static void Main(string[] args)
        {
            TestProperties.Initialize();

            if (args.Length != 0)
            {
                //Cycle through arguments.
                for (int index = 0; index < args.Length; index++)
                {
                    string arg = args[index].ToUpper().Trim();

                    arg = arg.Length > 2 ? arg.Substring(0, 2) : null;

                    switch (arg)
                    {
                        case "/s":
                        case "/S":
                            m_testSuiteFile = extractUriFromArg(args[index]);
                            break;

                        case "/c":
                        case "/C":
                            m_testPropertiesFile = extractUriFromArg(args[index]);
                            break;

                        case "/l":
                        case "/L":
                            m_testListenerFile = extractUriFromArg(args[index]);
                            break;
                        default:
                            break;
                    }
                }
            }

            m_testEngineer = new TestEngineer();
            Application.Run(m_testEngineer);
        }

        static private string extractUriFromArg(string arg)
        {
            string file = null;

            string[] parts = arg.Split('=');

            if (parts.Length == 2)
            {
                file = parts[1].Trim();
            }

            return file;
        }

        #region Unhandled exception code - valid?

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception;

            exception = e.ExceptionObject as Exception;

            if (exception == null)
            {
                // this is an unmanaged exception, you may want to handle it differently
                return;
            }

            publishOnMainThread(exception);
        }

        private static void publishOnMainThread(Exception exception)
        {
            if (m_testEngineer.InvokeRequired)
            {
                // Invoke executes a delegate on the thread that owns _MainForms's underlying window handle.    
                m_testEngineer.Invoke(new ExceptionDelegate(handleException), new object[] { exception });
            }
            else
            {
                handleException(exception);
            }
        }

        private static void handleException(Exception exception)
        {
            if (SystemInformation.UserInteractive)
            {
                using (ThreadExceptionDialog dialog = new ThreadExceptionDialog(exception))
                {
                    if (dialog.ShowDialog() == DialogResult.Cancel)
                    {
                        return;
                    }
                }

                Application.Exit();
                Environment.Exit(0);
            }
        }

        #endregion

        #region Constructors

        public TestEngineer()
        {
            InitializeComponent();

#if DEBUG
            m_testBtn.Visible = true;
            m_testBtn.Enabled = true;
#endif
            registerRuntimeEvents();

            TestScriptObjectEditorDialog.OnTestScriptObjectEditorActivated += TestScriptObjectEditorDialog_OnTestScriptObjectEditorActivated;
            TestScriptObjectEditorDialog.OnTestScriptObjectEditorClosed += TestScriptObjectEditorDialog_OnTestScriptObjectEditorClosed;
            TestScriptObject.OnTestPropertyChanged += TestScriptObject_OnTestPropertyChanged;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestEngineer));
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Open Editor...");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Node1");
            this.m_contextStatusBarMenu = new System.Windows.Forms.ContextMenu();
            this.m_miOpenNewConfig = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.m_miEditConfig = new System.Windows.Forms.MenuItem();
            this.m_miStatusSeparator = new System.Windows.Forms.MenuItem();
            this.m_miRefreshConfig = new System.Windows.Forms.MenuItem();
            this.m_mainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.m_miFile = new System.Windows.Forms.MenuItem();
            this.m_miFileNew = new System.Windows.Forms.MenuItem();
            this.m_miFileOpen = new System.Windows.Forms.MenuItem();
            this.m_miSeparator1 = new System.Windows.Forms.MenuItem();
            this.m_miFileSave = new System.Windows.Forms.MenuItem();
            this.m_miFileSaveAs = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.m_miFileReload = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.m_miFileExit = new System.Windows.Forms.MenuItem();
            this.m_miEdit = new System.Windows.Forms.MenuItem();
            this.m_miEditUndo = new System.Windows.Forms.MenuItem();
            this.m_miEditRedo = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.m_miEditReset = new System.Windows.Forms.MenuItem();
            this.m_miQueue = new System.Windows.Forms.MenuItem();
            this.m_miSuiteExecute = new System.Windows.Forms.MenuItem();
            this.m_miSeparator3 = new System.Windows.Forms.MenuItem();
            this.m_miHelp = new System.Windows.Forms.MenuItem();
            this.m_miHelpAbout = new System.Windows.Forms.MenuItem();
            this.m_toolBar = new System.Windows.Forms.ToolBar();
            this.m_tbFileNew = new System.Windows.Forms.ToolBarButton();
            this.m_tbFileOpen = new System.Windows.Forms.ToolBarButton();
            this.m_tbFileSave = new System.Windows.Forms.ToolBarButton();
            this.m_tbSeparator1 = new System.Windows.Forms.ToolBarButton();
            this.m_tbEditReset = new System.Windows.Forms.ToolBarButton();
            this.m_tbSeparator2 = new System.Windows.Forms.ToolBarButton();
            this.m_tbQueueExec = new System.Windows.Forms.ToolBarButton();
            this.m_ilToolbar = new System.Windows.Forms.ImageList(this.components);
            this.m_statusBar = new System.Windows.Forms.StatusBar();
            this.m_sbConfig = new System.Windows.Forms.StatusBarPanel();
            this.m_sbProgress = new System.Windows.Forms.StatusBarPanel();
            this.m_sbTotal = new System.Windows.Forms.StatusBarPanel();
            this.m_sbPass = new System.Windows.Forms.StatusBarPanel();
            this.m_sbFail = new System.Windows.Forms.StatusBarPanel();
            this.m_sbTime = new System.Windows.Forms.StatusBarPanel();
            this.m_panelClient = new System.Windows.Forms.Panel();
            this.m_panelOutput = new System.Windows.Forms.Panel();
            this.m_tabCtrlResults = new System.Windows.Forms.TabControl();
            this.m_tpTestResults = new System.Windows.Forms.TabPage();
            this.m_tpOutput = new System.Windows.Forms.TabPage();
            this.m_ilTabCtrls = new System.Windows.Forms.ImageList(this.components);
            this.m_splitterHorz = new System.Windows.Forms.Splitter();
            this.m_panelView = new System.Windows.Forms.Panel();
            this.m_samplingRateSpinner = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.m_prioritySelectionComboBoxLabel = new System.Windows.Forms.Label();
            this.m_prioritySelectionComboBox = new System.Windows.Forms.ComboBox();
            this.m_tagsSelectionComboBoxLabel = new System.Windows.Forms.Label();
            this.m_tagsSelectionComboBox = new System.Windows.Forms.ComboBox();
            this.m_levelAllBtn = new System.Windows.Forms.Button();
            this.m_level3Btn = new System.Windows.Forms.Button();
            this.m_level2Btn = new System.Windows.Forms.Button();
            this.m_level1Btn = new System.Windows.Forms.Button();
            this.m_testBtn = new System.Windows.Forms.Button();
            this.m_panelProperties = new System.Windows.Forms.Panel();
            this.m_tabCtrlProperties = new System.Windows.Forms.TabControl();
            this.m_tpProperties = new System.Windows.Forms.TabPage();
            this.m_propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.m_splitterVert = new System.Windows.Forms.Splitter();
            this.m_openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.m_saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.m_printDialog = new System.Windows.Forms.PrintDialog();
            this.m_printDocument = new System.Drawing.Printing.PrintDocument();
            this.m_printPreviewDialog = new System.Windows.Forms.PrintPreviewDialog();
            this.m_executionTimer = new System.Windows.Forms.Timer(this.components);
            this.m_TestResultViewer = new Quintity.TestFramework.TestEngineer.TestResultViewer();
            this.m_testOutputViewer = new Quintity.TestFramework.TestEngineer.TestOutputViewer();
            this.m_treeView = new Quintity.TestFramework.Core.TestTreeView();
            ((System.ComponentModel.ISupportInitialize)(this.m_sbConfig)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_sbProgress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_sbTotal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_sbPass)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_sbFail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_sbTime)).BeginInit();
            this.m_panelClient.SuspendLayout();
            this.m_panelOutput.SuspendLayout();
            this.m_tabCtrlResults.SuspendLayout();
            this.m_tpTestResults.SuspendLayout();
            this.m_tpOutput.SuspendLayout();
            this.m_panelView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_samplingRateSpinner)).BeginInit();
            this.m_panelProperties.SuspendLayout();
            this.m_tabCtrlProperties.SuspendLayout();
            this.m_tpProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_contextStatusBarMenu
            // 
            this.m_contextStatusBarMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.m_miOpenNewConfig,
            this.menuItem5,
            this.m_miEditConfig,
            this.m_miStatusSeparator,
            this.m_miRefreshConfig});
            // 
            // m_miOpenNewConfig
            // 
            this.m_miOpenNewConfig.Index = 0;
            this.m_miOpenNewConfig.Text = "Open New...";
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 1;
            this.menuItem5.Text = "-";
            // 
            // m_miEditConfig
            // 
            this.m_miEditConfig.Index = 2;
            this.m_miEditConfig.Text = "Edit";
            // 
            // m_miStatusSeparator
            // 
            this.m_miStatusSeparator.Index = 3;
            this.m_miStatusSeparator.Text = "-";
            // 
            // m_miRefreshConfig
            // 
            this.m_miRefreshConfig.Index = 4;
            this.m_miRefreshConfig.Text = "Refresh";
            // 
            // m_mainMenu
            // 
            this.m_mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.m_miFile,
            this.m_miEdit,
            this.m_miQueue,
            this.m_miHelp});
            // 
            // m_miFile
            // 
            this.m_miFile.Index = 0;
            this.m_miFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.m_miFileNew,
            this.m_miFileOpen,
            this.m_miSeparator1,
            this.m_miFileSave,
            this.m_miFileSaveAs,
            this.menuItem1,
            this.m_miFileReload,
            this.menuItem4,
            this.m_miFileExit});
            this.m_miFile.Text = "&File";
            // 
            // m_miFileNew
            // 
            this.m_miFileNew.Index = 0;
            this.m_miFileNew.Text = "&New";
            this.m_miFileNew.Click += new System.EventHandler(this.m_miFileNew_Click);
            // 
            // m_miFileOpen
            // 
            this.m_miFileOpen.Index = 1;
            this.m_miFileOpen.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.m_miFileOpen.Text = "&Open...";
            this.m_miFileOpen.Click += new System.EventHandler(this.m_miFileOpen_Click);
            // 
            // m_miSeparator1
            // 
            this.m_miSeparator1.Index = 2;
            this.m_miSeparator1.Text = "-";
            // 
            // m_miFileSave
            // 
            this.m_miFileSave.Index = 3;
            this.m_miFileSave.Text = "&Save";
            this.m_miFileSave.Click += new System.EventHandler(this.m_miFileSave_Click);
            // 
            // m_miFileSaveAs
            // 
            this.m_miFileSaveAs.Index = 4;
            this.m_miFileSaveAs.Text = "Save &As...";
            this.m_miFileSaveAs.Click += new System.EventHandler(this.m_miFileSaveAs_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 5;
            this.menuItem1.Text = "-";
            // 
            // m_miFileReload
            // 
            this.m_miFileReload.Index = 6;
            this.m_miFileReload.Text = "Reload";
            this.m_miFileReload.Click += new System.EventHandler(this.m_miFileReload_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 7;
            this.menuItem4.Text = "-";
            // 
            // m_miFileExit
            // 
            this.m_miFileExit.Index = 8;
            this.m_miFileExit.Text = "E&xit";
            this.m_miFileExit.Click += new System.EventHandler(this.m_miFileExit_Click);
            // 
            // m_miEdit
            // 
            this.m_miEdit.Index = 1;
            this.m_miEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.m_miEditUndo,
            this.m_miEditRedo,
            this.menuItem2,
            this.m_miEditReset});
            this.m_miEdit.Text = "&Edit";
            // 
            // m_miEditUndo
            // 
            this.m_miEditUndo.Index = 0;
            this.m_miEditUndo.Shortcut = System.Windows.Forms.Shortcut.CtrlZ;
            this.m_miEditUndo.Text = "Undo";
            this.m_miEditUndo.Click += new System.EventHandler(this.m_miEditUndo_Click);
            this.m_miEditUndo.Popup += new System.EventHandler(this.m_miEditUndo_Popup);
            // 
            // m_miEditRedo
            // 
            this.m_miEditRedo.Index = 1;
            this.m_miEditRedo.Shortcut = System.Windows.Forms.Shortcut.CtrlY;
            this.m_miEditRedo.Text = "Redo";
            this.m_miEditRedo.Click += new System.EventHandler(this.m_miEditRedo_Click);
            this.m_miEditRedo.Popup += new System.EventHandler(this.m_miEditRedo_Popup);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 2;
            this.menuItem2.Text = "-";
            // 
            // m_miEditReset
            // 
            this.m_miEditReset.Index = 3;
            this.m_miEditReset.Text = "&Reset";
            this.m_miEditReset.Click += new System.EventHandler(this.m_miEditReset_Click);
            // 
            // m_miQueue
            // 
            this.m_miQueue.Index = 2;
            this.m_miQueue.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.m_miSuiteExecute,
            this.m_miSeparator3});
            this.m_miQueue.Text = "&Suite";
            // 
            // m_miSuiteExecute
            // 
            this.m_miSuiteExecute.Index = 0;
            this.m_miSuiteExecute.Shortcut = System.Windows.Forms.Shortcut.F5;
            this.m_miSuiteExecute.Text = "E&xecute";
            this.m_miSuiteExecute.Click += new System.EventHandler(this.m_miSuiteExecute_Click);
            // 
            // m_miSeparator3
            // 
            this.m_miSeparator3.Index = 1;
            this.m_miSeparator3.Text = "-";
            this.m_miSeparator3.Visible = false;
            // 
            // m_miHelp
            // 
            this.m_miHelp.Index = 3;
            this.m_miHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.m_miHelpAbout});
            this.m_miHelp.Text = "&Help";
            // 
            // m_miHelpAbout
            // 
            this.m_miHelpAbout.Index = 0;
            this.m_miHelpAbout.Text = "&About...";
            this.m_miHelpAbout.Click += new System.EventHandler(this.m_miHelpAbout_Click);
            // 
            // m_toolBar
            // 
            this.m_toolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.m_tbFileNew,
            this.m_tbFileOpen,
            this.m_tbFileSave,
            this.m_tbSeparator1,
            this.m_tbEditReset,
            this.m_tbSeparator2,
            this.m_tbQueueExec});
            this.m_toolBar.DropDownArrows = true;
            this.m_toolBar.ImageList = this.m_ilToolbar;
            this.m_toolBar.Location = new System.Drawing.Point(0, 0);
            this.m_toolBar.Name = "m_toolBar";
            this.m_toolBar.ShowToolTips = true;
            this.m_toolBar.Size = new System.Drawing.Size(1084, 28);
            this.m_toolBar.TabIndex = 0;
            this.m_toolBar.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
            this.m_toolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.m_toolBar_ButtonClick);
            // 
            // m_tbFileNew
            // 
            this.m_tbFileNew.ImageIndex = 0;
            this.m_tbFileNew.Name = "m_tbFileNew";
            this.m_tbFileNew.Tag = this.m_miFileNew;
            this.m_tbFileNew.Text = "New";
            this.m_tbFileNew.ToolTipText = "Opens new test queue.";
            // 
            // m_tbFileOpen
            // 
            this.m_tbFileOpen.ImageIndex = 1;
            this.m_tbFileOpen.Name = "m_tbFileOpen";
            this.m_tbFileOpen.Tag = this.m_miFileOpen;
            this.m_tbFileOpen.Text = "Open";
            this.m_tbFileOpen.ToolTipText = "Opens existing queue.";
            // 
            // m_tbFileSave
            // 
            this.m_tbFileSave.ImageIndex = 2;
            this.m_tbFileSave.Name = "m_tbFileSave";
            this.m_tbFileSave.Tag = this.m_miFileSave;
            this.m_tbFileSave.Text = "Save";
            this.m_tbFileSave.ToolTipText = "Save current test queue.";
            // 
            // m_tbSeparator1
            // 
            this.m_tbSeparator1.Name = "m_tbSeparator1";
            this.m_tbSeparator1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // m_tbEditReset
            // 
            this.m_tbEditReset.ImageIndex = 3;
            this.m_tbEditReset.Name = "m_tbEditReset";
            this.m_tbEditReset.Tag = this.m_miEditReset;
            this.m_tbEditReset.Text = "Reset";
            this.m_tbEditReset.ToolTipText = "Reset current test queue.";
            // 
            // m_tbSeparator2
            // 
            this.m_tbSeparator2.Name = "m_tbSeparator2";
            this.m_tbSeparator2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // m_tbQueueExec
            // 
            this.m_tbQueueExec.ImageIndex = 4;
            this.m_tbQueueExec.Name = "m_tbQueueExec";
            this.m_tbQueueExec.Tag = this.m_miSuiteExecute;
            this.m_tbQueueExec.Text = "Execute";
            this.m_tbQueueExec.ToolTipText = "Executes current queue.";
            // 
            // m_ilToolbar
            // 
            this.m_ilToolbar.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("m_ilToolbar.ImageStream")));
            this.m_ilToolbar.TransparentColor = System.Drawing.Color.Magenta;
            this.m_ilToolbar.Images.SetKeyName(0, "");
            this.m_ilToolbar.Images.SetKeyName(1, "");
            this.m_ilToolbar.Images.SetKeyName(2, "");
            this.m_ilToolbar.Images.SetKeyName(3, "");
            this.m_ilToolbar.Images.SetKeyName(4, "Execute.bmp");
            this.m_ilToolbar.Images.SetKeyName(5, "StopExecute.bmp");
            // 
            // m_statusBar
            // 
            this.m_statusBar.Location = new System.Drawing.Point(0, 720);
            this.m_statusBar.Name = "m_statusBar";
            this.m_statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.m_sbConfig,
            this.m_sbProgress,
            this.m_sbTotal,
            this.m_sbPass,
            this.m_sbFail,
            this.m_sbTime});
            this.m_statusBar.ShowPanels = true;
            this.m_statusBar.Size = new System.Drawing.Size(1084, 21);
            this.m_statusBar.TabIndex = 1;
            this.m_statusBar.Text = "statusBar1";
            // 
            // m_sbConfig
            // 
            this.m_sbConfig.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            this.m_sbConfig.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
            this.m_sbConfig.Name = "m_sbConfig";
            this.m_sbConfig.Width = 423;
            // 
            // m_sbProgress
            // 
            this.m_sbProgress.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            this.m_sbProgress.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
            this.m_sbProgress.Name = "m_sbProgress";
            this.m_sbProgress.Style = System.Windows.Forms.StatusBarPanelStyle.OwnerDraw;
            this.m_sbProgress.Text = "statusBarPanel1";
            this.m_sbProgress.Width = 423;
            // 
            // m_sbTotal
            // 
            this.m_sbTotal.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.m_sbTotal.Name = "m_sbTotal";
            this.m_sbTotal.Text = "0";
            this.m_sbTotal.ToolTipText = "Total cases to be run.";
            this.m_sbTotal.Width = 40;
            // 
            // m_sbPass
            // 
            this.m_sbPass.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.m_sbPass.Name = "m_sbPass";
            this.m_sbPass.Text = "0";
            this.m_sbPass.ToolTipText = "Passed test cases.";
            this.m_sbPass.Width = 40;
            // 
            // m_sbFail
            // 
            this.m_sbFail.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.m_sbFail.Name = "m_sbFail";
            this.m_sbFail.Text = "0";
            this.m_sbFail.ToolTipText = "Failed and errored test cases.";
            this.m_sbFail.Width = 40;
            // 
            // m_sbTime
            // 
            this.m_sbTime.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.m_sbTime.Name = "m_sbTime";
            this.m_sbTime.Text = "00:00:00";
            this.m_sbTime.ToolTipText = "Elapsed run time.";
            // 
            // m_panelClient
            // 
            this.m_panelClient.Controls.Add(this.m_panelOutput);
            this.m_panelClient.Controls.Add(this.m_splitterHorz);
            this.m_panelClient.Controls.Add(this.m_panelView);
            this.m_panelClient.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_panelClient.Location = new System.Drawing.Point(268, 28);
            this.m_panelClient.Name = "m_panelClient";
            this.m_panelClient.Size = new System.Drawing.Size(816, 692);
            this.m_panelClient.TabIndex = 3;
            // 
            // m_panelOutput
            // 
            this.m_panelOutput.BackColor = System.Drawing.Color.Blue;
            this.m_panelOutput.Controls.Add(this.m_tabCtrlResults);
            this.m_panelOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_panelOutput.Location = new System.Drawing.Point(0, 393);
            this.m_panelOutput.Name = "m_panelOutput";
            this.m_panelOutput.Size = new System.Drawing.Size(816, 299);
            this.m_panelOutput.TabIndex = 0;
            // 
            // m_tabCtrlResults
            // 
            this.m_tabCtrlResults.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.m_tabCtrlResults.Controls.Add(this.m_tpTestResults);
            this.m_tabCtrlResults.Controls.Add(this.m_tpOutput);
            this.m_tabCtrlResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_tabCtrlResults.ImageList = this.m_ilTabCtrls;
            this.m_tabCtrlResults.Location = new System.Drawing.Point(0, 0);
            this.m_tabCtrlResults.Multiline = true;
            this.m_tabCtrlResults.Name = "m_tabCtrlResults";
            this.m_tabCtrlResults.SelectedIndex = 0;
            this.m_tabCtrlResults.Size = new System.Drawing.Size(816, 299);
            this.m_tabCtrlResults.TabIndex = 1;
            // 
            // m_tpTestResults
            // 
            this.m_tpTestResults.Controls.Add(this.m_TestResultViewer);
            this.m_tpTestResults.ImageIndex = 1;
            this.m_tpTestResults.Location = new System.Drawing.Point(4, 4);
            this.m_tpTestResults.Name = "m_tpTestResults";
            this.m_tpTestResults.Size = new System.Drawing.Size(808, 272);
            this.m_tpTestResults.TabIndex = 0;
            this.m_tpTestResults.Text = "Test Results";
            // 
            // m_tpOutput
            // 
            this.m_tpOutput.Controls.Add(this.m_testOutputViewer);
            this.m_tpOutput.ImageIndex = 2;
            this.m_tpOutput.Location = new System.Drawing.Point(4, 4);
            this.m_tpOutput.Name = "m_tpOutput";
            this.m_tpOutput.Size = new System.Drawing.Size(808, 272);
            this.m_tpOutput.TabIndex = 1;
            this.m_tpOutput.Text = "Output";
            // 
            // m_ilTabCtrls
            // 
            this.m_ilTabCtrls.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("m_ilTabCtrls.ImageStream")));
            this.m_ilTabCtrls.TransparentColor = System.Drawing.Color.Magenta;
            this.m_ilTabCtrls.Images.SetKeyName(0, "");
            this.m_ilTabCtrls.Images.SetKeyName(1, "TestResultsTab.bmp");
            this.m_ilTabCtrls.Images.SetKeyName(2, "OutputTab.bmp");
            // 
            // m_splitterHorz
            // 
            this.m_splitterHorz.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_splitterHorz.Location = new System.Drawing.Point(0, 390);
            this.m_splitterHorz.Name = "m_splitterHorz";
            this.m_splitterHorz.Size = new System.Drawing.Size(816, 3);
            this.m_splitterHorz.TabIndex = 1;
            this.m_splitterHorz.TabStop = false;
            // 
            // m_panelView
            // 
            this.m_panelView.BackColor = System.Drawing.Color.Transparent;
            this.m_panelView.Controls.Add(this.m_treeView);
            this.m_panelView.Controls.Add(this.m_samplingRateSpinner);
            this.m_panelView.Controls.Add(this.label1);
            this.m_panelView.Controls.Add(this.m_prioritySelectionComboBoxLabel);
            this.m_panelView.Controls.Add(this.m_prioritySelectionComboBox);
            this.m_panelView.Controls.Add(this.m_tagsSelectionComboBoxLabel);
            this.m_panelView.Controls.Add(this.m_tagsSelectionComboBox);
            this.m_panelView.Controls.Add(this.m_levelAllBtn);
            this.m_panelView.Controls.Add(this.m_level3Btn);
            this.m_panelView.Controls.Add(this.m_level2Btn);
            this.m_panelView.Controls.Add(this.m_level1Btn);
            this.m_panelView.Controls.Add(this.m_testBtn);
            this.m_panelView.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_panelView.Location = new System.Drawing.Point(0, 0);
            this.m_panelView.Name = "m_panelView";
            this.m_panelView.Size = new System.Drawing.Size(816, 390);
            this.m_panelView.TabIndex = 2;
            // 
            // m_samplingRateSpinner
            // 
            this.m_samplingRateSpinner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_samplingRateSpinner.Location = new System.Drawing.Point(754, 2);
            this.m_samplingRateSpinner.Name = "m_samplingRateSpinner";
            this.m_samplingRateSpinner.Size = new System.Drawing.Size(50, 20);
            this.m_samplingRateSpinner.TabIndex = 10;
            this.m_samplingRateSpinner.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(671, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Sampling Rate:";
            // 
            // m_prioritySelectionComboBoxLabel
            // 
            this.m_prioritySelectionComboBoxLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_prioritySelectionComboBoxLabel.AutoSize = true;
            this.m_prioritySelectionComboBoxLabel.Location = new System.Drawing.Point(253, 6);
            this.m_prioritySelectionComboBoxLabel.Name = "m_prioritySelectionComboBoxLabel";
            this.m_prioritySelectionComboBoxLabel.Size = new System.Drawing.Size(83, 13);
            this.m_prioritySelectionComboBoxLabel.TabIndex = 8;
            this.m_prioritySelectionComboBoxLabel.Text = "Priority Selector:";
            // 
            // m_prioritySelectionComboBox
            // 
            this.m_prioritySelectionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_prioritySelectionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_prioritySelectionComboBox.FormattingEnabled = true;
            this.m_prioritySelectionComboBox.Location = new System.Drawing.Point(342, 2);
            this.m_prioritySelectionComboBox.Name = "m_prioritySelectionComboBox";
            this.m_prioritySelectionComboBox.Size = new System.Drawing.Size(120, 21);
            this.m_prioritySelectionComboBox.TabIndex = 7;
            // 
            // m_tagsSelectionComboBoxLabel
            // 
            this.m_tagsSelectionComboBoxLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_tagsSelectionComboBoxLabel.AutoSize = true;
            this.m_tagsSelectionComboBoxLabel.Location = new System.Drawing.Point(468, 6);
            this.m_tagsSelectionComboBoxLabel.Name = "m_tagsSelectionComboBoxLabel";
            this.m_tagsSelectionComboBoxLabel.Size = new System.Drawing.Size(71, 13);
            this.m_tagsSelectionComboBoxLabel.TabIndex = 6;
            this.m_tagsSelectionComboBoxLabel.Text = "Tag Selector:";
            // 
            // m_tagsSelectionComboBox
            // 
            this.m_tagsSelectionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_tagsSelectionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_tagsSelectionComboBox.FormattingEnabled = true;
            this.m_tagsSelectionComboBox.Items.AddRange(new object[] {
            "----  No Selection  ----"});
            this.m_tagsSelectionComboBox.Location = new System.Drawing.Point(545, 2);
            this.m_tagsSelectionComboBox.Name = "m_tagsSelectionComboBox";
            this.m_tagsSelectionComboBox.Size = new System.Drawing.Size(120, 21);
            this.m_tagsSelectionComboBox.TabIndex = 5;
            // 
            // m_levelAllBtn
            // 
            this.m_levelAllBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_levelAllBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_levelAllBtn.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.m_levelAllBtn.Location = new System.Drawing.Point(66, 4);
            this.m_levelAllBtn.Name = "m_levelAllBtn";
            this.m_levelAllBtn.Size = new System.Drawing.Size(16, 18);
            this.m_levelAllBtn.TabIndex = 4;
            this.m_levelAllBtn.Text = "A";
            this.m_levelAllBtn.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.m_levelAllBtn.UseVisualStyleBackColor = true;
            this.m_levelAllBtn.Click += new System.EventHandler(this.m_levelAllBtn_Click);
            // 
            // m_level3Btn
            // 
            this.m_level3Btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_level3Btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_level3Btn.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.m_level3Btn.Location = new System.Drawing.Point(45, 4);
            this.m_level3Btn.Name = "m_level3Btn";
            this.m_level3Btn.Size = new System.Drawing.Size(16, 18);
            this.m_level3Btn.TabIndex = 3;
            this.m_level3Btn.Text = "3";
            this.m_level3Btn.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.m_level3Btn.UseVisualStyleBackColor = true;
            this.m_level3Btn.Click += new System.EventHandler(this.m_level3Btn_Click);
            // 
            // m_level2Btn
            // 
            this.m_level2Btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_level2Btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_level2Btn.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.m_level2Btn.Location = new System.Drawing.Point(24, 4);
            this.m_level2Btn.Name = "m_level2Btn";
            this.m_level2Btn.Size = new System.Drawing.Size(16, 18);
            this.m_level2Btn.TabIndex = 2;
            this.m_level2Btn.Text = "2";
            this.m_level2Btn.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.m_level2Btn.UseVisualStyleBackColor = true;
            this.m_level2Btn.Click += new System.EventHandler(this.m_level2Btn_Click);
            // 
            // m_level1Btn
            // 
            this.m_level1Btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_level1Btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_level1Btn.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.m_level1Btn.Location = new System.Drawing.Point(3, 4);
            this.m_level1Btn.Name = "m_level1Btn";
            this.m_level1Btn.Size = new System.Drawing.Size(16, 18);
            this.m_level1Btn.TabIndex = 1;
            this.m_level1Btn.Text = "1";
            this.m_level1Btn.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.m_level1Btn.UseVisualStyleBackColor = true;
            this.m_level1Btn.Click += new System.EventHandler(this.m_level1Btn_Click);
            // 
            // m_testBtn
            // 
            this.m_testBtn.Enabled = false;
            this.m_testBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_testBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_testBtn.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.m_testBtn.Location = new System.Drawing.Point(108, 4);
            this.m_testBtn.Name = "m_testBtn";
            this.m_testBtn.Size = new System.Drawing.Size(16, 18);
            this.m_testBtn.TabIndex = 4;
            this.m_testBtn.Text = "T";
            this.m_testBtn.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.m_testBtn.UseVisualStyleBackColor = true;
            this.m_testBtn.Visible = false;
            this.m_testBtn.Click += new System.EventHandler(this.m_testBtn_Click);
            // 
            // m_panelProperties
            // 
            this.m_panelProperties.BackColor = System.Drawing.Color.Lime;
            this.m_panelProperties.Controls.Add(this.m_tabCtrlProperties);
            this.m_panelProperties.Dock = System.Windows.Forms.DockStyle.Left;
            this.m_panelProperties.Location = new System.Drawing.Point(0, 28);
            this.m_panelProperties.Name = "m_panelProperties";
            this.m_panelProperties.Size = new System.Drawing.Size(265, 692);
            this.m_panelProperties.TabIndex = 4;
            // 
            // m_tabCtrlProperties
            // 
            this.m_tabCtrlProperties.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.m_tabCtrlProperties.Controls.Add(this.m_tpProperties);
            this.m_tabCtrlProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_tabCtrlProperties.ImageList = this.m_ilTabCtrls;
            this.m_tabCtrlProperties.Location = new System.Drawing.Point(0, 0);
            this.m_tabCtrlProperties.Name = "m_tabCtrlProperties";
            this.m_tabCtrlProperties.SelectedIndex = 0;
            this.m_tabCtrlProperties.Size = new System.Drawing.Size(265, 692);
            this.m_tabCtrlProperties.TabIndex = 0;
            // 
            // m_tpProperties
            // 
            this.m_tpProperties.Controls.Add(this.m_propertyGrid);
            this.m_tpProperties.ImageIndex = 0;
            this.m_tpProperties.Location = new System.Drawing.Point(4, 4);
            this.m_tpProperties.Name = "m_tpProperties";
            this.m_tpProperties.Size = new System.Drawing.Size(257, 665);
            this.m_tpProperties.TabIndex = 0;
            this.m_tpProperties.Text = "Properties";
            // 
            // m_propertyGrid
            // 
            this.m_propertyGrid.CommandsForeColor = System.Drawing.SystemColors.GrayText;
            this.m_propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_propertyGrid.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_propertyGrid.LineColor = System.Drawing.SystemColors.ScrollBar;
            this.m_propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.m_propertyGrid.Name = "m_propertyGrid";
            this.m_propertyGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.m_propertyGrid.Size = new System.Drawing.Size(257, 665);
            this.m_propertyGrid.TabIndex = 0;
            // 
            // m_splitterVert
            // 
            this.m_splitterVert.Location = new System.Drawing.Point(265, 28);
            this.m_splitterVert.Name = "m_splitterVert";
            this.m_splitterVert.Size = new System.Drawing.Size(3, 692);
            this.m_splitterVert.TabIndex = 5;
            this.m_splitterVert.TabStop = false;
            // 
            // m_printPreviewDialog
            // 
            this.m_printPreviewDialog.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.m_printPreviewDialog.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.m_printPreviewDialog.ClientSize = new System.Drawing.Size(400, 300);
            this.m_printPreviewDialog.Enabled = true;
            this.m_printPreviewDialog.Icon = ((System.Drawing.Icon)(resources.GetObject("m_printPreviewDialog.Icon")));
            this.m_printPreviewDialog.Name = "m_printPreviewDialog";
            this.m_printPreviewDialog.Visible = false;
            // 
            // m_executionTimer
            // 
            this.m_executionTimer.Interval = 1000;
            // 
            // m_TestResultViewer
            // 
            this.m_TestResultViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_TestResultViewer.Location = new System.Drawing.Point(0, 0);
            this.m_TestResultViewer.Name = "m_TestResultViewer";
            this.m_TestResultViewer.ReadOnly = true;
            this.m_TestResultViewer.Size = new System.Drawing.Size(808, 293);
            this.m_TestResultViewer.TabIndex = 0;
            this.m_TestResultViewer.Text = "";
            // 
            // m_testOutputViewer
            // 
            this.m_testOutputViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_testOutputViewer.Location = new System.Drawing.Point(0, 0);
            this.m_testOutputViewer.Name = "m_testOutputViewer";
            this.m_testOutputViewer.ReadOnly = true;
            this.m_testOutputViewer.Size = new System.Drawing.Size(808, 272);
            this.m_testOutputViewer.TabIndex = 0;
            this.m_testOutputViewer.Text = "";
            this.m_testOutputViewer.WordWrap = false;
            // 
            // m_treeView
            // 
            this.m_treeView.AllowDrop = true;
            this.m_treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_treeView.FullRowSelect = true;
            this.m_treeView.HideSelection = false;
            this.m_treeView.ImageKey = "folder.closed.bmp";
            this.m_treeView.Location = new System.Drawing.Point(0, 25);
            this.m_treeView.Name = "m_treeView";
            treeNode1.Name = "m_openEditorMenuItem";
            treeNode1.Text = "Open Editor...";
            treeNode2.Name = "Node1";
            treeNode2.Text = "Node1";
            this.m_treeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2});
            this.m_treeView.SelectedImageKey = "folder.closed.bmp";
            this.m_treeView.SelectedNode = null;
            this.m_treeView.ShowNodeToolTips = true;
            this.m_treeView.Size = new System.Drawing.Size(810, 362);
            this.m_treeView.TabIndex = 11;
            this.m_treeView.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.m_treeView_AfterLabelEdit);
            this.m_treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.m_treeView_AfterSelect);
            // 
            // TestEngineer
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1084, 741);
            this.Controls.Add(this.m_panelClient);
            this.Controls.Add(this.m_splitterVert);
            this.Controls.Add(this.m_panelProperties);
            this.Controls.Add(this.m_statusBar);
            this.Controls.Add(this.m_toolBar);
            this.ForeColor = System.Drawing.SystemColors.GrayText;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TestEngineer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Quintity TestEngineer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestEngineer_FormClosing);
            this.Load += new System.EventHandler(this.TestEngineer_Load);
            this.Shown += new System.EventHandler(this.TestEngineer_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.m_sbConfig)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_sbProgress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_sbTotal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_sbPass)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_sbFail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_sbTime)).EndInit();
            this.m_panelClient.ResumeLayout(false);
            this.m_panelOutput.ResumeLayout(false);
            this.m_tabCtrlResults.ResumeLayout(false);
            this.m_tpTestResults.ResumeLayout(false);
            this.m_tpOutput.ResumeLayout(false);
            this.m_panelView.ResumeLayout(false);
            this.m_panelView.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_samplingRateSpinner)).EndInit();
            this.m_panelProperties.ResumeLayout(false);
            this.m_tabCtrlProperties.ResumeLayout(false);
            this.m_tpProperties.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        #region  TestEngineer class helper functions.

        private void ShowSplash(bool bTimer)
        {
            Quintity.TestFramework.TestEngineer.SplashDialog splash = new SplashDialog(bTimer);
            splash.Owner = this;
            splash.StartPosition = FormStartPosition.CenterScreen;
            splash.Show();
        }

        #endregion

        #region Treeview event handlers

        private void m_treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TestTreeNode node = TestTreeNode.Convert(e.Node);

            m_propertyGrid.SelectedObject = node.TestScriptObject;

            if (node.TestScriptResult != null)
            {
                this.m_TestResultViewer.Text = node.TestScriptResult.ToString();
            }
        }

        private void m_treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            m_propertyGrid.Refresh();
        }

        #endregion

        #region Main menu event handlers

        private void m_miFileNew_Click(object sender, EventArgs e)
        {
            bool @continue = true;

            if (m_treeView.HasChanged)
            {
                @continue = promptToSave(m_testSuiteUri != null ? m_testSuiteUri.LocalPath : null);
            }

            if (@continue)
            {
                m_treeView.NewRootTestSuite();
                m_treeView.ExpandAll();
                m_testSuiteUri = null;
                setCaption();
            }
        }

        private void m_miFileOpen_Click(object sender, EventArgs e)
        {
            openTestSuite();
        }

        private void m_miFileSave_Click(object sender, EventArgs e)
        {
            saveTestSuite(m_testSuiteUri != null ? m_testSuiteUri.LocalPath : null);
        }

        private void m_miFileSaveAs_Click(object sender, EventArgs e)
        {
            saveTestSuite();
        }

        private void m_miFileReload_Click(object sender, EventArgs e)
        {
            reloadTestSuite();
        }

        private void m_miFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void m_miEditUndo_Click(object sender, EventArgs e)
        {
            m_treeView.Undo();
        }

        private void m_miEditRedo_Click(object sender, EventArgs e)
        {
            m_treeView.Redo();
        }

        private void m_miEditReset_Click(object sender, EventArgs e)
        {
            m_treeView.ResetResults();
        }

        private void m_miSuiteOptions_Click(object sender, EventArgs e)
        {

        }

        private void m_miHelpAbout_Click(object sender, EventArgs e)
        {

        }

        private void m_miSuiteExecute_Click(object sender, EventArgs e)
        {
            m_treeView.Execute();
            //m_treeView.ResetResults();
            //m_testOutputViewer.Clear();
            //m_tabCtrlResults.SelectedTab = this.m_tpOutput;

            //TestSuite testSuite = m_treeView.GetTestSuite();
            //TestExecutor executor = new TestExecutor();
            //executor.ExecuteTestSuite(testSuite, null as TestFilter);
        }

        #endregion

        #region Other event handlers

        private void TestEngineer_Load(object sender, EventArgs args)
        {
            this.Menu = m_mainMenu;
        }

        private void m_toolBar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            ToolBarButton tbb = e.Button;
            MenuItem mi = (MenuItem)tbb.Tag;
            mi.PerformClick();
        }

        private void m_level1Btn_Click(object sender, EventArgs e)
        {
            if (this.m_treeView.IsTestSuiteLoaded())
            {
                // Turn off painting
                this.m_treeView.BeginUpdate();

                // Collapse all of tree.
                this.m_treeView.CollapseAll();

                // Select queue node.
                this.m_treeView.SelectedNode = this.m_treeView.RootNode;

                // Turn painting back on
                this.m_treeView.EndUpdate();
            }
        }

        private void m_level2Btn_Click(object sender, EventArgs e)
        {
            if (this.m_treeView.IsTestSuiteLoaded())
            {
                // Turn off painting
                this.m_treeView.BeginUpdate();

                // Get the currently selected node.
                TreeNode selected = this.m_treeView.SelectedNode;

                // Collapse all of tree
                this.m_treeView.CollapseAll();

                // Expand all groups.
                this.m_treeView.RootNode.Expand();

                // Select current node if visible (or nearest visible parent)
                //this.SelectWorkNode(1, selected);

                // Turn painting back on.
                this.m_treeView.EndUpdate();
            }
        }

        private void m_level3Btn_Click(object sender, EventArgs e)
        {
            if (this.m_treeView.IsTestSuiteLoaded())
            {
                // Turn off painting.
                this.m_treeView.BeginUpdate();

                // Get the currently selected node.
                TreeNode selected = this.m_treeView.SelectedNode;

                // Collapse all of tree
                this.m_treeView.CollapseAll();

                // Expand groups so expanded suites will display.
                this.m_treeView.Nodes[0].Expand();

                // Get all suites, iterate through expanding each.
                TreeNodeCollection nodes = this.m_treeView.RootNode.Nodes;

                foreach (TreeNode node in nodes)
                {
                    node.Expand();
                }

                // Select current node if visible (or nearest visible parent)
                //this.SelectWorkNode(2, selected);

                // Turn painting back on.
                this.m_treeView.EndUpdate();
            }
        }

        private void m_levelAllBtn_Click(object sender, EventArgs e)
        {
            if (this.m_treeView.IsTestSuiteLoaded())
            {
                // Turn off painting.
                this.m_treeView.BeginUpdate();

                // Get the currently selected node.
                TreeNode selected = this.m_treeView.SelectedNode;

                // Expand all nodes.
                this.m_treeView.ExpandAll();

                // Select current node if visible (or nearest visible parent)
                //this.SelectWorkNode(3, selected);

                // Turn painting on.
                this.m_treeView.EndUpdate();
            }
        }

        private void m_testBtn_Click(object sender, EventArgs e)
        {
            this.m_treeView.TraverseNodes(m_treeView.RootNode, traverseTestTreeView);
        }

        private bool traverseTestTreeView(TestTreeNode node, object tag)
        {
            return true;
        }

        void TestScriptObjectEditorDialog_OnTestScriptObjectEditorClosed(object sender,
            TestScriptObjectEditorDialog.TestScriptObjectEditorClosedArgs e)
        {
            m_treeView.FindNode(e.TestScriptObject).TestScriptEditorDialog = null;
        }

        void TestScriptObjectEditorDialog_OnTestScriptObjectEditorActivated(object sender,
            TestScriptObjectEditorDialog.TestScriptObjectEditorActivatedArgs e)
        {
            m_treeView.SelectedNode = m_treeView.FindNode(e.TestScriptObject);
        }

        void TestScriptObject_OnTestPropertyChanged(TestScriptObject testScriptObject, TestPropertyChangedEventArgs args)
        {
            m_propertyGrid.Refresh();
        }

        #endregion

        #region Private methods

        private void openTestSuite()
        {
            bool @continue = true;

            if (m_treeView.HasChanged)
            {
                @continue = promptToSave(m_testSuiteUri != null ? m_testSuiteUri.LocalPath : null);
            }

            if (@continue)
            {
                m_openFileDialog.Title = "Open Test Suite";
                m_openFileDialog.InitialDirectory = TestProperties.ExpandString(TestProperties.GetPropertyValueAsString("TestSuites"));
                m_openFileDialog.RestoreDirectory = true;
                m_openFileDialog.Filter = "Test suites (*.ste)|*.ste|All files (*.*)|*.*";
                m_openFileDialog.FilterIndex = 1;

                if (DialogResult.OK == m_openFileDialog.ShowDialog())
                {
                    loadTestSuite(m_openFileDialog.FileName);
                }
            }
        }

        private bool saveTestSuite()
        {
            return saveTestSuite(null);
        }

        private bool saveTestSuite(string testSuiteFile)
        {
            bool @continue = true;

            try
            {
                Uri testSuiteUri = null;
                DialogResult result;

                if (string.IsNullOrEmpty(testSuiteFile))
                {
                    m_saveFileDialog.Title = "Save Test Suite As";
                    m_saveFileDialog.InitialDirectory = TestProperties.ExpandString(TestProperties.GetPropertyValueAsString("TestSuites"));
                    m_saveFileDialog.RestoreDirectory = true;
                    m_saveFileDialog.Filter = "Test suites (*.ste)|*.ste|All files (*.*)|*.*";
                    m_saveFileDialog.FilterIndex = 1;

                    result = m_saveFileDialog.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        testSuiteUri = new Uri(m_saveFileDialog.FileName);
                    }
                    else
                    {
                        @continue = false;
                    }
                }
                else
                {
                    testSuiteUri = new Uri(testSuiteFile);
                }

                if (@continue)
                {
                    TestSuite testSuite = m_treeView.GetTestSuite();
                    TestSuite.SerializeToFile(testSuite, testSuiteUri.LocalPath);
                    m_testSuiteUri = testSuiteUri;
                    m_treeView.ResetHasChangedFlags();
                    setCaption();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Quintity TestEngineer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return @continue;
        }

        private void reloadTestSuite()
        {
            DialogResult dlgResult = MessageBox.Show(
                string.Format("The test suite \"{0}\" has unsaved changes which will be lost, do you wish to continue?",
                        m_treeView.RootNode.TestScriptObjectAsTestSuite().Title),
                "Quintity TestEngineer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dlgResult == DialogResult.Yes)
            {
                loadTestSuite(m_testSuiteUri.LocalPath);
            }
        }

        private bool promptToSave(string testSuiteFile)
        {
            bool @continue = true;

            DialogResult dlgResult = MessageBox.Show(
                string.Format("The test suite \"{0}\" has unsaved changes, do you wish to save them?",
                        m_treeView.RootNode.TestScriptObjectAsTestSuite().Title),
                "Quintity TestEngineer", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (dlgResult == DialogResult.Yes)
            {
                @continue = saveTestSuite(testSuiteFile);
            }
            else if (dlgResult == DialogResult.Cancel)
            {
                @continue = false;
            }

            return @continue;
        }

        private void setCaption()
        {
            if (m_testSuiteUri != null)
            {
                Text = string.Format("{0} - Quintity TestEngineer", m_testSuiteUri.Segments[m_testSuiteUri.Segments.Length - 1]);
            }
            else
            {
                Text = string.Format("{0} - Quintity Test Framework", m_treeView.RootNode.TestScriptObject.Title);
            }
        }

        private void displayTestScriptObjectEditorDialog()
        {
            TestTreeNode node = m_treeView.SelectedNode;

            if (null != node)
            {
                Point position = Cursor.Position;

                if (node.TestScriptEditorDialog != null)
                {
                    node.TestScriptEditorDialog.Location = position;
                    node.TestScriptEditorDialog.Activate();
                }
                else
                {
                    node.TestScriptEditorDialog = new TestScriptObjectEditorDialog(node.TestScriptObject);
                    node.TestScriptEditorDialog.Location = position;
                    node.TestScriptEditorDialog.Show();
                }
            }
        }

        private void TestEngineer_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool @continue = true;

            if (m_treeView.HasChanged)
            {
                @continue = promptToSave(m_testSuiteUri != null ? m_testSuiteUri.LocalPath : null);
            }

            if (!@continue)
            {
                e.Cancel = true;
            }
        }

        private void TestEngineer_Shown(object sender, EventArgs e)
        {
            initializeTestProperties();

            initializeTestSuite();
        }

        private void initializeTestSuite()
        {
            if (!string.IsNullOrEmpty(m_testSuiteFile))
            {
                if (null == loadTestSuite(TestProperties.ExpandString(m_testSuiteFile)))
                {
                    var node = m_treeView.NewRootTestSuite();
                    m_treeView.ExpandAll();
                }
            }
            else
            {
                var node = m_treeView.NewRootTestSuite();
                m_treeView.ExpandAll();
            }

            setCaption();
        }

        private TestSuite loadTestSuite(string testSuiteFile)
        {
            TestSuite testSuite = null;

            try
            {
                if (!string.IsNullOrEmpty(testSuiteFile))
                {
                    m_testSuiteUri = new Uri(testSuiteFile);
                    testSuite = TestSuite.DeserializeFromFile(m_testSuiteUri.LocalPath);
                    this.m_treeView.SetTestSuite(testSuite);
                    setCaption();
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(
                    string.Format("Unable to locate test suite file \"{0}\".\r\n\r\nPlease verify the file path.", testSuiteFile),
                    "Quintity TestEngineer", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                m_testSuiteUri = null;
            }
            catch (SerializationException)
            {
                MessageBox.Show(
                    string.Format("Unable to read test suite \"{0}\".\r\n\r\nPlease verify the file is a valid test suite file.", testSuiteFile),
                    "Quintity TestEngineer", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                m_testSuiteUri = null;
            }
            catch (UriFormatException)
            {
                MessageBox.Show(
                    string.Format("Unable to read test suite \"{0}\".\r\n\r\nPlease verify the file is a valid test suite file.", testSuiteFile),
                    "Quintity TestEngineer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Quintity TestEngineer", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                m_testSuiteUri = null;
            }

            return testSuite;
        }

        private void initializeTestProperties()
        {
            if (!string.IsNullOrEmpty(m_testPropertiesFile))
            {
                if (!loadTestProperties(m_testPropertiesFile))
                {
                    TestProperties.Initialize();
                }
            }
            else
            {
                TestProperties.Initialize();
            }
        }

        private bool loadTestProperties(string testPropertiesFile)
        {
            bool successful = false;

            try
            {
                m_testPropertiesUri = new Uri(testPropertiesFile);
                TestProperties.Initialize(new Uri(m_testPropertiesFile).LocalPath);
                successful = true;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(
                    string.Format("Unable to locate test properties file \"{0}\".\r\n\r\nPlease verify the file path.", testPropertiesFile),
                    "Quintity TestEngineer", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                m_testSuiteUri = null;
            }
            catch (SerializationException)
            {
                MessageBox.Show(
                    string.Format("Unable to read test properties \"{0}\".\r\n\r\nPlease verify the file is a valid test suite file.", testPropertiesFile),
                    "Quintity TestEngineer", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                m_testSuiteUri = null;
            }
            catch (UriFormatException)
            {
                MessageBox.Show(
                    string.Format("Unable to read test properties \"{0}\".\r\n\r\nPlease verify the file is a valid test suite file.", testPropertiesFile),
                    "Quintity TestEngineer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Quintity TestEngineer", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                m_testSuiteUri = null;
            }

            return successful;
        }

        #endregion

        private void m_miEditUndo_Popup(object sender, EventArgs e)
        {
            m_miEditUndo.Enabled = m_treeView.UndoAvailable();
        }

        private void m_miEditRedo_Popup(object sender, EventArgs e)
        {
            m_miEditRedo.Enabled = m_treeView.UndoAvailable();
        }
    }
}
