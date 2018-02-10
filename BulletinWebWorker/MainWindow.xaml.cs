﻿using BulletinBridge;
using BulletinBridge.Messages.Base;
using BulletinWebWorker.Managers;
using BulletinWebWorker.Service;
using BulletinWebWorker.Tools;
using FessooFramework.Core;
using FessooFramework.Tools.DCT;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BulletinWebWorker
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DCT.Execute(d =>
            {
                WebWorker.SetBrowser((MyWebBrowser.Child as System.Windows.Forms.WebBrowser));

                UdpManager.Set("127.0.0.1", 5051, 5052);
                DCT.ExecuteAsync(d2 =>
                {
                    UdpManager.Receive(ResponseRouter.ExecuteRouting);
                });

                Tests.ClientCases.AddBulletins();
                WebWorkerManager.BulletinWork.Execute();
            });
           
        }

        //https://stackoverflow.com/questions/40922370/allow-system-windows-forms-webbrowser-to-run-javascript
        //https://stackoverflow.com/questions/17922308/use-latest-version-of-internet-explorer-in-the-webbrowser-control
        static void SetIE8KeyforWebBrowserControl(string appName)
        {
            RegistryKey Regkey = null;
            try
            {
                // For 64 bit machine
                //if (Environment.Is64BitOperatingSystem)
                //     Regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Wow6432Node\\Microsoft\\Internet Explorer\\MAIN\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
                //else  //For 32 bit machine
                Regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);

                // If the path is not correct or
                // if the user haven't priviledges to access the registry
                if (Regkey == null)
                {
                    Console.WriteLine("Application Settings Failed - Address Not found");
                    return;
                }

                string FindAppkey = Convert.ToString(Regkey.GetValue(appName));

                // Check if key is already present
                if (FindAppkey == "11001")
                {
                    Console.WriteLine("Required Application Settings Present");
                    Regkey.Close();
                    return;
                }

                // If a key is not present add the key, Key value 8000 (decimal)
                if (string.IsNullOrEmpty(FindAppkey))
                    Regkey.SetValue(appName, unchecked((int)0x1F40), RegistryValueKind.DWord);

                // Check for the key after adding
                FindAppkey = Convert.ToString(Regkey.GetValue(appName));

                if (FindAppkey == "11001")
                    Console.WriteLine("Application Settings Applied Successfully");
                else
                    Console.WriteLine("Application Settings Failed, Ref: " + FindAppkey);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Application Settings Failed");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Close the Registry
                if (Regkey != null)
                    Regkey.Close();
            }
        }
    }
}