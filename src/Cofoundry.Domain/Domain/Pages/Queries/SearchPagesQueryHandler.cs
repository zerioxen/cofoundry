﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cofoundry.Domain.Data;
using Cofoundry.Domain.CQS;
using Cofoundry.Core;
using System.Web;
using System.Data.Entity;
using Cofoundry.Core.Web;

namespace Cofoundry.Domain
{
    public class SearchPagesQueryHandler 
        : IQueryHandler<SearchPagesQuery, IEnumerable<PageSearchResult>>
        , IPermissionRestrictedQueryHandler<SearchPagesQuery, IEnumerable<PageSearchResult>>
    {
        #region constructor

        private readonly CofoundryDbContext _dbContext;
        private readonly IQueryExecutor _queryExecutor;
        private readonly IPageVersionModuleModelMapper _moduleDisplayDataFactory;
        private readonly IHtmlSanitizer _htmlSanitizer;

        public SearchPagesQueryHandler(
            CofoundryDbContext dbContext,
            IQueryExecutor queryExecutor,
            IPageVersionModuleModelMapper moduleDisplayDataFactory,
            IHtmlSanitizer htmlSanitizer
            )
        {
            _dbContext = dbContext;
            _queryExecutor = queryExecutor;
            _moduleDisplayDataFactory = moduleDisplayDataFactory;
            _htmlSanitizer = htmlSanitizer;
        }

        #endregion

        /// <remarks>
        /// This has just been copied with slight modification from PagesController and
        /// needs to be refactored.
        /// </remarks>
        public IEnumerable<PageSearchResult> Execute(SearchPagesQuery query, IExecutionContext executionContext)
        {
            if (string.IsNullOrWhiteSpace(query.Text)) yield break;

            var isAuthenticated = executionContext.UserContext.UserId.HasValue;

            // TODO: Search results should look at page titles as well as module content

            // Find any page versions that match by title
            // TODO: Ignore small words like 'of' and 'the'
            var titleMatchesPageVersions = _dbContext
                .Pages
                .Where(p => !p.IsDeleted
                          && p.PageVersions.Any(pv => !pv.IsDeleted && isAuthenticated ? true : pv.WorkFlowStatusId == (int)WorkFlowStatus.Published)
                          && !query.LocaleId.HasValue || p.LocaleId == query.LocaleId
                          && p.PageTypeId == (int)PageType.Generic
                )
                .Select(p => p.PageVersions
                              .OrderByDescending(v => v.CreateDate)
                              .First(pv => !pv.IsDeleted && isAuthenticated ? true : pv.WorkFlowStatusId == (int)WorkFlowStatus.Published)
                    )
                .ToList()
                .Where(v => v.Title.Contains(query.Text)
                        || v.Title.ToLower().Split(new char[] { ' ' }).Intersect(query.Text.ToLower().Split(new char[] { ' ' })).Any()
                    )
                ;


            // TODO: Search should split the search term and lookup individual words as well (and rank them less strongly)

            // Get a list of ALL the page modules for live pages - we'll search through these for any matches
            var pageModules = _dbContext
                .PageVersionModules
                .Include(m => m.PageModuleType)
                .Where(m => !m.PageVersion.IsDeleted)
                .Where(m => !m.PageVersion.Page.IsDeleted)
                .Where(m => isAuthenticated ? true : m.PageVersion.WorkFlowStatusId == (int)WorkFlowStatus.Published)
                .Where(m => !query.LocaleId.HasValue || m.PageVersion.Page.LocaleId == query.LocaleId)
                .Where(m => m.PageVersion.Page.PageTypeId == (int)PageType.Generic)
                .ToList();

            // This will store a list of matches for each module
            var matches = new Dictionary<PageVersionModule, List<string>>();

            foreach (var pageModule in pageModules.Where(p => !string.IsNullOrEmpty(query.Text)))
            {
                var dataProvider = _moduleDisplayDataFactory.MapDisplayModel(pageModule.PageModuleType.FileName, pageModule, WorkFlowStatusQuery.Published);
                Type dataProviderType = dataProvider.GetType();

                // If this module is searchable - ie there is content to search
                // TODO: Module Searching
                //if (dataProvider is ISearchable)
                //{
                    var props = dataProviderType.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(SearchableAttribute)));

                    foreach (var prop in props)
                    {
                        string str = _htmlSanitizer.StripHtml(((string)prop.GetValue(dataProvider, null)));

                        if (str.IndexOf(query.Text ?? "", StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            if (!matches.ContainsKey(pageModule)) matches[pageModule] = new List<string>();

                            int index = str.ToLower().IndexOf(query.Text.ToLower());

                            int startIndex = index - 100;
                            while (startIndex < 0)
                            {
                                startIndex++;
                            }

                            int length = (index - startIndex) + query.Text.Length + 100;
                            while ((startIndex + length) > str.Length)
                            {
                                length--;
                            }

                            var matchStr = str.Substring(startIndex, length);

                            // Stop the string at the last space
                            if ((startIndex + length) < str.Length - 1 && matchStr.LastIndexOf(" ") > -1)
                                matchStr = matchStr.Substring(0, matchStr.LastIndexOf(" ")) + " &hellip;";

                            // Stop the string at the first space
                            if (startIndex > 0 && matchStr.IndexOf(" ") > -1)
                                matchStr = "&hellip; " + matchStr.Substring(matchStr.IndexOf(" ") + 1);

                            // Highlight the search term
                            matchStr = Regex.Replace(matchStr, query.Text, @"<b>$0</b>", RegexOptions.IgnoreCase);

                            matches[pageModule].Add(matchStr);
                        }
                    }
                //}
            }

            // This is a list of pageversions matches to the number of matches
            var pageVersionMatches = matches
                .OrderByDescending(m => m.Value.Count)
                .GroupBy(m => m.Key.PageVersion.PageVersionId)
                .ToDictionary(
                    g => g.First().Key.PageVersion,
                    g => g.First().Value.Select(m => (IHtmlString)new HtmlString(m))
                    );
            
            // Add any pages matched by title to the list of matches
            foreach (var pageVersion in titleMatchesPageVersions)
            {
                if (!pageVersionMatches.ContainsKey(pageVersion))
                {
                    pageVersionMatches.Add(pageVersion, null);
                }
            }

            var searchResults = new List<PageSearchResult>();
            var pageroutes = _queryExecutor.GetAll<PageRoute>();

            foreach (var pageVersionMatch in pageVersionMatches
                   .OrderByDescending(m => titleMatchesPageVersions.Contains(m.Key))
                   .ThenByDescending(m => m.Value == null ? 0 : m.Value.Count()))
            {
                var version = pageVersionMatch.Key;
                var route = pageroutes.SingleOrDefault(r => r.PageId == version.PageId);

                if (route != null)
                {
                    var result = new PageSearchResult();
                    result.FoundText = pageVersionMatch.Value == null ? new HtmlString(version.MetaDescription) : pageVersionMatch.Value.First();
                    result.Title = version.Title;
                    result.Url = route.FullPath;

                    yield return result;
                }
            }
        }

        #region Permission

        public IEnumerable<IPermissionApplication> GetPermissions(SearchPagesQuery query)
        {
            yield return new PageReadPermission();
        }

        #endregion
    }
}
