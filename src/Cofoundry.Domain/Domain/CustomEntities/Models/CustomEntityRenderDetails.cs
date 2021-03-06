﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofoundry.Domain
{
    /// <summary>
    /// Data used for rendering a specific version of a custom entity
    /// out to a page.
    /// </summary>
    public class CustomEntityRenderDetails : ICustomEntityRoutable
    {
        public int CustomEntityId { get; set; }

        public string CustomEntityDefinitionCode { get; set; }

        public int CustomEntityVersionId { get; set; }

        public int? Ordering { get; set; }

        public ActiveLocale Locale { get; set; }

        public string Title { get; set; }

        public string UrlSlug { get; set; }

        public WorkFlowStatus WorkFlowStatus { get; set; }

        public ICustomEntityVersionDataModel Model { get; set; }

        public DateTime CreateDate { get; set; }

        public IEnumerable<CustomEntityPageSectionRenderDetails> Sections { get; set; }

        /// <summary>
        /// If this custom entity has page routes asspciated with it
        /// they will be included here. Typically you'd only expect a
        /// single page on a site to be associated with a custom entitiy 
        /// details, but it's technically possible to have many.
        /// </summary>
        public IEnumerable<string> DetailsPageUrls { get; set; }
    }
}
