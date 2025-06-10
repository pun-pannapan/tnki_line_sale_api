namespace tnki_line_sale_api.Entities
{
    public class m_campaign_period
    {
        public Guid camp_guid { get; set; }
        public string camp_code { get; set; }
        public string camp_name { get; set; }
        public DateTime camp_startdate { get; set; }
        public DateTime camp_enddate { get; set; }
		public string camp_banner1 { get; set; }
		public string camp_banner2 { get; set; }
		public string campBanner3 { get; set; }
		public string camp_status { get; set; }
		public string camp_remark { get; set; }
        public DateTime camp_topspend_startdate { get; set; }
        public DateTime camp_topspend_enddate { get; set; }
        public DateTime camp_redeem_startdate { get; set; }
        public DateTime camp_redeem_enddate { get; set; }
    }
}
