using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public class CosmeticInformationDAO
{
    private static CosmeticInformationDAO? instance = null;

    private CosmeticInformationDAO() { }

    public static CosmeticInformationDAO Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CosmeticInformationDAO();
            }
            return instance;
        }
    }

    public async Task<List<CosmeticInformation>> GetAllCosmetics()
    {
        try
        {
            using (var context = new CosmeticsDbContext())
            {
                var listCosmetics = await context.CosmeticInformations
                    .Include(x => x.Category)
                    .ToListAsync();
                return listCosmetics;
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<List<CosmeticCategory>> GetAllCategories()
    {
        using (var context = new CosmeticsDbContext())
        {
            var listCategories = await context.CosmeticCategories.ToListAsync();
            return listCategories;
        }
    }

    public async Task<CosmeticInformation> AddCosmeticInformation(CosmeticInformation cosmeticInformation)
    {
        using (var context = new CosmeticsDbContext())
        {
            var categoryObject = await context.CosmeticCategories
                .FirstOrDefaultAsync(x => x.CategoryId.Equals(cosmeticInformation.CategoryId));
            if (categoryObject == null)
            {
                throw new Exception("Category is not found");
            }

            cosmeticInformation.CosmeticId = GenerateId();
            await context.CosmeticInformations.AddAsync(cosmeticInformation);
            await context.SaveChangesAsync();
            return cosmeticInformation;
        }
    }

    public async Task<CosmeticInformation> GetById(string id)
    {
        using (var context = new CosmeticsDbContext())
        {
            var resultObject = await context.CosmeticInformations
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.CosmeticId.Equals(id));
            return resultObject!;
        }
    }

    public async Task<CosmeticInformation> Update(CosmeticInformation cosmeticInformation)
    {
        using (var context = new CosmeticsDbContext())
        {
            var updateObject = await context.CosmeticInformations
                .FirstOrDefaultAsync(x => x.CosmeticId == cosmeticInformation.CosmeticId);
            if (updateObject == null)
            {
                throw new Exception("CosmeticInformations not found");
            }

            var cate = await context.CosmeticCategories
                .FirstOrDefaultAsync(x => x.CategoryId.Equals(cosmeticInformation.CategoryId));
            if (cate == null)
            {
                throw new Exception("Cate not found");
            }

            updateObject.CosmeticName = cosmeticInformation.CosmeticName;
            updateObject.SkinType = cosmeticInformation.SkinType;
            updateObject.ExpirationDate = cosmeticInformation.ExpirationDate;
            updateObject.CosmeticSize = cosmeticInformation.CosmeticSize;
            updateObject.DollarPrice = cosmeticInformation.DollarPrice;
            updateObject.CategoryId = cosmeticInformation.CategoryId;

            context.CosmeticInformations.Update(updateObject);
            await context.SaveChangesAsync();
            return updateObject;
        }
    }

    public async Task<CosmeticInformation> Delete(string id)
    {
        using (var context = new CosmeticsDbContext())
        {
            var deleteObject = await context.CosmeticInformations
                .FirstOrDefaultAsync(p => p.CosmeticId.Equals(id));
            if (deleteObject == null)
            {
                throw new Exception("CosmeticInformations not found");
            }

            context.CosmeticInformations.Remove(deleteObject);
            await context.SaveChangesAsync();
            return deleteObject;
        }
    }

    private string GenerateId()
    {
        var random = new Random();
        var id = random.Next(100000, 999999);
        return "PL" + id.ToString();
    }
}
