using CaseCreator.Logic;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static CaseCreator.Logic.Logger;
using CaseManager.CaseData;
using System.Linq;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CaseCreator.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CaseViewer : Page
    {
        internal CaseHandler Case;
        private TreeViewNode selectedNodeBecasueMicrosoftSucksAndDoesntAddSupportForThis = null;

        public CaseViewer(IData data = null)
        {
            this.InitializeComponent();

            this.Case = MainScreen.Handler;

            commandBar.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right;
            commandBar.OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Collapsed;

            RefreshData();

            caseContent.ItemInvoked += CaseContent_ItemInvoked;
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            AddLog($"AppBarButton_Click.{sender}", true);

            if (enableEditing.IsChecked != true && sender != enableEditing)
            {
                CreateFlyout(sender as FrameworkElement, "You must enable editing in order to use these functions.");
                return;
            }

            if (sender == enableEditing) return;

            string id = GetNodeID();

            if (sender == addItem)
            {
                this.Frame.Navigate(typeof(ItemEditor));
            }
            else if (sender == deleteItem)
            {
                if (selectedNodeBecasueMicrosoftSucksAndDoesntAddSupportForThis == null)
                {
                    CreateFlyout(sender as FrameworkElement, "You must select an item first.");
                    return;
                }

                if (id == string.Empty) return;
                bool dialog = await CreateContentDialog("Permanently Delete?", "Are you sure you would like to delete this item?", "Cancel", "Yes");
                if (!dialog) return;
                DeleteItem(id);
            }
            else if (sender == copyItem)
            {
                this.Frame.Navigate(typeof(ItemEditor), id);
            }
            else if (sender == editItem)
            {
                if (selectedNodeBecasueMicrosoftSucksAndDoesntAddSupportForThis == null)
                {
                    CreateFlyout(sender as FrameworkElement, "You must select an item to edit.");
                    return;
                }
                if (id == string.Empty) return;
                this.Frame.Navigate(typeof(ItemEditor), id);
            }
            
            RefreshData();
        }

        private async System.Threading.Tasks.Task<bool> CreateContentDialog(string title, string content, string closeButtonText, string primaryButtonText)
        {
            ContentDialog nullHandler = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = closeButtonText,
                PrimaryButtonText = primaryButtonText
            };

            return await nullHandler.ShowAsync() == ContentDialogResult.Primary;
        }

        private static void CreateFlyout(FrameworkElement sender, string text)
        {
            Flyout flyout = new Flyout();
            flyout.Content = new TextBlock()
            {
                Text = text
            };
            flyout.AreOpenCloseAnimationsEnabled = true;
            flyout.LightDismissOverlayMode = LightDismissOverlayMode.On;
            flyout.ShowAt(sender);
        }

        private void CaseContent_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem.GetType() != typeof(TreeViewNode)) return;
            AddLog($"CaseContent_ItemInvoked.{args.InvokedItem}", true);
            selectedNodeBecasueMicrosoftSucksAndDoesntAddSupportForThis = (TreeViewNode) args.InvokedItem;
        }

        private void DeleteItem(string id)
        {
            AddLog($"DeleteItem", true);
            object o = Case.CurrentCase.GetObjectFromID(id);
            AddLog($"caseContent.o={o}", true);
            if (o == null)
            {
                return;
            }

            IData d = o as IData;

            AddLog($"DeleteItem:  d={d.ID}", false);
            if (d == null) return;
            AddLog($"DeleteItem: Removing Object = {d.ID}", false);
            Case.CurrentCase.RemoveObject(d);
            caseContent.RootNodes.Remove(selectedNodeBecasueMicrosoftSucksAndDoesntAddSupportForThis);
            caseContent.UpdateLayout();
        }

        private string GetNodeID()
        {
            if (selectedNodeBecasueMicrosoftSucksAndDoesntAddSupportForThis == null)
            {
                return string.Empty;
            }
           
            TreeViewNode node = selectedNodeBecasueMicrosoftSucksAndDoesntAddSupportForThis.Parent is TreeViewNode ? selectedNodeBecasueMicrosoftSucksAndDoesntAddSupportForThis.Parent : selectedNodeBecasueMicrosoftSucksAndDoesntAddSupportForThis;
            
            if (node == null || node.Content == null)
            {
                AddLog($"GetNodeID.node == null", true);
                return string.Empty;
            }
            if (!node.Content.ToString().Contains("|"))
            {
                AddLog($"GetNodeID.node.Content.ToString().Contains(\"|\") == false", true);
                return string.Empty;
            }

            string id = Case.CurrentCase.GetAllIDs().First(q => q == node.Content.ToString().Split('|')[1].Replace(" ", ""));
            AddLog($"GetNodeID.caseContent.id={id}", true);

            return string.IsNullOrWhiteSpace(id) ? string.Empty : id;
        }

        private void RefreshData()
        {
            if (Case == null || Case.CurrentCase == null) return;

            caseContent.RootNodes.Clear();

            foreach (CaseManager.CaseData.CSIData data in Case.CurrentCase.CSIData.Values)
            {
                TreeViewNode main = new TreeViewNode() { Content = $"CSIData \t| {data.ID}" };
                caseContent.RootNodes.Add(main);
            }
            foreach (CaseManager.CaseData.EntityData data in Case.CurrentCase.EntityData.Values)
            {
                TreeViewNode main = new TreeViewNode() { Content = $"EntityData \t| {data.ID}" };
                caseContent.RootNodes.Add(main);
                foreach (var item in data.Dialogue)
                {
                    TreeViewNode children = new TreeViewNode() { Content = $"EntityData \t| {item.Text}" };
                    main.Children.Add(children);
                }
            }
            foreach (CaseManager.CaseData.InterrogationData data in Case.CurrentCase.InterrogationData.Values)
            {
                TreeViewNode main = new TreeViewNode() { Content = $"IntData \t| {data.ID}" };
                caseContent.RootNodes.Add(main);
                foreach (var item in data.InterrogationLines)
                {
                    TreeViewNode children = new TreeViewNode() { Content = item.Question };
                    main.Children.Add(children);
                }
            }
            foreach (CaseManager.CaseData.SceneData data in Case.CurrentCase.SceneData.Values)
            {
                TreeViewNode main = new TreeViewNode() { Content = $"SceneData \t| {data.ID}" };
                caseContent.RootNodes.Add(main);
                foreach (var item in data.Items)
                {
                    TreeViewNode children = new TreeViewNode() { Content = $"{item.ItemType} \t| {item.Model}" };
                    main.Children.Add(children);
                }
            }
            foreach (CaseManager.CaseData.StageData data in Case.CurrentCase.StageData.Values)
            {
                TreeViewNode main = new TreeViewNode() { Content = $"StageData \t| {data.ID}" };
                caseContent.RootNodes.Add(main);
                foreach (var item in data.ID_CSIData)
                {
                    TreeViewNode children = new TreeViewNode() { Content = $"ID_CSIData \t| {item}" };
                    main.Children.Add(children);
                }
                foreach (var item in data.ID_EntityData)
                {
                    TreeViewNode children = new TreeViewNode() { Content = $"ID_EntityData \t| {item}" };
                    main.Children.Add(children);
                }
                foreach (var item in data.ID_InterrogationData)
                {
                    TreeViewNode children = new TreeViewNode() { Content = $"ID_IntData \t| {item}" };
                    main.Children.Add(children);
                }
                foreach (var item in data.ID_SceneData)
                {
                    TreeViewNode children = new TreeViewNode() { Content = $"ID_SceneData \t| {item}" };
                    main.Children.Add(children);
                }
                foreach (var item in data.ID_WrittenData)
                {
                    TreeViewNode children = new TreeViewNode() { Content = $"ID_WrittenData \t| {item}" };
                    main.Children.Add(children);
                }
            }
        }
    }


    // Data Types
    /*
     * CSI Data - magnifying glass
     * Entity Data - person
     * Interrogation Data - talk bubble
     * Scene Data - checklist icon
     * Stage Data - movie clip thing
     */
}
