﻿using BulletinCommand.Helpers;
using BulletinEngine.Core;
using BulletinHub.Tools;
using FessooFramework.Tools.DCT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletinCommand
{
    class Program
    {
        static string Help => "Help informations" + Environment.NewLine + Environment.NewLine
           + "TaskGeneration - full task generation" + Environment.NewLine
           + "TaskGenerationClear - clear data from task genearation" + Environment.NewLine
            ;
        static void Main(string[] args)
        {
            BCT.Execute(c => { });
            while (true)
            {
                #region DEBUG
               // GenerationHelpers.GenerationClearData();
                //Для отладки по Enter в цикле вызывает
                GenerationHelpers.GenerationFull();
                #endregion
                Console.Write("BC> ");
                var text = Console.ReadLine();
                BCT.Execute(c =>
                {
                    var enterArgs = text.Split(new[] { " -" }, StringSplitOptions.None);
                    var command = enterArgs[0];
                    switch (command)
                    {
                        case "TaskGeneration":
                            GenerationHelpers.GenerationFull();
                            break;
                        case "TaskGenerationClear":
                            GenerationHelpers.GenerationClearData();                           
                            break;
                        case "help":
                            Console.WriteLine(Help);
                            break;
                        default:
                            Console.WriteLine("Command not found");
                            break;
                    }
                });
            }
        }
    }
}