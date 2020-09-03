using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using todo_app.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;


namespace todo_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private TodoContext _todoContext;
        IOptions<Parameters> _options;

        public TodoController(IOptions<Parameters> options)
        {
            Console.WriteLine("Todo Controller Start!");
            _options = options;            
            _todoContext = new TodoContext(options);
        }

        // GET: api/todo
        [HttpGet]
        public List<Todo> Get()
        {
            return _todoContext.GetAllTodos();
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]Todo todoVal)
        {
            Console.WriteLine("Entering todo POST - " + JsonConvert.SerializeObject(todoVal, Formatting.Indented));

            if (todoVal != null && !string.IsNullOrEmpty(todoVal.Status) &&
                    !string.IsNullOrEmpty(todoVal.Task))
            {
                Console.WriteLine("Saving DBContext! - " + todoVal.Status + "   " + todoVal.Task);
                _todoContext.SaveTodo(todoVal.Status, todoVal.Task);
            }
        }

    }
}
