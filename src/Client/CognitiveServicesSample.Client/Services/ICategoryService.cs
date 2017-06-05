﻿using CognitiveServicesSample.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveServicesSample.Client.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<CategorizedImageResponse> LoadCategorizedImagesAsync(string category, string continuation);
    }
}
