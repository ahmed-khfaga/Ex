using ExaminationSystem.BLL.Interfaces;
using ExaminationSystem.BLL.Repositories;
using ExaminationSystem.DAL.Models;
using ExaminationSystem.PL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExaminationSystem.PL.Controllers
{
    public class QuestionController : Controller
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IExamRepository _examRepository;
        private readonly IChoiseRepository _choiseRepository;



        public QuestionController(IQuestionRepository questionRepository, IExamRepository examRepository, IChoiseRepository choiseRepository)
        {
            _questionRepository = questionRepository;
            _examRepository = examRepository;
            _choiseRepository = choiseRepository;


        }
        private void PopulateExamsDropdown(object? selectedExam = null)
        {
            var exams = _examRepository.GetAll(); // Synchronous
            ViewBag.ExamList = new SelectList(exams, "Id", "Title", selectedExam);
        }
        public IActionResult Index()
        {
            var questions = _questionRepository.GetAll();
            return View(questions);
        }

        // Create Action (GET) - Display the Create question form
        [HttpGet]
        public IActionResult Create(int? examId) // Optional: Pre-select Exam ID
        {
            PopulateExamsDropdown(examId); // Populate dropdown
            var viewModel = new QuestionViewModel();
            if (examId.HasValue)
            {
                viewModel.ExamID = examId.Value; // Pre-set if passed
            }
            return View(viewModel); // Use ViewModel for Create form
        }

        // POST: Question/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(QuestionViewModel viewModel)
        {
            // Remove properties not directly bound or relevant based on type
            ModelState.Remove("Id");
            ModelState.Remove("ExamList");
            if (viewModel.QuestionType == "TF") ModelState.Remove("Choices");
            if (viewModel.QuestionType == "MCQ") ModelState.Remove("CorrectTFAnswer");


            if (ModelState.IsValid)
            {
                // --- Custom Validation for MCQ ---
                if (viewModel.QuestionType == "MCQ")
                {
                    viewModel.Choices.RemoveAll(c => string.IsNullOrWhiteSpace(c.ChoiceText)); // Clean empty choices
                    if (viewModel.Choices.Count < 2)
                    {
                        ModelState.AddModelError("Choices", "MCQ questions require at least two choices.");
                    }
                    if (!viewModel.Choices.Any(c => c.IsCorrect))
                    {
                        ModelState.AddModelError("Choices", "At least one choice must be marked as correct for MCQ.");
                    }
                    if (!ModelState.IsValid) // Re-check after custom validation
                    {
                        PopulateExamsDropdown(viewModel.ExamID);
                        return View(viewModel);
                    }
                }
                // --- ---

                // --- Map ViewModel to DAL Model ---
                var question = new Question
                {
                    QuestionText = viewModel.QuestionText,
                    Mark = viewModel.Mark,
                    QuestionType = viewModel.QuestionType,
                    ExamID = viewModel.ExamID,
                    // Set CorrectTFAnswer only if type is TF
                    CorrectTFAnswer = (viewModel.QuestionType == "TF") ? viewModel.CorrectTFAnswer : null,
                    // Initialize choices collection (EF Core needs this)
                    Choices = new List<Choice>()
                };

                // Add choices only if MCQ
                if (viewModel.QuestionType == "MCQ")
                {
                    foreach (var choiceVm in viewModel.Choices)
                    {
                        question.Choices.Add(new Choice
                        {
                            ChoiceText = choiceVm.ChoiceText,
                            IsCorrect = choiceVm.IsCorrect
                            // QuestionID will be set by EF Core relationship fixup
                        });
                    }
                }
                // --- ---

                try
                {
                    // Add the Question object (EF Core will handle adding related Choices)
                    var count = _questionRepository.Add(question); // Synchronous Add with SaveChanges inside
                    if (count > 0)
                    {
                        // Redirect to the specific exam's question list
                        return RedirectToAction(nameof(Index), new { examId = question.ExamID });
                    }
                    ModelState.AddModelError(string.Empty, "Could not create the question.");
                }
                catch (Exception ex)
                {
                    // Log ex
                    ModelState.AddModelError(string.Empty, $"An error occurred while creating the question: {ex.Message}");
                }
            }

            // If we got here, something failed, redisplay form
            PopulateExamsDropdown(viewModel.ExamID);
            return View(viewModel);
        }



       


      

        // GET: Question/Delete/5
        [HttpGet]
        public IActionResult Delete(int? id) // Question ID is long
        {
            if (!id.HasValue) return BadRequest();
            var question = _questionRepository.GetByID(id.Value); // Assumes GetByID handles long
            if (question == null) return NotFound();
            // Maybe load Exam Title for confirmation view
            // var exam = _examRepository.GetByID(question.ExamID);
            // ViewBag.ExamTitle = exam?.Title;
            return View(question); // Pass model to confirmation view
        }
        // POST: Question/Delete/5
        // POST: Question/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id) // Question ID is long
        {
            var questionToDelete = _questionRepository.GetByID(id); // Get existing question
            if (questionToDelete == null) return RedirectToAction(nameof(Index)); // If question doesn't exist, return to the index

            long? examId = questionToDelete.ExamID; // Store exam ID for redirect before deletion

            try
            {
                // Manually delete related entities (Choices, Answers) if cascade delete is not configured
                if (questionToDelete.Choices != null && questionToDelete.Choices.Any())
                {
                    foreach (var choice in questionToDelete.Choices.ToList())
                    {
                        _choiseRepository.Delete(choice); // Delete each choice related to this question
                    }
                }

                // If you have answers related to the question, make sure to delete them first
                // If Answers are in a separate table, delete them similarly before deleting the question
                // var answers = _answerRepository.GetAnswersByQuestionId(id);
                // foreach (var answer in answers)
                // {
                //     _answerRepository.Delete(answer); // Delete related answers
                // }

                // Delete the question after deleting related entities
                var count = _questionRepository.Delete(questionToDelete); // Delete question (EF Core should handle saving changes)

                if (count > 0)
                {
                    // Optionally add TempData success message for user feedback
                    TempData["SuccessMessage"] = "The question was successfully deleted.";
                    return RedirectToAction(nameof(Index), new { examId = examId }); // Redirect back to the question list of the exam
                }

                ModelState.AddModelError(string.Empty, "Could not delete the question.");
            }
            catch (Exception ex) // Catch potential DB exceptions (e.g., foreign key constraints on Answers)
            {
                // Log the exception (you may want to use a logger for better logging in a real application)
                ModelState.AddModelError(string.Empty, $"An error occurred while deleting the question: {ex.Message}. Ensure no student answers reference this question.");
            }

            // If deletion failed, return to confirmation view with error message
            var question = _questionRepository.GetByID(id); // Refetch question in case of failure
            return View(question); // Return to the confirmation view with the model
        }




    }
}
