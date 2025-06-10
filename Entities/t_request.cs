namespace tnki_line_sale_apiEntities
{
    public class t_request
    {
        public Guid req_guid { get; set; }
        public Guid req_cust_guid { get; set; }
        public Guid req_store_guid { get; set; }
        public int req_total_point { get; set; }
        public string req_receipt_no { get; set; }
        public string req_status { get; set; }
        public string req_remark { get; set; }
        public DateTime req_createdate { get; set; }
        public DateTime req_updatedate { get; set; }
    }
}
