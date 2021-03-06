﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cofoundry.Domain.CQS;

namespace Cofoundry.Domain
{
    public class GetPageRouteByIdQueryHandler 
        : IQueryHandler<GetByIdQuery<PageRoute>, PageRoute>
        , IAsyncQueryHandler<GetByIdQuery<PageRoute>, PageRoute>
        , IPermissionRestrictedQueryHandler<GetByIdQuery<PageRoute>, PageRoute>
    {
        private readonly IQueryExecutor _queryExecutor;

        public GetPageRouteByIdQueryHandler(
            IQueryExecutor queryExecutor
            )
        {
            _queryExecutor = queryExecutor;
        }

        public PageRoute Execute(GetByIdQuery<PageRoute> query, IExecutionContext executionContext)
        {
            var result = _queryExecutor
                .GetAll<PageRoute>()
                .SingleOrDefault(p => p.PageId == query.Id);

            return result;
        }

        public async Task<PageRoute> ExecuteAsync(GetByIdQuery<PageRoute> query, IExecutionContext executionContext)
        {
            var result = (await _queryExecutor
                .GetAllAsync<PageRoute>(executionContext))
                .SingleOrDefault(p => p.PageId == query.Id);

            return result;
        }

        #region Permission

        public IEnumerable<IPermissionApplication> GetPermissions(GetByIdQuery<PageRoute> query)
        {
            yield return new PageReadPermission();
        }

        #endregion
    }
}
