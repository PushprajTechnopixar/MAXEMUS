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
    }
}
