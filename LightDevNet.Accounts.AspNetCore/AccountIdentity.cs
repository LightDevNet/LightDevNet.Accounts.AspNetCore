namespace LightDevNet.Accounts.AspNetCore
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;

    using Microsoft.AspNetCore.Http;

    public class AccountIdentity
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public AccountIdentity(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string Account => this.httpContextAccessor.HttpContext.Items["account"]?.ToString();

        public IIdentity Identity => this.httpContextAccessor.HttpContext.User.Identity;

        public int UserId
        {
            get
            {
                var str = ((ClaimsIdentity)this.Identity).Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).Select(x => x.Value).FirstOrDefault();
                if (!int.TryParse(str, out int userId))
                {
                    throw new UnauthorizedAccessException();
                }

                return userId;
            }
        }

        public T Impersonate<T>(int userId, Func<T> func)
        {
            var ci = (ClaimsIdentity)this.Identity;
            var claim = ci.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
            var newClaim = new Claim(claim.Type, userId.ToString());

            try
            {
                ci.RemoveClaim(claim);
                ci.AddClaim(newClaim);

                return func();
            }
            finally
            {
                ci.TryRemoveClaim(newClaim);
                ci.AddClaim(claim);
            }
        }
    }
}