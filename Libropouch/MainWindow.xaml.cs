﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Libropouch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow MW;

        public MainWindow()
        {
            MW = this;            
            InitializeComponent();

            //new UsbSync();

            if (Properties.Settings.Default.UsbAutoDetect)
                new ReaderDetector(this); //Start reader detection which automatically triggers UsbSync when reader is connected to the pc                      
        }

        private static readonly Queue<Tuple<string, byte>> InfoQueue = new Queue<Tuple<string, byte>>();        
        private static bool _infoBoxVisible;

        public static void Info(string text, byte type = 0)
        {
            
            if (Properties.Settings.Default.UseInfoBanner == false)
            {
                MessageBox.Show(text);
                return;
            }

            if (text != "")            
                InfoQueue.Enqueue(Tuple.Create(text, type)); //Add info into the info queue                                                              

            if (_infoBoxVisible || InfoQueue.Count == 0) //Proceed only if infobox isn't currently displayed and if the info queue isn't empty
                return;            
            
            
            var info = InfoQueue.Dequeue(); //item1 = text, item2 = type
            text = info.Item1;
            type = info.Item2;
            var delay = (int) (text.Length * 0.09); //How long will be the infobox displayed, based in the text length, 1 letter = 0.09 sec

            MW.InfoBox.Text = text;
            
            var border = (Border)LogicalTreeHelper.GetParent(MW.InfoBox);

            if (type == 0) //Change colors of the infobox depending on the type of info being displayed (normal info, or error)
                border.Style = (Style)MW.FindResource("InfoBoxOkBg");
            else
                border.Style = (Style)MW.FindResource("InfoBoxErrorBg");
            
            _infoBoxVisible = true;
                                
            var sb = (Storyboard) MW.FindResource("InfoDissolve");

            foreach (var animation in sb.Children.OfType<DoubleAnimation>().Where(animation => animation.Name == "OutOpacity"))
                animation.BeginTime = new TimeSpan(0, 0, delay);

            foreach (var keyFrame in sb.Children.OfType<ObjectAnimationUsingKeyFrames>().Where(oaukf => oaukf.Name == "OutVisibility").SelectMany(oaukf => oaukf.KeyFrames.Cast<DiscreteObjectKeyFrame>()))
            {
                keyFrame.KeyTime = new TimeSpan(0, 0, delay + 2);
            }

            Storyboard.SetTarget(sb, border);
            sb.Begin();                                     
        }

        private void BookGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
               
        }        

        private void Sync_OnClick(object sender, RoutedEventArgs e)
        {                        
            Info(String.Format("Searching for connected {0}...", Properties.Settings.Default.UsbModel));  

            new UsbSync();
        }

        private void InfoBox_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) //Upon closing (hiding) the infobox, trigger the info method, to check if there is another info waiting in the queue
        {
            var border = (Border) sender;

            if (border.Visibility == Visibility.Hidden)
            {
                _infoBoxVisible = false;
                Info("");
                
            }
            else
            {
                _infoBoxVisible = true;
            }

            Debug.WriteLine("Changed vis: " + border.Visibility);
        }
    }
}
