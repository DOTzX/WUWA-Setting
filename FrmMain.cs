using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WUWA_Setting {
    internal partial class FrmMain : Form {
        internal FrmMain () {
            InitializeComponent();
        }

        #region Variables
        private string WUWA_DIR;
        private SettingDataList settingDataList = new SettingDataList();
        private LocalStorageRepository localStorageRepository;
        private MenuStrip menuStrip;
        private ToolStripMenuItem runDx11MenuItem;
        private ToolStripMenuItem forceRtMenuItem;
        private ToolStripMenuItem exitAppOnLaunchMenuItem;
        private string local_wuwa_dir = "wuwa_dir.path";
        #endregion

        #region Core
        private void InitializeExtendedComponents () {
            this.Text = "WUWA Setting v" + Utilities.GetFileVersion();
            InitializeMenuStrip();

            if (File.Exists(local_wuwa_dir)) {
                string _WW = File.ReadAllText(local_wuwa_dir).Replace("\n", "").Trim();
                if (Directory.Exists(_WW)) {
                    string targetFile = "Wuthering Waves.exe";
                    bool fileExists = Directory.GetFiles(_WW, "*.exe")
                        .Any(file => Path.GetFileName(file).Equals(targetFile, StringComparison.OrdinalIgnoreCase));
                    if (fileExists) WUWA_DIR = _WW;
                }
            }

            if (string.IsNullOrEmpty(WUWA_DIR)) WUWA_DIR = Utilities.GetWuwaDir();
            RelocatedWuwaDir();
        }
        private void RelocatedWuwaDir () {
            txtDir.Text = WUWA_DIR;
            Console.WriteLine($"WUWA_DIR: {WUWA_DIR}");

            InitializeButtons((bool) Utilities.ReadLocalStorage(WUWA_DIR, ref settingDataList, ref localStorageRepository));
        }
        private void InitializeMenuStrip () {
            menuStrip = new MenuStrip();
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            menuStrip.Items.Add(fileMenu);

            ToolStripMenuItem optionMenu = new ToolStripMenuItem("Option");
            menuStrip.Items.Add(optionMenu);

            ToolStripMenuItem checkUpdateMenuItem = new ToolStripMenuItem("Check for Updates");
            checkUpdateMenuItem.Click += async (s, ev) => await Utilities.CheckForUpdate();
            fileMenu.DropDownItems.Add(checkUpdateMenuItem);

            ToolStripMenuItem openGitHubMenuItem = new ToolStripMenuItem("Visit GitHub Repo");
            openGitHubMenuItem.Click += (s, ev) => Utilities.OpenGitHubRepo();
            fileMenu.DropDownItems.Add(openGitHubMenuItem);

            ToolStripMenuItem exitAppMenuItem = new ToolStripMenuItem("Exit");
            exitAppMenuItem.Click += (s, ev) => Application.Exit();
            fileMenu.DropDownItems.Add(exitAppMenuItem);

            runDx11MenuItem = new ToolStripMenuItem("Launch with DirectX 11 (RT will be disabled)") {
                CheckOnClick = true
            };
            runDx11MenuItem.Checked = Properties.Settings.Default.RunDX11;
            runDx11MenuItem.CheckedChanged += RunDX11_CheckedChanged;
            optionMenu.DropDownItems.Add(runDx11MenuItem);

            forceRtMenuItem = new ToolStripMenuItem("Force use RT module without relaunch game (Supported GPU only)") {
                CheckOnClick = true
            };
            forceRtMenuItem.Checked = Properties.Settings.Default.ForceRT;
            forceRtMenuItem.CheckedChanged += ForceRT_CheckedChanged;
            optionMenu.DropDownItems.Add(forceRtMenuItem);

            exitAppOnLaunchMenuItem = new ToolStripMenuItem("Exit application after Launching Wuthering Waves") {
                CheckOnClick = true
            };
            exitAppOnLaunchMenuItem.Checked = Properties.Settings.Default.ExitAppOnLaunch;
            exitAppOnLaunchMenuItem.CheckedChanged += ExitAppOnLaunch_CheckedChanged;
            optionMenu.DropDownItems.Add(exitAppOnLaunchMenuItem);
        }
        private void InitializeButtons (bool addSettingButton = true) {
            int betweenSideGap = 10;
            int buttonWidth = 160;
            int buttonHeight = 40;
            lblDir.Top = menuStrip.Height + betweenSideGap;
            txtDir.Top = lblDir.Top + lblDir.Height + 5;
            lblDir.Left = betweenSideGap;
            txtDir.Left = betweenSideGap;
            int leftOffset = betweenSideGap;
            int topOffset = txtDir.Top + txtDir.Height + betweenSideGap;
            int buttonTop = topOffset;
            int highestTopOffset = topOffset;

            if (addSettingButton) {
                foreach (SettingStructure stgStc in settingDataList) {
                    stgStc.Button.Name = "DynamicButton_" + stgStc.Name;
                    stgStc.Button.Size = new Size(buttonWidth, buttonHeight);
                    stgStc.Button.Location = new Point(leftOffset, topOffset);
                    stgStc.Button.SettingStructure = stgStc;
                    ButtonState(stgStc);

                    if (stgStc.IsNewColumn) {
                        leftOffset += buttonWidth + betweenSideGap;
                        stgStc.Button.Left = leftOffset;

                        topOffset = buttonTop;
                        stgStc.Button.Top = topOffset;
                    }

                    switch (stgStc.DataTypeOption) {

                        case Type t when t == typeof(bool):
                            stgStc.Button.Click += ButtonBool_Click;
                            break;

                        default:
                            if (stgStc.DataTypeOption.GetType() == typeof(List<string>)) stgStc.Button.Click += ButtonList_Click;
                            break;

                    }

                    this.Controls.Add(stgStc.Button);
                    topOffset += buttonHeight + betweenSideGap;
                    if (highestTopOffset < topOffset) highestTopOffset = topOffset;
                }
            }

            topOffset = highestTopOffset + buttonHeight + betweenSideGap;
            leftOffset += buttonWidth + betweenSideGap;
            if (addSettingButton) {
                this.Width = leftOffset + ( betweenSideGap * 2 );
            } else {
                this.Width = 555;
            }

            btnApply.Top = highestTopOffset;
            btnApply.Left = betweenSideGap;
            btnApply.Width = this.Width - ( betweenSideGap * 4 );

            btnLaunch.Top = btnApply.Top + btnApply.Height + betweenSideGap;
            btnLaunch.Left = betweenSideGap;
            btnLaunch.Width = this.Width - ( betweenSideGap * 4 );

            forceRtMenuItem.Enabled = Utilities.IsGpuSupportedByOfficial();
            if (!forceRtMenuItem.Enabled) forceRtMenuItem.Checked = false;

            this.Height = btnLaunch.Top + btnLaunch.Height + ( betweenSideGap * 5 );

            btnOpen.Top = txtDir.Top;
            btnOpen.Left = this.Width - btnOpen.Width - ( betweenSideGap * 3 );

            btnChange.Top = txtDir.Top;
            btnChange.Left = this.Width - btnChange.Width - btnOpen.Width - ( betweenSideGap * 4 );

            txtDir.Width = this.Width - btnChange.Width - btnOpen.Width - ( betweenSideGap * 6 );

            btnApply.Enabled = addSettingButton;
            btnLaunch.Enabled = addSettingButton;
            btnOpen.Enabled = Directory.Exists(WUWA_DIR);
        }
        private void DeleteAddedButtons () {
            foreach (Control ctrl in this.Controls.OfType<Button>().Where(c => c.Name.StartsWith("DynamicButton_")).ToList()) {
                this.Controls.Remove(ctrl);
                ctrl.Dispose();
            }

            InitializeButtons(false);
        }
        private void ButtonState (SettingStructure stgStc) {
            string buttonText = stgStc.Value.ToString();

            switch (stgStc.DataTypeOption) {

                case Type t when t == typeof(bool):
                    try {
                        bool isEnabled = int.Parse(stgStc.Value.ToString()) > 0;
                        buttonText = isEnabled ? "On" : "Off";
                    } catch { }
                    break;

                default:
                    if (stgStc.DataTypeOption.GetType() == typeof(List<string>)) {
                        List<string> options = (List<string>) stgStc.DataTypeOption;

                        switch (stgStc.Name) {

                            case "CustomFrameRate":
                                int customValue = options.IndexOf(stgStc.Value.ToString());
                                if (customValue >= 0) buttonText = options[customValue];
                                else {
                                    try {
                                        int indexedValue = int.Parse(stgStc.Value.ToString());
                                        if (indexedValue < options.Count) buttonText = options[indexedValue];
                                    } catch { }
                                }
                                break;

                            case "NvidiaSuperSamplingQuality":
                                try {
                                    int indexedValue = int.Parse(stgStc.Value.ToString());
                                    int normalValue = options.IndexOf("Balanced") + indexedValue;
                                    if (indexedValue == 99) buttonText = options[options.IndexOf("Auto")];
                                    else if (normalValue >= 1 && normalValue < options.Count) buttonText = options[normalValue];
                                } catch { }
                                break;

                            default:
                                try {
                                    int indexedValue = int.Parse(stgStc.Value.ToString());
                                    if (indexedValue < options.Count) buttonText = options[indexedValue];
                                } catch { }
                                break;
                        }
                    }
                    break;

            }

            stgStc.Button.Text = $"{stgStc.Text}:\n{buttonText}";
            stgStc.Button.FlatStyle = buttonText == "Off" ? FlatStyle.Flat : FlatStyle.Standard;
        }
        #endregion

        #region Form Events
        private void Form1_Load (object sender, EventArgs e) {
            InitializeExtendedComponents();
        }
        private void ButtonList_Click (object sender, EventArgs e) {
            if (sender is SettingButton btn) {
                SettingStructure stgStc = btn.SettingStructure;
                List<string> options = (List<string>) stgStc.DataTypeOption;

                switch (stgStc.Name) {

                    case "CustomFrameRate":
                        int currentIndex = 0;
                        int customValue = options.IndexOf(stgStc.Value.ToString());
                        if (customValue >= 0) currentIndex = customValue;
                        else {
                            try {
                                int indexedValue = int.Parse(stgStc.Value.ToString());
                                if (indexedValue < options.Count) currentIndex = indexedValue;
                            } catch { }
                        }
                        stgStc.Value = ( currentIndex + 1 ) % options.Count;
                        break;

                    case "NvidiaSuperSamplingQuality":
                        int current2Index = 0;
                        try {
                            int indexedValue = int.Parse(stgStc.Value.ToString());
                            int normalValue = options.IndexOf("Balanced") + indexedValue;
                            if (indexedValue == 99) current2Index = options.IndexOf("Auto");
                            else if (normalValue >= 1 && normalValue < options.Count) current2Index = normalValue;
                        } catch { }
                        current2Index += 1;
                        if (current2Index >= options.Count) {
                            stgStc.Value = 99;
                        } else {
                            stgStc.Value = current2Index - options.IndexOf("Balanced");
                        }
                        break;

                    default:
                        int current3Index = 0;
                        try {
                            int indexedValue = int.Parse(stgStc.Value.ToString());
                            if (indexedValue < options.Count) current3Index = indexedValue;
                        } catch { }
                        stgStc.Value = ( current3Index + 1 ) % options.Count;
                        break;
                }

                Console.WriteLine($"ButtonList_Click: {stgStc.Name} | {stgStc.OldValue} => {stgStc.Value}");
                ButtonState(stgStc);
            }
        }
        private void ButtonBool_Click (object sender, EventArgs e) {
            if (sender is SettingButton btn) {
                object oldValue = btn.SettingStructure.Value;
                btn.SettingStructure.Value = (int) btn.SettingStructure.Value == 0 ? 1 : 0;
                Console.WriteLine($"ButtonBool_Click: {btn.SettingStructure.Name} | {oldValue} => {btn.SettingStructure.Value}");
                ButtonState(btn.SettingStructure);
            }
        }
        private void btnOpen_Click (object sender, EventArgs e) {
            Process.Start("explorer.exe", WUWA_DIR);
        }
        private void btnApply_Click (object sender, EventArgs e) {
            if (Utilities.IsGameRunning()) {
                MessageBox.Show("Wuthering Waves are still running, please close it first.");
                return;
            }

            string wnePath = Path.Combine(WUWA_DIR, "Client", "Saved", "Config", "WindowsNoEditor");
            string engineIniPath = Path.Combine(wnePath, "Engine.ini");
            string userIniPath = Path.Combine(wnePath, "GameUserSettings.ini");

            string rtxJsonPath = Path.Combine(WUWA_DIR, "Client", "Config", "RTX.json");

            bool isGpuSupportedByOfficial = Utilities.IsGpuSupportedByOfficial();

            bool isChangeDetected = false;

            foreach (SettingStructure stgStc in settingDataList) {
                bool isChanged = stgStc.Value != stgStc.OldValue;

                switch (stgStc.Name) { // overwriting each game execution.
                    case "RayTracing":
                        bool isEnabled = ( (int) stgStc.Value ) > 0;

                        if (File.Exists(rtxJsonPath)) {
                            string jsonContent = File.ReadAllText(rtxJsonPath);
                            JObject jsonObject = JObject.Parse(jsonContent);
                            jsonObject["bRayTracingEnable"] = isEnabled ? 1 : 0;
                            File.WriteAllText(rtxJsonPath, jsonObject.ToString());
                        }

                        if (File.Exists(engineIniPath) && !isGpuSupportedByOfficial) {
                            IniFile iniFile = new IniFile(engineIniPath);
                            if (isEnabled) {
                                iniFile.Write("/Script/Engine.RendererRTXSettings", "r.RayTracing", "1");
                                iniFile.Write("/Script/Engine.RendererRTXSettings", "r.RayTracing.LimitDevice", "0");
                                iniFile.Write("/Script/Engine.RendererRTXSettings", "r.RayTracing.Enable", "1");
                                iniFile.Write("/Script/Engine.RendererRTXSettings", "r.RayTracing.EnableInGame", "1");
                                iniFile.Write("/Script/Engine.RendererRTXSettings", "r.RayTracing.EnableOnDemand", "1");
                                iniFile.Write("/Script/Engine.RendererRTXSettings", "r.RayTracing.EnableInEditor", "1");
                            } else {
                                iniFile.DeleteSection("/Script/Engine.RendererRTXSettings");
                            }
                        }

                        if (File.Exists(userIniPath)) {
                            IniFile iniFile = new IniFile(userIniPath);
                            iniFile.Write("ScalabilityGroups", "sg.RayTracingQuality", stgStc.Value.ToString());
                        }

                        break;
                }

                if (isChanged) {
                    isChangeDetected = true;
                    Console.WriteLine($"DB_CHANGE: {stgStc.Name} | {stgStc.OldValue} => {stgStc.Value}");

                    switch (stgStc.Name) { // overwriting when changed
                        case "CustomFrameRate":
                            string numericNonIndexValue = "120";
                            List<string> options = (List<string>) stgStc.DataTypeOption;
                            int customValue = options.IndexOf(stgStc.Value.ToString());
                            if (customValue >= 0) numericNonIndexValue = options[customValue];
                            else {
                                try {
                                    int indexedValue = int.Parse(stgStc.Value.ToString());
                                    if (indexedValue < options.Count) numericNonIndexValue = options[indexedValue];
                                } catch { }
                            }

                            if (File.Exists(userIniPath)) {
                                IniFile iniFile = new IniFile(userIniPath);
                                iniFile.Write("/Script/Engine.GameUserSettings", "FrameRateLimit", numericNonIndexValue);
                            }
                            break;

                        case "PcVsync":
                            bool isEnabled = ( (int) stgStc.Value ) > 0;
                            if (File.Exists(userIniPath)) {
                                IniFile iniFile = new IniFile(userIniPath);
                                iniFile.Write("/Script/Engine.GameUserSettings", "bUseVSync", isEnabled ? "True" : "False");
                            }
                            break;
                    }

                    localStorageRepository.InsertOrUpdate(new LocalStorageEntry() {
                        Key = stgStc.Name,
                        Value = stgStc.Value.ToString(),
                    });
                    stgStc.OldValue = stgStc.Value;
                }
            }

            if (isChangeDetected) Utilities.ApplyLocalStorage(WUWA_DIR);
        }
        private void btnLaunch_Click (object sender, EventArgs e) {
            if (Utilities.IsGameRunning()) {
                MessageBox.Show("Wuthering Waves are still running, please close it first.");
                return;
            }

            List<string> list = new List<string>() { "Client" };

            if (runDx11MenuItem.Checked) {
                list.Add("-dx11");
            } else {
                list.Add("-dx12");

                string rtxJsonPath = Path.Combine(WUWA_DIR, "Client", "Config", "RTX.json");

                if (forceRtMenuItem.Checked) {
                    list.Add("-RTX");

                    if (File.Exists(rtxJsonPath)) {
                        string jsonContent = File.ReadAllText(rtxJsonPath);
                        JObject jsonObject = JObject.Parse(jsonContent);
                        jsonObject["bRayTracingEnable"] = 1;
                        File.WriteAllText(rtxJsonPath, jsonObject.ToString());
                    }
                } else {
                    List<SettingStructure> _btn = settingDataList.Where(stc => stc.Name == "RayTracing").ToList();
                    if (_btn.Count > 0) {
                        try {
                            int _value = int.Parse(_btn[0].Value.ToString());
                            bool isEnabled = _value > 0;
                            if (isEnabled) list.Add("-RTX");
                        } catch { }
                    }
                }
            }

            try {
                ProcessStartInfo startInfo = new ProcessStartInfo() {
                    FileName = "Client-Win64-Shipping.exe",
                    Arguments = string.Join(" ", list),
                    WorkingDirectory = Path.Combine(WUWA_DIR, "Client", "Binaries", "Win64"),
                    Verb = "runas",
                    UseShellExecute = true, // Required for 'runas'
                };

                Process process = Process.Start(startInfo);
                Console.WriteLine("Wuthering Waves process started successfully.");

                if (exitAppOnLaunchMenuItem.Checked) Application.Exit();
            } catch (Exception ex) {
                Console.WriteLine($"Failed to start elevated process: {ex.Message}");
            }
        }
        private void RunDX11_CheckedChanged (object sender, EventArgs e) {
            if (runDx11MenuItem.Checked) forceRtMenuItem.Checked = false;
            Properties.Settings.Default.RunDX11 = ( (ToolStripMenuItem) sender ).Checked;
            Properties.Settings.Default.Save();
        }
        private void ForceRT_CheckedChanged (object sender, EventArgs e) {
            if (forceRtMenuItem.Checked) runDx11MenuItem.Checked = false;
            Properties.Settings.Default.ForceRT = ( (ToolStripMenuItem) sender ).Checked;
            Properties.Settings.Default.Save();
        }
        private void ExitAppOnLaunch_CheckedChanged (object sender, EventArgs e) {
            Properties.Settings.Default.ExitAppOnLaunch = ( (ToolStripMenuItem) sender ).Checked;
            Properties.Settings.Default.Save();
        }
        private void btnChange_Click (object sender, EventArgs e) {
            while (true) {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                    dialog.Description = "Select a directory with Wuthering Waves.exe";

                    if (dialog.ShowDialog() == DialogResult.OK) {
                        string selectedPath = dialog.SelectedPath;
                        string targetFile = "Wuthering Waves.exe";

                        bool fileExists = Directory.GetFiles(selectedPath, "*.exe")
                            .Any(file => Path.GetFileName(file).Equals(targetFile, StringComparison.OrdinalIgnoreCase));

                        if (fileExists) {
                            WUWA_DIR = selectedPath;
                            File.WriteAllText(local_wuwa_dir, selectedPath);
                            RelocatedWuwaDir();
                            break;
                        } else {
                            MessageBox.Show($"'{targetFile}' not found in {selectedPath}", "File Not Found");
                        }
                    } else {
                        break;
                    }
                }
            }
        }
        #endregion

    }
}
