using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASM5.Data;
using ASM5.Models;
using ASM5.Areas.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace ASM5.Controllers
{
    public class JobListsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobListsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Apply(int jobListingId)
        {
            return RedirectToAction("Create", "JobApplications", new { jobListingId = jobListingId });
        }

        // GET: JobLists

        //[Authorize(Roles = "Employer")]
        public async Task<IActionResult> Index(string position)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //sử dụng để lấy ID của người dùng hiện tại đang đăng nhập vào hệ thống
            var user = _context.Users.Find(userId); //sử dụng ID người dùng để truy vấn cơ sở dữ liệu và lấy ra thông tin của người dùng tương ứng

            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            if (user.UserRole == "Employer") // Kiểm tra người đó có phải nhà tuyển dụng không 
            {

                IQueryable<JobList> jobs = _context.JobList.Where(j => j.UserId == userId).Include(j => j.User);

                if (!string.IsNullOrEmpty(position))
                {
                    jobs = jobs.Where(j => j.Vacancies != null && j.Vacancies.Contains(position));
                }

                //var applicationDbContext = _context.JobList.Where(j => j.UserId == userId).Include(j => j.User);
                return View(await jobs.ToListAsync());
            }
            else
            {
                //return RedirectToAction("Index", "Home");

                IQueryable<JobList> jobs = _context.JobList.Include(j => j.User);

                if (!string.IsNullOrEmpty(position))
                {
                    jobs = jobs.Where(j => j.Vacancies != null && j.Vacancies.Contains(position));
                }
                return View(jobs);
            }
        }




        // GET: JobLists/Details/5

        //[Authorize(Roles = "Employer")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.JobList == null)
            {
                return NotFound();
            }

            var jobList = await _context.JobList
                .Include(j => j.User)
                .Include(j => j.JobApplication) //lấy thông tin tất cả các đơn cin việc có liên quan đến công việc đó 
                .FirstOrDefaultAsync(m => m.Id == id);
            if (jobList == null)
            {
                return NotFound();
            }

            return View(jobList);
        }






        // GET: JobLists/Create
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //lấy Id của người đang đăng nhập 

            var user = _context.Users.Find(userId); //tìm thông tin của ID đó trong DB

            if (user.UserRole == "Employer")
            {
                var jobList = new JobList
                {
                    UserId = userId
                };
                ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", userId);
                return View(jobList);
            }
            else
            {
                return RedirectToAction("Index", "Home"); // Chuyển hướng đến trang chính của ứng dụng
            }
        }


        // POST: JobLists/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Vacancies,JobDescription,Request,Salary,Time,UserId")] JobList jobList)
        {
            if (ModelState.IsValid)
            {
                _context.Add(jobList);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", jobList.UserId);
            return View(jobList);
        }







        // GET: JobLists/Edit/5

        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.JobList == null)
            {
                return NotFound();
            }

            var jobList = await _context.JobList.FindAsync(id);

            if (jobList == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //lấy Id của người đang đăng nhập 
            var user = _context.Users.Find(userId); //sử dụng ID người dùng để truy vấn cơ sở dữ liệu và lấy ra thông tin của người dùng tương ứng
            if (user.UserRole != "Employer")
            {
                return RedirectToAction("Index", "Home"); // Chuyển hướng đến trang chính của ứng dụng
            }
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", jobList.UserId);
            return View(jobList);
        }

        // POST: JobLists/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Vacancies,JobDescription,Request,Salary,Time,UserId")] JobList jobList)
        {
            if (id != jobList.Id)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //lấy Id của người đang đăng nhập 
            var user = _context.Users.Find(userId); //sử dụng ID người dùng để truy vấn cơ sở dữ liệu và lấy ra thông tin của người dùng tương ứng
            if (user.UserRole != "Employer")
            {
                return RedirectToAction("Index", "Home"); // Chuyển hướng đến trang chính của ứng dụng
            }


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jobList);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobListExists(jobList.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", jobList.UserId);
            return View(jobList);
        }

        // GET: JobLists/Delete/5

        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.JobList == null)
            {
                return NotFound();
            }

            var jobList = await _context.JobList
                .Include(j => j.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (jobList == null)
            {
                return NotFound();
            }

            return View(jobList);
        }

        // POST: JobLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.JobList == null)
            {
                return Problem("Entity set 'ApplicationDbContext.JobList'  is null.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //lấy Id của người đang đăng nhập 
            var user = _context.Users.Find(userId); //sử dụng ID người dùng để truy vấn cơ sở dữ liệu và lấy ra thông tin của người dùng tương ứng
            if (user.UserRole != "Employer")
            {
                return RedirectToAction("Index", "Home"); // Chuyển hướng đến trang chính của ứng dụng
            }

            var jobList = await _context.JobList.FindAsync(id);
            if (jobList != null)
            {
                _context.JobList.Remove(jobList);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JobListExists(int id)
        {
            return (_context.JobList?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
