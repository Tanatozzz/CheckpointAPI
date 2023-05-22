using CheckpointAPI1.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using CheckpointAPI.Models;
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

        [HttpGet("Role_DataList")]
        public ActionResult<IEnumerable<Role>> GetAllRoles()
        {
            using (IDbConnection db = Connection)
            {
                var roles = db.Query<Role>(@"
                            SELECT * FROM [Role]").ToList();

                return Ok(roles);
            }
        }

        [HttpGet("CheckpointRole_DataList")]
        public ActionResult<IEnumerable<CheckpointRoleWithTitleID>> GetAllCheckpointRoles()
        {
            using (IDbConnection db = Connection)
            {
                var checkpointroles = db.Query<CheckpointRoleWithTitleID>(@"
                            SELECT cr.*, c.ID as 'CheckpointID', c.Title as 'CheckpointTitle' FROM [CheckpointRole] cr 
                            INNER JOIN [Checkpoint] c ON c.ID = cr.IDCheckpoint").ToList();

                return Ok(checkpointroles);
            }
        }

        [HttpGet("CheckpointAdditionalAccess_DataList")]
        public ActionResult<IEnumerable<CheckpointAdditionalAccess>> CheckpointAdditionalAccesses()
        {
            using (IDbConnection db = Connection)
            {
                var checkpointaddiotionaccess = db.Query<CheckpointAdditionalAccess>(@"
                            SELECT * FROM [CheckpointAdditionalAccess]").ToList();

                return Ok(checkpointaddiotionaccess);
            }
        }

        [HttpDelete("DeleteCheckpointRole")]
        public ActionResult DeleteCheckpointRole(int IDRole, int IDCheckpoint)
        {
            using (IDbConnection db = Connection)
            {
                // Проверяем наличие записи в таблице связи CheckpointRole
                var existingRecord = db.QueryFirstOrDefault<CheckpointRole>(@"
                                SELECT *
                                FROM CheckpointRole
                                WHERE IDRole = @IDRole AND IDCheckpoint = @IDCheckpoint",
                                        new { IDRole, IDCheckpoint });

                if (existingRecord == null)
                {
                    // Запись не найдена, возвращаем сообщение об ошибке или другой HTTP-код по вашему усмотрению
                    return NotFound();
                }

                // Удаляем запись из таблицы связи CheckpointRole
                int affectedRows = db.Execute(@"
                            DELETE FROM CheckpointRole
                            WHERE IDRole = @IDRole AND IDCheckpoint = @IDCheckpoint",
                                    new { IDRole, IDCheckpoint });

                if (affectedRows > 0)
                {
                    // Запись успешно удалена, возвращаем сообщение об успехе или другой HTTP-код по вашему усмотрению
                    return Ok();
                }
                else
                {
                    // При удалении возникла ошибка, возвращаем сообщение об ошибке или другой HTTP-код по вашему усмотрению
                    return StatusCode(500, "Failed to delete the record.");
                }
            }
        }

        [HttpPost("AddCheckpointRole")]
        public ActionResult AddCheckpointRole(int IDRole, int IDCheckpoint)
        {
            using (IDbConnection db = Connection)
            {

                // Проверяем наличие записи в таблице связи CheckpointRole
                var existingRecord = db.QuerySingleOrDefault<CheckpointRole>(@"
                                SELECT *
                                FROM CheckpointRole
                                WHERE IDRole = @IDRole AND IDCheckpoint = @IDCheckpoint",
                                            new { IDRole, IDCheckpoint });

                if (existingRecord != null)
                {
                    // Запись уже существует, возвращаем сообщение об ошибке или другой HTTP-код по вашему усмотрению
                    return Conflict("Record already exists.");
                }

                // Добавляем запись в таблицу связи CheckpointRole
                int affectedRows = db.Execute(@"
                            INSERT INTO CheckpointRole (IDRole, IDCheckpoint, DateAdd)
                            VALUES (@IDRole, @IDCheckpoint, GETDATE())",
                                        new { IDRole, IDCheckpoint });

                if (affectedRows > 0)
                {
                    // Запись успешно добавлена, возвращаем сообщение об успехе или другой HTTP-код по вашему усмотрению
                    return Ok();
                }
                else
                {
                    // При добавлении возникла ошибка, возвращаем сообщение об ошибке или другой HTTP-код по вашему усмотрению
                    return StatusCode(500, "Failed to add the record.");
                }
            }
        }
    }
}
