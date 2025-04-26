using ExaminationSystem.BLL.Interfaces;
using ExaminationSystem.DAL.Models;
using ExaminationSystem.PL.ViewModels; // Uses the ViewModel you provided
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // For DbUpdateException, DbUpdateConcurrencyException
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExaminationSystem.PL.Controllers
{
    public class QuestionController : Controller
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IExamRepository _examRepository;
        private readonly IChoiseRepository _choiceRepository; // Renamed for consistency
        private readonly ILogger<QuestionController> _logger;

        public QuestionController(
            IQuestionRepository questionRepository,
            IExamRepository examRepository,
            IChoiseRepository choiceRepository,
            ILogger<QuestionController> logger)
        {
            _questionRepository = questionRepository ?? throw new ArgumentNullException(nameof(questionRepository));
            _examRepository = examRepository ?? throw new ArgumentNullException(nameof(examRepository));
            _choiceRepository = choiceRepository ?? throw new ArgumentNullException(nameof(choiceRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // --- Helper Methods ---

        // Populates ViewBag.ExamId for dropdowns
        private void PopulateExamsDropdown(int? selectedExam = null)
        {
            try
            {
                var exams = _examRepository.GetAll() ?? Enumerable.Empty<Exam>();
                // Ensure properties 'ID' and 'Title' exist on the Exam DAL model
                ViewBag.ExamId = new SelectList(exams, "ID", "Title", selectedExam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating exams dropdown.");
                ModelState.AddModelError(string.Empty, "An error occurred loading exam data.");
                ViewBag.ExamId = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
            }
        }

        // Maps DAL Question to QuestionViewModel (Needed for Edit GET)
        // Ensure QuestionRepository.GetByID includes Choices
        private QuestionViewModel MapQuestionToViewModel(Question question)
        {
            if (question == null) return null;

            var viewModel = new QuestionViewModel
            {
                Id = question.ID, // Map DAL ID to ViewModel Id
                QuestionText = question.QuestionText,
                Mark = question.Mark,
                QuestionType = question.QuestionType,
                ExamID = question.ExamID,
                CorrectTFAnswer = (question.QuestionType == "TF") ? question.CorrectTFAnswer : null,
                // Map DAL Choices to ViewModel Choices
                Choices = new List<ChoiceViewModel>() // Initialize list
            };

            if (question.QuestionType == "MCQ" && question.Choices != null)
            {
                foreach (var choice in question.Choices)
                {
                    viewModel.Choices.Add(new ChoiceViewModel
                    {
                        // No Id mapping needed as ChoiceViewModel has no Id
                        ChoiceText = choice.ChoiceText,
                        IsCorrect = choice.IsCorrect
                    });
                }
            }
            // Ensure there are always placeholders if needed by the Edit view's dynamic form
            // while(viewModel.Choices.Count < 4) // Example: Ensure at least 4 slots
            // {
            //     viewModel.Choices.Add(new ChoiceViewModel());
            // }

            return viewModel;
        }


        // --- Controller Actions ---

        // GET: Question or Question/Index/5 (Exam ID)
        public IActionResult Index(int? examId)
        {
            ViewBag.ExamIdFilter = examId;
            try
            {
                IEnumerable<Question> questions;
                string examTitle = "All Questions"; // Default title

                if (examId.HasValue)
                {
                    // Filter manually (assumes GetAll includes Exam for title display)
                    // If performance is an issue, add GetByExamId(int id) to repo
                    questions = _questionRepository.GetAll().Where(q => q.ExamID == examId.Value);
                    // Attempt to get the specific exam title for the view heading
                    var exam = questions.FirstOrDefault()?.Exam ?? _examRepository.GetByID(examId.Value);
                    examTitle = exam?.Title ?? $"Exam ID {examId.Value}";
                }
                else
                {
                    questions = _questionRepository.GetAll(); // Get all questions
                }

                ViewBag.ExamTitle = examTitle; // Set title for the view
                return View(questions); // Pass DAL models to Index view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving questions for Index (ExamId: {ExamId})", examId);
                TempData["ErrorMessage"] = "Could not load questions.";
                return View(Enumerable.Empty<Question>());
            }
        }

        // GET: Question/Details/5 (Question ID)
        public IActionResult Details(int? id) // Question ID
        {
            if (id == null)
            {
                _logger.LogWarning("Details action called with null Question ID.");
                return BadRequest();
            }

            try
            {
                // Ensure GetByID includes Exam and Choices for display
                var question = _questionRepository.GetByID(id.Value);
                if (question == null)
                {
                    _logger.LogWarning("Question not found for Details with ID: {QuestionId}", id.Value);
                    return NotFound();
                }
                return View(question); // Pass DAL model to Details view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving question details for ID: {QuestionId}", id.Value);
                TempData["ErrorMessage"] = "Could not load question details.";
                return RedirectToAction(nameof(Index)); // Redirect on error
            }
        }


        // GET: Question/Create or Question/Create/5 (Exam ID)
        [HttpGet]
        public IActionResult Create(int? examId)
        {
            PopulateExamsDropdown(examId);
            // Use the constructor's default initialization (4 empty choices)
            var viewModel = new QuestionViewModel
            {
                ExamID = examId ?? 0 // Default to 0 if not passed, handle validation
            };
            return View(viewModel);
        }

        // POST: Question/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(QuestionViewModel viewModel)
        {
            // --- Custom Validation for MCQ ---
            bool isMcqValidationOk = true;
            if (viewModel.QuestionType == "MCQ")
            {
                // Remove choices submitted as completely empty before validation
                viewModel.Choices?.RemoveAll(c => string.IsNullOrWhiteSpace(c.ChoiceText) && !c.IsCorrect);

                if (viewModel.Choices == null || viewModel.Choices.Count < 2)
                {
                    ModelState.AddModelError("Choices", "MCQ questions require at least two non-empty choices.");
                    isMcqValidationOk = false;
                }
                // Check *after* removing empty ones if at least one is marked correct
                if (viewModel.Choices != null && viewModel.Choices.Any() && !viewModel.Choices.Any(c => c.IsCorrect))
                {
                    ModelState.AddModelError("Choices", "At least one choice must be marked as correct for MCQ.");
                    isMcqValidationOk = false;
                }
            }

            // Combine standard and custom validation checks
            if (ModelState.IsValid && isMcqValidationOk)
            {
                // --- Map ViewModel to DAL Model ---
                var question = new Question
                {
                    // ID is generated by DB
                    QuestionText = viewModel.QuestionText,
                    Mark = viewModel.Mark,
                    QuestionType = viewModel.QuestionType,
                    ExamID = viewModel.ExamID,
                    CorrectTFAnswer = (viewModel.QuestionType == "TF") ? viewModel.CorrectTFAnswer : null,
                    Choices = new List<Choice>() // Initialize DAL collection
                };

                // Add choices only if MCQ and choices exist in VM
                if (viewModel.QuestionType == "MCQ" && viewModel.Choices != null)
                {
                    foreach (var choiceVm in viewModel.Choices)
                    {
                        // Add only if text isn't empty (double check after cleaning)
                        if (!string.IsNullOrWhiteSpace(choiceVm.ChoiceText))
                        {
                            question.Choices.Add(new Choice
                            {
                                ChoiceText = choiceVm.ChoiceText,
                                IsCorrect = choiceVm.IsCorrect
                                // QuestionID set by EF relationship fixup
                            });
                        }
                    }
                }
                // --- ---

                try
                {
                    // Add Question and related Choices (if any)
                    int result = _questionRepository.Add(question);
                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Question created successfully!";
                        return RedirectToAction(nameof(Index), new { examId = question.ExamID });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to save the question.");
                        _logger.LogWarning("QuestionRepository.Add returned 0 for Question Text: {QuestionText}", viewModel.QuestionText);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding Question Text: {QuestionText}", viewModel.QuestionText);
                    ModelState.AddModelError(string.Empty, $"An error occurred saving the question: {ex.Message}");
                }
            }

            // If ModelState invalid or save failed, repopulate and return view
            _logger.LogInformation("ModelState invalid or save error for Question Create.");
            PopulateExamsDropdown(viewModel.ExamID);
            // Ensure Choices list isn't null if returning view
            viewModel.Choices ??= new List<ChoiceViewModel>();
            return View(viewModel);
        }


        // GET: Question/Edit/5 (Question ID)
        [HttpGet]
        public IActionResult Edit(int? id) // Question ID
        {
            if (id == null)
            {
                _logger.LogWarning("Edit GET action called with null Question ID.");
                return BadRequest();
            }

            try
            {
                // Ensure GetByID includes Choices and Exam
                var question = _questionRepository.GetByID(id.Value);
                if (question == null)
                {
                    _logger.LogWarning("Question not found for Edit GET with ID: {QuestionId}", id.Value);
                    return NotFound();
                }

                var viewModel = MapQuestionToViewModel(question);
                if (viewModel == null)
                {
                    _logger.LogError("Failed to map Question to ViewModel for Edit GET with ID: {QuestionId}", id.Value);
                    TempData["ErrorMessage"] = "Error preparing question data.";
                    return RedirectToAction(nameof(Index), new { examId = question.ExamID });
                }

                PopulateExamsDropdown(viewModel.ExamID);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving question for Edit GET with ID: {QuestionId}", id.Value);
                TempData["ErrorMessage"] = "Could not load question for editing.";
                // Redirect back to general index, maybe pass original ID if needed?
                return RedirectToAction(nameof(Index));
            }
        }


        // POST: Question/Edit/5 (Question ID)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, QuestionViewModel viewModel) // id from route
        {
            // Check route ID matches ViewModel ID
            if (id != viewModel.Id)
            {
                _logger.LogWarning("Mismatch between route ID ({RouteId}) and form ID ({FormId}) during Question Edit POST.", id, viewModel.Id);
                return BadRequest("ID mismatch.");
            }

            // --- Custom Validation for MCQ ---
            bool isMcqValidationOk = true;
            if (viewModel.QuestionType == "MCQ")
            {
                viewModel.Choices?.RemoveAll(c => string.IsNullOrWhiteSpace(c.ChoiceText) && !c.IsCorrect);
                if (viewModel.Choices == null || viewModel.Choices.Count < 2)
                {
                    ModelState.AddModelError("Choices", "MCQ questions require at least two non-empty choices.");
                    isMcqValidationOk = false;
                }
                if (viewModel.Choices != null && viewModel.Choices.Any() && !viewModel.Choices.Any(c => c.IsCorrect))
                {
                    ModelState.AddModelError("Choices", "At least one choice must be marked as correct for MCQ.");
                    isMcqValidationOk = false;
                }
            }

            if (ModelState.IsValid && isMcqValidationOk)
            {
                try
                {
                    // Get existing Question *including its Choices* from DB
                    var questionInDb = _questionRepository.GetByID(viewModel.Id); // MUST include Choices
                    if (questionInDb == null)
                    {
                        _logger.LogWarning("Question not found during Edit POST with ID: {QuestionId}", viewModel.Id);
                        TempData["ErrorMessage"] = "Question not found.";
                        return NotFound(); // Or RedirectToAction if preferred
                    }

                    // --- Update scalar properties ---
                    questionInDb.QuestionText = viewModel.QuestionText;
                    questionInDb.Mark = viewModel.Mark;
                    questionInDb.QuestionType = viewModel.QuestionType;
                    questionInDb.ExamID = viewModel.ExamID; // Allow changing exam? Ensure FK exists.
                    questionInDb.CorrectTFAnswer = (viewModel.QuestionType == "TF") ? viewModel.CorrectTFAnswer : null;

                    // --- Handle Choices Update (Remove/Add strategy) ---
                    // 1. Remove existing choices (requires GetByID to have included them)
                    if (questionInDb.Choices != null)
                    {
                        _logger.LogInformation("Removing existing choices for Question ID: {QuestionId}", questionInDb.ID);
                        foreach (var existingChoice in questionInDb.Choices.ToList())
                        {
                            _choiceRepository.Delete(existingChoice); // Assumes repo saves or UnitOfWork pattern
                        }
                        questionInDb.Choices.Clear(); // Clear on tracked entity
                    }
                    else { questionInDb.Choices = new List<Choice>(); } // Initialize if null

                    // 2. Add new choices from ViewModel if MCQ
                    if (viewModel.QuestionType == "MCQ" && viewModel.Choices != null)
                    {
                        _logger.LogInformation("Adding choices from ViewModel for Question ID: {QuestionId}", questionInDb.ID);
                        foreach (var choiceVm in viewModel.Choices.Where(c => !string.IsNullOrWhiteSpace(c.ChoiceText)))
                        {
                            questionInDb.Choices.Add(new Choice
                            {
                                ChoiceText = choiceVm.ChoiceText,
                                IsCorrect = choiceVm.IsCorrect,
                                QuestionID = questionInDb.ID // Set FK
                            });
                        }
                    }
                    // --- ---

                    int result = _questionRepository.Update(questionInDb); // Update Question and related Choices
                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Question updated successfully!";
                        return RedirectToAction(nameof(Index), new { examId = questionInDb.ExamID });
                    }
                    else
                    {
                        // Update might return 0 if no properties actually changed in the DB
                        _logger.LogWarning("QuestionRepository.Update returned 0 for Question ID: {QuestionId}. No changes might have been detected or saved.", viewModel.Id);
                        TempData["WarningMessage"] = "No changes were detected or saved for the question."; // Use a different TempData key?
                        return RedirectToAction(nameof(Index), new { examId = questionInDb.ExamID }); // Redirect anyway? Or show edit view?
                                                                                                      // Or:
                                                                                                      // ModelState.AddModelError(string.Empty, "No changes were detected or saved.");
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning(ex, "Concurrency conflict updating Question ID: {QuestionId}", viewModel.Id);
                    ModelState.AddModelError(string.Empty, "Concurrency conflict. The record was modified by another user.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating Question ID: {QuestionId}", viewModel.Id);
                    ModelState.AddModelError(string.Empty, $"An error occurred updating the question: {ex.Message}");
                }
            }

            // If ModelState invalid or error occurred, repopulate and return view
            _logger.LogInformation("ModelState invalid or update error for Question Edit POST for ID: {QuestionId}.", viewModel.Id);
            PopulateExamsDropdown(viewModel.ExamID);
            // Ensure Choices list isn't null if returning view
            viewModel.Choices ??= new List<ChoiceViewModel>();
            return View(viewModel);
        }


        // GET: Question/Delete/5 (Question ID)
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete GET action called with null Question ID.");
                return BadRequest();
            }

            try
            {
                // Ensure GetByID includes Exam if needed for display
                var question = _questionRepository.GetByID(id.Value);
                if (question == null)
                {
                    _logger.LogWarning("Question not found for Delete GET with ID: {QuestionId}", id.Value);
                    return NotFound();
                }
                // Pass DAL model to Delete confirmation view
                return View(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving question for Delete GET with ID: {QuestionId}", id.Value);
                TempData["ErrorMessage"] = "Could not load question for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }


        // POST: Question/Delete/5 (Question ID)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id) // Use ViewModel 'Id' property name for consistency? No, this is route param.
        {
            int? examIdRedirect = null;
            Question questionToDelete = null;

            try
            {
                // Get the question *including its choices* before deleting
                questionToDelete = _questionRepository.GetByID(id); // MUST include Choices

                if (questionToDelete == null)
                {
                    _logger.LogWarning("Question not found for Delete POST (already deleted?) with ID: {QuestionId}", id);
                    TempData["SuccessMessage"] = "Question already deleted."; // Treat as success?
                    return RedirectToAction(nameof(Index)); // Redirect silently
                }

                examIdRedirect = questionToDelete.ExamID; // Store before deletion

                // --- Explicitly delete related choices first ---
                if (questionToDelete.Choices != null && questionToDelete.Choices.Any())
                {
                    _logger.LogInformation("Deleting {ChoiceCount} choices for Question ID: {QuestionId}", questionToDelete.Choices.Count, id);
                    foreach (var choice in questionToDelete.Choices.ToList())
                    {
                        _choiceRepository.Delete(choice);
                    }
                }
                // --- ---

                int result = _questionRepository.Delete(questionToDelete);

                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Question deleted successfully!";
                    return RedirectToAction(nameof(Index), new { examId = examIdRedirect });
                }
                else
                {
                    _logger.LogWarning("QuestionRepository.Delete returned 0 for Question ID: {QuestionId}", id);
                    ModelState.AddModelError(string.Empty, "The question could not be deleted. Please try again.");
                    // Fall through to return View(questionToDelete)
                }

            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting Question ID: {QuestionId}", id);
                // Check for FK constraint violation (e.g., from Answers table if it exists)
                if (ex.InnerException?.Message.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    ModelState.AddModelError(string.Empty, "This question cannot be deleted because student answers or other records depend on it.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"A database error occurred: {ex.InnerException?.Message ?? ex.Message}");
                }
                // Fall through to return View(questionToDelete)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Question ID: {QuestionId}", id);
                ModelState.AddModelError(string.Empty, $"An error occurred deleting the question: {ex.Message}");
                // Fall through to return View(questionToDelete)
            }

            // If deletion failed, redisplay the confirmation view with errors
            if (questionToDelete == null && id > 0)
            {
                // Attempt to refetch if error occurred before finding the record
                questionToDelete = _questionRepository.GetByID(id);
            }
            // Make sure the Delete.cshtml view can accept the DAL Question model
            return View(questionToDelete ?? new Question { ID = id }); // Provide dummy if completely failed
        }

    }
}