namespace tnki_line_sale_api.Models
{
    public class LeaderBoardModel
    {
        public bool isCloseCamp { get; set; }
        public bool isShowCounter { get; set; }
        public bool isOpenCamp { get; set; }
        public string topSpenderPeriod { get; set; }
        public string topSpenderEndDate { get; set; }
        public LeaderBoard ownRank { get; set; }
        public List<LeaderBoard> lstRanking_01 { get; set; }
        public List<LeaderBoard> lstRanking_02 { get; set; }
        public List<LeaderBoard> lstRanking_03 { get; set; }
        public List<LeaderBoard> lstRanking_04 { get; set; }
    }
    public class LeaderBoard { 
        public int ranking { get; set; }
        public string lineName { get; set; }
        public string lineImg { get; set; }
        public int totalPoint { get; set; }
    }

    public class UpdateProductRec
    {
        public Guid reqGuid { get; set; }
        public string status { get; set; }
        public string remark { get; set; }
        public string recNo { get; set; }

        public List<UpdateProductRecDetail> lstData { get; set; }
    }

    public class UpdateProductRecDetail
    {
        public Guid reqProdProdGuid { get; set; }
        public Guid prodGuid { get; set; }
        public int qty { get; set; }
        public decimal pricePerUnit { get; set; }
        public decimal discount { get; set; }
        public string uom { get; set; }
        public string remark { get; set; }

    }
    public class SearchRecCriteria
    {
        public string dateFrom { get; set; }
        public string dateTo { get; set; }
        public string? status { get; set; }
    }
    public class RecDataDetail
    {      
        public DateTime reqDate { get; set; }
        public Guid reqGuid { get; set; }
        public string reqDate_str { get; set; }
        public string recNo { get; set; }
        public string custLineDisplayName { get; set; }
        public Guid custGuid { get; set; }
        public string custName { get; set; }
        public string custTel { get; set; }
        public string status { get; set; }
        public string statusDisp { get; set; }
        public string statusCss { get; set; }
        public string rejectRemark { get; set; }
        public string rejectRemarkOther { get; set; }
        public double totalPoint { get; set; }
        public string storeName { get; set; }
        public Guid storeGuid { get; set; }
        public int maxProdSeq { get; set; }
        public RecProdData dtoRecProd { get; set; }
        public List<RecProdData> lstRecProd { get; set; }
        public List<ProdModel> lstProduct { get; set; }
        public List<string> lstUOM { get; set; }
        public List<RecImage> lstRecImg { get; set; }
        public RecImage recImg { get; set; }
        public List<string> lstReason { get; set; }
    }
    public class RecProdData
    {
        public Guid recProdGuid { get; set; }
        public Guid recProdProdGuid { get; set; }
        public string recProdProdName { get; set; }
        public int recProdQty { get; set; }
        public decimal recProdPricePerUnit { get; set; }
        public decimal recProdTotalAmt { get; set; }
        public string recProdRemark { get; set; }
        public string recProdStatus { get; set; }
        public string recProdUOM { get; set; }
        public int recProdIndex { get; set; }
        public decimal reqProdDiscount { get; set; }
        public decimal reqProdNetTotal { get; set; }
        public int prodChanQty { get; set; }
        public int prodChanPoint { get; set; }
        public string prodPackSize { get; set; }
        public bool prodIsHavePoint { get; set; }
    }

    public class RecImage
    {
        public string imageName { get; set; }
        public string imageURL { get; set; }
    }
    public class ProdModel
    {
        public Guid prodGuid { get; set; }
        public string prodCode { get; set; }
        public string prodName01 { get; set; }
        public string prodName02 { get; set; }
        public string prodImage { get; set; }
        public string prodPackSize { get; set; }
        public string prodStatus { get; set; }
        public string prodRemark { get; set; }
        public int prodSeq { get; set; }
        public string prodExternalLink { get; set; }
        public int prodChanQty { get; set; }
        public int prodChanPoint { get; set; }
    }
    public class EventStateModel
    {
        public List<EventStateParam>? lstEventParam { get; set; }
        public string eventStateType { get; set; }
        public string eventName { get; set; }
    }
    public class EventStateParam
    {
        public string paramName { get; set; }
        public string paramValue { get; set; }
    }
    public class ProductModel {
        public Guid prodGuid { get; set; }
        public string prodCode { get; set; }
        public string prodName01 { get; set; }
        public string prodName02 { get; set; }
        public string prodImage { get; set; }
        public string prodPackSize { get; set; }
        public string prodStatus { get; set; }
        public string prodRemark { get; set; }
        public int prodSeq { get; set; }
        public string prodExternalLink { get; set; }
    }
    public class AddTransactionModel
    {
        public string storeCode { get; set; }
        public string prodCode { get; set; }
    }
    public class CustProfileModel
    {
        public CustModel custData { get; set; }
        public int totalPoint { get; set; }
    }
    public class CampModel {
        public Guid campGuid { get; set; }
        public string campName { get; set; }
        public DateTime campStartDate { get; set; }
        public DateTime campEndDate { get; set; }
        public string campBanner01 { get; set; }
        public string campBanner02 { get; set; }
        public string campBanner03 { get; set; }
        public string campStatus { get; set; }
        public string campRemark { get; set; }
    }
    public class StoreModel
    {
        public Guid storeGuid { get; set; }
        public int storeSeq { get; set; }
        public string storeGroup { get; set; }
        public string storeName { get; set; }
        public string storeImage { get; set; }
        public string storeSampRec { get; set; }
        public string storeStatus { get; set; }
        public string storeRemark { get; set; }
    }
    public class HistPointModel
    {
        public string requestDate_str { get; set; }
        public DateTime requestDate { get; set; }
        public string storeName { get; set; }
        public int totalPoint { get; set; }
        public string status { get; set; }
        public string statusText { get; set; }
        public string remark {get;set;}
        public Guid reqGuid { get; set; }
    }

    public class RewardModel
    {
        public Guid rewardGuid { get; set; }
        public List<RewardData> lstRewardHighTier { get; set; }
        public List<RewardData> lstRewardLowTier { get; set; }
        public CustModel custData { get; set; }
        public int totalPoint { get; set; }
    }
    public class RewardDataResp {
        public bool isPass { get; set; }
        public List<RewardData> lstReward { get; set; }
    }

    public class RewardData
    {
        public string deliveryTracking { get; set; }
        public bool isPass { get; set; }
        public string uploadMsg { get; set; }
        public string rewardImageIna { get; set; }
        public bool isEnable { get; set; }
        public Guid rewardGuid { get; set; }
        public int rewardSeq { get; set; }
        public string rewardName { get; set; }
        public string rewardSubName { get; set; }
        public string rewardImage { get; set; }
        public string rewardType { get; set; }
        public int rewardBurnPoint { get; set; }
        public int rewardLimitPerCust { get; set; }
        public int rewardTotalStock { get; set; }
        public int rewardRemainStock { get; set; }
        public string rewardStoreGroup { get; set; }
        public int rewardMinPointCond { get; set; }
        public string rewardStatus { get; set; }
        public string rewardRemark { get; set; }
        public int custSumRemainPoint { get; set; }
        public Guid redmHistGuid { get; set; }
        public Guid redmHistSumGuid { get; set; }
        public Guid redmHistRedmGuid { get; set; }
        public int redmHistQty { get; set; }
        public int redmHistPointPerUnit { get; set; }
        public Guid redmHistVCGuid { get; set; }
        public DateTime redmHistVCExpire { get; set; }
        public string redmHistTracking { get; set; }
        public DateTime redmHistCreateDate { get; set; }
        public string redmHistCreateDate_str { get; set; }
        public string custAddr01 { get; set; }
        public string custAddr02 { get; set; }
        public long custProvId { get; set; }
        public long custDistId { get; set; }
        public long custSubDistId { get; set; }
        public string provName { get; set; }
        public string distName { get; set; }
        public string subDistName { get; set; }
        public string postCode { get; set; }
        public string custName { get; set; }
        public Guid custGuid { get; set; }
        public string custFirstName { get; set; }
        public string custLastName { get; set; }
        public string custLineId { get; set; }
        public string custLineImg { get; set; }
        public string custLineDisplayName { get; set; }
        public string custTel { get; set; }
        public string custAddress { get; set; }
    }

    public class RedeemConfirmData
    {
        public Guid redeemGuid { get; set; }
        public int redmHistQty { get; set; }
    }
        public class AddrProviceModel
    {
        public long provId { get; set; }
        public string provCode { get; set; }
        public string provNameTh { get; set; }
        public string provNameEn { get; set; }
    }
    public class AddrDistrictModel
    {
        public long distId { get; set; }
        public string distCode { get; set; }
        public string name_th { get; set; }
        public string name_en { get; set; }
        public long province_id { get; set; }
    }
    public class AddrSubDistrictModel
    {
        public long subDistId { get; set; }
        public string subDistZipCode { get; set; }
        public string subDistNameTh { get; set; }
        public string subDistNameEn { get; set; }
        public long subDistDistId { get; set; }

    }
}
