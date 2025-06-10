namespace tnki_line_sale_apiEntities
{
    public class t_customer_point_history
    {
        public Guid cust_point_guid { get; set; }
        public Guid cust_point_sum_guid { get; set; }
        public string cust_point_act_type { get; set; }
		public string cust_point_activity_log { get; set; }
		public int cust_point_earn_point { get; set; }
		public int cust_point_earn_point_remain { get; set; }
		public int cust_point_burn_point { get; set; }
		public DateTime cust_point_earn_expiredate { get; set; }
		public string cust_point_status { get; set; }
		public DateTime cust_point_createdate { get; set; }
		public Guid cust_point_createby { get; set; }
		public DateTime cust_point_updatedate { get; set; }
		public Guid cust_point_updateby { get; set; }
		public string cust_point_storegroup { get; set; }
	}
}
