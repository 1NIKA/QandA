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
        private readonly IQuestionCache _questionCache;

        public QuestionsController(IDataRepository dataRepository, IQuestionCache questionCache)
        {
            _dataRepository = dataRepository;
            _questionCache = questionCache;
        }

        [HttpGet]
        public IEnumerable<QuestionGetManyResponse> GetQuestions(
            string search, bool includeAnswers, int page = 1, int pageSize = 20)
        {
            if (string.IsNullOrEmpty(search))
            {
                if (includeAnswers)
                {
                    return _dataRepository.GetQuestionsWithAnswers();
                }
                else
                {
                    return _dataRepository.GetQuestions();
                }
            }

            return _dataRepository.GetQuestionsBySearchWithPaging(search, page, pageSize);
        }

        [HttpGet("unanswered")]
        public async Task<IEnumerable<QuestionGetManyResponse>> GetUnansweredQuestions()
        {
            return await _dataRepository.GetUnansweredQuestionsAsync();
        }

        [HttpGet("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> GetQuestion(int questionId)
        {
            var question = _questionCache.Get(questionId);
            if (question == null)
            {
                question = _dataRepository.GetQuestion(questionId);
                if (question == null)
                {
                    return NotFound();
                }
                _questionCache.Set(question);
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

            var savedQuestion = _dataRepository.PutQuestion(questionId, questionPutRequest);
            _questionCache.Remove(savedQuestion.QuestionId);

            return savedQuestion;
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
            _questionCache.Remove(questionId);

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

            _questionCache.Remove(answerPostRequest.QuestionId.Value);

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
