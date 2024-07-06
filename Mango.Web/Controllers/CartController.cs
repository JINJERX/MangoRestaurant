using Mango.Web.Models;
using Mango.Web.Models.CartModels;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IProductService _productService;

    public CartController(ICartService cartService, IProductService productService)
    {
        _cartService = cartService;
        _productService = productService;
    }

    public async Task<IActionResult> CartIndex()
    {
        return View(await LoadCartDtoBasedOnLoggedInUserAsync());
    }
    
    public async Task<IActionResult> RemoveItem(int cartDetailsId)
    {
        var userId = User.Claims.Where(u => u.Type == "sub").FirstOrDefault()?.Value;
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        var response = await _cartService.RemoveFromCartAsync<ResponseDto>(cartDetailsId, accessToken);

        if (response is not null && response.IsSuccess)
        {
            return RedirectToAction(nameof(CartIndex));
        }

        return BadRequest();
    }

    public IActionResult Checkout()
    {
        throw new NotImplementedException();
    }

    private async Task<CartDto> LoadCartDtoBasedOnLoggedInUserAsync()
    {
        var userId = User.Claims.Where(u => u.Type == "sub").FirstOrDefault()?.Value;
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        var response = await _cartService.GetCartByUserIdAsync<ResponseDto>(userId, accessToken);

        CartDto cartDto = new CartDto();
        if (response is not null && response.Result is not null && response.IsSuccess)
        {
            cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
        }

        if (cartDto.CartHeader is not null)
        {
            foreach (var detail in cartDto.CartDetails)
            {
                cartDto.CartHeader.OrderTotal += (detail.Count * detail.Product.Price);
            }
        }

        return cartDto;
    }

    
}