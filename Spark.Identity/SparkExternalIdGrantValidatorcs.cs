using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace Spark.Identity
{
    public class SparkExternalIdGrantValidatorcs : IExtensionGrantValidator
    {
        private readonly ITokenValidator _validator;
        protected readonly IUserRepository _userRepository;
        public SparkExternalIdGrantValidatorcs(ITokenValidator validator, IUserRepository userRepository)
        {
            _validator = validator;
            _userRepository = userRepository;
        }

        public string GrantType => "delegation";

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {

            var userToken = context.Request.Raw.Get("Token");
            var str = context.Request.Raw.Get("Parameters");
            if (string.IsNullOrEmpty(userToken))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
                return;
            }

            var result = _userRepository.FindBySubjectId(userToken);

            if (result == null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
                return;
            }

            // get user's identity
            var sub = result.SubjectId;

            context.Result = new GrantValidationResult(sub, GrantType);
            return;
        }
    }
}
