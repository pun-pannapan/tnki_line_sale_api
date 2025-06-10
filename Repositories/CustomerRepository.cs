using tnki_line_sale_api.Entities;
using System.Data.SqlClient;
using Dapper;
using tnki_line_sale_api.Models;

namespace tnki_line_sale_api.Repositories
{
    public class CustomerRepository
    {
        public List<AddrProviceModel> getProvince(SqlConnection dbCon)
        {
            const string sql = @"select [prov_id] as provId
                                  ,[prov_code] as provCode
                                  ,[prov_name_th] as provNameTh
                                  ,[prov_name_en] as provNameEn
                            from m_address_province
                            order by prov_code";

            return dbCon.Query<AddrProviceModel>(sql).ToList();
        }

        public List<AddrDistrictModel> getDistByProvId(SqlConnection dbCon, long prov_id)
        {
            const string sql = @"select [dist_id] as distId
                                  ,[dist_code] as distCode
                                  ,[dist_name_th] as name_th
                                  ,[dist_name_en] as name_en
                                  ,[prov_id] as province_id
                            from m_address_district 
                            where prov_id = @prov_id 
                            order by dist_code";

            return dbCon.Query<AddrDistrictModel>(sql, new
            {
                prov_id = prov_id
            }).ToList();
        }

        public List<AddrSubDistrictModel> getSubDistByDistId(SqlConnection dbCon, long subdist_dist_id)
        {
            const string sql = @"select [subdist_id] as subDistId
          ,[subdist_zipcode] as subDistZipCode
          ,[subdist_name_th] as subDistNameTh
          ,[subdist_name_en] as subDistNameEn
          ,[subdist_dist_id] as subDistDistId
    from m_address_subdistrict 
    where subdist_dist_id = @subdist_dist_id 
    order by subdist_name_th";

            return dbCon.Query<AddrSubDistrictModel>(sql, new
            {
                subdist_dist_id = subdist_dist_id
            }).ToList();
        }

        public m_customer getCustomerWithLineId(SqlConnection dbCon, string lineUID)
        {
            const string sql = "select * from m_customer " +
                "where cust_line_id = @lineUID and cust_status = 'ACT'";

            return dbCon.QuerySingleOrDefault<m_customer>(sql, new
            {
                lineUID = lineUID
            });
        }

        public m_customer getCustomerWithTel(SqlConnection dbCon, string cust_phone_number)
        {
            const string sql = "select * from m_customer " +
                "where cust_phone_number = @cust_phone_number ";

            return dbCon.QuerySingleOrDefault<m_customer>(sql, new
            {
                cust_phone_number = cust_phone_number
            });
        }

        public CustModel getCustProfile(SqlConnection dbCon, Guid cust_guid)
        {
            const string sql = @"select cust_guid as custGuid, cust_firstname as custFirstName, cust_lastname as custLastName,
		cust_line_id as custLineId, cust_line_image as custLineImg, cust_line_display_name as custLineDisplayName,
		cust_phone_number as custTel, cust_check_pdpa1 as custCheckTerm,
		cust_check_pdpa2 as custCheckPrivacy, cust_status as custStatus, 
        cust_address1 as custAddr01, cust_address2 as custAddr02, cust_prov_id as custProvId,
		cust_dist_id as custDistId, cust_subdist_id as custSubDistId, prov.prov_name_th as provName,
        dist.dist_name_th as distName, sub_dist.subdist_name_th as subDistName,
		sub_dist.subdist_zipcode as postCode, cust_check_topspender as custCheckTopSpender
                            from m_customer as cust 
                            left join m_address_province as prov on cust.cust_prov_id = prov.prov_id
                            left join m_address_district as dist on cust.cust_dist_id = dist.dist_id
                            left join m_address_subdistrict as sub_dist on cust.cust_subdist_id = sub_dist.subdist_id
                            where cust_guid = @cust_guid";

            return dbCon.QuerySingleOrDefault<CustModel>(sql, new
            {
                cust_guid = cust_guid
            });
        }

        public m_customer getCustomerWithGuid(SqlConnection dbCon, Guid cust_guid)
        {
            const string sql = "select * from m_customer " +
                "where cust_guid = @cust_guid and cust_status = 'ACT'";

            return dbCon.QuerySingleOrDefault<m_customer>(sql, new
            {
                cust_guid = cust_guid
            });
        }

        public void updateCustomerLineData(SqlConnection dbCon, m_customer dtoCust)
        {
            dbCon.Execute(@"UPDATE dbo.m_customer  
                set cust_line_display_name = @cust_line_display_name, 
                    cust_line_image = @cust_line_image
                where cust_guid = @cust_guid "
              , new
              {
                  cust_guid = dtoCust.cust_guid,
                  cust_line_display_name = dtoCust.cust_line_display_name,
                  cust_line_image = dtoCust.cust_line_image
              });

        }
        public void updatePersonalData(SqlConnection dbCon, m_customer dtoCust)
        {

            dbCon.Execute(@"UPDATE m_customer
                       SET cust_firstname = @cust_firstname
                          ,cust_lastname = @cust_lastname
                          , cust_line_id = @cust_line_id
                          ,cust_line_image = @cust_line_image
                          ,cust_line_display_name = @cust_line_display_name
                          ,cust_phone_number = @cust_phone_number
                          ,cust_address1 = @cust_address1
                          ,cust_address2 = @cust_address2
                          ,cust_prov_id = @cust_prov_id
                          ,cust_dist_id = @cust_dist_id
                          ,cust_subdist_id = @cust_subdist_id
                          ,cust_check_pdpa1 = @cust_check_pdpa1
                          ,cust_check_pdpa2 = @cust_check_pdpa2
                          ,cust_status = @cust_status
                          ,cust_remark = @cust_remark
                          ,cust_check_topspender = @cust_check_topspender
                          ,cust_updatedate = @cust_updatedate
                        where cust_guid = @cust_guid"
              , new
              {
                  cust_check_topspender = dtoCust.cust_check_topspender, 
                  cust_line_id = dtoCust.cust_line_id,
                  cust_guid = dtoCust.cust_guid,
                  cust_firstname = dtoCust.cust_firstname,
                  cust_lastname = dtoCust.cust_lastname,
                  cust_line_image = dtoCust.cust_line_image,
                  cust_line_display_name = dtoCust.cust_line_display_name,
                  cust_phone_number = dtoCust.cust_phone_number,
                  cust_address1 = dtoCust.cust_address1,
                  cust_address2 = dtoCust.cust_address2,
                  cust_prov_id = dtoCust.cust_prov_id,
                  cust_dist_id = dtoCust.cust_dist_id,
                  cust_subdist_id = dtoCust.cust_subdist_id,
                  cust_check_pdpa1 = dtoCust.cust_check_pdpa1,
                  cust_check_pdpa2 = dtoCust.cust_check_pdpa2,
                  cust_status = dtoCust.cust_status,
                  cust_remark = dtoCust.cust_remark,
                  cust_updatedate = dtoCust.cust_updatedate
              });

        }
        public void insertPersonalData(SqlConnection dbCon, m_customer dtoCust, t_customer_summary dtoCustSum)
        {
            using (var transaction = dbCon.BeginTransaction())
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

                    dbCon.Execute(@"INSERT INTO m_customer
                           (cust_guid
                           ,cust_firstname
                           ,cust_lastname
                           ,cust_phone_number
                           ,cust_line_id
                           ,cust_line_display_name
                           ,cust_line_image
                           ,cust_address1
                           ,cust_address2
                           ,cust_prov_id
                           ,cust_dist_id
                           ,cust_subdist_id
                           ,cust_check_pdpa1
                           ,cust_check_pdpa2
                           ,cust_status
                           ,cust_remark
                           ,cust_check_topspender
                           ,cust_updatedate)
                     VALUES
                          (@cust_guid
                           ,@cust_firstname
                           ,@cust_lastname
                           ,@cust_phone_number
                           ,@cust_line_id
                           ,@cust_line_display_name
                           ,@cust_line_image
                           ,@cust_address1
                           ,@cust_address2
                           ,@cust_prov_id
                           ,@cust_dist_id
                           ,@cust_subdist_id
                           ,@cust_check_pdpa1
                           ,@cust_check_pdpa2
                           ,@cust_status
                           ,@cust_remark
                           ,@cust_check_topspender
                           ,@cust_updatedate)"
                       , new
                       {
                           cust_check_topspender = dtoCust.cust_check_topspender,
                           cust_guid = dtoCust.cust_guid,
                           cust_firstname = dtoCust.cust_firstname,
                           cust_lastname = dtoCust.cust_lastname,
                           cust_line_id = dtoCust.cust_line_id,
                           cust_line_image = dtoCust.cust_line_image,
                           cust_line_display_name = dtoCust.cust_line_display_name,
                           cust_phone_number = dtoCust.cust_phone_number,
                           cust_address1 = dtoCust.cust_address1,
                           cust_address2 = dtoCust.cust_address2,
                           cust_prov_id = dtoCust.cust_prov_id,
                           cust_dist_id = dtoCust.cust_dist_id,
                           cust_subdist_id = dtoCust.cust_subdist_id,
                           cust_check_pdpa2 = dtoCust.cust_check_pdpa2,
                           cust_check_pdpa1 = dtoCust.cust_check_pdpa1,
                           cust_status = dtoCust.cust_status,
                           cust_remark = dtoCust.cust_remark,
                           cust_updatedate = dtoCust.cust_updatedate
                       }, transaction);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
