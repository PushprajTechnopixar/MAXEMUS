using System.Net;
using System.Web.Helpers;
using AutoMapper;
using MaxemusAPI.Common;
using MaxemusAPI.Data;
using MaxemusAPI.Dtos;
using MaxemusAPI.Helpers;
using MaxemusAPI.IRepository;
using MaxemusAPI.Models;
using MaxemusAPI.Models.Dtos;
using MaxemusAPI.Models.Helper;
using MaxemusAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using static MaxemusAPI.Common.GlobalVariables;

namespace MaxemusAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IContentRepository _contentRepository;
        private readonly IAccountRepository _userRepo;
        private readonly RoleManager<IdentityRole> _roleManager;
        private string secretKey;
        private readonly IEmailManager _emailSender;
        private ITwilioManager _twilioManager;
        protected APIResponse _response;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AccountController(IAccountRepository userRepo, IWebHostEnvironment hostingEnvironment, ApplicationDbContext context, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, IMapper mapper, IContentRepository contentRepository, RoleManager<IdentityRole> roleManager, IEmailManager emailSender, ITwilioManager twilioManager)
        {
            _userRepo = userRepo;
            _response = new();
            _context = context;
            _mapper = mapper;
            _emailSender = emailSender;
            _twilioManager = twilioManager;
            _userManager = userManager;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _roleManager = roleManager;
            _contentRepository = contentRepository;
            _hostingEnvironment = hostingEnvironment;
        }

        #region Login
        /// <summary>
        ///  Login for SuperAdmin, Admin, Dealer and Distributor.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var loginResponse = await _userRepo.Login(model);
            if (loginResponse.firstName == null || string.IsNullOrEmpty(loginResponse.token))
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Username or password is incorrect.";
                return Ok(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "Login successfully.";
            _response.Data = loginResponse;
            return Ok(_response);
        }
        #endregion

        #region Register
        /// <summary>
        ///  Login for SuperAdmin, Admin, Dealer and Distributor.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterationRequestDTO model)
        {
            bool ifUserNameUnique = _userRepo.IsUniqueUser(model.email, model.phoneNumber);
            if (!ifUserNameUnique)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Email or phone number already exists.";
                return Ok(_response);
            }

            if (Gender.Male.ToString() != model.gender && Gender.Female.ToString() != model.gender && Gender.Other.ToString() != model.gender)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Please enter valid gender.";
                return Ok(_response);
            }

            if (Role.Admin.ToString() != model.role
            && Role.Dealer.ToString() != model.role
            && Role.SuperAdmin.ToString() != model.role
            && Role.Distributor.ToString() != model.role)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Please enter valid role.";
                return Ok(_response);
            }

            if (model.countryId != null && model.stateId != null)
            {
                if (model.countryId > 0)
                {
                    var countryId = await _context.CountryMaster.FindAsync(model.countryId);
                    if (countryId == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "please enter valid countryId.";
                        return Ok(_response);
                    }
                }
                if (model.stateId > 0)
                {
                    var stateId = await _context.StateMaster.FindAsync(model.stateId);
                    if (stateId == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "please enter valid stateId.";
                        return Ok(_response);
                    }
                }
            }

            var user = await _userRepo.Register(model);
            if (user.email == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Error while registering.";
                return Ok(_response);
            }

            var htmlPath = (user.role == Role.Distributor.ToString() ? (distributor_registration) : (dealer_registration));

            // Send email here
            var pathToFile =
                _hostingEnvironment.WebRootPath
                + Path.DirectorySeparatorChar.ToString()
                + mainTemplatesContainer
                + Path.DirectorySeparatorChar.ToString()
                + emailTemplatesContainer
                + Path.DirectorySeparatorChar.ToString()
                + htmlPath;

            var name =
                user.firstName + " " + user.lastName
                ?? string.Empty + " " + user.lastName;
            var body = new BodyBuilder();
            using (StreamReader reader = System.IO.File.OpenText(pathToFile))
            {
                body.HtmlBody = reader.ReadToEnd();
            }
            string messageBody = body.HtmlBody;
            messageBody = messageBody.Replace("{email}", name);
            messageBody = messageBody.Replace("{password}", model.password);
            messageBody = messageBody.Replace("{link}", "NA");
            messageBody = messageBody.Replace("{distributorName}", name);
            messageBody = messageBody.Replace("{DealerName}", name);

            await _emailSender.SendEmailAsync(
                email: user.email,
                subject: "Welcome to Maxemus!",
                htmlMessage: messageBody
            );
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = user;
            _response.Messages = "Registered successfully.";
            return Ok(_response);

        }
        #endregion

        #region EmailOTP
        /// <summary>
        ///  Send OTP on email.
        /// </summary>
        [HttpPost("EmailOTP")]
        public async Task<IActionResult> EmailOTP([FromBody] ForgotPasswordDTO model)
        {
            try
            {
                if (model.isVerify == true)
                {
                    bool isUniqueEmail = _userRepo.IsUniqueEmail(model.email);
                    if (isUniqueEmail == false)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "Email already exists.";
                        return Ok(_response);
                    }

                    int generatedOTP = CommonMethod.GenerateOTP();
                    //Send email here
                    var msg =
                        $"Hi , <br/><br/> Your Maxemus OTP is :- "
                        + generatedOTP
                        + " .<br/><br/>Thanks";
                    await _emailSender.SendEmailAsync(
                        email: model.email,
                        subject: "OTP for Registration Confirmation",
                        htmlMessage: msg
                    );

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = new { OTP = generatedOTP };
                    _response.Messages = ResponseMessages.msgOTPSentOneMailuccess;
                    return Ok(_response);
                }

                var isEmailExists = _userManager.FindByEmailAsync(model.email).GetAwaiter().GetResult();

                if (isEmailExists != null)
                {
                    int generatedOTP = CommonMethod.GenerateOTP();
                    //Send email here
                    var msg =
                        $"Hi , <br/><br/> Your ZigyKart OTP is :- "
                        + generatedOTP
                        + " .<br/><br/>Thanks";
                    await _emailSender.SendEmailAsync(
                        email: model.email,
                        subject: "OTP for Password Change Request",
                        htmlMessage: msg
                    );

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = new { OTP = generatedOTP };
                    _response.Messages = "OTP sent successfully.";
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgNotFound + "user.";
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region ResetPassword
        /// <summary>
        ///  Reset password.
        /// </summary>
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.email))
                {
                    var applicationUser = _userManager.FindByEmailAsync(model.email).GetAwaiter().GetResult();
                    if (applicationUser != null)
                    {
                        var password = Crypto.HashPassword(model.newPassword);
                        applicationUser.PasswordHash = password;

                        await _userManager.UpdateAsync(applicationUser);

                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        _response.Data = new { NewPassword = model.newPassword };
                        _response.Messages = "Password reset successfully.";
                        return Ok(_response);
                    }
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgNotFound + "user.";
                    return Ok(_response);
                }
                else
                {
                    var applicationUser = _context.ApplicationUsers.FirstOrDefault(x => (x.PhoneNumber == model.phoneNumber));
                    if (applicationUser != null)
                    {
                        var password = Crypto.HashPassword(model.newPassword);
                        applicationUser.PasswordHash = password;

                        await _userManager.UpdateAsync(applicationUser);

                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        _response.Data = new { NewPassword = model.newPassword };
                        _response.Messages = "Password reset successfully.";
                        return Ok(_response);
                    }
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgNotFound + "user.";
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region ChangePassword
        /// <summary>
        ///  Change password.
        /// </summary>
        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            try
            {
                string currentUserId = (HttpContext.User.Claims.First().Value);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }
                var user = _userManager.FindByIdAsync(currentUserId).GetAwaiter().GetResult();
                if (user != null)
                {
                    var checkPassword = await _userManager.CheckPasswordAsync(user, model.oldPassword);
                    if (checkPassword != true)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "Your old password was entered incorrectly. Please enter it again.";
                        return Ok(_response);

                    }

                    var password = Crypto.HashPassword(model.newPassword);
                    user.PasswordHash = password;

                    await _userManager.UpdateAsync(user);

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = new { Password = model.newPassword };
                    _response.Messages = "Password changed successfully.";
                    return Ok(_response);
                }
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "user.";
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region SendPhoneOtp
        /// <summary>
        /// Send OTP on phone.
        /// </summary>
        [HttpPost]
        [Route("SendPhoneOtp")]
        [AllowAnonymous]
        public async Task<IActionResult> SendPhoneOtp(PhoneModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgParametersNotCorrect;
                    return Ok(_response);
                }
                string userphoneNumber = model.dialCode + model.phoneNumber;
                bool isUniquePhone = _userRepo.IsUniquePhone(model.phoneNumber);
                if (model.isVerify == true)
                {
                    if (isUniquePhone == false)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "Phone already exists.";
                        return Ok(_response);
                    }

                    var verificationResult = await _twilioManager.StartVerificationAsync(
                    userphoneNumber,
                    GlobalVariables.TwilioChannelTypes.Sms.ToString().ToLower());
                    if (verificationResult.IsValid)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        _response.Data = new { };
                        _response.Messages = ResponseMessages.msgOTPSentOnMobileSuccess;
                        return Ok(_response);
                    }
                    else
                    {
                        if (verificationResult.Errors.FirstOrDefault().ToString() == "")
                        {
                            _response.Messages = verificationResult.Errors.FirstOrDefault().ToString();
                        }
                        else
                        {
                            _response.Messages = verificationResult.Errors.FirstOrDefault().ToString();
                        }
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Data = new { };
                        _response.Messages = verificationResult.Errors.FirstOrDefault().ToString();
                        return Ok(_response);
                    }
                }
                else
                {
                    var userDetail = await _context.ApplicationUsers.Where(u => u.PhoneNumber == model.phoneNumber).FirstOrDefaultAsync();
                    if (userDetail == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "User does not found.";
                        return Ok(_response);
                    }

                    var verificationResult = await _twilioManager.StartVerificationAsync(
                    userphoneNumber,
                    GlobalVariables.TwilioChannelTypes.Sms.ToString().ToLower());
                    if (verificationResult.IsValid)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        _response.Data = new { };
                        _response.Messages = ResponseMessages.msgOTPSentOnMobileSuccess;
                        return Ok(_response);
                    }
                    else
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Data = new { };
                        _response.Messages = verificationResult.Errors.FirstOrDefault().ToString();
                        return Ok(_response);
                    }
                }

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Data = new { };
                _response.Messages = ResponseMessages.msgSomethingWentWrong + ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region VerifyPhone
        /// <summary>
        /// Verify phone OTP.
        /// </summary>
        [HttpPost]
        [Route("VerifyPhone")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyPhone(VerifyPhoneModel model)
        {
            try
            {
                string userphoneNumber = model.dialCode + model.phoneNumber;

                var verificationResult = await _twilioManager.CheckVerificationAsync(
                    userphoneNumber,
                    model.otp
                );
                if (verificationResult.IsValid)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = new { };
                    _response.Messages = ResponseMessages.msgphoneNumberVerifiedSuccess;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Data = new { };
                    _response.Messages = verificationResult.Errors.FirstOrDefault().ToString();
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Data = new { };
                _response.Messages = ResponseMessages.msgSomethingWentWrong + ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region PhoneOTP
        /// <summary>
        ///  Send OTP on phone.
        /// </summary>
        [HttpPost("PhoneOTP")]
        public async Task<IActionResult> PhoneOTP([FromBody] PhoneOTPDTO model)
        {
            try
            {
                string userphoneNumber = model.dialCode + model.phoneNumber;
                bool isUniquePhone = _userRepo.IsUniquePhone(model.phoneNumber);
                if (model.isVerify == true)
                {
                    if (isUniquePhone == false)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "Phone already exists.";
                        return Ok(_response);
                    }

                    int generatedOTP = CommonMethod.GenerateOTP();
                    //Send email here
                    var msg =
                        $"Hi , Your Maxemus OTP is :- "
                        + generatedOTP;
                    await _twilioManager.SendMessage(msg, userphoneNumber);

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = new { OTP = generatedOTP };
                    _response.Messages = "OTP sent successfully.";
                    return Ok(_response);
                }

                if (isUniquePhone == false)
                {
                    int generatedOTP = CommonMethod.GenerateOTP();
                    //Send email here
                    var msg =
                        $"Hi , <br/><br/> Your OTP for password change is :- "
                        + generatedOTP;
                    await _twilioManager.SendMessage(msg, userphoneNumber);

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = new { OTP = generatedOTP };
                    _response.Messages = "OTP sent successfully.";
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgNotFound + "user.";
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return Ok(_response);
            }
        }
        #endregion


    }
}
