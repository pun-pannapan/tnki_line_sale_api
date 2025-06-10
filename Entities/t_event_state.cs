namespace tnki_line_sale_apiEntities
{
    public class t_event_state
    {
        public Guid ev_state_guid { get; set; }
        public Guid ev_state_session_guid { get; set; }
        public string ev_state_eventtype { get; set; }
        public Guid ev_state_cust_guid { get; set; }
        public Guid ev_state_group_guid { get; set; }
        public string ev_state_name { get; set; }
		public string ev_state_paramname { get; set; }
		public string ev_state_paramvalue { get; set; }
		public DateTime ev_state_updatedate { get; set; }
		public string ev_state_ref01 { get; set; }
		public string ev_state_ref02 { get; set; }
		public string ev_state_remark { get; set; }
	}
}
