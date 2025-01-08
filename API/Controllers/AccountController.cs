using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Api.Dtos;
using API.Dtos;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestSharp;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    //api/account
    public class AccountController: ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration
        )
        {
            _configuration=configuration;
            _roleManager=roleManager;
            _userManager=userManager;
        }

        //api/account/register
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(RegisterDto registerDto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user=new AppUser{
                Email=registerDto.Email,
                FullName=registerDto.FullName,
                UserName=registerDto.Email
            };

            var result=await _userManager.CreateAsync(user,registerDto.Password);
            if(!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            
            if(registerDto.Roles is null){
                await _userManager.AddToRoleAsync(user,"User");
            }else{
                foreach(var role in registerDto.Roles)
                {
                    await _userManager.AddToRoleAsync(user,role);
                }
            }

            return Ok(new AuthResponseDto{
                IsSuccess=true,
                Message="Account Created Successfully!"
            });
        }
        //api/account/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if(user is null)
            {
                return Unauthorized(new AuthResponseDto{
                    IsSuccess=false,
                    Message="User not found with this Email"
                });
            }
            var result=await _userManager.CheckPasswordAsync(user,loginDto.Password);
            if(!result){
                return Unauthorized(new AuthResponseDto{
                    IsSuccess=false,
                    Message="Invalid Password."
                });
            }

            var token = GenerateToken(user);

            return Ok(new AuthResponseDto{
                Token=token,
                IsSuccess=true,
                Message="Login Success."
            });
        }
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var user=await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if(user is null)
            {
                return Ok(new AuthResponseDto
                {
                    IsSuccess= false,
                    Message="User does not exist with this email"
                });
            }
            var token=await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink=$"http://localhost:4200/reset-password?email={user.Email}&token={WebUtility.UrlEncode(token)}";

            /*using RestSharp;

            var client = new RestClient("https://send.api.mailtrap.io/api/send");
            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer 7113e37136d2c973e9672a6bba58941e");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\"from\":{\"email\":\"hello@demomailtrap.com\",\"name\":\"Mailtrap Test\"},\"to\":[{\"email\":\"nawsish2018@gmail.com\"}],\"template_uuid\":\"f1cf3d2e-3cf5-41f7-b166-e0f6146a17a9\",\"template_variables\":{\"username\":\"Test_Username\",\"user_email\":\"Test_User_email\",\"reset_link\":\"Test_Reset_link\",\"year\":\"Test_Year\"}}", ParameterType.RequestBody);
            var response = client.Post(request);
            System.Console.WriteLine(response.Content);*/
            var client = new RestClient("https://send.api.mailtrap.io/api/send");
            var request = new RestRequest
            {
                Method=Method.Post,
                RequestFormat=DataFormat.Json
            };

            request.AddHeader("Authorization", "Bearer 7113e37136d2c973e9672a6bba58941e");
            request.AddJsonBody(new {
                from=new {email="mailtrap@demomailtrap.com"},
                to=new[]{new {email=user.Email}},
                template_uuid="f1cf3d2e-3cf5-41f7-b166-e0f6146a17a9",
                template_variables=new {user_email=user.Email, reset_link=resetLink}
            });
            var response= client.Execute(request);
            if(response.IsSuccessful){
                return Ok(new AuthResponseDto{
                    IsSuccess=true,
                    Message="Email sent with password reset link. Please check your email."
                });
            }else{
                return BadRequest(new AuthResponseDto{
                    IsSuccess= false,
                    Message=response.Content!.ToString()
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasword(ResetPaswordDto resetPaswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPaswordDto.Email);
            //resetPaswordDto.Token=WebUtility.UrlDecode(resetPaswordDto.Token);
            if(user is null){
                return BadRequest(new AuthResponseDto{
                    IsSuccess=false,
                    Message= "User does not exist with this email"
                });
            }
            var result = await _userManager.ResetPasswordAsync(user, resetPaswordDto.Token, resetPaswordDto.NewPassword);

            if(result.Succeeded)
            {
                return Ok(new AuthResponseDto{
                    IsSuccess = true,
                    Message="Password reset Successfully"
                });
            }
            return BadRequest(new AuthResponseDto{
                IsSuccess=false,
                Message= result.Errors.FirstOrDefault()!.Description
            });
        }

        private string GenerateToken(AppUser user){
            var tokenHandler=new JwtSecurityTokenHandler();
            var key=Encoding.ASCII
            .GetBytes(_configuration.GetSection("JWTSetting").GetSection("securityKey").Value!);

            var roles=_userManager.GetRolesAsync(user).Result;

            List<Claim> claims=
            [
                new (JwtRegisteredClaimNames.Email,user.Email ?? ""),
                new (JwtRegisteredClaimNames.Name,user.FullName ?? ""),
                new (JwtRegisteredClaimNames.NameId,user.Id ?? ""),
                new (JwtRegisteredClaimNames.Aud,
                _configuration.GetSection("JWTSetting").GetSection("ValidAudience").Value!),
                new (JwtRegisteredClaimNames.Iss, _configuration.GetSection("JWTSetting").GetSection("validIssuer").Value!)
            ];

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role,role));
            }
            var tokenDescriptor=new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials=new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256
                )
            };

            var token=tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        //api/account/detail
        [Authorize]
        [HttpGet("detail")]
        public async Task<ActionResult<UserDetailDto>> GetUserDetail()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId!);


            if(user is null)
            {
                return NotFound(new AuthResponseDto{
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            return Ok(new UserDetailDto{
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = [..await _userManager.GetRolesAsync(user)],
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                AccessFailedCount = user.AccessFailedCount,

            });

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsers()
        {
            var users = await _userManager.Users.Select(u=> new UserDetailDto{
                Id = u.Id,
                Email=u.Email,
                FullName=u.FullName,
                Roles=_userManager.GetRolesAsync(u).Result.ToArray()
            }).ToListAsync();

            return Ok(users);
        }
    }
}