using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Theme_Implemenet.Models;

namespace Theme_Implemenet.Controllers
{
    public class ProductController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment hostEnvironment;
    
        public ProductController(MyDbContext context,IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            this.hostEnvironment=hostEnvironment;
        }
        public IActionResult Home()
        {
            var product = _context.productVeriations.ToList();
            return View(product);
        }
        [Route("IndexProduct")]
        public IActionResult Index(string term = "", int currentpage = 1)
        {
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();
            var catdata = new ProductViewModel();
            //var cat = _context.categories.Include(p => p.Status).ToList();
            var cat = (from emp in _context.product.Include(c => c.Category).Include(s => s.SubCategory).Include(c => c.ProductSeason).Include(s => s.Productgender).Include(s => s.Status)
                       where term == "" || emp.productName.ToLower().StartsWith(term)
                       select emp);
                       //{
                       //    productId = emp.productId,
                       //    productName = emp.productName,
                       //    categoryId = emp.categoryId,
                       //    subcategoryId=emp.subcategoryId,
                       //    productseasonId=emp.productseasonId,
                       //    productgenderId=emp.productgenderId,
                       //    StatusId=emp.StatusId,
                          
                       //}
                       // );

            int totalrecord = cat.Count();
            int pagesize = 5;
            int totlapages = (int)Math.Ceiling(totalrecord / (double)pagesize);
            cat = cat.Skip((currentpage - 1) * pagesize).Take(pagesize);

            catdata.Product = cat;
            catdata.currentpage = currentpage;
            catdata.pagesize = pagesize;
            catdata.totalpage = totlapages;
            return View(catdata);
        }

        [HttpGet]
       //[Route("CreateProduct")]
        public IActionResult Create()
        {
            ViewBag.cat = _context.categories.ToList();
            ViewBag.style = _context.categoriestyle.ToList();
            ViewBag.sub = _context.subcategories.ToList();
            ViewBag.gender = _context.productgenders.ToList();
            ViewBag.material = _context.productMaterials.ToList();
            ViewBag.season = _context.productSeasons.ToList();
            ViewBag.status = _context.statuses.ToList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProductModel model, IFormFile file)
        {
            ViewBag.cat = _context.categories.ToList();
            ViewBag.style = _context.categoriestyle.ToList();
            ViewBag.sub = _context.subcategories.ToList();
            ViewBag.gender = _context.productgenders.ToList();
            ViewBag.material = _context.productMaterials.ToList();
            ViewBag.season = _context.productSeasons.ToList();
            ViewBag.status = _context.statuses.ToList();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var isUnique = !_context.product.Any(p => p.productName == model.productName);
            if (!isUnique)
            {
                ModelState.AddModelError("productName", "productName name must be unique");
                return View(model);
            }
            var product = new Product();
            product.productName = model.productName;
            product.productcod = model.productcod;
            product.title=model.title;
            product.description=model.description;
            product.price = model.price;
            product.sizeguideimage=model.sizeguideimage;
            product.categorystyleid=model.categorystyleid;
            product.Keyword = model.Keyword;
            product.categoryId=model.categoryId;
            product.subcategoryId=model.subcategoryId;
            product.productmaterialId = model.productmaterialId;
            product.productseasonId=model.productseasonId;
            product.productgenderId=model.productgenderId;
            product.StatusId =2;
            product.Lastmodifield = DateTime.Now;
            product.CreateDate = DateTime.Now;
            _context.product.Add(product);
            _context.SaveChanges();

            var uniqueFileName = $"{product.productId}.jpg";
            var imageDirectory = Path.Combine(hostEnvironment.WebRootPath, "Image/ProductLogo");
            var filename = "Image/ProductLogo/"+ uniqueFileName;
            var fullImagePath = Path.Combine(imageDirectory, uniqueFileName);
            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
            }

            using (var stream = new FileStream(fullImagePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);

            }
            product.productimage = filename;
            _context.product.Update(product);
            _context.SaveChanges();
            return RedirectToAction("Index", "Product");

        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewBag.cat = _context.categories.ToList();
            ViewBag.style = _context.categoriestyle.ToList();
            ViewBag.sub = _context.subcategories.ToList();
            ViewBag.gender = _context.productgenders.ToList();
            ViewBag.material = _context.productMaterials.ToList();
            ViewBag.season = _context.productSeasons.ToList();
            ViewBag.status = _context.statuses.ToList();
            var product = _context.product.Where(p => p.productId == id).FirstOrDefault();
            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Product model, IFormFile file)
        {
            ViewBag.cat = _context.categories.ToList();
            ViewBag.style = _context.categoriestyle.ToList();
            ViewBag.sub = _context.subcategories.ToList();
            ViewBag.gender = _context.productgenders.ToList();
            ViewBag.material = _context.productMaterials.ToList();
            ViewBag.season = _context.productSeasons.ToList();
            ViewBag.status = _context.statuses.ToList();
            var product = _context.product.FirstOrDefault(p => p.productId == model.productId);
            if (product != null)
            {
                if (product.productName != model.productName)
                {
                    var isUnique = !_context.product.Any(p => p.productName == model.productName);
                    if (!isUnique)
                    {
                        ModelState.AddModelError("productName", "productName name must be unique");
                        return View(model);
                    }
                }

                product.productName = model.productName;
                product.productcod = model.productcod;
                product.title = model.title;
                product.description = model.description;
                product.sizeguideimage = model.sizeguideimage;
                product.categorystyleid = model.categorystyleid;
                product.price = model.price;
                product.Keyword = model.Keyword;
                product.categoryId = model.categoryId;
                product.subcategoryId = model.subcategoryId;
                product.productmaterialId = model.productmaterialId;
                product.productseasonId = model.productseasonId;
                product.productgenderId = model.productgenderId;
                product.StatusId = model.StatusId;
                product.Lastmodifield = DateTime.Now;
                product.CreateDate = DateTime.Now;
         
                var uniqueFileName = $"{model.productId}.jpg";

                var imageDirectory = Path.Combine(hostEnvironment.WebRootPath, "Image/ProductLogo");
                var filename = "Image/ProductLogo/" + uniqueFileName;
                var fullImagePath = Path.Combine(imageDirectory, uniqueFileName);
                if (!Directory.Exists(imageDirectory))
                {
                    Directory.CreateDirectory(imageDirectory);
                }

                using (var stream = new FileStream(fullImagePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);

                }
                product.productimage = filename;
                _context.product.Update(product);
                _context.SaveChanges();
                return RedirectToAction("Index", "Product");

            }
            return View(model);
        }
        public IActionResult Delete(int id)
        {
            var data = _context.product.Find(id);
            var CurrentImage = Path.Combine(hostEnvironment.WebRootPath, data.productimage);
            if (System.IO.File.Exists(CurrentImage))
            {
                System.IO.File.Delete(CurrentImage);
            };
            _context.product.Remove(data);
            _context.SaveChanges();
            return RedirectToAction("Index", "Product");
        }

    }
}
