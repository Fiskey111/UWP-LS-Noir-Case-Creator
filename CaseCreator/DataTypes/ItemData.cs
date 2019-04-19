using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseCreator.DataTypes
{
    public class ItemData
    {
        public ItemData() { }

        public string Label { get; set; }
        public string SourceType { get; set; } = string.Empty;
        public InputType TypeOfInput { get; set; }
        public string DefaultValue { get; set; } = string.Empty;
        public int MaxSliderValue { get; set; } = 0;
        public string[] ShowOnLabelOutput { get; set; }

        public enum InputType { TextBox, ComboBox, CheckBox, Slider }
    }
}
