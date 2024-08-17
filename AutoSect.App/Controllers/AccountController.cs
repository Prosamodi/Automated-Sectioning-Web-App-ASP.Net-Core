using AutoSect.App.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using System.Security.Claims;
using DocumentFormat.OpenXml.Wordprocessing;


namespace AutoSect.App.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<IdentityUser> userManager;
        private SignInManager<IdentityUser> signInManager;

        public AccountController(UserManager<IdentityUser> userMgr,
                SignInManager<IdentityUser> signInMgr)
        {
            userManager = userMgr;
            signInManager = signInMgr;
        }

        public ViewResult Login()
        {
            return View(new LoginModel
            {
                Name = string.Empty,
                Password = string.Empty,
                //ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                IdentityUser? user =
                    await userManager.FindByNameAsync(loginModel.Name);
                if (user != null)
                {
                    await signInManager.SignOutAsync();
                    if ((await signInManager.PasswordSignInAsync(user,
                        loginModel.Password, false, false)).Succeeded)
                    {
                        return RedirectToAction("Index", "AppAutoSect", null);
                    }
                }
                ModelState.AddModelError("", "Invalid name or password");
            }
            return View(loginModel);
        }

        [Authorize]
        public async Task<RedirectResult> Logout(string returnUrl = "/")
        {
            await signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }

        public async Task<IActionResult> SignUp()
        {
            return View(new SignUpModel
            {
                Username = string.Empty,
                Password = string.Empty,
                ConfirmPassword = string.Empty
            });
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpModel signupModel)
        {
            if (ModelState.IsValid)
            {
                IdentityUser? user =
                await userManager.FindByNameAsync(signupModel.Username);
                if (user == null)
                {
                    user = new IdentityUser(signupModel.Username);
                    user.Email = signupModel.Email;
                    //user.PhoneNumber = "555-1234";
                    await userManager.CreateAsync(user, signupModel.ConfirmPassword);
                    await userManager.AddToRoleAsync(user, "User");

                    IdentityUser? registeredUser =
                    await userManager.FindByNameAsync(signupModel.Username);
                    await signInManager.SignOutAsync();
                    if(registeredUser != null)
                    {
                        if ((await signInManager.PasswordSignInAsync(registeredUser,
                        signupModel.ConfirmPassword, false, false)).Succeeded)
                        {
                            return RedirectToAction("Index", "AppAutoSect", null);
                        }
                    }
                    

                }
                ModelState.AddModelError("", "Invalid name or password");
            }
            return View(signupModel);
        }

        public IActionResult GoogleSignIn()
        {
            var redirectUrl = Url.Action("GoogleSignInCallback", "Account");

            var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);

            return new ChallengeResult("Google", properties);

            //await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
            //    new AuthenticationProperties
            //    {
            //        RedirectUri = Url.Action("GoogleSignInURI")
            //    });
        }

        public async Task<IActionResult> GoogleSignInCallback(string remoteError = "")
        {
            if(!string.IsNullOrEmpty(remoteError))
            {
                ModelState.AddModelError("", $"Error from external login provide: {remoteError}");
                return RedirectToAction("Index", "Home");
            }

            //Get Login Info
            var info = await signInManager.GetExternalLoginInfoAsync();

            if(info  == null)
            {
                ModelState.AddModelError("", $"Error from external login provide: {remoteError}");
                return RedirectToAction("Index", "Home");
            }

            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return RedirectToAction("Index", "AppAutoSect");
            }
            else
            {
                var userEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var user = await userManager.FindByEmailAsync(userEmail);

                    if(user == null)
                    {
                        user = new IdentityUser()
                        {
                            UserName = userEmail,
                            Email = userEmail,
                            EmailConfirmed = true
                        };

                        await userManager.CreateAsync(user);
                        await userManager.AddToRoleAsync(user, "User");
                    }

                    await signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "AppAutoSect");
                }
            }


            return RedirectToAction("Index", "Home");

            
        }

    }
}
