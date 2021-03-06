﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cofoundry.Domain.Data;
using Cofoundry.Core.EntityFramework;

namespace Cofoundry.Domain
{
    public class DocumentAssetCommandHelper
    {
        #region constructor

        private readonly CofoundryDbContext _dbContext;
        private readonly IFileStoreService _fileStoreService;
        private readonly ITransactionScopeFactory _transactionScopeFactory;

        public DocumentAssetCommandHelper(
            CofoundryDbContext dbContext,
            IFileStoreService fileStoreService,
            ITransactionScopeFactory transactionScopeFactory
            )
        {
            _dbContext = dbContext;
            _fileStoreService = fileStoreService;
            _transactionScopeFactory = transactionScopeFactory;
        }

        #endregion

        #region public methods

        public async Task SaveFile(IUploadedFile uploadedFile, DocumentAsset documentAsset)
        {
            using (var inputSteam = await uploadedFile.GetFileStreamAsync())
            {
                bool isNew = documentAsset.DocumentAssetId < 1;

                documentAsset.FileExtension = Path.GetExtension(uploadedFile.FileName).TrimStart('.');
                documentAsset.FileSizeInBytes = Convert.ToInt32(inputSteam.Length);
                documentAsset.ContentType = uploadedFile.MimeType;

                // Save at this point if it's a new image
                if (isNew)
                {
                    await _dbContext.SaveChangesAsync();
                }

                var fileName = Path.ChangeExtension(documentAsset.DocumentAssetId.ToString(), documentAsset.FileExtension);

                using (var scope = _transactionScopeFactory.Create())
                {
                    // Save the raw file directly
                    CreateFile(isNew, fileName, inputSteam);

                    if (!isNew)
                    {
                        await _dbContext.SaveChangesAsync();
                    }
                    scope.Complete();
                }
            }
        }

        /// <summary>
        /// Some shared validation to prevent image asset types from being added to documents.
        /// </summary>
        public static IEnumerable<ValidationResult> Validate(IUploadedFile file)
        {
            if (file != null && !string.IsNullOrWhiteSpace(file.FileName))
            {
                var ext = Path.GetExtension(file.FileName);

                if ((ImageAssetConstants.PermittedImageTypes.ContainsKey(ext))
                || (!string.IsNullOrEmpty(file.MimeType) && ImageAssetConstants.PermittedImageTypes.ContainsValue(file.MimeType)))
                {
                    yield return new ValidationResult("Image files shoud be uploaded in the image assets section.", new string[] { "File" });
                }
            }
        }
        
        #endregion

        #region private helpers

        private void CreateFile(bool isNew, string fileName, Stream outputStream)
        {
            if (isNew)
            {
                _fileStoreService.Create(DocumentAssetConstants.FileContainerName, fileName, outputStream);
            }
            else
            {
                _fileStoreService.CreateOrReplace(DocumentAssetConstants.FileContainerName, fileName, outputStream);
            }
        }

        #endregion
    }
}
