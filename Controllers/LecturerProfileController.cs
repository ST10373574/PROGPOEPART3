using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prog2bPOEPart2.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Prog2bPOEPart2.Controllers
{
    public class LecturerProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // Constructor
        public LecturerProfileController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: LecturerProfile/Index
        public async Task<IActionResult> Index()
        {
            // Get all users
            var users = _userManager.Users.ToList();

            var lecturerProfiles = new List<LecturerProfileViewModel>();

            foreach (var user in users)
            {
                // Get roles for the user
                var roles = await _userManager.GetRolesAsync(user);

                // Check if the user has the "Lecturer" role
                if (roles.Contains("Lecturer"))
                {
                    // Add to the lecturer profiles list if they are a lecturer
                    lecturerProfiles.Add(new LecturerProfileViewModel
                    {
                        UserID = user.Id,
                        Name = user.UserName,
                        Email = user.Email,
                        Role = "Lecturer"  // This assumes all users in this list are lecturers
                    });
                }
            }

            return View(lecturerProfiles);
        }

        // GET: LecturerProfile/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Construct LecturerProfileViewModel for the lecturer profile
            var lecturerModel = new LecturerProfileViewModel
            {
                UserID = user.Id,
                Name = user.UserName,  // Username as the lecturer's name
                Email = user.Email,
                Role = "Lecturer"  // Assuming the user is a lecturer
            };

            return View(lecturerModel);
        }

        // GET: LecturerProfile/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Populate LecturerProfileViewModel with current user info
            var lecturerModel = new LecturerProfileViewModel
            {
                UserID = user.Id,
                Name = user.UserName,
                Email = user.Email,
                Role = "Lecturer"
            };

            return View(lecturerModel);
        }

        // POST: LecturerProfile/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LecturerProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserID);
                if (user == null)
                {
                    return NotFound();
                }

                // Update lecturer details (email, username)
                user.UserName = model.Name;
                user.Email = model.Email;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Details), new { id = model.UserID });
                }
                else
                {
                    // Handle errors (e.g., invalid email format)
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }
    }
}
