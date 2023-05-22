using CheckpointAPI1.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using Dapper;


namespace CheckpointAPI1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataBaseController : ControllerBase
    {
        private readonly ILogger<DataBaseController> _logger;
        private readonly IConfiguration _config;
        public DataBaseController(ILogger<DataBaseController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpPost("Login_check", Name = "Login")]
        public ActionResult<Employee> Login(LoginRequest request)
        {
            using (IDbConnection db = Connection)
            {
                // Проверяем наличие пользователя в базе данных
                var employee = db.QueryFirstOrDefault<Employee>("SELECT * FROM Employee WHERE Login = @Login", new { Login = request.Username });

                // Проверяем правильность пароля
                if (employee != null && employee.Password == request.Password)
                {
                    // Возвращаем успешный результат с информацией о сотруднике
                    return Ok(employee);
                }

                // Возвращаем ошибку аутентификации
                return Unauthorized();
            }
        }

        [HttpGet("Employee_DataList")]
        public ActionResult<IEnumerable<EmployeeWithRole>> GetAllEmployees()
        {
            using (IDbConnection db = Connection)
            {
                // Получаем всех сотрудников из базы данных
                var employees = db.Query<EmployeeWithRole>(@"
                                                            SELECT e.*, r.Title AS RoleTitle
                                                            FROM Employee e
                                                            JOIN Role r ON e.IDRole = r.ID").ToList();

                // Возвращаем список сотрудников
                return Ok(employees);
            }
        }

        [HttpGet("AdditionAccess_DataList", Name = "AdditionAccessDataList")]
        public ActionResult<IEnumerable<AddiotionalAccess>> GetAdditionalAccesses()
        {
            // Установка соединения с базой данных
            using (IDbConnection db = Connection)
            {
                // Выполнение SQL-запроса для получения данных из таблицы AdditionAccess
                var result = db.Query<AddiotionalAccess>("SELECT * FROM AdditionAccess").ToList();
                // Возвращаем результат в виде HTTP 200 OK с данными в формате JSON
                return Ok(result);
            }
        }

        [HttpGet("Office_DataList", Name = "OfficeDataList")]
        public ActionResult<IEnumerable<OfficeWithCity>> GetAllOffices()
        {
            using (IDbConnection db = Connection)
            {
                var offices = db.Query<OfficeWithCity>(@"
                                                        SELECT o.*, c.Title AS CityName
                                                        FROM Office o
                                                        INNER JOIN City c ON c.ID = o.IDCity").ToList();
                return Ok(offices);
            }
        }

        [HttpGet("Checkpoint_DataList")]
        public ActionResult<IEnumerable<CheckpointWithOfficeName>> GetAllCheckpoints()
        {
            using (IDbConnection db = Connection)
            {
                var checkpoints = db.Query<CheckpointWithOfficeName>(@"
                            SELECT cp.*, o.Title AS OfficeTitle
                            FROM [Checkpoint] cp
                            JOIN Office o ON cp.IDOffice = o.ID").ToList();

                return Ok(checkpoints);
            }
        }

        public class CheckpointWithOfficeName : Checkpoint
        {
            public string OfficeTitle { get; set; }
        }

        public class Checkpoint
        {
            public int ID { get; set; }
            public string Title { get; set; }
            public int IDOffice { get; set; }
            public bool IsActive { get; set; }
        }

        public class OfficeWithCity : Office
        {
            public string CityName { get; set; }
        }

        public class Office
        {
            public int ID { get; set; }
            public string Title { get; set; }
            public string Address { get; set; }
            public int IDCity { get; set; }
        }


        public class AddiotionalAccess
        {
            public int ID { get; set; }
            public string Title { get; set; }
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class EmployeeWithRole : Employee
        {
            public string RoleTitle { get; set; }
        }

        public class Employee
        {
            public int ID { get; set; }
            public string FirstName { get; set; }
            public string Patronomyc { get; set; }
            public string LastName { get; set; }
            public DateTime LastVisitDate { get; set; }
            public bool isInside { get; set; }
            public int IDRole { get; set; }
            public int IDAdditionAccess { get; set; }
            public string PassportSeries { get; set; }
            public string PassportNumber { get; set; }
            public string INN { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
        }
    }
}
