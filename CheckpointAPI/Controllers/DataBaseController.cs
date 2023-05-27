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
        public ActionResult<IEnumerable<CheckpointAdditionalAccessWithTitleID>> CheckpointAdditionalAccesses()
        {
            using (IDbConnection db = Connection)
            {
                var checkpointaddiotionaccess = db.Query<CheckpointAdditionalAccessWithTitleID>(@"
                            SELECT caa.*, c.ID as 'CheckpointID', c.Title as 'CheckpointTitle' FROM [CheckpointAdditionalAccess] caa INNER JOIN [Checkpoint] c ON c.ID = caa.IDCheckpoint").ToList();

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
                    // Запись не найдена, возвращаем сообщение об ошибке
                    return NotFound();
                }

                // Удаляем запись из таблицы связи CheckpointRole
                int affectedRows = db.Execute(@"
                            DELETE FROM CheckpointRole
                            WHERE IDRole = @IDRole AND IDCheckpoint = @IDCheckpoint",
                                    new { IDRole, IDCheckpoint });

                if (affectedRows > 0)
                {
                    // Запись успешно удалена, возвращаем сообщение об успехе
                    return Ok();
                }
                else
                {
                    // При удалении возникла ошибка, возвращаем сообщение об ошибке
                    return StatusCode(500, "Failed to delete the record.");
                }
            }
        }

        [HttpDelete("DeleteCheckpointAdditionalAccess")]
        public ActionResult DeleteCheckpointAdditionalAccess(int IDAdditionalAccess, int IDCheckpoint)
        {
            using (IDbConnection db = Connection)
            {
                // Проверяем наличие записи в таблице связи CheckpointRole
                var existingRecord = db.QueryFirstOrDefault<CheckpointAdditionalAccess>(@"
                                SELECT *
                                FROM CheckpointAdditionalAccess
                                WHERE IDAdditionalAccess = @IDAdditionalAccess AND IDCheckpoint = @IDCheckpoint",
                                        new { IDAdditionalAccess, IDCheckpoint });

                if (existingRecord == null)
                {
                    // Запись не найдена, возвращаем сообщение об ошибке
                    return NotFound();
                }

                // Удаляем запись из таблицы связи CheckpointRole
                int affectedRows = db.Execute(@"
                            DELETE FROM CheckpointAdditionalAccess
                            WHERE IDAdditionalAccess = @IDAdditionalAccess AND IDCheckpoint = @IDCheckpoint",
                                    new { IDAdditionalAccess, IDCheckpoint });

                if (affectedRows > 0)
                {
                    // Запись успешно удалена, возвращаем сообщение об успехе
                    return Ok();
                }
                else
                {
                    // При удалении возникла ошибка, возвращаем сообщение об ошибке
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
                    // Запись уже существует, возвращаем сообщение об ошибке
                    return Conflict("Record already exists.");
                }

                // Добавляем запись в таблицу связи CheckpointRole
                int affectedRows = db.Execute(@"
                            INSERT INTO CheckpointRole (IDRole, IDCheckpoint, DateAdd)
                            VALUES (@IDRole, @IDCheckpoint, GETDATE())",
                                        new { IDRole, IDCheckpoint });

                if (affectedRows > 0)
                {
                    // Запись успешно добавлена, возвращаем сообщение об успехе
                    return Ok();
                }
                else
                {
                    // При добавлении возникла ошибка, возвращаем сообщение об ошибке
                    return StatusCode(500, "Failed to add the record.");
                }
            }
        }

        [HttpPost("AddCheckpointAdditionalAccess")]
        public ActionResult AddCheckpointAdditionalAccess(int IDAdditionalAccess, int IDCheckpoint)
        {
            using (IDbConnection db = Connection)
            {

                // Проверяем наличие записи в таблице связи CheckpointRole
                var existingRecord = db.QuerySingleOrDefault<CheckpointAdditionalAccess>(@"
                                SELECT *
                                FROM CheckpointAdditionalAccess
                                WHERE IDAdditionalAccess = @IDAdditionalAccess AND IDCheckpoint = @IDCheckpoint",
                                            new { IDAdditionalAccess, IDCheckpoint });

                if (existingRecord != null)
                {
                    // Запись уже существует, возвращаем сообщение об ошибке
                    return Conflict("Record already exists.");
                }

                // Добавляем запись в таблицу связи CheckpointRole
                int affectedRows = db.Execute(@"
                            INSERT INTO CheckpointAdditionalAccess (IDAdditionalAccess, IDCheckpoint, DateAdd)
                            VALUES (@IDAdditionalAccess, @IDCheckpoint, GETDATE())",
                                        new { IDAdditionalAccess, IDCheckpoint });

                if (affectedRows > 0)
                {
                    // Запись успешно добавлена, возвращаем сообщение об успехе
                    return Ok();
                }
                else
                {
                    // При добавлении возникла ошибка, возвращаем сообщение об ошибке
                    return StatusCode(500, "Failed to add the record.");
                }
            }
        }
        [HttpPut("UpdateEmployee/{id}")]
        public ActionResult UpdateEmployee(int id, Employee updatedEmployee)
        {
            using (IDbConnection db = Connection)
            {
                // Проверяем наличие сотрудника в базе данных
                var existingEmployee = db.QueryFirstOrDefault<Employee>("SELECT * FROM Employee WHERE ID = @ID", new { ID = id });

                if (existingEmployee == null)
                {
                    // Сотрудник не найден, возвращаем сообщение об ошибке
                    return NotFound();
                }

                // Обновляем все поля сотрудника
                existingEmployee.FirstName = updatedEmployee.FirstName;
                existingEmployee.Patronomyc = updatedEmployee.Patronomyc;
                existingEmployee.LastName = updatedEmployee.LastName;
                existingEmployee.LastVisitDate = updatedEmployee.LastVisitDate;
                existingEmployee.isInside = updatedEmployee.isInside;
                existingEmployee.IDRole = updatedEmployee.IDRole;
                existingEmployee.IDAdditionAccess = updatedEmployee.IDAdditionAccess;
                existingEmployee.PassportSeries = updatedEmployee.PassportSeries;
                existingEmployee.PassportNumber = updatedEmployee.PassportNumber;
                existingEmployee.INN = updatedEmployee.INN;
                existingEmployee.Login = updatedEmployee.Login;
                existingEmployee.Password = updatedEmployee.Password;
                int affectedRows;
                // Выполняем обновление записи в базе данных
                affectedRows = db.Execute(@"
                    UPDATE Employee
                    SET FirstName = @FirstName,
                        Patronomyc = @Patronomyc,
                        LastName = @LastName,
                        LastVisitDate = @LastVisitDate,
                        isInside = @isInside,
                        IDRole = @IDRole,
                        IDAdditionAccess = @IDAdditionAccess,
                        PassportSeries = @PassportSeries,
                        PassportNumber = @PassportNumber,
                        INN = @INN,
                        Login = @Login,
                        Password = @Password
                        WHERE ID = @ID",
                    existingEmployee);


                if (affectedRows > 0)
                {
                    // Запись успешно обновлена, возвращаем сообщение об успехе
                    return Ok();
                }
                else
                {
                    // При обновлении возникла ошибка, возвращаем сообщение об ошибке
                    return StatusCode(500, "Failed to update the record.");
                }
            }
        }
        // Метод для добавления нового работника
        [HttpPost("AddEmployee")]
        public ActionResult AddEmployee(Employee newEmployee)
        {
            using (IDbConnection db = Connection)
            {
                // Выполняем вставку новой записи в базу данных
                int affectedRows = db.Execute(@"
                    INSERT INTO Employee (FirstName, Patronomyc, LastName, IDRole, PassportSeries, PassportNumber, INN, Login, Password)
                    VALUES (@FirstName, @Patronomyc, @LastName, @IDRole, @PassportSeries, @PassportNumber, @INN, @Login, @Password)",
                        newEmployee);

                if (affectedRows > 0)
                {
                    // Запись успешно добавлена, возвращаем сообщение об успехе
                    return Ok();
                }
                else
                {
                    // При добавлении возникла ошибка, возвращаем сообщение об ошибке
                    return StatusCode(500, "Failed to add the record.");
                }
            }
        }

        // Метод для удаления работника по ID
        [HttpDelete("DeleteEmployee/{id}")]
        public ActionResult DeleteEmployee(int id)
        {
            using (IDbConnection db = Connection)
            {
                // Проверяем наличие сотрудника в базе данных
                var existingEmployee = db.QueryFirstOrDefault<Employee>("SELECT * FROM Employee WHERE ID = @ID", new { ID = id });

                if (existingEmployee == null)
                {
                    // Сотрудник не найден, возвращаем сообщение об ошибке
                    return NotFound();
                }

                // Выполняем удаление записи из базы данных
                int affectedRows = db.Execute("DELETE FROM Employee WHERE ID = @ID", new { ID = id });

                if (affectedRows > 0)
                {
                    // Запись успешно удалена, возвращаем сообщение об успехе
                    return Ok();
                }
                else
                {
                    // При удалении возникла ошибка, возвращаем сообщение об ошибке
                    return StatusCode(500, "Failed to delete the record.");
                }
            }
        }

        [HttpPut("UpdateCheckpoint/{id}")]
        public ActionResult UpdateCheckpoint(int id, Checkpointt checkpoint)
        {
            using (IDbConnection db = Connection)
            {
                // Проверяем наличие прохода в базе данных
                var existingCheckpoint = db.QueryFirstOrDefault<Checkpointt>("SELECT * FROM [Checkpoint] WHERE ID = @ID", new { ID = id });

                if (existingCheckpoint == null)
                {
                    // Проход не найден, возвращаем сообщение об ошибке
                    return NotFound();
                }

                // Обновляем активность прохода
                existingCheckpoint.ID = checkpoint.ID;
                existingCheckpoint.Title = checkpoint.Title;
                existingCheckpoint.IDOffice = checkpoint.IDOffice;
                existingCheckpoint.IsActive = checkpoint.IsActive;

                // Выполняем обновление записи в базе данных
                int affectedRows = db.Execute(@"
                    UPDATE [Checkpoint]
                    SET Title = @Title,
                        IDOffice = @IDOffice,
                        IsActive = @IsActive
                    WHERE ID = @ID",
                    existingCheckpoint);

                if (affectedRows > 0)
                {
                    // Запись успешно обновлена, возвращаем сообщение об успехе
                    return Ok();
                }
                else
                {
                    // При обновлении возникла ошибка, возвращаем сообщение об ошибке
                    return StatusCode(500, "Failed to update the record.");
                }
            }
        }
        [HttpPost("AddCheckpoint")]
        public ActionResult AddCheckpoint(Checkpointt newCheckpoint)
        {
            using (IDbConnection db = Connection)
            {
                // Выполняем вставку новой записи в базу данных
                int affectedRows = db.Execute(@"
                INSERT INTO [Checkpoint] (Title, IDOffice, isActive)
                VALUES (@Title, @IDOffice, @isActive)",
                    newCheckpoint);

                if (affectedRows > 0)
                {
                    // Запись успешно добавлена, возвращаем сообщение об успехе
                    return Ok();
                }
                else
                {
                    // При добавлении возникла ошибка, возвращаем сообщение об ошибке
                    return StatusCode(500, "Failed to add the record.");
                }
            }
        }
        [HttpGet("CheckAccess/{idCheckpoint}/{idEmployee}")]
        public ActionResult<bool> CheckAccess(int idCheckpoint, int idEmployee)
        {
            using (IDbConnection db = Connection)
            {
                // Проверяем наличие прохода в базе данных
                var checkpoint = db.QueryFirstOrDefault<Checkpointt>("SELECT * FROM [Checkpoint] WHERE ID = @ID", new { ID = idCheckpoint });

                if (checkpoint == null)
                {
                    // Проход не найден, возвращаем сообщение об ошибке или false в случае отсутствия прохода
                    return NotFound();
                }

                // Проверяем доступ сотрудника к проходу на основе триггера в базе данных
                var parameters = new
                {
                    EmployeeID = idEmployee,
                    CheckpointOpened = idCheckpoint
                };

                bool hasAccess = db.QuerySingleOrDefault<bool>(@"
                    DECLARE @EmployeeRole int = (SELECT IDRole FROM Employee WHERE ID = @EmployeeID);
                    DECLARE @EmployeeAddAccess int = (SELECT IDAdditionAccess FROM Employee WHERE ID = @EmployeeID);

                    IF ((SELECT COUNT(*) FROM CheckpointRole WHERE IDRole = @EmployeeID AND IDCheckpoint = @CheckpointOpened) < 1)
                        AND ((SELECT COUNT(*) FROM CheckpointAdditionalAccess WHERE IDAdditionalAccess = @EmployeeAddAccess AND IDCheckpoint = @CheckpointOpened) < 1)
                    BEGIN
                        SELECT CAST(0 AS BIT); -- Access denied
                    END
                    ELSE
                    BEGIN
                        SELECT CAST(1 AS BIT); -- Access granted
                    END",
                parameters);

                return hasAccess;
            }
        }
    }
}
