using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spark.Register.Model;
using Spark.Register.Repository;

using System.Text.Json;
using System.Text.Json.Serialization;
using RawRabbit;

namespace Spark.Register.Controllers
{
    public class PasswordRegisterModel
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public class ExternalRegisterModel
    {
        public string Name { get; set; }
        public string ExternalName { get; set; }

        public string ExternalId { get; set; }
    }

    public class ExternalIdModel
    {
        public string ExternalId { get; set; }
    }

    [ApiController]
    public class UserRegisterController : ControllerBase
    {
        private readonly ISparkUserRepository _userRepository;
        private readonly IBusClient _client;
        public UserRegisterController(ISparkUserRepository userRepository, IBusClient client)
        {
            _userRepository = userRepository;
            _client = client;
        }

        [HttpPost("api/register/withPassword")]
        public async Task<string> RegisterUserWithPassword([FromBody] PasswordRegisterModel Model)
        {
            string result = "Success";
            SparkUser su = new SparkUser
            {
                Name = Model.Name,
                Password = Model.Password
            };

            Guid userGuid = await _userRepository.RegisterUserWithPassword(su);

            return await Task.FromResult(result);
        }

        [HttpPost("api/register/externalId")]
        public async Task<string> RegisterUserWithExternalId([FromBody] ExternalRegisterModel Model)
        {
            string result = "Success";
            SparkUser su = new SparkUser
            {
                Name = Model.Name,
                
            };

            Guid userGuid = await _userRepository.RegisterUserWithExternalId(su, Model.ExternalName, Model.ExternalId);

            return await Task.FromResult(result);
        }

        [HttpPost("api/register/findByExternalId")]
        public async Task<string> FindByExternalId([FromBody] ExternalIdModel Model)
        {
            SparkUser userGuid = await _userRepository.FindByExternalId(Model.ExternalId);

            //JsonResult d = new JsonResult(userGuid);

            string res = JsonSerializer.ToString(userGuid);

            return await Task.FromResult(res);
        }
    }
}