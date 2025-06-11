using tnki_line_sale_api.Entities;
using tnki_line_sale_api.Utilities;
using tnki_line_sale_api.Models;
using tnki_line_sale_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;

namespace tnki_line_sale_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly ILogger<AddressController> _logger;
        private SqlConnection _dbCon;
        private string _connectionString;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AddressController(ILogger<AddressController> logger, IOptions<ConnectionString> connectionStrings, IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _connectionString = connectionStrings.Value.PrimaryDatabase;
            _dbCon = new SqlConnection(_connectionString);
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("getProvince")]
        public IActionResult getProvince()
        {
            try
            {
                _logger.LogInformation("getProvince");

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                List<AddrProviceModel> lstData = serv.getProvince(_dbCon);
                return Ok(lstData);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getProvince error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }

        }


        [HttpGet("getDistByProvId")]
        public IActionResult getDistByProvId(long provId)
        {
            try
            {
                _logger.LogInformation("getDistByProvId provId:" + provId);

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                List<AddrDistrictModel> lstData = serv.getDistByProvId(_dbCon, provId);
                return Ok(lstData);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getDistByProvId error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }

        [HttpGet("getSubDistByDistId")]
        public IActionResult getSubDistByDistId(long distId)
        {
            try
            {
                _logger.LogInformation("getSubDistByDistId distId:" + distId);

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                List<AddrSubDistrictModel> lstData = serv.getSubDistByDistId(_dbCon, distId);
                return Ok(lstData);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getSubDistByDistId error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }


    }
}
