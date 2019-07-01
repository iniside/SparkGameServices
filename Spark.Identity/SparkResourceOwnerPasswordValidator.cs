using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Validation;

namespace Spark.Identity
{
    public class SparkResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        //private readonly IUserRepository _userRepository;
        //
        //public SparkResourceOwnerPasswordValidator(IUserRepository userRepository)
        //{
        //    _userRepository = userRepository;
        //}

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            //if (_userRepository.ValidateCredentials(context.UserName, context.Password))
            //{
            //    var user = _userRepository.FindByUsername(context.UserName);
            //    context.Result = new GrantValidationResult(user.SubjectId, OidcConstants.AuthenticationMethods.Password);
            //}

            return Task.FromResult(0);
        }
    }
}
