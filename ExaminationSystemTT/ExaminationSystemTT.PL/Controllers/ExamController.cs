using ExaminationSystemTT.BLL.Interfaces;
using ExaminationSystemTT.DAL; // Assuming Context is here for Include example
using ExaminationSystemTT.DAL.Data;
using ExaminationSystemTT.DAL.Models;
using ExaminationSystemTT.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // For Include
using System;
using System.Linq;

namespace ExaminationSystemTT.PL.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    public class ExamController : Controller
    {
        // Inject necessary repositories
        private readonly IExamRepository _examRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorRepository _instructorRepository;
        // Inject QuestionRepository if needed for display (e.g., count in Details)
        private readonly IQuestionRepository _questionRepository;
        // Inject Context ONLY if using Include directly here (less ideal approach)
        private readonly ExaminationContext _context;

        public ExamController(
            IExamRepository examRepository,
            IQuestionRepository questionRepository, // Include for display purposes
            ICourseRepository courseRepository,
            IInstructorRepository instructorRepository,
            ExaminationContext context) // Injected for Include example
        {
            _examRepository = examRepository;
            _questionRepository = questionRepository;
            _courseRepository = courseRepository;
            _instructorRepository = instructorRepository;
            _context = context; // Assign for Include example
        }

        // GET: Exam/Index
        public IActionResult Index()
        {
            // Load related data for display using Include (or ideally a specific repo method)
            var exams = _context.Exams
                                .Include(e => e.Course)
                                .Include(e => e.Instructor)
                                .AsNoTracking()
                                .ToList();
            return View(exams);
        }

        // GET: Exam/Details/{id}
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            // Load Exam with Course, Instructor, and Questions for display
            var exam = _context.Exams
                                .Include(e => e.Course)
                                .Include(e => e.Instructor)
                                .Include(e => e.Questions) // Load questions for display
                                .FirstOrDefault(e => e.ExamId == id.Value); // Use FirstOrDefault with Include

            if (exam == null)
            {
                return NotFound();
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(exam); // Pass the exam with loaded questions to the view
        }

        // GET: Exam/Create
        public IActionResult Create()
        {
            // Prepare data for dropdown lists
            var courses = _courseRepository.GetAll();
            var instructors = _instructorRepository.GetAll();

            var viewModel = new ExamCreateViewModel
            {
                Courses = new SelectList(courses, nameof(Course.CourseId), nameof(Course.Name)),
                Instructors = new SelectList(instructors, nameof(Instructor.InstructorId), nameof(Instructor.FullName))
            };

            return View(viewModel);
        }

        // POST: Exam/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ExamCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var exam = new Exam
                    {
                        StartTime = viewModel.StartTime,
                        EndTime = viewModel.EndTime,
                        CourseId = viewModel.CourseId,
                        InstructorId = viewModel.InstructorId
                    };
                    int result = _examRepository.Add(exam); // Uses repo's Add
                    if (result > 0)
                    {
                        return RedirectToAction(nameof(Details), new { id = exam.ExamId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to save the exam.");
                    }
                }
                catch (Exception ex)
                {
                    // Log ex
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                }
            }
            // Repopulate dropdowns if failed
            var courses = _courseRepository.GetAll();
            var instructors = _instructorRepository.GetAll();
            viewModel.Courses = new SelectList(courses, nameof(Course.CourseId), nameof(Course.Name), viewModel.CourseId);
            viewModel.Instructors = new SelectList(instructors, nameof(Instructor.InstructorId), nameof(Instructor.FullName), viewModel.InstructorId);
            return View(viewModel);
        }


        // --- ADD QUESTION (Delegation Approach) ---
        // GET: Exam/AddQuestion/{examId}
        // This action simply redirects to the Question controller's Create action
        public IActionResult AddQuestion(int examId)
        {
            // Optional: Check if the exam actually exists first
            var exam = _examRepository.GetByID(examId); // Using repo's GetByID
            if (exam == null)
            {
                return NotFound($"Exam with ID {examId} not found.");
            }

            // Redirect to the Question controller's Create action, passing the examId
            return RedirectToAction("Create", "Question", new { examId = examId });
        }
        // --- NO AddQuestion POST action needed in ExamController ---


        // --- EDIT Exam Actions ---
        // GET: Exam/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) { return BadRequest(); }

            var exam = _examRepository.GetByID(id.Value); // Get the exam to edit
            if (exam == null) { return NotFound(); }

            // Prepare data for dropdown lists
            var courses = _courseRepository.GetAll();
            var instructors = _instructorRepository.GetAll();

            // Populate ViewModel
            var viewModel = new ExamCreateViewModel // Reusing Create VM
            {
                StartTime = exam.StartTime,
                EndTime = exam.EndTime,
                CourseId = exam.CourseId,
                InstructorId = exam.InstructorId,
                Courses = new SelectList(courses, nameof(Course.CourseId), nameof(Course.Name), exam.CourseId), // Set selected value
                Instructors = new SelectList(instructors, nameof(Instructor.InstructorId), nameof(Instructor.FullName), exam.InstructorId) // Set selected value
            };

            ViewBag.ExamId = id.Value; // Pass ID for the view if needed
            return View(viewModel);
        }

        // POST: Exam/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ExamCreateViewModel viewModel)
        {
            // We use 'id' from the route to identify the exam to update.
            if (!ModelState.IsValid)
            {
                // Repopulate dropdowns if validation fails
                var courses = _courseRepository.GetAll();
                var instructors = _instructorRepository.GetAll();
                viewModel.Courses = new SelectList(courses, nameof(Course.CourseId), nameof(Course.Name), viewModel.CourseId);
                viewModel.Instructors = new SelectList(instructors, nameof(Instructor.InstructorId), nameof(Instructor.FullName), viewModel.InstructorId);
                ViewBag.ExamId = id;
                return View(viewModel);
            }

            try
            {
                var examToUpdate = _examRepository.GetByID(id); // Get the original exam
                if (examToUpdate == null) { return NotFound(); }

                // Update properties from ViewModel
                examToUpdate.StartTime = viewModel.StartTime;
                examToUpdate.EndTime = viewModel.EndTime;
                examToUpdate.CourseId = viewModel.CourseId;
                examToUpdate.InstructorId = viewModel.InstructorId;

                int result = _examRepository.Update(examToUpdate); // Call repo's Update

                if (result > 0)
                {
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update the exam. Values might be unchanged.");
                    // Repopulate dropdowns if update fails logic needed
                    var courses = _courseRepository.GetAll();
                    var instructors = _instructorRepository.GetAll();
                    viewModel.Courses = new SelectList(courses, nameof(Course.CourseId), nameof(Course.Name), viewModel.CourseId);
                    viewModel.Instructors = new SelectList(instructors, nameof(Instructor.InstructorId), nameof(Instructor.FirstName), viewModel.InstructorId);
                    ViewBag.ExamId = id;
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                // Repopulate dropdowns on exception
                var courses = _courseRepository.GetAll();
                var instructors = _instructorRepository.GetAll();
                viewModel.Courses = new SelectList(courses, nameof(Course.CourseId), nameof(Course.Name), viewModel.CourseId);
                viewModel.Instructors = new SelectList(instructors, nameof(Instructor.InstructorId), nameof(Instructor.FirstName), viewModel.InstructorId);
                ViewBag.ExamId = id;
                return View(viewModel);
            }
        }

        // --- DELETE Exam Actions ---
        // GET: Exam/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) { return BadRequest(); }

            // Load exam with related data for confirmation display
            var exam = _context.Exams
                               .Include(e => e.Course)
                               .Include(e => e.Instructor)
                               .FirstOrDefault(e => e.ExamId == id.Value);

            // var exam = _examRepository.GetByID(id.Value); // Use repo's GetByID if no related data needed for view
            if (exam == null) { return NotFound(); }

            return View(exam);
        }

        // POST: Exam/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var exam = _examRepository.GetByID(id); // Get exam using repo
            if (exam == null)
            {
                TempData["ErrorMessage"] = "Exam not found or already deleted.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Assumes cascade delete settings in DbContext handle related Questions/Answers appropriately
                int result = _examRepository.Delete(exam); // Call repo's Delete
                if (result > 0)
                {
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete the exam. It might have been deleted already.";
                }
            }
            catch (Exception ex)
            {
                // Log ex
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}. Ensure related data allows deletion.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}