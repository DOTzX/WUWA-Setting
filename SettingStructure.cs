using System.Windows.Forms;

namespace WUWA_Setting {
    internal class SettingStructure {
        internal SettingButton Button { get; set; } = new SettingButton();
        internal string Name { get; set; }
        internal string Text { get; set; }
        internal object Value { get; set; }
        internal object OldValue { get; set; }
        internal object DataTypeOption { get; set; }
        internal bool IsNewColumn { get; set; } = false;
    }

    internal class SettingButton : Button {
        internal SettingStructure SettingStructure { get; set; }
    }
}
