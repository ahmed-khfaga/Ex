using ExaminationSystemTT.BLL.Interfaces; // For IInstructorRepository
using ExaminationSystemTT.DAL.Models;     // For Instructor model
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System; // For Exception handling

namespace ExaminationSystemTT.PL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InstructorController : Controller
    {
        private readonly IInstructorRepository _instructorRepository;

        public InstructorController(IInstructorRepository instructorRepository)
        {
            _instructorRepository = instructorRepository;
        }

        // GET: Instructor or Instructor/Index
        public IActionResult Index()
        {
            var instructors = _instructorRepository.GetAll();
            return View(instructors); // Pass the list to the Index view
        }

        // GET: Instructor/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var instructor = _instructorRepository.GetByID(id.Value);

            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor); // Pass the found instructor to the Details view
        }

        // GET: Instructor/Create
        public IActionResult Create()
        {
            // Display the empty form
            return View();
        }

        // POST: Instructor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Instructor instructor)
        {
            if (ModelState.IsValid) // Check model annotations ([Required], etc.)
            {
                try
                {
                    int result = _instructorRepository.Add(instructor);
                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Instructor created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to create the instructor.");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception ex
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }
            // If invalid or error, return view with current data
            return View(instructor);
        }

        // GET: Instructor/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var instructor = _instructorRepository.GetByID(id.Value);

            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor); // Pass the instructor to the Edit form
        }

        // POST: Instructor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Instructor instructor)
        {
            if (id != instructor.InstructorId)
            {
                return NotFound(); // Or BadRequest
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int result = _instructorRepository.Update(instructor);
                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Instructor updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        // Update might return 0 if no changes or row not found
                        ModelState.AddModelError(string.Empty, "Failed to update the instructor. It might not exist or values were unchanged.");
                    }
                }
                catch (Exception ex) // Catch potential concurrency issues etc.
                {
                    // Log the exception ex
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }
            // If invalid or error, return view with current data
            return View(instructor);
        }

        // GET: Instructor/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var instructor = _instructorRepository.GetByID(id.Value);

            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor); // Show the confirmation page
        }

        // POST: Instructor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var instructor = _instructorRepository.GetByID(id);

            if (instructor == null)
            {
                TempData["ErrorMessage"] = "Instructor not found or already deleted.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                int result = _instructorRepository.Delete(instructor);
                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Instructor deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete the instructor. It might have been deleted already.";
                }
            }
            catch (Exception ex) // Catch FK constraint issues if exams exist for this instructor
            {
                // Log the exception ex
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}. Ensure the instructor has no associated exams.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}