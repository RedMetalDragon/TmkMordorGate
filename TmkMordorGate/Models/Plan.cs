namespace TmkMordorGate.Models;
    
    public class Plan
    {
        public int PlanID { get; set; }
        public string PlanName { get; set; }
        public string Description { get; set; }
        public bool? IsActive { get; set; }
        public decimal Price { get; set; }
        public DateTime? CreationDate { get; set; }
    }