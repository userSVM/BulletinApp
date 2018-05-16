﻿using BulletinEngine.Core;
using BulletinEngine.Entity.Data;
using BulletinHub.Helpers;
using BulletinHub.Models;
using FessooFramework.Objects.Data;
using FessooFramework.Tools.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Helpers
{
    static class BulletinHelper
    {
        static string[] forbiddenWords = new[]
       {
            "запчасти", "магазин"
        };

        /// <summary>
        /// Создаем буллетину по шаблону, запускаем задачи на публикацию и активацию
        /// </summary>
        /// <param name="userLogin"></param>
        /// <param name="title"></param>
        public static void AutoPublicateBulletin(string userLogin, string brand, string model, string modifier)
        {
            BCT.Execute(d =>
            {
                //Находим пользователя
                var user = d.MainDb.UserAccesses.FirstOrDefault(q => q.Login == userLogin);
                if (user == null)
                {
                    ConsoleHelper.SendMessage($"AvitoPublicateBulletin => Пользователь с логином {userLogin} не найден");
                    return;
                }
                var userId = user.Id;

                //Находим подходящий шаблон
                var template = ChooseTemplate(brand, model, modifier);
                if (template == null)
                {
                    ConsoleHelper.SendMessage($"AvitoPublicateBulletin => Нет подходящего шаблона для {brand}");
                    return;
                }
                //Добавляем новый буллетин в БД
                string cardCategory1 = "Бытовая электроника";
                string cardCategory2 = "Телефоны";
                string cardCategory3 = "iPhone";
                var groupHash = StringToSha256String(cardCategory1, cardCategory2, cardCategory3, null, null);
                var bulletin = AddAvitoByTemplate(userId, template, brand, model, modifier, groupHash);
                if (bulletin == null)
                {
                    ConsoleHelper.SendMessage($"AvitoPublicateBulletin => Ошибка при создании буллетина");
                    return;
                }
                //Запускаем задачи на публикацию и активацию
                CreatePublicationTasks(bulletin);
            });
        }

        /// <summary>
        /// Накидываем шаблон на существующий буллетин, запускаем задачи на публикацию и активацию
        /// </summary>
        /// <param name="bulletinId"></param>
        public static void AutoPublicateBulletin(Guid bulletinId, string brand, string model, string modifier)
        {
            BCT.Execute(d =>
            {
                var bulletin = d.BulletinDb.Bulletins.FirstOrDefault(q => q.Id == bulletinId);
                if (bulletin == null) return;

                ChooseTemplate(bulletin, brand, model, modifier);
                CreatePublicationTasks(bulletin);
            });
        }


        static Bulletin AddAvitoByTemplate(Guid userId, BulletinTemplate template, string brand, string model, string modifier, string groupHash)
        {
            var result = default(Bulletin);
            BCT.Execute(d =>
            {
                var group = BCT.Context.BulletinDb.Groups.FirstOrDefault(q => q.Hash == groupHash);
                if (group == null)
                {
                    ConsoleHelper.SendMessage($"AvitoPublicateBulletin => Группа с хэшем:{groupHash} не найдена");
                    return;
                }

                result = new Bulletin();
                result.Brand = brand;
                result.Model = model;
                result.Modifier = modifier;
                result.GroupId = group.Id;
                result.Title = template.Title;
                result.Description = template.Description;
                result.Price = template.Price.ToString();
                result.Images = template.Images;
                result.UserId = userId;
                result.StateEnum = BulletinState.Created;

                d.SaveChanges();
            });
            return result;
        }

        static BulletinTemplate ChooseTemplate(string brand, string model, string modifier)
        {
            var result = default(BulletinTemplate);
            BCT.Execute(d =>
            {
                var templates = d.TempDB.BulletinTemplate.Where(q => q.IsIndividualSeller && q.State != (int)DefaultState.Disable
                && q.Category4 != "Запчасти").ToArray();
                // Исключаем шаблоны с запрещенными словами
                var temp = templates.Where(q => forbiddenWords.All(x => !q.Description.ToLower().Contains(x.ToLower()))
                && forbiddenWords.All(x => !q.Title.Contains(x))).ToArray();

                var modelParams = model.Replace("+", "").Split().ToList();
                if (model.Contains("+"))
                    modelParams.Add("+");

                var modifierParams = Enumerable.Empty<string>(); 
                if(modifier != null)
                {
                    modifierParams = modifier.Split();
                }
                //Фильтруем шаблоны по бренду, модели и модификаторам (цвет и т.д.)
                var allMatches = temp.Where(q =>
                q.Title.ToLower().Contains(brand.ToLower())
                && modelParams.All(x =>
                   (x == "+" && (q.Title.ToLower().Contains("+") || q.Title.ToLower().Contains("plus")))
                || (x == "plus" && (q.Title.ToLower().Contains("+") || q.Title.ToLower().Contains("plus")))
                || q.Title.ToLower().Split(' ', '/', ',').Any(qq => qq == x))
                && modifierParams.All(x => q.Title.ToLower().Contains(x)));

                result = allMatches.FirstOrDefault();
                if (result != null)
                {
                    result.StateEnum = DefaultState.Disable;
                    d.SaveChanges();
                }
            });
            return result;
        }

        static void ChooseTemplate(Bulletin bulletin, string brand, string model, string modifier)
        {
            BCT.Execute(d =>
            {
                var chosenBrand = brand ?? bulletin.Brand;
                var chosenModel = model ?? bulletin.Model;
                var chosenModifier = modifier ?? bulletin.Modifier;
                var chosenTemplate = ChooseTemplate(chosenBrand, chosenModel, chosenModifier);
                if (chosenTemplate == null)
                {
                    ConsoleHelper.SendMessage($"AvitoPublicateBulletin => Нет подходящего шаблона для Title:{chosenBrand}");
                    return;
                }
                bulletin.Title = chosenTemplate.Title;
                bulletin.Description = chosenTemplate.Description;
                bulletin.Images = chosenTemplate.Images;
                if (brand != null) bulletin.Brand = brand;
                if (model != null) bulletin.Model = model;
                if (modifier != null) bulletin.Modifier = modifier;

                bulletin.StateEnum = bulletin.StateEnum;
                d.SaveChanges();
            });
        }

        static void CreatePublicationTasks(Bulletin bulletin)
        {
            BCT.Execute(d =>
            {
                var tasks = d.TempDB.Tasks.Where(q => (q.State == (int)BulletinHub.Entity.Data.TaskState.Created || q.State == (int)BulletinHub.Entity.Data.TaskState.Enabled) && q.BulletinId == bulletin.Id && q.Command == (int)BulletinHub.Entity.Data.TaskCommand.InstancePublication).ToArray();
                if (tasks != null && tasks.Any())
                    TaskHelper.Remove(tasks);
                var instances = BulletinHub.Helpers.BulletinHelper.CreateInstance(bulletin);
                if (instances.Any())
                {
                    bulletin.SetGenerationCheck();
                    var datePublish = DateTime.Now;

                    foreach (var instance in instances)
                    {
                        var access = BulletinEngine.Helpers.AccessHelper.GetFreeAccess(bulletin.UserId, instance.BoardId, instance.BulletinId);
                        if (access == null)
                        {
                            ConsoleHelper.SendMessage($"AvitoPublicateBulletin => Не найден свободный доступ для буллетина {bulletin.Id}");
                            continue;
                        }

                        instance.AccessId = access.Id;
                        instance.StateEnum = instance.StateEnum;
                        d.SaveChanges();
                        TaskHelper.CreateInstancePublication(bulletin.UserId, instance, datePublish);
                        var now = DateTime.Now;
                        var activationDate = now.Date.AddDays(1);
                        activationDate.Date.Subtract(now);
                        TaskHelper.CreateActivateInstance(bulletin.UserId, instance, activationDate);
                    }
                }
            });
        }

        /// <summary>
        /// Конвертирую строку в Hash строку шифрованную с помощью 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string StringToSha256String(params string[] str)
        {
            var temp = string.Empty;
            foreach (var s in str)
            {
                temp += s ?? string.Empty;
            }
            return CryptographyHelper.StringToSha256String(temp);
        }
    }
}
