using Classes.Models;
using Domain.Models;
using Npgsql;

namespace DataAccess
{
    public class ResultDB
    {

        public void Insert(ResultModel resultModel, NpgsqlConnection connection)
        {
            
            string insertQuery = "INSERT INTO result (wpm, keystrokes, accuracy, correct_words, wrong_words, date) VALUES (@wpm, @keystrokes, @accuracy, @correct_words, @wrong_words, @date);";
            using (NpgsqlCommand command = new NpgsqlCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@wpm", resultModel.Wpm);
                command.Parameters.AddWithValue("@keystrokes", resultModel.Keystrokes);
                command.Parameters.AddWithValue("@accuracy", resultModel.Accuracy);
                command.Parameters.AddWithValue("@correct_words", resultModel.CorrectWords);
                command.Parameters.AddWithValue("@wrong_words", resultModel.WrongWords);
                command.Parameters.AddWithValue("@date", DateTime.Now);
                command.ExecuteNonQuery();
            }
        }
    }
}
