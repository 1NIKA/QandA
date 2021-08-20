using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QandA.Data;
using QandA.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QandA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IDataRepository _dataRepository;

        public QuestionsController(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        [HttpGet]
        public IEnumerable<QuestionGetManyResponse> GetQuestions(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return _dataRepository.GetQuestions();
            }

            return _dataRepository.GetQuestionsBySearch(search);
        }

        [HttpGet("unanswered")]
        public IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions()
        {
            return _dataRepository.GetUnansweredQuestions();
        }

        [HttpGet("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> GetQuestion(int questionId)
        {
            var question = _dataRepository.GetQuestion(questionId);
            if (question == null)
            {
                return NotFound();
            }

            return question;
        }

        [HttpPost]
        public ActionResult<QuestionGetSingleResponse> PostQuestion(QuestionPostRequest question)
        {
            var savedQuestion = _dataRepository.PostQuestion(
                new QuestionPostFullRequest()
                {
                    Title = question.Title,
                    Content = question.Content,
                    UserId = "1",
                    UserName = "bob.test@test.com",
                    Created = DateTime.UtcNow
                });
            
            return CreatedAtAction(nameof(GetQuestion), new 
            { 
                questionId = savedQuestion.QuestionId 
            }, savedQuestion);
        }

        [HttpPut("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> PutQuestion(
            int questionId, QuestionPutRequest questionPutRequest)
        {
            var question = _dataRepository.GetQuestion(questionId);
            if (question == null)
            {
                return NotFound();
            }

            questionPutRequest.Title = string.IsNullOrEmpty(questionPutRequest.Title) ? question.Title
                : questionPutRequest.Title;
            questionPutRequest.Content = string.IsNullOrEmpty(questionPutRequest.Content) ? question.Content
                : questionPutRequest.Content;

            return _dataRepository.PutQuestion(questionId, questionPutRequest);
        }

        [HttpDelete("{questionId}")]
        public ActionResult DeleteQuestion(int questionId)
        {
            var question = _dataRepository.GetQuestion(questionId);
            if (question == null)
            {
                return NotFound();
            }

            _dataRepository.DeleteQuestion(questionId);
            return NoContent();
        }

        [HttpPost("answer")]
        public ActionResult<AnswerGetResponse> PostAnswer(AnswerPostRequest answerPostRequest)
        {
            var questionExists = _dataRepository.QuestionExists(answerPostRequest.QuestionId.Value);
            if (!questionExists)
            {
                return NotFound();
            }

            return _dataRepository.PostAnswer(
                new AnswerPostFullRequest()
                {
                    QuestionId = answerPostRequest.QuestionId.Value,
                    Content = answerPostRequest.Content,
                    UserId = "1",
                    UserName = "bob.test@test.com",
                    Created = DateTime.UtcNow
                });
        }
    }
}
