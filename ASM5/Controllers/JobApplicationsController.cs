using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASM5.Data;
using ASM5.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace ASM5.Controllers
{
    public class JobApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobApplicationsController(ApplicationDbContext context)
        {
            _context = context;
        }


        //public IActionResult Apply(int jobListingId)
        //{
        //    // Tạo một đối tượng JobApplication mới với JobListId được truyền vào từ parameter
        //    var jobApplication = new JobApplication
        //    {
        //        JobListId = jobListingId
        //    };

        //    // Trả về view "Create" và truyền vào đối tượng JobApplication
        //    return View("Create", jobApplication);

        //}

            // GET: JobApplications
            //[Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Index(string position)
        {
            IQueryable<JobApplication> applicationDbContext;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //sử dụng để lấy ID của người dùng hiện tại đang đăng nhập vào hệ thống
            var user = _context.Users.Find(userId); //sử dụng ID người dùng để truy vấn cơ sở dữ liệu và lấy ra thông tin của người dùng tương ứng

            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            if (user.UserRole == "JobSeeker") // Kiểm tra người đó có phải ng xin việc không 
            {
                applicationDbContext = _context.JobApplication
                .Where(j => j.UserId == userId)
                .Include(j => j.User)
                .Include(j => j.JobList);


                if (!string.IsNullOrEmpty(position))
                {
                    applicationDbContext = applicationDbContext.Where(j => j.JobList.Vacancies != null && j.JobList.Vacancies.Contains(position));
                }

                return View(await applicationDbContext.ToListAsync());
              


                //return View(await jobs.ToListAsync());
            }
            else
            {
                IQueryable<JobApplication> employerJobApplications = _context.JobApplication
            .Where(ja => ja.JobList.UserId == userId) // Lọc theo UserId của Employer
            .Include(ja => ja.User)
            .Include(ja => ja.JobList);

                return View(await employerJobApplications.ToListAsync());
            }
            return View(await applicationDbContext.ToListAsync());
        }





        // GET: JobApplications/Details/5
        //[Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.JobApplication == null)
            {
                return NotFound();
            }

            var jobApplication = await _context.JobApplication
                .Include(j => j.JobList)
                .Include(j => j.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (jobApplication == null)
            {
                return NotFound();
            }

            return View(jobApplication);
        }

        // GET: JobApplications/Create
        [Authorize(Roles = "JobSeeker")]
        public IActionResult Create(int jobListingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //sử dụng để lấy ID của người dùng hiện tại đang đăng nhập vào hệ thống
            var user = _context.Users.Find(userId); //sử dụng ID người dùng để truy vấn cơ sở dữ liệu và lấy ra thông tin của người dùng tương ứng
            if (user.UserRole == "JobSeeker") // Kiểm tra người đó có phải ng xin việc không 
            {
                var jobListing = _context.JobList.FirstOrDefault(j => j.Id == jobListingId);
                if (jobListing != null)
                {
                    var application = new JobApplication
                    {
                        UserId = userId,
                        JobListId = jobListingId,
                        JobList = jobListing
                    };
                    ViewData["JobListId"] = new SelectList(_context.Set<JobList>(), "Id", "Id", jobListingId);
                    ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", userId);
                    return View(application);

                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {

                return RedirectToAction("Index", "Home");
            }

        }

        // POST: JobApplications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Experience,JobListId,UserId")] JobApplication jobApplication)
        {
            if (ModelState.IsValid)
            {
                _context.Add(jobApplication);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["JobListId"] = new SelectList(_context.Set<JobList>(), "Id", "Id", jobApplication.JobListId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", jobApplication.UserId);
            return View(jobApplication);
        }

        // GET: JobApplications/Edit/5
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.JobApplication == null)
            {
                return NotFound();
            }

            var jobApplication = await _context.JobApplication.Include(ja => ja.JobList).FirstOrDefaultAsync(ja => ja.Id == id);
            if (jobApplication == null)
            {
                return NotFound();
            }
            ViewData["JobListId"] = new SelectList(_context.JobList, "Id", "Id", jobApplication.JobListId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", jobApplication.UserId);
            return View(jobApplication);
        }

        // POST: JobApplications1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Experience,JobListId,UserId")] JobApplication jobApplication)
        {
            if (id != jobApplication.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jobApplication);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobApplicationExists(jobApplication.Id))
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
            ViewData["JobListId"] = new SelectList(_context.JobList, "Id", "Id", jobApplication.JobListId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", jobApplication.UserId);
            return View(jobApplication);
        }

        // GET: JobApplications1/Delete/5
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.JobApplication == null)
            {
                return NotFound();
            }

            var jobApplication = await _context.JobApplication
                .Include(j => j.JobList)
                .Include(j => j.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (jobApplication == null)
            {
                return NotFound();
            }

            return View(jobApplication);
        }

        // POST: JobApplications1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.JobApplication == null)
            {
                return Problem("Entity set 'ApplicationDbContext.JobApplication'  is null.");
            }
            var jobApplication = await _context.JobApplication.FindAsync(id);
            if (jobApplication != null)
            {
                _context.JobApplication.Remove(jobApplication);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JobApplicationExists(int id)
        {
            return (_context.JobApplication?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
