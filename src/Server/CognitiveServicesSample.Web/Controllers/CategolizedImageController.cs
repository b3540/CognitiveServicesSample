﻿using CognitiveServicesSample.Commons;
using CognitiveServicesSample.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CognitiveServicesSample.Web.Controllers
{
    public class CategolizedImageController : ApiController
    {
        private ICategolizedImageRepository CategolizedImageRepository { get; }
        private ILogger Logger { get; }

        public CategolizedImageController(ICategolizedImageRepository categolizedImageRepository, ILogger logger)
        {
            this.CategolizedImageRepository = categolizedImageRepository;
            this.Logger = logger;
        }

        public async Task<IHttpActionResult> Get(string category, string continuation = null)
        {
            this.Logger.Info($"{nameof(CategolizedImageController)}.{nameof(Get)}({category}, {continuation})");

            var r = await this.CategolizedImageRepository.LoadAsync(category, continuation);
            return Ok(new Commons.CategolizedImageResponse
            {
                CategolizedImages = r.Image.Select(x => new Commons.CategolizedImage
                {
                    Id = x.Id,
                    Category = x.Category,
                    Description = x.Description,
                    Image = x.Image,
                    JaCategory = x.JaCategory,
                    JaDescription = x.JaDescription,
                    Text = x.Text,
                    ThemeColor = x.ThemeColor,
                    TweetId = x.TweetId,
                }),
                Continuation = r.Continues,
            });
        }
    }
}
