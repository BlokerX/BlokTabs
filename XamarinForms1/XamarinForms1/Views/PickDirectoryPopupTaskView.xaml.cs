using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SimplePopupForm.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PickDirectoryPopupTaskView
    {
        public PickDirectoryPopupTaskView(string InitialPath, PickType ItemsPickType = PickType.File)
        {
            InitializeComponent();
            this.ThisDirePath = InitialPath;
            this.ItemsPicksType = ItemsPickType;

            LoadDiresAndFiles(ThisDirePath);

            if (ItemsPicksType == PickType.File)
            {
                SelectButton.IsEnabled = false;
                SetAbsolutePath("Select file");
            }
            else if (ItemsPicksType == PickType.Directory)
                SetAbsolutePath(ThisDirePath);
        }

        // Paths:
        public string ThisDirePath { get; private set; }
        public string AbsolutePath { get; private set; }

        private void SetAbsolutePath(string path)
        {
            this.ThisDireNameLabel.Text = AbsolutePath = path;
            SelectButton.IsEnabled = true;
        }

        // Props:
        public readonly PickType ItemsPicksType = PickType.File;

        // Events:
        public event EventHandler SelectClicked;
        public event EventHandler CancelClicked;

        private void SelectButton_Clicked(object sender, EventArgs e)
        {
            SelectClicked?.Invoke(this, e);
            this.OnChildRemoved(this);
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            CancelClicked?.Invoke(this, e);
            this.OnChildRemoved(this);
        }

        // Environment properties:
        private string[] Dires { get; set; }
        private View[] DiresButtons { get; set; }
        private string[] Files { get; set; }
        private View[] FilesButtons { get; set; }

        private void LoadDiresAndFiles(string direPath)
        {
            ClearDireView();
            LoadDires(direPath);
            if (ItemsPicksType == PickType.File)
                LoadFiles(direPath);
            this.ThisDireAbsolutePathLabel.Text = ThisDirePath;
        }

        private void LoadDires(string direPath)
        {
            ClearDireView();
            var D = Directory.GetDirectories(direPath);
            Dires = D;
            DiresButtons = new View[Dires.Length];
            for (int i = 0; i < Dires.Length; i++)
            {
                Button b = new Button()
                {
                    Text = Dires[i].Substring(Dires[i].LastIndexOf(Path.VolumeSeparatorChar) + 1),
                    TextTransform = TextTransform.None,
                    HorizontalOptions = LayoutOptions.Fill,
                    Padding = 0,
                    CornerRadius = 0
                };
                b.Clicked += DireButton_Clicked;
                DireView.Children.Add(b);
                DiresButtons[i] = b;
            }
        }

        private void LoadFiles(string direPath)
        {
            var F = new DirectoryInfo(direPath).GetFiles("*", SearchOption.TopDirectoryOnly);
            Files = new string[F.Length];
            for (int i = 0; i < F.Length; i++)
            {
                Files[i] = F[i].FullName;
            }
            FilesButtons = new View[Files.Length];
            for (int i = 0; i < Files.Length; i++)
            {
                Button b = new Button()
                {
                    Text = Files[i].Substring(Files[i].LastIndexOf(Path.VolumeSeparatorChar) + 1),
                    TextTransform = TextTransform.None,
                    HorizontalOptions = LayoutOptions.Fill,
                    Padding = 0,
                    CornerRadius = 0
                };
                b.Clicked += FileButton_Clicked;
                DireView.Children.Add(b);
                FilesButtons[i] = b;
            }
        }

        private void FileButton_Clicked(object sender, EventArgs e)
        {
            var path = Path.Combine(ThisDirePath, (sender as Button).Text);
            ThisDirePath = Path.Combine(path);
            if (ItemsPicksType == PickType.File)
                SetAbsolutePath(path);
        }

        private void ClearDireView()
        {
            if (DiresButtons?.Length > 0)
            {
                foreach (var item in DiresButtons)
                {
                    this.OnChildRemoved(item);
                }
                DiresButtons = null;
                Dires = null;
            }
            if (ItemsPicksType == PickType.File && FilesButtons?.Length > 0)
            {
                foreach (var item in FilesButtons)
                {
                    this.OnChildRemoved(item);
                }
                FilesButtons = null;
                Files = null;
            }
            DireView.Children.Clear();
        }

        private void DireButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var path = Path.Combine(ThisDirePath, (sender as Button).Text);
                if (Directory.GetFileSystemEntries(path)?.Length >= 0)
                {
                    ThisDirePath = Path.Combine(path);
                    if (ItemsPicksType == PickType.Directory)
                        SetAbsolutePath(ThisDirePath);
                    LoadDiresAndFiles(ThisDirePath);
                }
            }
            catch (System.UnauthorizedAccessException)
            {

            }
        }

        private void UpDireButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (Directory.GetParent(ThisDirePath).Exists &&
                    Directory.GetParent(ThisDirePath).GetDirectories()?.Length > 0)
                {
                    LoadDiresAndFiles(ThisDirePath = Directory.GetParent(ThisDirePath).FullName);
                    if (ItemsPicksType == PickType.Directory)
                        SetAbsolutePath(ThisDirePath);
                }
            }
            catch (System.IO.DirectoryNotFoundException)
            {

            }
        }

        private void NewFolderButton_Clicked(object sender, EventArgs e)
        {
            PopupEditTextTaskView popup;
            PopupNavigation.PushAsync(popup = new PopupEditTextTaskView("Create new folder","Dire name:", "NewFolder"));
            popup.OKClicked += NewFolder_DireNamePopup_Clicked;
        }

        private void NewFolder_DireNamePopup_Clicked(object sender, EventArgs e)
        {
            try
            {
                var path = Path.Combine(ThisDirePath, (sender as PopupEditTextTaskView).EntryText);
                if (path != null && path != "")
                {
                    Directory.CreateDirectory(path);
                    LoadDiresAndFiles(ThisDirePath);
                }
            }
            catch (System.UnauthorizedAccessException) { }
            catch (System.ArgumentNullException) { }
        }

        public enum PickType
        {
            File,
            Directory
        }

    }
}