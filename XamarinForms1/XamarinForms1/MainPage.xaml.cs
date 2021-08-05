using Rg.Plugins.Popup.Services;
using SimplePopupForm.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BlokTabs
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            PageBackgroundColor = ((ContentPage)FindByName("MainContentPage")).BackgroundColor;
            InitializeTabFields();
            InitializePaths();

            FileName = "TabTestFile";
        }

        #region Varibles and objects:

        // Readonly values:
        private readonly Color PageBackgroundColor;

        // Tabs:
        private readonly Grid[,] TabFields = new Grid[6, 28];
        private readonly View[,] TabFieldsView = new View[6, 28];


        // Paths:
        private string _fileAbsolutePath = null;
        public string FileAbsolutePath
        {
            get { return _fileAbsolutePath; }
            set { _fileAbsolutePath = value; }
        }

        public string FileName
        {
            get { return FileNameEditor.Text; }
            set { FileNameEditor.Text = value; }
        }

        private string SavesDirePath;

        #endregion

        #region InitializeMethods

        private void InitializePaths()
        {
            //todo
            SavesDirePath = Path.Combine(App.ExternalStorageAbsolutePath, "BlokTabsSaves");
            if (!File.Exists(SavesDirePath))
                Directory.CreateDirectory(SavesDirePath);
        }

        private void InitializeTabFields()
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    TabFields[i, j] = (Grid)FindByName($"Tab_{i + 1}_{j + 1}");
                }
            }
        }

        #endregion

        #region ButtonClickedMehtods

        private void SaveButton_Clicked(object sender, EventArgs e)
        {
            if (File.Exists(Path.Combine(SavesDirePath, FileName)))
            {
                SaveTabsToFile(Path.Combine(SavesDirePath, FileName));
                SwitchToViewMode();
            }
            else
            {
                SaveAsButton_Clicked(sender, e);
            }
        }

        private async void SaveAsButton_Clicked(object sender, EventArgs e)
        {
            //var result = await DisplayPromptAsync("Write file name", "File name:", "OK", "Cancel", null, -1, null, FileName);
            PickDirectoryPopupTaskView popup;
            await PopupNavigation.PushAsync(popup = new PickDirectoryPopupTaskView(Path.Combine(SavesDirePath), PickDirectoryPopupTaskView.PickType.Directory));
            popup.SelectClicked += SaveAs_SetLocalisationPopup_SelectClicked;
        }

        private string tmpPath;
        private async void SaveAs_SetLocalisationPopup_SelectClicked(object sender, EventArgs e)
        {
            var pdp = (sender as PickDirectoryPopupTaskView);
            tmpPath = pdp.AbsolutePath;
            PopupEditTextTaskView popup;
            await PopupNavigation.PushAsync(popup = new PopupEditTextTaskView("Save as", "File name:", FileName, "Write file name..."));
            popup.OKClicked += SaveAs_SetNamePopup_OKClicked;
        }

        private void SaveAs_SetNamePopup_OKClicked(object sender, EventArgs e)
        {
            var popup = sender as PopupEditTextTaskView;

            if (popup.EntryText.Contains(".BlokTabSave"))
                FileName = popup.EntryText;
            else
                FileName = popup.EntryText + ".BlokTabSave";

            FileAbsolutePath = Path.Combine(tmpPath, FileName);

            SaveTabsToFile(FileAbsolutePath);
            SwitchToViewMode();
        }

        private void ReadFromFileButton_Clicked(object sender, EventArgs e)
        {
            SelectFileFromFilePicker_MethodAsync();
        }

        private void EditModeOnButton_Clicked(object sender, EventArgs e)
        {
            SwitchToEditMode();
        }

        private void ViewModeOnButton_Clicked(object sender, EventArgs e)
        {
            SwitchToViewMode();
        }

        #endregion

        #region SaveAndReadMethods

        private void SaveTabsToFile(string path)
        {
            if (!File.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (File.Exists(path))
                File.Delete(path);

            using (StreamWriter sw = File.CreateText(path))
            {
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 28; j++)
                    {
                        if (TabFieldsView[i, j] != null)
                        {
                            if (TabFieldsView[i, j] is Label)
                            {
                                sw.WriteLine((TabFieldsView[i, j] as Label).Text);
                            }
                            else if (TabFieldsView[i, j] is Entry)
                            {
                                sw.WriteLine((TabFieldsView[i, j] as Entry).Text);
                            }
                        }
                        else
                        {
                            sw.WriteLine("");
                        }
                    }
                }
                sw.Close();
            }
        }

        private void ReadTabsFromFile(string path)
        {
            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        for (int j = 0; j < 28; j++)
                        {
                            if (TabFieldsView[i, j] != null)
                            {
                                TabFields[i, j].Children.Remove(TabFieldsView[i, j]);
                                TabFieldsView[i, j] = null;
                            }
                            string a = sr.ReadLine();
                            if (a != "" && a != null)
                            {
                                SetTabLayer(TabFields[i, j], a, out TabFieldsView[i, j]);
                            }
                        }
                    }
                    sr.Close();
                }
                ViewModeOnButton.IsVisible = false;
                EditModeOnButton.IsVisible = true;
            }
        }

        // Event Read:
        private async void SelectFileFromFilePicker_MethodAsync()
        {
            var result = await FilePicker.PickAsync();
            if (result != null)
            {
                FileName = result.FileName;
                //var pathForImage = Path.Combine(SavedDirePath, result.FileName);
                //if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                //    result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase) ||
                //    result.FileName.EndsWith("gif", StringComparison.OrdinalIgnoreCase) ||
                //    result.FileName.EndsWith("bmp", StringComparison.OrdinalIgnoreCase) ||
                //    result.FileName.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase))
                ReadTabsFromFile(result.FullPath);
                FileAbsolutePath = result.FullPath;
            }
        }

        #endregion

        #region SetControlinTabulature

        private void SetTabLayer(Grid grid, string text, out View view)
        {
            Label label = new Label()
            {
                Text = text,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                BackgroundColor = PageBackgroundColor,
                FontSize = 16,
                VerticalOptions = LayoutOptions.Center
            };
            grid.Children.Add(label);
            view = label;
        }

        private void SetTabEntry(Grid grid, string text, out View view)
        {
            Entry entry = new Entry()
            {
                Text = text,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                BackgroundColor = PageBackgroundColor,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill,
                IsTextPredictionEnabled = false,

            };
            grid.Children.Add(entry);
            view = entry;
        }

        #endregion

        #region SwitchModeMethods

        private void SwitchToEditMode()
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 28; j++)
                {

                    // Pobieranie informacji z tego trybu
                    string a = null;
                    if (TabFieldsView[i, j] != null)
                    {
                        if (TabFieldsView[i, j] is Label)
                        {
                            a = (TabFieldsView[i, j] as Label).Text;
                        }
                        else if (TabFieldsView[i, j] is Entry)
                        {
                            a = (TabFieldsView[i, j] as Entry).Text;
                        }

                        TabFields[i, j].Children.Remove(TabFieldsView[i, j]);
                        TabFieldsView[i, j] = null;
                    }

                    // Tworzenie editora
                    SetTabEntry(TabFields[i, j], a, out TabFieldsView[i, j]);
                }
            }

            EditModeOnButton.IsVisible = false;
            ViewModeOnButton.IsVisible = true;
        }

        private void SwitchToViewMode()
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 28; j++)
                {

                    // Pobieranie informacji z tego trybu
                    string a = null;
                    if (TabFieldsView[i, j] != null)
                    {
                        if (TabFieldsView[i, j] is Entry)
                        {
                            a = (TabFieldsView[i, j] as Entry).Text;
                        }
                        else if (TabFieldsView[i, j] is Label)
                        {
                            a = (TabFieldsView[i, j] as Label).Text;
                        }

                        TabFields[i, j].Children.Remove(TabFieldsView[i, j]);
                        TabFieldsView[i, j] = null;
                    }

                    if (a != null && a != "")
                    {
                        // Tworzenie editora
                        SetTabLayer(TabFields[i, j], a, out TabFieldsView[i, j]);
                    }
                }
            }

            ViewModeOnButton.IsVisible = false;
            EditModeOnButton.IsVisible = true;
        }

        #endregion


    }
}
