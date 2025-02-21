using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WUWA_Tweaker {
    public partial class FrmMain : Form {
        public FrmMain () {
            InitializeComponent();
        }

        #region Variables
        private string WUWA_DIR;

        private List<SettingStructure> buttonData = new List<SettingStructure>() {
            {
                new SettingStructure () {
                    Name = "CustomFrameRate",
                    Text = "Frame Rate",
                    Value = 3,
                    DataTypeOption = new List<string> { "30", "45", "60", "120" },
                }
            },
            {
                new SettingStructure () {
                    Name = "PcVsync",
                    Text = "V-Sync",
                    Value = 1,
                    DataTypeOption = typeof(bool),
                }
            },
            {
                new SettingStructure () {
                    Name = "AntiAliasing",
                    Text = "Anti Aliasing",
                    Value = 0,
                    DataTypeOption = typeof(bool),
                }
            },
            {
                new SettingStructure () {
                    Name = "RayTracing",
                    Text = "Ray Tracing",
                    Value = 0,
                    DataTypeOption = new List < string > { "Off", "Low", "Medium", "High" },
                    IsNewLine = true,
                }
            },
            {
                new SettingStructure () {
                    Name = "RayTracedGI",
                    Text = "RT Global Illumination",
                    Value = 0,
                    DataTypeOption = typeof(bool),
                }
            },
            {
                new SettingStructure () {
                    Name = "RayTracedReflection",
                    Text = "RT Reflections",
                    Value = 0,
                    DataTypeOption = typeof(bool),
                }
            },
            {
                new SettingStructure () {
                    Name = "RayTracedShadow",
                    Text = "RT Shadows",
                    Value = 0,
                    DataTypeOption = typeof(bool),
                }
            },
            {
                new SettingStructure () {
                    Name = "NvidiaSuperSamplingEnable",
                    Text = "NVIDIA DLSS",
                    Value = 1,
                    DataTypeOption = typeof(bool),
                    IsNewLine = true,
                }
            },
            {
                new SettingStructure () {
                    Name = "NvidiaSuperSamplingFrameGenerate",
                    Text = "DLSS Frame Generation",
                    Value = 1,
                    DataTypeOption = typeof(bool),
                }
            },
            {
                new SettingStructure () {
                    Name = "NvidiaSuperSamplingQuality",
                    Text = "DLSS Resolution",
                    Value = 99,
                    DataTypeOption = new List < string > { "Auto", "Ult. Perf", "Perf", "Balanced", "Quality", "Ult. Quality" },
                }
            },
            // auto = 99, balanced = 0, rest is negative/positive
            {
                new SettingStructure () {
                    Name = "NvidiaSuperSamplingSharpness",
                    Text = "DLSS Sharpening",
                    Value = 0,
                    DataTypeOption = typeof(float),
                    Button = new SettingButton () {
                        Enabled = false,
                    },
                }
            },
            {
                new SettingStructure () {
                    Name = "FsrEnable",
                    Text = "AMD FSR",
                    Value = 1,
                    DataTypeOption = typeof(bool),
                    IsNewLine = true,
                }
            },
            {
                new SettingStructure () {
                    Name = "XessEnable",
                    Text = "Intel XeSS",
                    Value = 1,
                    DataTypeOption = typeof(bool),
                }
            },
            {
                new SettingStructure () {
                    Name = "XessQuality",
                    Text = "XeSS Quality",
                    Value = 1,
                    DataTypeOption = typeof(bool),
                    Button = new SettingButton () {
                        Enabled = false,
                    },
                }
            },
            {
                new SettingStructure () {
                    Name = "IrxEnable",
                    Text = "Intel Iris Xe",
                    Value = 1,
                    DataTypeOption = typeof(bool),
                }
            },
            {
                new SettingStructure () {
                    Name = "MetalFxEnable",
                    Text = "Apple MetalFX",
                    Value = 1,
                    DataTypeOption = typeof(bool),
                }
            },
        };
        #endregion

        #region Core
        private void CheckRegistryValue () {
            string registryPath = @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache";
            string patternKey = @"(Client-Win64-Shipping\.exe\.FriendlyAppName|launcher\.exe\.FriendlyAppName)$";
            string expectedValue = "Wuthering Waves";
            List<string> detectedPath = new List<string>();

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath)) {
                if (key != null) {
                    foreach (string valueName in key.GetValueNames()) {
                        if (Regex.IsMatch(valueName, patternKey, RegexOptions.IgnoreCase)) {
                            string value = key.GetValue(valueName).ToString();
                            if (value.ToLower() == expectedValue.ToLower()) {
                                string path = valueName.Replace(".FriendlyAppName", "");
                                if (Directory.Exists(Path.GetDirectoryName(path))) {
                                    Console.WriteLine($"DETECTED: {path}");
                                    detectedPath.Add(path);
                                }
                            }
                        }
                    }
                }
            }

            foreach (string path in detectedPath) {
                if (path.ToLower().EndsWith("client-win64-shipping.exe")) {
                    WUWA_DIR = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), "..", "..", ".."));
                    txtDir.Text = WUWA_DIR;
                    Console.WriteLine($"WUWA_DIR: {WUWA_DIR}");
                    break;
                } else if (path.ToLower().EndsWith("launcher.exe")) {
                    WUWA_DIR = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), "Wuthering Waves Game"));
                    txtDir.Text = WUWA_DIR;
                    Console.WriteLine($"WUWA_DIR: {WUWA_DIR}");
                    break;
                }
            }
        }
        private bool? ReadSettings () {
            string dbPath = Path.Combine(WUWA_DIR, "Client", "Saved", "LocalStorage", "LocalStorage.db");

            try {
                if (File.Exists(dbPath)) {
                    string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".tmp");
                    string localPath = Path.Combine(folderPath, Path.GetFileName(dbPath));
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                    File.Copy(dbPath, localPath, true);

                    var storageRepo = new LocalStorageRepository(localPath);

                    List<LocalStorageEntry> allEntries = storageRepo.GetAll();
                    foreach (LocalStorageEntry ety in allEntries) {
                        List<SettingStructure> _btn = buttonData.Where(stc => stc.Name == ety.Key).ToList();
                        if (_btn.Count > 0) {
                            if (int.TryParse(ety.Value, out int valueInt)) {
                                Console.WriteLine($"DB_READ: {ety.Key} = {ety.Value} | int => {valueInt}");
                                _btn[0].Value = valueInt;
                            } else if (float.TryParse(ety.Value, out float valueFloat)) {
                                Console.WriteLine($"DB_READ: {ety.Key} = {ety.Value} | float => {valueFloat}");
                                _btn[0].Value = valueFloat;
                            } else {
                                Console.WriteLine($"DB_READ: {ety.Key} = {ety.Value}");
                                _btn[0].Value = ety.Value;
                            }
                        }
                    }
                }

                return File.Exists(dbPath);
            } catch (Exception ex) {
                Console.WriteLine($"Error reading settings: {ex.Message}");
            }

            return null;
        }
        private void InitializeButtons (bool addSettingButton = true) {
            int betweenSideGap = 10;
            int buttonWidth = 160;
            int buttonHeight = 40;
            lblDir.Top = betweenSideGap;
            txtDir.Top = lblDir.Top + lblDir.Height + 5;
            lblDir.Left = betweenSideGap;
            txtDir.Left = betweenSideGap;
            int leftOffset = betweenSideGap;
            int topOffset = txtDir.Top + txtDir.Height + betweenSideGap;
            int buttonTop = topOffset;
            int highestTopOffset = topOffset;

            if (addSettingButton) {
                foreach (SettingStructure stgStc in buttonData) {
                    stgStc.Button.Name = "DynamicButton_" + stgStc.Name;
                    stgStc.Button.Size = new Size(buttonWidth, buttonHeight);
                    stgStc.Button.Location = new Point(leftOffset, topOffset);
                    stgStc.Button.SettingStructure = stgStc;
                    ButtonState(stgStc);

                    if (stgStc.IsNewLine) {
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
            this.Height = topOffset + btnApply.Height + betweenSideGap + btnLaunch.Height;

            btnApply.Top = highestTopOffset;
            btnApply.Left = betweenSideGap;
            btnApply.Width = this.Width - ( betweenSideGap * 4 );

            btnLaunch.Top = btnApply.Top + btnApply.Height + betweenSideGap;
            btnLaunch.Left = betweenSideGap;
            btnLaunch.Width = this.Width - ( betweenSideGap * 4 );

            btnLocate.Top = txtDir.Top;
            btnLocate.Left = this.Width - btnLocate.Width - ( betweenSideGap * 3 );
            txtDir.Width = this.Width - btnLocate.Width - ( betweenSideGap * 5 );

            btnApply.Enabled = addSettingButton;
            btnLaunch.Enabled = addSettingButton;
        }
        private void DeleteAddedButtons () {
            foreach (Control ctrl in this.Controls.OfType<Button>().Where(c => c.Name.StartsWith("DynamicButton_")).ToList()) {
                this.Controls.Remove(ctrl);
                ctrl.Dispose();
            }

            InitializeButtons(false);
        }
        #endregion

        #region Form Events
        private void Form1_Load (object sender, EventArgs e) {
            CheckRegistryValue();
            bool? isSettingRead = ReadSettings();
            InitializeButtons((bool) isSettingRead);
        }
        private void ButtonList_Click (object sender, EventArgs e) {
            if (sender is SettingButton btn) {
                SettingStructure stgStc = btn.SettingStructure;
                List<string> options = (List<string>) stgStc.DataTypeOption;
                object oldValue = stgStc.Value;

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

                Console.WriteLine($"ButtonList_Click: {stgStc.Name} | {oldValue} => {stgStc.Value}");
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
        #endregion

        #region Utility / Helper
        private static string GetGPUName () {
            try {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController")) {
                    foreach (ManagementObject mo in searcher.Get()) {
                        string name = mo["Name"]?.ToString();
                        if (!string.IsNullOrEmpty(name)) {
                            return name;
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error detecting GPU: {ex.Message}");
            }
            return string.Empty;
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
    }

    public class SettingStructure {
        public SettingButton Button { get; set; } = new SettingButton();
        public string Name { get; set; }
        public string Text { get; set; }
        public object Value { get; set; }
        public object DataTypeOption { get; set; }
        public bool IsNewLine { get; set; } = false;
    }

    public class SettingButton : Button {
        public SettingStructure SettingStructure { get; set; }
    }

}
