using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Theme_Implemenet.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Humanizer;
using System.Xml.Schema;
using Microsoft.AspNetCore.Localization;
using System.Net;
using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;

namespace Theme_Implemenet.Controllers
{
    public class UserController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;



        public UserController(MyDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            this.hostEnvironment = hostEnvironment;
            _httpContextAccessor = httpContextAccessor; 

        }
        public IActionResult Index()
        {
            ViewData["HttpContext"] = HttpContext;

            List<ShoppingCarts> cartItems = HttpContext.Session.GetObject<List<ShoppingCarts>>("Cart") ?? new List<ShoppingCarts>();

            // Calculate the count of cart items
            int cartItemCount = cartItems.Count;

            // Store the count in ViewBag
            ViewBag.CartItemCount = cartItemCount;

            ViewBag.cat = _context.categories.ToList();
            var product = _context.product.ToList();

            return View(product);
        }
        public IActionResult ProductDetails(int id)
        {
            
            var product = _context.productVeriations.Include(p => p.Product).Where(p => p.productId == id).ToList();
            ViewBag.size = _context.productSizes.ToList();
            ViewBag.color = _context.productColors.ToList();
            ViewBag.ProductName = _context.product.ToList();


            var productName = _context.product
                        .Where(p => p.productId == id)
                        .Select(p => p.productName)
                        .FirstOrDefault();

            ViewBag.ProductName = productName;

            var price = _context.product
                        .Where(p => p.productId == id)
                        .Select(p => p.price)
                        .FirstOrDefault();

            ViewBag.price = price;
            int? veriationId = product.FirstOrDefault()?.veriationId;

            // Pass the veriationId to the view
            ViewBag.veriationId = veriationId;
           

            return View(product);
        }
        [HttpPost]
        public IActionResult ProductDetails(int id,int qty,string color,string size)
        {
            //List<ShoppingCarts> cartList = TempData["cat"] != null ? JsonConvert.DeserializeObject<List<ShoppingCarts>>(TempData["cat"].ToString()) : new List<ShoppingCarts>();

            //var products = _context.productVeriations.Include(p => p.Product).Where(p => p.productId == id).ToList();

            ProductVeriation p = _context.productVeriations.Include(p=>p.Product).Where(p => p.veriationId == id).FirstOrDefault();

            ShoppingCarts c = new ShoppingCarts();
            c.veriationId=id;
            c.Price = p.costprice;
            c.Size = size;
            c.color = color;
            c.image = p.image;
            c.Quantity = Convert.ToInt32(qty);
            c.bill = c.Price * c.Quantity;
            List<ShoppingCarts> cartItems = HttpContext.Session.GetObject<List<ShoppingCarts>>("Cart") ?? new List<ShoppingCarts>();

            // Add the new item to the cart
            cartItems.Add(c);

            // Update the session with the updated cart items
            HttpContext.Session.SetObject("Cart", cartItems);
            return RedirectToAction("Index","User");
        }

        public IActionResult CheckOut()
        {
            //total price count
            decimal totalBill = 0;
            if (HttpContext.Session.GetObject<List<ShoppingCarts>>("Cart") != null)
            {
                List<ShoppingCarts> cartItem = HttpContext.Session.GetObject<List<ShoppingCarts>>("Cart");

                // Calculate total bill
                foreach (var item in cartItem)
                {
                    totalBill += item.bill;
                }
            }
            ViewBag.TotalBill = totalBill;
            //Show basket item
            List<ShoppingCarts> cart = HttpContext.Session.GetObject<List<ShoppingCarts>>("Cart") ?? new List<ShoppingCarts>();

            // Calculate the count of cart items
            int cartItemCount = cart.Count;

            // Store the count in ViewBag
            ViewBag.CartItemCount = cartItemCount;

            //Show card data
            var product = _context.productVeriations.Include(p => p.Product).ToList();
            List<ShoppingCarts> cartItems = HttpContext.Session.GetObject<List<ShoppingCarts>>("Cart") ?? new List<ShoppingCarts>();
            return View(cartItems);
        }
        public IActionResult Remove(int id)
        {
            if (HttpContext.Session.GetObject<List<ShoppingCarts>>("Cart") == null)
            {
                ViewBag.Remove("TotalBill");

            }
            else
            {
                List<ShoppingCarts> cartItem = HttpContext.Session.GetObject<List<ShoppingCarts>>("Cart");
                ShoppingCarts c= cartItem.Where(p=>p.Id==id).FirstOrDefault();
                cartItem.Remove(c);
                decimal s = 0;
                foreach (var item in cartItem)
                {
                    s -= item.bill;
                }
                ViewBag.TotalBill= s;
            }
            return RedirectToAction("Index","User");
        }
        public IActionResult ProcessCheck()
        {
            var payment = _context.paymentTypes.ToList();
            ViewBag.payment = payment;
            return View();
        }
        [HttpPost]
        public IActionResult ProcessCheck(string name, string email, string password, string number, string address, int PaymentTypeId)
        {
            var payment = _context.paymentTypes.ToList();
            ViewBag.Payment = payment;
           
            //var customer = !_context.customers.Any(p => p.CustName == name);
            //if (!customer)
            //{
            //    ModelState.AddModelError("CustName", "name must be unique");
            //    return View();
            //}

            List<ShoppingCarts> cart = HttpContext.Session.GetObject<List<ShoppingCarts>>("Cart") ?? new List<ShoppingCarts>();
            Customer cust = new Customer();
            cust.CustName = name;
            cust.CustEmail = email;
            cust.Password = password;
            cust.CustNumber = number;
            cust.Address = address;
            cust.CreateDate = DateTime.Now;
            cust.LastModified = DateTime.Now;
            _context.customers.Add(cust);
            _context.SaveChanges();
            foreach (var item in cart)
            {
                _context.ShoppingCarts.Add(item);
                Order od = new Order();
                od.veriationId = item.veriationId;
                od.CustId = cust.CustId;
                od.quantity = item.Quantity;
                od.subtotal = item.bill;
                od.shipping = 300;
                od.totalamount = item.bill+od.shipping;
                od.StatusId = 1;
                od.CreateDate = DateTime.Now;
                od.Lastmodifield = DateTime.Now;
                _context.Orders.Add(od);
                _context.SaveChanges();
                Payment pay = new Payment();
                pay.OrderId = od.OrderId;
                pay.CustId = cust.CustId;
                pay.totalamount = od.totalamount;
                pay.PaymentTypeId = PaymentTypeId;
                pay.CreateDate = DateTime.Now;
                pay.Lastmodifield = DateTime.Now;
                _context.payments.Add(pay);
                _context.SaveChanges();

            }
            _context.SaveChanges();
            SendOrderConfirmationEmail(cust, cart);

            return RedirectToAction("Index", "User");
           
        }
        private void SendOrderConfirmationEmail(Customer customer, List<ShoppingCarts> cart)
        {
            string emailBody = $"Dear {customer.CustName},\n\n";
            emailBody += "Thank you for your order!\n\n";
            emailBody += "Below are the details of your order:\n\n";

            foreach (var item in cart)
            {
                emailBody += $"Item: {item.veriationId}, Quantity: {item.Quantity}, Total: {item.bill}\n";
            }

            emailBody += "\n\nRegards,\nYour Company";

            //using (var client = new SmtpClient("smtp.gmail.com"))
            //{
            //    client.Port = 587;
            //    client.Credentials = new NetworkCredential(customer.CustEmail,customer.Password);
            //    client.EnableSsl = true;

            //    var mailMessage = new MailMessage
            //    {
            //        From = new MailAddress(customer.CustEmail),
            //        Subject = "Order Confirmation",
            //        Body = emailBody
            //    };

            //    mailMessage.To.Add(customer.CustEmail);
            //    client.Send(mailMessage);
            //}
            using (MailMessage mail = new MailMessage())
            {
               mail.From = new MailAddress("email@gmail.com");
                mail.To.Add(customer.CustEmail);
                mail.Subject = "Order Confirmation";
                mail.Body = emailBody;
                mail.IsBodyHtml = true;
                //mail.Attachments.Add(new Attachment("C:\\file.zip"));

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = "smtp.gmail.com";
                    smtp.EnableSsl = true;
                    NetworkCredential Credentials = new NetworkCredential(customer.CustEmail,customer.Password);
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials= Credentials;
                    smtp.Port = 587;
                    smtp.Send(mail);
                }
            }

        }
        public IActionResult Login(int id)
        {
            ViewBag.UserId = id; // Pass the id to the view

            var payment = _context.paymentTypes.ToList();
            ViewBag.Payment = payment;
            return View();
        }
        [HttpPost]
        public IActionResult Login(string email, string password, int PaymentTypeId)
        {

            var payment = _context.paymentTypes.ToList();
            ViewBag.Payment = payment;

            var user = _context.customers.FirstOrDefault(u => u.CustEmail == email && u.Password == password);

            if (user != null)
            {
                int custId = user.CustId; 

                List<ShoppingCarts> cart = HttpContext.Session.GetObject<List<ShoppingCarts>>("Cart") ?? new List<ShoppingCarts>();
                foreach (var item in cart)
                {
                    _context.ShoppingCarts.Add(item);
                    Order od = new Order();
                    od.veriationId = item.veriationId;
                    od.CustId = custId;
                    od.quantity = item.Quantity;
                    od.subtotal = item.bill;
                    od.shipping = 300;
                    od.totalamount = item.bill + od.shipping; 
                    od.StatusId = 1;
                    od.CreateDate = DateTime.Now;
                    od.Lastmodifield = DateTime.Now;
                    _context.Orders.Add(od);
                    _context.SaveChanges();
                    Payment pay = new Payment();
                    pay.OrderId = od.OrderId;
                    pay.CustId = custId;
                    pay.totalamount = od.totalamount;
                    pay.PaymentTypeId = PaymentTypeId;
                    pay.CreateDate = DateTime.Now;
                    pay.Lastmodifield = DateTime.Now;
                    _context.payments.Add(pay);
                    _context.SaveChanges();
                    //SendOrderConfirmationEmail(user, cart);

                    return RedirectToAction("Index", "User");
                }
            }
            else
            {
                // Authentication failed
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View();
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            return View();
        }
        //public IActionResult YourAction()
        //{
        //    var httpContext = _httpContextAccessor.HttpContext;
        //    var userEmail = httpContext.Session.GetString("CustEmail");

        //    var isAuthenticated = !string.IsNullOrEmpty(userEmail);

        //    return View(isAuthenticated ? userEmail : null);
        //}
        public IActionResult Emailset()
        {
            // Retrieve the logged-in user's email
            string userEmail = User.Identity.Name;

            // Query the Customer table to get the customer with the matching email
            Customer customer = _context.customers.FirstOrDefault(c => c.CustEmail == userEmail);

            //if (customer != null)
            //{
            //    Console.WriteLine("Customer found:");
            //    Console.WriteLine($"Customer ID: {customer.CustId}");
            //    Console.WriteLine($"Customer Email: {customer.CustEmail}");
            //}
            //else
            //{
            //    Console.WriteLine("No customer found.");
            //}

            // Pass the email to the view
            return View(customer.CustEmail);
        }

    }
}

