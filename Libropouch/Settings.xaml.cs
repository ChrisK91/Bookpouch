﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Libropouch
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>

    public partial class Settings
    {
        public Settings()
        {            
            InitializeComponent();
        }

        private void Language_OnLoaded(object sender, RoutedEventArgs e)
        {
            var comboBox = (ComboBox) sender;
            var langList = new List<LanguageOption>
            {
                new LanguageOption("- Automatic -", "", ""),
                new LanguageOption("English", "en", "US"),
                new LanguageOption("Česky", "cs", "CZ")
            };

            var position = Array.IndexOf(new[] {"", "en-US", "cs-CZ"}, Properties.Settings.Default.Language);
            comboBox.ItemsSource = langList;
            comboBox.SelectedIndex = (position > 0 ? position : 0);
            
        }

        //Save language change and also start using the new language
        private void Language_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox) sender;
            var language = (LanguageOption) comboBox.SelectedItem;
            var languageCode = (language.CountryCode != "" ? String.Format("{0}-{1}", language.Code, language.CountryCode) : "");

            Properties.Settings.Default.Language = languageCode;            
            Properties.Settings.Default.Save();

            Thread.CurrentThread.CurrentUICulture = new CultureInfo((languageCode != "" ? languageCode : CultureInfo.CurrentCulture.Name));        
        }

        //Populating reader drop down list
        private void Reader_OnLoaded(object sender, RoutedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var langList = new List<ReaderOption>
            {                    
                new ReaderOption("kindle", "KINDLE", "Documents"),
                new ReaderOption("nook", "NOOK", "my documents"),                
                new ReaderOption("- other -", "", ""),        
            };

            var position = Array.IndexOf(new[] { "kindle", "nook"}, Properties.Settings.Default.DeviceModel);
            comboBox.ItemsSource = langList;            
            comboBox.SelectedIndex = (position != -1 ? position : 2);

        }

        //Saving change from the reader drop down list
        private void Reader_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox) sender;
            var readerOption = (ReaderOption) comboBox.SelectedItem;

            if (readerOption.PnpDeviceId == "") //If user selects unknown device, don't save anything and display form for manual device info input
            {
                UnknownDeviceForm.Visibility = Visibility.Visible;
                return;
            }
            
            UnknownDeviceForm.Visibility = Visibility.Collapsed;

            Properties.Settings.Default.DeviceModel = readerOption.Model;
            Properties.Settings.Default.DevicePnpId = readerOption.PnpDeviceId;
            Properties.Settings.Default.DeviceRootDir = readerOption.RootDir;
            Properties.Settings.Default.Save();

        }

        private void UnknownDeviceHint_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You have selected an unknown reader device and therefore you will need to manually input the identification strings, so that your reader can be correctly recognized and handled by Libropouch.\nHover over the fields with your cursor to display tooltips.");
        }

        //Handle loading  values for all settings checkboxes
        private void CheckBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            var box = (CheckBox) sender;
            var prop = Properties.Settings.Default.GetType().GetProperty(box.Name);

            if(prop != null)
                box.IsChecked = (bool) prop.GetValue(Properties.Settings.Default);                       

        }

        //Handle saving  values for all settings checkboxes
        private void CheckBox_OnChecked(object sender, RoutedEventArgs e)
        {           
            var box = (CheckBox) sender;
            var prop = Properties.Settings.Default.GetType().GetProperty(box.Name);             

            if(prop != null)
                prop.SetValue(Properties.Settings.Default, box.IsChecked);                      

            Properties.Settings.Default.Save();            
        }

        //Handle loading values for all settings text fields
        private void TextBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            var box = (TextBox) sender;
            var prop = Properties.Settings.Default.GetType().GetProperty(box.Name);

            if (prop != null)
                box.Text = (string) prop.GetValue(Properties.Settings.Default);        
        }

        //Handle saving values for all settings text fields
        private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        { 
            var box = (TextBox) sender;
            var prop = Properties.Settings.Default.GetType().GetProperty(box.Name);

            if (prop != null)
                prop.SetValue(Properties.Settings.Default, box.Text);

            

            Properties.Settings.Default.Save();            
        }

        internal sealed class LanguageOption //Class representing items in the language selection drop down menu
        {
            public string Name { private set; get; }
            public string FlagPath { private set; get; }

            public readonly string Code;
            public readonly string CountryCode;

            public LanguageOption(string name, string code, string coutnryCode)
            {
                Name = name;
                Code = code;
                CountryCode = coutnryCode;                
                FlagPath = "Flags/" + (coutnryCode != "" ? coutnryCode : "_unknown") + ".png";                
            }

        }

        internal sealed class ReaderOption //Class representing items in the reader selection drop down menu
        {            
            public string Model { private set; get; }

            public string PnpDeviceId;
            public string RootDir;
            private readonly String[] _readerImgs = {"kindle", "nook"}; //List of model names for which we have pictures present in the readers folder

            public string ImagePath
            {
                get //Return image path for the reader model name specified in Model or default picture if unkwnon model name is supplied
                {
                    return "Readers/" + (Array.IndexOf(_readerImgs, Model) != -1 ? Model.ToLower() : "_unknown") + ".png";
                }
            }
                     

            public ReaderOption(string model, string pnpDeviceId, string rootDir)
            {                
                Model = model;
                PnpDeviceId = pnpDeviceId;
                RootDir = rootDir;
            }

        }

 
    }
    
}