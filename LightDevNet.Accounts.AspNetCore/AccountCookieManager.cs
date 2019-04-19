namespace LightDevNet.Accounts.AspNetCore
{
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Http;

    public class AccountCookieManager : ICookieManager
    {
        private readonly ICookieManager innerManager;
        private readonly string coockiePrefix;
        private readonly string defaultAccount;

        public AccountCookieManager(string coockiePrefix, string defaultAccount = null)
        {
            this.innerManager = new ChunkingCookieManager();
            this.coockiePrefix = coockiePrefix;
            this.defaultAccount = defaultAccount;
        }

        public string GetRequestCookie(HttpContext context, string key)
        {
            this.GetAccountKey(context, ref key);
            return this.innerManager.GetRequestCookie(context, key);
        }

        public void AppendResponseCookie(HttpContext context, string key, string value, CookieOptions options)
        {
            this.GetAccountKey(context, ref key);
            this.innerManager.AppendResponseCookie(context, key, value, options);
        }

        public void DeleteCookie(HttpContext context, string key, CookieOptions options)
        {
            this.GetAccountKey(context, ref key);
            this.innerManager.DeleteCookie(context, key, options);
        }

        private string GetAccountKey(HttpContext context, ref string key)
        {
            var account = context.Items["account"]?.ToString() ?? this.defaultAccount;
            key = this.coockiePrefix + account;
            return account.ToString();
        }
    }
}