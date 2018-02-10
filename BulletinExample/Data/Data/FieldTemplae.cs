﻿using FessooFramework.Objects.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Шаблон поля буллетина. Содержит данные для доступа к полю и его заполнению </summary>
    ///
    /// <remarks>   SV Milovanov, 01.02.2018. </remarks>
    ///-------------------------------------------------------------------------------------------------

    public class FieldTemplate : EntityObject
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Наименование поля </summary>
        ///
        /// <value> The name. </value>
        ///-------------------------------------------------------------------------------------------------

        public string Name { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Тэг поля </summary>
        ///
        /// <value> The tag. </value>
        ///-------------------------------------------------------------------------------------------------

        public string Tag { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Атрибут доступа </summary>
        ///
        /// <value> The attribute. </value>
        ///-------------------------------------------------------------------------------------------------

        public string Attribute { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Является ли картинкой </summary>
        ///
        /// <value> True if this object is image, false if not. </value>
        ///-------------------------------------------------------------------------------------------------

        public bool IsImage { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Является ли значение динамическим </summary>
        ///
        /// <value> True if this object is dynamic, false if not. </value>
        ///-------------------------------------------------------------------------------------------------

        public bool IsDynamic { get; set; }
    }
}