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
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repo;

        public CustomerService(ICustomerRepository repo)
        {
            _repo = repo;
        }

        public async Task<ApiResponse<CustomerDto>> GetAsync(string accNo)
        {
            var entity = await _repo.GetByAccNoAsync(accNo);
            if (entity == null)
                return ApiResponse<CustomerDto>.Fail("Customer Not Found");

            return ApiResponse<CustomerDto>.Ok(ToDto(entity));
        }
        public async Task<ApiResponse<List<CustomerDto>>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            if (entities == null || entities.Count == 0)
                return ApiResponse<List<CustomerDto>>.Fail("Customer Not Found");

            return ApiResponse<List<CustomerDto>>.Ok(
                entities.Select(ToDto).ToList());
        }
        public async Task<ApiResponse<CustomerDto>> SaveAsync(CustomerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.AccNo))
                return ApiResponse<CustomerDto>.Fail("Account No is required");

            var entity = await _repo.GetByAccNoAsync(dto.AccNo);

            if (entity == null)
            {
                entity = FromDto(dto);
               // entity.CreatedOn = DateTime.Now;
                await _repo.AddAsync(entity);
            }
            else
            {
                // Update without touching Id or CreatedOn
                var tempEntity = FromDto(dto, entity);
                entity = tempEntity;
            }

            await _repo.SaveChangesAsync();
            return ApiResponse<CustomerDto>.Ok(dto, "Saved Successfully");
        }

        public async Task<ApiResponse<object>> DeleteAsync(string accNo)
        {
            var entity = await _repo.GetByAccNoAsync(accNo);
            if (entity == null)
                return ApiResponse<object>.Fail("Customer not found");

            entity.Stop = true;
            await _repo.SaveChangesAsync();

            return ApiResponse<object>.Ok(null, "Customer deleted successfully");
        }

        private CustomerDto ToDto(Customer x) =>
            new CustomerDto
            {
                AccNo = x.AccNo,
                Title = x.Title,
                Initials = x.Initials,
                Firstname = x.Firstname,
                Surname = x.Surname,
                HouseName = x.HouseName,
                Postcode = x.Postcode,
                Telephone = x.Telephone,
                Mobile = x.Mobile,
                Email = x.Email,
                LoyaltyNo = x.LoyaltyNo,
                Balance = x.Balance,
                Address1 = x.Address1,
                Address2 = x.Address2,
                Address3 = x.Address3,
                Address4 = x.Address4,
                Stop = x.Stop,
                SendLetter = x.SendLetter,
                CreditLimit = x.CreditLimit,
                VATExempt = x.VATExempt,
                Category = x.Category,
                Autopay = x.Autopay,
                Country = x.Country,
                WorkPhone = x.WorkPhone,
                DOB = x.DOB,
                DiscountMinor = x.DiscountMinor,
                DiscountMajor = x.DiscountMajor,
                DeliveryTitle = x.DeliveryTitle,
                DeliveryInitials = x.DeliveryInitials,
                DeliverySurname = x.DeliverySurname,
                DeliveryFirstname = x.DeliveryFirstname,
                DeliveryHousename = x.DeliveryHousename,
                DeliveryAddress1 = x.DeliveryAddress1,
                DeliveryAddress2 = x.DeliveryAddress2,
                DeliveryAddress3 = x.DeliveryAddress3,
                DeliveryAddress4 = x.DeliveryAddress4,
                DeliveryPostcode = x.DeliveryPostcode,
                DeliveryCountry = x.DeliveryCountry,
            };

        private Customer FromDto(CustomerDto dto, Customer? entity = null)
        {
            entity ??= new Customer();
            entity.AccNo = dto.AccNo;
            // Copy all editable properties
            entity.Title = dto.Title;
            entity.Initials = dto.Initials;
            entity.Firstname = dto.Firstname;
            entity.Surname = dto.Surname;
            entity.HouseName = dto.HouseName;
            entity.Postcode = dto.Postcode;
            entity.Telephone = dto.Telephone;
            entity.Mobile = dto.Mobile;
            entity.Email = dto.Email;
            entity.LoyaltyNo = dto.LoyaltyNo;
            entity.Balance = dto.Balance;
            entity.Address1 = dto.Address1;
            entity.Address2 = dto.Address2;
            entity.Address3 = dto.Address3;
            entity.Address4 = dto.Address4;
            entity.Stop = dto.Stop;
            entity.SendLetter = dto.SendLetter;
            entity.CreditLimit = dto.CreditLimit;
            entity.VATExempt = dto.VATExempt;
            entity.Category = dto.Category;
            entity.Autopay = dto.Autopay;
            entity.Country = dto.Country;
            entity.WorkPhone = dto.WorkPhone;
            entity.DOB = dto.DOB;
            entity.DiscountMinor = dto.DiscountMinor;
            entity.DiscountMajor = dto.DiscountMajor;
            entity.DeliveryTitle = dto.DeliveryTitle;
            entity.DeliveryInitials = dto.DeliveryInitials;
            entity.DeliverySurname = dto.DeliverySurname;
            entity.DeliveryFirstname = dto.DeliveryFirstname;
            entity.DeliveryHousename = dto.DeliveryHousename;
            entity.DeliveryAddress1 = dto.DeliveryAddress1;
            entity.DeliveryAddress2 = dto.DeliveryAddress2;
            entity.DeliveryAddress3 = dto.DeliveryAddress3;
            entity.DeliveryAddress4 = dto.DeliveryAddress4;
            entity.DeliveryPostcode = dto.DeliveryPostcode;
            entity.DeliveryCountry = dto.DeliveryCountry;

            return entity;
        }
    }
}
