using CaseCreator.DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace CaseCreator.Logic
{
    public class CaseHandler
    {
        internal StorageFolder Folder { get; }
        internal CaseManager.CaseLoader.Case CurrentCase { get; private set; }
        internal List<string> Files = null;
        internal List<CaseManager.CaseData.IData> DataFiles { get; private set; } = new List<CaseManager.CaseData.IData>();
        
        internal DateTime LastUpdatedTime = DateTime.MinValue;

        private int updateInterval = 500;

        public CaseHandler(StorageFolder location)
        {
            Folder = location;

            TestMethod();

            LoadCase();
        }

        private async Task TestMethod()
        {
            Logger.AddLog($"!!!!TestMethod!!!!", true);
            DataMaster sceneItem = new DataMaster();
            sceneItem.DataEntry.Add(new ItemData()
            {
                Label = "Data Type",
                SourceType = typeof(CaseManager.CaseData.SceneItem.SceneItemType).ToString(),
                TypeOfInput = ItemData.InputType.ComboBox,
            });
            sceneItem.DataEntry.Add(new ItemData()
            {
                Label = "Model",
                TypeOfInput = ItemData.InputType.TextBox
            });
            sceneItem.DataEntry.Add(new ItemData()
            {
                Label = "Scenario",
                SourceType = typeof(Common.Scenarios.ScenarioList).ToString(),
                TypeOfInput = ItemData.InputType.ComboBox,
                DefaultValue = Common.Scenarios.ScenarioList.NONE.ToString(),
                ShowOnLabelOutput = new string[]
                {
                    "Data Type", "Ped"
                }
            });
            sceneItem.DataEntry.Add(new ItemData()
            {
                Label = "Animation Dictionary",
                TypeOfInput = ItemData.InputType.TextBox,
                ShowOnLabelOutput = new string[]
                {
                    "Data Type", "Ped"
                }
            });
            sceneItem.DataEntry.Add(new ItemData()
            {
                Label = "Animation Name",
                TypeOfInput = ItemData.InputType.TextBox,
                ShowOnLabelOutput = new string[]
                {
                    "Data Type", "Ped"
                }
            });
            sceneItem.DataEntry.Add(new ItemData()
            {
                Label = "Blend In Speed",
                TypeOfInput = ItemData.InputType.Slider,
                DefaultValue = "3",
                MaxSliderValue = 10,
                ShowOnLabelOutput = new string[]
                {
                    "Data Type", "Ped"
                }
            });
            sceneItem.DataEntry.Add(new ItemData()
            {
                Label = "Animation Flags",
                SourceType = typeof(CaseManager.Resources.Animations.AnimationFlags).ToString(),
                TypeOfInput = ItemData.InputType.ComboBox,
                ShowOnLabelOutput = new string[]
                {
                    "Data Type", "Ped"
                }
            });
            sceneItem.DataEntry.Add(new ItemData()
            {
                Label = "Activate When Near",
                TypeOfInput = ItemData.InputType.CheckBox,
                DefaultValue = "False",
                ShowOnLabelOutput = new string[]
                {
                    "Data Type", "Ped"
                }
            });
            sceneItem.DataEntry.Add(new ItemData()
            {
                Label = "Is Siren On",
                TypeOfInput = ItemData.InputType.CheckBox,
                DefaultValue = "False",
                ShowOnLabelOutput = new string[]
                {
                    "Data Type", "Vehicle"
                }
            });
            sceneItem.DataEntry.Add(new ItemData()
            {
                Label = "Is Siren Silent",
                TypeOfInput = ItemData.InputType.CheckBox,
                DefaultValue = "True",
                ShowOnLabelOutput = new string[]
                {
                    "Data Type", "Vehicle"
                }
            });

            Logger.AddLog($"Created scene item...", true);
            Logger.AddLog($"{sceneItem.DataEntry.Count}", true);
            StorageFile file = await Folder.CreateFileAsync($"SceneItem.type");
            await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(sceneItem));
        }

        internal async Task Save()
        {
            Logger.AddLog($"Saving case from {Folder.Path}", false);
            if (CurrentCase == null) return;

            await SaveData(Folder);
            return;
        }

        internal bool HasBeenChangedSinceUpdate() => DateTime.Compare(LastUpdatedTime, DateTime.Now) < 0;

        internal async Task UpdateDataList()
        {
            if (DataFiles == null) DataFiles = new List<CaseManager.CaseData.IData>();

            await Task.Run(() =>
            {
                DataFiles.AddRange(CurrentCase.CSIData.Values);
                DataFiles.AddRange(CurrentCase.EntityData.Values);
                DataFiles.AddRange(CurrentCase.EvidenceData.Values);
                DataFiles.AddRange(CurrentCase.InterrogationData.Values);
                DataFiles.AddRange(CurrentCase.SceneData.Values);
                DataFiles.AddRange(CurrentCase.StageData.Values);
                DataFiles.AddRange(CurrentCase.WrittenData.Values);
            });
        }

        private async Task SaveData(StorageFolder folder)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var file in await folder.GetFilesAsync())
            {
                await file.DeleteAsync();
            }

            await Task.Run(async () =>
            {
                foreach (string loc in CurrentCase.Progress.Keys)
                {
                    await SerializeAndSave(CurrentCase.Progress[loc], CurrentCase.Progress[loc].ID, folder);
                }
                foreach (string loc in CurrentCase.CSIData.Keys)
                {
                    await SerializeAndSave(CurrentCase.CSIData[loc], CurrentCase.CSIData[loc].ID, folder);
                }
                foreach (string loc in CurrentCase.CurrentData.Keys)
                {
                    await SerializeAndSave(CurrentCase.CurrentData[loc], CurrentCase.CurrentData[loc].ID, folder);
                }
                foreach (string loc in CurrentCase.EntityData.Keys)
                {
                    await SerializeAndSave(CurrentCase.EntityData[loc], CurrentCase.EntityData[loc].ID, folder);
                }
                foreach (string loc in CurrentCase.EvidenceData.Keys)
                {
                    await SerializeAndSave(CurrentCase.EvidenceData[loc], CurrentCase.EvidenceData[loc].ID, folder);
                }
                foreach (string loc in CurrentCase.InterrogationData.Keys)
                {
                    await SerializeAndSave(CurrentCase.InterrogationData[loc], CurrentCase.InterrogationData[loc].ID, folder);
                }
                foreach (string loc in CurrentCase.SceneData.Keys)
                {
                    await SerializeAndSave(CurrentCase.SceneData[loc], CurrentCase.SceneData[loc].ID, folder);
                }
                foreach (string loc in CurrentCase.StageData.Keys)
                {
                    await SerializeAndSave(CurrentCase.StageData[loc], CurrentCase.StageData[loc].ID, folder);
                }
                foreach (string loc in CurrentCase.WrittenData.Keys)
                {
                    await SerializeAndSave(CurrentCase.WrittenData[loc], CurrentCase.WrittenData[loc].ID, folder);
                }
            });

            stopWatch.Stop();

            Logger.AddLog($"Total time to save: {stopWatch.ElapsedMilliseconds * 1000}s", false);

            return;
        }

        private async Task SerializeAndSave(object data, string id, StorageFolder root)
        {
            Logger.AddLog($"Saving {id} to {root.Path}", true);
            StorageFile file = await root.CreateFileAsync($"{id}.data");
            await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(data));
        }

        private async void LoadCase()
        {
            Logger.AddLog($"Loading case from {Folder.Path}", false);

            IReadOnlyList<StorageFile> files = await Folder.GetFilesAsync();
            Logger.AddLog($"Found total of {files.Count} data files in case folder", false);

            Dictionary<string, string> readFiles = new Dictionary<string, string>();
            foreach (StorageFile file in files)
            {
                Logger.AddLog($"Adding file {file.Name}", true);
                readFiles.Add(file.Name, await FileIO.ReadTextAsync(file));
            }

            foreach (string data in readFiles.Values)
            {
                Logger.AddLog($"File data: {data}", true);
            }

            CurrentCase = files.Count > 0 ? new CaseManager.CaseLoader.Case(readFiles) : new CaseManager.CaseLoader.Case();
            
            Logger.AddLog($"Case contains {CurrentCase.TotalCurrentFiles()} files", false);
            Logger.AddLog($"Loaded case successfully", false);

            Thread t = new Thread(ProcessAsync);
            t.Start();
        }

        private async void ProcessAsync()
        {
            int elapsed = 0;
            while (true)
            {
                if (elapsed >= updateInterval)
                {
                    elapsed = 0;
                    try
                    {
                        LastUpdatedTime = await LastModifiedDate();
                        OnModifiedTimeUpdated(LastUpdatedTime);
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"Error: {ex}", false);
                    }
                }

                elapsed++;

                Thread.Sleep(0);
            }
        }

        private async Task<DateTime> LastModifiedDate()
        {
            BasicProperties properties = await Folder.GetBasicPropertiesAsync();
            return properties.DateModified.DateTime;
        }

        public event EventHandler<DateTime> ModifiedUpdated;
        protected virtual void OnModifiedTimeUpdated(DateTime e) => ModifiedUpdated?.Invoke(this, e);

    }
}
