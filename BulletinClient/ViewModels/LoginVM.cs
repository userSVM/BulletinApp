﻿using BulletinClient.Core;
using BulletinClient.Helpers;
using BulletinClient.HelperService;
using BulletinClient.Properties;
using FessooFramework.Objects.Delegate;
using FessooFramework.Objects.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace BulletinClient.ViewModels
{
    public class LoginVM : VM
    {
        public string UserLogin { get; set; }
        public string UserPassword { get; set; }

        public ICommand CommandLogin { get; set; }
        public ICommand CommandRegistration { get; set; }
        public LoginVM()
        {
            CommandLogin = new DelegateCommand(() => SignIn(UserLogin, UserPassword));
            CommandRegistration = new DelegateCommand(Registration);
            if (!string.IsNullOrEmpty(DataHelper.UserLogin.Value)
              && !string.IsNullOrEmpty(DataHelper.UserPassword.Value))
            {
                SignIn();
            }
        }
        private void Registration()
        {
            DataHelper.UserLogin.Value = UserLogin;
            DataHelper.UserPassword.Value = UserPassword;
            MainServiceHelper.Registration(DataHelper.UserLogin.Value, DataHelper.UserPassword.Value, "", "", "", SignInCallback);
        }
        void SignIn(string userLogin, string userPassword)
        {
            DataHelper.UserLogin.Value = UserLogin;
            DataHelper.UserPassword.Value = UserPassword;
            SignIn();
        }
        void SignIn()
        {
            MainServiceHelper.SignIn(DataHelper.UserLogin.Value, DataHelper.UserPassword.Value, SignInCallback);
        }
        void SignInCallback(bool result)
        {
            DCT.Execute(c =>
            {
                if (result)
                {
                    Console.WriteLine($"Signin succesfull");

                    Settings.Default.UserLogin = DataHelper.UserLogin.Value;
                    Settings.Default.UserPassword = DataHelper.UserPassword.Value;

                    Settings.Default.HashUID = c._SessionInfo.HashUID;
                    Settings.Default.SessionUID = c._SessionInfo.SessionUID;
                    Settings.Default.Save();
                    DataHelper.IsAuth.Value = true;

                }
                else
                    MessageBox.Show($"Signin not sucessfull");

                Console.WriteLine($"SessionUID Request = {c._SessionInfo.SessionUID}");
                Console.WriteLine($"HashUID Request = {c._SessionInfo.HashUID}");
            });
        }
    }
}