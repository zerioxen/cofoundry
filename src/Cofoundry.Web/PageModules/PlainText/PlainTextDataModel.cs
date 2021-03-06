﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cofoundry.Domain;

namespace Cofoundry.Web
{
    /// <summary>
    /// Data model representing multiple lines of simple text 
    /// </summary>
    public class PlainTextDataModel : IPageModuleDataModel, IPageModuleDisplayModel
    {
        [MultiLineText]
        public string PlainText { get; set; }
    }
}