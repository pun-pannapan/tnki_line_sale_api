namespace tnki_line_sale_api.Entities
{
    public class m_reward
    {
        public Guid reward_guid { get; set; }
        public int reward_seq { get; set; }
        public string reward_name { get; set; }
        public string reward_subname { get; set; }
        public string reward_image { get; set; }
        public string reward_type { get; set; }
        public int reward_burn_point { get; set; }
        public int reward_limit_per_cust { get; set; }
        public int reward_total_stock { get; set; }
        public int reward_remain_stock { get; set; }
        public string reward_store_group { get; set; }
        public int reward_min_point_cond { get; set; }
        public string reward_status { get; set; }
        public string reward_remark { get; set; }
        public string reward_image_ina { get; set; }
        public string reward_camp_code { get; set; }
        public Guid row_version { get; set; }        
    }
}