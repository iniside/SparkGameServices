using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace Spark.Identity
{
    public class SparkExternalIdGrantValidator : IExtensionGrantValidator
    {
        private readonly ITokenValidator _validator;
        
        public SparkExternalIdGrantValidator(ITokenValidator validator)
        {
            _validator = validator;
        
        }

        public string GrantType => "userid";

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var userId = context.Request.Raw.Get("UserId");
            
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
                return;
            }

            var sub = userId;

            context.Result = new GrantValidationResult(sub, GrantType);
            return;
        }
    }
}
