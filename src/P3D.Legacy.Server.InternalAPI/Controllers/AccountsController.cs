﻿using Microsoft.AspNetCore.Mvc;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models.Users;
using P3D.Legacy.Server.Infrastructure.Repositories.Users;
using P3D.Legacy.Server.UI.Shared.Models;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.InternalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public AccountsController(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] RegisterModel model, CancellationToken ct)
        {
            var newUser = new UserEntity(PlayerId.FromName(model.Username), model.Username);

            var result = await _userRepository.CreateAsync(newUser, model.Password, false, ct);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(static x => x.Description);

                return Ok(new RegisterResult { Successful = false, Errors = errors });
            }

            return Ok(new RegisterResult { Successful = true });
        }
    }
}