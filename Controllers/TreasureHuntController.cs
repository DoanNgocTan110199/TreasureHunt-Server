using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Assignment.Data;
using Microsoft.Extensions.Configuration;
using Assignment.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Threading;

namespace Assignment.Data
{
    [ApiController]
    [Route("api/[controller]")]
    public class TreasureMapsController : ControllerBase
    {
        private readonly TreasureMapContext _context;
        public IConfiguration _configuration { get; }
        public SqlConnectionStringBuilder _builder { get; }
        
        public TreasureMapsController(TreasureMapContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        protected SqlConnectionStringBuilder CreateSqlConnectionStringBuilder()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = _configuration.GetConnectionString("DataSource") ;
            builder.InitialCatalog = _configuration.GetConnectionString("InitialCatalog"); ;
            builder.UserID = _configuration.GetConnectionString("UserID"); ;
            builder.Password = _configuration.GetConnectionString("Password"); ;
            builder.Encrypt = true;
            builder.TrustServerCertificate = true;
            //Optional settings.
            //builder.ConnectTimeout = 30; // in seconds
            //builder.IntegratedSecurity = false; // Explicitly set to false when using user/pass.
            //builder.MultipleActiveResultSets = true; // Allows multiple open readers.
            //builder.Encrypt = true; // Use encryption.
            //builder.TrustServerCertificate = false; // Validate server certificate.
            return builder;
        }
        [HttpGet]
        public IActionResult GetTreasureMaps()
        {
            
            var responseModel = new ResponseModel
            {
                Success = true,
            };
            try
            {
                var list = DatabaseConnection.GetListData(CreateSqlConnectionStringBuilder());
                var str = JsonConvert.SerializeObject(list);
                List<TreasureMap> lst = JsonConvert.DeserializeObject<List<TreasureMap>>(str);
                responseModel.Data = lst;
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message;
            }
            return new OkObjectResult(responseModel);
        }

        [HttpPost]
        public ActionResult<TreasureMap> PostTreasureMap(MatrixInputDto matrixInputDto)
        {
            var matrixInput = new MatrixInput();
            matrixInput.Rows = matrixInputDto.Rows;
            matrixInput.Columns = matrixInputDto.Columns;
            matrixInput.MaxTreasureValue = matrixInputDto.MaxTreasureValue;
            matrixInput.Matrix = JsonConvert.DeserializeObject<int[,]>(matrixInputDto.Matrix);

            if (!IsValidMatrix(matrixInput))
            {
                return BadRequest("Invalid matrix input.");
            }

            var treasureMap = new TreasureMap
            {
                Rows = matrixInput.Rows,
                Columns = matrixInput.Columns,
                MaxTreasureValue = matrixInput.MaxTreasureValue,
                Matrix = JsonConvert.SerializeObject(matrixInput.Matrix),
                MinFuel = DijkstraAlgorithm.CalculateMinFuel(
                    matrixInput.Rows,
                    matrixInput.Columns,
                    matrixInput.MaxTreasureValue,
                    DijkstraAlgorithm.TwoDToJagged(matrixInput.Matrix)) //CalculateMinFuel(matrixInput)
            };

            string columns = "Rows, Columns, MaxTreasureValue, Matrix, MinFuel";
            string values = "@Value1, @Value2, @Value3, @Value4, @Value5";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Value1", treasureMap.Rows),
                new SqlParameter("@Value2", treasureMap.Columns),
                new SqlParameter("@Value3", treasureMap.MaxTreasureValue),
                new SqlParameter("@Value4", treasureMap.Matrix),
                new SqlParameter("@Value5", treasureMap.MinFuel)
            };

            DatabaseConnection.InsertData(CreateSqlConnectionStringBuilder(),"TreasureMap", columns, values, parameters);

            return CreatedAtAction(nameof(GetTreasureMaps), new { id = treasureMap.Id }, treasureMap);
        }

        private bool IsValidMatrix(MatrixInput matrixInput)
        {
            if (matrixInput.Rows <= 0 || matrixInput.Columns <= 0 || matrixInput.MaxTreasureValue <= 0)
            {
                return false;
            }

            if (matrixInput.Matrix.GetLength(0) != matrixInput.Rows || matrixInput.Matrix.GetLength(1) != matrixInput.Columns)
            {
                return false;
            }

            for (int i = 0; i < matrixInput.Rows; i++)
            {
                for (int j = 0; j < matrixInput.Columns; j++)
                {
                    if (matrixInput.Matrix[i, j] < 1 || matrixInput.Matrix[i, j] > matrixInput.MaxTreasureValue)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private double? CalculateMinFuel(MatrixInput matrixInput)
        {
            int rows = matrixInput.Rows;
            int cols = matrixInput.Columns;
            int maxTreasureValue = matrixInput.MaxTreasureValue;
            int[,] matrix = matrixInput.Matrix;

            Dictionary<int, (int row, int col)> treasureLocations = new Dictionary<int, (int row, int col)>();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    treasureLocations[matrix[i, j]] = (i, j);
                }
            }

            double totalFuel = 0;
            int currentRow = 0;
            int currentCol = 0;

            for (int treasureValue = 1; treasureValue <= maxTreasureValue; treasureValue++)
            {
                (int nextRow, int nextCol) = treasureLocations[treasureValue];
                totalFuel += Math.Sqrt(Math.Pow(currentRow - nextRow, 2) + Math.Pow(currentCol - nextCol, 2));
                currentRow = nextRow;
                currentCol = nextCol;
            }

            return totalFuel;
        }
    }
}
