using AutoMapper;
using tnki_line_sale_api.Utilities;
using tnki_line_sale_api.Models;
using tnki_line_sale_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using ClosedXML.Excel;
using ExcelDataReader;

namespace tnki_line_sale_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private SqlConnection _dbCon;
        private string _connectionString;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        public AdminController(ILogger<AdminController> logger, IOptions<ConnectionString> connectionStrings, IUserService userService, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _logger = logger;
            _connectionString = connectionStrings.Value.PrimaryDatabase;
            _dbCon = new SqlConnection(_connectionString);
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        // GET: Owner
        [Authorize]
        [HttpPost("getListForAppvRec")]
        public IActionResult getListForAppvRec(SearchRecCriteria req)
        {
            try
            {
                Guid userGuid = ConnectionHandle.getAdminFromToken(_httpContextAccessor);
                _logger.LogInformation("getListForAppvRec [ req:" + ConvertUtil.obj2string(req) + " ]");

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                List<RecDataDetail> resp = serv.getListForAppvRec(_dbCon, userGuid, req);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getListForAppvRec error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }

        }

        [Authorize]
        [HttpPost("getListForAllRec")]
        public IActionResult getListForAllRec(SearchRecCriteria req)
        {
            try
            {
                Guid userGuid = ConnectionHandle.getAdminFromToken(_httpContextAccessor);
                _logger.LogInformation("getListForAllRec [ req:" + ConvertUtil.obj2string(req) + " ]");

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                List<RecDataDetail> resp = serv.getListForAllRec(_dbCon, userGuid, req);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getListForAllRec error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }

        }

        [Authorize]
        [HttpGet("getRecDetailByGuid")]
        public IActionResult getRecDetailByGuid(Guid reqGuid)
        {
            try
            {
                Guid userGuid = ConnectionHandle.getAdminFromToken(_httpContextAccessor);
                _logger.LogInformation("getRecDetailByGuid [ userGuid:" + userGuid + " ], [reqGuid: " + reqGuid + "]");

                CampaignService serv = new CampaignService(_logger);

                RecDataDetail resp = serv.getRequestByGuid(_dbCon, reqGuid);
              
                return Ok(resp);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getRecDetailByGuid error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }

        


        [Authorize]
        [HttpPost("updateRecProductDetail")]
        public IActionResult updateRecProductDetail(UpdateProductRec req)
        {
            try
            {
                Guid userGuid = ConnectionHandle.getAdminFromToken(_httpContextAccessor);
                _logger.LogInformation("updateRecProductDetail [ userGuid:" + userGuid + " ], [req: " + ConvertUtil.obj2string(req) + " ]");
                ConnectionHandle.openConnection(_dbCon);

                CampaignService serv = new CampaignService(_logger);
                serv.updateRecProductDetail(_dbCon, userGuid, req);

                return Ok("Success");
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("updateRecProductDetail error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }

        [Authorize]
        [HttpPost("getAllHistRedeem")]
        public IActionResult getAllHistRedeem(SearchRecCriteria req)
        {
            try
            {
                Guid userGuid = ConnectionHandle.getAdminFromToken(_httpContextAccessor);
                _logger.LogInformation("getAllHistRedeem [ req:" + ConvertUtil.obj2string(req) + " ]");

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                List<RewardData> resp = serv.getAdminHistRedeem(_dbCon, req);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getAllHistRedeem error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }

        }

        [Authorize]
        [HttpGet("HistRedeem_Download")]
        public async Task<IActionResult> HistRedeem_Download()
        {
            try
            {
                _logger.LogInformation("Start HistRedeem_Download");
                ConnectionHandle.openConnection(_dbCon);

                CampaignService serv = new CampaignService(_logger);
                SearchRecCriteria req = new SearchRecCriteria();
                req.dateFrom = DateTime.Now.AddDays(-150).ToString("yyyy-MM-dd");
                req.dateTo = DateTime.Now.ToString("yyyy-MM-dd");

                List<RewardData> lstData = serv.getAdminHistRedeem(_dbCon, req);
                XLWorkbook wb = await serv.generateHistRedeemToExcel(lstData);
                _logger.LogInformation("HistRedeem_Download Success ");

                var ms = new MemoryStream();
                wb.SaveAs(ms);
                return File(ms.ToArray(), "application/octet-stream", "omo_historyRedeem_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("StoreLink_Download error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }

        }

        [Authorize]
        [HttpPost("Tracking_Upload")]
        public IActionResult Tracking_Upload()
        {
            try
            {
                _logger.LogInformation("Start Tracking_Upload");
                ConnectionHandle.openConnection(_dbCon);

                RewardDataResp resp = new RewardDataResp();

                if (Request.Form.Files != null && Request.Form.Files.Count == 1)
                {
                    Stream stream = Request.Form.Files[0].OpenReadStream();


                    string fileName = Request.Form.Files[0].FileName;
                    IExcelDataReader reader = null;

                    if (fileName.EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (fileName.EndsWith(".xlsx"))
                    {
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    else
                    {
                        throw new Exception("The file format is not supported.");
                    }

                    if (reader != null)
                    {
                        CampaignService serv = new CampaignService(_logger);
                        resp = serv.readUploadTrackingFromFile(_dbCon, reader);
                    }
                    else
                    {
                        throw new Exception("Selected file is missing.");
                    }

                    return Ok(resp);
                }
                else
                {
                    throw new Exception("Selected file is missing.");
                }
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("Tracking_Upload error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }

        }

        [Authorize]
        [HttpPost("Tracking_SaveUpload")]
        public IActionResult Tracking_SaveUpload(List<RewardData> data)
        {
            try
            {
                ConnectionHandle.openConnection(_dbCon);
                _logger.LogInformation("Tracking_SaveUpload [count data: " + data.Count() + "]");

             
                if (data == null || data.Count() == 0)
                {
                    throw new Exception("Data is missing.");
                }

                CampaignService serv = new CampaignService(_logger);
                serv.saveTrackingFromFile(_dbCon, data);

                _logger.LogInformation("End CoreValue_SaveUpload");

                return Ok("Success");
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("CoreValue_SaveUpload error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }

        }
    }
}
