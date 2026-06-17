using BusinessObjects;

namespace Services;

public interface ICosmeticInformationService
{
    Task<List<CosmeticInformation>> GetAllCosmetics();
    Task<CosmeticInformation> GetOne(string id);
    Task<CosmeticInformation> Add(CosmeticInformation cosmeticInformation);
    Task<CosmeticInformation> Update(CosmeticInformation cosmeticInformation);
    Task<CosmeticInformation> Delete(string id);
    Task<List<CosmeticCategory>> GetAllCategories();
}
