using Domain.Models;
using Npgsql;

namespace DataAccess
{
    public class LogDb
    {
        public void Insert(LogModel logModel, NpgsqlConnection connection)
        {

            string insertQuery = "INSERT INTO log (text, date) VALUES (@text, @date);";
            using (NpgsqlCommand command = new NpgsqlCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@text", logModel.Text.Substring(0, logModel.Text.Length < 250 ? logModel.Text.Length : 250));
                command.Parameters.AddWithValue("@date", logModel.Date);
                command.ExecuteNonQuery();
            }
        }
    }
}
