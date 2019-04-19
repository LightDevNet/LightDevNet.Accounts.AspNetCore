namespace LightDevNet.Accounts.AspNetCore
{
    using System;
    using System.IO;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class AspNetCoreExtensions
    {
        public static AuthenticationBuilder AddAccounts(
            this AuthenticationBuilder builder,
            IDataProtectionProvider dataProtectionProvider,
            Action<CookieAuthenticationOptions> configureOptions = null,
            string coockiePrefix = null,
            string defaultAccount = null)
        {
            var dataProtector = dataProtectionProvider
                .CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", "Cookies", "v2");

            builder.Services.AddHttpContextAccessor();
            builder.Services.TryAddSingleton<AccountIdentity>();

            var coockieManager = new AccountCookieManager(coockiePrefix, defaultAccount);
            return builder
                .AddCookie(x =>
             {
                 x.Cookie.Name = coockiePrefix;
                 x.DataProtectionProvider = dataProtector;
                 x.TicketDataFormat = new TicketDataFormat(dataProtector);
                 x.CookieManager = coockieManager;

                 configureOptions?.Invoke(x);
             });
        }

        public static AuthenticationBuilder AddAccounts(
            this AuthenticationBuilder builder,
            string keyDirectory,
            Action<CookieAuthenticationOptions> configureOptions = null,
            string coockiePrefix = null,
            string defaultAccount = null)
        {
            return builder.AddAccounts(
                DataProtectionProvider.Create(new DirectoryInfo(keyDirectory)),
                configureOptions,
                coockiePrefix,
                defaultAccount);
        }

        public static IApplicationBuilder UseAccounts(
            this IApplicationBuilder app,
            string coockiePrefix = null,
            string defaultAccount = null)
        {
            return app.Use(async (context, next) =>
            {
                if (context.Request.Path != null)
                {
                    context.Items["account"] = context.Request.Path.Value.Trim().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[0];
                }

                await next();
            });
        }

        public static IApplicationBuilder UseAccountsMvc(this IApplicationBuilder app)
        {
            return app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{account}/{controller}/{action=Get}/{id?}");
            });
        }
    }
}