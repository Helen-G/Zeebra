﻿using System;
using AFT.RegoV2.Core.Common.Interfaces.Brand;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class DeleteContentTranslationData : IDeleteContentTranslationData
    {
        public Guid Id { get; set; }
    }
}