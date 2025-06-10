using tnki_line_sale_api.Entities;
using System.Data.SqlClient;
using Dapper;
using tnki_line_sale_api.Models;
using tnki_line_sale_api.Constant;

namespace tnki_line_sale_api.Repositories
{
    public class CampaignRepository
    {
        private string _fromDate = "2023-07-01 00:00:00.000";
        private string _toDate = "2023-12-31 00:00:00.000";

        public int getReceivePointByStoreGroup(SqlConnection dbCon, Guid cust_guid, string storeGroup)
        {
            const string sql = @"select sum(req.req_total_point) 
                                    from t_request as req 
                                    inner join m_store as store on req.req_store_guid = store.store_guid
                                    where store.store_group = @storeGroup 
                                        and req.req_status = 'APPV' 
                                        and req.req_cust_guid = @reqcust_guid 
                                        and store.store_status = 'ACT'";

            return dbCon.ExecuteScalar<int>(sql, new
            {
                storeGroup = storeGroup, 
                reqcust_guid = cust_guid
            });
        }
        public t_customer_summary getCustSumByCustGuid(SqlConnection dbCon, Guid cust_guid)
        {
            const string sql = @"select * from t_customer_summary
                                          where cust_sum_cust_guid = @cust_guid";

            return dbCon.QuerySingleOrDefault<t_customer_summary>(sql, new
            {
                cust_guid = cust_guid
            });
        }
        public m_customer getCustDataByCustSumGuid(SqlConnection dbCon, Guid cust_sum_guid)
        {
            const string sql = @"select * from m_customer as cust 
                                    inner join t_customer_summary as custSum on cust.cust_guid = custSum.cust_sum_cust_guid
                                    where custSum.cust_sum_guid = @cust_sum_guid";

            return dbCon.QuerySingleOrDefault<m_customer>(sql, new
            {
                cust_sum_guid = cust_sum_guid
            });
        }
        public List<RewardData> getAllActiveRewardByType(SqlConnection dbCon, string rewardType)
        {
            //and rewardRemainStock > 0
            const string sql = @"select
	                               mr.reward_guid as rewardGuid
                                  ,mr.reward_seq as rewardSeq
                                  ,mr.reward_name as rewardName
                                  ,mr.reward_subname as rewardSubName
                                  ,mr.reward_image as rewardImage
                                  ,mr.reward_image_ina as rewardImageIna
                                  ,mr.reward_type as rewardType
                                  ,mr.reward_burn_point as rewardBurnPoint
                                  ,mr.reward_limit_per_cust as rewardLimitPerCust
                                  ,mr.reward_total_stock as rewardTotalStock
                                  ,mr.reward_remain_stock as rewardRemainStock
                                  ,mr.reward_store_group as rewardStoreGroup
                                  ,mr.reward_min_point_cond as rewardMinPointCond
                                  ,mr.reward_status as rewardStatus
                                  ,mr.reward_remark as rewardRemark
                                  ,mr.reward_camp_code
                                  ,mr.row_version
                            from m_reward mr
							left join m_campaign_period mcp on mr.reward_camp_code = mcp.camp_code
                                where mr.reward_type = @rewardType 
                                      and mr.reward_status = 'ACT'
									  and mcp.camp_startdate <= getdate() 
							            and mcp.camp_enddate >= getdate() 
                                order by reward_seq";

            return dbCon.Query<RewardData>(sql, new
            {
                rewardType = rewardType,
                status = ENUM_STATUS.active
            }).ToList();
        }
        public t_event_state getFirstEventStateInPeriod(SqlConnection dbCon, Guid cust_guid, DateTime dt)
        {
            const string sql = @"select top 1 * 
                                    from t_event_state 
                                    where ev_state_cust_guid = @cust_guid
                                        and ev_state_updatedate >= @dt
                                        order by ev_state_updatedate desc";

            return dbCon.QueryFirstOrDefault<t_event_state>(sql, new
            {
                cust_guid = cust_guid,
                dt = dt
            });
        }
        public void insertEventState(SqlConnection dbCon, List<t_event_state> lstData)
        {
            using (var transaction = dbCon.BeginTransaction())
            {
                try
                {
                    for (int i = 0; lstData != null && i < lstData.Count(); i++)
                    {
                        dbCon.Execute(@"INSERT INTO t_event_state
                                           (ev_state_guid
                                           ,ev_state_session_guid
                                           ,ev_state_eventtype
                                           ,ev_state_cust_guid
                                           ,ev_state_group_guid
                                           ,ev_state_name
                                           ,ev_state_paramname
                                           ,ev_state_paramvalue
                                           ,ev_state_updatedate
                                           ,ev_state_ref01
                                           ,ev_state_ref02
                                           ,ev_state_remark)
                                     VALUES
                                           (@ev_state_guid
                                           ,@ev_state_session_guid
                                           ,@ev_state_eventtype
                                           ,@ev_state_cust_guid
                                           ,@ev_state_group_guid
                                           ,@ev_state_name
                                           ,@ev_state_paramname
                                           ,@ev_state_paramvalue
                                           ,@ev_state_updatedate
                                           ,@ev_state_ref01
                                           ,@ev_state_ref02
                                           ,@ev_state_remark)"
                         , new
                         {
                             ev_state_guid = lstData[i].ev_state_guid,
                             ev_state_session_guid = lstData[i].ev_state_session_guid,
                             ev_state_eventtype = lstData[i].ev_state_eventtype,
                             ev_state_cust_guid = lstData[i].ev_state_cust_guid,
                             ev_state_group_guid = lstData[i].ev_state_group_guid,
                             ev_state_name = lstData[i].ev_state_name,
                             ev_state_paramname = lstData[i].ev_state_paramname,
                             ev_state_paramvalue = lstData[i].ev_state_paramvalue,
                             ev_state_updatedate = lstData[i].ev_state_updatedate,
                             ev_state_ref01 = lstData[i].ev_state_ref01,
                             ev_state_ref02 = lstData[i].ev_state_ref02,
                             ev_state_remark = lstData[i].ev_state_remark
                         }, transaction);
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        public m_store getStoreByGuid(SqlConnection dbCon, Guid store_guid)
        {
            const string sql = @"select *
                                from  m_store
                                where store_guid = @store_guid 
								    and store_status = @status";

            return dbCon.QuerySingleOrDefault<m_store>(sql, new
            {
                store_guid = store_guid, 
                status = ENUM_STATUS.active
            });
        }

        public m_reward getRewardByGuid(SqlConnection dbCon, Guid rewardGuid)
        {
            const string sql = @"select *
                                from  m_reward 
                                where  reward_guid = @rewardGuid 
                                    and reward_status = @status";

            return dbCon.QuerySingleOrDefault<m_reward>(sql, new
            {
                rewardGuid = rewardGuid, 
                status = ENUM_STATUS.active
            });
        }
        public CampModel getActiveCamp(SqlConnection dbCon)
        {
            const string sql = @"select camp_guid as campGuid
                                  ,camp_code 
                                  ,camp_name as campName
                                  ,camp_startdate as campStartDate
                                  ,camp_enddate as campEndDate
                                  ,camp_banner1 as campBanner01
                                  ,camp_banner2 as campBanner02
                                  ,camp_banner3 as campBanner03
                                  ,camp_topspend_startdate
                                  ,camp_topspend_enddate
                                  ,camp_redeem_startdate
                                  ,camp_redeem_enddate
                                  ,camp_status as campStatus
                                  ,camp_remark as campRemark
                                from  m_campaign_period
                                where camp_startdate <= getdate() 
		                            and camp_enddate >= getdate() 
                                    and camp_status = @status";

            
            return dbCon.QuerySingleOrDefault<CampModel>(sql, new
            {
                status = ENUM_STATUS.active
            });
        }
        public CampModel getActiveCamp(SqlConnection dbCon, DateTime date)
        {
            const string sql = @"select 
                                   camp_guid as campGuid
                                  ,camp_code 
                                  ,camp_name as campName
                                  ,camp_startdate as campStartDate
                                  ,camp_enddate as campEndDate
                                  ,camp_banner1 as campBanner01
                                  ,camp_banner2 as campBanner02
                                  ,camp_banner3 as campBanner03
                                  ,camp_topspend_startdate
                                  ,camp_topspend_enddate
                                  ,camp_redeem_startdate
                                  ,camp_redeem_enddate
                                  ,camp_status as campStatus
                                  ,camp_remark as campRemark
                                    from  m_campaign_period
                                    where camp_startdate <= @date 
								        and camp_enddate >= @date 
								        and camp_status = @status ";

            return dbCon.QuerySingleOrDefault<CampModel>(sql, new
            {
                status = ENUM_STATUS.active,
                date = date
            });
        }

        public List<HistPointModel> getHistRecByCust(SqlConnection dbCon, Guid cust_guid)
        {
            const string sql = @"select req_createdate as  requestDate, store.store_name as storeName
									, req.req_total_point as  totalPoint
									, req.req_status as  [status]
									, req_remark as remark
									, req_guid as reqGuid
                                 from t_request as req 
                                 inner join m_store as store on req.req_store_guid = store.store_guid 
                                 where req_cust_guid= @cust_guid 
                                    order by req_createdate desc";

            return dbCon.Query<HistPointModel>(sql, new
            {
                cust_guid = cust_guid
            }).ToList();
        }
        public List<ProductModel> getListProduct(SqlConnection dbCon)
        {
            const string sql = @"select [prod_guid] as prodGuid
                                  ,[prod_code] as prodCode
                                  ,[prod_name1] as prodName01
                                  ,[prod_name2] as prodName02
                                  ,[prod_image] as prodImage
                                  ,[prod_packsize] as prodPackSize
                                  ,[prod_seq] as prodSeq
                                  ,[prod_external_link] as prodExternalLink
                                  ,[prod_status] as prodStatus
                                  ,[prod_remark] as prodRemark
                                from m_product as req                                  
                                where prod_status= @status 
                                order by prod_seq";

            return dbCon.Query<ProductModel>(sql, new
            {
                status = ENUM_STATUS.active
            }).ToList();
        }

        public t_request getRequestForEditByGuid(SqlConnection dbCon, Guid req_guid)
        {
            const string sql = @"select * from t_request
                                    where req_guid = @req_guid
                                    and req_status = @status";

            return dbCon.QuerySingleOrDefault<t_request>(sql, new
            {
                req_guid = req_guid,
                status = ENUM_STATUS.reject
            });
        }

        public t_request getRequestByGuid(SqlConnection dbCon, Guid req_guid)
        {
            const string sql = @"select * from t_request
                                    where req_guid = @req_guid 
		                            and req_status = @status";
            return dbCon.QuerySingleOrDefault<t_request>(sql, new
            {
                req_guid = req_guid,
                status = ENUM_STATUS.active
            });
        }

        public List<StoreModel> getListStore(SqlConnection dbCon)
        {
            const string sql = @"select [store_guid] as storeGuid
                                  ,[store_seq] as storeSeq
                                  ,[store_group] as storeGroup
                                  ,[store_name] as storeName
                                  ,[store_image] as storeImage
                                  ,[store_samp_rec] as storeSampRec
                                  ,[store_status] as storeStatus
                                  ,[store_remark] as storeRemark
                                    from m_store
                                    where  store_status = @status 
		                            order by store_seq";

            return dbCon.Query<StoreModel>(sql, new
            {
                status = ENUM_STATUS.active
            }).ToList();
        }
     
        public void insertRequest(SqlConnection dbCon, t_request request, List<t_request_image> lstImage)
        {
            using (var transaction = dbCon.BeginTransaction())
            {
                try
                {
                    dbCon.Execute(@"INSERT INTO t_request
                                       (req_guid
                                       ,req_cust_guid
                                       ,req_store_guid
                                       ,req_total_point
                                       ,req_receipt_no
                                       ,req_status
                                       ,req_remark
                                       ,req_createdate
                                       ,req_updatedate)
                                 VALUES
                                       (@req_guid
                                       ,@req_cust_guid
                                       ,@req_store_guid
                                       ,@req_total_point
                                       ,@req_receipt_no
                                       ,@req_status
                                       ,@req_remark
                                       ,@req_createdate
                                       ,@req_updatedate)"
                   , new
                   {
                       req_guid = request.req_guid,
                       req_cust_guid = request.req_cust_guid,
                       req_store_guid = request.req_store_guid,
                       req_total_point = request.req_total_point,
                       req_receipt_no = request.req_receipt_no,
                       req_status = request.req_status,
                       req_remark = request.req_remark,
                       req_createdate = request.req_createdate,
                       req_updatedate = request.req_updatedate
                   }, transaction);

                    for (int i = 0; lstImage != null && i < lstImage.Count(); i++)
                    {
                        dbCon.Execute(@"INSERT INTO t_request_image
                                       (req_img_guid
                                       ,req_img_req_guid
                                       ,req_img_image_path)
                                    VALUES
                                       (@req_img_guid
                                       ,@req_img_req_guid
                                       ,@req_img_image_path)"
                       , new
                       {
                           req_img_guid = lstImage[i].req_img_guid,
                           req_img_req_guid = lstImage[i].req_img_req_guid,
                           req_img_image_path = lstImage[i].req_img_image_path
                       }, transaction);
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        public void updateRequest(SqlConnection dbCon, t_request request, List<t_request_image> lstImage)
        {
            using (var transaction = dbCon.BeginTransaction())
            {
                try
                {
                    dbCon.Execute(@"UPDATE t_request
                           SET req_store_guid = @req_store_guid
                              ,req_total_point = @req_total_point
                              ,req_receipt_no = @req_receipt_no
                              ,req_status = @req_status
                              ,req_updatedate = @req_updatedate
                         WHERE req_guid = @req_guid"
                       , new
                       {
                           req_store_guid = request.req_store_guid,
                           req_total_point = request.req_total_point,
                           req_receipt_no = request.req_receipt_no,
                           req_status = request.req_status,
                           req_updatedate = request.req_updatedate,
                           req_guid = request.req_guid
                       }, transaction);

                     dbCon.Execute("DELETE FROM t_request_image WHERE req_img_req_guid = @req_img_req_guid"
                     , new
                     {
                         req_img_req_guid = request.req_guid
                     }, transaction);

                    for (int i = 0; lstImage != null && i < lstImage.Count(); i++)
                    {

                        dbCon.Execute(@"INSERT INTO t_request_image
                                       (req_img_guid
                                       ,req_img_req_guid
                                       ,req_img_image_path)
                                    VALUES
                                       (@req_img_guid
                                       ,@req_img_req_guid
                                       ,@req_img_image_path)"
                       , new
                       {
                           req_img_guid = lstImage[i].req_img_guid,
                           req_img_req_guid = lstImage[i].req_img_req_guid,
                           req_img_image_path = lstImage[i].req_img_image_path
                       }, transaction);
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        internal m_link_logstate getLinkLogStateByState(SqlConnection dbCon, string state)
        {
            const string sql = @"select * 
                                    from m_link_logstate 
                                    where link_logstate = @state
                                    and link_status = @linkStatus ";

            return dbCon.QuerySingleOrDefault<m_link_logstate>(sql, new
            {
                state = state,
                linkStatus = ENUM_STATUS.active
            });
        }
        internal void insertLogVisit(SqlConnection dbCon, t_log_visit data)
        {
            dbCon.Execute(@"INSERT INTO t_log_visit
                                   (@log_guid
                                   , @log_cust_guid
                                   , @log_state
                                   , @log_createdate
                                   , @log_remark)
                             VALUES
                                   (@log_guid
                                   , @log_cust_guid
                                   , @log_state
                                   , @log_createdate
                                   , @log_remark)"
                        , new
                        {
                            log_createdate = data.log_createdate,
                            log_cust_guid = data.log_cust_guid,
                            log_guid = data.log_guid,
                            log_remark = data.log_remark,
                            log_state = data.log_state
                        });
        }


        public List<RecDataDetail> getCustomerReqByCriteria(SqlConnection dbCon, SearchRecCriteria req)
        {
            string sql = @"select req_store_guid as storeGuid, store.store_name as storeName, 
		cust.cust_firstname + ' ' + cust.cust_lastname as custName,
		cust.cust_phone_number as custTel, 
		cust.cust_line_display_name as custLineDisplayName, 
		req_receipt_no as recNo, 
		req_createdate as reqDate, 
		req_guid reqGuid, 
		req_status as [status], 
		req_total_point as totalPoint, 
		convert(nvarchar(20), req_createdate, 120) as  reqDate_str, 
		case when req_status = 'ACT' then N'รอการตรวจสอบ'  when req_status = 'APPV' then N'ผ่านการตรวจสอบ' when req_status = 'REJT' then N'ไม่ผ่านการตรวจสอบ' end as statusDisp, 
		case when req_status = 'ACT' then 'btn-success'  when req_status = 'APPV' then 'btn-light' when req_status = 'REJT' then 'btn-primary' end as statusCss
			from t_request as req  
			inner join m_customer as cust on req.req_cust_guid = cust.cust_guid 
			inner join m_store as store on req.req_store_guid = store.store_guid
				where convert(nvarchar(10), req.req_createdate, 120) >= @dateFrom 
				and convert(nvarchar(10), req.req_createdate, 120) <= @dateTo 
				and cust.cust_status = 'ACT'";

            if (req.status == null || !req.status.Equals("ALL"))
            {
                sql = sql + " and req_status = @status " +
                    "order by req_createdate desc";

                return dbCon.Query<RecDataDetail>(sql, new
                {
                    status = req.status,
                    dateFrom = req.dateFrom,
                    dateTo = req.dateTo
                }).ToList();
            }
            else
            {
                sql = sql + " and req_status = 'ACT' " + " order by req_createdate desc";
                return dbCon.Query<RecDataDetail>(sql, new
                {
                    dateFrom = req.dateFrom,
                    dateTo = req.dateTo
                }).ToList();
            }

        }

        public List<RecDataDetail> getCustomerReqForAllByCriteria(SqlConnection dbCon, SearchRecCriteria req)
        {
            string sql = @"select req_store_guid as storeGuid
	                        ,store.store_name as storeName
                            ,cust.cust_firstname + ' ' + cust.cust_lastname as custName
                            ,cust.cust_phone_number as custTel
                            ,cust.cust_line_display_name as custLineDisplayName
                            ,req_receipt_no as recNo 
                            ,req_createdate as reqDate 
                            ,req_guid as reqGuid
                            ,req_status as [status]
                            ,req_total_point as totalPoint
                            ,convert(nvarchar(20), req_createdate, 120) as  reqDate_str
                            ,case when req_status = 'ACT' then N'รอการตรวจสอบ'  when req_status = 'APPV' then N'ผ่านการตรวจสอบ' when req_status = 'REJT' then N'ไม่ผ่านการตรวจสอบ' end as statusDisp
                            ,case when req_status = 'ACT' then 'btn-success'  when req_status = 'APPV' then 'btn-light' when req_status = 'REJT' then 'btn-primary' end as statusCss 
			            from t_request as req  
			            inner join m_customer as cust on req.req_cust_guid = cust.cust_guid 
			            inner join m_store as store on req.req_store_guid = store.store_guid
				            where convert(nvarchar(10), req.req_createdate, 120) >= @dateFrom 
				            and convert(nvarchar(10), req.req_createdate, 120) <= @dateTo 
				            and cust.cust_status = 'ACT'";

            if (req.status == null || !req.status.Equals("ALL"))
            {
                sql = sql + " and req_status = @status " +
                    "order by req_createdate desc";

                return dbCon.Query<RecDataDetail>(sql, new
                {
                    status = req.status,
                    dateFrom = req.dateFrom,
                    dateTo = req.dateTo
                }).ToList();
            }
            else
            {
                sql = sql + " order by req_createdate desc";

                return dbCon.Query<RecDataDetail>(sql, new
                {
                    dateFrom = req.dateFrom,
                    dateTo = req.dateTo
                }).ToList();
            }

        }

        public RecDataDetail getCustomerReqByGuid(SqlConnection dbCon, Guid req_guid)
        {
            string sql = @"select req_store_guid as storeGuid
		                    ,store.store_name as storeName
                            ,cust.cust_firstname + ' ' + cust.cust_lastname as custName
                            ,cust.cust_phone_number as custTel, cust.cust_guid as custGuid
                            ,cust.cust_line_display_name as custLineDisplayName
                            ,req_receipt_no as recNo
                            ,req_createdate as reqDate
		                    ,req_remark as rejectRemark
                            ,req_guid as reqGuid
                            ,req_status as [status]
                            ,req_total_point as totalPoint
                            ,convert(nvarchar(20), req_createdate, 120) as  reqDate_str
                            ,case when req_status = 'ACT' then N'รอการตรวจสอบ'  when req_status = 'APPV' then N'ผ่านการตรวจสอบ' when req_status = 'REJT' then N'ไม่ผ่านการตรวจสอบ' end as statusDisp
                            ,case when req_status = 'ACT' then 'btn-success'  when req_status = 'APPV' then 'btn-light' when req_status = 'REJT' then 'btn-primary' end as statusCss 
	                    from t_request as req  
	                    inner join m_customer as cust on req.req_cust_guid = cust.cust_guid 
	                    inner join m_store as store on req.req_store_guid = store.store_guid
                            where req.req_guid = @req_guid";

            return dbCon.QuerySingleOrDefault<RecDataDetail>(sql, new
            {
                req_guid = req_guid
            });

        }
       
        public List<RecImage> getRecImageByReqGuid(SqlConnection dbCon, Guid req_guid)
        {
            string sql = @"select @serverURL + req_img_image_path as imageURL 
                            from  t_request_image
                            where req_img_req_guid = @req_guid 
                            order by req_img_guid";
            return dbCon.Query<RecImage>(sql, new
            {
                serverURL = ENUM_SERVERURL.baseURL + ENUM_SERVERURL.folderRec + "/",
                req_guid = req_guid
            }).ToList();
        }
       
        public List<RecProdData> getRecProdByReqGuid(SqlConnection dbCon, Guid req_guid)
        {
            string sql = @"select req_prod_prod_guid as recProdGuid
                            ,req_prod_price_per_unit as recProdPricePerUnit
                            ,prod_packsize as prodPackSize
                            ,prod_guid as recProdProdGuid  
                            ,prod_name1 as recProdProdName 
                            ,prod_packsize as prodPackSize
                            ,req_prod_qty as recProdQty
                            ,req_prod_qty * req_prod_price_per_unit as recProdTotalAmt
                            ,rp.req_prod_total_point as reqreqProdProdPoint
                            ,ROW_NUMBER() OVER(ORDER BY req_prod_guid ASC) AS recProdIndex
                            ,rp.req_prod_uom as recProdUOM
                            ,rp.req_prod_remark as recProdRemark
                            ,req_prod_discount as reqProdDiscount
                            ,(req_prod_qty * req_prod_price_per_unit) - req_prod_discount as reqProdNetTotal 
                        from t_request_product as rp  
                        inner join m_product as prod on rp.req_prod_guid = prod.prod_guid 
                        where req_prod_prod_guid = @req_guid 
                        order by recProdProdName";
            return dbCon.Query<RecProdData>(sql, new
            {
                req_guid = req_guid
            }).ToList();
        }
     
        public List<t_request_product> getRequestProdByReqGuid(SqlConnection dbCon, Guid req_guid)
        {
            const string sql = @"select * 
                                    from t_request_product 
                                    where req_prod_req_guid= @req_guid ";

            return dbCon.Query<t_request_product>(sql, new
            {
                req_guid = req_guid
            }).ToList();
        }

        public t_request checkExistRecNo(SqlConnection dbCon, Guid req_guid, string recNo, Guid channelGuid)
        {
            const string sql = @"select * 
                                    from t_request
                                      where req_guid != @req_guid
                                      and req_receipt_no = @req_receipt_no
                                      and req_status != @reject
                                      and req_store_guid = @req_store_guid ";

            return dbCon.QuerySingleOrDefault<t_request>(sql, new
            {
                req_guid = req_guid,
                req_receipt_no = recNo,
                reject = ENUM_STATUS.reject,
                req_store_guid = channelGuid
            });
        }

        internal void updateRequest(SqlConnection dbCon, SqlTransaction transaction, t_request req)
        {
            dbCon.Execute(@"UPDATE t_request
                           SET req_store_guid = @req_store_guid
                              ,req_total_point = @req_total_point
                              ,req_receipt_no = @req_receipt_no
                              ,req_status = @req_status
                              ,req_remark = @req_remark
                              ,req_updatedate = @req_updatedate
                         WHERE req_guid = @req_guid"
             , new
             {
                 req_store_guid = req.req_store_guid,
                 req_total_point = req.req_total_point,
                 req_receipt_no = req.req_receipt_no,
                 req_status = req.req_status,
                 req_remark = req.req_remark,
                 req_updatedate = req.req_updatedate,
                 req_guid = req.req_guid
             }, transaction);
        }

        internal void insertRequestProduct(SqlConnection dbCon, SqlTransaction transaction, Guid req_guid, List<t_request_product> lstReqProd)
        {
            dbCon.Execute("DELETE FROM t_request_product WHERE req_prod_req_guid = @reqProdReqGuid"
                   , new
                   {
                       reqProdReqGuid = req_guid
                   }, transaction);

            for (int i = 0; lstReqProd != null && i < lstReqProd.Count(); i++)
            {
                dbCon.Execute(@"INSERT INTO t_request_product
                                   (req_prod_guid
                                   ,req_prod_req_guid
                                   ,req_prod_prod_guid
                                   ,req_prod_qty
                                   ,req_prod_total_point
                                   ,req_prod_price_per_unit
                                   ,req_prod_uom
                                   ,req_prod_remark
                                   ,req_prod_createdate
                                   ,req_prod_updatedate
                                   ,req_prod_discount)
                             VALUES
                                   (@req_prod_guid
                                   ,@req_prod_req_guid
                                   ,@req_prod_prod_guid
                                   ,@req_prod_qty
                                   ,@req_prod_total_point
                                   ,@req_prod_price_per_unit
                                   ,@req_prod_uom
                                   ,@req_prod_remark
                                   ,@req_prod_createdate
                                   ,@req_prod_updatedate
                                   ,@req_prod_discount)"
                 , new
                 {
                     req_prod_guid = lstReqProd[i].req_prod_guid,
                     req_prod_req_guid = lstReqProd[i].req_prod_req_guid,
                     req_prod_prod_guid = lstReqProd[i].req_prod_prod_guid,
                     //reqProdProdPointConfig = lstReqProd[i].reqProdProdPointConfig,
                     //reqProdProdQtyConfig = lstReqProd[i].reqProdProdQtyConfig,
                     req_prod_qty = lstReqProd[i].req_prod_qty,
                     req_prod_total_point = lstReqProd[i].req_prod_total_point,
                     req_prod_price_per_unit = lstReqProd[i].req_prod_price_per_unit,
                     req_prod_uom = lstReqProd[i].req_prod_uom,
                     req_prod_remark = lstReqProd[i].req_prod_remark,
                     req_prod_createdate = lstReqProd[i].req_prod_createdate,
                     req_prod_updatedate = lstReqProd[i].req_prod_updatedate,
                     req_prod_discount = lstReqProd[i].req_prod_discount
                 }, transaction);
            }
        }

        public m_campaign_period getActiveCampDBModel(SqlConnection dbCon)
        {
            const string sql = @"select *
                                    from  m_campaign_period
                                    where camp_startdate <= getdate() 
		                            and camp_enddate >= getdate() and camp_status = @status";
                       
            return dbCon.QuerySingleOrDefault<m_campaign_period>(sql, new
            {
                status = ENUM_STATUS.active
            });
        }

        public t_history_redeem getRedeemHistDBByCustAndReward(SqlConnection dbCon, Guid cust_guid, Guid rewardGuid)
        {
            const string sql = @"select hist.*
                                    from  t_history_redeem as hist 
                                    inner join t_customer_summary as custSum on hist.redm_hist_sum_guid = custSum.cust_sum_guid
                                    where hist.redm_hist_redm_guid = @redmHistRedmGuid 
                                    and custSum.cust_sum_cust_guid = @cust_sum_cust_guid";

            return dbCon.QuerySingleOrDefault<t_history_redeem>(sql, new
            {
                cust_sum_cust_guid = cust_guid, 
                redmHistRedmGuid = rewardGuid
            });
        }

        public void insertCustSum(SqlConnection dbCon, SqlTransaction transaction, t_customer_summary dtoCustSum)
        {
            try
            {

                dbCon.Execute(@"INSERT INTO t_customer_summary
                        (cust_sum_guid
                        ,cust_sum_cust_guid
                        ,cust_sum_total_point
                        ,cust_sum_remain_point
                        ,cust_sum_updatedate)
                    VALUES
                        (@cust_sum_guid
                        ,@cust_sum_cust_guid
                        ,@cust_sum_total_point
                        ,@cust_sum_remain_point
                        ,@cust_sum_updatedate)"
                , new
                {
                    cust_sum_guid = dtoCustSum.cust_sum_guid,
                    cust_sum_cust_guid = dtoCustSum.cust_sum_cust_guid,
                    cust_sum_total_point = dtoCustSum.cust_sum_total_point,
                    cust_sum_remain_point = dtoCustSum.cust_sum_remain_point,
                    cust_sum_updatedate = dtoCustSum.cust_sum_updatedate
                }, transaction);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void updateCustSum(SqlConnection dbCon, SqlTransaction transaction, t_customer_summary dtoCustSum)
        {


            dbCon.Execute(@"UPDATE t_customer_summary
                       SET cust_sum_remain_point = @cust_sum_remain_point
                          ,cust_sum_total_point = @cust_sum_total_point
                          ,cust_sum_updatedate = @cust_sum_updatedate
                        where cust_sum_guid = @cust_sum_guid"
              , new
              {
                  cust_sum_remain_point = dtoCustSum.cust_sum_remain_point,
                  cust_sum_total_point = dtoCustSum.cust_sum_total_point,
                  cust_sum_updatedate = dtoCustSum.cust_sum_updatedate,
                  cust_sum_guid = dtoCustSum.cust_sum_guid
              }, transaction);

        }
        public void insertCustPointHist(SqlConnection dbCon, SqlTransaction transaction, t_customer_point_history dtoCustPointHist)
        {
            try
            {
                dbCon.Execute(@"INSERT INTO t_customer_point_history
                                   (cust_point_guid
                                   ,cust_point_sum_guid
                                   ,cust_point_act_type
                                   ,cust_point_activity_log
                                   ,cust_point_storegroup
                                   ,cust_point_earn_point
                                   ,cust_point_earn_point_remain
                                   ,cust_point_burn_point
                                   ,cust_point_earn_expiredate
                                   ,cust_point_status
                                   ,cust_point_createdate
                                   ,cust_point_createby
                                   ,cust_point_updatedate
                                   ,cust_point_updateby)
                             VALUES
                                   (@cust_point_guid
                                   ,@cust_point_sum_guid
                                   ,@cust_point_act_type
                                   ,@cust_point_activity_log
                                   ,@cust_point_storegroup
                                   ,@cust_point_earn_point
                                   ,@cust_point_earn_point_remain
                                   ,@cust_point_burn_point
                                   ,@cust_point_earn_expiredate
                                   ,@cust_point_status
                                   ,@cust_point_createdate
                                   ,@cust_point_createby
                                   ,@cust_point_updatedate
                                   ,@cust_point_updateby)"
                , new
                {
                    cust_point_guid = dtoCustPointHist.cust_point_guid,
                    cust_point_sum_guid = dtoCustPointHist.cust_point_sum_guid,
                    cust_point_act_type = dtoCustPointHist.cust_point_act_type,
                    cust_point_activity_log = dtoCustPointHist.cust_point_activity_log,
                    cust_point_earn_point = dtoCustPointHist.cust_point_earn_point,
                    cust_point_earn_point_remain = dtoCustPointHist.cust_point_earn_point_remain,
                    custPBurnPoint = dtoCustPointHist.cust_point_burn_point,
                    cust_point_burn_point = dtoCustPointHist.cust_point_earn_expiredate,
                    cust_point_status = dtoCustPointHist.cust_point_status,
                    cust_point_createdate = dtoCustPointHist.cust_point_createdate,
                    cust_point_createby = dtoCustPointHist.cust_point_createby,
                    cust_point_updatedate = dtoCustPointHist.cust_point_updatedate,
                    cust_point_updateby = dtoCustPointHist.cust_point_updateby,
                    cust_point_storegroup = dtoCustPointHist.cust_point_storegroup
                }, transaction);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int countTotalRedeemGuidByCustSum(SqlConnection dbCon, Guid cust_sum_guid, Guid redeemGuid)
        {
            const string sql = @"select sum(redm_hist_qty) 
		                            from t_history_redeem
                                    where redm_hist_sum_guid = @cust_sum_guid 
		                            and redm_hist_redm_guid = @redeemGuid";

            return dbCon.ExecuteScalar<int>(sql, new
            {
                cust_sum_guid = cust_sum_guid,
                redeemGuid = redeemGuid
            });
        }    

        public void updateRedeemRemain(SqlConnection dbCon, SqlTransaction transaction, m_reward dtoRedeem)
        {
            const string sql = @"select * 
		                            from m_reward
                                    where reward_guid = @redeemGuid 
		                            and reward_status = @status";

            m_reward redmCheck = dbCon.QuerySingleOrDefault<m_reward>(sql, new
            {
                redeemGuid = dtoRedeem.reward_guid,
                status = ENUM_STATUS.active
            }, transaction);

            if (redmCheck == null || redmCheck.row_version != dtoRedeem.row_version)
            {
                throw new Exception("ขออภัย กรุณารีเฟรชเว็บไซต์และทำรายการใหม่อีกครั้ง");
            }

            dbCon.Execute(@"UPDATE m_reward
                            SET reward_remain_stock = @redeemRemainStock
                                ,row_version = @rowVersion
                            where reward_guid = @redeemGuid"
              , new
              {
                  redeemRemainStock = dtoRedeem.reward_remain_stock,
                  rowVersion = Guid.NewGuid(),
                  redeemGuid = dtoRedeem.reward_guid
              }, transaction);

        }

        public void insertHistRedeem(SqlConnection dbCon, SqlTransaction transaction, t_history_redeem dtoHistRedeem)
        {
            try
            {
                dbCon.Execute(@"INSERT INTO t_history_redeem
                                       (redm_hist_guid
                                       ,redm_hist_sum_guid
                                       ,redm_hist_redm_guid
                                       ,redm_hist_qty
                                       ,redm_hist_point_per_unit
                                       ,redm_hist_tracking
                                       ,redm_hist_status
                                       ,redm_hist_createdate
                                       ,redm_hist_updatedate)
                                 VALUES
                                       (@redm_hist_guid
                                       ,@redm_hist_sum_guid
                                       ,@redm_hist_redm_guid
                                       ,@redm_hist_qty
                                       ,@redm_hist_point_per_unit
                                       ,@redm_hist_tracking
                                       ,@redm_hist_status
                                       ,@redm_hist_createdate
                                       ,@redm_hist_updatedate)"
                , new
                {
                    redm_hist_guid = dtoHistRedeem.redm_hist_guid,
                    redm_hist_sum_guid = dtoHistRedeem.redm_hist_sum_guid,
                    redm_hist_redm_guid = dtoHistRedeem.redm_hist_redm_guid,
                    redm_hist_qty = dtoHistRedeem.redm_hist_qty,
                    redm_hist_point_per_unit = dtoHistRedeem.redm_hist_point_per_unit,
                    redm_hist_tracking = dtoHistRedeem.redm_hist_tracking,
                    redm_hist_status = dtoHistRedeem.redm_hist_status,
                    redm_hist_createdate = dtoHistRedeem.redm_hist_createdate,
                    redm_hist_updatedate = dtoHistRedeem.redm_hist_updatedate
                }, transaction);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<RewardData> getHistRedeem(SqlConnection dbCon, Guid cust_guid)
        {
            const string sql = @"select redm_hist_guid as redmHistGuid
      ,redm_hist_sum_guid as redmHistSumGuid
      ,redm_hist_redm_guid as redmHistRedmGuid
      ,redm_hist_qty as redmHistQty
      ,redm_hist_point_per_unit as redmHistPointPerUnit
      ,redm_hist_tracking as redmHistTracking
      ,redm_hist_status
      ,redm_hist_createdate as redmHistCreateDate
      ,redm_hist_updatedate
	  ,cust_sum_guid as custGuid
      ,cust_sum_cust_guid as custGuid
      ,cust_sum_total_point
      ,cust_sum_remain_point
      ,cust_sum_updatedate
	  ,reward_guid as rewardGuid
      ,reward_seq as rewardSeq
      ,reward_name as rewardName
      ,reward_subname as rewardSubName
      ,reward_image as rewardImage
      ,reward_image_ina as rewardImageIna
      ,reward_type as rewardType
      ,reward_burn_point as rewardBurnPoint
      ,reward_limit_per_cust as rewardLimitPerCust
      ,reward_total_stock as rewardTotalStock
      ,reward_remain_stock as rewardRemainStock
      ,reward_store_group as rewardStoreGroup
      ,reward_min_point_cond as rewardMinPointCond
      ,reward_status as rewardStatus
      ,reward_remark as rewardRemark
      ,reward_camp_code
      ,row_version
    from t_history_redeem as hist
    inner join t_customer_summary as custSum on hist.redm_hist_sum_guid = custSum.cust_sum_guid
    inner join m_reward as redm on hist.redm_hist_redm_guid = redm.reward_guid
    where custSum.cust_sum_cust_guid = @cust_guid
    order by hist.redm_hist_updatedate desc";

            return dbCon.Query<RewardData>(sql, new
            {
                cust_guid = cust_guid
            }).ToList();
        }

        //public   List<LeaderBoard> getLeaderBoardData(SqlConnection dbCon, DateTime fromDate , DateTime toDate)
        public List<LeaderBoard> getLeaderBoardData(SqlConnection dbCon)
        {
            const string sql = @"select * from 
            (select row_number() OVER(ORDER BY totalPoint desc) AS ranking, * from (
                select   cust.cust_line_image as lineImg, cust.cust_line_display_name as lineName, sum(req.req_total_point) as totalPoint
				from t_request as req
                inner join m_customer as cust on req.req_cust_guid = cust.cust_guid
                where cust.cust_check_topspender = 1 and cust.cust_status = 'ACT' and req.req_status = 'APPV'
                and req.req_createdate >= @fromDate and req.req_createdate <= @toDate    
                group by cust.cust_line_image , cust.cust_line_display_name
            ) as d 
        ) as a where ranking <= 50 order by ranking";

            return dbCon.Query<LeaderBoard>(sql, new
            {
                fromDate = _fromDate,
                toDate = _toDate
            }).ToList();
        }

        //public LeaderBoard getLeaderBoardDataByCustGuid(SqlConnection dbCon, Guid cust_guid, DateTime fromDate, DateTime toDate)
        public LeaderBoard getLeaderBoardDataByCustGuid(SqlConnection dbCon, Guid cust_guid)
        {
            const string sql = @"
                select * from (
                select row_number() OVER(ORDER BY totalPoint desc) AS ranking, * from (
                select cust.cust_guid as custGuid,  cust.cust_line_image as lineImg, cust.cust_line_display_name as lineName, sum(req.req_total_point) as totalPoint from t_request as req
                inner join m_customer as cust on req.req_cust_guid = cust.cust_guid
                where cust.cust_check_topspender = 1 and cust.cust_status = 'ACT' and req.req_status = 'APPV'
and req.req_createdate >= @fromDate and req.req_createdate <= @toDate    
                group by cust.cust_line_image , cust.cust_line_display_name, cust.cust_guid) as a) as d where custGuid = @cust_guid";

            return dbCon.QuerySingleOrDefault<LeaderBoard>(sql, new
            {
                cust_guid = cust_guid,
                fromDate = _fromDate,
                toDate = _toDate
            });
        }

        public List<RewardData> getAdminHistRedeem(SqlConnection dbCon, string startDate, string endDate)
        {
            const string sql = @"select prov.prov_name_th as provName,dist.dist_name_th as distName , 
        subd.subdist_name_th as subDistName,subd.subdist_zipcode as postCode
		,redm_hist_guid as redmHistGuid
		  ,redm_hist_sum_guid as redmHistSumGuid ,redm_hist_redm_guid as redmHistRedmGuid
		  ,redm_hist_qty as redmHistQty ,redm_hist_point_per_unit as redmHistPointPerUnit
		  ,redm_hist_tracking as redmHistTracking ,redm_hist_status
		  ,redm_hist_createdate as redmHistCreateDate ,redm_hist_updatedate

		  ,cust_sum_guid as custGuid ,cust_sum_cust_guid as custGuid
		  ,cust_sum_total_point ,cust_sum_remain_point
		  ,cust_sum_updatedate ,reward_guid as rewardGuid

		  ,reward_seq as rewardSeq ,reward_name as rewardName
		  ,reward_subname as rewardSubName ,reward_image as rewardImage
		  ,reward_image_ina as rewardImageIna ,reward_type as rewardType
		  ,reward_burn_point as rewardBurnPoint ,reward_limit_per_cust as rewardLimitPerCust
		  ,reward_total_stock as rewardTotalStock ,reward_remain_stock as rewardRemainStock
		  ,reward_store_group as rewardStoreGroup ,reward_min_point_cond as rewardMinPointCond
		  ,reward_status as rewardStatus ,reward_remark as rewardRemark
		  ,reward_camp_code ,row_version

		  ,cust_guid as custGuid, cust_firstname as custFirstName, cust_lastname as custLastName
		  ,cust_line_id as custLineId, cust_line_image as custLineImg, cust_line_display_name as custLineDisplayName
		  ,cust_phone_number as custTel, cust_check_pdpa1 as custCheckTerm
		  ,cust_check_pdpa2 as custCheckPrivacy, cust_status as custStatus 
          ,cust_address1 as custAddr01, cust_address2 as custAddr02, cust_prov_id as custProvId
		  ,cust_dist_id as custDistId, cust_subdist_id as custSubDistId

            from t_history_redeem as hist
            inner join t_customer_summary as custSum on hist.redm_hist_sum_guid = custSum.cust_sum_guid
            inner join m_reward as redm on hist.redm_hist_redm_guid = redm.reward_guid
            inner join m_customer as cust on custSum.cust_sum_cust_guid = cust.cust_guid
            left join m_address_province as prov on cust.cust_prov_id = prov.prov_id
            left join m_address_district as dist on cust.cust_dist_id = dist.dist_id
            left join m_address_subdistrict as subd on cust.cust_subdist_id = subd.subdist_id
            where cust.cust_status = 'ACT' 
                and (convert(nvarchar(10), hist.redm_hist_createdate, 120) >= '2023-07-01' 
            or convert(nvarchar(10), hist.redm_hist_createdate, 120)  <= '2024-01-15')
            order by hist.redm_hist_createdate ,redm.reward_name, cust.cust_phone_number";

            return dbCon.Query<RewardData>(sql, new
            {
                startDate = startDate,
                endDate = endDate
            }).ToList();
        }

        public List<m_reward> getAllReward(SqlConnection dbCon)
        {
            const string sql = @"select * from m_reward where reward_status = @rewardStatus";

            return dbCon.Query<m_reward>(sql, new
            {
                rewardStatus = ENUM_STATUS.active
            }).ToList();
        }

        public t_history_redeem getHistRedeemByGuid(SqlConnection dbCon, SqlTransaction transaction, Guid redmHistGuid)
        {
            const string sql = @"select * from t_history_redeem where redm_hist_guid = @redmHistGuid";

            return dbCon.QuerySingleOrDefault<t_history_redeem>(sql, new
            {
                redmHistGuid = redmHistGuid
            }, transaction);
        }
        public void updateTrackingNo(SqlConnection dbCon, SqlTransaction transaction, RewardData data)
        {
            dbCon.Execute(@"UPDATE t_history_redeem
                           SET redm_hist_tracking = @redmHistTracking
                              ,redm_hist_updatedate = @redmHistUpdateDate
                         WHERE redm_hist_guid = @redmHistGuid"
                       , new
                       {
                           redmHistTracking = data.redmHistTracking,
                           redmHistUpdateDate = DateTime.Now,
                           redmHistGuid = data.redmHistGuid
                       }, transaction);
            
        }
    }
}
