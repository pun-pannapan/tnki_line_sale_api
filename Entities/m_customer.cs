namespace tnki_line_sale_apiEntities
{
    public class m_customer
    {
        public Guid cust_guid { get; set; }
        public string cust_firstname { get; set; }
        public string cust_lastname { get; set; }
        public string cust_line_id { get; set; }
        public string cust_line_image { get; set; }
        public string cust_line_display_name { get; set; }
        public string cust_phone_number { get; set; }
		public bool cust_check_pdpa1 { get; set; }
		public bool cust_check_pdpa2 { get; set; }
		public string cust_status { get; set; }
		public string cust_remark { get; set; }
        public DateTime cust_updatedate { get; set; }
        public string cust_address1 { get; set; }
        public string cust_address2 { get; set; }
        public long cust_prov_id { get; set; }
        public long cust_dist_id { get; set; }
        public long cust_subdist_id { get; set; }
        public bool cust_check_topspender { get; set; }
    }
}
