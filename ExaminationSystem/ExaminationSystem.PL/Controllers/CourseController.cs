using ExaminationSystem.BLL.Interfaces;
using ExaminationSystem.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExaminationSystem.PL.Controllers
{
    public class CourseController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorRepository _instructorRepository; // Needed for Instructor dropdown
        public CourseController(ICourseRepository courseRepository, IInstructorRepository instructorRepository)
        {
            _courseRepository = courseRepository;
            _instructorRepository = instructorRepository;
        }
        private void PopulateInstructorDropdown(object selectedInstructor = null)
        {
            var instructors = _instructorRepository.GetAll()
                .Select(i => new SelectListItem
                {
                    Value = i.ID.ToString(),
                    Text = $"{i.FirstName} {i.LastName}",
                    Selected = (selectedInstructor != null && selectedInstructor.ToString() == i.ID.ToString())
                }).ToList();

            ViewBag.InstructorList = instructors;
        }

        public IActionResult Index()
        {
            // Get all courses
            var courses = _courseRepository.GetAll();

            // Loop through courses and populate Instructor Name for each course
            foreach (var course in courses)
            {
                var instructor = _instructorRepository.GetByID(course.InstructorID);
                course.Instructor = instructor; // Add instructor object to course (populating the Instructor navigation property)
            }

            return View(courses);
        }


        [HttpGet]
        public IActionResult Create()
        {
            PopulateInstructorDropdown();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Name,InstructorID,CreationDate")] Course course)
        {
            ModelState.Remove("Instructor");
            ModelState.Remove("Enrollments");
            ModelState.Remove("Exams");

            if (ModelState.IsValid)
            {
                try
                {
                    var count = _courseRepository.Add(course);
                    if (count > 0)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError(string.Empty, "Failed to create the course.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred while creating the course: {ex.Message}");
                }
            }

            PopulateInstructorDropdown(course.InstructorID);
            return View(course);
        }

        [HttpGet]
        public IActionResult Details(int? id) // Course ID is long
        {
            if (!id.HasValue)
            {
                return BadRequest("Course ID is required.");
            }

            var course = _courseRepository.GetByID(id.Value);
            if (course == null)
            {
                return NotFound($"Course with ID {id.Value} not found.");
            }

            // Check if InstructorID is valid
            if (course.InstructorID != 0)
            {
                var instructor = _instructorRepository.GetByID(course.InstructorID);
                ViewBag.InstructorName = instructor != null ? $"{instructor.FirstName} {instructor.LastName}" : "Instructor not found";
            }
            else
            {
                ViewBag.InstructorName = "Instructor not assigned";
            }

            return View(course);
        }


        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return BadRequest("Course ID is required.");
            }

            var course = _courseRepository.GetByID(id.Value);
            if (course == null)
            {
                return NotFound($"Course with ID {id.Value} not found.");
            }

            PopulateInstructorDropdown(course.InstructorID);
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("ID,Name,InstructorID,CreationDate")] Course course)
        {
            if (id != course.ID)
            {
                return BadRequest("Course ID mismatch.");
            }

            // Remove navigation properties from model validation
            ModelState.Remove("Instructor");
            ModelState.Remove("Enrollments");
            ModelState.Remove("Exams");

            if (ModelState.IsValid)
            {
                try
                {
                    _courseRepository.Update(course); // Save the updated course
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred while updating the course: {ex.Message}");
                }
            }

            // If update fails or model is invalid, reload dropdown and return to view
            PopulateInstructorDropdown(course.InstructorID);
            return View(course);
        }








        [HttpGet]
        public IActionResult Delete(int? id) // Course ID is long
        {
            if (!id.HasValue)
            {
                return BadRequest("Course ID is required.");
            }
            var course = _courseRepository.GetByID(id.Value); // Assumes GetByID handles long
            if (course == null)
            {
                return NotFound($"Course with ID {id.Value} not found.");
            }
            // Optionally load Instructor name for display on confirmation page
            // var instructor = _instructorRepository.GetByID(course.InstructorID);
            // ViewBag.InstructorName = instructor != null ? $"{instructor.FirstName} {instructor.LastName}" : "N/A";
            return View(course); // Pass course to confirmation view
        }



        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id) // Course ID is long
        {
            // Fetch again before deleting is safer
            var courseToDelete = _courseRepository.GetByID(id);
            if (courseToDelete == null)
            {
                // Already deleted or never existed
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var count = _courseRepository.Delete(courseToDelete); // Synchronous call with SaveChanges inside
                if (count > 0)
                {
                    // Optionally add TempData success message
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Could not delete the course.");

            }
            catch (Exception ex) // Catch potential DB exceptions (e.g., foreign key constraints on Exam/Enrollment)
            {
                // Log the exception
                ModelState.AddModelError(string.Empty, $"An error occurred while deleting the course: {ex.Message}. Ensure no exams or enrollments reference this course.");
                // Return the Delete confirmation view with the error message
                // Repopulate instructor name if needed for the view
                return View("Delete", courseToDelete); // Pass model back to view
            }
            // If deletion failed without exception (count <=0)
            return View("Delete", courseToDelete); // Pass model back to view
        }
    }
}
