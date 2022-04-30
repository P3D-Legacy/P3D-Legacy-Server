using Microsoft.AspNetCore.Mvc;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models.Users;
using P3D.Legacy.Server.Infrastructure.Services.Users;
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
        private readonly IUserManager _userManager;

        public AccountsController(IUserManager userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] RegisterModel model, CancellationToken ct)
        {
            var newUser = new UserEntity(PlayerId.FromName(model.Username), model.Username);

            var result = await _userManager.CreateAsync(newUser, model.Password, false, ct);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description);

                return Ok(new RegisterResult { Successful = false, Errors = errors });
            }

            return Ok(new RegisterResult { Successful = true });
        }
    }
}