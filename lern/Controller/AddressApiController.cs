using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[ApiController, Route("api/address")]
public class AddressApiController : ControllerBase
{
    private readonly ShopDbContext _db;
    public AddressApiController(ShopDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var key = EnsureCustomerKey();
        await EnsureAddressTableAsync();
        var address = await _db.Addresses.AsNoTracking().Where(x => x.CustomerKey == key)
            .OrderByDescending(x => x.IsDefault).ThenByDescending(x => x.UpdatedAt).FirstOrDefaultAsync();
        return address is null ? NoContent() : Ok(ToResponse(address));
    }

    [HttpPut]
    public async Task<IActionResult> Save(AddressRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Province) ||
            string.IsNullOrWhiteSpace(request.City) || string.IsNullOrWhiteSpace(request.Details) ||
            string.IsNullOrWhiteSpace(request.PostalCode) || string.IsNullOrWhiteSpace(request.Phone) ||
            string.IsNullOrWhiteSpace(request.ReceiverName))
            return BadRequest("لطفاً همه فیلدهای الزامی را تکمیل کنید.");
        if (request.PostalCode.Length != 10 || !request.PostalCode.All(char.IsDigit))
            return BadRequest("کد پستی باید ۱۰ رقم باشد.");

        var key = EnsureCustomerKey();
        var values = new { Title = request.Title.Trim(), Province = request.Province.Trim(), City = request.City.Trim(), Details = request.Details.Trim(), PostalCode = request.PostalCode.Trim(), Phone = request.Phone.Trim(), ReceiverName = request.ReceiverName.Trim(), request.IsDefault };
        await EnsureAddressTableAsync();
        var requestedId = int.TryParse(Request.Query["id"], out var id) ? id : 0;
        var address = await _db.Addresses.Where(x => x.CustomerKey == key && (requestedId == 0 || x.Id == requestedId)).OrderByDescending(x => x.IsDefault).ThenByDescending(x => x.UpdatedAt).FirstOrDefaultAsync();
        if (address is null) { address = new Address { CustomerKey = key }; _db.Addresses.Add(address); }
        if (request.IsDefault) await _db.Addresses.Where(x => x.CustomerKey == key && x.Id != address.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDefault, false));
        address.Title = values.Title; address.Province = values.Province; address.City = values.City; address.Details = values.Details;
        address.PostalCode = values.PostalCode; address.Phone = values.Phone; address.ReceiverName = values.ReceiverName;
        address.IsDefault = values.IsDefault; address.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ToResponse(address));
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddressRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Province) || string.IsNullOrWhiteSpace(request.City) || string.IsNullOrWhiteSpace(request.Details) || string.IsNullOrWhiteSpace(request.PostalCode) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.ReceiverName)) return BadRequest("لطفاً همه فیلدهای الزامی را تکمیل کنید.");
        if (request.PostalCode.Length != 10 || !request.PostalCode.All(char.IsDigit)) return BadRequest("کد پستی باید ۱۰ رقم باشد.");
        await EnsureAddressTableAsync(); var key = EnsureCustomerKey();
        if (await _db.Addresses.CountAsync(x => x.CustomerKey == key) >= 3)
            return BadRequest("شما نمی‌توانید بیشتر از سه آدرس ذخیره کنید.");
        if (request.IsDefault) await _db.Addresses.Where(x => x.CustomerKey == key).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDefault, false));
        var address = new Address { CustomerKey = key, Title = request.Title.Trim(), Province = request.Province.Trim(), City = request.City.Trim(), Details = request.Details.Trim(), PostalCode = request.PostalCode.Trim(), Phone = request.Phone.Trim(), ReceiverName = request.ReceiverName.Trim(), IsDefault = request.IsDefault, UpdatedAt = DateTime.UtcNow };
        _db.Addresses.Add(address); await _db.SaveChangesAsync(); return Ok(ToResponse(address));
    }

    [HttpGet("list")]
    public async Task<IActionResult> List()
    {
        await EnsureAddressTableAsync();
        var key = EnsureCustomerKey();
        var addresses = await _db.Addresses.AsNoTracking().Where(x => x.CustomerKey == key)
            .OrderByDescending(x => x.IsDefault).ThenByDescending(x => x.UpdatedAt).ToListAsync();
        return Ok(addresses.Select(ToResponse));
    }

    [HttpPost("{id:int}/default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        await EnsureAddressTableAsync(); var key = EnsureCustomerKey();
        var address = await _db.Addresses.FirstOrDefaultAsync(x => x.Id == id && x.CustomerKey == key);
        if (address is null) return NotFound();
        await _db.Addresses.Where(x => x.CustomerKey == key).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDefault, false));
        address.IsDefault = true; address.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return Ok(ToResponse(address));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await EnsureAddressTableAsync();
        var key = EnsureCustomerKey();
        var address = await _db.Addresses.FirstOrDefaultAsync(x => x.Id == id && x.CustomerKey == key);
        if (address is null) return NotFound();

        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private object ToResponse(Address x) => new { x.Id, x.Title, x.Province, x.City, Details = x.Details, x.PostalCode, x.Phone, x.ReceiverName, x.IsDefault };
    private Task<int> EnsureAddressTableAsync() => _db.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[Addresses]', N'U') IS NULL
BEGIN
    CREATE TABLE [Addresses] (
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Addresses] PRIMARY KEY,
        [CustomerKey] nvarchar(450) NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Province] nvarchar(max) NOT NULL,
        [City] nvarchar(max) NOT NULL,
        [Details] nvarchar(max) NOT NULL,
        [PostalCode] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [ReceiverName] nvarchar(max) NOT NULL,
        [IsDefault] bit NOT NULL,
        [UpdatedAt] datetime2 NOT NULL
    );
    CREATE INDEX [IX_Addresses_CustomerKey_Title] ON [Addresses] ([CustomerKey], [Title]);
END");

    private string EnsureCustomerKey()
    {
        var key = Request.Cookies["customer-key"];
        if (!string.IsNullOrWhiteSpace(key)) return key;
        key = Guid.NewGuid().ToString("N");
        Response.Cookies.Append("customer-key", key, new CookieOptions { HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Lax, Expires = DateTimeOffset.UtcNow.AddYears(1) });
        return key;
    }
}

public record AddressRequest(string Title, string Province, string City, string Details, string PostalCode, string Phone, string ReceiverName, bool IsDefault);
