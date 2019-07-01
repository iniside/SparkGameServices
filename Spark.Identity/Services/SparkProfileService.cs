using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;

namespace Spark.Identity.Services
{
    public class SparkProfileService : IProfileService
    {
        protected readonly ILogger Logger;

        public SparkProfileService(ILogger<SparkProfileService> logger)
        {
            Logger = logger;
        }


        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();

            Logger.LogDebug("Get profile called for subject {subject} from client {client} with claim types {claimTypes} via {caller}",
                context.Subject.GetSubjectId(),
                context.Client.ClientName ?? context.Client.ClientId,
                context.RequestedClaimTypes,
                context.Caller);


            var claims = new List<Claim>
            {
                new Claim("id", context.Subject.GetSubjectId()),
                new Claim("username", ""),
                new Claim("email", "")

            };

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            context.IsActive = sub != null;
        }
    }
}
