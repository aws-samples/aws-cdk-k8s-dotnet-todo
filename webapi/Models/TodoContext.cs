using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace todo_app.Models
{
    public class TodoContext: DbContext
    {
        public string ConnectionString { get; set; }    
    
        public TodoContext(IOptions<Parameters> options)
        {
            this.ConnectionString = options.Value.AuroraConnectionString;
            Console.WriteLine("Connection string - " + ConnectionString);
        }

        private MySqlConnection GetConnection()    
        {    
            return new MySqlConnection(ConnectionString);    
        }

        public List<Todo> GetAllTodos()
        {
            CreateToDosTable();
            List<Todo> list = new List<Todo>();

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from ToDos", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Todo()
                        {
                            Status = reader["Status"].ToString(),
                            Task = reader["Task"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        public void SaveTodo(string status, string task)
        {
            CreateToDosTable();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                cmd.CommandText = "INSERT INTO ToDos(status,task) VALUES(?status,?task)";
                cmd.Parameters.Add("?status", MySqlDbType.VarChar).Value = status;
                cmd.Parameters.Add("?task", MySqlDbType.VarChar).Value = task;
                cmd.ExecuteNonQuery();
            }
            Console.WriteLine("Succesfully saved values");
        }

        public void CreateToDosTable()
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string createTableSql = "use todo; ";
                createTableSql += "create table IF NOT EXISTS ToDos(";
                createTableSql += "id MEDIUMINT not null auto_increment,";
                createTableSql += "   CreatedTime TIMESTAMP DEFAULT now(),";
                createTableSql += "   Status VARCHAR(50),";
                createTableSql += "   Task VARCHAR(50),";
                createTableSql += "   primary key(id)";
                createTableSql += "); ";
                cmd.CommandText = createTableSql;
                cmd.ExecuteNonQuery();
            }
            Console.WriteLine("Table created successfully!!");
        }
    }

    
}
