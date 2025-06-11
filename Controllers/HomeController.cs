using AutoMapper;
using tnki_line_sale_api.Constant;
using tnki_line_sale_api.Utilities;
using tnki_line_sale_api.Models;
using tnki_line_sale_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Data.SqlClient;
using tnki_line_sale_api.Entities;

namespace tnki_line_sale_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private SqlConnection _dbCon;
        private string _connectionString;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        public HomeController(ILogger<HomeController> logger, IOptions<ConnectionString> connectionStrings, IUserService userService, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _logger = logger;
            _connectionString = connectionStrings.Value.PrimaryDatabase;
            _dbCon = new SqlConnection(_connectionString);
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }
        [Authorize]
        [HttpPost("joinTopSpender")]
        public IActionResult joinTopSpender()
        {
            try
            {
                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);
                _logger.LogInformation("joinTopSpender custGuid: " + custGuid);

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                serv.joinTopSpender(_dbCon, custGuid);

                return Ok("SUCCESS");
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("joinTopSpender error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }
        [Authorize]
        [HttpPost("cancelMember")]
        public IActionResult cancelMember()
        {
            try
            {
                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);
                _logger.LogInformation("cancelMember custGuid: " + custGuid);

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                serv.cancelMember(_dbCon,  custGuid);

                return Ok("SUCCESS");
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("cancelMember error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }
       
        [Authorize]
        [HttpPost("rejectCancelMember")]
        public IActionResult rejectCancelMember()
        {
            try
            {
                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);
                _logger.LogInformation("rejectCancelMember custGuid: " + custGuid);

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                serv.rejectCancelMember(_dbCon, custGuid);

                return Ok("SUCCESS");
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("rejectCancelMember error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }
       
        [Authorize]
        [HttpGet("getActiveCampaign")]
        public IActionResult getActiveCampaign()
        {
            try
            {
                _logger.LogInformation("getActiveCampaign ");

                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                CampModel resp = serv.getActiveCampaign(_dbCon);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getActiveCampaign error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }

        [Authorize]
        [HttpPost("Receipt_Upload")]
        public IActionResult Receipt_Upload()
        {
            try
            {
                _logger.LogInformation("Start Receipt_Upload");
                ConnectionHandle.openConnection(_dbCon);
                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);
                StringValues storeGuidStr = "";
                List<string> lstFileName = new List<string>();


                if (Request.Form.Files != null && Request.Form.Files.Count > 0)
                {
                  

                    Request.Form.TryGetValue("storeGuid", out storeGuidStr);
                    for (int i = 0; i < Request.Form.Files.Count(); i++)
                    {
                        string fileName = "";
                        Stream stream = Request.Form.Files[i].OpenReadStream();

                        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fileUpload\\" + ENUM_SERVERURL.folderRec);

                        string extenstion = Path.GetExtension(Request.Form.Files[i].FileName);
                        fileName = Guid.NewGuid().ToString() + (extenstion == null || extenstion.Trim().Equals("") ? ".png" : extenstion);

                        path = Path.Combine(path, fileName);
                        using (FileStream outputFileStream = new FileStream(path, FileMode.Create))
                        {
                            stream.CopyTo(outputFileStream);
                        }
                        stream.Position = 0;

                        lstFileName.Add(fileName);
                    }
                }
                else
                {
                    throw new Exception("Selected file is missing.");
                }

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                serv.uploadReceipt(_dbCon, custGuid, new Guid(storeGuidStr.ToString()), lstFileName);

                return Ok("Success");
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("Receipt_Upload error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }

        }

        [Authorize]
        [HttpPost("ReceiptEdit_Upload")]
        public IActionResult ReceiptEdit_Upload()
        {
            try
            {
                _logger.LogInformation("Start ReceiptEdit_Upload");
                ConnectionHandle.openConnection(_dbCon);
                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);
                StringValues storeGuidStr = "", reqGuidStr = "" ;
                List<string> lstFileName = new List<string>();


                if (Request.Form.Files != null && Request.Form.Files.Count > 0)
                {


                    Request.Form.TryGetValue("storeGuid", out storeGuidStr);
                    Request.Form.TryGetValue("reqGuid", out reqGuidStr);
                    for (int i = 0; i < Request.Form.Files.Count(); i++)
                    {
                        string fileName = "";
                        Stream stream = Request.Form.Files[i].OpenReadStream();

                        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fileUpload\\" + ENUM_SERVERURL.folderRec);

                        string extenstion = Path.GetExtension(Request.Form.Files[i].FileName);
                        fileName = Guid.NewGuid().ToString() + (extenstion == null || extenstion.Trim().Equals("") ? ".png" : extenstion);

                        path = Path.Combine(path, fileName);
                        using (FileStream outputFileStream = new FileStream(path, FileMode.Create))
                        {
                            stream.CopyTo(outputFileStream);
                        }
                        stream.Position = 0;

                        lstFileName.Add(fileName);
                    }
                }
                else
                {
                    throw new Exception("Selected file is missing.");
                }

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                serv.uploadEditReceipt(_dbCon, custGuid, new Guid(reqGuidStr.ToString()), new Guid(storeGuidStr.ToString()), lstFileName);

                return Ok("Success");
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("ReceiptEdit_Upload error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }

        }
        
                [Authorize]
        [HttpGet("insertVisitLog")]
        public IActionResult insertVisitLog(string state)
        {
            try
            {
                _logger.LogInformation("insertVisitLog state:" + state);
                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);

                ConnectionHandle.openConnection(_dbCon);
                if (state == null || state.Trim().Equals(""))
                {
                    return Ok(new m_link_logstate());
                }
                else
                {
                    CampaignService serv = new CampaignService(_logger);
                    state = state.Trim().ToUpper().Replace("#", "");
                    m_link_logstate data = serv.getLinkLogStateByState(_dbCon, state.Trim().ToUpper());
                    if (data != null)
                    {
                        serv.insertVisitLog(_dbCon, custGuid, state);
                    }
                    else
                    {
                        data = new m_link_logstate();
                    }
                    return Ok(data);
                }

            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("insertVisitLog error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }

        [Authorize]
        [HttpPost("insertEventStatePageLoad")]
        public IActionResult insertEventStatePageLoad(EventStateModel req)
        {
            try
            {
                _logger.LogInformation("insertEventStatePageLoad req:" + ConvertUtil.obj2string(req));
                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);

                ConnectionHandle.openConnection(_dbCon);

                CampaignService serv = new CampaignService(_logger);
                serv.insertEventState(_dbCon, custGuid, req);
                return Ok("Success");

            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("insertVisitLog error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }


        }
       

        [Authorize]
        [HttpGet("getListStore")]
        public IActionResult getListStore()
        {
            try
            {
                _logger.LogInformation("getListStore ");

                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                List<StoreModel> resp = serv.getListStore(_dbCon, custGuid);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getListChannel error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }


        [Authorize]
        [HttpGet("getListProduct")]
        public IActionResult getListProduct()
        {
            try
            {
                _logger.LogInformation("getListProduct ");

               
                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                List<ProductModel> resp = serv.getListProduct(_dbCon);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getListProduct error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }

        [Authorize]
        [HttpGet("getHistRec")]
        public IActionResult getHistRec()
        {
            try
            {
                _logger.LogInformation("getHistRec ");

                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                List<HistPointModel> resp = serv.getHistRecByCust(_dbCon, custGuid);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getHistRec error: " + ConvertUtil.obj2string(ex));
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
                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);
                _logger.LogInformation("getRecDetailByGuid [ custGuid:" + custGuid + " ], [reqGuid: " + reqGuid + "]");

                CampaignService serv = new CampaignService(_logger);

                RecDataDetail resp = serv.getRequestForEditByGuid(_dbCon, custGuid,  reqGuid);
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
        [HttpGet("getAllActiveReward")]
        public IActionResult getAllActiveReward()
        {
            try
            {
                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);
                _logger.LogInformation("getAllActiveReward [ custGuid:" + custGuid + " ]");
                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);

                RewardModel resp = serv.getAllActiveReward(_dbCon, custGuid);
                CustProfileModel custData = serv.getCustomerProfile(_dbCon, custGuid);

                resp.custData = custData.custData;
                resp.totalPoint = custData.totalPoint; 

                return Ok(resp);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getAllActiveReward error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }

        [HttpPost("confirmRedeem")]
        public IActionResult confirmRedeem(RedeemConfirmData request)
        {
            try
            {
                _logger.LogInformation("confirmRedeem request: " + ConvertUtil.obj2string(request));

                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                request.redmHistQty = 1;
                serv.confirmRedeemLowTier(_dbCon, request, custGuid);


                return Ok("SUCCESS");
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("confirmRedeem error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }

        [HttpPost("confirmRedeemSpc")]
        public IActionResult confirmRedeemSpc(RedeemConfirmData request)
        {
            try
            {
                _logger.LogInformation("confirmRedeemSpc request: " + ConvertUtil.obj2string(request));

                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                request.redmHistQty = 1;
                serv.confirmRedeemHeightTier(_dbCon, request, custGuid);


                return Ok("SUCCESS");
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("confirmRedeemSpc error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }

        [Authorize]
        [HttpGet("getHistRedeem")]
        public IActionResult getHistRedeem()
        {
            try
            {
                _logger.LogInformation("getHistRedeem ");

                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                List<RewardData> resp = serv.getHistRedeem(_dbCon, custGuid);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getHistRedeem error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }

        [Authorize]
        [HttpGet("getLeaderBoardData")]
        public IActionResult getLeaderBoardData()
        {
            try
            {
                _logger.LogInformation("getLeaderBoardData ");

                Guid custGuid = ConnectionHandle.getCustomerFromToken(_httpContextAccessor);

                ConnectionHandle.openConnection(_dbCon);
                CampaignService serv = new CampaignService(_logger);
                LeaderBoardModel resp = serv.getLeaderBoard(_dbCon, custGuid);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                ConnectionHandle.closeConnection(_dbCon);
                _logger.LogError("getLeaderBoardData error: " + ConvertUtil.obj2string(ex));
                return BadRequest(ex.Message);
            }
            finally
            {
                ConnectionHandle.closeConnection(_dbCon);
            }
        }
    }
}
