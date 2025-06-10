namespace tnki_line_sale_apiEntities
{
    public class t_history_redeem
    {
        public Guid redm_hist_guid { get; set; }
        public Guid redm_hist_sum_guid { get; set; }
        public Guid redm_hist_redm_guid { get; set; }
		public int redm_hist_qty { get; set; }
		public int redm_hist_point_per_unit { get; set; }
		public string redm_hist_tracking { get; set; }
		public string redm_hist_status { get; set; }
		public DateTime redm_hist_createdate { get; set; }
		public DateTime redm_hist_updatedate { get; set; }
	}
}
