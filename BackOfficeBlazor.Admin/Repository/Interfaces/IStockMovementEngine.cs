using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface IStockMovementEngine
    {
        Task SellMajorAsync(string partNumber, string stockNumber, string location);
        Task SellMinorAsync(string partNumber, int qty, string location);
        Task ReturnMajorAsync(string partNumber, string stockNumber, string location);
        Task ReturnMinorAsync(string partNumber, int qty, string location);
    }
}
