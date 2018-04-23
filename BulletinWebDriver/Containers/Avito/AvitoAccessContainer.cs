﻿using BulletinBridge.Data;
using BulletinWebDriver;
using BulletinWebDriver.Tools;
using BulletinWebWorker.Containers.Base.Access;
using FessooFramework.Tools.DCT;
using OpenQA.Selenium;
using System;

namespace BulletinWebWorker.Containers.Avito
{
    class AvitoAccessContainer : AccessContainerBase
    {
        public override Guid Uid => BoardIds.Avito;
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Стандарный url для авторизации </summary>
        ///
        /// <value> The login URL. </value>
        ///-------------------------------------------------------------------------------------------------
        public string LoginUrl => @"https://www.avito.ru/profile/login?next=%2Fprofile";
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Стандартный url для входа в профиль  </summary>
        ///
        /// <value> The profile URL. </value>
        ///-------------------------------------------------------------------------------------------------
        public string ProfileUrl => @"https://www.avito.ru/profile";
        AccessPackage currentAccess { get; set; }
        public override bool TryAuth(AccessPackage access)
        {
            var result = false;
            DCT.Execute(d =>
            {
                if(currentAccess == null || 
                (currentAccess.Login != access.Login && currentAccess.Password != access.Password))
                {
                    currentAccess = access;

                    Exit();
                    result = Auth();
                }
                else
                {
                    result = true;
                }
            });
            return result;
        }
        protected override bool Auth()
        {
            var result = false;
            DCT.Execute(d =>
            {
                WebDriver.NavigatePage(LoginUrl);
                if(WebDriver.GetTitle().Contains("Доступ с вашего IP-адреса временно ограничен"))
                {
                    WebDriver.RestartDriver();
                    throw new Exception("Блокировка по IP - требуется смена прокси");
                }
                WebDriver.DoAction(By.Name("login"), e =>  e.SendKeys(currentAccess.Login));
                WebDriver.DoAction(By.Name("password"), e => e.SendKeys(currentAccess.Password));

                WebDriver.DoAction(By.ClassName("login-form-submit"), e => e.Click());
                result = true;
            }, continueExceptionMethod: (d, e) => Auth());
            return result;
        }
        protected override void Exit()
        {
            DCT.Execute(data =>
            {
                WebDriver.NavigatePage("https://www.avito.ru/profile/exit");
            });
        }
    }
}