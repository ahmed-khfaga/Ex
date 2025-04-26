using ExaminationSystem.BLL.Interfaces;
using ExaminationSystem.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Needed for DbUpdateConcurrencyException
using Microsoft.Extensions.Logging; // Optional: For logging errors
using System;
using System.Linq; // Needed for Enumerable.Empty

namespace ExaminationSystem.PL.Controllers
{
    public class ExamController : Controller
    {
        private readonly IExamRepository _examRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly ILogger<ExamController> _logger; // Optional: For logging

        // Constructor Injection (Ensure ILogger is registered in Program.cs if used)
        public ExamController(
            IExamRepository examRepository,
            ICourseRepository courseRepository,
            IInstructorRepository instructorRepository,
            ILogger<ExamController> logger) // Inject logger
        {
            _examRepository = examRepository ?? throw new ArgumentNullException(nameof(examRepository));
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _instructorRepository = instructorRepository ?? throw new ArgumentNullException(nameof(instructorRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Helper method to populate dropdown data
        private void PopulateDropdowns(int? selectedCourse = null, int? selectedInstructor = null)
        {
            try
            {
                var courses = _courseRepository.GetAll() ?? Enumerable.Empty<Course>();
                var instructors = _instructorRepository.GetAll() ?? Enumerable.Empty<Instructor>();

                // Use the correct property names based on your models
                ViewBag.CourseId = new SelectList(courses, "ID", "Name", selectedCourse);
                ViewBag.InstructorId = new SelectList(instructors, "ID", "FullName", selectedInstructor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating dropdowns for Exam Create/Edit.");
                ModelState.AddModelError(string.Empty, "An error occurred loading selection data.");
                ViewBag.CourseId = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
                ViewBag.InstructorId = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
            }
        }

        // GET: Exam/Index
        public IActionResult Index()
        {
            try
            {
                // Assumes GetAll includes Course and Instructor (as implemented previously)
                var exams = _examRepository.GetAll();
                return View(exams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exams for Index.");
                TempData["ErrorMessage"] = "Could not load exams.";
                return View(Enumerable.Empty<Exam>());
            }
        }

        // GET: Exam/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details action called with null ID.");
                return BadRequest(); // Or NotFound() - BadRequest indicates client error
            }

            try
            {
                // Assumes GetByID includes Course and Instructor (as implemented previously)
                var exam = _examRepository.GetByID(id.Value);
                if (exam == null)
                {
                    _logger.LogWarning("Exam not found for Details with ID: {ExamId}", id.Value);
                    return NotFound();
                }
                return View(exam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exam details for ID: {ExamId}", id.Value);
                TempData["ErrorMessage"] = "Could not load exam details.";
                return RedirectToAction(nameof(Index)); // Redirect on error
            }
        }


        // GET: Exam/Create
        [HttpGet]
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View(new Exam());
        }

        // POST: Exam/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Exam exam)
        {
            // Ensure navigation properties don't cause validation issues
            // (Best handled with [ValidateNever] on the model properties)
            if (ModelState.IsValid)
            {
                try
                {
                    int result = _examRepository.Add(exam);
                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Exam created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to save the exam.");
                        _logger.LogWarning("ExamRepository.Add returned 0 for Exam Title: {ExamTitle}", exam.Title);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding Exam Title: {ExamTitle}", exam.Title);
                    ModelState.AddModelError(string.Empty, $"An error occurred saving the exam: {ex.Message}");
                }
            }

            // If we get here, something failed—repopulate dropdowns
            _logger.LogInformation("ModelState invalid or save error for Exam Create. Repopulating dropdowns.");
            PopulateDropdowns(exam.CourseID, exam.InstructorID);
            return View(exam);
        }

        // GET: Exam/Edit/5
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit GET action called with null ID.");
                return BadRequest();
            }

            try
            {
                // GetByID likely already includes Course/Instructor if Details/Index works
                var exam = _examRepository.GetByID(id.Value);
                if (exam == null)
                {
                    _logger.LogWarning("Exam not found for Edit GET with ID: {ExamId}", id.Value);
                    return NotFound();
                }
                PopulateDropdowns(exam.CourseID, exam.InstructorID); // Populate dropdowns with current selections
                return View(exam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exam for Edit GET with ID: {ExamId}", id.Value);
                TempData["ErrorMessage"] = "Could not load exam for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Exam/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Exam exam) // id from route, exam from form binding
        {
            // Basic check: Route ID must match the ID in the submitted model
            if (id != exam.ID)
            {
                _logger.LogWarning("Mismatch between route ID ({RouteId}) and form ID ({FormId}) during Exam Edit POST.", id, exam.ID);
                return BadRequest("ID mismatch.");
            }

            // Ensure navigation properties don't cause validation issues
            if (ModelState.IsValid)
            {
                try
                {
                    int result = _examRepository.Update(exam);
                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Exam updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        // This might happen if Update returns 0 rows affected (e.g., no actual change)
                        // or if there was an undetected issue.
                        ModelState.AddModelError(string.Empty, "Failed to update the exam. No changes might have been detected.");
                        _logger.LogWarning("ExamRepository.Update returned 0 for Exam ID: {ExamId}", exam.ID);
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // Handle concurrency conflict (someone else edited the record)
                    _logger.LogWarning(ex, "Concurrency conflict occurred while updating Exam ID: {ExamId}", exam.ID);
                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                        + "was modified by another user after you got the original value. The "
                        + "edit operation was canceled. If you still want to edit this record, "
                        + "please go back to the list and try again.");
                    // Optional: Could load database values and show conflicts to user
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating Exam ID: {ExamId}", exam.ID);
                    ModelState.AddModelError(string.Empty, $"An error occurred updating the exam: {ex.Message}");
                }
            }
            else // ModelState is invalid
            {
                _logger.LogInformation("ModelState invalid for Exam Edit POST for ID: {ExamId}. Repopulating dropdowns.", exam.ID);
            }


            // If we reach here, ModelState was invalid OR an error occurred during update.
            // Redisplay the form with current data and errors.
            PopulateDropdowns(exam.CourseID, exam.InstructorID);
            return View(exam);
        }

        // GET: Exam/Delete/5
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete GET action called with null ID.");
                return BadRequest();
            }

            try
            {
                // GetByID likely already includes Course/Instructor if Details/Index works
                var exam = _examRepository.GetByID(id.Value);
                if (exam == null)
                {
                    _logger.LogWarning("Exam not found for Delete GET with ID: {ExamId}", id.Value);
                    return NotFound();
                }
                return View(exam); // Show the confirmation view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exam for Delete GET with ID: {ExamId}", id.Value);
                TempData["ErrorMessage"] = "Could not load exam for deletion confirmation.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Exam/Delete/5
        [HttpPost, ActionName("Delete")] // Maps POST requests for /Exam/Delete/5
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id) // Only need the ID for deletion
        {
            try
            {
                // Get the entity again before attempting to delete
                var exam = _examRepository.GetByID(id);
                if (exam == null)
                {
                    // Could have been deleted between GET and POST
                    _logger.LogWarning("Exam not found for Delete POST (already deleted?) with ID: {ExamId}", id);
                    TempData["ErrorMessage"] = "Exam not found, it might have already been deleted.";
                    return RedirectToAction(nameof(Index)); // Or return NotFound()
                }

                int result = _examRepository.Delete(exam);
                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Exam deleted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Delete returned 0, maybe DB trigger prevented it? Or issue in repo?
                    _logger.LogWarning("ExamRepository.Delete returned 0 for Exam ID: {ExamId}", id);
                    ModelState.AddModelError(string.Empty, "The exam could not be deleted. Please try again.");
                    // Return the confirmation view again with the error
                    return View(exam);
                }

            }
            catch (DbUpdateException ex) // Catch specific DB errors like foreign key violations
            {
                _logger.LogError(ex, "Database error occurred while deleting Exam ID: {ExamId}", id);
                // Provide a more user-friendly message if possible (check inner exception?)
                // Example: Check if it's a foreign key constraint violation
                if (ex.InnerException?.Message.Contains("REFERENCE constraint") ?? false)
                {
                    ModelState.AddModelError(string.Empty, "This exam cannot be deleted because it has related questions or submissions.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"A database error occurred: {ex.InnerException?.Message ?? ex.Message}");
                }

                // Need to get the exam again to display the view with error
                var examWithError = _examRepository.GetByID(id); // Or handle if this fails too
                return View(examWithError ?? new Exam { ID = id }); // Return confirmation view with error
            }
            catch (Exception ex) // Catch any other unexpected errors
            {
                _logger.LogError(ex, "Error deleting Exam ID: {ExamId}", id);
                ModelState.AddModelError(string.Empty, $"An error occurred deleting the exam: {ex.Message}");

                var examWithError = _examRepository.GetByID(id);
                return View(examWithError ?? new Exam { ID = id }); // Return confirmation view with error
            }
        }
    }
}