﻿using MaxemusAPI.Models.Dtos;
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
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
            _context.SaveChangesAsync();
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

        #region UploadBrandImage
        /// <summary>
        ///  Upload brand image.
        /// </summary>
        [HttpPost("UploadBrandImage")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> UploadBrandImage([FromForm] UploadBrandImageDTO model)
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

                var brandDetail = await _context.Brand.FirstOrDefaultAsync(u => u.BrandId == model.brandId);
                if (brandDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Record not found.";
                    return Ok(_response);
                }

                // Delete previous file
                if (!string.IsNullOrEmpty(brandDetail.BrandImage))
                {
                    var chk = await _uploadRepository.DeleteFilesFromServer("FileToSave/" + brandDetail.BrandImage);
                }
                var documentFile = ContentDispositionHeaderValue.Parse(model.brandImage.ContentDisposition).FileName.Trim('"');
                documentFile = CommonMethod.EnsureCorrectFilename(documentFile);
                documentFile = CommonMethod.RenameFileName(documentFile);

                var documentPath = brandImageContainer + documentFile;
                bool uploadStatus = await _uploadRepository.UploadFilesToServer(
                        model.brandImage,
                        brandImageContainer,
                        documentFile
                    );
                brandDetail.BrandImage = documentPath;
                _context.Brand.Update(brandDetail);
                await _context.SaveChangesAsync();
                var brandResponse = _mapper.Map<BrandDTO>(brandDetail);

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = brandResponse;
                _response.Messages = "Brand image uploaded successfully.";
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

        #region UploadCategoryImage
        /// <summary>
        ///  Upload Category image.
        /// </summary>
        [HttpPost("UploadCategoryImage")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> UploadCategoryImage([FromForm] UploadCategoryImageDTO model)
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

                if (model.mainCategoryId > 0 && model.subCategoryId > 0)
                {
                    var subCategory = await _context.SubCategory.Where(u => u.MainCategoryId == model.mainCategoryId && u.SubCategoryId == model.subCategoryId).FirstOrDefaultAsync();
                    if (subCategory == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "sub Category not found.";
                        return Ok(_response);
                    }

                    if (!string.IsNullOrEmpty(subCategory.SubCategoryImage))
                    {
                        var chk = await _uploadRepository.DeleteFilesFromServer("FileToSave/" + subCategory.SubCategoryImage);
                    }

                    var documentFile = ContentDispositionHeaderValue.Parse(model.categoryImage.ContentDisposition).FileName.Trim('"');
                    documentFile = CommonMethod.EnsureCorrectFilename(documentFile);
                    documentFile = CommonMethod.RenameFileName(documentFile);

                    var documentPath = subCategoryImageContainer + documentFile;
                    bool uploadStatus = await _uploadRepository.UploadFilesToServer(
                            model.categoryImage,
                            subCategoryImageContainer,
                            documentFile
                        );
                    subCategory.SubCategoryImage = documentPath;
                    _context.SubCategory.Update(subCategory);
                    await _context.SaveChangesAsync();

                    var subResponse = _mapper.Map<CategoryImageDTO>(subCategory);

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = subResponse;
                    _response.Messages = "sub category image uploaded successfully.";

                    return Ok(_response);
                }
                else if (model.mainCategoryId > 0)
                {
                    var mainCategory = await _context.MainCategory.Where(u => u.MainCategoryId == model.mainCategoryId).FirstOrDefaultAsync();
                    if (mainCategory == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "main Category not found.";
                        return Ok(_response);
                    }

                    if (!string.IsNullOrEmpty(mainCategory.MainCategoryImage))
                    {
                        var chk = await _uploadRepository.DeleteFilesFromServer("FileToSave/" + mainCategory.MainCategoryImage);
                    }

                    var documentFile = ContentDispositionHeaderValue.Parse(model.categoryImage.ContentDisposition).FileName.Trim('"');
                    documentFile = CommonMethod.EnsureCorrectFilename(documentFile);
                    documentFile = CommonMethod.RenameFileName(documentFile);

                    var documentPath = mainCategoryImageContainer + documentFile;
                    bool uploadStatus = await _uploadRepository.UploadFilesToServer(
                            model.categoryImage,
                            mainCategoryImageContainer,
                            documentFile
                        );
                    mainCategory.MainCategoryImage = documentPath;
                    _context.MainCategory.Update(mainCategory);
                    await _context.SaveChangesAsync();

                    var mainResponse = _mapper.Map<CategoryImageDTO>(mainCategory);

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = mainResponse;
                    _response.Messages = "main category image uploaded successfully.";

                    return Ok(_response);
                }

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

        #region UploadProductImage
        /// <summary>
        ///  Upload product image.
        /// </summary>
        [HttpPost("UploadProductImage")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> UploadProductImage([FromForm] UploadProductImageDTO model)
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

                if (model.ProductImage == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Please select at least one image.";
                    return Ok(_response);
                }

                if (model.ProductImage.Count > 5)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Can't upload more than five images.";
                    return Ok(_response);
                }

                var productDetail = await _context.Product.Where(u => (u.ProductId == model.ProductId)).FirstOrDefaultAsync();
                if (productDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Record not found.";
                    return Ok(_response);
                }

                // Delete previous file
                if (!string.IsNullOrEmpty(productDetail.Image1))
                {
                    var chk = await _uploadRepository.DeleteFilesFromServer("FileToSave/" + productDetail.Image1);
                }
                if (!string.IsNullOrEmpty(productDetail.Image2))
                {
                    var chk = await _uploadRepository.DeleteFilesFromServer("FileToSave/" + productDetail.Image2);
                }
                if (!string.IsNullOrEmpty(productDetail.Image3))
                {
                    var chk = await _uploadRepository.DeleteFilesFromServer("FileToSave/" + productDetail.Image3);
                }
                if (!string.IsNullOrEmpty(productDetail.Image4))
                {
                    var chk = await _uploadRepository.DeleteFilesFromServer("FileToSave/" + productDetail.Image4);
                }
                if (!string.IsNullOrEmpty(productDetail.Image5))
                {
                    var chk = await _uploadRepository.DeleteFilesFromServer("FileToSave/" + productDetail.Image5);
                }

                // Delete document path
                productDetail.Image1 = string.Empty;
                productDetail.Image2 = string.Empty;
                productDetail.Image3 = string.Empty;
                productDetail.Image4 = string.Empty;
                productDetail.Image5 = string.Empty;

                _context.Update(productDetail);
                await _context.SaveChangesAsync();

                int imageNo = 1;

                foreach (var item in model.ProductImage)
                {
                    var documentFile = ContentDispositionHeaderValue.Parse(item.ContentDisposition).FileName.Trim('"');
                    documentFile = CommonMethod.EnsureCorrectFilename(documentFile);
                    documentFile = CommonMethod.RenameFileName(documentFile);

                    var documentPath = productImageContainer + documentFile;
                    bool uploadStatus = await _uploadRepository.UploadFilesToServer(
                            item,
                            productImageContainer,
                            documentFile
                        );
                    if (imageNo == 1)
                    {
                        productDetail.Image1 = documentPath;
                        imageNo++;
                    }
                    else if (imageNo == 2)
                    {
                        productDetail.Image2 = documentPath;
                        imageNo++;
                    }
                    else if (imageNo == 3)
                    {
                        productDetail.Image3 = documentPath;
                        imageNo++;
                    }
                    else if (imageNo == 4)
                    {
                        productDetail.Image4 = documentPath;
                        imageNo++;
                    }
                    else
                    {
                        productDetail.Image5 = documentPath;
                        imageNo++;
                    }
                }

                _context.Update(productDetail);
                await _context.SaveChangesAsync();

                var getProduct = await _context.Product.FirstOrDefaultAsync(u => u.ProductId == productDetail.ProductId);
                var response = _mapper.Map<ProductResponselistDTO>(getProduct);

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = response;
                _response.Messages = "Product image uploaded successfully.";
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

        #region UploadDistributorDetailImage
        /// <summary>
        ///  Upload DistributorDetail Image.
        /// </summary>
        [HttpPost("UploadDistributorDetailImage")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> UploadDistributorDetailImage([FromForm] UploadDistributorDetailImageDTO model)
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

                var distributorDetail = await _context.DistributorDetail.FirstOrDefaultAsync(u => u.DistributorId == model.DistributorId);
                if (distributorDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Record not found.";
                    return Ok(_response);
                }

                // Delete previous file
                if (!string.IsNullOrEmpty(distributorDetail.Image))
                {
                    var chk = await _uploadRepository.DeleteFilesFromServer("FileToSave/" + distributorDetail.Image);
                }
                var documentFile = ContentDispositionHeaderValue.Parse(model.Image.ContentDisposition).FileName.Trim('"');
                documentFile = CommonMethod.EnsureCorrectFilename(documentFile);
                documentFile = CommonMethod.RenameFileName(documentFile);

                var documentPath = brandImageContainer + documentFile;
                bool uploadStatus = await _uploadRepository.UploadFilesToServer(
                        model.Image,
                        brandImageContainer,
                        documentFile
                    );
                distributorDetail.Image = documentPath;
                _context.DistributorDetail.Update(distributorDetail);
                await _context.SaveChangesAsync();
                var response = _mapper.Map<BrandDTO>(distributorDetail);

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = response;
                _response.Messages = "distributorDetail image uploaded successfully.";
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

        #region InstallationDocument
        /// <summary>
        ///  Upload InstallationDocument link.
        /// </summary>
        [HttpPost("UploadInstallationDocument")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> UploadInstallationDocument([FromForm] UploadInstallationDocumentFileDto model)
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
                var camera = await _context.CameraVariants.FirstOrDefaultAsync(u => u.ProductId == model.ProductId && u.VariantId == model.VariantId);
                if (camera == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Record not found.";
                    return Ok(_response);
                }
                var installationDocument = await _context.InstallationDocumentVariants.FirstOrDefaultAsync();

                string pdflink64 = Convert.ToBase64String(installationDocument?.PdfLink);
                if (!string.IsNullOrEmpty(pdflink64))
                {
                    var chk = await _uploadRepository.DeleteFilesFromServer("FileToSave/" + installationDocument?.PdfLink);
                }


                var documentFile = ContentDispositionHeaderValue.Parse(model.PdfLink.ContentDisposition).FileName.Trim('"');
                documentFile = CommonMethod.EnsureCorrectFilename(documentFile);
                documentFile = CommonMethod.RenameFileName(documentFile);

                var documentPath = installationDocumentContainer + documentFile;
                bool uploadStatus = await _uploadRepository.UploadFilesToServer(
                        model.PdfLink,
                        installationDocumentContainer,
                        documentFile
                    );
                byte[] documentBytes = Encoding.UTF8.GetBytes(documentPath);
                installationDocument.VariantId = model.VariantId;
                installationDocument.ProductId = model.ProductId;
                installationDocument.PdfLink = documentBytes;
                _context.InstallationDocumentVariants.Update(installationDocument);
                await _context.SaveChangesAsync();
                var response = _mapper.Map<InstallationDocumentDTO>(installationDocument);

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = response;
                _response.Messages = "installationDocument uploaded successfully.";
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

        //#region UploadPaymentReceipt
        ///// <summary>
        /////  Upload payment receipt.
        ///// </summary>
        //[HttpPost("UploadPaymentReceipt")]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[Authorize(Roles = "Customer,Vendor,Admin,SuperAdmin,Distributor")]
        //public async Task<IActionResult> UploadPaymentReceipt([FromForm] UploadPaymentReceipt model)
        //{
        //    try
        //    {
        //        string currentUserId = (HttpContext.User.Claims.First().Value);
        //        if (string.IsNullOrEmpty(currentUserId))
        //        {
        //            _response.StatusCode = HttpStatusCode.OK;
        //            _response.IsSuccess = false;
        //            _response.Messages = "Token expired.";
        //            return Ok(_response);
        //        }

        //        var addPaymentReceipt = new PaymentReceipt();
        //        var documentFile = ContentDispositionHeaderValue.Parse(model.paymentReceipt.ContentDisposition).FileName.Trim('"');
        //        documentFile = CommonMethod.EnsureCorrectFilename(documentFile);
        //        documentFile = CommonMethod.RenameFileName(documentFile);

        //        var documentPath = paymentReceipt + documentFile;
        //        bool uploadStatus = await _uploadRepository.UploadFilesToServer(
        //                model.paymentReceipt,
        //                paymentReceipt,
        //                documentFile
        //            );
        //        addPaymentReceipt.PaymentReceiptImage = documentPath;
        //        addPaymentReceipt.UserId = currentUserId;

        //        //await _paymentReceiptRepository.CreateEntity(addPaymentReceipt);
        //        _context.PaymentReceipt.Add(addPaymentReceipt);
        //        await _context.SaveChangesAsync();

        //        var response = _mapper.Map<PaymentReceiptDTO>(addPaymentReceipt);

        //        _response.StatusCode = HttpStatusCode.OK;
        //        _response.IsSuccess = true;
        //        _response.Data = response;
        //        _response.Messages = "Payment receipt uploaded successfully.";
        //        return Ok(_response);
        //    }
        //    catch (Exception ex)
        //    {
        //        _response.StatusCode = HttpStatusCode.InternalServerError;
        //        _response.IsSuccess = false;
        //        _response.Messages = ex.Message;
        //        return Ok(_response);
        //    }
        //}
        //#endregion

    }
}
