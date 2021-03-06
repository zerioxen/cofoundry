﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofoundry.Core.AutoUpdate;
using Cofoundry.Domain.CQS;

namespace Cofoundry.Domain.Installation
{
    public class RegisterPageTemplatesAndModuleTypesCommandHandler : IAsyncAlwaysRunUpdateCommandHandler<RegisterPageTemplatesAndModuleTypesCommand>
    {
        private readonly ICommandExecutor _commandExecutor;
        private readonly IExecutionContextFactory _executionContextFactory;

        public RegisterPageTemplatesAndModuleTypesCommandHandler(
            ICommandExecutor commandExecutor,
            IExecutionContextFactory executionContextFactory
            )
        {
            _commandExecutor = commandExecutor;
            _executionContextFactory = executionContextFactory;
        }


        public async Task ExecuteAsync(RegisterPageTemplatesAndModuleTypesCommand command)
        {
            var cx = await _executionContextFactory.CreateSystemUserExecutionContextAsync();
            await _commandExecutor.ExecuteAsync(new RegisterPageTemplatesCommand(), cx);
            await _commandExecutor.ExecuteAsync(new RegisterPageModuleTypesCommand(), cx);
        }
    }
}
