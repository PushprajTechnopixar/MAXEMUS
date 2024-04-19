﻿using AutoMapper;
using MaxemusAPI.Data;
using MaxemusAPI.Firebase;
using MaxemusAPI.Models.Helper;
using MaxemusAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AutoMapper.QueryableExtensions;
using MaxemusAPI.Common;
using MaxemusAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static MaxemusAPI.Common.GlobalVariables;
using System.Net;

namespace MaxemusAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        protected APIResponse _response;
        private readonly IMobileMessagingClient _mobileMessagingClient;
        public NotificationController(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration config, IMapper mapper, IMobileMessagingClient mobileMessagingClient)
        {
            _context = context;
            _config = config;
            _mapper = mapper;
            _mobileMessagingClient = mobileMessagingClient;
            _userManager = userManager;
            _response = new();
            _roleManager = roleManager;
        }

        #region getBroadcastNotificationList
        /// <summary>
        /// get notification list for admin.
        /// </summary>        
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Admin,Distributor")]
        [HttpGet]
        [Route("getBroadcastNotificationList")]
        public async Task<IActionResult> getBroadcastNotificationList([FromQuery] int pageNumber, int pageSize, string? searchByRole, string? searchQuery)
        {
            try
            {
                var currentUserId = HttpContext.User.Claims.First().Value;
                var currentUserDetail = _userManager.FindByIdAsync(currentUserId).GetAwaiter().GetResult();
                if (currentUserDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }

                var getNotification = await _context.Notification.Where(u => u.CreatedBy == currentUserId)
                    .ProjectTo<NotificationDTO>(_mapper.ConfigurationProvider)
                    .OrderByDescending(i => i.CreateDate)
                    .ToListAsync();

                if (!string.IsNullOrEmpty(searchByRole))
                {
                    getNotification = getNotification.Where(x => (x.userRole?.IndexOf(searchByRole, StringComparison.OrdinalIgnoreCase) >= 0)
                   ).ToList();
                }
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    getNotification = getNotification.Where(x => (x.title?.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (x.description?.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                   ).ToList();
                }

                FilterationListDTO model = new FilterationListDTO();
                model.pageNumber = (pageNumber <= 0) ? 1 : pageNumber;
                model.pageSize = (pageSize <= 0) ? 10 : pageSize;

                // Get's No of Rows Count
                int count = getNotification.Count();

                // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1
                int CurrentPage = model.pageNumber;

                // Parameter is passed from Query string if it is null then it default Value will be pageSize:20
                int PageSize = model.pageSize;

                // Display TotalCount to Records to User
                int TotalCount = count;

                // Calculating Totalpage by Dividing (No of Records / Pagesize)
                int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                // Returns List of Customer after applying Paging
                var items = getNotification
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                // if CurrentPage is greater than 1 means it has previousPage
                var previousPage = CurrentPage > 1 ? "Yes" : "No";

                // if TotalPages is greater than CurrentPage means it has nextPage
                var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

                // Returing List of Customers Collections
                FilterationResponseModel<NotificationDTO> obj1 =
                    new FilterationResponseModel<NotificationDTO>();
                obj1.totalCount = TotalCount;
                obj1.pageSize = PageSize;
                obj1.currentPage = CurrentPage;
                obj1.totalPages = TotalPages;
                obj1.previousPage = previousPage;
                obj1.nextPage = nextPage;
                obj1.searchQuery = string.IsNullOrEmpty(model.searchQuery) ? "no parameter passed" : model.searchQuery;
                obj1.dataList = items.ToList();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = obj1;
                _response.Messages = "Notification list shown successfully.";
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

        #region broadcastNotification
        /// <summary>
        /// Broadcast notification.
        /// </summary>
        [HttpPost]
        [Route("broadcastNotification")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Admin,Distributor")]
        public async Task<IActionResult> broadcastNotification([FromBody] AddNotificationDTO model)
        {
            try
            {
                var currentUserId = HttpContext.User.Claims.First().Value;
                var currentUserDetail = _userManager.FindByIdAsync(currentUserId).GetAwaiter().GetResult();
                if (currentUserDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }

                var roles = await _userManager.GetRolesAsync(currentUserDetail);
                int distributorId = 0;

                if (roles.FirstOrDefault().ToString() == Role.Distributor.ToString())
                {
                    if (model.sendToRole != Role.Dealer.ToString())
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "Please enter valid role.";
                        return Ok(_response);
                    }
                    model.sendToRole = Role.Dealer.ToString();
                    distributorId = await _context.DistributorDetail.Where(u => u.UserId == currentUserId).Select(u => u.DistributorId).FirstOrDefaultAsync();
                }

                if (model.sendToRole != Role.Admin.ToString()
                && model.sendToRole != Role.Dealer.ToString()
                && model.sendToRole != Role.Distributor.ToString())
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Please enter valid role.";
                    return Ok(_response);
                }

                var addNotification = new Notification();
                addNotification.Title = model.title;
                addNotification.Description = model.description;
                addNotification.UserRole = model.sendToRole;
                addNotification.CreatedBy = currentUserId;
                addNotification.NotificationType = NotificationType.Broadcast.ToString();

                await _context.AddAsync(addNotification);
                await _context.SaveChangesAsync();

                var mapData = _mapper.Map<NotificationDTO>(addNotification);

                // send notification
                var allUsers = await _userManager.GetUsersInRoleAsync(model.sendToRole);

                if (allUsers.Count > 0)
                {
                    foreach (var user in allUsers)
                    {
                        var token = "";
                        var userDetail = await _context.ApplicationUsers.Where(u => u.Id == user.Id).FirstOrDefaultAsync();
                        if (userDetail != null)
                        {
                            if (roles.FirstOrDefault().ToString() == Role.Distributor.ToString())
                            {
                                var customerShop = await _context.DealerProduct.Where(u => u.DistributorId == distributorId).FirstOrDefaultAsync();
                                if (customerShop == null)
                                {
                                    continue;
                                }
                            }
                            if (!string.IsNullOrEmpty(userDetail.Fcmtoken))
                            {
                                // if (user.IsNotificationEnabled == true)
                                // {
                                token = userDetail.Fcmtoken;
                                var resp = await _mobileMessagingClient.SendNotificationAsync(token, addNotification.Title, addNotification.Description);
                                if (!string.IsNullOrEmpty(resp))
                                {
                                    // update notification sent
                                    var notificationSent = new NotificationSent();
                                    notificationSent.Title = addNotification.Title;
                                    notificationSent.Description = addNotification.Description;
                                    notificationSent.UserId = user.Id;
                                    notificationSent.NotificationType = NotificationType.Broadcast.ToString();

                                    await _context.AddAsync(notificationSent);
                                    await _context.SaveChangesAsync();
                                }
                                // }
                            }
                        }
                    }
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Notification sent sucessfullly.";
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

        #region deleteBroadcastNotification
        /// <summary>
        /// Delete broadcast notification.
        /// </summary>
        [HttpDelete]
        [Route("deleteBroadcastNotification")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> deleteBroadcastNotification([FromQuery] int? notificationId)
        {
            try
            {
                var currentUserId = HttpContext.User.Claims.First().Value;
                var currentUserDetail = _userManager.FindByIdAsync(currentUserId).GetAwaiter().GetResult();
                if (currentUserDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }
                var roles = await _userManager.GetRolesAsync(currentUserDetail);

                if (notificationId > 0)
                {
                    var notification = await _context.Notification
                        .FindAsync(notificationId);

                    if (notification != null)
                    {
                        _context.Remove(notification);
                        await _context.SaveChangesAsync();

                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        _response.Messages = "Notification deleted successfully.";
                        return Ok(_response);
                    }
                    else
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Messages = "Notification id must be greater than zero.";
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

        #region getNotificationList
        /// <summary>
        /// Get notification list.
        /// </summary>
        [HttpGet]
        [Route("getNotificationList")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> getNotificationList([FromQuery] int pageNumber, int pageSize, string? searchQuery)
        {
            try
            {
                var currentUserId = HttpContext.User.Claims.First().Value;
                var currentUserDetail = _userManager.FindByIdAsync(currentUserId).GetAwaiter().GetResult();
                if (currentUserDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }
                var roles = await _userManager.GetRolesAsync(currentUserDetail);

                var getNotificationSent = await _context.NotificationSent
                    .Where(u => u.UserId == currentUserId)
                    .OrderByDescending(i => i.CreateDate)
                    .ToListAsync();

                var response = new List<NotificationSentDTO>();

                foreach (var item in getNotificationSent)
                {
                    var notificationDetail = new NotificationSentDTO();
                    notificationDetail.isNotificationRead = item.IsNotificationRead;
                    notificationDetail.description = item.Description;
                    notificationDetail.title = item.Title;
                    notificationDetail.notificationSentId = item.NotificationSentId;
                    notificationDetail.userId = item.UserId;
                    notificationDetail.notificationType = item.NotificationType;
                    notificationDetail.createDate = (Convert.ToDateTime(item.CreateDate).ToString(@"yyyy-MM-dd"));

                    response.Add(notificationDetail);
                }

                //if (!string.IsNullOrEmpty(searchQuery))
                //{
                //    response = response.Where(a => (a.title.ToLower() == searchQuery.ToLower())
                //    || (a.description.ToLower() == searchQuery.ToLower())
                //    ).ToList();
                //}

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    searchQuery = searchQuery.TrimEnd(); // Trim trailing spaces from the searchQuery
                    response = response.Where(a => (a.title.ToLower() == searchQuery.ToLower().TrimEnd())
                                                 || (a.description.ToLower() == searchQuery.ToLower().TrimEnd())
                                            ).ToList();
                }



                FilterationListDTO model = new FilterationListDTO();
                model.pageNumber = (pageNumber <= 0) ? 1 : pageNumber;
                model.pageSize = (pageSize <= 0) ? 10 : pageSize;

                // Get's No of Rows Count
                int count = response.Count();

                // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1
                int CurrentPage = model.pageNumber;

                // Parameter is passed from Query string if it is null then it default Value will be pageSize:20
                int PageSize = model.pageSize;

                // Display TotalCount to Records to User
                int TotalCount = count;

                // Calculating Totalpage by Dividing (No of Records / Pagesize)
                int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                // Returns List of Customer after applying Paging
                var items = response
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                // if CurrentPage is greater than 1 means it has previousPage
                var previousPage = CurrentPage > 1 ? "Yes" : "No";

                // if TotalPages is greater than CurrentPage means it has nextPage
                var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

                // Returing List of Customers Collections
                FilterationResponseModel<NotificationSentDTO> obj1 =
                    new FilterationResponseModel<NotificationSentDTO>();
                obj1.totalCount = TotalCount;
                obj1.pageSize = PageSize;
                obj1.currentPage = CurrentPage;
                obj1.totalPages = TotalPages;
                obj1.previousPage = previousPage;
                obj1.nextPage = nextPage;
                obj1.searchQuery = string.IsNullOrEmpty(model.searchQuery) ? "no parameter passed" : model.searchQuery;
                obj1.dataList = items.ToList();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = obj1;
                _response.Messages = "Notification list shown successfully.";
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

        #region readNotification
        /// <summary>
        /// Read notification.
        /// </summary>
        [HttpGet]
        [Route("readNotification")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> readNotification(int? notificationSentId)
        {
            try
            {
                var currentUserId = HttpContext.User.Claims.First().Value;
                var currentUserDetail = _userManager.FindByIdAsync(currentUserId).GetAwaiter().GetResult();
                if (currentUserDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }

                var roles = await _userManager.GetRolesAsync(currentUserDetail);
                if (notificationSentId > 0)
                {
                    var getNotificationSent = await _context.NotificationSent
                        .Where(i => i.NotificationSentId == notificationSentId)
                        .FirstOrDefaultAsync();

                    if (getNotificationSent == null)
                    {
                        return Ok(new
                        {
                            status = false,
                            message = ResponseMessages.msgNotFound + "record.",
                            code = StatusCodes.Status200OK,
                        });
                    }

                    getNotificationSent.IsNotificationRead = true;

                    _context.Update(getNotificationSent);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    List<NotificationSent> getNotificationSent;

                    getNotificationSent = await _context.NotificationSent
                       .Where(i => (i.UserId == currentUserId))
                       .ToListAsync();

                    if (getNotificationSent.Count > 0)
                    {
                        foreach (var a in getNotificationSent)
                        {
                            a.IsNotificationRead = true;

                            _context.Update(a);
                            await _context.SaveChangesAsync();
                        }
                    }

                    getNotificationSent = await _context.NotificationSent
                       .Where(i => (i.UserId == currentUserId))
                       .ToListAsync();

                    if (getNotificationSent.Count > 0)
                    {
                        foreach (var a in getNotificationSent)
                        {
                            a.IsNotificationRead = true;

                            _context.Update(a);
                            await _context.SaveChangesAsync();
                        }
                    }

                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                // _response.Data = obj1;
                _response.Messages = "Notification status updated successfully.";
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

        #region deleteNotification
        /// <summary>
        /// Delete notification.
        /// </summary>
        [HttpDelete]
        [Route("deleteNotification")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> deleteNotification([FromQuery] int? notificationSentId)
        {
            try
            {
                var currentUserId = HttpContext.User.Claims.First().Value;
                var currentUserDetail = _userManager.FindByIdAsync(currentUserId).GetAwaiter().GetResult();
                if (currentUserDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }
                var roles = await _userManager.GetRolesAsync(currentUserDetail);

                if (notificationSentId > 0)
                {
                    var notification = await _context.NotificationSent
                        .FindAsync(notificationSentId);

                    if (notification != null)
                    {
                        _context.Remove(notification);
                        await _context.SaveChangesAsync();

                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        _response.Messages = "Notification deleted successfully.";
                        return Ok(_response);
                    }
                    else
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }
                }
                else
                {
                    List<NotificationSent> notification;

                    notification = await _context.NotificationSent.Where(a => (a.UserId == currentUserId)).ToListAsync(); ;
                    foreach (var item in notification)
                    {
                        _context.Remove(item);
                        await _context.SaveChangesAsync();
                    }

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Messages = "Notification deleted successfully.";
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

        #region updateFCMToken
        /// <summary>
        /// Update FCM token.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("updateFCMToken")]
        public async Task<IActionResult> updateFCMToken(FCMTokenDTO model)
        {
            try
            {
                var currentUserId = HttpContext.User.Claims.First().Value;
                var currentUserDetail = _userManager.FindByIdAsync(currentUserId).GetAwaiter().GetResult();
                if (currentUserDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }
                var roles = await _userManager.GetRolesAsync(currentUserDetail);

                var user = await _context.ApplicationUsers.Where(a => a.Id == currentUserId).FirstOrDefaultAsync();

                if (user != null)
                {
                    user.Fcmtoken = model.fcmToken;

                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = model;
                    _response.Messages = "Token updated successfully.";
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgNotFound + "record.";
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

        #region getNotificationCount
        /// <summary>
        /// Get notification list.
        /// </summary>
        [HttpGet]
        [Route("getNotificationCount")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> getNotificationCount()
        {
            try
            {
                var currentUserId = HttpContext.User.Claims.First().Value;
                var currentUserDetail = _userManager.FindByIdAsync(currentUserId).GetAwaiter().GetResult();
                if (currentUserDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }
                // var roles = await _userManager.GetRolesAsync(currentUserDetail);

                var getNotificationSent = await _context.NotificationSent
                    .Where(u => u.UserId == currentUserId && u.IsNotificationRead != true)
                    .ToListAsync();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = new { notificationCount = getNotificationSent.Count };
                _response.Messages = "Notification count shown successfully.";
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
    }
}
