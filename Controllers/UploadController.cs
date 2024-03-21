using MaxemusAPI.Models.Dtos;
using MaxemusAPI.Models.Helper;
using MaxemusAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static MaxemusAPI.Common.GlobalVariables;
using MaxemusAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using MaxemusAPI.Models;
using MaxemusAPI.Repository;
using MaxemusAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using AutoMapper;
using Amazon.S3.Model;
using Amazon.S3;
using MaxemusAPI.Common;

namespace MaxemusAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        protected APIResponse _response;
        private readonly IEmailManager _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUploadRepository _uploadRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public UploadController(
            UserManager<ApplicationUser> userManager,
            IEmailManager emailSender,
            IMapper mapper,
            IUploadRepository uploadRepository,
            ApplicationDbContext context

        )
        {
            _response = new();
            _emailSender = emailSender;
            _userManager = userManager;
            _uploadRepository = uploadRepository;
            _context = context;
            _mapper = mapper;

        }
        #region UploadProfilePic
        /// <summary>
        ///  Upload profile picture.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        [Route("UploadProfilePic")]
        public async Task<IActionResult> Login([FromForm] UploadProfilePicDto model)
        {
            var currentUserId = HttpContext.User.Claims.First().Value;
            var currentUser = _userManager.FindByIdAsync(currentUserId).GetAwaiter().GetResult();
            if (currentUser == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgUserNotFound;
                return Ok(_response);
            }

            if (!string.IsNullOrEmpty(model.id))
            {
                currentUserId = model.id;
            }

            var userDetail = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == currentUserId);

            // Delete previous file
            if (!string.IsNullOrEmpty(userDetail.ProfilePic))
            {
                var chk = await _uploadRepository.DeleteFilesFromServer("FileToSave/" + userDetail.ProfilePic);
            }
            var documentFile = ContentDispositionHeaderValue.Parse(model.profilePic.ContentDisposition).FileName.Trim('"');
            documentFile = CommonMethod.EnsureCorrectFilename(documentFile);
            documentFile = CommonMethod.RenameFileName(documentFile);

            var documentPath = profilePicContainer + documentFile;
            userDetail.ProfilePic = documentPath;
             _context.ApplicationUsers.Update(userDetail);
            bool uploadStatus = await _uploadRepository.UploadFilesToServer(
                    model.profilePic,
                    profilePicContainer,
                    documentFile
                );


            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "Uploaded successfully.";
            _response.Data = documentPath;
            return Ok(_response);
        }
        #endregion
    }
}
