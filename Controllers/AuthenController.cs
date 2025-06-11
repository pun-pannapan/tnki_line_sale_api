using AutoMapper;
using tnki_line_sale_api.Entities;
using tnki_line_sale_api.Utilities;
using tnki_line_sale_api.Models;
using tnki_line_sale_api.Repositories;
using tnki_line_sale_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;

namespace tnki_line_sale_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenController : ControllerBase
    {
        private readonly ILogger<AuthenController> _logger;
        private SqlConnection _dbCon;
        private string _connectionString;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        public AuthenController(ILogger<AuthenController> logger, IOptions<ConnectionString> connectionStrings, IUserService userService, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _logger = logger;
            _connectionString = connectionStrings.Value.PrimaryDatabase;
            _dbCon = new SqlConnection(_connectionString);
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        [HttpPost("checkExistLineID")]
        public IActionResult checkExistLineId(CheckLineExistModel req)
        {
            try
            {
                _logger.LogInformation("checkExistLineID req[" + ConvertUtil.obj2string(req) + "]");

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                APIAuthen_ResponseResult resp = serv.checkIsExistingCustomerWithLine(_dbCon, _userService, _mapper, req.lineId, req.lineImg, req.lineDispName);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("checkExistLineId error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }

        [AllowAnonymous]
        [HttpPost("insertPersonalData")]
        public IActionResult insertPersonalData(CustModel req)
        {
            try
            {
                _logger.LogInformation("insertPersonalData req[" + ConvertUtil.obj2string(req) + "]");

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                APIAuthen_ResponseResult resp = serv.insertPersonalData(_dbCon, _userService, req);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("insertPersonalData error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }

        [Authorize]
        [HttpGet("getCustGuidFromToken")]
        public IActionResult getCustGuidFromToken()
        {
            try
            {
                _logger.LogInformation("getCustGuidFromToken ");
                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);

                CustomerRepository repo = new CustomerRepository();
                m_customer custDB = repo.getCustomerWithGuid(_dbCon, custGuid);
                if (custDB == null)
                {
                    return Ok(Guid.Empty);
                }
                else
                {
                    return Ok(custDB.cust_guid);
                }

            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getCustGuidFromToken error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }

        }

        [Authorize]
        [HttpGet("getCustomerData")]
        public IActionResult getCustomerData()
        {
            try
            {
                _logger.LogInformation("getCustomerData ");
                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);
                CampaignService serv = new CampaignService(_logger);

                CustProfileModel resp =  serv.getCustomerProfile(_dbCon, custGuid);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getCustomerData error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }

        }

        [Authorize]
        [HttpPost("updatePersonalData")]
        public IActionResult updatePersonalData(CustModel req)
        {
            try
            {
                _logger.LogInformation("updatePersonalData req[" + ConvertUtil.obj2string(req) + "]");
                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);
                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                 serv.updatePersonalData(_dbCon, _userService, req, custGuid);

                return Ok("SUCCESS");
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("updatePersonalData error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }

        // GET: Owner
        [AllowAnonymous]
        [HttpGet("adminLogin")]
        public IActionResult adminLogin(string userName, string password)
        {
            try
            {
                _logger.LogInformation("adminLogin [ userName:" + userName + ", password:" + password + " ]");

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                Login_ResponseResult resp = serv.adminLogin(_dbCon, _userService, userName, password);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("adminLogin error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }

        }

    }
}
