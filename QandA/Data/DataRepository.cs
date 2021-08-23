using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QandA.Data.Models;
using static Dapper.SqlMapper;

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

        public IEnumerable<QuestionGetManyResponse> GetQuestionsWithAnswers()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"EXEC dbo.Question_GetMany_WithAnswers";

                var questionDictionary = new Dictionary<int, QuestionGetManyResponse>();
                return connection
                    .Query<QuestionGetManyResponse, AnswerGetResponse, QuestionGetManyResponse>(
                    sql, map: (q, a) =>
                    {
                        if (!questionDictionary.TryGetValue(
                            q.QuestionId, out QuestionGetManyResponse question))
                        {
                            question = q;
                            question.Answers = new List<AnswerGetResponse>();
                            questionDictionary.Add(question.QuestionId, question);
                        }
                        question.Answers.Add(a);
                        return question;
                    },
                    splitOn: "QuestionId"
                    )
                    .Distinct()
                    .ToList();
            }
        }

        public QuestionGetSingleResponse GetQuestion(int questionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"EXEC dbo.Question_GetSingle @QuestionId; 
                               EXEC dbo.Answer_Get_ByQuestionId @QuestionId";

                using (GridReader results = connection.QueryMultiple(sql, new 
                { QuestionId = questionId }))
                {
                    var question = results.Read<QuestionGetSingleResponse>().FirstOrDefault();
                    if (question != null)
                    {
                        question.Answers = results.Read<AnswerGetResponse>().ToList();
                    }

                    return question;
                }
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

        public IEnumerable<QuestionGetManyResponse> GetQuestionsBySearchWithPaging(
            string search, int pageNumber, int pageSize)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"EXEC dbo.Question_GetMany_BySearch_WithPaging 
                             @Search, @PageNumber, @PageSize";

                return connection.Query<QuestionGetManyResponse>(sql, new 
                {
                    Search = search,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
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

        public async Task<IEnumerable<QuestionGetManyResponse>> GetUnansweredQuestionsAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"EXEC dbo.Question_GetUnanswered";
                return await connection.QueryAsync<QuestionGetManyResponse>(sql);
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
