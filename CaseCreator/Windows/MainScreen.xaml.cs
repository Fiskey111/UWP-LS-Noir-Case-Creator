using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using static CaseCreator.Logic.Logger;
using CaseCreator.Logic;
using Windows.System;
using Windows.UI.Core;
using System.Threading.Tasks;
using System.Threading;

namespace CaseCreator.Windows
{
    public sealed partial class MainScreen : Page
    {
        internal static CaseHandler Handler;

        private bool _isNewCase = false;
        private StorageFolder _folder;

        public MainScreen()
        {
            this.InitializeComponent();

            Logger.OnLogAdded += Logger_OnLogAdded;
        }

        private void Logger_OnLogAdded(object sender, EventArgs e)
        {
            if (navLogSaved == null) return;

            Thread t = new Thread(async () =>
            {
                await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal,
                    async () =>
                    {
                        navLogSaved.Content = "Log Saved";
                        navLogSaved.Icon.Visibility = Visibility.Visible;
                    });
                Thread.Sleep(350);
                await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal,
                    async () =>
                    {
                        navLogSaved.Content = "";
                        navLogSaved.Icon.Visibility = Visibility.Collapsed;
                    });
            });
            t.Start();
        }

        private void NavView_ItemInvoked(NavigationView sender,
                                         NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked == true)
            {
                AddLog($"NavView_ItemInvoked | settings", true);
                NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItemContainer != null)
            {
                AddLog($"NavView_ItemInvoked | {args.InvokedItemContainer.Tag.ToString()}", true);
                var navItemTag = args.InvokedItemContainer.Tag.ToString();
                if (navItemTag == "navViewLog" || navItemTag == "navHome")
                {
                    NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
                }
                else
                {
                    MenuPressed(navItemTag);
                }
            }
        }

        private void MenuPressed(string navItemTag)
        {
            AddLog($"MenuPressed({navItemTag})", false);
            switch (navItemTag)
            {
                case "navOpenCase":
                    OpenCaseAsync();
                    break;
                case "navViewSource":
                    OpenWebView();
                    break;
                case "navNewCase":
                    NewCaseAsync();
                    break;
                case "navSaveCase":
                    SaveCaseAsync();
                    break;
            }
        }

        private async void OpenWebView()
        {
            Uri uri = new Uri("https://github.com/Fiskey111/LSNCaseManager");
            ContentDialog confirmNavigate = new ContentDialog
            {
                Title = "Confirm Navigation",
                Content = $"Continue navigating to {uri}",
                CloseButtonText = "No",
                PrimaryButtonText = "Yes",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = await confirmNavigate.ShowAsync();

            AddLog($"OpenWebView.confirmNavigate | result={result}", true);

            if (result != ContentDialogResult.Primary)
            {
                // Cancelled
                return;
            }

            var success = await Launcher.LaunchUriAsync(uri);
            if (!success)
            {
                CreateFlyout(ContentFrame, $"Unable to navigate to {uri}");
            }
        }

        private async void OpenCaseAsync()
        {
            AddLog($"OpenCase()", true);

            bool cont = await CheckSave();
            if (!cont) return;

            Flyout f = CreateFlyout(navOpenCase, "Please select the ROOT folder for your case"
                + Environment.NewLine + "Typically, this is the name of the case"
                + Environment.NewLine + "Example: "
                + Environment.NewLine + "Grand Theft Auto V>Plugins>LSPDFR>LSNoir>Cases>My Case" + Environment.NewLine + "You will select \"My Case\", not any subfolders");

            _isNewCase = false;

            f.Closed += Flyout_Closed;
        }

        private async void NewCaseAsync()
        {
            AddLog($"NewCase()", true);

            bool cont = await CheckSave();
            if (!cont) return;

            Flyout f = CreateFlyout(navNewCase, "Please select the ROOT folder for your case"
                + Environment.NewLine + "Typically, this will be where your case is saved"
                + Environment.NewLine + "Example: "
                + Environment.NewLine + "Grand Theft Auto V>Plugins>LSPDFR>LSNoir>Cases>My Case" + Environment.NewLine + "You will select \"My Case\", not any ubfolders or files." 
                + Environment.NewLine + "**THIS MUST BE AN EMPTY FOLDER**");

            _isNewCase = true;

            f.Closed += Flyout_Closed;
        }

        private async Task<bool> CheckSave()
        {
            if (Handler == null) return true;

            if (Handler.HasBeenChangedSinceUpdate())
            {
                ContentDialog confirm = new ContentDialog
                {
                    Title = "Continue Without Saving?",
                    Content = $"You have not saved your changes.  Are you sure you would like to continue without saving them?",
                    CloseButtonText = "Cancel",
                    PrimaryButtonText = "Continue",
                    SecondaryButtonText = "Save Changes and Continue",
                    DefaultButton = ContentDialogButton.Close
                };

                ContentDialogResult result = await confirm.ShowAsync();

                AddLog($"CheckSave.confirm | result={result}", true);

                if (result == ContentDialogResult.Primary)
                {
                    return true;
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    SaveCaseAsync();

                    return true;
                }
            }
            return false;
        }

        private async void Flyout_Closed(object sender, object e)
        {
            FolderPicker openPicker = new FolderPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            openPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                _folder = folder;

                ContentDialog confirmFolder = new ContentDialog
                {
                    Title = "Confirm Selection",
                    Content = $"Selected folder: {_folder.Path}",
                    CloseButtonText = "Cancel",
                    PrimaryButtonText = "Accept",
                    DefaultButton = ContentDialogButton.Primary
                };

                ContentDialogResult result = await confirmFolder.ShowAsync();

                AddLog($"Flyout_Closed.confirmFolder | result={result}", true);

                if (result != ContentDialogResult.Primary)
                {
                    // Cancelled
                    return;
                }
            }

            if (_folder == null)
            {
                ContentDialog invalidFolder = new ContentDialog
                {
                    Title = "No Folder Selected",
                    Content = "Please select a folder to open",
                    CloseButtonText = "Ok"
                };

                ContentDialogResult result = await invalidFolder.ShowAsync();

                AddLog($"Flyout_Closed.invalidFolder | result={result}", true);
                return;
            }

            var files = await _folder.GetFilesAsync();

            if (_isNewCase && files.Count > 0)
            {
                ContentDialog invalidFolder = new ContentDialog
                {
                    Title = "Not an Empty Folder",
                    Content = "Please select an empty folder",
                    CloseButtonText = "Ok"
                };

                ContentDialogResult result = await invalidFolder.ShowAsync();

                AddLog($"Flyout_Closed.invalidFolder | result={result} | _isNewCase={_isNewCase}", true);
                return;
            }

            AddLog($"Flyout_Closed.new Logic.CaseHandler({_folder.Path})", true);
            Handler = new Logic.CaseHandler(_folder);
            navCurrentCaseName.Content = _folder.DisplayName;

            Handler.ModifiedUpdated += Handler_ModifiedUpdated;
        }

        private async void Handler_ModifiedUpdated(object sender, DateTime e)
        {
            if (sender == null || sender != Handler) return;

            await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    navLastSavedDate.Content = $"Last Saved: {e.ToShortDateString()} {e.ToLongTimeString()}";
                });
        }

        private Flyout CreateFlyout(UIElement elementToAttach, string text)
        {
            Flyout flyout = new Flyout();
            flyout.Content = new TextBlock()
            {
                Text = text
            };
            flyout.AreOpenCloseAnimationsEnabled = true;
            flyout.LightDismissOverlayMode = LightDismissOverlayMode.On;
            flyout.ShowAt(elementToAttach as FrameworkElement);
            return flyout;
        }
        
        private async void SaveCaseAsync()
        {
            AddLog($"SaveCase()", false);
            if (Handler == null || Handler.CurrentCase == null)
            {
                AddLog($"SaveCase.Handler == null", true);
                ContentDialog nullHandler = new ContentDialog
                {
                    Title = "No Case Found",
                    Content = "Please load/create a case before trying to save",
                    CloseButtonText = "Ok"
                };

                ContentDialogResult result = await nullHandler.ShowAsync();
                return;
            }
            
            Handler.Save();
        }

        private void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo)
        {
            AddLog($"NavView_Navigate | {navItemTag}", true);
            Type _page = null;
            if (navItemTag == "settings")
            {
                _page = typeof(SettingsPage);
            }
            else if (navItemTag == "navViewLog")
            {
                _page = typeof(LogScreen);
            }
            else if (navItemTag == "navHome")
            {
                _page = typeof(CaseViewer);
            }

            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            var preNavPageType = ContentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (!(_page is null) && !Type.Equals(preNavPageType, _page))
            {
                AddLog($"NavView_Navigate.Frame.Navigate({_page})", true);
                ContentFrame.Navigate(_page, null, transitionInfo);
            }
        }

        private void NavView_BackRequested(NavigationView sender,
                                           NavigationViewBackRequestedEventArgs args)
        {
            On_BackRequested();
        }

        private void NavView_Loaded(object sender, RoutedEventArgs args)
        {
            AddLog($"NavView_Loaded", true);
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            AddLog($"ContentFrame_NavigationFailed : {e.SourcePageType.FullName})", false);
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void BackInvoked(KeyboardAccelerator sender,
                                 KeyboardAcceleratorInvokedEventArgs args)
        {
            On_BackRequested();
            args.Handled = true;
        }

        private bool On_BackRequested()
        {
            AddLog($"On_BackRequested", true);
            if (!ContentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed.
            if (NavView.IsPaneOpen &&
                (NavView.DisplayMode == NavigationViewDisplayMode.Compact ||
                 NavView.DisplayMode == NavigationViewDisplayMode.Minimal))
                return false;

            AddLog($"On_BackRequested.ContentFrame.GoBack()", true);
            ContentFrame.GoBack();
            return true;
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;
        }
    }
}
