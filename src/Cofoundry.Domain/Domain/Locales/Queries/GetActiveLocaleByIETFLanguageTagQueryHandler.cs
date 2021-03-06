﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofoundry.Domain.Data;
using Cofoundry.Domain.CQS;
using AutoMapper.QueryableExtensions;
using System.IO;
using System.Text.RegularExpressions;

namespace Cofoundry.Domain
{
    public class GetActiveLocaleByIETFLanguageTagQueryHandler 
        : IQueryHandler<GetActiveLocaleByIETFLanguageTagQuery, ActiveLocale>
        , IAsyncQueryHandler<GetActiveLocaleByIETFLanguageTagQuery, ActiveLocale>
        , IIgnorePermissionCheckHandler
    {
        private readonly IQueryExecutor _queryExecutor;

        public GetActiveLocaleByIETFLanguageTagQueryHandler(
            IQueryExecutor queryExecutor
            )
        {
            _queryExecutor = queryExecutor;
        }


        public ActiveLocale Execute(GetActiveLocaleByIETFLanguageTagQuery query, IExecutionContext executionContext)
        {
            if (!IsTagValid(query.IETFLanguageTag)) return null;

            var result = _queryExecutor
                .GetAll<ActiveLocale>()
                .SingleOrDefault(l => l.IETFLanguageTag.Equals(query.IETFLanguageTag, StringComparison.OrdinalIgnoreCase));

            return result;
        }

        public async Task<ActiveLocale> ExecuteAsync(GetActiveLocaleByIETFLanguageTagQuery query, IExecutionContext executionContext)
        {
            if (!IsTagValid(query.IETFLanguageTag)) return null;

            var result = (await _queryExecutor
                .GetAllAsync<ActiveLocale>())
                .SingleOrDefault(l => l.IETFLanguageTag.Equals(query.IETFLanguageTag, StringComparison.OrdinalIgnoreCase));

            return result;
        }

        private bool IsTagValid(string languageTag)
        {
            return Regex.IsMatch(languageTag, "^[a-zA-Z]{2}(-[a-zA-Z]{2})?$");
        }
    }
}
