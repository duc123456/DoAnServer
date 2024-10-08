﻿using Application.IServices;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ChatGPTService : IChatGPTService
    {

        public async Task<string> GetChatGPTAnswer(string questionText)
        {
            string chatGPTAPIkey = "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";
            string answer = string.Empty;

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {chatGPTAPIkey}");

            //get database schema from azure db
            string schemaInfo = await GetDatabaseSchemaInfo();

            if (string.IsNullOrEmpty(schemaInfo))
            {
                schemaInfo = await GetDatabaseSchemaInfo();
            }

            var identifyRequestBody = new
            {
                model = "gpt-4-turbo",
                messages = new[]
                {
                    new {
                        role = "system",
                        content = $@"
Bạn là một trợ lý AI  được phát triển đặc biệt để hỗ trợ người dùng trên trang web https://www.goodjobs.works/.
Trang web này là một nền tảng tìm kiếm việc làm freelance cho sinh viên tại Việt Nam.
Nhiệm vụ của bạn bao gồm việc trả lời các câu hỏi của người dùng, xác định xem câu hỏi có liên quan đến cơ sở dữ liệu hay không, 
và nếu có, bạn cần phải tạo ra truy vấn SQL dùng trên Azure database phù hợp. Bạn cần phải đưa ra câu trả lời ngắn gọn, chính xác và dễ hiểu. 
Cấu trúc cơ sở dữ liệu của trang web này là: {schemaInfo}.
Hãy trả lời như một trợ lý AI chuyên nghiệp, luôn hướng đến việc giúp đỡ người dùng một cách hiệu quả nhất, và trả lời theo ngôn ngữ của người dùng.
Ngoài ra, nếu câu hỏi liên quan đến dữ liệu ngoài dự án, danh mục, kỹ năng hoặc bài viết thì bạn trả lời là 'Tôi không có đủ thẩm quyền để trả lời câu hỏi này'"
                    },
                    new {
                        role = "user",
                        content = $"Câu hỏi sau đây có phải là truy vấn cơ sở dữ liệu không? Nếu có, hãy CHỈ tạo truy vấn SQL phù hợp, lưu ý, chỉ cần câu truy vấn và không cần giải thích gì. Nếu không, chỉ cần trả lời 'Không'. \"{questionText}\". " 
                    }
                }
            };


            var jsonIdentifyRequestBody = JsonConvert.SerializeObject(identifyRequestBody);
            var identifyContent = new StringContent(jsonIdentifyRequestBody, Encoding.UTF8, "application/json");

            var identifyResponse = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", identifyContent);
            var identifyResponseString = await identifyResponse.Content.ReadAsStringAsync();

            if (identifyResponse.IsSuccessStatusCode)
            {
                var identifyResponseJson = JsonConvert.DeserializeObject<dynamic>(identifyResponseString);
                string chatGptResponse = identifyResponseJson.choices[0].message.content.ToString().Trim();

                //if question related to database
                if (chatGptResponse.Contains("SELECT"))
                {
                    chatGptResponse = chatGptResponse.Replace("```sql", "").Replace("```", "").Trim();

                    var queryResponse = QueryDatabase(chatGptResponse);
                    
                    var answerAfterQuery = await GetChatGPTAnswerAfterQuery(queryResponse);

                    return answerAfterQuery;
                }
            }
            else
            {
                throw new Exception($"Error: {identifyResponse.StatusCode}, Content: {identifyResponseString}");
            }
            // not related to database
            var chatRequestBody = new
            {
                model = "gpt-4-turbo",
                messages = new[]
                {
                    new {
                        role = "system",
                        content = $@"
Bạn tên là 'Trợ lý GoodJobs'. Nếu người dùng chào bạn, bạn hãy chào lại với tên của bạn, ví dụ 'Trợ lý ảo GoodJobs chào bạn, tôi có thể giúp gì cho bạn không?'.
Nếu những câu hỏi bạn không chắc chắc về câu trả lời hoặc câu trả lời bạn nghĩ nó sẽ rất dài dòng, hãy trả lời là 
'Xin lỗi, vì tôi là một trợ lý ảo, nhiều điều tôi có thể chưa rõ. Vui lòng liên hệ Admin để được hỗ trợ chi tiết hơn. Cảm ơn bạn' 
hoặc nếu người dùng cố muốn hỏi về 1 nội dung vài lần mà bạn không chắc chắn, hãy trả lời 'Chịu, bạn hỏi Admin nhé, tôi là AI chứ có phải người đâu'"
                    },
            new { role = "user", content = questionText }
        }
            };

            var jsonChatRequestBody = JsonConvert.SerializeObject(chatRequestBody);
            var chatContent = new StringContent(jsonChatRequestBody, Encoding.UTF8, "application/json");

            var chatResponse = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", chatContent);
            var chatResponseString = await chatResponse.Content.ReadAsStringAsync();

            if (chatResponse.IsSuccessStatusCode)
            {
                var chatResponseJson = JsonConvert.DeserializeObject<dynamic>(chatResponseString);
                answer = chatResponseJson.choices[0].message.content.ToString();
            }
            else
            {
                throw new Exception($"Error: {chatResponse.StatusCode}, Content: {chatResponseString}");
            }

            return answer;
        }

        private async Task<string> GetDatabaseSchemaInfo()
        {
            string connectionString = "Data Source=tcp:sv-doan-2.database.windows.net,1433;Initial Catalog=DoAnServer2.0;User Id=anhn2592@sv-doan-2;Password=Leduc2810;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            StringBuilder schemaInfo = new StringBuilder();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                //get tables from azure db schema
                DataTable tables = conn.GetSchema("Tables");
                foreach (DataRow row in tables.Rows)
                {
                    string tableName = row["TABLE_NAME"].ToString();
                    schemaInfo.AppendLine($"Table: {tableName}");

                    // get column details
                    SqlCommand cmd = new SqlCommand($"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'", conn);
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        schemaInfo.AppendLine($"    Column: {reader["COLUMN_NAME"]}, Type: {reader["DATA_TYPE"]}");
                    }

                    reader.Close();
                }

                conn.Close();
            }

            return schemaInfo.ToString();
        }

        private string QueryDatabase(string query)
        {
            string connectionString = "Data Source=tcp:sv-doan-2.database.windows.net,1433;Initial Catalog=DoAnServer2.0;User Id=anhn2592@sv-doan-2;Password=Leduc2810;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            string result = string.Empty;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    //read data from column
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result += reader.GetName(i) + ": " + reader[i].ToString() + "\n";
                    }
                    result += "\n";
                }

                conn.Close();
            }

            return string.IsNullOrEmpty(result) ? "Không tìm thấy kết quả phù hợp trong database." : result;
        }

        public async Task<string> GetChatGPTAnswerAfterQuery(string questionText)
        {
            var chatGPTAPIkey = "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";
            string answer = string.Empty;

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {chatGPTAPIkey}");

            var question = @"
Từ câu trả lời này, nếu dữ liệu trả về là chi tiết các dự án, thì thay vì gửi tất cả dữ liệu, hãy gửi câu trả lời là các đường link với Id của dự án đó (lưu ý chỉ lấy dự án với statusId =2, và isDeleted = false và khi trả lời, hãy nhớ không ghi nội dung lưu ý này vào mà chỉ trả lời trọng tâm), ví dụ: 'https://www.goodjobs.works/detail/152'
Còn nếu là số, thì hãy dựa vào những câu hỏi trước đó và đưa ra câu trả lời thích hợp.
Tương tự nếu người dùng hỏi 1 danh sách thì bạn chỉ cũng chỉ cần đưa ra 1 số thông tin cơ bản (nếu là về dự án thì chỉ cần tên dự án, thời gian, ngân sách tối thiểu, tối đa,
nếu về kĩ năng, thì chỉ cần tên kĩ năng, nếu về danh mục, chỉ cần tên danh mục)
" + questionText;

            var requestBody = new
            {
                model = "gpt-4-turbo",
                messages = new[]
                {
                    new { role = "user", content = question }
                }
            };

            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var responseJson = JsonConvert.DeserializeObject<dynamic>(responseString);
                answer = responseJson.choices[0].message.content.ToString();
            }
            else
            {
                throw new Exception($"Error: {response.StatusCode}, Content: {responseString}");
            }

            return answer;

        }

        public async Task<string> GetChatGPTAnswer1(string questionText)
        {

            //var chatGPTAPIkey = "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";
            //string answer = string.Empty;

            //var httpClient = new HttpClient();
            //httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {chatGPTAPIkey}");

           
            //var requestBody = new
            //{
            //    model = "gpt-4-turbo",
            //    messages = new[]
            //    {
            //        new { role = "user", content = questionText }
            //    }
            //};

            //var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
            //var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            //var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            //var responseString = await response.Content.ReadAsStringAsync();

            //if (response.IsSuccessStatusCode)
            //{
            //    var responseJson = JsonConvert.DeserializeObject<dynamic>(responseString);
            //    answer = responseJson.choices[0].message.content.ToString();
            //}
            //else
            //{
            //    throw new Exception($"Error: {response.StatusCode}, Content: {responseString}");
            //}

            //return answer;
            return "k trả lời đâu nhé, e đang tắt AI r, nào cần e bật lại cho, tốn tiền điên";

        }


    }
}
