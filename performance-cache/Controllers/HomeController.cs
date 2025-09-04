using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Newtonsoft.Json;
using performance_cache.Model;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace performance_cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class HomeController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //Implentar o cachê
            string key = "get-users";
            var redis = ConnectionMultiplexer.Connect("localhost:6379"); //conectando o redis
            IDatabase db = redis.GetDatabase(); //criando um db cache
            await db.KeyExpireAsync(key, TimeSpan.FromSeconds(20)); //definindo o tempo que ele vai durar e a key do db
            string userValue = await db.StringGetAsync(key);
            if (!string.IsNullOrEmpty(userValue)) { 
                return Ok(userValue);
            }
            //buscando no banco
            using var connection = new MySqlConnection("Server=localhost;database=fiap;User=root;Password=123"); //colocando credenciais e qual o Server
            await connection.OpenAsync(); //fazendo conexão com o BD MySQL
            string sql = "select id, name, email from users; "; //Fazendo o mock da requisição SQL
            var users = await connection.QueryAsync<Users>(sql); //Fazendo uma query assincrona com o banco de dados
            var usersJson= JsonConvert.SerializeObject(users);
            await db.StringSetAsync(key, usersJson); //Settando o valor que estará no BD cachê  
            Thread.Sleep(3000); //Somente para forçar uma espera no código
            return Ok(users);
        }
    }
}
