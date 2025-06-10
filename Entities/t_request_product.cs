namespace tnki_line_sale_apiEntities
{
    public class t_request_product
    {
        public Guid req_prod_guid { get; set; }
        public Guid req_prod_req_guid { get; set; }
        public Guid req_prod_prod_guid { get; set; }
        public int req_prod_prod_point_config { get; set; }
        public int req_prod_prod_qty_config { get; set; }
        public int req_prod_qty { get; set; }
		public int req_prod_total_point { get; set; }
		public decimal req_prod_price_per_unit { get; set; }
		public string req_prod_uom { get; set; }
		public string req_prod_remark { get; set; }
		public DateTime req_prod_createdate { get; set; }
		public DateTime req_prod_updatedate { get; set; }
		public decimal req_prod_discount { get; set; }

    }
}
