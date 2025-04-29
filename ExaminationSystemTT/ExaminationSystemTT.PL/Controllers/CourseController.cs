using ExaminationSystemTT.BLL.Interfaces; // For ICourseRepository
using ExaminationSystemTT.DAL.Models;     // For Course model
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System; // For Exception handling (optional)

namespace ExaminationSystemTT.PL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CourseController : Controller
    {
        private readonly ICourseRepository _courseRepository;

        public CourseController(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        // GET: Course  or  Course/Index
        public IActionResult Index()
        {
            var courses = _courseRepository.GetAll();
            return View(courses); // Pass the list of courses to the Index view
        }

        // GET: Course/Details/5
        public IActionResult Details(int? id) // Make id nullable for validation
        {
            if (id == null)
            {
                return BadRequest(); // Or NotFound()
            }

            var course = _courseRepository.GetByID(id.Value); // Use id.Value since we checked for null

            if (course == null)
            {
                return NotFound(); // Course with the given ID doesn't exist
            }

            return View(course); // Pass the found course to the Details view
        }

        // GET: Course/Create
        public IActionResult Create()
        {
            // Just display the empty form
            return View();
        }

        // POST: Course/Create
        [HttpPost]
        [ValidateAntiForgeryToken] // Protect against CSRF attacks
        public IActionResult Create(Course course)
        {
            // Check if the submitted data is valid based on model annotations ([Required], [StringLength], etc.)
            if (ModelState.IsValid)
            {
                try
                {
                    int result = _courseRepository.Add(course);
                    if (result > 0)
                    {
                        return RedirectToAction(nameof(Index)); // Redirect to the list after successful creation
                    }
                    else
                    {
                        // If Add returns 0, SaveChanges didn't affect any rows (shouldn't happen on Add unless concurrency issue)
                        ModelState.AddModelError(string.Empty, "Failed to create the course.");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception ex
                    ModelState.AddModelError(string.Empty, $"An error occurred while creating the course: {ex.Message}");
                }
            }

            // If ModelState is invalid or save failed, return the view with the submitted data to show errors
            return View(course);
        }

        // GET: Course/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var course = _courseRepository.GetByID(id.Value);

            if (course == null)
            {
                return NotFound();
            }

            return View(course); // Pass the course to the Edit view form
        }

        // POST: Course/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Course course) // id comes from the route, course from the form body
        {
            // Security check: Ensure the ID in the route matches the ID in the submitted model
            if (id != course.CourseId)
            {
                return NotFound(); // Or BadRequest()
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int result = _courseRepository.Update(course);
                    if (result > 0)
                    {
                        return RedirectToAction(nameof(Index)); // Redirect to list after successful update
                    }
                    else
                    {
                        // Update might return 0 if no actual changes were detected by EF or if row wasn't found (less likely with prior GetByID)
                        ModelState.AddModelError(string.Empty, "Failed to update the course. It might not exist or values were unchanged.");
                    }
                }
                catch (Exception ex) // Catch potential DbUpdateConcurrencyException etc.
                {
                    // Log the exception ex
                    ModelState.AddModelError(string.Empty, $"An error occurred while updating the course: {ex.Message}");
                }
            }

            // If ModelState is invalid or update failed, return the view with the data
            return View(course);
        }


        // GET: Course/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var course = _courseRepository.GetByID(id.Value);

            if (course == null)
            {
                return NotFound();
            }

            return View(course); // Show the confirmation page
        }

        // POST: Course/Delete/5
        [HttpPost, ActionName("Delete")] // Maps this POST action to the same URL as the GET Delete, but only for POST requests
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var course = _courseRepository.GetByID(id);

            if (course == null)
            {
                // Optionally handle case where course was deleted between GET and POST
                // return NotFound(); or just redirect to index
                TempData["ErrorMessage"] = "Course not found or already deleted.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                int result = _courseRepository.Delete(course);
                if (result < 0) 
                {
                    // This could happen if the row was deleted by another process just before SaveChanges
                    TempData["ErrorMessage"] = "Failed to delete the course. It might have been deleted already.";
                }
            }
            catch (Exception ex) // Catch potential issues, e.g., FK constraints if not handled by DB cascade rules
            {
                // Log the exception ex
                TempData["ErrorMessage"] = $"An error occurred while deleting the course: {ex.Message}";
            }


            return RedirectToAction(nameof(Index)); // Redirect to the list after attempting delete
        }
    }
}