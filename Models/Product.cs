﻿using System;
using System.Collections.Generic;
using MaxemusAPI;
using MaxemusAPI.Models;


namespace MaxemusAPI.Models
{
    public partial class Product
    {
        public Product()
        {
            AudioVariants = new HashSet<AudioVariants>();
            CameraVariants = new HashSet<CameraVariants>();
            CertificationVariants = new HashSet<CertificationVariants>();
            DealerOrderedProduct = new HashSet<DealerOrderedProduct>();
            DistributorOrderedProduct = new HashSet<DistributorOrderedProduct>();
            EnvironmentVariants = new HashSet<EnvironmentVariants>();
            GeneralVariants = new HashSet<GeneralVariants>();
            InstallationDocumentVariants = new HashSet<InstallationDocumentVariants>();
            NetworkVariants = new HashSet<NetworkVariants>();
            ProductStock = new HashSet<ProductStock>();
            RedeemedProducts = new HashSet<RedeemedProducts>();
        }

        public int ProductId { get; set; }
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int BrandId { get; set; }
        public string? Model { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Image1 { get; set; }
        public string? Image2 { get; set; }
        public string? Image3 { get; set; }
        public string? Image4 { get; set; }
        public string? Image5 { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int RewardPoint { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }

        public virtual Brand Brand { get; set; } = null!;
        public virtual MainCategory MainCategory { get; set; } = null!;
        public virtual SubCategory SubCategory { get; set; } = null!;
        public virtual ICollection<AudioVariants> AudioVariants { get; set; }
        public virtual ICollection<CameraVariants> CameraVariants { get; set; }
        public virtual ICollection<CertificationVariants> CertificationVariants { get; set; }
        public virtual ICollection<DealerOrderedProduct> DealerOrderedProduct { get; set; }
        public virtual ICollection<DistributorOrderedProduct> DistributorOrderedProduct { get; set; }
        public virtual ICollection<EnvironmentVariants> EnvironmentVariants { get; set; }
        public virtual ICollection<GeneralVariants> GeneralVariants { get; set; }
        public virtual ICollection<InstallationDocumentVariants> InstallationDocumentVariants { get; set; }
        public virtual ICollection<NetworkVariants> NetworkVariants { get; set; }
        public virtual ICollection<ProductStock> ProductStock { get; set; }
        public virtual ICollection<RedeemedProducts> RedeemedProducts { get; set; }
    }
}
