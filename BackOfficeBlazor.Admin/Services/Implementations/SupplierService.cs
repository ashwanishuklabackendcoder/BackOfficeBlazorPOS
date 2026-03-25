using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using BackOfficeBlazor.Admin.Services;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _repo;

        public SupplierService(ISupplierRepository repo)
        {
            _repo = repo;
        }
        public async Task<List<SupplierDto>> GetAll()
        {
            var suppliers = await _repo.GetAllSupplier();

            return suppliers.Select(ToDto).ToList();
        }

        public async Task<ApiResponse<SupplierDto>> GetAsync(string accountNo)
        {
            try
            {
                var entity = await _repo.GetByAccountNoAsync(accountNo);

                if (entity == null)
                    return ApiResponse<SupplierDto>.Fail("Supplier not found");

                return ApiResponse<SupplierDto>.Ok(ToDto(entity));
            }
            catch (Exception ex)
            {
                return ApiResponse<SupplierDto>.Fail(ex.Message);
            }
        }

        public async Task<ApiResponse<SupplierDto>> SaveAsync(SupplierDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.AccountNo))
                {
                    var last = await _repo.GetLastAccountNumberAsync();
                    dto.AccountNo = SequenceHelper.GenerateNextFiveDigitCode(last);
                }

                dto.AccountNo = dto.AccountNo?.Trim().ToUpperInvariant();

                var entity = await _repo.GetByAccountNoAsync(dto.AccountNo);

                if (entity == null)
                {
                    entity = FromDto(dto);
                    entity.IsDeleted = false;
                    entity.DateCreated = DateTime.Now;

                    await _repo.AddAsync(entity);
                }
                else
                {
                    // Update only editable fields
                    entity = FromDto(dto, entity);
                    entity.DateUpdated = DateTime.Now;

                    await _repo.UpdateAsync(entity);
                }

                await _repo.SaveChangesAsync();
                return ApiResponse<SupplierDto>.Ok(dto, "Supplier saved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<SupplierDto>.Fail($"Save failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> DeleteAsync(string accountNo)
        {
            var entity = await _repo.GetByAccountNoAsync(accountNo);

            if (entity == null)
                return ApiResponse<object>.Fail("Supplier not found");

            entity.IsDeleted = true;
            entity.DateUpdated = DateTime.Now;

            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();

            return ApiResponse<object>.Ok(null, "Supplier deleted successfully");
        }

        private SupplierDto ToDto(Supplier x) =>
            new SupplierDto
            {
                AccountNo = x.AccountNo,
                Name = x.Name,
                Address1 = x.Address1,
                Address2 = x.Address2,
                Address3 = x.Address3,
                Address4 = x.Address4,
                Postcode = x.Postcode,
                Telephone = x.Telephone,
                Fax = x.Fax,
                Email = x.Email,
                B2BFileName = x.B2BFileName,
                B2BFileType = x.B2BFileType,
                B2BFileHasHeaderRow = x.B2BFileHasHeaderRow,
                B2BFileAppendLocationCode = x.B2BFileAppendLocationCode,
                SettlementDiscount = x.SettlementDiscount,
                CarriagePaidAmount = x.CarriagePaidAmount
            };

        private Supplier FromDto(SupplierDto dto, Supplier? entity = null)
        {
            entity ??= new Supplier();

            entity.AccountNo = dto.AccountNo.Trim().ToUpper();
            entity.Name = dto.Name;
            entity.Address1 = dto.Address1;
            entity.Address2 = dto.Address2;
            entity.Address3 = dto.Address3;
            entity.Address4 = dto.Address4;
            entity.Postcode = dto.Postcode;
            entity.Telephone = dto.Telephone;
            entity.Fax = dto.Fax;
            entity.Email = dto.Email;
            entity.B2BFileName = dto.B2BFileName;
            entity.B2BFileType = dto.B2BFileType;
            entity.B2BFileHasHeaderRow = dto.B2BFileHasHeaderRow;
            entity.B2BFileAppendLocationCode = dto.B2BFileAppendLocationCode;
            entity.SettlementDiscount = dto.SettlementDiscount;
            entity.CarriagePaidAmount = dto.CarriagePaidAmount;

            return entity;
        }


        public async Task<List<SupplierDto>> SuggestSuppliers(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return new();

            return await _repo.SuggestSuppliersAsync(term);
        }

    }
}
