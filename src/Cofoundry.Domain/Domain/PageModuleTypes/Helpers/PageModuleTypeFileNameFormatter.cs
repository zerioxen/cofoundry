﻿using Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofoundry.Domain
{
    /// <summary>
    /// Used to format the 'FileName' property for a PageModuleType base
    /// on set postfix conventions.
    /// </summary>
    public class PageModuleTypeFileNameFormatter : IPageModuleTypeFileNameFormatter
    {
        const string DATAMODEL_SUFFIX = "DataModel";

        /// <summary>
        /// Formats the clean module file name from the name of the
        /// specified data model type . E.g. for 'SingleLineTextDataModel' 
        /// this returns 'SingleLineText'. This should be fairly forgiving, e.g. if 
        /// the postfix isn't in the model name it should just pass back the model name.
        /// </summary>
        /// <param name="dataModelType">Data model type to get the formatted name for</param>
        public string FormatFromDataModelName(string dataModelName)
        {
            Condition.Requires(dataModelName).IsNotNull();
            var fileName = dataModelName;

            if (dataModelName.EndsWith(DATAMODEL_SUFFIX, StringComparison.OrdinalIgnoreCase))
            {
                fileName = dataModelName.Remove(dataModelName.IndexOf(DATAMODEL_SUFFIX));
            }

            return fileName;
        }

        /// <summary>
        /// Formats the clean module file name from the data model
        /// type name. E.g. for 'SingleLineTextDataModel' this returns
        /// 'SingleLineText'. This should be fairly forgiving, e.g. if 
        /// I pass in 'SingleLineText' I should receive 'SingleLineText' back. 
        /// </summary>
        /// <param name="dataModelName">Type name without namespace of the module data model</param>
        public string FormatFromDataModelType(Type dataModelType)
        {
            Condition.Requires(dataModelType).IsNotNull();

            return FormatFromDataModelName(dataModelType.Name);
        }
    }
}
