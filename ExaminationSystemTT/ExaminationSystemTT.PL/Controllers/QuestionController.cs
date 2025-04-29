using ExaminationSystemTT.BLL.Interfaces; // Repositories
using ExaminationSystemTT.DAL.Models;     // Models
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ExaminationSystemTT.PL.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    public class QuestionController : Controller
    {
        // Use standard naming convention for injected fields in the controller
        private readonly IQuestionRepository _questionRepository;
        private readonly IExamRepository _examRepository;

        public QuestionController(IQuestionRepository questionRepository, IExamRepository examRepository)
        {
            _questionRepository = questionRepository;
            _examRepository = examRepository;
        }

        // GET: Question/Index?examId=5  (List questions for a specific exam)
        public IActionResult Index(int? examId)
        {
            if (examId == null)
            {
                ViewBag.ErrorMessage = "Please select an Exam first to view its questions.";
                return View(Enumerable.Empty<Question>()); // Return empty list
            }

            var exam = _examRepository.GetByID(examId.Value);
            if (exam == null)
            {
                return NotFound($"Exam with ID {examId} not found.");
            }
            ViewBag.ExamId = examId.Value;
            // --- Correction: Use GetAll and filter here ---
            // 1. Get ALL questions from the repository
            var allQuestions = _questionRepository.GetAll();

            // 2. Filter the questions in memory for the specific examId
            var questionsForExam = _questionRepository.GetByExamId(examId.Value);
            // --- End of Correction ---

            return View(questionsForExam); // Pass the filtered list to the view
        }

        // GET: Question/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var question = _questionRepository.GetByID(id.Value);
            if (question == null) // MUST check for null here
            {
                return NotFound();
            }
            // ViewBag.ExamId = question.ExamId; // Pass if needed
            // ViewBag.ExamCourseName = _examRepository.GetByID(question.ExamId)?.Course?.Name; // Requires loading course
            return View(question);
        }

        // GET: Question/Create?examId=5
        public IActionResult Create(int? examId)
        {
            if (examId == null)
            {
                return RedirectToAction("Index", "Exam");
            }

            var exam = _examRepository.GetByID(examId.Value);
            if (exam == null) // MUST check for null
            {
                return NotFound($"Exam with ID {examId} not found.");
            }

            var question = new Question { ExamId = examId.Value };
            ViewBag.ExamId = examId.Value;
            // ViewBag.ExamCourseName = exam.Course?.Name; // Requires loading course

            return View(question);
        }

        // POST: Question/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Question question) // Receives the Question model from the form
        {
            // --- Perform conditional validation AFTER basic model validation ---

            bool isMCQ = question.QuestionType == "MCQ"; // Use constants like QuestionTypes.MCQ is better
            bool isTF = question.QuestionType == "TF";   // Use constants like QuestionTypes.TF is better

            if (!isMCQ && !isTF)
            {
                // If a type *was* submitted but it's not recognized.
                // If type is null/empty, the [Required] on the model property should catch it.
                if (!string.IsNullOrEmpty(question.QuestionType))
                {
                    ModelState.AddModelError(nameof(Question.QuestionType), "Invalid question type selected.");
                }
                // Don't proceed with type-specific validation if type is invalid/missing
            }
            else if (isMCQ)
            {
                // Add errors if required MCQ fields are missing/invalid
                if (string.IsNullOrWhiteSpace(question.Option1)) ModelState.AddModelError(nameof(Question.Option1), "Option 1 is required for MCQ.");
                if (string.IsNullOrWhiteSpace(question.Option2)) ModelState.AddModelError(nameof(Question.Option2), "Option 2 is required for MCQ.");
                // Add checks for Option3/4 if desired
                if (question.CorrectOptionIndex == null || question.CorrectOptionIndex < 1 || question.CorrectOptionIndex > 4)
                {
                    ModelState.AddModelError(nameof(Question.CorrectOptionIndex), "A correct option number (1-4) is required for MCQ.");
                }

                // IMPORTANT: Null out the fields not used by MCQ *before* checking ModelState.IsValid again or saving
                question.CorrectAnswerTF = null;
            }
            else // isTF must be true
            {
                // Add errors if required TF fields are missing/invalid
                if (question.CorrectAnswerTF == null)
                {
                    ModelState.AddModelError(nameof(Question.CorrectAnswerTF), "A correct answer (True/False) is required for TF.");
                }

                // IMPORTANT: Null out the fields not used by TF *before* checking ModelState.IsValid again or saving
                question.Option1 = null;
                question.Option2 = null;
                question.Option3 = null;
                question.Option4 = null;
                question.CorrectOptionIndex = null;
            }
            // --- End Conditional Validation ---

            // NOW check ModelState AFTER adding custom errors and nulling fields
            if (ModelState.IsValid)
            {
                try
                {
                    // Optional but good: Verify the target Exam exists
                    var examExists = _examRepository.GetByID(question.ExamId);
                    if (examExists == null)
                    {
                        ModelState.AddModelError("ExamId", $"Target Exam (ID: {question.ExamId}) not found.");
                        // Must return view here if exam doesn't exist
                        ViewBag.ExamId = question.ExamId; // Pass back ExamId
                                                          // ViewBag.ExamCourseName = ...; // Reload context if needed
                        return View(question);
                    }

                    // Call synchronous Add
                    int result = _questionRepository.Add(question);
                    if (result > 0)
                    {
                        // Redirect to the list of questions for that exam
                        return RedirectToAction("Index", new { examId = question.ExamId });
                    }
                    else
                    {
                        // SaveChanges returned 0, which is unusual for Add unless maybe DB trigger prevented it
                        ModelState.AddModelError(string.Empty, "Failed to save the question (no rows affected).");
                    }
                }
                catch (Exception ex)
                {
                    // Log ex
                    ModelState.AddModelError(string.Empty, $"An error occurred while saving: {ex.Message}");
                }
            }
            else
            {
                // If ModelState is invalid AFTER our custom checks
                ModelState.AddModelError("", "Please correct the validation errors."); // Add general message
            }


            ViewBag.ExamId = question.ExamId; // Pass ExamId back to the view

            return View(question); // Return the view with the invalid model state
        }


        // GET: Question/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var question = _questionRepository.GetByID(id.Value);
            if (question == null) // MUST check for null
            {
                return NotFound();
            }

            ViewBag.ExamId = question.ExamId;
            // ViewBag.ExamCourseName = _examRepository.GetByID(question.ExamId)?.Course?.Name; // Reload context if needed
            return View(question);
        }

        // POST: Question/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Question question)
        {
            if (id != question.QuestionId)
            {
                return NotFound();
            }

            // Repeat validation logic from Create POST
            if (question.QuestionType == "MCQ") { /* ... validation ... */ }
            else if (question.QuestionType == "TF") { /* ... validation ... */ }
            else if (!string.IsNullOrEmpty(question.QuestionType)) { /* ... invalid type error ... */ }

            if (question.QuestionType == "TF") { /* Remove MCQ errors */ }
            if (question.QuestionType == "MCQ") { /* Remove TF errors */ }

            if (ModelState.IsValid)
            {
                try
                {
                    // Call synchronous Update
                    int result = _questionRepository.Update(question);
                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Question updated successfully!";
                        return RedirectToAction(nameof(Index), new { examId = question.ExamId });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to update the question. It might not exist or values were unchanged.");
                    }
                }
                catch (Exception ex)
                {
                    // Log ex
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }

            // If invalid or error, redisplay form
            ViewBag.ExamId = question.ExamId;
            // ViewBag.ExamCourseName = _examRepository.GetByID(question.ExamId)?.Course?.Name; // Reload context if needed
            return View(question);
        }

        // GET: Question/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var question = _questionRepository.GetByID(id.Value);
            if (question == null) // MUST check for null
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Question/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var question = _questionRepository.GetByID(id);
            if (question == null) // MUST check for null
            {
                TempData["ErrorMessage"] = "Question not found or already deleted.";
                return RedirectToAction("Index", "Exam");
            }

            int examId = question.ExamId;

            try
            {
                // Call synchronous Delete
                int result = _questionRepository.Delete(question);
                if (result > 0)
                {
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete the question. It might have been deleted already.";
                }
            }
            catch (Exception ex)
            {
                // Log ex
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { examId = examId });
        }
    }
}