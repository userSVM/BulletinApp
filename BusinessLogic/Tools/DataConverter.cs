﻿using BusinessLogic.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BusinessLogic.Tools
{
    internal static class DataConverter
    {
        public static IEnumerable<Data.Group> ToGroupCollection(this List<CategoryTree> tree)
        {
            var groups = new List<Data.Group>();

            foreach(var t in tree)
            {
                var categories = new List<string>();
                GetChildCategory(t, categories, groups);
            }
            return groups.ToList();
        }


        static void GetChildCategory(CategoryTree tree, List<string> categories, List<Data.Group> groups)
        {
            categories.Add(tree.Name);

            if(tree.Children != null && tree.Children.Count == 0)
            {
                var group = new Data.Group(categories.ToArray());
                groups.Add(group);
                categories.Remove(tree.Name);
                return;
            }
            foreach(var c in tree.Children)
            {
                GetChildCategory(c, categories, groups);
            }
            categories.Remove(tree.Name);
        }


    }
}
