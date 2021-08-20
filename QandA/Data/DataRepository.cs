using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QandA.Data.Models;

namespace QandA.Data
{
    public class DataRepository : IDataRepository
    {
        private readonly string _connectionString;

        public DataRepository(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionStrings:DefaultConnection"];
        }

        public AnswerGetResponse GetAnswer(int answerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"EXEC dbo.Answer_Get_ByAnswerId @AnswerId";
                return connection.QueryFirstOrDefault<AnswerGetResponse>(sql, new
                {
                    AnswerId = answerId
                });
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestions()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"EXEC dbo.Question_GetMany";
                return connection.Query<QuestionGetManyResponse>(sql);
            }
        }

        public QuestionGetSingleResponse GetQuestion(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"EXEC dbo.Question_GetSingle @QuestionId";
                var question = connection.QueryFirstOrDefault<QuestionGetSingleResponse>(sql, new
                {
                    QuestionId = questionId
                });

                if (question != null)
                {
                    question.Answers = connection.Query<AnswerGetResponse>(
                    @"EXEC dbo.Answer_Get_ByQuestionId @QuestionId", new { QuestionId = questionId });
                }
                
                return question;
            }
        }
        
        public IEnumerable<QuestionGetManyResponse> GetQuestionsBySearch(string search)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"EXEC dbo.Question_GetMany_BySearch @Search";
                return connection.Query<QuestionGetManyResponse>(sql, new { Search = search });
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"EXEC dbo.Question_GetUnanswered";
                return connection.Query<QuestionGetManyResponse>(sql);
            }
        }

        public bool QuestionExists(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"EXEC dbo.Question_Exists @QuestionId";
                return connection.QueryFirst<bool>(sql, new { QuestionId = questionId });
            }
        }

        public QuestionGetSingleResponse PostQuestion(QuestionPostFullRequest question)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"EXEC dbo.Question_Post @Title, @Content, @UserId, @UserName, @Created";
                var questionId = connection.QueryFirst<int>(sql, question);

                return GetQuestion(questionId);
            }
        }

        public QuestionGetSingleResponse PutQuestion(int questionId, QuestionPutRequest question)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"EXEC dbo.Question_Put @QuestionId, @Title, @Content";
                connection.Execute(sql, new
                {
                    QuestionId = questionId,
                    Title = question.Title,
                    Content = question.Content
                });

                return GetQuestion(questionId);
            }
        }

        public void DeleteQuestion(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"EXEC dbo.Question_Delete @QuestionId";
                connection.Execute(sql, new { QuestionId = questionId });
            }
        }

        public AnswerGetResponse PostAnswer(AnswerPostFullRequest answer)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"EXEC dbo.Answer_Post @QuestionId, @Content, @UserId, @UserName, @Created";
                return connection.QueryFirst<AnswerGetResponse>(sql, answer);
            }
        }
    }
}
