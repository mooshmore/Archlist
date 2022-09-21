using Archlist.ProgramData.Bases;
using Archlist.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ToastMessageService
{
    public enum IconType
    {
        None,
        Error,
        Warning
    }

    public class ToastMessage : ViewModelBase
    {
        public static ToastMessage ClassInstance { get; set; } = new ToastMessage();

        public ToastMessage()
        {
            ClassInstance = this;
            ButtonPressedCommand = new RelayCommand(ButtonPressed);
        }

        private static List<string> DisplaysQuery { get; set; } = new();

        public static async Task Display(string text, bool dissapear = true)
        {
            ClassInstance.Text = text;
            ClassInstance.Visibility = true;
            await Task.Delay(50);

            // The whole displays query is here because of a scenario:
            // Toast messages by default can get piled up on each other, so for example:

            // 1: Toast message "1" is displayed with a delay of 3000ms
            // 2: 2000ms later, a Toast message "2" is displayed with a delay of 3000ms

            // And normally, you would want the toast message "2" to dissapear in 3000ms,
            // but the thing is a hide delay from the first message is still active,
            // so it will hide the "2" message in a 1000ms.
            // The DisplaysQuery will prevent that and only hide stuff if it is the only
            // message in queue.
            if (dissapear)
            {
                DisplaysQuery.Add(text);

                // Averate to 12 characters per second
                await WaitDisplayTimeAsync(text);

                // Don't remove if there are other displays active
                if (DisplaysQuery.Count == 1)
                    ClassInstance.Visibility = false;

                DisplaysQuery.Remove(text);
            }

        }

        public RelayCommand ButtonPressedCommand { get; }

        public static void InformationDialog(string text, IconType iconType = IconType.None)
        {
            InformationMessagesQueue.Add(text);
            // If there is nothing in the queue display the item straight away
            if (InformationMessagesQueue.Count == 1)
            {
                ResetAndSetText(InformationMessagesQueue[0]);
                ClassInstance.DisplayButton = true;
            }
        }

        public static List<string> InformationMessagesQueue { get; set; } = new();

        private void ButtonPressed()
        {
            InformationMessagesQueue.RemoveAt(0);

            if (InformationMessagesQueue.Count != 0)
            {
                ResetAndSetText(InformationMessagesQueue[0]);
                ClassInstance.DisplayButton = true;
            }
            else
                ClassInstance.Visibility = false;
        }

        private static void ResetAndSetText(string text)
        {
            ClassInstance.DisplayButton = false;
            ClassInstance.ShowLoadingImage = false;
            ClassInstance.AdditionalInfoVisibility = false;
            ClassInstance.Visibility = true;

            if (text.Contains("';'"))
            {
                ClassInstance.Text = text.TrimTo("';'");
                ClassInstance.AdditionalInfoVisibility = true;
                ClassInstance.AdditionalInfoText = text.TrimFrom("';'");
            }
            else
                ClassInstance.Text = text;

        }

        private static async Task WaitDisplayTimeAsync(string text)
        {
            int displayTime = text.Length / 12 * 1000;
            // Display time to minimal of 3 seconds
            if (displayTime < 2000)
                displayTime = 2000;

            await Task.Delay(displayTime);
        }

        private static async Task WaitHideDisplayTimeAsync(string text)
        {
            await WaitDisplayTimeAsync(text);
            ClassInstance.Visibility = false;
        }


        public static async Task Hide(bool forceHide = false)
        {
            if (InformationMessagesQueue.Count != 0 && !forceHide)
                return;

            ClassInstance.Text = "";
            ClassInstance.Visibility = false;
            ClassInstance.ShowLoadingImage = false;
        }

        #region Default styles

        public static async Task Succes(string text)
        {
            if (InformationMessagesQueue.Count != 0)
                return;

            ResetAndSetText(text);
            await WaitHideDisplayTimeAsync(text);
        }

        public static async Task Warning(string text)
        {
            if (InformationMessagesQueue.Count != 0)
                return;

            ResetAndSetText(text);
            await WaitHideDisplayTimeAsync(text);
        }


        public static void Loading(string text)
        {
            if (InformationMessagesQueue.Count != 0)
                return;

            ResetAndSetText(text);
            ClassInstance.ShowLoadingImage = true;
        }

        public static void Information(string text)
        {
            if (InformationMessagesQueue.Count != 0)
                return;

            ResetAndSetText(text);
            ClassInstance.ShowLoadingImage = true;
        }


        public static void NotImplemented()
        {
            Display("Not implemented yet");
        }

        #endregion

        #region Progress toast

        public static async Task ProgressToast(int totalCount, string frontText, string backText = "", bool displaySucces = false, string succesText = "")
        {
            if (InformationMessagesQueue.Count != 0)
                return;

            ProgressCounter = 0;
            TotalCount = totalCount;
            FrontText = frontText + " ";
            BackText = " " + backText;
            SuccesText = succesText;
            DisplaySucces = displaySucces;
            ClassInstance.Text = $"{FrontText}{ProgressCounter + 1} of {TotalCount}{BackText}";
            ClassInstance.Visibility = true;
            ClassInstance.ShowLoadingImage = true;

            await Task.Delay(50);
        }

        public static void ForceEndProgressToast()
        {
            if (InformationMessagesQueue.Count != 0)
                return;

            ClassInstance.Text = "";
            ClassInstance.Visibility = false;
        }

        public static void IncrementProgress()
        {
            if (InformationMessagesQueue.Count != 0)
                return;

            ProgressCounter++;
            ClassInstance.Text = $"{FrontText}{ProgressCounter + 1} of {TotalCount}{BackText}";
            ClassInstance.Visibility = true;
            BlockDissapearing = true;

            if (ProgressCounter >= TotalCount)
            {
                BlockDissapearing = false;
                if (DisplaySucces)
                    ProgressSucces(SuccesText == "" ? "Done!" : SuccesText);
                else
                    ClassInstance.Visibility = false;
            }
        }

        public static async Task ProgressSucces(string text)
        {
            if (InformationMessagesQueue.Count != 0)
                return;

            ResetAndSetText(text);

            if (!BlockDissapearing)
            {
                await WaitDisplayTimeAsync(text);
                ClassInstance.Visibility = false;
            }
        }

        #endregion

        private static bool BlockDissapearing { get; set; }
        private static int TotalCount { get; set; }
        private static string FrontText { get; set; }
        private static string BackText { get; set; }
        private static string SuccesText { get; set; }
        private static bool DisplaySucces { get; set; }
        private static int ProgressCounter { get; set; }

        private string _additionalInfoText;
        public string AdditionalInfoText
        {
            get => _additionalInfoText;
            set
            {
                _additionalInfoText = value;
                RaisePropertyChanged();
            }
        }

        private bool _additionalInfoVisibility = false;
        public bool AdditionalInfoVisibility
        {
            get => _additionalInfoVisibility;
            set
            {
                _additionalInfoVisibility = value;
                RaisePropertyChanged();
            }
        }

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                RaisePropertyChanged();
            }
        }

        private string _informationButtonText = "Ok";
        public string InformationButtonText
        {
            get => _informationButtonText;
            set
            {
                _informationButtonText = value;
                RaisePropertyChanged();
            }
        }

        private bool _showLoadingImage = false;
        public bool ShowLoadingImage
        {
            get => _showLoadingImage;
            set
            {
                _showLoadingImage = value;
                RaisePropertyChanged();
            }
        }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get => _image;
            set
            {
                _image = value;
                RaisePropertyChanged();
            }
        }

        private bool _displayButton = false;

        public bool DisplayButton
        {
            get => _displayButton;
            set
            {
                _displayButton = value;
                RaisePropertyChanged();
            }
        }

        private bool _visibility = false;

        public bool Visibility
        {
            get => _visibility;
            set
            {
                _visibility = value;
                RaisePropertyChanged();
            }
        }
    }
}
