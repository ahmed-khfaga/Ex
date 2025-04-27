using ExaminationSystem.BLL.Interfaces;
using ExaminationSystem.BLL.Repositories;
using ExaminationSystem.DAL.Data;
using ExaminationSystem.DAL.Models;
// using ExaminationSystem.PL.ViewModels; // No longer needed
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // For Includes and exception types
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Can make actions async if repo supports it

namespace ExaminationSystem.PL.Controllers
{
    public class QuestionController : Controller
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IExamRepository _examRepository;
        private readonly ExaminitionSystemDbContext _dbContext; // Inject DbContext for direct manipulation in Edit POST

        // Inject DbContext along with repositories
        public QuestionController(IQuestionRepository questionRepository, IExamRepository examRepository, ExaminitionSystemDbContext dbContext)
        {
            _questionRepository = questionRepository;
            _examRepository = examRepository;
            _dbContext = dbContext; // Store context
        }


        public IActionResult Index()
        {

            var questions = _questionRepository.GetAll();

            return View(questions);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Exams = _examRepository.GetAll();  // Ensure this repository has all exams

            return View();
        }


        [HttpPost]
        public IActionResult Create(Question question)
        {
            if (ModelState.IsValid)
            {
                var count = _questionRepository.Add(question);
                if (count > 0)
                {
                    return RedirectToAction("Index");
                }
            }
            return View(question);
        }

      
    }
}