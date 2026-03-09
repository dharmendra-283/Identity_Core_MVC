using Identity_Core_MVC.Models;
using Identity_Core_MVC.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;

namespace Identity_Core_MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        public IConfiguration Configuration { get; set; }

        public AccountController(SignInManager<User> _signInManager,
                                UserManager<User> _userManager,
                                RoleManager<IdentityRole> _roleManager,
                                IConfiguration _configuration)
        {
            this.signInManager = _signInManager;
            this.userManager = _userManager;
            this.roleManager = _roleManager;
            this.Configuration = _configuration;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await this.signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, "Login not allowed. Check your Email and and confirm Account");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid user name or password");
                }
                return View(model);
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.Email,
                    Email = model.Email
                };
                IdentityResult result = await this.userManager.CreateAsync(user, model.Password!);
                if (result.Succeeded)
                {
                    string emailConfirmationToken = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
                    string url = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = emailConfirmationToken }, Request.Scheme);
                    string activationLink = string.Format("Please confirm your account by <a href='{0}'>clicking here</a>.", HtmlEncoder.Default.Encode(url));
                    this.SendEmail(user.Email, "Email Confirmation", activationLink);

                    //await this.signInManager.SignInAsync(user, false);
                    return RedirectToAction("Login", "Account");
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await this.signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordModel model)
        {
            if (User.Identity!.IsAuthenticated)
            {
                User? loggedInUser = await userManager.GetUserAsync(User);
                IdentityResult result = await this.userManager.ChangePasswordAsync(loggedInUser!, model.CurrentPassword!, model.NewPassword!);
                if (result.Succeeded)
                {
                    await this.signInManager.SignOutAsync();
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Incorrect old Password");
                    return View(model);
                }
            }
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> ForgotPassword()
        {
            if (User.Identity!.IsAuthenticated)
            {
                await this.signInManager.SignOutAsync();
                return RedirectToAction("ForgotPassword", "Account");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPasswordAsync(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                User? user = await userManager.FindByNameAsync(model.Email!);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Email not found in Database");
                    return View(model);
                }
                else
                {
                    IdentityResult result = await this.userManager.RemovePasswordAsync(user);
                    if (result.Succeeded)
                    {
                        result = await this.userManager.AddPasswordAsync(user!, model.NewPassword!);
                        if (result.Succeeded)
                        {
                            return RedirectToAction("Login", "Account");
                        }
                        else
                        {
                            foreach (IdentityError error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(model);
                        }
                    }
                    else
                    {
                        foreach (IdentityError error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                            return View(model);
                        }
                    }
                }
            }
            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User");
                return View();
            }

            User user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                IdentityResult result = await userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error confirming your email.");
                    return View();
                }
            }

            return View();
        }

        private void SendEmail(string emailAddress, string subject, string body)
        {
            string from = this.Configuration.GetValue<string>("Smtp:From");
            string userName = this.Configuration.GetValue<string>("Smtp:Username");
            string password = this.Configuration.GetValue<string>("Smtp:Password");

            using (MailMessage mm = new MailMessage(from, emailAddress))
            {
                mm.Subject = subject;
                mm.Body = body;
                mm.IsBodyHtml = true;
                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = this.Configuration.GetValue<string>("Smtp:Server");
                    smtp.Port = this.Configuration.GetValue<int>("Smtp:Port");
                    smtp.EnableSsl = this.Configuration.GetValue<bool>("Smtp:EnableSsl");
                    smtp.UseDefaultCredentials = this.Configuration.GetValue<bool>("Smtp:DefaultCredentials");
                    smtp.Credentials = new NetworkCredential(userName, password);
                    smtp.Send(mm);
                }
            }
        }

        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoleAsync(RoleModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await this.roleManager
                    .CreateAsync(new IdentityRole
                    {
                        Name = model.RoleName
                    });
                if (result.Succeeded)
                {
                    return RedirectToAction("AllRoles", "Account");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult AllRoles()
        {
            return View(this.roleManager.Roles.ToList());
        }

        public async Task<IActionResult> EditRole(string id)
        {
            IdentityRole? role = await this.roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ModelState.AddModelError(string.Empty, string.Format("Role with Id {0} not found", id));
                return View();
            }
            else
            {
                List<User> users = new List<User>();
                foreach (User user in this.userManager.Users.ToList())
                {
                    if (await this.userManager.IsInRoleAsync(user, role.Name))
                    {
                        users.Add(new User { UserName = user.UserName });
                    }
                }

                RoleModel roleModel = new RoleModel
                {
                    Id = role.Id,
                    RoleName = role.Name,
                    Users = users
                };
                return View(roleModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditRoleAsync(RoleModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole? role = await this.roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    ModelState.AddModelError(string.Empty, string.Format("Role with Id {0} not found", role.Id));
                    return View();
                }
                else
                {
                    role.Name = model.RoleName;
                    IdentityResult result = await this.roleManager.UpdateAsync(role);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("AllRoles", "Account");
                    }
                    else
                    {
                        foreach (IdentityError error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUserInRole(string id)
        {
            IdentityRole role = await this.roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ModelState.AddModelError(string.Empty, string.Format("Role with Id {0} not found", id));
                return View();
            }
            else
            {
                ViewBag.RoleId = id;
                ViewBag.RoleName = role.Name;
                List<UserRoleModel> userRoleModels = new List<UserRoleModel>();
                foreach (User user in this.userManager.Users.ToList())
                {
                    UserRoleModel userRoleModel = new UserRoleModel
                    {
                        UserId = user.Id,
                        UserName = user.UserName
                    };
                    if (await this.userManager.IsInRoleAsync(user, role.Name))
                    {
                        userRoleModel.IsSelected = true;
                    }
                    else
                    {
                        userRoleModel.IsSelected = false;
                    }
                    userRoleModels.Add(userRoleModel);
                }

                return View(userRoleModels);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditUserInRoleAsync(List<UserRoleModel> model, string id)
        {
            IdentityRole role = await this.roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ModelState.AddModelError(string.Empty, string.Format("Role with Id {0} not found", id));
                return View();
            }
            foreach (UserRoleModel userRoleModel in model)
            {
                User user = await this.userManager.FindByIdAsync(userRoleModel.UserId);
                if (userRoleModel.IsSelected && !(await this.userManager.IsInRoleAsync(user, role.Name)))
                {
                    await this.userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!userRoleModel.IsSelected && (await this.userManager.IsInRoleAsync(user, role.Name)))
                {
                    await this.userManager.RemoveFromRoleAsync(user, role.Name);
                }
            }
            return RedirectToAction("EditRole", "Account", new { Id = id });
        }
    }
}