using AutoMapper;
using tnki_line_sale_api.Constant;
using tnki_line_sale_api.Entities;
using tnki_line_sale_api.Models;
using tnki_line_sale_api.Repositories;
using tnki_line_sale_api.Utilities;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.VariantTypes;
using ExcelDataReader;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace tnki_line_sale_api.Services
{
    public class CampaignService
    {
        public System.Globalization.CultureInfo _cultureTHInfo = new System.Globalization.CultureInfo("th-TH");
        private readonly ILogger _logger;
        public CampaignService(ILogger logger)
        {
            _logger = logger;
        }

        internal RewardModel getAllActiveReward(SqlConnection dbCon, Guid cust_guid)
        {
            CampaignRepository campRepo = new CampaignRepository();
            RewardModel resp = new RewardModel();
            resp.lstRewardLowTier = campRepo.getAllActiveRewardByType(dbCon, ENUM_REWARDTYPE.lowTier);
            resp.lstRewardHighTier = campRepo.getAllActiveRewardByType(dbCon, ENUM_REWARDTYPE.hightTier);

            DateTime currentDate = DateTime.Now;
            m_campaign_period campDB = campRepo.getActiveCampDBModel(dbCon);
            t_customer_summary custSum = campRepo.getCustSumByCustGuid(dbCon, cust_guid); 


            for (int i = 0; resp.lstRewardLowTier != null && i < resp.lstRewardLowTier.Count(); i++)
            {
                checkIsEnableRedeemForRewardLowTier(dbCon, cust_guid, campRepo, resp.lstRewardLowTier[i], currentDate, campDB, custSum);
            }
            for (int i = 0; resp.lstRewardHighTier != null && i < resp.lstRewardHighTier.Count(); i++)
            {
                checkIsEnableRedeemForRewardHeightTier(dbCon, cust_guid, campRepo, resp.lstRewardHighTier[i], currentDate, campDB, custSum);
            }
            return resp;
        }

        internal  void checkIsEnableRedeemForRewardLowTier(SqlConnection dbCon, Guid cust_guid, CampaignRepository campRepo, RewardData rewardData, DateTime currentDate, m_campaign_period campDB, t_customer_summary custSum)
        {
                t_history_redeem hist = campRepo.getRedeemHistDBByCustAndReward(dbCon, cust_guid, rewardData.rewardGuid);
                // check ว่าอยู่ใน camp การแลกรึป่าว
                if (campDB.camp_redeem_startdate <= currentDate && currentDate <= campDB.camp_redeem_enddate)
                {
                    rewardData.isEnable = (rewardData.rewardRemainStock <= 0 ? false : true);

                    if (rewardData.isEnable)
                    {
                        // check ว่าแต้มถึงรึป่าว 
                        if (custSum.cust_sum_remain_point >= rewardData.rewardBurnPoint)
                        {
                            rewardData.isEnable = true;

                            // check ว่าเคยแลกไปรึยัง                     
                            if (hist == null)
                            {
                                rewardData.isEnable = true;
                            }
                            else
                            {
                                rewardData.rewardImage = rewardData.rewardImageIna; 
                                rewardData.isEnable = false;

                            }
                        }
                        else
                        {
                            rewardData.isEnable = false;
                        }
                    }
                }
                if (hist != null)
                {
                    rewardData.rewardImage = rewardData.rewardImageIna;
                    rewardData.isEnable = false;

                }
            }

        internal void checkIsEnableRedeemForRewardHeightTier(SqlConnection dbCon, Guid cust_guid, CampaignRepository campRepo, RewardData rewardData, DateTime currentDate, m_campaign_period campDB, t_customer_summary custSum)
        {
                t_history_redeem hist = campRepo.getRedeemHistDBByCustAndReward(dbCon, cust_guid, rewardData.rewardGuid);
                // check ว่าอยู่ใน camp การแลกรึป่าว
                if (campDB.camp_redeem_startdate <= currentDate && currentDate <= campDB.camp_redeem_enddate)
                {
                    rewardData.isEnable = (rewardData.rewardRemainStock <= 0 ? false : true);

                    if (rewardData.isEnable)
                    {
                        int condReceivePoint = campRepo.getReceivePointByStoreGroup(dbCon, cust_guid, rewardData.rewardStoreGroup.Trim());
                        // check ว่าแต้มถึงรึป่าว 
                        if (condReceivePoint >= rewardData.rewardMinPointCond)
                        {
                            rewardData.isEnable = true;
                            // check ว่าเคยแลกไปรึยัง
                            if (hist == null)
                            {
                                rewardData.isEnable = true;
                            }
                            else
                            {
                                rewardData.isEnable = false;
                            }
                        }
                        else
                        {
                            rewardData.isEnable = false;
                        }
                    }
                }
                if (hist != null)
                {
                    rewardData.rewardImage = rewardData.rewardImageIna;
                    rewardData.isEnable = false;
                }
            }

        internal List<AddrProviceModel> getProvince(SqlConnection dbCon)
        {
            CustomerRepository repo = new CustomerRepository();
            List<AddrProviceModel>  resp = repo.getProvince(dbCon);
            return resp;
        }

        internal List<AddrDistrictModel> getDistByProvId(SqlConnection dbCon, long provId)
        {
            CustomerRepository repo = new CustomerRepository();
            if (provId == 0)
            {
                throw new Exception("ข้อมูลจังหวัดไม่ถูกต้อง");
            }
            List<AddrDistrictModel> resp = repo.getDistByProvId(dbCon, provId);
            return resp;
        }

        internal List<AddrSubDistrictModel> getSubDistByDistId(SqlConnection dbCon, long distId)
        {
            CustomerRepository repo = new CustomerRepository();
            if (distId == 0)
            {
                throw new Exception("ข้อมูลอำเภอไม่ถูกต้อง");
            }
            List<AddrSubDistrictModel> resp = repo.getSubDistByDistId(dbCon, distId);
            return resp;
        }
        public APIAuthen_ResponseResult checkIsExistingCustomerWithLine(SqlConnection dbCon, IUserService userService, IMapper mapper, string lineId, string linePic, string lineDispName)
        {
            CustomerRepository repo = new CustomerRepository();
            m_customer custDb = repo.getCustomerWithLineId(dbCon, lineId);

            // update line pic into db
            if (custDb != null)
            {
                custDb.cust_line_image = (linePic != null ? linePic.Trim() : "");
                custDb.cust_line_display_name = (lineDispName != null ? lineDispName.Trim() : "");
                repo.updateCustomerLineData(dbCon, custDb);
            }

            APIAuthen_ResponseResult resp = new APIAuthen_ResponseResult();
            resp = assignAuthenResp(dbCon, repo, custDb, resp, userService);

            return resp;
        }
       
        public APIAuthen_ResponseResult assignAuthenResp(SqlConnection dbCon, CustomerRepository repo, m_customer cust, APIAuthen_ResponseResult resp, IUserService userServ)
        {
            if (cust != null)
            {
                resp.custGuid = cust.cust_guid;
                resp.custTel = cust.cust_phone_number;
                resp.customerName = cust.cust_firstname + " " + cust.cust_lastname;
                resp.Token = userServ.generateCustomerJwtToken(cust.cust_guid);
                resp.isExisted = true;
                resp.linePic = cust.cust_line_image;
            }
            else
            {
                resp.custGuid = Guid.Empty;
                resp.customerName = "";
                resp.custTel = "";
                resp.Token = "";
                resp.isExisted = false;
                resp.linePic = "";
            }
            return resp;
        }


        private bool validateCustRegister(CustModel req)
        {
            if (req.custFirstName == null || req.custFirstName.Trim().Equals(""))
            {
                throw new Exception("Firstname is required.");
            }
            if (req.custLineId == null || req.custLineId.Trim().Equals(""))
            {
                throw new Exception("Line ID is required.");
            }
            if (req.custLineImg == null || req.custLineImg.Trim().Equals(""))
            {
                req.custLineImg = "";
            }
            if (req.custLineDisplayName == null || req.custLineDisplayName.Trim().Equals(""))
            {
                req.custLineDisplayName = "";
            }

            if (req.custLastName == null || req.custLastName.Trim().Equals(""))
            {
                throw new Exception("Lastname is required.");
            }
            if (req.custTel == null || req.custTel.Trim().Equals("") || req.custTel.Replace("-", "").Trim().Equals(""))
            {
                throw new Exception("Tel no is required.");
            }
           if (!req.custCheckPrivacy)
            {
                throw new Exception("Privacy is required.");
            }
            return true;
        }

        internal APIAuthen_ResponseResult insertPersonalData(SqlConnection dbCon, IUserService userService, CustModel req)
        {
            CustomerRepository repo = new CustomerRepository();
         

            bool isValid = validateCustRegister(req);
            req.custTel = req.custTel.Replace("-", "");

            

            m_customer dtoCust = new m_customer();
            m_customer dtoCust_line = repo.getCustomerWithLineId(dbCon, req.custLineId);
            m_customer dtoCust_tel = repo.getCustomerWithTel(dbCon, req.custTel);

            if (dtoCust_tel != null || dtoCust_line != null )
            {
                throw new Exception("เบอร์โทรศัพท์ / Line นี้ถูกใช้งานแล้ว กรุณาตรวจสอบอีกครั้ง");
            }

            if (dtoCust_tel != null || dtoCust_line != null)
            {
                dtoCust = dtoCust_line;
                dtoCust.cust_check_pdpa2 = req.custCheckPrivacy;
                dtoCust.cust_check_pdpa1 = req.custCheckTerm;
                dtoCust.cust_firstname = req.custFirstName.Trim();
                dtoCust.cust_guid = dtoCust_line.cust_guid; ;
                dtoCust.cust_lastname = req.custLastName.Trim();
                dtoCust.cust_line_display_name = req.custLineDisplayName.Trim();
                dtoCust.cust_line_id = req.custLineId.Trim();
                dtoCust.cust_line_image = req.custLineImg.Trim();
                dtoCust.cust_status = ENUM_STATUS.active;
                dtoCust.cust_phone_number = req.custTel.Trim();
                dtoCust.cust_updatedate = DateTime.Now;
                dtoCust.cust_check_topspender = req.custCheckTopSpender;

                repo.updatePersonalData(dbCon, dtoCust);
            }
            else
            {
                dtoCust.cust_check_pdpa2 = req.custCheckPrivacy;
                dtoCust.cust_check_pdpa1 = req.custCheckTerm;
                dtoCust.cust_firstname = req.custFirstName.Trim();
                dtoCust.cust_guid = Guid.NewGuid();
                dtoCust.cust_lastname = req.custLastName.Trim();
                dtoCust.cust_line_display_name = req.custLineDisplayName.Trim();
                dtoCust.cust_line_id = req.custLineId.Trim();
                dtoCust.cust_line_image = req.custLineImg.Trim();
                dtoCust.cust_remark = "";
                dtoCust.cust_status = ENUM_STATUS.active;
                dtoCust.cust_phone_number = req.custTel.Trim();
                dtoCust.cust_updatedate = DateTime.Now;
                dtoCust.cust_address1 = "";
                dtoCust.cust_address2 = "";
                dtoCust.cust_prov_id = 0;
                dtoCust.cust_dist_id = 0;
                dtoCust.cust_subdist_id = 0;
                dtoCust.cust_check_topspender = req.custCheckTopSpender;

                t_customer_summary custSumCustDB = new t_customer_summary();
                custSumCustDB = new t_customer_summary();
                custSumCustDB.cust_sum_cust_guid = dtoCust.cust_guid;
                custSumCustDB.cust_sum_guid = Guid.NewGuid();
                custSumCustDB.cust_sum_remain_point = 0;
                custSumCustDB.cust_sum_total_point = 0;
                custSumCustDB.cust_sum_updatedate = DateTime.Now;

                repo.insertPersonalData(dbCon, dtoCust, custSumCustDB);
            }

            APIAuthen_ResponseResult resp = new APIAuthen_ResponseResult();
            resp = assignAuthenResp(dbCon, repo, dtoCust, resp, userService);
            return resp;
        }

        internal void joinTopSpender(SqlConnection dbCon, Guid cust_guid)
        {
            CustomerRepository repo = new CustomerRepository();

            m_customer dtoCust = repo.getCustomerWithGuid(dbCon, cust_guid);
            if (dtoCust == null)
            {
                throw new Exception("Cust Guid is incorrect");
            }

            dtoCust.cust_check_topspender = true;
            repo.updatePersonalData(dbCon, dtoCust);
        }
        internal void cancelMember(SqlConnection dbCon, Guid cust_guid)
        {
            CustomerRepository repo = new CustomerRepository();

            m_customer dtoCust = repo.getCustomerWithGuid(dbCon, cust_guid);
            if (dtoCust == null)
            {
                throw new Exception("Cust Guid is incorrect");
            }

            dtoCust.cust_status = ENUM_STATUS.inactive;
            dtoCust.cust_firstname = "CANCEL MEMBER";
            dtoCust.cust_lastname = "CANCEL MEMBER";
            dtoCust.cust_phone_number = "CANCEL MEMBER";
            dtoCust.cust_line_display_name = "CANCEL MEMBER";
            dtoCust.cust_line_image = "CANCEL MEMBER";
            dtoCust.cust_line_id = "CANCEL_" + dtoCust.cust_line_id;
            dtoCust.cust_remark = "CANCEL_REQUEST";
            dtoCust.cust_address1 = "";
            dtoCust.cust_dist_id = 0;
            dtoCust.cust_prov_id = 0;
            dtoCust.cust_subdist_id = 0;
            dtoCust.cust_updatedate = DateTime.Now;
            repo.updatePersonalData(dbCon, dtoCust);
        }

        internal void rejectCancelMember(SqlConnection dbCon, Guid cust_guid)
        {
            CustomerRepository repo = new CustomerRepository();

            m_customer dtoCust = repo.getCustomerWithGuid(dbCon, cust_guid);
            if (dtoCust == null)
            {
                throw new Exception("Cust Guid is incorrect");
            }


            dtoCust.cust_remark = "";
            dtoCust.cust_updatedate = DateTime.Now;
            repo.updatePersonalData(dbCon, dtoCust);
        }
        internal void updatePersonalData(SqlConnection dbCon, IUserService userService, CustModel req, Guid cust_guid)
        {
            CustomerRepository repo = new CustomerRepository();

            bool isValid = validateCustRegister(req);
            req.custTel = req.custTel.Replace("-", "");

            m_customer dtoCust = repo.getCustomerWithGuid(dbCon, cust_guid);
            m_customer dtoCustCheckTel = repo.getCustomerWithTel(dbCon, req.custTel);
            if (dtoCustCheckTel != null && dtoCustCheckTel.cust_guid != dtoCust.cust_guid)
            {
                throw new Exception("เบอร์โทรศัพท์ / Line นี้ถูกใช้งานแล้ว กรุณาตรวจสอบอีกครั้ง");
            }


            dtoCust.cust_address1 = (req.custAddr01 == null ? "" : req.custAddr01.Trim());
            dtoCust.cust_address2 = "";
            dtoCust.cust_prov_id = req.custProvId;
            dtoCust.cust_dist_id = req.custDistId;
            dtoCust.cust_subdist_id = req.custSubDistId;
            dtoCust.cust_check_pdpa2 = req.custCheckPrivacy;
            dtoCust.cust_check_pdpa1 = req.custCheckTerm;
            dtoCust.cust_firstname = req.custFirstName.Trim();
            dtoCust.cust_lastname = req.custLastName.Trim();
            dtoCust.cust_line_display_name = req.custLineDisplayName.Trim();
            dtoCust.cust_line_image = req.custLineImg.Trim();
            dtoCust.cust_status = ENUM_STATUS.active;
            dtoCust.cust_phone_number = req.custTel.Trim();
            dtoCust.cust_updatedate = DateTime.Now;
            dtoCust.cust_check_topspender = req.custCheckTopSpender; 
            repo.updatePersonalData(dbCon, dtoCust);
          

        }


        internal CustProfileModel getCustomerProfile(SqlConnection dbCon, Guid cust_guid)
        {
            CustomerRepository repo = new CustomerRepository();

            CustModel custData = repo.getCustProfile(dbCon, cust_guid);
            if (custData == null)
            {
                throw new Exception("Cust Guid is missing");
            }
            else
            {
                CampaignRepository campRepo = new CampaignRepository();
                CustProfileModel resp = new CustProfileModel();
                resp.custData = custData;
                t_customer_summary custSumDB = campRepo.getCustSumByCustGuid(dbCon, cust_guid);
                if (custSumDB == null)
                {
                    resp.totalPoint = 0;
                } else
                {
                    resp.totalPoint = custSumDB.cust_sum_remain_point;
                }

                return resp;
            }
        }

        internal CampModel getActiveCampaign(SqlConnection dbCon)
        {
            CampaignRepository campRepo = new CampaignRepository();
            CampModel resp = campRepo.getActiveCamp(dbCon);

            return resp;
        }
        internal List<StoreModel> getListStore(SqlConnection dbCon, Guid cust_guid)
        {
            CampaignRepository campRepo = new CampaignRepository();
            List<StoreModel> resp = campRepo.getListStore(dbCon);

            return resp;
        }

        internal List<ProductModel> getListProduct(SqlConnection dbCon)
        {
            CampaignRepository campRepo = new CampaignRepository();
            List<ProductModel> resp = campRepo.getListProduct(dbCon);

            return resp;
        }
        internal void uploadReceipt(SqlConnection dbCon, Guid cust_guid, Guid store_guid, List<string> lstFileName)
        {

            CustomerRepository repo = new CustomerRepository();
            CampaignRepository campRepo = new CampaignRepository();

            m_customer custDB =  repo.getCustomerWithGuid(dbCon, cust_guid);
            if (custDB == null)
            {
                throw new Exception("ข้อมูลไม่ถูกต้อง cust_guid ไม่มีในระบบ");
            }

            m_store storeDB = campRepo.getStoreByGuid(dbCon, store_guid);
            if (storeDB == null)
            {
                throw new Exception("ข้อมูลไม่ถูกต้อง Store Guid ไม่มีในระบบ");
            }

            t_request req = new t_request();
            req.req_store_guid = storeDB.store_guid;
            req.req_createdate = DateTime.Now;
            req.req_cust_guid = cust_guid;
            req.req_guid = Guid.NewGuid() ;
            req.req_receipt_no = "";
            req.req_remark = "";
            req.req_status = ENUM_STATUS.active;
            req.req_total_point = 0;
            req.req_updatedate = DateTime.Now;

            if (lstFileName == null || lstFileName.Count() == 0)
            {
                throw new Exception("ไม่พบข้อมูลรูปภาพใบเสร็จ กรุณาตรวจสอบอีกครั้ง");
            }

            List<t_request_image> lstImg = new List<t_request_image>(); 
            for (int i = 0; i < lstFileName.Count(); i++)
            {
                t_request_image reqImg = new t_request_image();
                reqImg.req_img_guid =Guid.NewGuid() ;
                reqImg.req_img_image_path = lstFileName[i];
                reqImg.req_img_req_guid = req.req_guid;
                lstImg.Add(reqImg);
            }
            


            campRepo.insertRequest(dbCon, req, lstImg);

            LineAndMailService lineServ = new LineAndMailService(); 
            lineServ.sendLine_Wait(custDB.cust_line_id, storeDB.store_name, req.req_createdate.ToString("dd MMM yyyy HH:mm", this._cultureTHInfo));
        }
        internal void uploadEditReceipt(SqlConnection dbCon, Guid cust_guid,Guid req_guid, Guid store_guid, List<string> lstFileName)
        {

            CustomerRepository repo = new CustomerRepository();
            CampaignRepository campRepo = new CampaignRepository();

            m_customer custDB = repo.getCustomerWithGuid(dbCon, cust_guid);
            if (custDB == null)
            {
                throw new Exception("ข้อมูลไม่ถูกต้อง cust_guid ไม่มีในระบบ");
            }

            m_store storeDB = campRepo.getStoreByGuid(dbCon, store_guid);
            if (storeDB == null)
            {
                throw new Exception("ข้อมูลไม่ถูกต้อง Store Guid ไม่มีในระบบ");
            }
            t_request req = campRepo.getRequestForEditByGuid(dbCon, req_guid); 
            if (req.req_cust_guid != custDB.cust_guid)
            {
                throw new Exception("ใบเสร็จไม่ถูกต้อง");
            }
            req.req_store_guid = storeDB.store_guid;
            req.req_receipt_no = "";
            req.req_status = ENUM_STATUS.active;
            req.req_total_point = 0;
            req.req_updatedate = DateTime.Now;

            if (lstFileName == null || lstFileName.Count() == 0)
            {
                throw new Exception("ไม่พบข้อมูลรูปภาพใบเสร็จ กรุณาตรวจสอบอีกครั้ง");
            }

            List<t_request_image> lstImg = new List<t_request_image>();
            for (int i = 0; i < lstFileName.Count(); i++)
            {
                t_request_image reqImg = new t_request_image();
                reqImg.req_img_guid = Guid.NewGuid();
                reqImg.req_img_image_path = lstFileName[i];
                reqImg.req_img_req_guid = req.req_guid;
                lstImg.Add(reqImg);
            }
            campRepo.updateRequest(dbCon, req, lstImg);

            LineAndMailService lineServ = new LineAndMailService();
            lineServ.sendLine_Wait(custDB.cust_line_id, storeDB.store_name, req.req_createdate.ToString("dd MMM yyyy HH:mm", this._cultureTHInfo));
        }

        internal m_link_logstate getLinkLogStateByState(SqlConnection dbCon, string state)
        {
            CampaignRepository repo = new CampaignRepository();
            m_link_logstate data = repo.getLinkLogStateByState(dbCon, state);
            return data;
        }
       
        internal void insertVisitLog(SqlConnection dbCon, Guid cust_guid, string state)
        {
            CampaignRepository repo = new CampaignRepository();
            t_log_visit data = new t_log_visit();
            data.log_createdate = DateTime.Now;
            data.log_cust_guid = cust_guid;
            data.log_guid = Guid.NewGuid();
            data.log_remark = "";
            data.log_state = state.Trim().ToUpper();

            repo.insertLogVisit(dbCon, data);
        }

        public void insertEventState(SqlConnection dbCon, Guid cust_guid, EventStateModel req)
        {
            List<t_event_state> lstDataDB = new List<t_event_state>();
            CampaignRepository campRepo = new CampaignRepository();

            Guid groupGuid = Guid.NewGuid();
            Guid sessionGuid = Guid.Empty;

            DateTime dtNow = DateTime.Now.AddMinutes(ENUM_CONFIG.stateMin);

            var prevState = campRepo.getFirstEventStateInPeriod(dbCon, cust_guid, dtNow);
            if (prevState == null)
            {
                sessionGuid = Guid.NewGuid();
            }
            else
            {
                if (prevState.ev_state_session_guid != Guid.Empty)
                {
                    sessionGuid = prevState.ev_state_session_guid;
                }
                else
                {
                    sessionGuid = Guid.NewGuid(); 
                }
            }


            

            for (int i = 0; req.lstEventParam != null && i < req.lstEventParam.Count(); i++)
            {
                t_event_state evState = new t_event_state();
                evState.ev_state_session_guid = sessionGuid;
                evState.ev_state_cust_guid = cust_guid;
                evState.ev_state_group_guid = groupGuid;
                evState.ev_state_name = req.eventName.Trim();
                evState.ev_state_eventtype = req.eventStateType.Trim().ToUpper();
                evState.ev_state_ref01 = "";
                evState.ev_state_ref02 = "";
                evState.ev_state_remark = "";
                evState.ev_state_updatedate = DateTime.Now;
                evState.ev_state_guid = Guid.NewGuid();
                evState.ev_state_paramname = req.lstEventParam[i].paramName.Trim();
                evState.ev_state_paramvalue = req.lstEventParam[i].paramValue.Trim();
                lstDataDB.Add(evState);
            }

            if (lstDataDB == null || lstDataDB.Count() == 0)
            {
                t_event_state evState = new t_event_state();
                evState.ev_state_session_guid = sessionGuid;
                evState.ev_state_cust_guid = cust_guid;
                evState.ev_state_group_guid = groupGuid;
                evState.ev_state_name = req.eventName.Trim();
                evState.ev_state_eventtype = req.eventStateType.Trim().ToUpper();
                evState.ev_state_ref01 = "";
                evState.ev_state_ref02 = "";
                evState.ev_state_remark = "";
                evState.ev_state_updatedate = DateTime.Now;
                evState.ev_state_paramname = "";
                evState.ev_state_paramvalue = "";
                evState.ev_state_guid = Guid.NewGuid();
                lstDataDB.Add(evState);
            }


            campRepo.insertEventState(dbCon, lstDataDB);
        }

        internal List<HistPointModel> getHistRecByCust(SqlConnection dbCon, Guid cust_guid)
        {
            CampaignRepository campRepo = new CampaignRepository();
            List<HistPointModel> resp = campRepo.getHistRecByCust(dbCon, cust_guid);
            for (int i = 0; resp != null && i < resp.Count(); i++)
            {
                resp[i].requestDate_str = resp[i].requestDate.ToString("dd MMM yyyy HH:mm", this._cultureTHInfo);
                resp[i].statusText = (getStatusText(resp[i].status)); 
            }
            return resp;
        }
        internal string getStatusText(string status)
        {
            if (status.Equals(ENUM_STATUS.active))
            {
                return ENUM_STATUS.active_txt; 
            }
            else if (status.Equals(ENUM_STATUS.approve))
            {
                return ENUM_STATUS.approve_txt;
            }
            else if (status.Equals(ENUM_STATUS.reject))
            {
                return ENUM_STATUS.reject_txt;
            }

            return "";
        }


        internal List<RecDataDetail> getListForAppvRec(SqlConnection dbCon, Guid userGuid, SearchRecCriteria req)
        {
            CampaignRepository campRepo = new CampaignRepository();
           
            List<RecDataDetail> resp = new List<RecDataDetail>();
            if (req.dateFrom == null || req.dateFrom.Trim().Equals(""))
            {
                req.dateFrom = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd");
            }

            if (req.dateTo == null || req.dateTo.Trim().Equals(""))
            {
                req.dateTo = DateTime.Now.ToString("yyyy-MM-dd");
            }

            resp = campRepo.getCustomerReqByCriteria(dbCon, req);

            return resp;
        }

        internal List<RecDataDetail> getListForAllRec(SqlConnection dbCon, Guid userGuid, SearchRecCriteria req)
        {
            CampaignRepository campRepo = new CampaignRepository();
            
            List<RecDataDetail> resp = new List<RecDataDetail>();
            if (req.dateFrom == null || req.dateFrom.Trim().Equals(""))
            {
                req.dateFrom = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd");
            }

            if (req.dateTo == null || req.dateTo.Trim().Equals(""))
            {
                req.dateTo = DateTime.Now.ToString("yyyy-MM-dd");
            }

            resp = campRepo.getCustomerReqForAllByCriteria(dbCon, req);

            return resp;
        }

        public RecDataDetail getRequestByGuid(SqlConnection dbCon, Guid req_guid)
        {
            try
            {
                CampaignRepository repo = new CampaignRepository();
                RecDataDetail resp = repo.getCustomerReqByGuid(dbCon, req_guid);
              
                resp.lstRecProd = repo.getRecProdByReqGuid(dbCon, req_guid);
                resp.lstRecImg = repo.getRecImageByReqGuid(dbCon, req_guid);
                resp.dtoRecProd = new RecProdData();
           
                resp.lstRecProd = (resp.lstRecProd == null ? new List<RecProdData>() : resp.lstRecProd);
                resp.maxProdSeq = (resp.lstRecProd == null ? 0 : resp.lstRecProd.Count);
                resp.lstProduct = new List<ProdModel>();


                resp.lstUOM = new List<string>();
                resp.lstUOM.Add("ลัง");
                resp.lstUOM.Add("แพค");
                resp.lstUOM.Add("ชิ้น");

                resp.lstReason = new List<string>();
                resp.lstReason.Add("ใบเสร็จซ้ำ");
                resp.lstReason.Add("ไม่มีสินค้าที่ร่วมรายการ");
                resp.lstReason.Add("กรุณาส่งใบเสร็จที่ชัดเจน เห็นวันที่ เลขที่ใบเสร็จ ชื่อร้านค้า รายการสินค้า และราคาสินค้า");
                resp.lstReason.Add("วันที่ไม่ถูกต้อง");
                resp.lstReason.Add("ไม่เห็นวันที่");
                resp.lstReason.Add("ไม่เห็นเลขที่ใบเสร็จ");
                resp.lstReason.Add("ไม่เห็นชื่อร้านค้า");
                resp.lstReason.Add("ใบเสร็จไม่ถูกต้อง");
                resp.lstReason.Add("ร้านค้าไม่ร่วมรายการ");
                resp.lstReason.Add("กรุณาส่งใบกำกับภาษี/ใบเสร็จรับเงินเท่านั้น");
                resp.lstReason.Add("กรุณาลงชื่อร้านค้าให้ตรงกับใบเสร็จร้านค้าที่ส่งมา");
                resp.lstReason.Add("กรุณาส่ง 1 เลขที่ใบเสร็จต่อ 1 ครั้ง");
                resp.lstReason.Add("ไม่เสร็จไม่ถูกต้อง/ไม่รับบิลเงินสดเขียนมือ");
                resp.lstReason.Add("ต้องเป็นใบเสร็จสินค้าจากร้านค้าที่จดทะเบียนถูกต้องตามกฎหมาย");
                resp.lstReason.Add("SHP/LZD รับเฉพาะใบกำกับภาษีที่ออกโดยร้านค้า official เท่านั้น");
                resp.lstReason.Add("อื่นๆ");

                return resp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }
        public RecDataDetail getRequestForEditByGuid(SqlConnection dbCon,Guid cust_guid, Guid req_guid)
        {
            try
            {
                CampaignRepository repo = new CampaignRepository();
                RecDataDetail resp = repo.getCustomerReqByGuid(dbCon, req_guid);
                if (cust_guid != resp.custGuid)
                {
                    throw new Exception("เลือกใบเสร็จไม่ถูกต้อง");
                }

                resp.lstRecImg = repo.getRecImageByReqGuid(dbCon, req_guid);
           

                return resp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }

        internal t_customer_point_history generateCustPointHist(Guid sumGuid, Guid cust_guid, string actLog, string actType, int earnPoint, int burnPoint, string storeGroup)
        {
            t_customer_point_history pointHist = new t_customer_point_history();
            pointHist.cust_point_activity_log = actLog;
            pointHist.cust_point_act_type = actType;
            pointHist.cust_point_burn_point = burnPoint;
            pointHist.cust_point_createby = cust_guid;
            pointHist.cust_point_createdate = DateTime.Now;
            pointHist.cust_point_earn_expiredate = new DateTime(2000, 1, 1);
            pointHist.cust_point_earn_point = earnPoint;
            pointHist.cust_point_earn_point_remain = earnPoint;
            pointHist.cust_point_guid = Guid.NewGuid();
            pointHist.cust_point_status = ENUM_STATUS.active;
            pointHist.cust_point_sum_guid = sumGuid;
            pointHist.cust_point_updateby = cust_guid;
            pointHist.cust_point_updatedate = DateTime.Now;
            pointHist.cust_point_storegroup = storeGroup; 
            return pointHist;
        }
        public void updateRecProductDetail(SqlConnection dbCon, Guid userGuid, UpdateProductRec req)
        {
            CampaignRepository campRepo = new CampaignRepository();
            UserRepository userRepo = new UserRepository();


          
            t_request reqDB = campRepo.getRequestByGuid(dbCon, req.reqGuid);
            if (reqDB == null)
            {
                throw new Exception("ไม่สามารถทำรายการได้");
            }

            CampModel campDB = campRepo.getActiveCamp(dbCon, reqDB.req_createdate);
            if (campDB == null)
            {
                throw new Exception("ไม่พบ Campaign ที่ Active");
            }


            m_store storeDB = campRepo.getStoreByGuid(dbCon, reqDB.req_store_guid);
            if (reqDB.req_status == ENUM_STATUS.approve || reqDB.req_status == ENUM_STATUS.reject)
            {
                throw new Exception("ไม่สามารถแก้ไขรายการที่ได้รับการ approve หรือ reject แล้วได้");
            }

            reqDB.req_updatedate = DateTime.Now;
            reqDB.req_receipt_no = (req.recNo == null ? "" : req.recNo.Trim().ToUpper());
            reqDB.req_status = req.status;
            reqDB.req_remark = (req.remark == null ? "" : req.remark.Trim());
            reqDB.req_total_point = 0;
            
            if (!reqDB.req_status.Equals(ENUM_STATUS.reject))
            {
                t_request reqCheckDup = campRepo.checkExistRecNo(dbCon, reqDB.req_guid, reqDB.req_receipt_no, reqDB.req_store_guid);
                if (reqCheckDup != null)
                {
                    throw new Exception("เลขที่ใบเสร็จนี้ซ้ำ");
                }
            }


            decimal totalPoint = 0;
            decimal totalDisc = 0;
            List<t_request_product> lstReqProd = new List<t_request_product>();
            for (int i = 0; req.lstData != null && i < req.lstData.Count(); i++)
            {
                t_request_product item = new t_request_product();
          
                item.req_prod_createdate = DateTime.Now;
                item.req_prod_discount = req.lstData[i].discount;
                item.req_prod_guid = Guid.NewGuid();
                item.req_prod_price_per_unit = req.lstData[i].pricePerUnit;
                item.req_prod_prod_guid = req.lstData[i].reqProdProdGuid;
                item.req_prod_qty = req.lstData[i].qty;
                item.req_prod_remark = (req.lstData[i].remark == null ? "" : req.lstData[i].remark.Trim());
                item.req_prod_req_guid = req.reqGuid;
                item.req_prod_uom = req.lstData[i].uom.Trim();
                item.req_prod_updatedate = DateTime.Now;
                item.req_prod_total_point = 0; 
                item.req_prod_prod_point_config = 0;
                item.req_prod_prod_qty_config = 0; 

                lstReqProd.Add(item);

                totalDisc += item.req_prod_discount;
                totalPoint += ( Convert.ToDecimal(item.req_prod_qty) * item.req_prod_price_per_unit);
            }

            reqDB.req_total_point = Convert.ToInt32(totalPoint) - Convert.ToInt32(totalDisc);

            t_customer_summary custSumDB = campRepo.getCustSumByCustGuid(dbCon, reqDB.req_cust_guid);
           



            using (var transaction = dbCon.BeginTransaction())
            {
                try
                {
                    if (reqDB.req_status.Equals(ENUM_STATUS.approve) && reqDB.req_total_point > 0)
                    {
                        if (custSumDB == null)
                        {
                            custSumDB = new t_customer_summary();
                            custSumDB.cust_sum_cust_guid = reqDB.req_cust_guid;
                            custSumDB.cust_sum_guid = Guid.NewGuid();
                            custSumDB.cust_sum_remain_point = reqDB.req_total_point;
                            custSumDB.cust_sum_total_point = reqDB.req_total_point;
                            custSumDB.cust_sum_updatedate = DateTime.Now;
                            campRepo.insertCustSum(dbCon, transaction, custSumDB);
                        }
                        else
                        {
                            custSumDB.cust_sum_remain_point = custSumDB.cust_sum_remain_point + reqDB.req_total_point;
                            custSumDB.cust_sum_total_point = custSumDB.cust_sum_total_point + reqDB.req_total_point;
                            custSumDB.cust_sum_updatedate = DateTime.Now;
                            campRepo.updateCustSum(dbCon, transaction, custSumDB);

                        }

                        t_customer_point_history pointHist = generateCustPointHist(custSumDB.cust_sum_guid, reqDB.req_cust_guid, ENUM_ACTLOG.earn_upload, ENUM_ACTIONTYPE.earn, reqDB.req_total_point, 0, storeDB.store_group);
                        campRepo.insertCustPointHist(dbCon, transaction, pointHist);
                    }
                    

                    campRepo.updateRequest(dbCon, transaction, reqDB);
                    campRepo.insertRequestProduct(dbCon, transaction, reqDB.req_guid, lstReqProd);
                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }


            // send line noti to customer
            if (reqDB.req_status.Equals(ENUM_STATUS.approve) || reqDB.req_status.Equals(ENUM_STATUS.reject))
            {
                CustomerRepository custRepo = new CustomerRepository();
                m_customer custDB = custRepo.getCustomerWithGuid(dbCon, reqDB.req_cust_guid);

                LineAndMailService lineServ = new LineAndMailService();
                if (reqDB.req_status.Equals(ENUM_STATUS.approve))
                {

                    lineServ.sendLine_Approve(custDB.cust_line_id, reqDB.req_total_point, storeDB.store_name, reqDB.req_receipt_no, reqDB.req_createdate.ToString("dd MMM yyyy HH:mm", this._cultureTHInfo));
                }
                else if (reqDB.req_status.Equals(ENUM_STATUS.reject))
                {
                    lineServ.sendLine_Reject(custDB.cust_line_id, reqDB.req_guid, storeDB.store_name, reqDB.req_createdate.ToString("dd MMM yyyy HH:mm", this._cultureTHInfo),  reqDB.req_remark.Trim());
                }
            }
        }

        public Login_ResponseResult adminLogin(SqlConnection dbCon, IUserService userServ, string username, string password)
        {
            try
            {
                if (username == null || username.Trim().Equals("") || password == null || password.Trim().Equals(""))
                {
                    throw new Exception("Username / Password is missing");
                }
                password = password.Trim();
                String passEn = ConvertUtil.GetMyEncryptPassword(password.Trim());

                CampaignRepository campRepo = new CampaignRepository();
                UserRepository userRepo = new UserRepository();
                m_user dtoUser = userRepo.getUserByNameAndPass(dbCon, username.Trim(), passEn);

                if (dtoUser == null)
                {
                    throw new Exception("ใส่ Username / Password ไม่ถูกต้อง");
                }

                Login_ResponseResult resp = new Login_ResponseResult();
                resp.Name = dtoUser.user_name;
                resp.UserName = dtoUser.user_username;
                resp.Token = userServ.generateAdminJwtToken(dtoUser.user_guid);

                return resp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }

        internal void confirmRedeemLowTier(SqlConnection dbCon, RedeemConfirmData request, Guid cust_guid)
        {
            CustomerRepository repo = new CustomerRepository();
            CampaignRepository campRepo = new CampaignRepository();
            
            m_campaign_period campDB = campRepo.getActiveCampDBModel(dbCon); 
                if (campDB == null)
                {
                    throw new Exception("ไม่มีแคมเปญที่เปิดให้แลกรางวัลในขณะนี้");
                }

            m_customer custDB = repo.getCustomerWithGuid(dbCon, cust_guid);
            if (custDB == null)
            {
                throw new Exception("ข้อมูลไม่ถูกต้อง cust_guid ไม่มีในระบบ");
            }

            m_reward redeemData = campRepo.getRewardByGuid(dbCon, request.redeemGuid);
            if (redeemData == null || redeemData.reward_remain_stock <= 0)
            {
                throw new Exception("ขออภัย ของรางวัลคงเหลือไม่เพียงพอ");
            }

            if (request.redmHistQty <= 0)
            {
                throw new Exception("ขออภัย กรุณาระบุจำนวนของรางวัลที่ต้องการแลก");
            }
            if (!redeemData.reward_type.Equals(ENUM_REWARDTYPE.lowTier))
            {
                throw new Exception("ขออภัย ของรางวัลที่ท่านเลือกผิดประเภท");
            }
            if (!(campDB.camp_redeem_startdate <= DateTime.Now && campDB.camp_redeem_enddate >= DateTime.Now))
            {
                throw new Exception("ขออภัย ของรางวัลนี้ปิดรับแลกแล้ว");
            }

            if (request.redmHistQty > redeemData.reward_remain_stock)
            {
                throw new Exception("ขออภัย ของรางวัลคงเหลือไม่เพียงพอ");
            }

            t_customer_summary custSumDB = campRepo.getCustSumByCustGuid(dbCon, cust_guid);
            if (custSumDB.cust_sum_remain_point < (request.redmHistQty * redeemData.reward_burn_point))
            {
                throw new Exception("ขออภัย แต้มคงเหลือของท่านไม่เพียงพอ");
            }

            int totalCountRedeemByLDGuid = campRepo.countTotalRedeemGuidByCustSum(dbCon, custSumDB.cust_sum_guid, redeemData.reward_guid);
            if (totalCountRedeemByLDGuid >= redeemData.reward_limit_per_cust)
            {
                throw new Exception("ขออภัย ท่านแลกของรางวัลครบกำหนดตามโควต้าแล้ว");
            }


            custSumDB.cust_sum_remain_point = custSumDB.cust_sum_remain_point - (request.redmHistQty * redeemData.reward_burn_point);
            custSumDB.cust_sum_updatedate = DateTime.Now;

            // check row version 
            redeemData.reward_remain_stock = redeemData.reward_remain_stock - request.redmHistQty;


            List<t_history_redeem> lstHistRedeem = new List<t_history_redeem>();
            for (int i = 1; i <= request.redmHistQty; i++)
            {
                t_history_redeem histRedm = new t_history_redeem();
                histRedm.redm_hist_createdate = DateTime.Now;
                histRedm.redm_hist_guid = Guid.NewGuid();
                histRedm.redm_hist_point_per_unit = redeemData.reward_burn_point;
                histRedm.redm_hist_qty = 1;
                histRedm.redm_hist_redm_guid = redeemData.reward_guid;
                histRedm.redm_hist_status = ENUM_STATUS.inactive;
                histRedm.redm_hist_sum_guid = custSumDB.cust_sum_guid;
                histRedm.redm_hist_tracking = "";
                histRedm.redm_hist_updatedate = DateTime.Now;

                lstHistRedeem.Add(histRedm);
            }

            t_customer_point_history pointHist = generateCustPointHist(custSumDB.cust_sum_guid, cust_guid, ENUM_ACTLOG.earn_redeem + redeemData.reward_name, ENUM_ACTIONTYPE.burn, 0, (request.redmHistQty * redeemData.reward_burn_point), "");

            using (var transaction = dbCon.BeginTransaction())
            {
                try
                {
                    campRepo.updateCustSum(dbCon, transaction, custSumDB);
                    campRepo.insertCustPointHist(dbCon, transaction, pointHist);
                    campRepo.updateRedeemRemain(dbCon, transaction, redeemData);
                    for (int i = 0; i < lstHistRedeem.Count(); i++)
                    {
                        campRepo.insertHistRedeem(dbCon, transaction, lstHistRedeem[i]);
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }

            LineAndMailService lineSev = new LineAndMailService();
            lineSev.sendLine_RedeemVCSuccess(custDB.cust_line_id, pointHist.cust_point_burn_point, request.redmHistQty, redeemData.reward_name);

        }

        internal void confirmRedeemHeightTier(SqlConnection dbCon, RedeemConfirmData request, Guid cust_guid)
        {
            CustomerRepository repo = new CustomerRepository();
            CampaignRepository campRepo = new CampaignRepository();

            m_campaign_period campDB = campRepo.getActiveCampDBModel(dbCon);
                if (campDB == null)
                {
                    throw new Exception("ไม่มีแคมเปญที่เปิดให้แลกรางวัลในขณะนี้");
                }

            m_customer custDB = repo.getCustomerWithGuid(dbCon, cust_guid);
            if (custDB == null)
            {
                throw new Exception("ข้อมูลไม่ถูกต้อง cust_guid ไม่มีในระบบ");
            }

            m_reward redeemData = campRepo.getRewardByGuid(dbCon, request.redeemGuid);
            if (redeemData == null || redeemData.reward_remain_stock <= 0)
            {
                throw new Exception("ขออภัย ของรางวัลคงเหลือไม่เพียงพอ");
            }

            if (request.redmHistQty <= 0)
            {
                throw new Exception("ขออภัย กรุณาระบุจำนวนของรางวัลที่ต้องการแลก");
            }
            if (!redeemData.reward_type.Equals(ENUM_REWARDTYPE.hightTier))
            {
                throw new Exception("ขออภัย ของรางวัลที่ท่านเลือกผิดประเภท");
            }

            if (request.redmHistQty > redeemData.reward_remain_stock)
            {
                throw new Exception("ขออภัย ของรางวัลคงเหลือไม่เพียงพอ");
            }

            t_customer_summary custSumDB = campRepo.getCustSumByCustGuid(dbCon, cust_guid);
            int recPointByStoreGroup = campRepo.getReceivePointByStoreGroup(dbCon, cust_guid, redeemData.reward_store_group); 
            if (recPointByStoreGroup < redeemData.reward_min_point_cond)
            {
                throw new Exception("ขออภัย ท่านต้องได้รับคะแนนขั้นต่ำตามเงื่อนไขจึงจะสามารถแลกของรางวัลได้");
            }

            int totalCountRedeemByLDGuid = campRepo.countTotalRedeemGuidByCustSum(dbCon, custSumDB.cust_sum_guid, redeemData.reward_guid);
            if (totalCountRedeemByLDGuid >= redeemData.reward_limit_per_cust)
            {
                throw new Exception("ขออภัย ท่านแลกของรางวัลครบกำหนดตามโควต้าแล้ว");
            }

            // check row version 
            redeemData.reward_remain_stock = redeemData.reward_remain_stock - request.redmHistQty;

            List<t_history_redeem> lstHistRedeem = new List<t_history_redeem>();
            for (int i = 1; i <= request.redmHistQty; i++)
            {
                t_history_redeem histRedm = new t_history_redeem();
                histRedm.redm_hist_createdate = DateTime.Now;
                histRedm.redm_hist_guid = Guid.NewGuid();
                histRedm.redm_hist_point_per_unit = redeemData.reward_burn_point;
                histRedm.redm_hist_qty = 1;
                histRedm.redm_hist_redm_guid = redeemData.reward_guid;
                histRedm.redm_hist_status = ENUM_STATUS.inactive;
                histRedm.redm_hist_sum_guid = custSumDB.cust_sum_guid;
                histRedm.redm_hist_tracking = "";
                histRedm.redm_hist_updatedate = DateTime.Now;

                lstHistRedeem.Add(histRedm);
            }

            t_customer_point_history pointHist = generateCustPointHist(custSumDB.cust_sum_guid, cust_guid, ENUM_ACTLOG.earn_redeem + redeemData.reward_name, ENUM_ACTIONTYPE.burn, 0, (request.redmHistQty * redeemData.reward_burn_point), "");

            using (var transaction = dbCon.BeginTransaction())
            {
                try
                {
                    campRepo.insertCustPointHist(dbCon, transaction, pointHist);
                    campRepo.updateRedeemRemain(dbCon, transaction, redeemData);
                    for (int i = 0; i < lstHistRedeem.Count(); i++)
                    {
                        campRepo.insertHistRedeem(dbCon, transaction, lstHistRedeem[i]);
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }

            LineAndMailService lineSev = new LineAndMailService();
            lineSev.sendLine_RedeemVCSuccess(custDB.cust_line_id, pointHist.cust_point_burn_point, request.redmHistQty, redeemData.reward_name);

        }

        internal List<RewardData> getHistRedeem(SqlConnection dbCon, Guid cust_guid)
        {
            CampaignRepository campRepo = new CampaignRepository();
            List<RewardData> resp = campRepo.getHistRedeem(dbCon, cust_guid);

            for (int i = 0; resp != null && i < resp.Count(); i++)
            {
                resp[i].deliveryTracking = ENUM_SERVERURL.deliveryOwner; 
                resp[i].redmHistCreateDate_str = resp[i].redmHistCreateDate.ToString("dd MMM yyyy HH:mm", this._cultureTHInfo);
               
            }
            return resp;
        }
        internal LeaderBoardModel getLeaderBoard(SqlConnection dbCon, Guid cust_guid)
        {
            CampaignRepository campRepo = new CampaignRepository();

            DateTime dtNow = DateTime.Now;
            //not implement topspender by campaign period
            //but implement topsender by all campaign period
            //m_campaign_period campDB = campRepo.getActiveCampDBModel(dbCon);
            //if (campDB == null)
            //{
            //    throw new Exception("ไม่มีแคมเปญเปิดอยู่ในขณะนี้");
            //}
            //List<LeaderBoard> lstData = campRepo.getLeaderBoardData(dbCon, campDB.camp_topspend_startdate, campDB.camp_topspend_enddate);

            List<LeaderBoard> lstData = campRepo.getLeaderBoardData(dbCon);
            LeaderBoardModel resp = new LeaderBoardModel();
            resp.isOpenCamp = false;
            resp.isCloseCamp = true;
            resp.lstRanking_01 = new List<LeaderBoard>();
            resp.lstRanking_02 = new List<LeaderBoard>();
            resp.lstRanking_03 = new List<LeaderBoard>();
            resp.lstRanking_04 = new List<LeaderBoard>();

            resp.isOpenCamp = true;
            resp.isCloseCamp = false;
            resp.isShowCounter = true;
            resp.topSpenderEndDate = "2023-12-31 23:59:59 GMT+7";
            resp.topSpenderPeriod = "2023-07-01 - 2023-12-31";
            resp.ownRank = new LeaderBoard();

            //not implement topspender by campaign period
            //but implement topsender by all campaign period
            //if (campDB != null)
            //{
            //    if (campDB.camp_topspend_startdate <= dtNow)
            //    {
            //        resp.isOpenCamp = true;
            //    }
            //    if (campDB.camp_topspend_startdate <= dtNow && campDB.camp_topspend_enddate.AddDays(45) >= dtNow)
            //    {
            //        resp.isCloseCamp = false;
            //    }
            //    if (campDB.camp_topspend_startdate <= dtNow && campDB.camp_topspend_enddate >= dtNow)
            //    {
            //        resp.isShowCounter = true;
            //    }
            //    resp.topSpenderEndDate = campDB.camp_topspend_enddate.ToString("yyyy-MM-dd") ;
            //    resp.topSpenderPeriod = campDB.camp_topspend_startdate.ToString("dd MMM yyyy", this._cultureTHInfo) + " - " + campDB.camp_topspend_enddate.ToString("dd MMM yyyy", this._cultureTHInfo);
            //}

            if (lstData != null)
            {
                resp.lstRanking_01 = lstData.Where(a => a.ranking <= 3).ToList();
                resp.lstRanking_02 = lstData.Where(a => a.ranking >= 4 && a.ranking <= 8).ToList();
                resp.lstRanking_03 = lstData.Where(a => a.ranking >= 9 && a.ranking <= 50).ToList();

                //cut off ranking only top 50
                //resp.lstRanking_04 = lstData.Where(a => a.ranking >= 51).ToList();
            }
            resp.lstRanking_01 = (resp.lstRanking_01 == null ? new List<LeaderBoard>() : resp.lstRanking_01);
            resp.lstRanking_02 = (resp.lstRanking_02 == null ? new List<LeaderBoard>() : resp.lstRanking_02);
            resp.lstRanking_03 = (resp.lstRanking_03 == null ? new List<LeaderBoard>() : resp.lstRanking_03);

            //cut off ranking only top 50
            //resp.lstRanking_04 = (resp.lstRanking_04 == null ? new List<LeaderBoard>() : resp.lstRanking_04);

            //resp.ownRank = campRepo.getLeaderBoardDataByCustGuid(dbCon, cust_guid, campDB.camp_topspend_startdate, campDB.camp_topspend_enddate);

            resp.ownRank = campRepo.getLeaderBoardDataByCustGuid(dbCon, cust_guid);
            resp.ownRank = (resp.ownRank == null ? new LeaderBoard() : resp.ownRank); 
            return resp;
        }

        internal List<RewardData> getAdminHistRedeem(SqlConnection dbCon, SearchRecCriteria req)
        {
            CampaignRepository campRepo = new CampaignRepository();

            if (req.dateFrom == null || req.dateFrom.Trim().Equals(""))
            {
                req.dateFrom = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd");
            }

            if (req.dateTo == null || req.dateTo.Trim().Equals(""))
            {
                req.dateTo = DateTime.Now.ToString("yyyy-MM-dd");
            }

            List<RewardData> resp = campRepo.getAdminHistRedeem(dbCon, req.dateFrom, req.dateTo);

            for (int i = 0; resp != null && i < resp.Count(); i++)
            {
                resp[i].custAddress = resp[i].custAddr01 + " " + resp[i].subDistName + " " + resp[i].distName + " " + resp[i].provName + " รหัสไปรษณีย์ " + resp[i].postCode; 
                resp[i].redmHistCreateDate_str = resp[i].redmHistCreateDate.ToString("dd MMM yyyy HH:mm", this._cultureTHInfo);

            }
            return resp;
        }
        public RewardDataResp readUploadTrackingFromFile(SqlConnection dbCon, IExcelDataReader reader)
        {
            RewardDataResp resp = new RewardDataResp();
            DataSet dsexcelRecords = reader.AsDataSet();
            reader.Close();

            CultureInfo provider = CultureInfo.InvariantCulture;
            if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
            {
                DataTable excelData = dsexcelRecords.Tables["Redeem"];
                Dictionary<Guid, RewardData> dicCheckDupGuid = new Dictionary<Guid, RewardData>();


                resp.isPass = true;
                resp.lstReward = new List<RewardData>();

                CampaignRepository repo = new CampaignRepository();
               
                for (int i = 1; excelData != null && excelData.Rows != null && i < excelData.Rows.Count; i++)
                {
                    RewardData data = new RewardData();

                    data.redmHistGuid = new Guid(Convert.ToString(excelData.Rows[i][0]).Trim());
                    data.redmHistCreateDate_str = Convert.ToString(excelData.Rows[i][1]).Trim();
                    data.rewardName = Convert.ToString(excelData.Rows[i][2]).Trim();
                    data.custTel = Convert.ToString(excelData.Rows[i][3]).Trim();
                    data.custName = Convert.ToString(excelData.Rows[i][4]).Trim();
                    data.custAddress = Convert.ToString(excelData.Rows[i][5]).Trim();
                    data.redmHistTracking = Convert.ToString(excelData.Rows[i][6]).Trim();

                    data.isPass = true;
                    resp.lstReward.Add(data);
                }
            }
            else
            {
                throw new Exception("Selected file is empty.");
            }

            return resp;
        }
        public async Task<XLWorkbook> generateHistRedeemToExcel(List<RewardData> lstData)
        {
            var t = Task.Run(() =>
            {
                var MyWorkBook = new XLWorkbook();
                var MyWorkSheet = MyWorkBook.Worksheets.Add("Redeem");

                int rowDataNo = 2;

                MyWorkSheet.Cell(1, 1).Value = "Hist Guid";
                MyWorkSheet.Cell(1, 2).Value = "วันที่ทำรายการ";
                MyWorkSheet.Cell(1, 3).Value = "ของรางวัล";
                MyWorkSheet.Cell(1, 4).Value = "เบอร์โทรศัพท์";
                MyWorkSheet.Cell(1, 5).Value = "ชื่อลูกค้า";
                MyWorkSheet.Cell(1, 6).Value = "ที่อยู่";
                MyWorkSheet.Cell(1, 7).Value = "Tracking";


                for (int i = 0; lstData != null && i < lstData.Count(); i++)
                {
                    MyWorkSheet.Cell(rowDataNo, 1).SetValue(lstData[i].redmHistGuid.ToString());
                    MyWorkSheet.Cell(rowDataNo, 2).SetValue(lstData[i].redmHistCreateDate_str);
                    MyWorkSheet.Cell(rowDataNo, 3).SetValue(lstData[i].rewardName);
                    MyWorkSheet.Cell(rowDataNo, 4).SetValue(lstData[i].custTel);
                    MyWorkSheet.Cell(rowDataNo, 5).SetValue(lstData[i].custFirstName + " " + lstData[i].custLastName);
                    MyWorkSheet.Cell(rowDataNo, 6).SetValue(lstData[i].custAddress);
                    MyWorkSheet.Cell(rowDataNo, 7).SetValue(lstData[i].redmHistTracking);

                    rowDataNo++;
                }


                var rngTable1 = MyWorkSheet.Range(1, 1, 1, 7);
                rngTable1.Style.Font.Bold = true;
                rngTable1.Style.Font.FontSize = 11;
                rngTable1.Style.Fill.BackgroundColor = XLColor.Amber;

                MyWorkSheet.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                MyWorkSheet.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                return MyWorkBook;
            });
            return await t;
        }

        public void saveTrackingFromFile(SqlConnection dbCon, List<RewardData> lstData)
        {
            // get authorize user
         

            var error = (from a in lstData
                         where !a.isPass
                         select a).FirstOrDefault();
            if (error != null)
            {
                throw new Exception("ไม่สามารถทำรายการได้ เนื่องจากพบ error ในข้อมูล กรุณาตรวจสอบก่อน");
            }

            List<RewardData> lstSendLine = new List<RewardData>(); 
            CampaignRepository repo = new CampaignRepository();
            using (var transaction = dbCon.BeginTransaction())
            {
                for (int i = 0; lstData != null && i < lstData.Count(); i++)
                {
                    t_history_redeem dataDB = repo.getHistRedeemByGuid(dbCon, transaction, lstData[i].redmHistGuid);
                    if (dataDB == null)
                    {
                        throw new Exception("ข้อมูลไม่ถูกต้อง ไม่สามารถอัพเดต Tracking No ได้");
                    }
                    else
                    {
                        if (lstData[i].redmHistTracking != null && !lstData[i].redmHistTracking.Trim().Equals("") && !lstData[i].redmHistTracking.Trim().Equals(dataDB.redm_hist_tracking))
                        {
                            repo.updateTrackingNo(dbCon, transaction, lstData[i]);
                            lstData[i].redmHistSumGuid = dataDB.redm_hist_sum_guid;
                            lstSendLine.Add(lstData[i]);
                        }
                       
                    }
                }

                transaction.Commit();
                LineAndMailService lineServ = new LineAndMailService();
                for (int i = 0; lstSendLine != null && i < lstSendLine.Count(); i++)
                {
                    m_customer custDB = repo.getCustDataByCustSumGuid(dbCon, lstSendLine[i].redmHistSumGuid); 
                    lineServ.sendLine_UpdateTrackingSuccess(custDB.cust_line_id, lstSendLine[i].redmHistTracking, lstSendLine[i].rewardName); 
                }
            }
        }
    }
}