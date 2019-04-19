using CaseCreator.DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using static CaseCreator.Logic.Logger;
using CaseManager.CaseLoader;
using CaseManager.CaseData;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CaseCreator.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ItemEditor : Page
    {
        private Dictionary<string, DataMaster> _dataMasterDict;
        private Case _currentCase;
        private IData _selectedData;
        private List<Control> _controlList;

        public ItemEditor(Case currentCase, IData selectedData = null)
        {
            this.InitializeComponent();

            _dataMasterDict = new Dictionary<string, DataMaster>();
            _controlList = new List<Control>();

            _currentCase = currentCase;
            _selectedData = selectedData;

            LoadDataTypes();
        }

        private async void LoadDataTypes()
        {
            AddLog($"LoadDataTypes", true);
            StorageFolder folder = await Package.Current.InstalledLocation.GetFolderAsync("DataTypes");
            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
            dataBox.ItemsSource = files.Select(x => x.DisplayName);
            dataBox.SelectionChanged += Item_SelectionChanged;
            AddLog($"Total Data Types found: {files.Count}", true);
            foreach (var dataType in files)
            {
                AddLog($"Adding data type: {dataType.DisplayName}", true);
                string fileText = await FileIO.ReadTextAsync(dataType);
                DataMaster data = JsonConvert.DeserializeObject<DataMaster>(fileText);
                _dataMasterDict.Add(dataType.DisplayName, data);
            }

            if (_selectedData != null)
            {
                UpdateData(_selectedData);
            }

            AddLog($"Total data types added: {_dataMasterDict.Count}", false);
        }

        private void UpdateData(IData selectedData)
        {
            switch (selectedData.DataType)
            {
                case "CSIData":
                    var csiData = selectedData as CSIData;
                    dataBox.SelectedItem = _dataMasterDict.Keys.First(p => p ==  "CSIData");
                    idBox.Text = csiData.ID;
                    string selectedValue = dataBox.Items[dataBox.SelectedIndex].ToString();
                    AddDataTypes(selectedValue);

                    break;
                case "EntityData":
                    var entityData = selectedData as EntityData;
                    dataBox.SelectedItem = _dataMasterDict.Keys.First(p => p == "EntityData");
                    idBox.Text = entityData.ID;

                    break;
                case "InterrogationData":
                    var intData = selectedData as InterrogationData;
                    dataBox.SelectedItem = _dataMasterDict.Keys.First(p => p == "InterrogationData");
                    idBox.Text = intData.ID;

                    break;
                case "SceneData":
                    var sceneData = selectedData as SceneData;
                    dataBox.SelectedItem = _dataMasterDict.Keys.First(p => p == "SceneData");
                    idBox.Text = sceneData.ID;

                    break;
                case "StageData":
                    var stageData = selectedData as StageData;
                    dataBox.SelectedItem = _dataMasterDict.Keys.First(p => p == "StageData");
                    idBox.Text = stageData.ID;

                    break;
            }
        }

        private void AddItems()
        {

        }

        private void AddDataTypes(string selected)
        {
            if (!_dataMasterDict.ContainsKey(selected))
            {
                AddLog($"[AddDataTypes] Data type {selected} not found", false);
                return;
            }
            DataMaster master = _dataMasterDict[selected];
            foreach (var entry in master.DataEntry)
            {
                AddLog($"[AddDataTypes] Adding {entry.TypeOfInput}", true);

                StackPanel panel = stackPanel2;

                switch (entry.TypeOfInput)
                {
                    case ItemData.InputType.TextBox:
                        _controlList.Add(CreateTextBox(entry, panel));
                        break;
                    case ItemData.InputType.CheckBox:
                        _controlList.Add(CreateToggleSwitch(entry, panel));
                        break;
                    case ItemData.InputType.ComboBox:
                        _controlList.Add(CreateComboBox(entry, panel));
                        break;
                    case ItemData.InputType.Slider:
                        _controlList.Add(CreateSlider(entry, panel));
                        break;
                }
            }
        }

        private TextBox CreateTextBox(ItemData data, StackPanel panel)
        {
            TextBox item = new TextBox();
            item.AcceptsReturn = false;
            if (!string.IsNullOrWhiteSpace(data.Label)) item.Header = data.Label;
            if (!string.IsNullOrWhiteSpace(data.DefaultValue)) item.Text = data.DefaultValue;

            if (data.ShowOnLabelOutput != null && data.ShowOnLabelOutput.Length > 0)
            {
                _updateList.Add(new ControlCheck(item, data));
                if (IsRequired(data.ShowOnLabelOutput[0].ToLowerInvariant()))
                {
                    item.TextChanged += TextChanged;
                }
            }
            item.MinWidth = 200;
            item.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(item);
            return item;
        }
        private ToggleSwitch CreateToggleSwitch(ItemData data, StackPanel panel)
        {
            ToggleSwitch item = new ToggleSwitch();
            if (!string.IsNullOrWhiteSpace(data.Label)) item.Header = data.Label;
            if (!string.IsNullOrWhiteSpace(data.DefaultValue))
            {
                bool converted = Boolean.TryParse(data.DefaultValue, out bool value);
                if (converted) item.IsOn = value;
            }

            if (data.ShowOnLabelOutput != null && data.ShowOnLabelOutput.Length > 0)
            {
                _updateList.Add(new ControlCheck(item, data));
                if (IsRequired(data.ShowOnLabelOutput[0].ToLowerInvariant()))
                {
                    item.Toggled += SwitchToggled;
                }
            }

            item.MinWidth = 200;
            item.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(item);
            return item;
        }
        private ComboBox CreateComboBox(ItemData data, StackPanel panel)
        {
            ComboBox item = new ComboBox();
            if (!string.IsNullOrWhiteSpace(data.Label)) item.Header = data.Label;
            if (!string.IsNullOrWhiteSpace(data.SourceType))
            {
                AddLog($"CreateComboBox.source:{GetEnumType(data.SourceType)}", true);
                Type t = GetEnumType(data.SourceType);
                if (t != null) item.ItemsSource = Enum.GetValues(t).Cast<Enum>();
            }

            item.SelectedIndex = 0;

            if (data.ShowOnLabelOutput != null && data.ShowOnLabelOutput.Length > 0)
            {
                _updateList.Add(new ControlCheck(item, data));
                if (IsRequired(data.ShowOnLabelOutput[0].ToLowerInvariant()))
                {
                    item.SelectionChanged += Item_SelectionChanged;
                }
            }

            item.MinWidth = 200;
            item.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(item);
            return item;
        }
        private Slider CreateSlider(ItemData data, StackPanel panel)
        {
            Slider item = new Slider();
            if (!string.IsNullOrWhiteSpace(data.Label)) item.Header = data.Label;
            if (!string.IsNullOrWhiteSpace(data.DefaultValue))
            {
                bool converted = Int32.TryParse(data.DefaultValue, out int value);
                if (converted) item.Value = value;
            }
            item.Maximum = data.MaxSliderValue;

            if (data.ShowOnLabelOutput != null && data.ShowOnLabelOutput.Length > 0)
            {
                _updateList.Add(new ControlCheck(item, data));
                if (IsRequired(data.ShowOnLabelOutput[0].ToLowerInvariant()))
                {
                    item.ValueChanged += Item_ValueChanged;
                }
            }

            item.MinWidth = 300;
            item.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(item);
            return item;
        }

        private void Item_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender as Control != null) CheckControls(sender as Control);
        }

        private void Item_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender as Control != null) CheckControls(sender as Control);
        }

        private void SwitchToggled(object sender, RoutedEventArgs e)
        {
            if (sender as Control != null) CheckControls(sender as Control);
        }

        private List<ControlCheck> _updateList = new List<ControlCheck>();

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender as Control != null) CheckControls(sender as Control);
        }

        private void CheckControls(Control sender)
        {
            string header = string.Empty;
            string content = string.Empty;
            if (Equals(sender.GetType(), typeof(TextBox)))
            {
                var s = sender as TextBox;
                header = s.Header.ToString();
                content = s.Text;
            }
            else if (Equals(sender.GetType(), typeof(ToggleSwitch)))
            {
                var s = sender as ToggleSwitch;
                header = s.Header.ToString();
                content = s.IsOn.ToString();
            }
            else if (Equals(sender.GetType(), typeof(ComboBox)))
            {
                var s = sender as ComboBox;
                header = s.Header.ToString();
                content = s.Items[s.SelectedIndex].ToString();
            }
            else if (Equals(sender.GetType(), typeof(Slider)))
            {
                var s = sender as Slider;
                header = s.Header.ToString();
                content = s.Value.ToString();
            }

            AddLog($"CheckControls.header={header} | content={content}", true);
            if (string.IsNullOrEmpty(header) || string.IsNullOrEmpty(content)) return;

            foreach (var item in _updateList)
            {
                item.UIControl.Visibility = Visibility.Collapsed;
            }

            if (_updateList.Any(x => IsRequired(x.Header)))
            {
                foreach (ControlCheck c in _updateList.Where(ctrl => IsRequired(ctrl.Header)))
                {
                    c.UIControl.Visibility = Visibility.Visible;
                }
            }

            if (_updateList.Any(c => c.Header == header))
            {
                foreach (ControlCheck c in _updateList.Where(ctrl => ctrl.Header == header))
                {
                    if (c.CheckValue != content) continue;
                    c.UIControl.Visibility = Visibility.Visible;
                }
            }
                        
            foreach (var ctrl in _updateList)
            {
                if (ctrl.UIControl.Visibility == Visibility.Visible) continue;
                if (Equals(ctrl.UIControl.GetType(), typeof(TextBox)))
                {
                    var s = ctrl.UIControl as TextBox;
                    AddLog($"{s} | {ctrl.Data.DefaultValue}", true);
                    if (s == null) return;
                    string defaultValue = string.Empty;
                    if (!string.IsNullOrWhiteSpace(ctrl.Data.DefaultValue))
                    {
                        defaultValue = ctrl.Data.DefaultValue;
                    }
                    s.Text = defaultValue;
                }
                else if (Equals(ctrl.UIControl.GetType(), typeof(ToggleSwitch)))
                {
                    var s = ctrl.UIControl as ToggleSwitch;
                    AddLog($"{s} | {ctrl.Data.DefaultValue}", true);
                    if (s == null) return;

                    bool isOn = false;
                    if (!string.IsNullOrWhiteSpace(ctrl.Data.DefaultValue))
                    {
                        bool converted = Boolean.TryParse(ctrl.Data.DefaultValue, out bool value);
                        if (converted) isOn = value;
                    }
                    s.IsOn = isOn;
                }
                else if (Equals(ctrl.UIControl.GetType(), typeof(ComboBox)))
                {
                    var s = ctrl.UIControl as ComboBox;
                    AddLog($"{s} | {ctrl.Data.DefaultValue}", true);
                    if (s == null) return;
                    s.SelectedIndex = 0;
                }
                else if (Equals(ctrl.UIControl.GetType(), typeof(Slider)))
                {
                    var s = ctrl.UIControl as Slider;
                    AddLog($"{s} | {ctrl.Data.DefaultValue}", true);
                    if (s == null) return;

                    int defValue = 0;
                    if (!string.IsNullOrWhiteSpace(ctrl.Data.DefaultValue))
                    {
                        bool converted = Int32.TryParse(ctrl.Data.DefaultValue, out int value);
                        if (converted) defValue = value;
                    }
                    s.Value = defValue;
                }
            }
        }

        private bool IsRequired(string a) => string.Compare(a, "required", true) == 0;

        private void DataBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != dataBox) return;
            
            string selectedValue = dataBox.Items[dataBox.SelectedIndex].ToString();
            AddDataTypes(selectedValue);
        }

        private Type GetEnumType(string enumName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(enumName);
                if (type == null)
                    continue;
                if (type.IsEnum)
                    return type;
            }
            return null;
        }
    }

    internal class ControlCheck
    {
        internal Control UIControl { get; }
        internal string Header { get; }
        internal string CheckValue { get; }
        internal ItemData Data { get; }

        internal ControlCheck(Control control, ItemData data)
        {
            UIControl = control;
            Header = data.ShowOnLabelOutput[0];
            CheckValue = data.ShowOnLabelOutput[1];
            Data = data;
        }
    }
}
