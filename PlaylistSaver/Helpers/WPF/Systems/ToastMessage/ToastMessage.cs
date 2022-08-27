﻿using PlaylistSaver.Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ToastMessageService
{
    public class ToastMessage : ViewModelBase
    {
        public ToastMessage()
        {
            ClassInstance = this;
        }

        public static ToastMessage ClassInstance { get; set; } = new ToastMessage();

        private static List<string> DisplaysQuery { get; set; } = new();

        public static async Task Display(string text, bool dissapear = true)
        {
            ClassInstance.Text = text;
            ClassInstance.Visibility = true;
            await Task.Delay(50);
            if (dissapear)
            {
                DisplaysQuery.Add(text);

                // Averate to 12 characters per second
                await Task.Delay(text.Length / 12 * 1000);

                // Don't remove if there are other displays active
                if (DisplaysQuery.Count == 1)
                    ClassInstance.Visibility = false;

                DisplaysQuery.Remove(text);
            }

        }

        public static async Task ProgressSucces(string text)
        {
            ClassInstance.ShowLoadingImage = false;
            ClassInstance.Text = text;
            ClassInstance.Visibility = true;

            if (!BlockDissapearing)
            {
                await Task.Delay(GetDisplayTime(text));
                ClassInstance.Visibility = false;
            }
        }

        private static int GetDisplayTime(string text)
        {
            int displayTime = text.Length / 12 * 1000;
            // Display time to minimal of 3 seconds
            if (displayTime < 4000)
                return 4000;
            else
                return displayTime;
        }

        public static async Task Loading(string text)
        {
            ClassInstance.Text = text;
            ClassInstance.Visibility = true;
            ClassInstance.ShowLoadingImage = true;
            await Task.Delay(50);
        }

        public static async Task Succes(string text)
        {
            ClassInstance.ShowLoadingImage = false;
            ClassInstance.Text = text;
            ClassInstance.Visibility = true;

            await Task.Delay(GetDisplayTime(text));
            ClassInstance.Visibility = false;
        }

        public static async Task ProgressToast(int totalCount, string frontText, string backText = "", bool displaySucces = false, string succesText = "")
        {
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
            ClassInstance.Text = "";
            ClassInstance.Visibility = false;
        }

        public static void IncrementProgress()
        {
            // Suboperations!

            ProgressCounter++;
            ClassInstance.Text = $"{FrontText}{ProgressCounter + 1} of {TotalCount}{BackText}";
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

        private static bool BlockDissapearing { get; set; }
        private static int TotalCount { get; set; }
        private static string FrontText { get; set; }
        private static string BackText { get; set; }
        private static string SuccesText { get; set; }
        private static bool DisplaySucces { get; set; }
        private static int ProgressCounter { get; set; }

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
