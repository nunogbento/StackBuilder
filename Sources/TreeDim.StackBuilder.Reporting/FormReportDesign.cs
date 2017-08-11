﻿#region Using directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

using log4net;

using treeDiM.StackBuilder.Basics;
using treeDiM.StackBuilder.Reporting.Properties;
#endregion

namespace treeDiM.StackBuilder.Reporting
{
    public partial class FormReportDesign : Form
    {
        #region Constructor
        public FormReportDesign()
        {
            InitializeComponent();
        }
        #endregion

        #region Form override
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // caption
            Text = string.Format(Resources.ID_REPORTANALYSIS, _analysis.Name);
            // toolbar show dimension
            toolSBDimensions.Checked = Settings.Default.ShowDimensions;
            // tree initialize root
            _rnRoot = new ReportNode(Resources.ID_REPORT);
            try
            {
                // font size controls
                cbFontSizeDetail.InitializeContent();
                cbFontSizeDetail.FontSizeRatio = Settings.Default.FontSizeRatioDetail;
                cbFontSizeLarge.InitializeContent();
                cbFontSizeLarge.FontSizeRatio = Settings.Default.FontSizeRatioLarge;
                // image definition
                cbDefinitionDetail.InitializeContent();
                cbDefinitionDetail.SelectedIndex = Settings.Default.ImageDefinitionDetail;
                cbDefinitionLarge.InitializeContent();
                int iDefLarge = Settings.Default.ImageDefinitionLarge;
                cbDefinitionLarge.SelectedIndex = iDefLarge >= 4 ? iDefLarge : 4;
                // image layout
                cbHTMLSizeDetail.InitializeContent();
                cbHTMLSizeDetail.SelectedIndex = Settings.Default.ImageHTMLSizeDetail;
                cbHTMLSizeLarge.InitializeContent();
                int iHTMLSizeLarge = Settings.Default.ImageHTMLSizeLarge;
                cbHTMLSizeLarge.SelectedIndex = iHTMLSizeLarge >= 5 ? iHTMLSizeLarge : 5;
            }
            catch (Exception /*ex*/)
            {
            }
            // define event handling after initializing size/font combo boxes
            cbDefinitionDetail.SelectedIndexChanged += new EventHandler(OnUpdateReport);
            cbDefinitionLarge.SelectedIndexChanged += new EventHandler(OnUpdateReport);
            cbFontSizeDetail.SelectedIndexChanged += new EventHandler(OnUpdateReport);
            cbFontSizeLarge.SelectedIndexChanged += new EventHandler(OnUpdateReport);
            cbHTMLSizeDetail.SelectedIndexChanged += new EventHandler(OnUpdateReport);
            cbHTMLSizeLarge.SelectedIndexChanged += new EventHandler(OnUpdateReport);

            UpdateReport();
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // font size
            Settings.Default.FontSizeRatioDetail = cbFontSizeDetail.FontSizeRatio;
            Settings.Default.FontSizeRatioLarge = cbFontSizeLarge.FontSizeRatio;
            // image definition
            Settings.Default.ImageDefinitionDetail = cbDefinitionDetail.SelectedIndex;
            Settings.Default.ImageDefinitionLarge = cbDefinitionLarge.SelectedIndex;
            // image layout
            Settings.Default.ImageHTMLSizeDetail = cbHTMLSizeDetail.SelectedIndex;
            Settings.Default.ImageHTMLSizeLarge = cbHTMLSizeLarge.SelectedIndex;
            // toolbar show dimension
            Settings.Default.ShowDimensions = toolSBDimensions.Checked; 
        }
        #endregion

        #region Public properties
        public Analysis Analysis
        {
            get { return _analysis; }
            set { _analysis = value; }
        }
        #endregion

        #region Helpers
        private string OutputFilePath
        {
            get
            {
                try
                {
                    _webBrowser.Navigate(string.Empty);
                    if (File.Exists(_outputFilePath))
                        File.Delete(_outputFilePath);
                }
                catch (Exception /*ex*/)
                {
                }
                return _outputFilePath = Path.ChangeExtension(Path.GetTempFileName(), "html");
            } 
        }

        private void UpdateReport()
        {
            try
            {
                if (null == _rnRoot)
                    throw new Exception("null == _rnRoot?");
                // generate report
                string htmlFilePath = OutputFilePath;
                Reporter.SetImageSize(cbDefinitionDetail.ImageSize, cbDefinitionLarge.ImageSize);
                Reporter.SetImageHTMLSize(cbHTMLSizeDetail.ImageSize, cbHTMLSizeLarge.ImageSize);
                Reporter.SetFontSizeRatios(cbFontSizeDetail.FontSizeRatio, cbFontSizeLarge.FontSizeRatio);
                ReporterHtml reporter = new ReporterHtml(new ReportData(_analysis), ref _rnRoot, Reporter.TemplatePath, htmlFilePath);
                // display html
                _webBrowser.Navigate(htmlFilePath, string.Empty, null, string.Empty);
                // update tree view
                FillTree();
            }
            catch (FileNotFoundException ex)
            {
                _log.Error(ex.Message);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }
        private void FillTree()
        {
            _treeView.Nodes.Clear();
            _treeView.CheckBoxes = true;

            TreeNode treeNodeRoot = new TreeNode(_rnRoot.Name);
            treeNodeRoot.Checked = true;
            treeNodeRoot.Tag = _rnRoot;
            _treeView.Nodes.Add(treeNodeRoot);

            foreach (ReportNode rn in _rnRoot.Children)
                AddNode(rn, treeNodeRoot);
            treeNodeRoot.Expand();
        }
        private void AddNode(ReportNode rn, TreeNode tnParent)
        {
            TreeNode tn = new TreeNode(rn.Name);
            tn.Checked = rn.Activated;
            tn.Tag = rn;
            tnParent.Nodes.Add(tn);

            foreach (ReportNode rnChild in rn.Children)
                AddNode(rnChild, tn);
            tn.Expand();
        }
        #endregion

        #region Toolbar event handlers
        private void OnReportMSWord(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.InitialDirectory = Properties.Settings.Default.ReportInitialDirectory;
                dlg.FileName = Path.ChangeExtension(CleanString(Analysis.Name), "doc");
                dlg.Filter = Properties.Resources.ID_FILTER_MSWORD;
                dlg.DefaultExt = "doc";
                dlg.ValidateNames = true;

                if (DialogResult.OK == dlg.ShowDialog())
                {
                    // save directory
                    Properties.Settings.Default.ReportInitialDirectory = Path.GetDirectoryName(dlg.FileName);
                    // generate report
                    ReporterMSWord reporter = new ReporterMSWord(
                        new ReportData(_analysis)
                        , ref _rnRoot
                        , Reporter.TemplatePath
                        , dlg.FileName
                        , new Margins());
                }
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                _log.Error("MS Word not installed? : " + ex.Message);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }
        private void OnReportHTML(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.InitialDirectory = Properties.Settings.Default.ReportInitialDirectory;
                dlg.FileName = Path.ChangeExtension(CleanString(Analysis.Name), "html");
                dlg.Filter = Properties.Resources.ID_FILTER_HTML;
                dlg.DefaultExt = "html";
                dlg.ValidateNames = true;

                if (DialogResult.OK == dlg.ShowDialog())
                {
                    // save directory
                    Properties.Settings.Default.ReportInitialDirectory = Path.GetDirectoryName(dlg.FileName);
                    // generate report
                    ReporterHtml reporter = new ReporterHtml(
                        new ReportData(_analysis)
                        , ref _rnRoot
                        , Reporter.TemplatePath
                        , dlg.FileName);

                    Process.Start( new ProcessStartInfo(dlg.FileName) ); 
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }
        private void OnReportPDF(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog()
                {
                    InitialDirectory = Properties.Settings.Default.ReportInitialDirectory,
                    FileName = Path.ChangeExtension(CleanString(Analysis.Name), "pdf"),
                    Filter = Properties.Resources.ID_FILTER_PDF,
                    DefaultExt = "pdf",
                    ValidateNames = true
                };

                if (DialogResult.OK == dlg.ShowDialog())
                {
                    // save directory
                    Properties.Settings.Default.ReportInitialDirectory = Path.GetDirectoryName(dlg.FileName);
                    // generate report
                    ReporterPDF reporter = new ReporterPDF(
                        new ReportData(_analysis)
                        , ref _rnRoot
                        , Reporter.TemplatePath
                        , dlg.FileName);
                    Process.Start(new ProcessStartInfo(dlg.FileName));
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }
        private void OnReportPrint(object sender, EventArgs e)
        {
            WebBrowser wb = new WebBrowser();
            wb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(OnDocumentCompleted);
            wb.Navigate(_outputFilePath);
            wb.Print();
        }
        private void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                WebBrowser wb = (WebBrowser)sender;
                if (wb.ReadyState.Equals(WebBrowserReadyState.Complete))
                    wb.ShowPrintDialog();
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }
        private void OnNodeChecked(object sender, TreeViewEventArgs e)
        {
            ReportNode rn = e.Node.Tag as ReportNode;
            if (null != rn)
                rn.Activated = !rn.Activated;
            UpdateReport();
        }
        private void OnShowDimensions(object sender, EventArgs e)
        {
            Reporter.ShowDimensions = !Reporter.ShowDimensions;
            UpdateReport();
            toolSBDimensions.Checked = Reporter.ShowDimensions;
        }
        private void OnUpdateReport(object sender, EventArgs e)
        {
            UpdateReport();
        }
        #endregion

        #region Helpers
        private static string CleanString(string name)
        {
            string nameCopy = name;
            char[] specialChars = { ' ', '*', '.', ';', ':' };
            foreach (char c in specialChars)
                nameCopy = nameCopy.Replace(c, '_');
            return nameCopy;
        }
        #endregion

        #region Data members
        protected string _outputFilePath;
        protected Analysis _analysis;
        protected ReportNode _rnRoot;
        protected static ILog _log = LogManager.GetLogger(typeof(FormReportDesign));
        #endregion


    }
}
