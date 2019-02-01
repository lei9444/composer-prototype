﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Types;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TestBot.Json.Recognizers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            RegisterTypes();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IStorage dataStore = new MemoryStorage();
            var conversationState = new ConversationState(dataStore);

            // Get Bot file
            string rootDialog = string.Empty;
            var botFile = Configuration.GetSection("Bot").Get<BotFile>();
            var botProject = BotProject.Load(botFile);
            if (botProject.Dialogs.Count > 0)
            {
                rootDialog = botProject.Dialogs[0];
            }
           
            var accessors = new TestBotAccessors
            {
                ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                ConversationState = conversationState,
                RootDialogFile = botProject.path + rootDialog
            };

            services.AddBot<IBot>(
                (IServiceProvider sp) =>
                {
                    return new TestBot(accessors);
                },
                (BotFrameworkOptions options) =>
                {
                    options.OnTurnError = async (turnContext, exception) =>
                    {
                        await conversationState.ClearStateAsync(turnContext);
                        await conversationState.SaveChangesAsync(turnContext);
                    };
                    options.Middleware.Add(new AutoSaveStateMiddleware(conversationState));
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }

        private void RegisterTypes()
        {
            Factory.Register("http://schemas.botframework.com/RuleRecognizer", typeof(RuleRecognizer));
        }
    }
}
