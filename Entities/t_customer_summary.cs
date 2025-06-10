namespace tnki_line_sale_api.Entities
{
    public class t_customer_summary
    {
        public Guid cust_sum_guid { get; set; }
        public Guid cust_sum_cust_guid { get; set; }
        public int cust_sum_total_point { get; set; }
		public int cust_sum_remain_point { get; set; }
		public DateTime cust_sum_updatedate { get; set; }
	}
}
