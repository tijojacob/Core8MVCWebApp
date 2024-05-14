using Core8MVC.DataAccess.Repository.IRepository;
using Core8MVC.Models.Models;
using Microsoft.AspNetCore.Mvc;

namespace Core8MVCWebApp.ViewComponents
{
    public class ProductImageCarouselViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductImageCarouselViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync(int productId)
        {
            Product product = _unitOfWork._productRepository.Get(u => u.Id == productId, includeProperties: "ProductImages");            
            return View(product);
        }
    }
}
