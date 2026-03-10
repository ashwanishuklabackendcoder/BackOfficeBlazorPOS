using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly IProductLevelRepository _levelRepo;
        private readonly IProductGroupRepository _groupRepo;
        private readonly IProductGroupItemRepository _groupItemRepo;

        public ProductService(
           IProductRepository productRepo,
           IProductLevelRepository levelRepo,
           IProductGroupRepository groupRepo,
           IProductGroupItemRepository groupItemRepo)
        {
            _productRepo = productRepo;
            _levelRepo = levelRepo;
            _groupRepo = groupRepo;
            _groupItemRepo = groupItemRepo;
        }


        public async Task<ApiResponse<ProductDto>> GetAsync(string partNumber)
        {
            var entity = await _productRepo.GetByPartNumberAsync(partNumber);
            if (entity == null)
                return ApiResponse<ProductDto>.Fail("Product not found");

            return ApiResponse<ProductDto>.Ok(ToDto(entity));
        }
        public async Task<ApiResponse<ProductDto>> GetMFRPartNo(string MfrpartNumber)
        {
            var entity = await _productRepo.GetByMfrPartNumberAsync(MfrpartNumber);
            if (entity == null)
                return ApiResponse<ProductDto>.Fail("Product not found");

            return ApiResponse<ProductDto>.Ok(ToDto(entity));
        }
        public async Task<ApiResponse<ProductDto>> GetBarcode(string Barcode)
        {
            var entity = await _productRepo.GetByBarcodeNumberAsync(Barcode);
            if (entity == null)
                return ApiResponse<ProductDto>.Fail("Product not found");

            return ApiResponse<ProductDto>.Ok(ToDto(entity));
        }
        public async Task<ApiResponse<List<ProductDto>>> GetAllAsync(ProductFilterDto filter)
        {
            var entities = await _productRepo.GetAllAsync(filter);

            if (entities == null || !entities.Any())
                return ApiResponse<List<ProductDto>>.Fail("No products found");

            var result = entities.Select(ToDto).ToList();

            return ApiResponse<List<ProductDto>>.Ok(result);
        }

        public async Task<ApiResponse<ProductDto>> SaveAsync(ProductDto dto)
        {
            try
            {
                ProductItem? entity = null;

                if (!string.IsNullOrWhiteSpace(dto.PartNumber))
                    entity = await _productRepo.GetByPartNumberAsync(dto.PartNumber);
                if (!string.IsNullOrWhiteSpace(dto.MfrPartNumber))
                {
                    entity = await _productRepo.GetByMfrPartNumberAsync(dto.MfrPartNumber);
                    if (entity != null && entity.PartNumber != dto.PartNumber)
                    {
                        return ApiResponse<ProductDto>.Fail("MfrPartNumber already exists for another product");
                    }
                }
                if (!string.IsNullOrWhiteSpace(dto.Barcode))
                {
                    entity = await _productRepo.GetByBarcodeNumberAsync(dto.Barcode);
                    if (entity != null && entity.PartNumber != dto.PartNumber)
                    {
                        return ApiResponse<ProductDto>.Fail("Barcode already exists for another product");
                    }
                }
                
                    

                if (entity == null)
                {
                    // New product → generate part number
                    var nextPartNumber = await GenerateNextPartNumberAsync();
                    dto.PartNumber = nextPartNumber;

                    entity = FromDto(dto);
                    entity.CreatedOn = DateTime.Now;

                    await _productRepo.AddAsync(entity);
                    await _productRepo.SaveChangesAsync();

                    // Auto create ProductLevels with defaults
                    var existingLevels = await _levelRepo.GetByPartNumberAsync(nextPartNumber);
                    if (existingLevels == null)
                    {
                        var level = new ProductLevel
                        {
                            PartNumber = nextPartNumber
                            // all Min/Max/Replenish default 0/false by CLR default
                        };
                        await _levelRepo.AddAsync(level);
                        await _levelRepo.SaveChangesAsync();
                    }

                    return ApiResponse<ProductDto>.Ok(dto, $"Product added with PartNumber {nextPartNumber}");
                }
                else
                {
                    // Update existing
                    FromDto(dto, entity);
                    entity.UpdatedOn = DateTime.Now;

                    await _productRepo.UpdateAsync(entity);
                    await _productRepo.SaveChangesAsync();

                    return ApiResponse<ProductDto>.Ok(dto, "Product updated successfully");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<ProductDto>.Fail($"Save failed: {ex.Message}");
            }
        }


        public async Task<ApiResponse<bool>> SaveGroupProduct(GroupProductDto dto)
        {
            try
            {
                // 1. Validate
                if (string.IsNullOrWhiteSpace(dto.GroupName))
                    return ApiResponse<bool>.Fail("Group name is required");

                if (string.IsNullOrWhiteSpace(dto.GroupCode))
                {
                    var nextGroupNumber = await GenerateNextGroupNumberAsync();
                    dto.GroupCode = nextGroupNumber;
                }
                   

                if (dto.Variants == null || !dto.Variants.Any())
                    return ApiResponse<bool>.Fail("At least one variant is required");

                // 2. Create group
                var group = new ProductGroup
                {
                    GroupName = dto.GroupName,
                    GroupCode = dto.GroupCode,
                    CreatedOn = DateTime.Now
                };

                await _groupRepo.AddAsync(group);
                await _groupRepo.SaveChangesAsync();

                // 3. Save each variant as a normal product
                foreach (var variant in dto.Variants)
                {
                    // Reuse EXISTING SaveAsync logic
                    var saveResult = await SaveAsync(variant);

                    if (!saveResult.Success)
                        return ApiResponse<bool>.Fail(saveResult.Message);

                    // 4. Link product to group
                    var link = new ProductGroupItem
                    {
                        GroupId = group.GroupId,
                        PartNumber = variant.PartNumber
                    };

                    await _groupItemRepo.AddAsync(link);
                }

                await _groupItemRepo.SaveChangesAsync();

                return ApiResponse<bool>.Ok(true, "Group item saved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Group save failed: {ex.Message}");
            }
        }
        private async Task<string> GenerateNextGroupNumberAsync()
        {
            var last = await _groupRepo.GetLastGroupNumberAsync();

            if (string.IsNullOrWhiteSpace(last))
                return "00001";

            const string maxNumeric = "99999";

            bool isNumeric = last.All(char.IsDigit);

            if (isNumeric)
            {
                if (last == maxNumeric)
                {
                    // Switch to alpha sequence
                    return "A0001";
                }

                var n = long.Parse(last);
                n++;
                return n.ToString("D5"); // preserve 10 chars with leading zeros
            }
            else
            {
                // Alpha pattern, e.g. A100000000, A100000001...
                char prefix = last[0];
                var numericPart = last.Substring(1);

                if (!long.TryParse(numericPart, out var num))
                {
                    // fallback if corrupt – restart at A100000000
                    return "A0001";
                }

                num++;
                // keep same numeric length
                var numericLen = numericPart.Length;
                var nextNumeric = num.ToString(new string('0', numericLen));
                return prefix + nextNumeric;
            }
        }

        private async Task<string> GenerateNextPartNumberAsync()
        {
            var last = await _productRepo.GetLastPartNumberAsync();

            if (string.IsNullOrWhiteSpace(last))
                return "00001";

            const string maxNumeric = "99999";

            bool isNumeric = last.All(char.IsDigit);

            if (isNumeric)
            {
                if (last == maxNumeric)
                {
                    // Switch to alpha sequence
                    return "A0001";
                }

                var n = long.Parse(last);
                n++;
                return n.ToString("D5"); // preserve 10 chars with leading zeros
            }
            else
            {
                // Alpha pattern, e.g. A100000000, A100000001...
                char prefix = last[0];
                var numericPart = last.Substring(1);

                if (!long.TryParse(numericPart, out var num))
                {
                    // fallback if corrupt – restart at A100000000
                    return "A0001";
                }

                num++;
                // keep same numeric length
                var numericLen = numericPart.Length;
                var nextNumeric = num.ToString(new string('0', numericLen));
                return prefix + nextNumeric;
            }
        }

        private ProductDto ToDto(ProductItem x) => new()
        {
            PartNumber = x.PartNumber,
            MfrPartNumber = x.MfrPartNumber,
            MfrPartNumber2 = x.MfrPartNumber2,
            Major = x.Major ,
            Gender = (ProductDto.Genders)(x.Gender ?? 0),
            Suitability = (ProductDto.AgeRange)(x.Suitability ?? 0),
            Make = x.Make,
            MakeCode = x.MakeCode,
            GroupCode = x.GroupCode,
            IsVariant = x.IsVariant,
            GroupName = x.GroupName,
            Search1 = x.Search1,
            Details = x.Details,
            Size = x.Size,
            Color = x.Color,
            Weight = x.Weight,
            CostPrice = (decimal)x.CostPrice,
            Discount = x.Discount,
            Markup = x.Markup,
            OfferCode=x.OfferCode,
            VatCode = x.VatCode,
            SuggestedRRP = x.SuggestedRRP,
            StorePrice = x.StorePrice,
            TradePrice = x.TradePrice,
            MailOrderPrice = x.MailOrderPrice,
            WebPrice = x.WebPrice,
            Current = x.Current,
            PrintLabel = x.PrintLabel,
            AllowDiscount = x.AllowDiscount,
            Season = (ProductDto.Seasons)(x.Season ?? 0),
            Year = x.Year,
            BoxQuantity = x.BoxQuantity,
            PromoName = x.PromoName,
            PromoPrice = (decimal)x.PromoPrice,
            PromoStart = x.PromoStart,
            PromoEnd = x.PromoEnd,
            Supplier1Code = x.Supplier1Code,
            Supplier2Code = x.Supplier2Code,
            CatA = x.CatA,
            CatB = x.CatB,
            CatC = x.CatC,
            ImageMain = x.ImageMain,
            Image2 = x.Image2,
            Image3 = x.Image3,
            Image4 = x.Image4,
            CatACode = x.CatACode,
            CatBCode = x.CatBCode,
            CatCCode = x.CatCCode,
            KeyItem = x.KeyItem,
            AllowPoints = x.AllowPoints,
            Website = x.Website,
            WebOnly = x.WebOnly,
            ShortDescription = x.ShortDescription,
            FullDescription = x.FullDescription,
            Specifications = x.Specifications,
            Geometry = x.Geometry
        };

        private ProductItem FromDto(ProductDto dto, ProductItem? entity = null)
        {
            entity ??= new ProductItem();

            entity.PartNumber = dto.PartNumber ?? entity.PartNumber;
            entity.MfrPartNumber = dto.MfrPartNumber;
            entity.MfrPartNumber2 = dto.MfrPartNumber2;
            entity.Major = dto.Major;
            entity.Gender = (int)dto.Gender;
            entity.GroupCode = dto.GroupCode;
            entity.IsVariant = dto.IsVariant;
            entity.GroupName = dto.GroupName;
            entity.Suitability = (int)dto.Suitability; 
            entity.Make = dto.Make;
            entity.MakeCode = dto.MakeCode;
            entity.Search1 = dto.Search1;
            entity.Details = dto.Details;
            entity.Size = dto.Size;
            entity.Color = dto.Color;
            entity.ImageMain = dto.ImageMain;
            entity.Image2 = dto.Image2;
            entity.Image3 = dto.Image3;
            entity.Image4 = dto.Image4;
            entity.Weight = dto.Weight;
            entity.CostPrice = dto.CostPrice;
            entity.OfferCode = dto.OfferCode;
            entity.Discount = dto.Discount;
            entity.Markup = dto.Markup;
            entity.VatCode = dto.VatCode;
            entity.SuggestedRRP = dto.SuggestedRRP;
            entity.StorePrice = dto.StorePrice;
            entity.TradePrice = dto.TradePrice;
            entity.MailOrderPrice = dto.MailOrderPrice;
            entity.WebPrice = dto.WebPrice;
            entity.Current = dto.Current;
            entity.PrintLabel = dto.PrintLabel;
            entity.AllowDiscount = dto.AllowDiscount;
            entity.Season = (int)dto.Season;
            entity.Year = dto.Year;
            entity.BoxQuantity = dto.BoxQuantity;
            entity.PromoName = dto.PromoName;
            entity.PromoPrice = dto.PromoPrice;
            entity.PromoStart = dto.PromoStart;
            entity.PromoEnd = dto.PromoEnd;
            entity.Supplier1Code = dto.Supplier1Code;
            entity.Supplier2Code = dto.Supplier2Code;
            entity.CatA = dto.CatA;
            entity.CatB = dto.CatB;
            entity.CatC = dto.CatC;
            entity.CatACode = dto.CatACode;
            entity.CatBCode = dto.CatBCode;
            entity.CatCCode = dto.CatCCode;
            entity.KeyItem = dto.KeyItem;
            entity.AllowPoints = dto.AllowPoints;
            entity.Website = dto.Website;
            entity.WebOnly = dto.WebOnly;
            entity.ShortDescription = dto.ShortDescription;
            entity.FullDescription = dto.FullDescription;
            entity.Specifications = dto.Specifications;
            entity.Geometry = dto.Geometry;

            return entity;
        }
    }
}
