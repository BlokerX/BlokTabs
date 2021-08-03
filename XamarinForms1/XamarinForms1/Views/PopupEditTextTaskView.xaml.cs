using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SimplePopupForm.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PopupEditTextTaskView
    {
        public PopupEditTextTaskView(string Title, string Question, string DefaultText=null, string EntryPlaceHolder=null)
        {
            InitializeComponent();
            TitleLabel.Text = Title;
            QuestionLabel.Text = Question;
            Entry1.Text = DefaultText;
            Entry1.Placeholder = EntryPlaceHolder;
        }

        public Task Task { get; set; }
        public string EntryText { get { return Entry1.Text; } }
        public event EventHandler OKClicked;
        public event EventHandler CancelClicked;

        private void PositiveButton_Clicked(object sender, EventArgs e)
        {
            OKClicked?.Invoke(this, e);
            this.OnChildRemoved(this);
        }

        private void NegativeButton_Clicked(object sender, EventArgs e)
        {
            CancelClicked?.Invoke(this, e);
            this.OnChildRemoved(this);
        }
    }
}