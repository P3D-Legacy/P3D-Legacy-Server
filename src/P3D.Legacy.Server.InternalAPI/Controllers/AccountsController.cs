using LiteDB;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using P3D.Legacy.Server.Abstractions.Identity;
using P3D.Legacy.Server.UI.Shared.Models;

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.InternalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<ServerIdentity> _userManager;

        public AccountsController(UserManager<ServerIdentity> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RegisterModel model)
        {
            var newUser = new ServerIdentity { Id = ObjectId.NewObjectId(), UserName = model.Email, Email = model.Email };

            var result = await _userManager.CreateAsync(newUser, model.Password);
            //var claimResult = await _userManager.AddClaimAsync(newUser, new Claim("server:gamejoltid", model.GameJoltId));

            if (!result.Succeeded/* || !claimResult.Succeeded*/)
            {
                var errors = result.Errors.Select(x => x.Description)/*.Concat(claimResult.Errors.Select(x => x.Description))*/;

                return Ok(new RegisterResult { Successful = false, Errors = errors });
            }

            return Ok(new RegisterResult { Successful = true });
        }
    }
}