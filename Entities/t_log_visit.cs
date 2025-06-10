namespace tnki_line_sale_api.Entities
{
    public class t_log_visit
    {
        public Guid log_guid { get; set; }
        public Guid log_cust_guid { get; set; }
        public string log_state { get; set; }
        public DateTime log_createdate { get; set; }
        public string log_remark { get; set; }
    }
}
