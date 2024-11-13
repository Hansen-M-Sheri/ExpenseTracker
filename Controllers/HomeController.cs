using Microsoft.AspNetCore.Mvc;
using SpendSmart.Models;
using System.Diagnostics;

namespace SpendSmart.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly SpendSmartDbContext _context;

        public HomeController(ILogger<HomeController> logger, SpendSmartDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Expenses()
        {
            // want to see all expenses that have been added
            var allExpenses = _context.Expenses.ToList(); //ToList (or ToArray, etc) executes the query

            var totalExpenses = allExpenses.Sum(x => x.Value);

            ViewBag.Expenses = totalExpenses; //ViewBag is like a bag you can put things in and access in the view
            return View(allExpenses);
        }
        public IActionResult CreateEditExpense(int? id)
        {
            if(id != null)
            {
                //editing -> load an expense by id
                var expenseInDb = _context.Expenses.SingleOrDefault(expense => expense.Id == id);
                return View(expenseInDb);
            }

            return View();
        }

        public IActionResult DeleteExpense(int id)
        {
            var expenseInDb = _context.Expenses.SingleOrDefault(expense => expense.Id == id);
            _ = _context.Expenses.Remove(expenseInDb); // the leading _ will discard the returned value of Remove (the entity removed)
                    // since we are not checking or using it later, we can discard that return entity
            _context.SaveChanges();
            return RedirectToAction("Expenses");
        }
        public IActionResult SubmitCreateEditExpenseForm(Expense model) //the form will send the expense model datatype b/c it is bound to that model 
        {
            if(model.Id == 0)
            {
                // Create
                _context.Expenses.Add(model); //if you change something in db you must save them, not just add

            } else
            {
                // Editing
                _context.Expenses.Update(model);
            }
            
           
            _context.SaveChanges();
            
            return RedirectToAction("Expenses");
        }

        [HttpGet]
        public IActionResult Search(string searchTerm, decimal? minAmount, decimal? maxAmount)
        {
            var expenses = _context.Expenses.AsQueryable();

            //Apply search filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                expenses = expenses.Where(e => e.Description.Contains(searchTerm));
            }
            
            if (minAmount.HasValue)
            {
                expenses = expenses.Where(e => e.Value >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                expenses = expenses.Where(e => e.Value <= maxAmount.Value);
            }

            // Return the partial view with the filtered expenses
            return PartialView("_ExpenseListPartial", expenses.ToList());
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
